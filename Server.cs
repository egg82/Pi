using System;
using Network;
using Crypto;
using Patterns.Observer;
using Events.Network;
using System.IO;
using Enums;
using System.Collections.Generic;
using System.Text;

namespace Pi {
	public class Server {
		//vars
		private TCPServer socket = new TCPServer(128, false);
		private DiffieHellman dh = new DiffieHellman();
		private Hash hash = new Hash();
		private Rijndael aes;

		private Observer socketObserver = new Observer();

		private MemoryStream packetBuilder = new MemoryStream();
		private List<MemoryStream> packetBuffer = new List<MemoryStream>();

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
				packetBuffer.Add(new MemoryStream());
			} else if (evnt == TCPServerEvent.CLIENT_DOWNLOAD_COMPLETE) {
				readPackets(args.client, args.data);
			} else if (evnt == TCPServerEvent.CLIENT_DISCONNECTED) {
				packetBuffer[args].Close();
				packetBuffer[args] = null;
			} else if (evnt == TCPServerEvent.ERROR) {
				Console.WriteLine("[Server] Error: " + args);
			}
		}

		private void sendPacket(int client, ushort type, byte[] data) {
			packetBuilder.SetLength(0L);
			packetBuilder.Write(BitConverter.GetBytes(type), 0, 2);
			packetBuilder.Write(data, 0, data.Length);

			socket.send(client, PacketHelper.writePacket(packetBuilder.ToArray()));
		}
		private ushort getPacketType(byte[] data) {
			return BitConverter.ToUInt16(data, 0);
		}
		private byte[] getPacketData(byte[] data) {
			byte[] newData = new byte[data.Length - 2];
			Buffer.BlockCopy(data, 2, newData, 0, data.Length - 2);
			return newData;
		}

		private void readPackets(int client, byte[] data) {
			int bufferRead = 0;
			byte[] stream = new byte[packetBuffer[client].Length + data.Length];

			Buffer.BlockCopy(packetBuffer[client].ToArray(), 0, stream, 0, (int) packetBuffer[client].Length);
			Buffer.BlockCopy(data, 0, stream, (int) packetBuffer[client].Length, data.Length);

			do {
				byte[] nextPacket = PacketHelper.readPacket(stream, bufferRead);

				if (nextPacket == null) {
					break;
				}

				handlePacket(client, nextPacket);
				bufferRead += PacketHelper.getPacketLength(stream, bufferRead) + 2;
			} while (true);

			packetBuffer[client].SetLength(0L);

			if (stream.Length - bufferRead > 0) {
				packetBuffer[client].Write(stream, bufferRead, stream.Length - bufferRead);
			}
		}
		private void handlePacket(int client, byte[] packet) {
			ushort packetType = getPacketType(packet);
			byte[] packetData = getPacketData(packet);

			if (packetType == PacketType.DIFFIE_HELLMAN) {
				Console.WriteLine("[Server] Recieved DH key.");
				byte[] S = dh.S(packetData);
				key = hash.generate256Key("0keeP+attentioN+wateR+herE1+", S);
				iv = hash.generate256Key("1-Knew-Carbon-Involved-State2", S);
				aes = new Rijndael(key, iv);
				Console.WriteLine("[Server] DH key exchanged, AES key created.");
				sendPacket(client, PacketType.DIFFIE_HELLMAN, dh.AB);
				Console.WriteLine("[Server] Sent DH handshake.");
			}
		}
	}
}