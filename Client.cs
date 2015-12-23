using System;
using Network;
using Crypto;
using Patterns.Observer;
using Events.Network;
using Enums;
using Util;
using Speech;

namespace Pi {
	public class Client {
		//vars
		private TCPClient socket = new TCPClient();
		private DiffieHellman dh = new DiffieHellman();
		private Rijndael aes;

		private SpeechEngine engine = new SpeechEngine();

		private Observer socketObserver = new Observer();

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
				sendPacket(PacketType.DIFFIE_HELLMAN, dh.AB);
				Console.WriteLine("[Client] Sent DH handshake.");
				//socket.send(ByteUtil.toByte("GET / HTTP/1.0\r\n\r\n"));
			} else if (evnt == TCPClientEvent.DISCONNECTED) {
				Console.WriteLine("[Client] Disconnected.");
				socket.connect(_host, _port);
			} else if (evnt == TCPClientEvent.DOWNLOAD_COMPLETE) {
				//Console.WriteLine("[Client] Received " + args.Length + " bytes.");
				handlePacket(args);
			} else if (evnt == TCPClientEvent.ERROR) {
				Console.WriteLine("[Client] Error: " + args);
			} else if (evnt == TCPClientEvent.DEBUG) {
				//Console.WriteLine("[Client] Debug: " + args);
			}
		}

		private void sendPacket(ushort type, byte[] data) {
			socket.send(PacketUtil.createPacket(data, type));
		}

		private void handlePacket(byte[] packet) {
			ushort packetType = PacketUtil.getPacketType(packet);
			byte[] packetData = PacketUtil.getPacketData(packet);

			if (packetType == PacketType.DIFFIE_HELLMAN) {
				Console.WriteLine("[Client] Recieved DH key.");
				byte[] S = dh.S(packetData);
				key = Hash.sha256(ByteUtil.combine(ByteUtil.toByte("0keeP+attentioN+wateR+herE1+"), S));
				iv = Hash.sha256(ByteUtil.combine(ByteUtil.toByte("1-Knew-Carbon-Involved-State2"), S));
				aes = new Rijndael(key, iv);
				Console.WriteLine("[Client] DH key exchanged, AES key created.");
				Console.WriteLine("[Client] Sending encrypted test string.");
				sendPacket(PacketType.TEST, aes.encrypt(ByteUtil.toByte("Hello! I'm the client's test string.")));
			} else if (packetType == PacketType.TEST) {
				Console.WriteLine("[Client] " + ByteUtil.toString(aes.decrypt(packetData)));
				Console.WriteLine("[Client] Listening for speech..");
				engine.start();
			}
		}
	}
}