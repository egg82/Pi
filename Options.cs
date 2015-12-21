using System;
using CommandLine;

namespace Pi {
	public class Options {
		//vars
		[Option('h', "host", Required = false, DefaultValue = "127.0.0.1", HelpText = "Host to connect to.")]
		public string host {get; set;}
		[Option('p', "port", Required = false, DefaultValue = (ushort) 41123, HelpText = "Port to connect to / listen on.")]
		public ushort port {get; set;}
		[Option('c', "client", Required = true, HelpText = "True if client, false if server.")]
		public bool isClient {get; set;}

		//constructor
		public Options() {
			
		}

		//public

		//private

	}
}