using Events.Database;
using Patterns.Observer;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Text.RegularExpressions;
using Util;

namespace Database {
	class SQLite : IDispatchable {
		//vars
		public static readonly List<Observer> OBSERVERS = new List<Observer>();

		private SQLiteConnection connection = null;
		private bool available = true;

		private string _path = null;

		//constructor
		public SQLite() {

		}

		//public
		public void connect(string path, string pass = null, bool compact = true) {
			if (connection != null) {
				dispatch(SQLiteEvent.ERROR, "SQLite is already connected to " + _path + ".");
				return;
			}
			if (path == null || path == "") {
				dispatch(SQLiteEvent.ERROR, "Path cannot be null.");
				return;
			}

			available = false;

			if (FileUtil.exists(path) && FileUtil.isDirectory(path)) {
				FileUtil.deleteDirectory(path);
			}
			if (!FileUtil.exists(path)) {
				SQLiteConnection.CreateFile(path);
			}

			connection = new SQLiteConnection("Data Source=" + path + "; Version=3;" + ((pass != null) ? " Password=" + pass + ";" : ""));
		}
		public void disconnect() {

		}

		public void query(string q) {
			if (connection == null) {
				return;
			}

			SQLiteCommand command = connection.CreateCommand();
			command.CommandType = CommandType.Text;
			command.CommandText = q;


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
				return false;
			}
		}

		public void dispatch(string evnt, object data = null) {
			Observer.dispatch(OBSERVERS, this, evnt, data);
		}

		//private

	}
}
