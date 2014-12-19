using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace Satya
{
	public class SatComboBox : ComboBox
	{
		private SatMsSQLServerConnector connector;
		private string sql = "";
		private List<string> listId;
		public List<string> ListId {
			get { return listId; }
			set { listId = value; }
		}
		
		public SatComboBox() {
			listId = new List<string>();
		}
		
		public string getSelectedId() {
			if(this.Items.Count>0 && SelectedIndex>=0)
				return listId.ElementAt(SelectedIndex);
			return "";
		}
		public void setSelectedId(int index)
		{
			if (this.Items.Count > 0 && listId.IndexOf(index.ToString()) >= 0)
				this.SelectedIndex = listId.IndexOf(index.ToString());
		}
		
		public string selectedId() {
			return listId.ElementAt(SelectedIndex);
		}
		public string getIdAt(int index) {
			return listId.ElementAt(index);
		}
		public string idAt(int index) {
			return listId.ElementAt(index);
		}

		public void setData(SatMsSQLServerConnector connector, string sql)
		{
			connector.setRowSource(this, sql);
			this.sql = sql;
			this.connector = connector;
		}
		public void setDataByTableName(SatMsSQLServerConnector connector, string tableName)
		{ 
			string sql = " SELECT COLUMN_NAME" + 
				         " FROM INFORMATION_SCHEMA.COLUMNS" +
						 " WHERE TABLE_NAME = '" + tableName + "'";
			SqlDataReader reader = connector.executeReader(sql);
			sql = "Select ";
			for (int i = 0; i < 2; i++)
				if (reader.Read())
				{
					sql += reader[0].ToString();
					if (i == 0) sql += " , ";
				}
			sql += " From " + tableName;
			connector.setRowSource(this, sql);
			this.sql = sql;
			this.connector = connector;
		}
		public void refreshData()
		{
			connector.setRowSource(this, sql);
		}
	}
}
