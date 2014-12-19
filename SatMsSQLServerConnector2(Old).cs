using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace Satya
{
	/// <summary>
	/// SatMsSqlServerConnector2 is an implementation of SatMsSqlServerConnector.
	/// New Features: Insert, Update, Delete can be used with TableName
	/// Connect to Controls: TextBox, ComboBox, Buttons, ...
	/// </summary>
	public class SatMsSQLServerConnector2 : SatMsSQLServerConnector
	{
        public List<SatMsSQLServerTable> tables;
		private SatMsSQLServerTable tableForInsertAndUpdate;
		private int columnIndexForInsertAndUpdate = -1;

		public SatMsSQLServerConnector2(string connectionString)
			: base(connectionString) {
			tables = new List<SatMsSQLServerTable>();
			findAllTables(); 
		}
		public SatMsSQLServerConnector2(string serverName, string databaseName)
			: base(serverName, databaseName) { 			
			tables = new List<SatMsSQLServerTable>();
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
			base.startInsert(tableName);
			tableForInsertAndUpdate = getTableByName(tableName);
			columnIndexForInsertAndUpdate = 0;
		}
		public void insert(string fieldValue)
		{
			if (sql.StartsWith("Insert"))
			{
				DBDataType dataType = tableForInsertAndUpdate.Fields[columnIndexForInsertAndUpdate].DataType;
				validateString(ref fieldValue, dataType);
				sql += (fieldValue + ",");
                columnIndexForInsertAndUpdate++;
			}
		}
		public new void endInsert()
		{
			base.endInsert();
			columnIndexForInsertAndUpdate = -1;
		}

		public new void startUpdate(string tableName)
		{
            base.startUpdate(tableName);
            tableForInsertAndUpdate = getTableByName(tableName);
            columnIndexForInsertAndUpdate = 1;
		}
		public void update(string fieldValue)
		{
			if (sql.StartsWith("Update"))
			{
                DBDataType dataType = tableForInsertAndUpdate.Fields[columnIndexForInsertAndUpdate].DataType;
                String fieldName = tableForInsertAndUpdate.Fields[columnIndexForInsertAndUpdate].Name;
                validateString(ref fieldValue, dataType);
				sql += (fieldName + "=" + fieldValue + ",");
                columnIndexForInsertAndUpdate++;
			}
		}
		public int endUpdateOnePK(string idVal)
		{
			if (sql.StartsWith("Update"))
			{
                DBDataType dataType = tableForInsertAndUpdate.getPKField().DataType;
                String IDFieldName = tableForInsertAndUpdate.getPKField().Name;
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
		public new int endUpdateMultiPK(string whereClause)
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
					string sql = "Insert Into " + tableName + " Values(";
					foreach (SatMsSQLServerTableField field in table.Fields)
						if (table.Fields.IndexOf(field) != 0)
						{
							string fieldValue = "";
                            if (field.Control is SatComboBox)
                            {
                                if (((SatComboBox)field.Control).SelectedIndex >= 0)
                                    fieldValue = ((SatComboBox)field.Control).getSelectedId();
                                else
                                    return;
                            }
                            else if (field.Control is TextBox || field.Control is ComboBox || field.Control is DateTimePicker)
                                fieldValue = field.Control.Text;
                            else if (field.Control is CheckBox)
                            {
                                if (((CheckBox)field.Control).Checked)
                                    fieldValue = "1";
                                else
                                    fieldValue = "0";
                            }							
							
							validateString(ref fieldValue, field.DataType);
							sql += (fieldValue + ",");
						}
					sql = sql.Substring(0, sql.Length - 1);
					sql += ")";
					executeNonQuery(sql);
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
                            else if (field.Control is CheckBox)
                            {
                                if (((CheckBox)field.Control).Checked)
                                    update("1");
                                else
                                    update("0");
                            }                            
						}
					return endUpdateOnePK(idValue, table.Fields[0].DataType, table.Fields[0].Name);
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
                startUpdate(table.Name);
                foreach (SatMsSQLServerTableField field in table.Fields)
                    if (table.Fields.IndexOf(field) != 0 && field.DataType != DBDataType.Picture)
                    {
                        if (field.Control is SatComboBox)
                            update(((SatComboBox)field.Control).getSelectedId());
                        else if (field.Control is TextBox || field.Control is ComboBox || field.Control is DateTimePicker)
                            update(field.Control.Text);
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
	}
}
