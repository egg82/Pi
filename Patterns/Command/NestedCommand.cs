using Events.Patterns.Command;
using System;

namespace Patterns.Command {
	class NestedCommand : Command {
		//vars
		private Command command;
		private Observer.Observer commandObserver = new Observer.Observer();

		//constructor
		public NestedCommand(Command command, double delay = 0.0d) : base(delay) {
			this.command = command;
			commandObserver.add(onCommandObserverNotify);
		}

		//public

		//private
		protected override void execute() {
			if (command == null) {
				return;
			}

			Observer.Observer.add(OBSERVERS, commandObserver);
			command.start();
		}
		protected virtual void postExecute(object data) {
			dispatch(CommandEvent.COMPLETE);
		}
		protected virtual void postExecuteError(object data) {
			dispatch(CommandEvent.ERROR);
		}

		private void onCommandObserverNotify(object sender, string evnt, dynamic data) {
			if (sender != command) {
				return;
			}

			Observer.Observer.remove(OBSERVERS, commandObserver);
			if (evnt == CommandEvent.COMPLETE) {
				postExecute(data);
			} else if (evnt == CommandEvent.ERROR) {
				postExecuteError(data);
			}
		}
	}
}
