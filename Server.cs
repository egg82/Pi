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
		private TCPServer socket = new TCPServer(1024, false);
		private DiffieHellman dh = new DiffieHellman();
		private Rijndael aes;

		private Observer socketObserver = new Observer();

		private MemoryStream packetBuilder = new MemoryStream();

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
				Console.WriteLine("[Server] Received " + args.data.Length + " bytes from client #" + args.client + ".");
				//readPackets(args.client, args.data);
			} else if (evnt == TCPServerEvent.ERROR) {
				Console.WriteLine("[Server] Error: " + args);
			} else if (evnt == TCPServerEvent.DEBUG) {
				//Console.WriteLine("[Server] Debug: " + args);
			}
		}
	}
}