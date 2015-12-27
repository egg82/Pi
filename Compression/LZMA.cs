using System;
using SevenZip;

namespace Compression {
	public class LZMA {
		//vars

		//constructor
		public LZMA() {
			
		}

		//public
		public static byte[] compress(byte[] input) {
			return SevenZipCompressor.CompressBytes(input);
		}
		public static byte[] decompress(byte[] input) {
			return SevenZipExtractor.ExtractBytes(input);
		}

		//private

	}
}