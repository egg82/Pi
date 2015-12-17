using System;
using Network;
using Patterns.Observer;

namespace Pi {
	public class Pi {
		//vars
		private TCPClient client = new TCPClient();
		private Observer clientObserver = new Observer();

		//constructor
		public Pi(string[] args) {
			clientObserver.add(onClientObserverNotify);
			Observer.add(TCPClient.OBSERVERS, clientObserver);

			client.connect("127.0.0.1", 41123);

			while (true) {
				Console.ReadLine();
			}
		}

		//public

		//private
		private void onClientObserverNotify(object sender, string evnt, object args) {
			if (sender != client) {
				return;
			}

			Console.WriteLine(evnt);
			Console.WriteLine(args);
		}
	}
}