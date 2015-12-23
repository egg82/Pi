using Events.Network;
using Events.Patterns.Command;
using Network;
using Patterns.Command;
using Patterns.Observer;
using System;
using Util;

namespace Commands {
	class ClientTCPCommand : Command {
		//vars
		private byte[] tcpData;
		private ushort type;
		private TCPClient client;

		private Observer tcpClientObserver = new Observer();

		//constructor
		public ClientTCPCommand(ushort type, byte[] data, TCPClient client, double delay = 0.0d) : base(delay) {
			tcpData = data;
			this.type = type;
			this.client = client;

			tcpClientObserver.add(onTcpClientObserverNotify);
		}

		//public

		//private
		protected override void execute() {
			Observer.add(TCPClient.OBSERVERS, tcpClientObserver);
			client.send(PacketUtil.createPacket(tcpData, type));
		}

		private void onTcpClientObserverNotify(object sender, string evnt, dynamic data) {
			if (sender != client) {
				return;
			}

			if(evnt == TCPClientEvent.ERROR) {
				Observer.remove(TCPClient.OBSERVERS, tcpClientObserver);
				dispatch(CommandEvent.ERROR, data);
			} else if (evnt == TCPClientEvent.DOWNLOAD_COMPLETE) {
				handleData(PacketUtil.getPacketType(data), PacketUtil.getPacketData(data));
			}
		}
		private void handleData(ushort type, byte[] data) {
			if (type != this.type) {
				return;
			}

			Observer.remove(TCPClient.OBSERVERS, tcpClientObserver);
			dispatch(CommandEvent.COMPLETE, new {
				type = type,
				data = data
			});
		}
	}
}
