using System;
using System.Collections.Generic;

namespace Patterns.Observer {
	public class Observer {
		//vars
		private List<Action<object, string, dynamic>> listeners = new List<Action<object, string, dynamic>>();

		//constructor
		public Observer() {
			
		}

		//public
		public static void add(List<Observer> list, Observer observer) {
			if (list == null || observer == null) {
				return;
			}

			if (list.Contains(observer)) {
				return;
			}

			list.Add(observer);
		}
		public static void remove(List<Observer> list, Observer observer) {
			if (list == null || observer == null) {
				return;
			}

			int index = list.IndexOf(observer);

			if (index > -1) {
				list.RemoveAt(index);
			}
		}

		public static void dispatch(List<Observer> list, object sender, string evnt, object data) {
			if (list == null || list.Count == 0) {
				return;
			}

			for (int i = 0; i < list.Count; i++) {
				list[i].dispatch(sender, evnt, data);
			}
		}

		public void add(Action<object, string, dynamic> listener) {
			if (listener == null) {
				return;
			}

			if (listeners.Contains(listener)) {
				return;
			}

			listeners.Add(listener);
		}
		public void remove(Action<object, string, dynamic> listener) {
			if (listener == null) {
				return;
			}

			int index = listeners.IndexOf(listener);

			if (index > -1) {
				listeners.RemoveAt(index);
			}
		}

		public void dispatch(object sender, string evnt, object data) {
			for (int i = 0; i < listeners.Count; i++) {
				listeners[i].Invoke(sender, evnt, data);
			}
		}

		//private

	}
}