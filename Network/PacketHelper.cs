using System;

namespace Network {
	public class PacketHelper {
		//vars

		//constructor
		public PacketHelper() {
			
		}

		//public
		public static byte[] readPacket(byte[] stream, int pos) {
			ushort length = BitConverter.ToUInt16(stream, 0);
			if (stream.Length <= pos + length) {
				return null;
			}

			byte[] temp = new byte[length];
			Buffer.BlockCopy(stream, pos + 2, temp, 0, length);

			return temp;
		}
		public static byte[] writePacket(byte[] data) {
			byte[] temp = new byte[data.Length + 2];
			Buffer.BlockCopy(BitConverter.GetBytes((ushort) data.Length), 0, temp, 0, 2);
			Buffer.BlockCopy(data, 0, temp, 2, data.Length);

			return temp;
		}

		public static ushort getPacketLength(byte[] stream, int pos) {
			return BitConverter.ToUInt16(stream, pos);
		}

		//private

	}
}