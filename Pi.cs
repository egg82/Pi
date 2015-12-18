using System;
using Network;
using Patterns.Observer;
using Events.Network;
using System.Text;
using Crypto;

namespace Pi {
	public class Pi {
		//vars
		private DiffieHellman client = new DiffieHellman();
		private DiffieHellman server = new DiffieHellman();
		private Rijndael aes;

		//constructor
		public Pi(string[] args) {
			//server.open(41123);

			byte[] a = client.ab;
			byte[] A = client.AB;
			byte[] clientS;

			byte[] b = server.ab;
			byte[] B = server.AB;
			byte[] serverS;

			clientS = client.S(B);
			serverS = server.S(A);

			//Console.WriteLine(Encoding.UTF8.GetString(clientS));
			//Console.WriteLine(Encoding.UTF8.GetString(serverS));

			aes = new Rijndael(clientS, client.AB);
			Console.WriteLine(Encoding.UTF8.GetString(aes.encrypt(Encoding.ASCII.GetBytes("Hello!"))));
			Console.WriteLine(Encoding.ASCII.GetString(aes.decrypt(aes.encrypt(Encoding.ASCII.GetBytes("Hello!")))));

			while (true) {
				Console.ReadLine();
			}
		}

		//public

		//private

	}
}