using System;
using System.Collections.Generic;
using Patterns.Observer;
using System.Net.Sockets;
using Events.Network;
using System.Net;
using System.Timers;
using System.IO;

namespace Network {
	public class TCPServer : IDispatchable {
		//vars
		public static readonly List<Observer> OBSERVERS = new List<Observer>();

		private Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		private List<Socket> clients = new List<Socket>();

		private Timer openTimer = new Timer(100.0d);

		private int bufferSize;
		private struct State {
			public byte[] buffer;
			public MemoryStream data;
			public Socket socket;
		}

		//constructor
		public TCPServer(int bufferSize = 1024) {
			openTimer.Elapsed += new ElapsedEventHandler(onOpenTimer);

			if (bufferSize < 1) {
				bufferSize = 1024;
			}

			this.bufferSize = bufferSize;
			server.ExclusiveAddressUse = true;
		}

		//public
		public void open(ushort port) {
			if (server.IsBound) {
				close();
			}

			try {
				server.Bind(new IPEndPoint(IPAddress.Any, port));
				server.Listen(1024);
			} catch (Exception ex) {
				dispatch(TCPServerEvent.ERROR, ex.Message);
				return;
			}

			openTimer.Start();
		}
		public void close() {
			if (!server.IsBound) {
				return;
			}

			for (int i = 0; i < clients.Count; i++) {
				try {
					clients[i].BeginDisconnect(false, new AsyncCallback(onClientClose), clients[i]);
				} catch (Exception ex) {
					dispatch(TCPServerEvent.ERROR, ex.Message);
				}
			}

			clients.Clear();

			try {
				server.Close();
			} catch (Exception ex) {
				dispatch(TCPServerEvent.ERROR, ex.Message);
			}
			GC.Collect();

			openTimer.Stop();
			dispatch(TCPServerEvent.CLOSED);
		}

		public void dispatch(string evnt, object data = null) {
			Observer.dispatch(OBSERVERS, this, evnt, data);
		}

		public void send(int client, byte[] data) {
			if (data == null | data.Length == 0) {
				return;
			}
			if (client < 0 || client >= clients.Count) {
				return;
			}

			try {
				clients[client].BeginSend(data, 0, data.Length, SocketFlags.None, new AsyncCallback(onClientSend), clients[client]);
			} catch (Exception ex) {
				dispatch(TCPClientEvent.ERROR, ex.Message);
				return;
			}

			dispatch(TCPClientEvent.DEBUG, "Sent " + data.Length + " bytes to client " + client);
		}
		public void sendAll(byte[] data) {
			if (data == null | data.Length == 0) {
				return;
			}

			for (int i = 0; i < clients.Count; i++) {
				try {
					clients[i].BeginSend(data, 0, data.Length, SocketFlags.None, new AsyncCallback(onClientSend), clients[i]);
					dispatch(TCPClientEvent.DEBUG, "Sent " + data.Length + " bytes to client " + i);
				} catch (Exception ex) {
					dispatch(TCPClientEvent.ERROR, ex.Message);
				}
			}
		}

		public void disconnect(int client) {
			if (client < 0 || client >= clients.Count) {
				return;
			}

			try {
				clients[client].BeginDisconnect(false, new AsyncCallback(onClientClose), clients[client]);
			} catch (Exception ex) {
				dispatch(TCPServerEvent.ERROR, ex.Message);
			}
		}
		public void disconnectAll() {
			for (int i = 0; i < clients.Count; i++) {
				try {
					clients[i].BeginDisconnect(false, new AsyncCallback(onClientClose), clients[i]);
				} catch (Exception ex) {
					dispatch(TCPServerEvent.ERROR, ex.Message);
				}
			}

			clients.Clear();
			GC.Collect();
		}

		//private
		private void onConnect(IAsyncResult e) {
			Socket clientSocket;

			try {
				clientSocket = server.EndAccept(e);
			} catch (Exception ex) {
				dispatch(TCPServerEvent.ERROR, ex.Message);
				return;
			}

			server.BeginAccept(new AsyncCallback(onConnect), null);

			if (clients.Contains(clientSocket)) {
				return;
			}

			State state = new State();
			state.buffer = new byte[bufferSize];
			state.data = new MemoryStream();
			state.socket = clientSocket;

			clientSocket.SendBufferSize = clientSocket.ReceiveBufferSize = bufferSize;

			try {
				clientSocket.BeginReceive(state.buffer, 0, state.buffer.Length, SocketFlags.None, new AsyncCallback(onClientRecieve), state);
			} catch (Exception ex) {
				dispatch(TCPServerEvent.ERROR, ex.Message);
			}

			clients.Add(clientSocket);
			dispatch(TCPServerEvent.CONNECTION, clients.Count - 1);
		}

		private void onClientClose(IAsyncResult e) {
			Socket clientSocket = e.AsyncState as Socket;

			try {
				clientSocket.EndDisconnect(e);
			} catch (Exception ex) {
				dispatch(TCPServerEvent.ERROR, ex.Message);
			}

			int index = clients.IndexOf(clientSocket);

			clientSocket.Close();
			dispatch(TCPServerEvent.CLIENT_DISCONNECTED, index);
			if (index > -1) {
				clients.RemoveAt(index);
			}
		}
		private void onClientSend(IAsyncResult e) {
			Socket socket = (Socket) e.AsyncState;

			try {
				socket.EndSend(e);
			} catch (Exception ex) {
				dispatch(TCPServerEvent.ERROR, ex.Message);
				return;
			}

			dispatch(TCPServerEvent.CLIENT_UPLOAD_COMPLETE);
		}
		private void onClientRecieve(IAsyncResult e) {
			int bytesRead;
			State state = (State) e.AsyncState;

			try {
				bytesRead = state.socket.EndReceive(e);
			} catch (Exception ex) {
				dispatch(TCPServerEvent.ERROR, ex.Message);
				return;
			}

			if (bytesRead > 0) {
				state.data.Write(state.buffer, 0, bytesRead);
			}

			try {
				state.socket.BeginReceive(state.buffer, 0, state.buffer.Length, SocketFlags.None, new AsyncCallback(onClientRecieve), state);
			} catch (Exception ex) {
				dispatch(TCPServerEvent.ERROR, ex.Message);
			}

			byte[] temp = state.data.ToArray();
			state.data.SetLength(0L);

			dispatch(TCPServerEvent.DEBUG, "Recieved " + temp.Length + " bytes");
			dispatch(TCPServerEvent.CLIENT_DOWNLOAD_COMPLETE, new {
				client = clients.IndexOf(state.socket),
				data = temp
			});
		}

		private void onOpenTimer(object sender, ElapsedEventArgs e) {
			if (server.IsBound) {
				openTimer.Stop();
				server.BeginAccept(new AsyncCallback(onConnect), null);
				dispatch(TCPServerEvent.OPENED);
			}
		}
	}
}