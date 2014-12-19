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
	public enum EType
	{
	    Num,
	    String,
	    VarChar,
	    Date,
	    Char
	}

	public class SatyaMsSqlServerConnector : object
    {
		public SqlConnection connection;
		public SqlDataAdapter da;
		public bool isOpen = false;
        private string sql;

		public SatyaMsSqlServerConnector(string connectionString)
		{
			openConnection(connectionString);
		}
		public SatyaMsSqlServerConnector(string serverName, string databaseName) //serverName = USER-PC06, databaseName = SoCafe
        {
			string connectionString = "Data Source=" + serverName +
									  "; Initial Catalog=" + databaseName +
									  "; Integrated Security=TRUE";
			openConnection(connectionString);      
        }		
		public bool openConnection(string connectionString)
		{
			connection = new SqlConnection();
			if (da == null) da = new SqlDataAdapter();
			connection.ConnectionString = connectionString;
			try { connection.Open(); }
			catch (Exception e)
			{
				showDbErrorMsg(connection.ConnectionString, 
					             e.Message + ". Cannot connect to Server");
				return false;
			}
			isOpen = true;
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
			if (isOpen)
			{
				connection.Close();
				isOpen = false;
			}
        }
		public int executeNonQuery(string sql)
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
		public string executeScalar(string sql)
		{
			SqlCommand cmd = new SqlCommand();
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
		}
		public int runQuery(string sql) {
			return executeNonQuery(sql);
		}
		public SqlDataReader executeReader(string sql) {
			SqlCommand command = new SqlCommand();
			command.Connection = connection;
			command.CommandText = sql;
			try
			{
				return command.ExecuteReader();
			}
			catch (SqlException ode)
			{
				showDbErrorMsg(sql, ode.Message);
				return null;
			}
		}
		public SqlDataReader getData(string sql)
        {
			return executeReader(sql);
        }
        public string getAutoID(string tb, string idField = "id")
        {
            string sql = "Select Max(" + idField + ")+1 From " + tb;
			SqlCommand cmd = new SqlCommand();
			cmd.Connection = connection;
			cmd.CommandText = sql;
			try
			{
				object obj = cmd.ExecuteScalar();
				if(obj == null)
					return "1";
				else 
					return (string)obj;
			}
			catch (OleDbException e)
			{
				showDbErrorMsg(sql, e.Message);
				return null;
			}			
        }
        public string getIdOf(string tableName, string field, string value)
        {
			string sql = "Select id from " + tableName + 
			             " Where " + field + "=" + value;
			SqlCommand cmd = new SqlCommand();
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
        public void insertData(string tb, string data)
        {
            string sql = "Insert Into " + tb + " VALUES(" +
                getAutoID(tb) + "," + data + ")";
			executeNonQuery(sql);
        }
		public void insertDataFieldByField(string tb, params string[] data)
        {
            string sql = "Insert Into " + tb + " VALUES(";
            foreach (string d in data)
				sql = sql + d + ",";

			sql = sql.Substring(0, sql.Length - 1) + ")";
			executeNonQuery(sql);
        }
        public void startAddNew1PK(string tb, string PKField = "id")
        {
            sql = "Insert Into " + tb + " Values(" +
                  getAutoID(tb, PKField) + ",";
        }
		public void startAddNewMultiPK(string tb)
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
        public void validateString(ref string s, EType t)
        {
			if (s == null) return;
			
			if (t == EType.Num) {
				s = s.Replace("$", "");
				s = s.Replace("R", "");
				s = s.Replace("Riel", "");
				s = s.Replace("riel", "");
				s = s.Replace(" ", "");
				
				double d;
				if (! double.TryParse(s, out d))
					showDataErrorMsg(s, "Number");
			}
			else if (t == EType.String || t == EType.VarChar) {
				if (s.Length == 0) s = "''";
				else
				{
					if (!s.StartsWith("'"))
						s = "\'" + s;
					if (!s.EndsWith("'"))
						s = s + "\'";
				}
			}
			else if (t == EType.Date)
			{
				s = "Convert(Date, '" + s + "', 103)";
				DateTime d;
				if (! DateTime.TryParse(s, out d))
					showDataErrorMsg(s, "Date");
			}			
        }
		public void setEdit(string field, string value, EType t = EType.Num) //For Edit
        {
            if (sql.StartsWith("Update"))
            {
				validateString(ref value, t);
                sql += (field + "=" + value + ",");
            }
        }
		public void setAddNew(string value, EType t = EType.Num)//For Add New
        {
            if (sql.StartsWith("Insert"))
            {
				validateString(ref value, t);
                sql += (value + ",");
            }
        }
        public int endEdit1PK(string idVal, EType t, string idField="id")
        {
            if (sql.StartsWith("Update"))
            {
				validateString(ref idVal, t);
                sql = sql.Substring(0, sql.Length - 1) +
                      " Where " + idField + "=" + idVal;
				sql = "";
                return executeNonQuery(sql);
            }

			sql = "";
			return -1;
        }
		public int endEditMutiPK(string whereClause)
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
        public void endAddNew()
        {
            if (sql.StartsWith("Insert"))
            {
                sql = sql.Substring(0, sql.Length - 1);
                sql += ")";
				executeNonQuery(sql);
            }
            sql = "";
        }
        public void delete(string table, string value, EType t, string field = "id")
        {
			validateString(ref value, t);
			executeNonQuery("Delete From " + table + " Where " + field + "=" + value);
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
			catch (OleDbException e)
			{
				showDbErrorMsg(sql, e.Message);
				return;
			}
			if (reader == null || reader.FieldCount != 2) return;
			while (reader.Read())
				dic.Add(reader[0].ToString(), reader[1].ToString());
		}
		public void setRowSource(ref ListView lv, string sql, int c)
        {
			SqlCommand command = new SqlCommand();
			command.Connection = connection;
			command.CommandText = sql;
			SqlDataReader reader;
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
                for (int i = 1; i < c; i++)
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
			catch (OleDbException e)
			{
				showDbErrorMsg(sql, e.Message);
				return;
			}
            while (reader.Read())
                cb.Items.Add(reader[0].ToString());
        }
        public void setRowSource(ref ComboBox cb, string tb, string field)
        {
            setRowSource(ref cb, "Select " + field + " From " + tb);
        }
        public void showDbErrorMsg(string sql, string  errorMessage)
        {
			MessageBox.Show("Query:\n" + sql + "\n -> " + errorMessage,
							"Database Error", MessageBoxButtons.OK, 
							MessageBoxIcon.Error);
            System.Windows.Forms.Clipboard.SetText(sql);
        }
		public void showDataErrorMsg(string dataValue, string dataType) {
			sql = "";
			MessageBox.Show("'" + dataValue + "' is incorrect as " + dataType, 
				            "Incorrect Data", MessageBoxButtons.OK, 
							MessageBoxIcon.Error);
		}
    }
}