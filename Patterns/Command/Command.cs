using Events.Patterns.Command;
using Patterns.Observer;
using System;
using System.Collections.Generic;
using System.Timers;

namespace Patterns.Command {
	class Command : IDispatchable {
		//vars
		public static readonly List<Observer.Observer> OBSERVERS = new List<Observer.Observer>();

		private Timer timer;
		private object _data;

		//constructor
		public Command(double delay = 0.0d) {
			if (delay <= 0) {
				return;
			}

			timer = new Timer(delay);
			timer.Elapsed += new ElapsedEventHandler(onTimer);
		}

		//public
		public void start() {
			if (timer != null) {
				timer.Start();
				return;
			}

			execute();
		}
		public void startSerialized(object data) {
			_data = data;
			start();
		}

		public object data {
			get {
				return _data;
			}
		}

		public void dispatch(string evnt, object data = null) {
			Observer.Observer.dispatch(OBSERVERS, this, evnt, data);
		}

		//private
		protected virtual void execute() {
			dispatch(CommandEvent.COMPLETE);
		}

		private void onTimer(object sender, ElapsedEventArgs e) {
			timer.Stop();
			dispatch(CommandEvent.TIMER);
			execute();
		}
	}
}
