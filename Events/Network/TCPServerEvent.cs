using System;

namespace Events.Network {
	public class TCPServerEvent {
		//vars
		public const string ERROR = "error";
		public const string DEBUG = "debug";
		public const string OPENED = "opened";
		public const string CLOSED = "closed";
		public const string CONNECTION = "connection";
		public const string CLIENT_ERROR = "clientError";
		public const string CLIENT_DISCONNECTED = "clientDisconnected";
		public const string CLIENT_CONNECTED = "clientConnected";
		public const string CLIENT_UPLOAD_PROGRESS = "clientUploadProgress";
		public const string CLIENT_UPLOAD_COMPLETE = "clientUploadComplete";
		public const string CLIENT_DOWNLOAD_PROGRESS = "clientDownloadProgress";
		public const string CLIENT_DOWNLOAD_COMPLETE = "clientDownloadComplete";

		//constructor
		public TCPServerEvent() {
			
		}

		//public

		//private

	}
}