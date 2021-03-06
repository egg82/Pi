﻿using System;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Utilities.Encoders;
using System.Text;

namespace Crypto {
	public class Hash {
		//vars
		private static IDigest sha256D = new Sha256Digest();

		//constructor
		public Hash() {
			
		}

		//public
		public static byte[] sha256(byte[] input) {
			byte[] retArr = new byte[32];

			lock (sha256D) {
				sha256D.BlockUpdate(input, 0, input.Length);
				sha256D.DoFinal(retArr, 0);
			}

			return retArr;
		}

		//private

	}
}