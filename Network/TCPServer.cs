using System;
using System.Collections.Generic;
using Patterns.Observer;
using System.Net.Sockets;
using System.IO;
using System.Timers;
using Events.Network;
using System.Net;

namespace Network {
	public class TCPServer : IDispatchable {
		//vars
		public static readonly List<Observer> OBSERVERS = new List<Observer>();

		private TcpListener server;
		private List<State> clients = new List<State>();
		private int _bufferSize;
		private bool _exclusive;

		private Timer openTimer = new Timer(100.0d);

		private ushort _port = 0;

		private struct State {
			public byte[] buffer;
			public NetworkStream stream;
			public MemoryStream outStream;
			public int pos;
			public TcpClient client;
		}

		//constructor
		public TCPServer(int bufferSize = 1024, bool exclusive = true) {
			openTimer.Elapsed += new ElapsedEventHandler(onOpenTimer);

			if (bufferSize < 1) {
				bufferSize = 1024;
			}

			_bufferSize = bufferSize;
			_exclusive = exclusive;
		}

		//public
		public void open(ushort port) {
			if (server != null && server.Server.IsBound) {
				dispatch(TCPServerEvent.ERROR, "TCPServer is already bound to " + _port + ".");
				return;
			}

			server = new TcpListener(IPAddress.Any, (int) port);
			server.ExclusiveAddressUse = _exclusive;
			try {
				server.Start();
			} catch (Exception ex) {
				dispatch(TCPServerEvent.ERROR, ex.Message);
				return;
			}

			_port = port;
			openTimer.Start();
		}
		public void close() {
			if (server == null || !server.Server.IsBound) {
				return;
			}

			for (int i = 0; i < clients.Count; i++) {
				if (clients[i].client != null && clients[i].client.Connected) {
					try {
						clients[i].client.Close();
						clients[i].stream.Close();
						clients[i].stream.Dispose();
					} catch (Exception ex) {
						dispatch(TCPServerEvent.ERROR, ex.Message);
					}

					clients[i].outStream.SetLength(0L);

					dispatch(TCPClientEvent.DISCONNECTED, clients[i].pos);
				}
			}

			clients.Clear();

			try {
				server.Stop();
			} catch (Exception ex) {
				dispatch(TCPServerEvent.ERROR, ex.Message);
				return;
			}

			server = null;
			_port = 0;
			openTimer.Stop();
			GC.Collect();

			dispatch(TCPServerEvent.CLOSED);
		}

		public void send(int client, byte[] data) {
			if (data == null || data.Length == 0) {
				return;
			}
			if (client < 0 || client >= clients.Count) {
				return;
			}
			if (clients[client].client == null || !clients[client].client.Connected) {
				return;
			}

			byte[] newData = new byte[data.Length + 1];
			Buffer.BlockCopy(data, 0, newData, 0, data.Length);

			try {
				clients[client].stream.BeginWrite(newData, 0, newData.Length, new AsyncCallback(onClientSend), clients[client]);
			} catch (Exception ex) {
				dispatch(TCPServerEvent.ERROR, ex.Message);
				return;
			}

			dispatch(TCPServerEvent.DEBUG, "Sending " + data.Length + " bytes to client #" + client + ".");
		}
		public void sendAll(byte[] data) {
			if (data == null || data.Length == 0) {
				return;
			}

			byte[] newData = new byte[data.Length + 1];
			Buffer.BlockCopy(data, 0, newData, 0, data.Length);

			for (int i = 0; i < clients.Count; i++) {
				try {
					clients[i].stream.BeginWrite(newData, 0, newData.Length, new AsyncCallback(onClientSend), clients[i]);
					dispatch(TCPServerEvent.DEBUG, "Sent " + data.Length + " bytes to client #" + i + ".");
				} catch (Exception ex) {
					dispatch(TCPServerEvent.ERROR, ex.Message);
				}
			}
		}

		public void disconnect(int client) {
			if (client < 0 || client >= clients.Count) {
				return;
			}
			if (clients[client].client == null || !clients[client].client.Connected) {
				return;
			}

			disconnectClientInternal(clients[client]);
		}
		public void disconnectAll() {
			for (int i = 0; i < clients.Count; i++) {
				if (clients[i].client != null && clients[i].client.Connected) {
					try {
						clients[i].client.Close();
						clients[i].stream.Close();
						clients[i].stream.Dispose();
					} catch (Exception ex) {
						dispatch(TCPServerEvent.ERROR, ex.Message);
					}
					
					clients[i].outStream.SetLength(0L);

					dispatch(TCPClientEvent.DISCONNECTED, clients[i].pos);
				}
			}

			clients.Clear();

			GC.Collect();
		}

		public ushort port {
			get {
				return _port;
			}
		}

		public void dispatch(string evnt, object data = null) {
			Observer.dispatch(OBSERVERS, this, evnt, data);
		}

		//private
		private void onOpenTimer(object sender, ElapsedEventArgs e) {
			if (server.Server.IsBound) {
				openTimer.Stop();
				server.BeginAcceptTcpClient(new AsyncCallback(onConnect), null);
				dispatch(TCPServerEvent.OPENED);
			}
		}

		private void disconnectClientInternal(State state) {
			try {
				state.client.Close();
				state.stream.Close();
				state.stream.Dispose();
			} catch (Exception ex) {
				dispatch(TCPClientEvent.ERROR, ex.Message);
			}

			state.client = null;
			state.outStream.SetLength(0L);

			GC.Collect();

			dispatch(TCPClientEvent.DISCONNECTED, state.pos);
		}
		private void onConnect(IAsyncResult e) {
			TcpClient client;

			try {
				client = server.EndAcceptTcpClient(e);
			} catch (Exception ex) {
				dispatch(TCPServerEvent.ERROR, ex.Message);
				return;
			}

			server.BeginAcceptTcpClient(new AsyncCallback(onConnect), null);

			for (int i = 0; i < clients.Count; i++) {
				if (clients[i].client == client) {
					return;
				}
			}

			State state = new State();
			state.buffer = new byte[_bufferSize];
			state.stream = client.GetStream();
			state.outStream = new MemoryStream();
			state.client = client;
			state.pos = clients.Count;

			//client.SendBufferSize = client.ReceiveBufferSize = _bufferSize;

			clients.Add(state);
			receiveNext(state);

			dispatch(TCPServerEvent.CONNECTION, state.pos);
		}

		private void onClientSend(IAsyncResult e) {
			State state = (State) e.AsyncState;

			try {
				state.stream.EndWrite(e);
				state.stream.Flush();
			} catch (Exception ex) {
				dispatch(TCPServerEvent.ERROR, ex.Message);
				return;
			}
			
			dispatch(TCPServerEvent.CLIENT_UPLOAD_COMPLETE);
		}
		private void onClientReceive(IAsyncResult e) {
			int bytesRead;
			State state = (State) e.AsyncState;

			try {
				bytesRead = state.stream.EndRead(e);
			} catch (Exception ex) {
				dispatch(TCPServerEvent.ERROR, ex.Message);
				return;
			}
			
			dispatch(TCPServerEvent.DEBUG, "Received " + ((bytesRead > 0 && state.buffer[bytesRead - 1] == (byte) 0) ? bytesRead - 1 : bytesRead) + " bytes.");

			if (bytesRead == 0 || state.buffer[bytesRead - 1] == (byte) 0) {
				lock (state.outStream) {
					if (bytesRead > 0 && state.buffer[bytesRead - 1] == (byte) 0) {
						state.outStream.Write(state.buffer, 0, bytesRead - 1);
					}

					if (state.outStream.Length > 0) {
						byte[] outBuffer = state.outStream.ToArray();
						state.outStream.SetLength(0L);
						dispatch(TCPServerEvent.CLIENT_DOWNLOAD_COMPLETE, new {
							client = state.pos,
							data = outBuffer
						});
					}

					if (bytesRead == 0) {
						disconnectClientInternal(state);
					}
				}
				return;
			} else {
				receiveNext(state);
				lock (state.outStream) {
					state.outStream.Write(state.buffer, 0, bytesRead);
				}
			}
		}

		private void receiveNext(State state) {
			try {
				state.stream.BeginRead(state.buffer, 0, _bufferSize, new AsyncCallback(onClientReceive), state);
			} catch (Exception ex) {
				dispatch(TCPClientEvent.ERROR, ex.Message);
				return;
			}
		}
	}
}