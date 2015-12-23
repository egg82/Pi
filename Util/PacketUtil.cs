using System;

namespace Util {
	public class PacketUtil {
		//vars

		//constructor
		public PacketUtil() {

		}

		//public
		public static ushort getPacketType(byte[] packet) {
			if (packet.Length < 2) {
				return 0;
			}

			return BitConverter.ToUInt16(ByteUtil.truncate(packet, 0, 2), 0);
		}
		public static byte[] getPacketData(byte[] packet) {
			if (packet.Length < 2) {
				return packet;
			}

			return ByteUtil.truncate(packet, 2);
		}

		public static byte[] createPacket(byte[] data, ushort type) {
			return ByteUtil.combine(BitConverter.GetBytes(type), data);
		}

		//private

	}
}