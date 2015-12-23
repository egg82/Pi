using System;
using Network;
using Patterns.Observer;
using Events.Network;
using System.Text;
using Crypto;
using System.Diagnostics;

namespace Pi {
	public class Pi {
		//vars
		/*private DiffieHellman client = new DiffieHellman();
		private DiffieHellman server = new DiffieHellman();
		private Rijndael aes;
		private Hash hash = new Hash();

		private byte[] key;
		private byte[] iv;*/

		private Server server;
		private Client client;
		private Options options;

		//constructor
		public Pi(string[] args) {
			options = new Options();
			if (!CommandLine.Parser.Default.ParseArguments(args, options)) {
				Console.WriteLine("Error: Could not parse arguments.");
				return;
			}

			/*byte[] a = client.ab;
			byte[] A = client.AB;
			byte[] clientS;

			byte[] b = server.ab;
			byte[] B = server.AB;
			byte[] serverS;

			clientS = client.S(B);
			serverS = server.S(A);

			Console.WriteLine(Encoding.UTF8.GetString(clientS));
			Console.WriteLine(Encoding.UTF8.GetString(serverS));

			key = hash.generate256Key("0keeP+attentioN+wateR+herE1+", clientS);
			iv = hash.generate256Key("1-Knew-Carbon-Involved-State2", clientS);

			aes = new Rijndael(key, iv);
			Console.WriteLine(Encoding.ASCII.GetString(hash.hex(key)));
			Console.WriteLine(Encoding.ASCII.GetString(hash.hex(iv)));
			Console.WriteLine(Encoding.UTF8.GetString(aes.encrypt(Encoding.ASCII.GetBytes("Hello!"))));
			Console.WriteLine(Encoding.ASCII.GetString(aes.decrypt(aes.encrypt(Encoding.ASCII.GetBytes("Hello!")))));*/

			if (options.isClient) {
				server = new Server(options.port);
				//client = new Client(options.host, options.port);
				client = new Client("www.google.com", 80);
			} else {
				server = new Server(options.port);
			}
		}

		//public
		public void wait() {
			while (true) {
				Console.ReadLine();
			}
		}

		//private

	}
}