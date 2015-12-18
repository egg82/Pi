using System;
using System.Security.Cryptography;

namespace Crypto {
	public class Rijndael {
		//vars
		private byte[] key = new byte[32];
		private byte[] iv = new byte[32];

		private ICryptoTransform encryptor;
		private ICryptoTransform decryptor;

		//constructor
		public Rijndael(byte[] key, byte[] iv) {
			Array.Copy(key, this.key, Math.Min(key.Length, this.key.Length));
			Array.Copy(iv, this.iv, Math.Min(iv.Length, this.iv.Length));

			RijndaelManaged managed;
			managed = new RijndaelManaged();
			managed.Mode = CipherMode.CFB;
			managed.Padding = PaddingMode.PKCS7;
			managed.KeySize = 256;
			managed.BlockSize = 256;
			managed.Key = this.key;
			managed.IV = this.iv;

			encryptor = managed.CreateEncryptor();
			decryptor = managed.CreateDecryptor();
		}

		//public
		public byte[] encrypt(byte[] input) {
			return encryptor.TransformFinalBlock(input, 0, input.Length);
		}
		public byte[] decrypt(byte[] input) {
			return decryptor.TransformFinalBlock(input, 0, input.Length);
		}

		//private

	}
}