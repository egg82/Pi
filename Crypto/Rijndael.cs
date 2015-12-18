using System;
using System.Security.Cryptography;

namespace Crypto {
	public class Rijndael {
		//vars
		private byte[] key = new byte[32];
		private byte[] iv;

		private RijndaelManaged managed;

		//constructor
		public Rijndael(byte[] key, byte[] iv) {
			Array.Copy(key, this.key, Math.Min(key.Length, this.key.Length));
			this.iv = iv;

			managed = new RijndaelManaged();
			managed.Mode = CipherMode.CFB;
			managed.Padding = PaddingMode.PKCS7;
			managed.KeySize = 256;
			managed.BlockSize = 256;
			managed.Key = this.key;
			managed.IV = this.iv;
		}

		//public
		public byte[] encrypt(byte[] input) {
			
		}
		public byte[] decrypt(byte[] input) {
			
		}

		//private

	}
}