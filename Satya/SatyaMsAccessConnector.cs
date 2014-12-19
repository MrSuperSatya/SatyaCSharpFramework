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
	public class SatyaMsAccessConnector : object
	{
		public OleDbConnection connection;
		private bool isOpen = false;
		private string sql;

		public SatyaMsAccessConnector(string filePath, bool isInTheSameDir = false)
		{
			connection = new OleDbConnection();
			connection.ConnectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" +
									   filePath + ";Persist Security Info=False;";
			
			try { connection.Open(); }
			catch (Exception e)
			{
				showErrorMessage(connection.ConnectionString, e.Message + ". Cannot connection to Server");
				return ;
			}
			isOpen = true;        
		}
		public bool openConnection(string filePath)
		{
			connection = new OleDbConnection();
			connection.ConnectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" +
										filePath + ";Persist Security Info=False;";
			try { connection.Open(); }
			catch (Exception e)
			{
				showErrorMessage(connection.ConnectionString, e.Message + ". Cannot connect to Server");
				return false;
			}
			isOpen = true;
			return true;           		
		}
		public void closeConnection()
		{
			if (isOpen)
			{
				connection.Close();
				isOpen = false;
			}
		}
		public string executeScalar(string sql)
		{
			OleDbCommand command = new OleDbCommand();
			command.Connection = connection;			
			command.CommandText = sql;
			try
			{
				return (string)command.ExecuteScalar();
			}
			catch (OleDbException e)
			{
				showErrorMessage(sql, e.Message);
				return null;
			}
		}
		public int executeNonQuery(string sql)
		{
			OleDbCommand command = new OleDbCommand();
			command.Connection = connection;
			command.CommandText = sql;
			try
			{
				return command.ExecuteNonQuery();
			}
			catch (OleDbException e)
			{
				showErrorMessage(sql, e.Message);
				return -1;
			}
		}
		public int runQuery(string sql)
		{
			return executeNonQuery(sql);
		}
		public OleDbDataReader executeReader(string sql)
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
				showErrorMessage(sql, e.Message);
				return null;
			}
		}
		public OleDbDataReader getData(string sql)
		{
			return executeReader(sql);
		}
		public string getAutoID(string tb, string idField = "id")
		{
			string sql = "Select Max(" + idField + ")+1 From " + tb;
			OleDbDataReader reader = getData(sql);
			if (reader != null && reader.Read())
			{
				if (reader[0].ToString().Length == 0)
					return "1";
				return reader[0].ToString();
			}
			return "1";
		}
		public string getIdOf(string tableName, string field, string value)
		{
			validateString(ref value);
			OleDbDataReader reader = getData("Select id from " +
				tableName + " Where " + field + "=" + value);
			if (reader != null && reader.Read())
				return reader[0].ToString();

			return "";
		}
		public void insertData(string tb, string data)
		{
			string sql = "Insert Into " + tb + " VALUES(" +
				getAutoID(tb) + "," + data + ")";
			runQuery(sql);
		}
		public void insertDataFieldByField(string tb, params string[] data)
		{
			string sql = "Insert Into " + tb + " VALUES(";
			foreach (string d in data)
				sql = sql + d + ",";

			sql = sql.Substring(0, sql.Length - 1) + ")";
			runQuery(sql);
		}
		public void addNew1PK(string tb, string PKField = "id")
		{
			sql = "Insert Into " + tb + " Values(" +
				  getAutoID(tb, PKField) + ",";
		}
		public void addNewMultiPK(string tb)
		{
			sql = "Insert Into " + tb + " Values(";
		}
		public void startEdit(string tableName)
		{
			sql = "Update " + tableName + " Set ";
		}
		public void startUpdate(string tableName)
		{
			sql = "Update " + tableName + " Set ";
		}
		public void validateString(ref string s)
		{
			if (s == null) return;
			double a;
			s = s.Replace("$", "");
			if (s.Length > 0)
			{
				if (s[0] == '\'' || s[0] == '#')
					return;
				else if (s.Contains('/'))
					s = "#" + s + "#";
				else if (!double.TryParse(s, out a))
					s = "'" + s + "'";
			}
		}
		public void setEdit(string field, string value, bool isString = false) //For Edit
		{
			if (sql.StartsWith("Update"))
			{
				if (isString)
					value = "'" + value + "'";
				validateString(ref value);
				sql += (field + "=" + value + ",");
			}
		}
		public void setAddNew(string value, bool isString = false)//For Add New
		{
			if (sql.StartsWith("Insert"))
			{
				if (isString)
					value = "'" + value + "'";
				validateString(ref value);
				sql += (value + ",");
			}
		}
		public void endEdit1PK(string idField, string idVal)
		{
			if (sql.StartsWith("Update"))
			{
				validateString(ref idVal);
				sql = sql.Substring(0, sql.Length - 1) +
					  " Where " + idField + "=" + idVal;
				runQuery(sql);
			}
			sql = "";
		}
		public void endEditMutiPK(string whereClause)
		{
			if (sql.StartsWith("Update"))
			{
				sql = sql.Substring(0, sql.Length - 1) + whereClause;
				runQuery(sql);
			}
			sql = "";
		}
		public void endAddNew()
		{
			if (sql.StartsWith("Insert"))
			{
				sql = sql.Substring(0, sql.Length - 1);
				sql += ")";
				runQuery(sql);
			}
			sql = "";
		}
		public void delete(string table, string val, string field = "id")
		{
			validateString(ref val);
			runQuery("Delete From " + table + " Where " + field + "=" + val);
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
				showErrorMessage(sql, e.Message);
				return;
			}
			if (reader == null || reader.FieldCount != 2) return;
			while (reader.Read())
				dic.Add(reader[0].ToString(), reader[1].ToString());
		}
		public void setRowSource(ref ListView lv, string sql, int c)
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
				showErrorMessage(sql, e.Message);
				return;
			}
			if (reader == null) return;
			while (reader.Read())
			{
				ListViewItem lvi = new ListViewItem(reader[0].ToString());
				for (int i = 1; i < c; i++)
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
				showErrorMessage(sql, e.Message);
				return;
			}
			while (reader.Read())
			{
				cb.Items.Add(reader[0].ToString());
			}
		}
		public void setRowSource(ref ComboBox cb, string tb, string field)
		{
			setRowSource(ref cb, "Select " + field + " From " + tb);
		}
		public void showErrorMessage(string sql, string message)
		{
			MessageBox.Show("Query:\n" + sql + "\n -> " + message, "Database Error");
			System.Windows.Forms.Clipboard.SetText(sql);
		}
	}
}