using System;
using System.Net.Sockets;
using System.Collections.Generic;
using Patterns.Observer;
using Events.Network;
using System.IO;
using System.Net;

namespace Network {
	public class TCPClient : IDispatchable {
		//vars
		public static readonly List<Observer> OBSERVERS = new List<Observer>();

		private Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		private List<byte[]> backlog = new List<byte[]>();
		private bool available;

		private byte[] buffer;
		private MemoryStream data = new MemoryStream();

		//constructor
		public TCPClient(int bufferSize = 1024) {
			if (bufferSize < 1) {
				bufferSize = 1024;
			}

			buffer = new byte[bufferSize];
			socket.SendBufferSize = socket.ReceiveBufferSize = bufferSize;
		}

		//public
		public void connect(string host, ushort port) {
			if (socket.Connected) {
				socket.Disconnect(true);
				socket.Close();
				GC.Collect();
				backlog.Clear();
				dispatch(TCPClientEvent.DISCONNECTED);
			}

			available = false;
			try {
				socket.BeginConnect(host, (int) port, new AsyncCallback(onConnect), null);
			} catch (Exception ex) {
				dispatch(TCPClientEvent.ERROR, ex.Message);
				return;
			}

			backlog.Clear();
		}
		public void disconnect() {
			if (!socket.Connected) {
				return;
			}

			try {
				socket.BeginDisconnect(true, new AsyncCallback(onClose), null);
			} catch (Exception ex) {
				dispatch(TCPClientEvent.ERROR, ex.Message);
				return;
			}
		}

		public void send(byte[] data) {
			if (!socket.Connected || data == null || data.Length == 0) {
				return;
			}

			if (!available || backlog.Count > 0) {
				backlog.Add(data);
			} else {
				available = false;
				sendInternal(data);
			}
		}

		public bool connected {
			get {
				return socket.Connected;
			}
		}

		public void dispatch(string evnt, object data = null) {
			Observer.dispatch(OBSERVERS, this, evnt, data);
		}

		//private
		private void sendInternal(byte[] data) {
			available = false;

			try {
				socket.BeginSend(data, 0, data.Length, SocketFlags.None, new AsyncCallback(onSend), null);
			} catch (Exception ex) {
				available = true;
				dispatch(TCPClientEvent.ERROR, ex.Message);
				return;
			}

			dispatch(TCPClientEvent.DEBUG, "Sent " + data.Length + " bytes");
		}

		private void onConnect(IAsyncResult e) {
			try {
				socket.EndConnect(e);
			} catch (Exception ex) {
				dispatch(TCPClientEvent.ERROR, ex.Message);
				return;
			}

			try {
				socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(onRecieve), null);
			} catch (Exception ex) {
				dispatch(TCPClientEvent.ERROR, ex.Message);
			}

			dispatch(TCPClientEvent.CONNECTED);
			sendNext();
		}
		private void onClose(IAsyncResult e) {
			try {
				socket.EndDisconnect(e);
			} catch (Exception ex) {
				dispatch(TCPClientEvent.ERROR, ex.Message);
				return;
			}

			socket.Close();
			GC.Collect();

			backlog.Clear();
			dispatch(TCPClientEvent.DISCONNECTED);
		}

		private void onSend(IAsyncResult e) {
			try {
				socket.EndSend(e);
			} catch (Exception ex) {
				dispatch(TCPClientEvent.ERROR, ex.Message);
				return;
			}

			dispatch(TCPClientEvent.UPLOAD_COMPLETE);
			sendNext();
		}
		private void onRecieve(IAsyncResult e) {
			int bytesRead;

			try {
				bytesRead = socket.EndReceive(e);
			} catch (Exception ex) {
				dispatch(TCPClientEvent.ERROR, ex.Message);
				return;
			}

			if (bytesRead > 0) {
				data.Write(buffer, 0, bytesRead);
			}

			try {
				socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(onRecieve), null);
			} catch (Exception ex) {
				dispatch(TCPClientEvent.ERROR, ex.Message);
			}

			byte[] temp = data.ToArray();
			data.SetLength(0L);

			dispatch(TCPClientEvent.DEBUG, "Recieved " + temp.Length + " bytes");
			dispatch(TCPClientEvent.DOWNLOAD_COMPLETE, temp);
		}

		private void sendNext() {
			if (backlog.Count == 0) {
				available = true;
				return;
			}

			dispatch(TCPClientEvent.SEND_NEXT);

			byte[] data = backlog[0];
			backlog.RemoveAt(0);
			sendInternal(data);
		}
	}
}