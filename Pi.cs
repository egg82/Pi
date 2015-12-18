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
		private DiffieHellman client = new DiffieHellman();
		private DiffieHellman server = new DiffieHellman();
		private Rijndael aes;
		private Hash hash = new Hash();

		private byte[] key;
		private byte[] iv;

		private Stopwatch watch;

		//constructor
		public Pi(string[] args) {
			//server.open(41123);

			byte[] a = client.ab;
			byte[] A = client.AB;
			byte[] clientS;

			byte[] b = server.ab;
			byte[] B = server.AB;
			byte[] serverS;

			watch = Stopwatch.StartNew();
			for (int i = 0; i < 10; i++) {
				client.S(B);
				server.S(A);
			}
			watch.Stop();
			Console.WriteLine("DH key took " + ((double) watch.ElapsedMilliseconds / 10.0d) + "ms");

			clientS = client.S(B);
			serverS = server.S(A);

			//Console.WriteLine(Encoding.UTF8.GetString(clientS));
			//Console.WriteLine(Encoding.UTF8.GetString(serverS));

			watch = Stopwatch.StartNew();
			for (int i = 0; i < 100000; i++) {
				key = hash.generate256Key("0keeP+attentioN+wateR+herE1+", clientS);
				iv = hash.generate256Key("1-Knew-Carbon-Involved-State2", clientS);
			}
			watch.Stop();
			Console.WriteLine("AES key took " + ((double) watch.ElapsedMilliseconds / 100000.0d) + "ms");

			aes = new Rijndael(key, iv);
			/*Console.WriteLine(Encoding.ASCII.GetString(hash.hex(key)));
			Console.WriteLine(Encoding.ASCII.GetString(hash.hex(iv)));
			Console.WriteLine(Encoding.UTF8.GetString(aes.encrypt(Encoding.ASCII.GetBytes("Hello!"))));
			Console.WriteLine(Encoding.ASCII.GetString(aes.decrypt(aes.encrypt(Encoding.ASCII.GetBytes("Hello!")))));*/

			byte[] enc = Encoding.ASCII.GetBytes("Hello!");

			watch = Stopwatch.StartNew();
			for (int i = 0; i < 1000000; i++) {
				aes.encrypt(enc);
			}
			watch.Stop();
			Console.WriteLine("AES encrypt took " + ((double) watch.ElapsedMilliseconds / 1000000.0d) + "ms");

			enc = aes.encrypt(enc);

			watch = Stopwatch.StartNew();
			for (int i = 0; i < 1000000; i++) {
				aes.decrypt(enc);
			}
			watch.Stop();
			Console.WriteLine("AES decrypt took " + ((double) watch.ElapsedMilliseconds / 1000000.0d) + "ms");

			/*while (true) {
				Console.ReadLine();
			}*/
		}

		//public

		//private

	}
}