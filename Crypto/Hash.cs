using System;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto;

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
			sha256D.DoFinal(retArr, 0);
			return retArr;
		}

		//private

	}
}