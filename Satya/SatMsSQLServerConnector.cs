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
    //use for string validate only
	public enum DBDataType 
	{
        String,
        Num,
        Date,	   
        Picture
	}

	public class SatMsSQLServerConnector : Object, SatIDBConnector
	{
		protected SqlConnection connection;		
		protected string sql;
        protected List<string> queryParas = new List<string>();

		public SqlConnection Connection
		{
			get { return connection; }
			set { connection = value; }
		}		
		
        public SatMsSQLServerConnector(string connectionString)
		{
			connection = new SqlConnection();
			openConnection(connectionString);
		}
		public SatMsSQLServerConnector(string serverName, string databaseName) 
		{
			connection = new SqlConnection();
			string connectionString = "Data Source=" + serverName +
									  "; Initial Catalog=" + databaseName +
									  "; Integrated Security=TRUE" +
									  "; MultipleActiveResultSets=True";
			openConnection(connectionString);
		}
                
        public bool openConnection(string connectionString)
		{
            connection.ConnectionString = connectionString;
			try { 
                connection.Open(); 
            }catch (Exception e) {
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
        public bool openLocalConnection(string databaseName)
        {
            string connectionString = "Data Source=" + System.Environment.MachineName +
                                      "; Initial Catalog=" + databaseName +
                                      "; Integrated Security=TRUE" +
                                      "; MultipleActiveResultSets=True";
            return openConnection(connectionString);
        }
        public void closeConnection()
		{
			if (connection.State == System.Data.ConnectionState.Open)
				connection.Close();
		}
        public bool isOpen()
        {
            return connection.State == System.Data.ConnectionState.Open;
        }

		public int executeNonQuery(string sql) //executeNonQuery: Insert, Update, Delete
		{
            SqlCommand command = new SqlCommand(sql, connection);
			try
			{
                return command.ExecuteNonQuery();
			}
			catch (SqlException e)
			{
				showDbErrorMsg(sql, e.Message);
				return -1;
			}
		}
		public object executeScalar(string sql) //return 1 value
		{
            SqlCommand command = new SqlCommand(sql, connection);
			try
			{
                return command.ExecuteScalar();
			}
			catch (SqlException e)
			{
				showDbErrorMsg(sql, e.Message);
				return null;
			}
		}
		public SqlDataReader executeReader(string sql) //return mutiple values
		{
            SqlCommand command = new SqlCommand(sql, connection);	
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

		public string getAutoID(string tableName, string IdFieldName = "ID")
		{
            string sql = " SELECT IDENT_CURRENT('" + tableName +
                         "') + IDENT_INCR('" + tableName + "')" ;  //From TableName
                         //"Select Max(" + IdFieldName + ")+1 From " + tableName;
			SqlCommand cmd = new SqlCommand();
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
			catch (SqlException e)
			{
				showDbErrorMsg(sql, e.Message);
				return null;
			}
		}
		public string getIdOf(string tableName, string fieldName, string fieldValue, DBDataType dataType)
		{
			//validateString(ref fieldValue, dataType);
			string sql = "Select id from " + tableName +
						 " Where " + fieldName + "= @FieldValue";
            SqlCommand cmd = new SqlCommand(sql,connection);
            cmd.Parameters.AddWithValue("@FieldValue", fieldValue);

			try
			{
                return cmd.ExecuteScalar().ToString();
			}
			catch (SqlException e)
			{
				showDbErrorMsg(sql, e.Message);
				return null;
			}		
		}

        [Obsolete("Deprecated: Use 'insertStart' instead")]
		public void startInsert(string tableName)
		{
			sql = "Insert Into " + tableName + " Values(";
		}
        public void insertStart(string tableName)
        {
            startInsert( tableName);
        }
		public void insert(string fieldValue, DBDataType dataType)
		{
			if (sql.StartsWith("Insert"))
			{
				validateString(ref fieldValue, dataType);
                sql += ("@para" + (queryParas.Count).ToString() + ",");
                queryParas.Add(fieldValue);
			}
		}
        [Obsolete("Deprecated: Use 'insertEnd' instead")]
        public int endInsert()
		{
			if (sql.StartsWith("Insert"))
			{
				sql = sql.Substring(0, sql.Length - 1);
				sql += ")";
                SqlCommand com = new SqlCommand(sql, connection);
                for (int i=0;i<com.Parameters.Count;i++)
                    com.Parameters["para" + i.ToString()].Value = queryParas[i];

                return com.ExecuteNonQuery();
			}
			sql = "";
            return -1;
		}
        public int insertEnd()
        {
            return endInsert();
        }

        [Obsolete("Deprecated: Use 'updateStart' instead")]
		public void startUpdate(string tableName)
		{
			sql = "Update " + tableName + " Set ";
		}
        public void updateStart(string tableName)
        {
            startUpdate(tableName);
        }
        public void update(string fieldName, string fieldValue, DBDataType dataType) 
		{
			if (sql.StartsWith("Update"))
			{
				validateString(ref fieldValue, dataType);
				sql += (fieldName + "=" + fieldValue + ",");
			}
		}
        [Obsolete("Deprecated: Use 'updateEndWithOnePK' instead")]
        public int endUpdateOnePK(string idVal, DBDataType dataType = DBDataType.Num, string IDFieldName="ID")
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
        public int updateEndWithOnePK(string idVal, DBDataType dataType = DBDataType.Num, string IDFieldName = "ID")
        {
            return endUpdateOnePK(idVal, dataType, IDFieldName);
        }
        [Obsolete("Deprecated: Use 'updateEndWithWhereClause' instead")]
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
        public int updateEndWithWhereWhereClause(string whereClause)
        {
            return endUpdateMultiPK(whereClause);
        }
        		
		public void deleteByID(string tableName, string IdFieldValue, string IDFieldName="ID")
		{			
				validateString(ref IdFieldValue, DBDataType.Num);
				executeNonQuery(" Delete From " + tableName +
								" Where " + IDFieldName + "=" + IdFieldValue);			
		}
        public void deleteByIDWithMsgBox(string tableName, string IdFieldValue, string IDFieldName = "Id")
        {
            DialogResult dr =
            MessageBox.Show("Are you sure want to delete '" + tableName + "' ?",
                            connection.Database,
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Question,
                            MessageBoxDefaultButton.Button2);
            if (dr == DialogResult.Yes)
                deleteByID(tableName, IdFieldValue, IDFieldName);
        }
        public void deleteByWhereClause(string tableName, string whereClause)
		{
			executeNonQuery(" Delete From " + tableName +
							" Where " + whereClause);
		}
		public void deleteByWhereClauseWithMsgBox(string tableName, string whereClause)
		{
			DialogResult dr =
			MessageBox.Show("Are you sure want to delete '" + tableName + "' ?",
							connection.Database,
							MessageBoxButtons.YesNo,
							MessageBoxIcon.Question,
							MessageBoxDefaultButton.Button2);
			if (dr == DialogResult.Yes)
				executeNonQuery(" Delete From " + tableName +
								" Where " + whereClause);
		}

		public void validateString(ref string s, DBDataType dataType)
		{
			if (s == null)
			{
				MessageBox.Show("Data is NULL. Please recheck your data.",
								"Incorrect Data", MessageBoxButtons.OK,
								MessageBoxIcon.Error);
				return;
			}

            if (dataType == DBDataType.Num)
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
            else if (dataType == DBDataType.String)
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
                    s = "N" + s.Substring(1, s.Length - 2);
                }
            }
            else if (dataType == DBDataType.Date)
            {
                //DateTime d;
                //if (!DateTime.TryParse(s, out d))
                //{
                //    showDataErrorMsg(s, "Date");
                //    return;
                //}
                s = "Convert(DateTime, '" + s + "', 103)";
                //if (!s.StartsWith("'"))
                //    s = "'" + s;
                //if (!s.EndsWith("'"))
                //    s = s + "'";
            }
            //else if (dataType == DBDataType.Picture)
            //    s = "0x5468697320697320612074657374";
		}

		public void setRowSource(Dictionary<string, string> dictionary, string sql)
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
				dictionary.Add(reader[0].ToString(), reader[1].ToString());
		}
        public void setRowSource(List<string> list, string sql)
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
            list.Clear();
            while (reader.Read())
                list.Add(reader[0].ToString());
        }

        public void setRowSource(ListView listView, string sql, int columnCount)
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
			listView.Items.Clear();
            
            while (reader.Read())
			{
				ListViewItem lvi = new ListViewItem(reader[0].ToString());
                for (int i = 1; i < columnCount; i++)
					lvi.SubItems.Add(reader[i].ToString());
				listView.Items.Add(lvi);
			}
            if (listView is SatListView)
                ((SatListView)listView).copyListViewItemsToList();
		}
		public void setRowSource(ListView listView, string sql)
		{
			setRowSource(listView, sql, listView.Columns.Count);
		}
		
		public void setRowSource(ListBox listBox, string sql)
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
			listBox.Items.Clear();
			while (reader.Read())
				listBox.Items.Add(reader[0].ToString());
		}
		public void setRowSource(ComboBox comboBox, string sql)
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
			comboBox.Items.Clear();
			while (reader.Read())
				comboBox.Items.Add(reader[0].ToString());
		}
		public void setRowSource(ComboBox comboBox, string fieldName, string tableName) {
			setRowSource(comboBox, "Select " + fieldName + " From " + tableName); 
		}
		public void setRowSource(SatComboBox comboBox, string sql) {
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
		public void setRowSource(SatComboBox comboBox, string fieldIdName, string filedDisplayName, string tableName) {
			setRowSource((ComboBox)comboBox, "Select " + filedDisplayName + " From " + tableName);
			setRowSource(comboBox.ListId, "Select " + fieldIdName + " From " + tableName);
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
			sql = "";
			if (dataValue != null)
				if(dataValue.Length > 0)
				{
					System.Windows.Forms.Clipboard.SetText(dataValue);
					MessageBox.Show("'" + dataValue + "' is incorrect as " + dataType,
									"Incorrect Data", MessageBoxButtons.OK,
									MessageBoxIcon.Error);
				}
		}			
	}
}
