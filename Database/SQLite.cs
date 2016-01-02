using Events.Database;
using Patterns.Observer;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Text.RegularExpressions;
using Util;
using System.Threading.Tasks;
using System.Data.Common;

namespace Database {
	class SQLite : IDispatchable {
		//vars
		public static readonly List<Observer> OBSERVERS = new List<Observer>();

		private SQLiteConnection connection = null;
		private bool available = true;
		private List<string> backlog = new List<string>();

		private string _path = null;
		private bool _connected = false;

		private Task<DbDataReader> lastQuery = null;

		//constructor
		public SQLite() {

		}

		//public
		public void connect(string path, string pass = null, bool compact = true) {
			if (connection != null || _connected) {
				dispatch(SQLiteEvent.ERROR, "SQLite is already connected to " + _path + ".");
				return;
			}
			if (path == null || path == "") {
				dispatch(SQLiteEvent.ERROR, "Path cannot be null.");
				return;
			}

			available = false;
			_connected = false;

			if (FileUtil.exists(path) && FileUtil.isDirectory(path)) {
				FileUtil.deleteDirectory(path);
			}
			if (!FileUtil.exists(path)) {
				SQLiteConnection.CreateFile(path);
			}

			try {
				connection = new SQLiteConnection("Data Source=" + path + "; Version=3;" + ((pass != null) ? " Password=" + pass + ";" : ""));
				connection.StateChange += new StateChangeEventHandler(onStateChange);
				connection.Commit += new SQLiteCommitHandler(onCommit);
				connection.OpenAsync();
			} catch (Exception ex) {
				dispatch(SQLiteEvent.ERROR, ex.Message);
				return;
			}

			backlog.Clear();
		}
		public void disconnect() {
			if (connection == null || !_connected) {
				return;
			}

			disconnectInternal();
		}

		public void query(string q) {
			if (connection == null || !_connected) {
				dispatch(SQLiteEvent.ERROR, "SQLite is not connected.");
				return;
			}

			if (!available || backlog.Count > 0) {
				backlog.Add(q);
			} else {
				available = false;
				queryInternal(q);
			}
		}
		/*public void query(SQLiteCommand q) {

		}

		public SQLiteCommand build(string input, params SQLiteParameter[] p) {
			if (connection == null) {
				return null;
			}

			SQLiteCommand retCom = connection.CreateCommand();
			retCom.CommandType = CommandType.Text;
			retCom.CommandText = input;
			retCom.Parameters.AddRange(p);

			return retCom;
		}*/
		public string sanitize(string input) {
			if (input == null) {
				return null;
			}

			return Regex.Replace(input, @"[\r\n\x00\x1a\\'""]", @"\$0");
		}

		public bool connected {
			get {
				return _connected;
			}
		}

		public void dispatch(string evnt, object data = null) {
			Observer.dispatch(OBSERVERS, this, evnt, data);
		}

		//private
		private void disconnectInternal() {
			try {
				connection.StateChange -= new StateChangeEventHandler(onStateChange);
				connection.Close();
				connection.Dispose();
			} catch (Exception ex) {
				dispatch(SQLiteEvent.ERROR, ex.Message);
				return;
			}

			connection = null;
			_path = null;
			_connected = false;
			lastQuery = null;
			backlog.Clear();

			GC.Collect();

			dispatch(SQLiteEvent.DISCONNECTED);
		}
		private void queryInternal(string q) {
			SQLiteCommand command = connection.CreateCommand();
			command.CommandType = CommandType.Text;
			command.CommandText = q;

			try {
				lastQuery = command.ExecuteReaderAsync();
			} catch (Exception ex) {
				dispatch(SQLiteEvent.ERROR, ex.Message);
			}
		}

		private void onStateChange(object sender, StateChangeEventArgs e) {
			if (e.CurrentState == ConnectionState.Open) {
				_connected = true;
			} else if (e.CurrentState == ConnectionState.Broken || e.CurrentState == ConnectionState.Closed) {
				disconnectInternal();
				_connected = false;
			}
		}
		private void onCommit(object sender, CommitEventArgs e) {
			List<object[]> rows = new List<object[]>();

			while (lastQuery.Result.Read()) {
				object[] row = new object[lastQuery.Result.FieldCount];
				lastQuery.Result.GetValues(row);
				rows.Add(row);
			}

			dispatch(SQLiteEvent.RESULT, new SQLResult(rows.ToArray(), (uint) connection.LastInsertRowId, (uint) lastQuery.Result.RecordsAffected));
			sendNext();
		}

		private void sendNext() {
			if (backlog.Count == 0) {
				available = true;
				return;
			}

			dispatch(SQLiteEvent.SEND_NEXT);

			string q = backlog[0];
			backlog.RemoveAt(0);
			queryInternal(q);
		}
	}
}
