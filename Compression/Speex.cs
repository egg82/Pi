using System;
using NSpeex;

namespace Compression {
	public class Speex {
		//vars
		private SpeexEncoder enc = new SpeexEncoder(BandMode.Wide);
		private SpeexDecoder dec = new SpeexDecoder(BandMode.Wide);

		//constructor
		public Speex() {
			
		}

		//public
		public byte[] compress(byte[] input) {
			short[] data = new short[input.Length / 2];
			byte[] encodedData = new byte[input.Length]; // Wait, how is this compressed?

			Buffer.BlockCopy(input, 0, data, 0, input.Length);

			lock (enc) {
				enc.Encode(data, 0, data.Length, encodedData, 0, encodedData.Length);
			}

			return encodedData;
		}
		public byte[] decompress(byte[] input) {
			short[] decodedFrame = new short[input.Length / 2];
			byte[] decodedData = new byte[input.Length]; // I really don't think this is a compression algo

			lock (dec) {
				dec.Decode(input, 0, input.Length, decodedFrame, 0, false);
			}

			Buffer.BlockCopy(decodedFrame, 0, decodedData, 0, decodedFrame.Length);

			return decodedData;
		}

		//private

	}
}