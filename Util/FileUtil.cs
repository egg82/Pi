using System;
using System.IO;

namespace Util {
	class FileUtil {
		//vars
		public static readonly string CWD = Directory.GetCurrentDirectory();
		public static readonly char SEPARATOR = Path.DirectorySeparatorChar;

		//constructor
		public FileUtil() {

		}

		//public
		public static bool exists(string path) {
			if (path == null) {
				return false;
			}

			return (Directory.Exists(path) || File.Exists(path)) ? true : false;
		}
		
		public static string getPathDirectory(string path) {
			if (path == null) {
				return null;
			}

			return Path.GetDirectoryName(path);
		}

		public static string getAbsolutePath(string path) {
			if (path == null) {
				return null;
			}

			return Path.GetFullPath(path);
		}
		public static string getFileName(string path) {
			if (path == null) {
				return null;
			}

			return Path.GetFileNameWithoutExtension(path);
		}
		public static string getFileExtension(string path) {
			if (path == null) {
				return null;
			}

			return Path.GetExtension(path);
		}
		public static string getFullFileName(string path) {
			if (path == null) {
				return null;
			}

			return Path.GetFileName(path);
		}

		public static void createDirectory(string path) {
			if (path == null) {
				return;
			}

			Directory.CreateDirectory(path);
		}
		public static void deleteDirectory(string path, bool recursive = true) {
			if (path == null) {
				return;
			}

			Directory.Delete(path, recursive);
		}

		public static void createFile(string path) {
			if (path == null) {
				return;
			}

			File.Create(path);
		}
		public static void deleteFile(string path) {
			if (path == null) {
				return;
			}

			File.Delete(path);
		}

		public static bool isDirectory(string path) {
			if (path == null) {
				return true;
			}

			return ((File.GetAttributes(path) & FileAttributes.Directory) == FileAttributes.Directory) ? true : false;
		}

		//private

	}
}
