using System;
using Network;
using Crypto;
using Patterns.Observer;
using Events.Network;
using System.IO;
using Enums;
using System.Text;

namespace Pi {
	public class Client {
		//vars
		private TCPClient socket = new TCPClient();
		private DiffieHellman dh = new DiffieHellman();
		private Rijndael aes;

		private Observer socketObserver = new Observer();

		private MemoryStream packetBuilder = new MemoryStream();
		private MemoryStream packetBuffer = new MemoryStream();

		private byte[] key;
		private byte[] iv;

		private string _host;
		private ushort _port;

		//constructor
		public Client(string host, ushort port) {
			socketObserver.add(onSocketObserverNotify);
			Observer.add(TCPClient.OBSERVERS, socketObserver);

			_host = host;
			_port = port;
			socket.connect(host, port);
		}

		//public

		//private
		private void onSocketObserverNotify(object sender, string evnt, dynamic args) {
			if (sender != socket) {
				return;
			}

			if (evnt == TCPClientEvent.CONNECTED) {
				Console.WriteLine("[Client] Connected to " + socket.host + " on port " + socket.port + ".");
				//sendPacket(PacketType.DIFFIE_HELLMAN, dh.AB);
				//Console.WriteLine("[Client] Sent DH handshake.");
				socket.send(Encoding.UTF8.GetBytes("GET / HTTP/1.0\r\n\r\n"));
			} else if (evnt == TCPClientEvent.DISCONNECTED) {
				Console.WriteLine("[Client] Disconnected.");
				//socket.connect(_host, _port);
			} else if (evnt == TCPClientEvent.DOWNLOAD_COMPLETE) {
				Console.WriteLine("[Client] Received " + args.Length + " bytes.");
				//readPackets(args);
			} else if (evnt == TCPClientEvent.ERROR) {
				Console.WriteLine("[Client] Error: " + args);
			} else if (evnt == TCPClientEvent.DEBUG) {
				//Console.WriteLine("[Client] Debug: " + args);
			}
		}

		private void sendPacket(ushort type, byte[] data) {
			packetBuilder.SetLength(0L);
			packetBuilder.Write(BitConverter.GetBytes(type), 0, 2);
			packetBuilder.Write(data, 0, data.Length);

			socket.send(PacketHelper.writePacket(packetBuilder.ToArray()));
		}
		private ushort getPacketType(byte[] data) {
			return BitConverter.ToUInt16(data, 0);
		}
		private byte[] getPacketData(byte[] data) {
			byte[] newData = new byte[data.Length - 2];
			Buffer.BlockCopy(data, 2, newData, 0, data.Length - 2);
			return newData;
		}

		private void readPackets(byte[] data) {
			int bufferRead = 0;
			byte[] stream = new byte[packetBuffer.Length + data.Length];

			Buffer.BlockCopy(packetBuffer.ToArray(), 0, stream, 0, (int) packetBuffer.Length);
			Buffer.BlockCopy(data, 0, stream, (int) packetBuffer.Length, data.Length);

			do {
				byte[] nextPacket = PacketHelper.readPacket(stream, bufferRead);

				if (nextPacket == null) {
					break;
				}

				handlePacket(nextPacket);
				bufferRead += PacketHelper.getPacketLength(stream, bufferRead) + 2;
			} while (true);

			packetBuffer.SetLength(0L);

			if (stream.Length - bufferRead > 0) {
				packetBuffer.Write(stream, bufferRead, stream.Length - bufferRead);
			}
		}
		private void handlePacket(byte[] packet) {
			ushort packetType = getPacketType(packet);
			byte[] packetData = getPacketData(packet);

			if (packetType == PacketType.DIFFIE_HELLMAN) {
				Console.WriteLine("[Client] Recieved DH key.");
				byte[] S = dh.S(packetData);
				key = Hash.generate256Key("0keeP+attentioN+wateR+herE1+", S);
				iv = Hash.generate256Key("1-Knew-Carbon-Involved-State2", S);
				aes = new Rijndael(key, iv);
				Console.WriteLine("[Client] DH key exchanged, AES key created.");
			}
		}
	}
}