using System;
using Network;
using Crypto;
using Patterns.Observer;
using Events.Network;
using Util;
using Enums;

namespace Pi {
	public class Server {
		//vars
		private TCPServer socket = new TCPServer(1024, false);
		private DiffieHellman dh = new DiffieHellman();
		private Rijndael aes;

		private Observer socketObserver = new Observer();

		private byte[] key;
		private byte[] iv;

		//constructor
		public Server(ushort port) {
			socketObserver.add(onSocketObserverNotify);
			Observer.add(TCPServer.OBSERVERS, socketObserver);

			socket.open(port);
		}

		//public

		//private
		private void onSocketObserverNotify(object sender, string evnt, dynamic args) {
			if (sender != socket) {
				return;
			}

			if (evnt == TCPServerEvent.OPENED) {
				Console.WriteLine("[Server] Opened on port " + socket.port + ".");
			} else if (evnt == TCPServerEvent.CONNECTION) {
				Console.WriteLine("[Server] Client #" + ((int) args) + " connected.");
			} else if (evnt == TCPServerEvent.CLIENT_DISCONNECTED) {
				Console.WriteLine("[Server] Client #" + ((int) args) + " disconnected.");
			} else if (evnt == TCPServerEvent.CLIENT_DOWNLOAD_COMPLETE) {
				//Console.WriteLine("[Server] Received " + args.data.Length + " bytes from client #" + args.client + ".");
				handlePacket(args.client, args.data);
			} else if (evnt == TCPServerEvent.ERROR) {
				Console.WriteLine("[Server] Error: " + args);
			} else if (evnt == TCPServerEvent.DEBUG) {
				//Console.WriteLine("[Server] Debug: " + args);
			}
		}

		private void sendPacket(int client, ushort type, byte[] data) {
			socket.send(client, PacketUtil.createPacket(data, type));
		}

		private void handlePacket(int client, byte[] packet) {
			ushort packetType = PacketUtil.getPacketType(packet);
			byte[] packetData = PacketUtil.getPacketData(packet);

			if (packetType == PacketType.DIFFIE_HELLMAN) {
				Console.WriteLine("[Server] Recieved DH key.");
				byte[] S = dh.S(packetData);
				key = Hash.sha256(ByteUtil.combine(ByteUtil.toByte("0keeP+attentioN+wateR+herE1+"), S));
				iv = Hash.sha256(ByteUtil.combine(ByteUtil.toByte("1-Knew-Carbon-Involved-State2"), S));
				aes = new Rijndael(key, iv);
				Console.WriteLine("[Server] DH key exchanged, AES key created.");
				sendPacket(client, PacketType.DIFFIE_HELLMAN, dh.AB);
				Console.WriteLine("[Server] Sent DH handshake.");
			} else if (packetType == PacketType.TEST) {
				Console.WriteLine("[Server] " + ByteUtil.toString(aes.decrypt(packetData)));
				Console.WriteLine("[Server] Sending encrypted test string.");
				sendPacket(client, PacketType.TEST, aes.encrypt(ByteUtil.toByte("Hello, client! I'm the server's test string.")));
			}
		}
	}
}