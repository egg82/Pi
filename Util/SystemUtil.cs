using System;
using System.IO;

namespace Util {
	public class SystemUtil {
		//vars
		public static readonly PlatformID PLATFORM = Environment.OSVersion.Platform;
		public static readonly Version MACHINE_VERSION = Environment.OSVersion.Version;
		public static readonly bool IS_64 = Environment.Is64BitProcess;
		public static readonly string MACHINE_NAME = Environment.MachineName;
		public static readonly int PROCESSORS = Environment.ProcessorCount;
		public static readonly string USER = Environment.UserName;
		public static readonly Version MONO_VERSION = Environment.Version;

		public static readonly string CWD = Directory.GetCurrentDirectory();
		public static readonly char SEPARATOR = Path.DirectorySeparatorChar;

		//constructor
		public SystemUtil() {
			
		}

		//public

		//private

	}
}