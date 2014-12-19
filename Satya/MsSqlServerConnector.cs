using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.OleDb;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Data.SqlClient;

namespace Satya
{
	public enum DBDataType //use for string validate only
	{
	    Date,
		Num,
	    String
	}

	public class MsSqlServerConnector : Object, IDBConnector
	{
		private SqlConnection connection;		
		private string sql;

		public SqlConnection Connection
		{
			get { return connection; }
		}
		
		public MsSqlServerConnector(string connectionString)
		{
			connection = new SqlConnection();
			openConnection(connectionString);
		}
		public MsSqlServerConnector(string serverName, string databaseName) //serverName = USER-PC06, databaseName = SoCafe
		{
			connection = new SqlConnection();
			string connectionString = "Data Source=" + serverName +
									  "; Initial Catalog=" + databaseName +
									  "; Integrated Security=TRUE" +
									  "; MultipleActiveResultSets=True";
			openConnection(connectionString);
		}

		public bool isOpen() {
			return connection.State == System.Data.ConnectionState.Open;
		}
		public bool openConnection(string connectionString)
		{			
			connection.ConnectionString = connectionString;
			try { connection.Open(); }
			catch (Exception e)
			{
				showDbErrorMsg(connection.ConnectionString,
								 e.Message + ". Cannot connect to Server");
				return false;
			}
			return true;
		}
		public bool openConnection(string serverName, string databaseName)
		{
			string connectionString = "Data Source=" + serverName +
											  "; Initial Catalog=" + databaseName +
											  "; Integrated Security=TRUE";
			return openConnection(connectionString);
		}
		public void closeConnection()
		{
			if (connection.State == System.Data.ConnectionState.Open)
				connection.Close();
		}

		public int executeNonQuery(string sql) //executeNonQuery: Insert, Update, Delete
		{
			SqlCommand cmd = new SqlCommand();
			cmd.Connection = connection;
			cmd.CommandText = sql;
			try
			{
				return cmd.ExecuteNonQuery();
			}
			catch (SqlException e)
			{
				showDbErrorMsg(sql, e.Message);
				return -1;
			}
		}
		public string executeScalar(string sql) //return 1 value
		{
			SqlCommand cmd = new SqlCommand();
			cmd.Connection = connection;
			cmd.CommandText = sql;
			try
			{
				object obj = cmd.ExecuteScalar();
				if (obj != null) return obj.ToString();
				return "";
			}
			catch (SqlException e)
			{
				showDbErrorMsg(sql, e.Message);
				return "";
			}
		}
		public SqlDataReader executeReader(string sql) //return mutiple values
		{
			SqlCommand command = new SqlCommand();
			command.Connection = connection;
			command.CommandText = sql;			
			try
			{
				return command.ExecuteReader();
			}
			catch (SqlException e)
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

		public string getAutoID(string tableName, string IDFieldName = "id")
		{
			string sql = "Select Max(" + IDFieldName + ")+1 From " + tableName;
			SqlCommand cmd = new SqlCommand();
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
			catch (SqlException e)
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
				return executeScalar(sql);
			}
			catch (SqlException e)
			{
				showDbErrorMsg(sql, e.Message);
				return null;
			}		
		}

		public SqlDataReader selectAll(string tableName) {
			return executeReader("Select * From " + tableName);
		}
		public SqlDataReader select(params string[] fields)
		{
			string sql = "Select ";
			foreach (string field in fields)
				sql = sql + field + ",";

			sql = sql.Substring(0, sql.Length - 1);

			return executeReader(sql);
		} 
				
		public void startInsertOnePK(string tableName, string PKFieldName="Id")
		{
			sql = "Insert Into " + tableName + " Values(";
				//+ getAutoID(tableName, PKFieldName) + ",";
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
		public int endUpdateOnePK(string idVal, DBDataType dataType = DBDataType.Num, string IDFieldName="Id")
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
					s = s.Trim();
					if (!s.StartsWith("'"))
						s = "'" + s;
					if (!s.EndsWith("'"))
						s = s + "'";
					s = s.Replace("'", "''");
					s = s.Substring(1,s.Length-2);
				}
			}
			else if (t == DBDataType.Date)
			{				
				DateTime d;
				if (!DateTime.TryParse(s, out d))
				{					
					showDataErrorMsg(s, "Date");
					return;
				}
				s = "Convert(DateTime, '" + s + "', 103)";
				//if (!s.StartsWith("'"))
				//    s = "'" + s;
				//if (!s.EndsWith("'"))
				//    s = s + "'";
			}
		}

		public void setRowSource(ref Dictionary<string, string> dic, string sql)
		{
			SqlCommand command = new SqlCommand();
			command.Connection = connection;
			command.CommandText = sql;
			SqlDataReader reader;
			try
			{
				reader = command.ExecuteReader();
			}
			catch (SqlException e)
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
			SqlCommand command = new SqlCommand();
			command.Connection = connection;
			command.CommandText = sql;
			SqlDataReader reader;
			try
			{
				reader = command.ExecuteReader();
			}
			catch (SqlException e)
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
			SqlCommand command = new SqlCommand();
			command.Connection = connection;
			command.CommandText = sql;
			SqlDataReader reader;
			try
			{
				reader = command.ExecuteReader();
			}
			catch (SqlException e)
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





























/*
class DBTable : Object
	{
		private string name;;
		private List<Column> columns;

		internal List<Column> Columns
		{
		  get { return columns; }
		  set { columns = value; }
		}

		public void addColumn(Column column){
			columns.Add(column);
		}
	
		public void select(params string[] fields) {
			string sql = "Insert Into " + name + " VALUES(";
			foreach (string field in fields)
				sql = sql + field + ",";

			sql = sql.Substring(0, sql.Length - 1) + ")";
		}
		public void insert() { }
		public void update() { }
		public void delete() { }
	
	
	}
 
  
  class Column : Object
	{
		private string name;	
		private DBDataType dataType;		
		private EKeyType keyType;		
		private int length;
		private Control control;	

		public string Name
		{
			get { return name; }
			set { name = value; }
		}
		public DBDataType DataType
		{
			get { return dataType; }
			set { dataType = value; }
		}
		public EKeyType KeyType
		{
			get { return keyType; }
			set { keyType = value; }
		}
		public int Length
		{
			get { return length; }
			set { if(length>0)length = value; }
		}
		public Control Control
		{
			get { return control; }
			set { control = value; }
		}


		Column(string name,DBDataType dataType,EKeyType keyType, Control control) {
			this.name = name;
			this.dataType = dataType;
			this.keyType = keyType;
			this.control = control;
		}
		Column(string name, DBDataType dataType, EKeyType keyType, Control control, int length)
		{
			this.name = name;
			this.dataType = dataType;
			this.keyType = keyType;
			this.length = length;
			this.control = control;
		}
	}
*/