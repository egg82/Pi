using System;

namespace Events.Network {
	public class TCPClientEvent {
		//vars
		public const string CONNECTED = "connected";
		public const string ERROR = "error";
		public const string DEBUG = "debug";
		public const string DOWNLOAD_PROGRESS = "downloadProgress";
		public const string DOWNLOAD_COMPLETE = "downloadComplete";
		public const string UPLOAD_PROGRESS = "uploadProgress";
		public const string UPLOAD_COMPLETE = "uploadComplete";
		public const string DISCONNECTED = "disconnected";

		public const string SEND_NEXT = "sendNext";

		//constructor
		public TCPClientEvent() {
			
		}

		//public

		//private

	}
}