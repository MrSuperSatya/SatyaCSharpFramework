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
	public class MsAccessConnector : Object//, IDBConnector
	{
		private OleDbConnection connection;
		private string sql;

		public OleDbConnection Connection
		{
			get { return connection; }
		}

		public MsAccessConnector(string connectionString)
		{
			connection = new OleDbConnection();
			openConnection(connectionString);
		}
		public MsAccessConnector(string filePath, bool isInTheSameDir)
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
		public string executeScalar(string sql) //return 1 value
		{
			OleDbCommand cmd = new OleDbCommand();
			cmd.Connection = connection;
			cmd.CommandText = sql;
			try
			{
				return cmd.ExecuteScalar().ToString();
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
			catch (OleDbException ode)
			{
				showDbErrorMsg(sql, ode.Message);
				return null;
			}
		}

		public string getAutoID(string tableName, string IDFieldName = "id")
		{
			string sql = "Select Max(" + IDFieldName + ")+1 From " + tableName;
			OleDbCommand cmd = new OleDbCommand();
			cmd.Connection = connection;
			cmd.CommandText = sql;
			try
			{
				object obj = cmd.ExecuteScalar();
				if (obj == null)
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
		public string getIdOf(string tableName, string fieldName, string fieldValue)
		{
			string sql = "Select id from " + tableName +
						 " Where " + fieldName + "=" + fieldValue;
			OleDbCommand cmd = new OleDbCommand();
			cmd.Connection = connection;
			cmd.CommandText = sql;
			try
			{
				return (string)cmd.ExecuteScalar();
			}
			catch (OleDbException e)
			{
				showDbErrorMsg(sql, e.Message);
				return null;
			}

			//validateString(ref value);
			//OleDbDataReader reader = getData("Select id from " +
			//    tableName + " Where " + field + "=" + value);
			//if (reader != null && reader.Read())
			//    return reader[0].ToString();

			//return "";
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

		public void insertFieldByField(string tableName, params string[] datas)
		{
			string sql = "Insert Into " + tableName + " VALUES(";
			foreach (string d in datas)
				sql = sql + d + ",";

			sql = sql.Substring(0, sql.Length - 1) + ")";
			executeNonQuery(sql);
		}
		public void startInsertOnePK(string tableName, string PKFieldName="Id")
		{
			sql = "Insert Into " + tableName + " Values(" +
				  getAutoID(tableName, PKFieldName) + ",";
		}
		public void startInsertMultiPK(string tableName)
		{
			sql = "Insert Into " + tableName + " Values(";
		}
		public void setInsert(string fieldValue, DBDataType dataType = DBDataType.Num)//For Add New
		{
			if (sql.StartsWith("Insert"))
			{
				validateString(ref fieldValue, dataType);
				sql += (fieldValue + ",");
			}
		}
		public void endInsert()
		{
			if (sql.StartsWith("Insert"))
			{
				sql = sql.Substring(0, sql.Length - 1);
				sql += ")";
				executeNonQuery(sql);
			}
			sql = "";
		}

		public void startUpdate(string tableName)
		{
			sql = "Update " + tableName + " Set ";
		}
		public void setUpdate(string fieldName, string fieldValue, DBDataType dataType = DBDataType.Num) //For Edit
		{
			if (sql.StartsWith("Update"))
			{
				validateString(ref fieldValue, dataType);
				sql += (fieldName + "=" + fieldValue + ",");
			}
		}
		public int endUpdateOnePK(string idVal, DBDataType dataType, string IDFieldName="Id")
		{
			if (sql.StartsWith("Update"))
			{
				validateString(ref idVal, dataType);
				sql = sql.Substring(0, sql.Length - 1) +
					  " Where " + IDFieldName + "=" + idVal;
				sql = "";
				return executeNonQuery(sql);
			}

			sql = "";
			return -1;
		}
		public int endUpdateMultiPK(string whereClause)
		{
			if (sql.StartsWith("Update"))
			{
				sql = sql.Substring(0, sql.Length - 1) + whereClause;
				sql = "";
				return executeNonQuery(sql);
			}
			sql = "";
			return -1;
		}

		public void deleteByID(string tableName, string IDFieldValue, string IDFieldName="Id")
		{
			validateString(ref IDFieldValue, DBDataType.Num);
			executeNonQuery(" Delete From " + tableName +
							" Where " + IDFieldName + "=" + IDFieldValue);
		}
		public void deleteByWhereClause(string tableName, string whereClause)
		{
			executeNonQuery(" Delete From " + tableName +
							" Where " + whereClause);
		}

		public void validateString(ref string s, DBDataType t)
		{
			if (s == null) return;

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
				if (s.Length == 0) s = "''";
				else
				{
					if (!s.StartsWith("'"))
						s = "\'" + s;
					if (!s.EndsWith("'"))
						s = s + "\'";
				}
			}
			else if (t == DBDataType.Date)
			{
				s = "Convert(Date, '" + s + "', 103)";
				DateTime d;
				if (!DateTime.TryParse(s, out d))
					showDataErrorMsg(s, "Date");
			}
		}

		public void setRowSource(ref Dictionary<string, string> dic, string sql)
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
				dic.Add(reader[0].ToString(), reader[1].ToString());
		}
		public void setRowSource(ref ListView lv, string sql, int columnCount)
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
			while (reader.Read())
			{
				ListViewItem lvi = new ListViewItem(reader[0].ToString());
				for (int i = 1; i < columnCount; i++)
					lvi.SubItems.Add(reader[i].ToString());
				lv.Items.Add(lvi);
			}
		}
		public void setRowSource(ref ComboBox cb, string sql)
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
				cb.Items.Add(reader[0].ToString());
		}
		public void setRowSource(ref ComboBox cb, string tableName, string fieldName)
		{
			setRowSource(ref cb, "Select " + fieldName + " From " + tableName);
		}

		public void showDbErrorMsg(string sql, string errorMessage)
		{
			MessageBox.Show("Query:\n" + sql + "\n -> " + errorMessage,
							"Database Error", MessageBoxButtons.OK,
							MessageBoxIcon.Error);
			System.Windows.Forms.Clipboard.SetText(sql);
		}
		public void showDataErrorMsg(string dataValue, string dataType)
		{
			sql = "";
			MessageBox.Show("'" + dataValue + "' is incorrect as " + dataType,
							"Incorrect Data", MessageBoxButtons.OK,
							MessageBoxIcon.Error);
		}			
	}
}