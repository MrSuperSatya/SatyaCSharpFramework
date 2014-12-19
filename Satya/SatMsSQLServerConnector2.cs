using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.IO;
using System.Drawing;
using System.Data;
using System.Reflection;

namespace Satya
{
	/// <summary>
	/// SatMsSqlServerConnector2 is an implementation of SatMsSqlServerConnector.
	/// New Features: Insert, Update, Delete can be used with TableName
	/// Connect to Controls: TextBox, ComboBox, Buttons, ...
	/// </summary>
	public class SatMsSQLServerConnector2 : SatMsSQLServerConnector
	{
        private List<SatMsSQLServerTable> tables;        
		private SatMsSQLServerTable tableForInsertAndUpdate;
        private SqlCommand command;
		private int columnIndexForInsertAndUpdate = -1;

        public List<SatMsSQLServerTable> Tables
        {
            get { return tables; }
            set { tables = value; }
        }

        /// <param name="connectionString"> Sql Server Connection String</param>
		public SatMsSQLServerConnector2(string connectionString)
			: base(connectionString) {
			tables = new List<SatMsSQLServerTable>();
            command = new SqlCommand();
            command.Connection = this.Connection;
			findAllTables(); 
		}
		public SatMsSQLServerConnector2(string serverName, string databaseName)
			: base(serverName, databaseName) { 			
			tables = new List<SatMsSQLServerTable>();
            command = new SqlCommand();
            command.Connection = this.Connection;
            findAllTables(); 
		}

		private void findAllTables()
		{
			SqlDataReader reader = executeReader("SELECT name FROM sys.Tables");
			while (reader.Read())
				tables.Add(new SatMsSQLServerTable(reader[0].ToString(), this));
		}
        public SatMsSQLServerTable getTableByName(string tableName)
		{
			foreach (SatMsSQLServerTable table in tables)
				if (table.Name.ToLower() == tableName.ToLower())
					return table;
			return null;
		}

		public new void startInsert(string tableName)
		{
            string sql = "Insert Into " + tableName + " Values(";
            SatMsSQLServerTable table = getTableByName(tableName);

            foreach (SatMsSQLServerTableField f in table.Fields)
            {
                if (f.KeyType != KeyType.PK)
                    sql += "@" + f.Name + ", ";
            }
            sql = sql.Substring(0, sql.Length - 2) + ")";
            command.CommandText = sql;
            command.Parameters.Clear();

            tableForInsertAndUpdate = table;
            columnIndexForInsertAndUpdate = 1;
		}
		public void insert(object fieldValue)
		{
            if (command.CommandText.StartsWith("Insert Into "))
            {
                DBDataType dataType = tableForInsertAndUpdate.Fields[columnIndexForInsertAndUpdate].DataType;
                String fieldName = tableForInsertAndUpdate.Fields[columnIndexForInsertAndUpdate].Name;
                SqlDbType sqlDataType = findSqlDbType(dataType);
                command.Parameters.Add("@" + fieldName, sqlDataType).Value = fieldValue;
                columnIndexForInsertAndUpdate++;
            }
		}
		public new int endInsert()
		{
            if (command.CommandText.StartsWith("Insert Into "))
            {
                int i = -1;
                try
                {
                    i = command.ExecuteNonQuery();
                }
                catch (SqlException e) {
                    showDbErrorMsg(" ", e.Message);
                }
                command.CommandText = "";
                return i;
            }

            command.CommandText = "";
            return -1;
		}

		public new void startUpdate(string tableName)
		{
            string sql = "Update " + tableName + " Set ";
            SatMsSQLServerTable table = getTableByName(tableName);
            
            foreach (SatMsSQLServerTableField f in table.Fields) {
                if (f.KeyType != KeyType.PK)
                    sql += "[" + f.Name + "] = @" + f.Name + ", ";
            }
            sql = sql.Substring(0, sql.Length - 2);
            sql += " Where [" + table.getPKField().Name + "] = @" + 
                   table.getPKField().Name;
            command.CommandText = sql;
            command.Parameters.Clear();

            tableForInsertAndUpdate = table;
            columnIndexForInsertAndUpdate = 1;
		}
        public void update(object fieldValue)
		{
            if (command.CommandText.StartsWith("Update"))
			{
                DBDataType dataType = tableForInsertAndUpdate.Fields[columnIndexForInsertAndUpdate].DataType;
                String fieldName = tableForInsertAndUpdate.Fields[columnIndexForInsertAndUpdate].Name;
                SqlDbType sqlDataType = findSqlDbType(dataType);
                command.Parameters.Add("@" + fieldName, sqlDataType).Value = fieldValue;
                columnIndexForInsertAndUpdate++;
			}
		}
		public int endUpdateOnePK(object idVal)
		{
            if (command.CommandText.StartsWith("Update"))
            {
                SatMsSQLServerTableField field = tableForInsertAndUpdate.getPKField();
                SqlDbType sqlDataType = findSqlDbType(field.DataType);
                command.Parameters.Add("@" + field.Name, sqlDataType).Value = idVal;

                int i = -1;
                try
                {
                    i = command.ExecuteNonQuery();
                }
                catch (SqlException e) { showDbErrorMsg(" ",e.Message); }
                command.CommandText = "";
                return i;
			}

            command.CommandText = "";
			return -1;
		}
		public new int endUpdateMultiPK(string whereClause)
		{
            if (command.CommandText.StartsWith("Update"))
            {
                string sql = command.CommandText;
                sql = sql.Substring(0,sql.IndexOf("Where [") + 6);
                sql += whereClause.ToLower().Replace("where",""); ;
                command.CommandText = sql;

                int i = command.ExecuteNonQuery();
                command.CommandText = "";
                return i;
            }

            command.CommandText = "";
            return -1;
		}

		public new void deleteByIDWithMsgBox(string tableName, string IdFieldValue, string IDFieldName = "Id")
		{
			DialogResult dr =
			MessageBox.Show("Are you sure want to delete " + tableName + " ?",
							connection.Database,
							MessageBoxButtons.YesNo,
							MessageBoxIcon.Question,
							MessageBoxDefaultButton.Button2);
			if (dr == DialogResult.Yes)
				deleteByID(tableName, IdFieldValue, IDFieldName);
		}
		public new void deleteByID(string tableName, string IdFieldValue, string IDFieldName = "Id")
		{
			validateString(ref IdFieldValue, DBDataType.Num);
			executeNonQuery(" Delete From " + tableName +
							" Where " + IDFieldName + "=" + IdFieldValue);
		}
		public new void deleteByWhereClause(string tableName, string whereClause)
		{
			executeNonQuery(" Delete From " + tableName +
							" Where " + whereClause);
		}
		public new void deleteByWhereClauseWithMsgBox(string tableName, string whereClause)
		{
			DialogResult dr =
			MessageBox.Show("Are you sure want to delete " + tableName + " ?",
							connection.Database,
							MessageBoxButtons.YesNo,
							MessageBoxIcon.Question,
							MessageBoxDefaultButton.Button2);
			if (dr == DialogResult.Yes)
				executeNonQuery(" Delete From " + tableName +
								" Where " + whereClause);
		}
		
		public void insertTable(string tableName)
		{			
			SatMsSQLServerTable table = getTableByName(tableName);
			if (table != null)
			{
				if (table.HasControl)
				{
                    startInsert(tableName);
					foreach (SatMsSQLServerTableField field in table.Fields)
						if (table.Fields.IndexOf(field) != 0)
						{
                            if (field.Control is SatComboBox)
                            {
                                if (((SatComboBox)field.Control).SelectedIndex >= 0)
                                    insert(((SatComboBox)field.Control).getSelectedId());
                                else
                                    return;
                            }
                            else if (field.Control is TextBox || field.Control is ComboBox)
                                insert(field.Control.Text);
                            else if (field.Control is DateTimePicker)
                                insert(((DateTimePicker)field.Control).Value);
                            else if (field.Control is PictureBox)
                            {
                                Image image;
                                if (((PictureBox)field.Control).Image == null)
                                {
                                    image = Image.FromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("Satya.Resources.nopicture.png"));
                                    ((PictureBox)field.Control).Image = image;
                                }
                                else
                                    image = ((PictureBox)field.Control).Image;
                                insert(SatMethods.imageToByteArray(image));
                            }
                            else if (field.Control is CheckBox)
                            {
                                if (((CheckBox)field.Control).Checked)
                                    insert("1");
                                else
                                    insert("0");
                            }
						}
                    endInsert();
				}
				else
					MessageBox.Show("No Controls for table: '" + tableName + "'.\n" +
				                    " Please insert controls in to table.",
							        "Database Error", MessageBoxButtons.OK,
							        MessageBoxIcon.Error);	
			}
		}
		public int updateTableOnePK(string tableName) 
		{ 
			SatMsSQLServerTable table = getTableByName(tableName);
			if (table != null)
			{
				if (table.HasControl)
				{
					string idValue = table.Fields[0].Control.Text;
					startUpdate(table.Name);
					foreach (SatMsSQLServerTableField field in table.Fields)
						if (table.Fields.IndexOf(field) != 0 )
						{
                            if (field.Control is SatComboBox)
                            {
                                if (((SatComboBox)field.Control).SelectedIndex >= 0)
                                    update(((SatComboBox)field.Control).getSelectedId());
                                else
                                    return -1;
                            }
                            else if (field.Control is TextBox || field.Control is ComboBox)
                                update(field.Control.Text);  
                            else if (field.Control is DateTimePicker)
                                update(((DateTimePicker)field.Control).Value);
                            else if (field.Control is PictureBox)
                                update(SatMethods.imageToByteArray(((PictureBox)field.Control).Image));
                            else if (field.Control is CheckBox)
                            {
                                if (((CheckBox)field.Control).Checked)
                                    update("1");
                                else
                                    update("0");
                            }                            
						}
					return endUpdateOnePK(idValue);
				}
				else
					MessageBox.Show("No controls for table: '" + tableName + "'.\n" +
									" Please insert controls in to table.",
									"Database Error", MessageBoxButtons.OK,
									MessageBoxIcon.Error);
			}
			return 0;
		}
		public int updateTableMutiPK(string tableName, string whereClause)
		{
			SatMsSQLServerTable table = getTableByName(tableName);
			if (table != null)
			{
				startUpdate(table.Name);
                foreach (SatMsSQLServerTableField field in table.Fields)
                    if (table.Fields.IndexOf(field) != 0 && field.DataType != DBDataType.Picture)
                    {
                        if (field.Control is SatComboBox)
                        {
                            if (((SatComboBox)field.Control).SelectedIndex >= 0)
                                update(((SatComboBox)field.Control).getSelectedId());
                            else
                                return -1;
                        }
                        else if (field.Control is TextBox || field.Control is ComboBox || field.Control is DateTimePicker)
                            update(field.Control.Text);
                        else if (field.Control is PictureBox)
                            update(SatMethods.imageToByteArray(((PictureBox)field.Control).Image));
                        else if (field.Control is CheckBox)
                        {
                            if (((CheckBox)field.Control).Checked)
                                update("1");
                            else
                                update("0");
                        }      
                    }
				return endUpdateMultiPK(whereClause);
			}
			else
				MessageBox.Show("No controls for table: '" + tableName + "'.\n" +
								" Please insert controls in to table.",
								"Database Error", MessageBoxButtons.OK,
								MessageBoxIcon.Error);
			return 0;
		}
		public void deleteTableById(string tableName)
		{
			SatMsSQLServerTable table = getTableByName(tableName);
			if (table != null)
				deleteByID(tableName, table.Fields[0].Control.Text, table.Fields[0].Name);
		}
		public void deleteTableByIdWithMsgBox(string tableName)
		{
			SatMsSQLServerTable table = getTableByName(tableName);
			if (table != null)
			{
				DialogResult dr =
				MessageBox.Show("Are you sure want to delete " + tableName + " ?",
					 			Connection.Database,
								MessageBoxButtons.YesNo,
								MessageBoxIcon.Question,
								MessageBoxDefaultButton.Button2);
				if (dr == DialogResult.Yes)
					deleteByID(tableName, table.Fields[0].Control.Text, table.Fields[0].Name);
			}
		}

		/// <summary>
		/// Add all Controls such as TextBox or ComboBox that represents each field of the Table
		/// </summary>
		/// <param name="tableName">The name of the Table</param>
		/// <param name="controls">Controls that represent each field of the Table</param>
		public void addControls(string tableName, params Control[] controls)
		{
			SatMsSQLServerTable table = getTableByName(tableName);
			if (table != null)
				table.addControls(controls);
		}
		public void addButtons(string tableName, params Button[] buttons)
		{
			SatMsSQLServerTable table = getTableByName(tableName);
			if (table != null)
				table.addButtons(buttons);
		}
        public void addListView(string tableName, SatListView listView, int searchColumnIndex = 1, ListViewSearchMode searchMode = ListViewSearchMode.Hide)
		{
			SatMsSQLServerTable table = getTableByName(tableName);
			if (table != null)
				table.addListView(listView, searchColumnIndex, searchMode);
		}
        public void addListView(string tableName, SatListView listView, ListViewSearchMode searchMode)
        {
            addListView(tableName, listView, 1 , searchMode);
        }
		public void addSearchTextBox(string tableName, SatTextBox serachTextBox)
		{
			SatMsSQLServerTable table = getTableByName(tableName);
			if (table != null)
				table.addSearchTextBox(serachTextBox);
		}
        public void addPictureBox(string tableName, PictureBox pictureBox)
        {
            SatMsSQLServerTable table = getTableByName(tableName);
            if (table != null)
                table.addpictureBox(pictureBox);
        }
        public void addPictureBrowseLabel(string tableName, LinkLabel linkLabel) {
            SatMsSQLServerTable table = getTableByName(tableName);
            if (table != null)
                table.addPictureBrowseLabel(linkLabel);
        }

        private SqlDbType findSqlDbType(DBDataType dbDataType) {
            if (dbDataType == DBDataType.String)
                return SqlDbType.NVarChar;
            else if (dbDataType == DBDataType.Num)
                return SqlDbType.Decimal;
            else if (dbDataType == DBDataType.Date)
                return SqlDbType.DateTime;
            else //if (dbDataType == DBDataType.Picture)
                return SqlDbType.Image;
        }
        
    }
}
