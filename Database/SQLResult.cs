using System;

namespace Database {
	public class SQLResult {
		//vars
		private object[][] _data;
		private uint _rowId;
		private uint _rowsAffected;

		//constructor
		public SQLResult(object[][] data, uint rowId, uint rowsAffected) {
			_data = data;
			_rowId = rowId;
			_rowsAffected = rowsAffected;
		}

		//public
		public object[][] data {
			get {
				return _data;
			}
		}
		public uint rowId {
			get {
				return _rowId;
			}
		}
		public uint rowsAffected {
			get {
				return _rowsAffected;
			}
		}

		//private

	}
}