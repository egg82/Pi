using System;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Utilities.Encoders;
using System.Text;

namespace Crypto {
	public class Hash {
		//vars
		private IDigest sha256D = new Sha256Digest();
		private int sha256S;

		//constructor
		public Hash() {
			sha256S = sha256D.GetDigestSize();
		}

		//public
		public byte[] sha256(byte[] input) {
			byte[] retArr = new byte[sha256S];
			sha256D.BlockUpdate(input, 0, input.Length);
			sha256D.DoFinal(retArr, 0);
			return retArr;
		}

		public byte[] generate256Key(string password, byte[] salt) {
			byte[] temp = Encoding.UTF8.GetBytes(password);
			byte[] temp2 = new byte[temp.Length + salt.Length];
			Buffer.BlockCopy(temp, 0, temp2, 0, temp.Length);
			Buffer.BlockCopy(salt, 0, temp2, temp.Length, salt.Length);
			return sha256(temp2);
		}

		public byte[] hex(byte[] input) {
			return Hex.Encode(input);
		}

		//private

	}
}