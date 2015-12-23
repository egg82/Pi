using Events.Patterns.Command;
using System;

namespace Patterns.Command {
	class ParallelCommand : Command {
		//vars
		private Command[] commands;
		private int completed;

		private Observer.Observer commandObserver = new Observer.Observer();

		//constructor
		public ParallelCommand(double delay = 0.0d, params Command[] commands) : base(delay) {
			this.commands = commands;
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

			for (int i = 0; i < commands.Length; i++) {
				commands[i].start();
			}
		}
		protected virtual void postExecute() {
			dispatch(CommandEvent.COMPLETE);
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

			if (evnt == CommandEvent.ERROR) {
				dispatch(CommandEvent.ERROR, data);
			}

			if (completed == commands.Length) {
				Observer.Observer.remove(OBSERVERS, commandObserver);
				postExecute();
			}
		}
	}
}
