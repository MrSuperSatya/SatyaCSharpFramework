using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.OleDb;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;

namespace Satya
{
	public class SatMsAccessConnector : SatIDBConnector
	{
		protected OleDbConnection connection;
		protected string sql;

		public OleDbConnection Connection
		{
			get { return connection; }
		}

		public SatMsAccessConnector(string connectionString)
		{
			connection = new OleDbConnection();
			openConnection(connectionString);
		}
		public SatMsAccessConnector(string filePath, bool isInTheSameDir)
		{
			connection = new OleDbConnection();
			openConnection(filePath, isInTheSameDir);
		}

		public bool isOpen()
		{
			return connection.State == System.Data.ConnectionState.Open;
		}
		public bool openConnection(string connectionString)
		{
			connection.ConnectionString = connectionString;

			try { connection.Open(); }
			catch (Exception e)
			{
				showDbErrorMsg(connection.ConnectionString, e.Message + ". Cannot connection to Server");
				return false;
			}
			return true;
		}
		public bool openConnection(string filePath, bool isInTheSameDir)
		{
            connection.ConnectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" +
                                           filePath + ";Persist Security Info=False;";
			try { connection.Open(); }
			catch (Exception e)
			{
				showDbErrorMsg(connection.ConnectionString, e.Message + ". Cannot connect to Server");
				return false;
			}
			return true;
		}
		public void closeConnection()
		{
			if (connection.State == System.Data.ConnectionState.Open)
				connection.Close();
		}

		public int executeNonQuery(string sql) //executeNonQuery: Insert, Update, Delete
		{
			OleDbCommand cmd = new OleDbCommand();
			cmd.Connection = connection;
			cmd.CommandText = sql;
			try
			{
				return cmd.ExecuteNonQuery();
			}
			catch (OleDbException e)
			{
				showDbErrorMsg(sql, e.Message);
				return -1;
			}
		}
		public object executeScalar(string sql) //return 1 value
		{
			OleDbCommand cmd = new OleDbCommand();
			cmd.Connection = connection;
			cmd.CommandText = sql;
			try
			{
                return cmd.ExecuteScalar();
			}
			catch (OleDbException e)
			{
				showDbErrorMsg(sql, e.Message);
				return null;
			}
		}
		public OleDbDataReader executeReader(string sql) //return mutiple values
		{
			OleDbCommand command = new OleDbCommand();
			command.Connection = connection;
			command.CommandText = sql;
			try
			{
				return command.ExecuteReader();
			}
			catch (OleDbException e)
			{
				showDbErrorMsg(sql, e.Message);
				return null;
			}
			catch (InvalidOperationException e)
			{
				showDbErrorMsg(sql, e.Message);
				return null;
			}
		}
        
        public string getMaxID(string tableName, string IdFieldName = "Id")
        {
            string sql = "Select Max(" + IdFieldName + ") From " + tableName;
            OleDbCommand cmd = new OleDbCommand();
            cmd.Connection = connection;
            cmd.CommandText = sql;
            try
            {
                object obj = cmd.ExecuteScalar();
                if (obj == null || obj.ToString() == "")
                    return "0";
                else
                    return obj.ToString();
            }
            catch (OleDbException e)
            {
                showDbErrorMsg(sql, e.Message);
                return null;
            }
        }
		public string getAutoID(string tableName, string IdFieldName = "Id")
		{
			string sql = "Select Max(" + IdFieldName + ")+1 From " + tableName;
			OleDbCommand cmd = new OleDbCommand();
			cmd.Connection = connection;
			cmd.CommandText = sql;
			try
			{
				object obj = cmd.ExecuteScalar();
				if (obj == null || obj.ToString() == "")
					return "1";
				else
					return obj.ToString();
			}
			catch (OleDbException e)
			{
				showDbErrorMsg(sql, e.Message);
                return null;
			}
		}
		public string getIdOf(string tableName, string fieldName, string fieldValue, DBDataType dataType)
		{
			validateString(ref fieldValue, dataType);
			string sql = "Select id from " + tableName +
						 " Where " + fieldName + "=" + fieldValue;
			try
			{
				return executeScalar(sql).ToString();
			}
			catch (OleDbException e)
			{
				showDbErrorMsg(sql, e.Message);
				return null;
			}
		}

		public OleDbDataReader selectAll(string tableName)
		{
			return executeReader("Select * From " + tableName);
		}
		public OleDbDataReader select(params string[] fields)
		{
			string sql = "Select ";
			foreach (string field in fields)
				sql = sql + field + ",";

			sql = sql.Substring(0, sql.Length - 1);

			return executeReader(sql);
		}

		public void startInsertOnePK(string tableName, string PKFieldName = "Id")
		{
			sql = "Insert Into " + tableName + " Values( "
			      + getAutoID(tableName, PKFieldName) + ",";
		}
		public void startInsertMultiPK(string tableName)
		{
			sql = "Insert Into " + tableName + " Values(";
		}
		public void insert(string fieldValue, DBDataType dataType)
		{
			if (sql.StartsWith("Insert"))
			{
				validateString(ref fieldValue, dataType);
				sql += (fieldValue + ",");
			}
		}
		public int endInsert()
		{
			if (sql.StartsWith("Insert"))
			{
				sql = sql.Substring(0, sql.Length - 1);
				sql += ")";
				return executeNonQuery(sql);
			}
			sql = "";
            return -1;
		}

		public void startUpdate(string tableName)
		{
			sql = "Update " + tableName + " Set ";
		}
		public void update(string fieldName, string fieldValue, DBDataType dataType)
		{
			if (sql.StartsWith("Update"))
			{
				validateString(ref fieldValue, dataType);
				sql += (fieldName + "=" + fieldValue + ",");
			}
		}
		public int endUpdateOnePK(string idVal, DBDataType dataType = DBDataType.Num, string IDFieldName = "Id")
		{
			if (sql.StartsWith("Update"))
			{
				validateString(ref idVal, dataType);
				sql = sql.Substring(0, sql.Length - 1) +
					  " Where " + IDFieldName + "=" + idVal;
				string newSQL = sql;
				sql = "";
				return executeNonQuery(newSQL);
			}

			sql = "";
			return -1;
		}
		public int endUpdateMultiPK(string whereClause)
		{
			if (sql.StartsWith("Update"))
			{
				sql = sql.Substring(0, sql.Length - 1) + whereClause;
				string newSQL = sql;
				sql = "";
				return executeNonQuery(newSQL);
			}
			sql = "";
			return -1;
		}

		public void deleteByID(string tableName, string IdFieldValue, string IDFieldName = "Id")
		{
			validateString(ref IdFieldValue, DBDataType.Num);
			executeNonQuery(" Delete From " + tableName +
							" Where " + IDFieldName + "=" + IdFieldValue);
		}
		public void deleteByWhereClause(string tableName, string whereClause)
		{
			executeNonQuery(" Delete From " + tableName +
							" Where " + whereClause);
		}

		public void validateString(ref string s, DBDataType t)
		{
			if (s == null)
			{
				MessageBox.Show("Data is NULL. Please recheck your data.",
								"Incorrect Data", MessageBoxButtons.OK,
								MessageBoxIcon.Error);
				return;
			}

			if (t == DBDataType.Num)
			{
				s = s.Replace("$", "");
				s = s.Replace("R", "");
				s = s.Replace("Riel", "");
				s = s.Replace("riel", "");
				s = s.Replace(" ", "");

				double d;
				if (!double.TryParse(s, out d))
					showDataErrorMsg(s, "Number");
			}
			else if (t == DBDataType.String)
			{
				if (s.Length == 0) s = "\"\"";
				else
				{
					if (!s.StartsWith("\""))
						s = "\"" + s;
					if (!s.EndsWith("\""))
						s = s + "\"";
				}
			}
			else if (t == DBDataType.Date)
			{
				s = "#" + s + "#";
			}
		}
		
		public void setRowSource(Dictionary<string, string> dictionary, string sql)
		{
			OleDbCommand command = new OleDbCommand();
			command.Connection = connection;
			command.CommandText = sql;
			OleDbDataReader reader;
			try
			{
				reader = command.ExecuteReader();
			}
			catch (OleDbException e)
			{
				showDbErrorMsg(sql, e.Message);
				return;
			}
			if (reader == null || reader.FieldCount != 2) return;
			while (reader.Read())
				dictionary.Add(reader[0].ToString(), reader[1].ToString());
		}
		public void setRowSource(ListView listView, string sql, int columnCount)
		{
			OleDbCommand command = new OleDbCommand();
			command.Connection = connection;
			command.CommandText = sql;
			OleDbDataReader reader;
			try
			{
				reader = command.ExecuteReader();
			}
			catch (OleDbException e)
			{
				showDbErrorMsg(sql, e.Message);
				return;
			}
			if (reader == null) return;
			listView.Items.Clear();
			while (reader.Read())
			{
				ListViewItem lvi = new ListViewItem(reader[0].ToString());
				for (int i = 1; i < columnCount; i++)
					lvi.SubItems.Add(reader[i].ToString());
				listView.Items.Add(lvi);
			}
		}
		public void setRowSource(ListView listView, string sql)
		{
			setRowSource(listView, sql, listView.Columns.Count);
		}
		public void setRowSource(ListBox listBox, string sql) {
			OleDbCommand command = new OleDbCommand();
			command.Connection = connection;
			command.CommandText = sql;
			OleDbDataReader reader;
			try
			{
				reader = command.ExecuteReader();
			}
			catch (OleDbException e)
			{
				showDbErrorMsg(sql, e.Message);
				return;
			}
			listBox.Items.Clear();
			while (reader.Read())
				listBox.Items.Add(reader[0].ToString());
		}
		public void setRowSource(ComboBox comboBox, string sql)
		{
			OleDbCommand command = new OleDbCommand();
			command.Connection = connection;
			command.CommandText = sql;
			OleDbDataReader reader;
			try
			{
				reader = command.ExecuteReader();
			}
			catch (OleDbException e)
			{
				showDbErrorMsg(sql, e.Message);
				return;
			}
			comboBox.Items.Clear();
			while (reader.Read())
				comboBox.Items.Add(reader[0].ToString());
		}
		public void setRowSource(ComboBox comboBox, string fieldName, string tableName)
		{
			setRowSource(comboBox, "Select " + fieldName + " From " + tableName);
		}
		public void setRowSource(SatComboBox comboBox, string sql)
		{
			if (!sql.Contains(","))
				return;
			sql = sql.ToLower();
			string selectCluase = sql.Substring(0, sql.IndexOf(" from"));
			selectCluase = selectCluase.Replace("select", "");
			int indexOfComma = selectCluase.IndexOf(",");
			string field1 = selectCluase.Substring(0, indexOfComma);
			string field2 = selectCluase.Substring(indexOfComma + 1);
			sql = sql.Substring(sql.IndexOf(" from"));
			setRowSource((ComboBox)comboBox, "Select " + field2 + sql);
			setRowSource(comboBox.ListId, "Select " + field1 + sql);
		}
		public void setRowSource(SatComboBox comboBox, string fieldIdName, string filedDisplayName, string tableName)
		{
			setRowSource((ComboBox)comboBox, "Select " + filedDisplayName + " From " + tableName);
			setRowSource(comboBox.ListId, "Select " + fieldIdName + " From " + tableName);
		}
		public void setRowSource(List<string> list, string sql)
		{
			OleDbCommand command = new OleDbCommand();
			command.Connection = connection;
			command.CommandText = sql;
			OleDbDataReader reader;
			try
			{
				reader = command.ExecuteReader();
			}
			catch (OleDbException e)
			{
				showDbErrorMsg(sql, e.Message);
				return;
			}
			while (reader.Read())
				list.Add(reader[0].ToString());
		}

		public void showDbErrorMsg(string sql, string errorMessage)
		{
			System.Windows.Forms.Clipboard.SetText(sql);
			MessageBox.Show("Query:\n" + sql + "\n -> " + errorMessage,
							"Database Error", MessageBoxButtons.OK,
							MessageBoxIcon.Error);
		}
		public void showDataErrorMsg(string dataValue, string dataType)
		{
			System.Windows.Forms.Clipboard.SetText(dataValue);
			sql = "";
			MessageBox.Show("'" + dataValue + "' is incorrect as " + dataType,
							"Incorrect Data", MessageBoxButtons.OK,
							MessageBoxIcon.Error);
		}		
	}
}
