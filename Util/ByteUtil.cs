using Org.BouncyCastle.Utilities.Encoders;
using System;
using System.Text;

namespace Util {
	class ByteUtil {
		//vars

		//constructor
		public ByteUtil() {

		}

		//public
		public static byte[] combine(byte[] input1, byte[] input2) {
			byte[] temp = new byte[input1.Length + input2.Length];
			Buffer.BlockCopy(input1, 0, temp, 0, input1.Length);
			Buffer.BlockCopy(input2, 0, temp, input1.Length, input2.Length);
			return temp;
		}
		public static byte[] truncate(byte[] input, int start, int end = int.MaxValue) {
			if (start < 0) {
				start = 0;
			}
			if (end < 0) {
				end = 0;
			}

			if (start > input.Length) {
				start = input.Length;
			}
			if (end > input.Length) {
				end = input.Length;
			}

			if (end == start) {
				return new byte[0];
			}
			if (end < start) {
				int tempNum = start;
				start = end;
				end = tempNum;
			}

			byte[] temp = new byte[end - start];
			Buffer.BlockCopy(input, start, temp, 0, end - start);
			return temp;
		}

		public static byte[] toByte(string input) {
			return Encoding.UTF8.GetBytes(input);
		}
		public static string toString(byte[] input) {
			return Encoding.UTF8.GetString(input);
		}

		public static byte[] hex(byte[] input) {
			return Hex.Encode(input);
		}
		public static byte[] base64(byte[] input) {
			return Base64.Encode(input);
		}

		//private

	}
}
