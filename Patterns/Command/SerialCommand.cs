using Events.Patterns.Command;
using System;

namespace Patterns.Command {
	class SerialCommand : Command {
		//vars
		private object startData;
		private Command[] commands;
		private int completed;

		private Observer.Observer commandObserver = new Observer.Observer();

		//constructor
		public SerialCommand(double delay = 0.0d, object startData = null, params Command[] serializableCommands) : base(delay) {
			this.startData = startData;
			commands = serializableCommands;

			commandObserver.add(onCommandObserverNotify);
		}

		//public

		//private
		protected override void execute() {
			if (commands == null || commands.Length == 0) {
				dispatch(CommandEvent.COMPLETE);
				return;
			}

			Observer.Observer.add(OBSERVERS, commandObserver);
			completed = 0;

			commands[completed].startSerialized(startData);
		}
		protected virtual void postExecute(object data) {
			dispatch(CommandEvent.COMPLETE);
		}
		protected virtual void postExecuteError(object data) {
			dispatch(CommandEvent.ERROR);
		}

		private void onCommandObserverNotify(object sender, string evnt, dynamic data) {
			bool inArr = false;
			for (int i = 0; i < commands.Length; i++) {
				if (sender == commands[i]) {
					inArr = true;
					break;
				}
			}
			if (!inArr) {
				return;
			}

			completed++;

			if (evnt == CommandEvent.COMPLETE) {
				if (completed == commands.Length) {
					postExecute(data);
				} else {
					commands[completed].startSerialized(startData);
				}
			} else if (evnt == CommandEvent.ERROR) {
				Observer.Observer.remove(OBSERVERS, commandObserver);
				postExecuteError(data);
			}
		}
	}
}
