using System;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Utilities.Encoders;
using System.Text;

namespace Crypto {
	public class Hash {
		//vars

		//constructor
		public Hash() {
			
		}

		//public
		public static byte[] sha256(byte[] input) {
			IDigest sha256D = new Sha256Digest();
			byte[] retArr = new byte[32];

			sha256D.BlockUpdate(input, 0, input.Length);
			sha256D.DoFinal(retArr, 0);

			return retArr;
		}

		public static byte[] generate256Key(string password, byte[] salt) {
			byte[] temp = Encoding.UTF8.GetBytes(password);
			byte[] temp2 = new byte[temp.Length + salt.Length];
			Buffer.BlockCopy(temp, 0, temp2, 0, temp.Length);
			Buffer.BlockCopy(salt, 0, temp2, temp.Length, salt.Length);
			return sha256(temp2);
		}

		public static byte[] hex(byte[] input) {
			return Hex.Encode(input);
		}

		//private

	}
}