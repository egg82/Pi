using System;

namespace Patterns.Observer {
	public interface IDispatchable {
		//functions
		void dispatch(string evnt, object data = null);
	}
}