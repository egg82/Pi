using System;
using System.Collections.Generic;
using Patterns.Observer;
using System.Net.Sockets;
using Events.Network;
using System.IO;

namespace Network {
	public class TCPClient : IDispatchable {
		//vars
		public static readonly List<Observer> OBSERVERS = new List<Observer>();

		private TcpClient socket;
		private NetworkStream stream;
		private MemoryStream outStream;
		private List<byte[]> backlog = new List<byte[]>();
		private bool available = true;
		private int _bufferSize;

		private string _host = null;
		private ushort _port = 0;

		//constructor
		public TCPClient(int bufferSize = 1024) {
			if (bufferSize < 1) {
				bufferSize = 1024;
			}

			_bufferSize = bufferSize;
			//socket.SendBufferSize = socket.ReceiveBufferSize = bufferSize;
			outStream = new MemoryStream();
		}

		//public
		public void connect(string host, ushort port) {
			if (socket != null && socket.Connected) {
				dispatch(TCPClientEvent.ERROR, "TCPClient is already connected to " + _host + ":" + _port + ".");
				return;
			}

			available = false;

			socket = new TcpClient();
			try {
				socket.BeginConnect(host, (int) port, new AsyncCallback(onConnect), null);
			} catch (Exception ex) {
				available = true;
				dispatch(TCPClientEvent.ERROR, ex.Message);
				return;
			}

			_host = host;
			_port = port;
			backlog.Clear();
			outStream.SetLength(0L);
		}
		public void disconnect() {
			if (socket == null || !socket.Connected) {
				return;
			}

			disconnectInternal();
		}

		public void send(byte[] data) {
			if (socket == null || !socket.Connected || data == null || data.Length == 0) {
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
		public string host {
			get {
				return _host;
			}
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
		private void disconnectInternal() {
			try {
				socket.Close();
				stream.Close();
				stream.Dispose();
			} catch (Exception ex) {
				dispatch(TCPClientEvent.ERROR, ex.Message);
				return;
			}

			socket = null;
			_host = null;
			_port = 0;
			backlog.Clear();
			outStream.SetLength(0L);

			GC.Collect();

			dispatch(TCPClientEvent.DISCONNECTED);
		}
		private void sendInternal(byte[] data) {
			dispatch(TCPClientEvent.DEBUG, "Sending " + data.Length + " bytes.");

			byte[] newData = new byte[data.Length + 1];
			Buffer.BlockCopy(data, 0, newData, 0, data.Length);

			try {
				stream.BeginWrite(newData, 0, newData.Length, new AsyncCallback(onSend), data.Length);
			} catch (Exception ex) {
				dispatch(TCPClientEvent.ERROR, ex.Message);
				sendNext();
				return;
			}
		}

		private void onConnect(IAsyncResult e) {
			try {
				socket.EndConnect(e);
			} catch (Exception ex) {
				dispatch(TCPClientEvent.ERROR, ex.Message);
				return;
			}

			stream = socket.GetStream();
			receiveNext();

			dispatch(TCPClientEvent.CONNECTED);
			sendNext();
		}
		private void onSend(IAsyncResult e) {
			try {
				stream.EndWrite(e);
				stream.Flush();
			} catch (Exception ex) {
				dispatch(TCPClientEvent.ERROR, ex.Message);
				return;
			}

			dispatch(TCPClientEvent.DEBUG, "Sent " + ((int) e.AsyncState) + " bytes.");
			dispatch(TCPClientEvent.UPLOAD_COMPLETE);
			sendNext();
		}
		private void onReceive(IAsyncResult e) {
			int bytesRead;
			byte[] buffer = (byte[]) e.AsyncState;

			try {
				bytesRead = stream.EndRead(e);
			} catch (Exception ex) {
				dispatch(TCPClientEvent.ERROR, ex.Message);
				return;
			}

			dispatch(TCPClientEvent.DEBUG, "Received " + ((bytesRead > 0 && buffer[bytesRead - 1] == (byte) 0) ? bytesRead - 1 : bytesRead) + " bytes.");

			if (bytesRead == 0 || buffer[bytesRead - 1] == (byte) 0) {
				if (bytesRead > 0) {
					receiveNext();
				}

				lock (outStream) {
					if (bytesRead > 0 && buffer[bytesRead - 1] == (byte) 0) {
						outStream.Write(buffer, 0, bytesRead - 1);
					}

					if (outStream.Length > 0) {
						byte[] outBuffer = outStream.ToArray();
						outStream.SetLength(0L);
						dispatch(TCPClientEvent.DOWNLOAD_COMPLETE, outBuffer);
					}

					if (bytesRead == 0) {
						disconnectInternal();
					}
				}
				return;
			} else {
				receiveNext();
				lock (outStream) {
					outStream.Write(buffer, 0, bytesRead);
				}
			}
		}

		private void receiveNext() {
			byte[] buffer = new byte[_bufferSize];

			try {
				stream.BeginRead(buffer, 0, _bufferSize, new AsyncCallback(onReceive), buffer);
			} catch (Exception ex) {
				dispatch(TCPClientEvent.ERROR, ex.Message);
				return;
			}
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