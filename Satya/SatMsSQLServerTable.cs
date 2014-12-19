using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Data;
using System.IO;
using System.Drawing;

namespace Satya
{
	public class SatMsSQLServerTable : Object
	{
		private SatMsSQLServerConnector2 connector;
		private string name;
        private List<SatMsSQLServerTableField> fields;
        private LinkLabel pictureBrowseLabel;
        private PictureBox pictureBox;
        private bool hasControl, hasButton;
		private List<Button> buttons;
		private SatListView listView;
		private SatTextBox searchTextBox;
		private bool isWeakEnitiy;

        public LinkLabel PictureBrowseLabel
        {
            get { return pictureBrowseLabel; }
            set { pictureBrowseLabel = value; }
        }
        public PictureBox PictureBox
        {
            get { return pictureBox; }
            set { pictureBox = value; }
        }
		public bool IsWeakEnitiy
		{
			get { return isWeakEnitiy; }
		}
		public SatTextBox SearchTextBox
		{
			get { return searchTextBox; }
			set { searchTextBox = value; }
		}
		public bool HasButton
		{
			get { return hasButton; }
			set { hasButton = value; }
		}
		public List<Button> Buttons
		{
			get { return buttons; }
			set { buttons = value; }
		}
		public bool HasControl
		{
			get { return hasControl; }
			set { hasControl = value; }
		}
		public List<SatMsSQLServerTableField> Fields
		{
			get { return fields; }
			set { fields = value; }
		}
		public string Name
		{
			get { return name; }
			set { name = value; }
		}
		public SatListView ListView
		{
			get { return listView; }
			set { listView = value; }
		}

		public SatMsSQLServerTable(string name, SatMsSQLServerConnector2 connector)
		{
			this.connector = connector;
			hasControl = hasButton = false;
			fields = new List<SatMsSQLServerTableField>();
			this.name = name;
			isWeakEnitiy = false;
			
			//Find all Fields in the table
			string sql = " SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE" + 
				         " FROM INFORMATION_SCHEMA.COLUMNS " + 
						 " WHERE TABLE_NAME = '" + name + "'";
			SqlDataReader reader1 = connector.executeReader(sql);
			while (reader1.Read())
			{
				string dataTypeString = reader1[1].ToString();
				DBDataType dataType;
                if (dataTypeString.Contains("char"))
                    dataType = DBDataType.String;
                else if (dataTypeString.Contains("date"))
                    dataType = DBDataType.Date;
                else if (dataTypeString.Contains("varbinary"))
                    dataType = DBDataType.Picture;
                else
                    dataType = DBDataType.Num;
                //Nullable
                bool allowNull = true;
                if (reader1[2].ToString() == "0" || reader1[2].ToString()=="NO")
                    allowNull = false;
                
				fields.Add(new SatMsSQLServerTableField(reader1[0].ToString(), 
                                                        dataType,allowNull));
			}
            //Find all PK in the table
            sql = " SELECT column_name " +
                  " FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE" +
                  " WHERE OBJECTPROPERTY(OBJECT_ID(constraint_name), 'IsPrimaryKey') = 1" +
                  " AND table_name = '" + name + "'";
            SqlDataReader reader3 = connector.executeReader(sql);
            while (reader3.Read()) 
                foreach (SatMsSQLServerTableField field in fields)
                    if (field.Name == reader3[0].ToString())
                        field.KeyType = KeyType.PK;

			//Find all FK in the table
			sql = " SELECT COL_NAME(fc.parent_object_id,fc.parent_column_id) AS FKField," +
				  " OBJECT_NAME (fk.referenced_object_id)" +
				  " FROM sys.foreign_keys AS fk INNER JOIN sys.foreign_key_columns AS fc " +
				  " ON fk.OBJECT_ID = fc.constraint_object_id" +
				  " WHERE OBJECT_NAME(fk.parent_object_id) = '" + name + "'";
			SqlDataReader reader2 = connector.executeReader(sql);
			if(reader2 != null)
				while (reader2.Read())
					foreach (SatMsSQLServerTableField field in fields)
						if (field.Name == reader2[0].ToString())
						{
							isWeakEnitiy = true;
							field.KeyType = KeyType.FK;
							field.ReferenceTableName = reader2[1].ToString(); 
						}
		}
		public void addControls(params Control[] controls)
		{
			if (controls.Length == fields.Count)
			{
				for (int i = 0; i < fields.Count; i++)
				{
					fields[i].Control = controls[i];
					if (fields[i].KeyType == KeyType.FK && controls[i] is SatComboBox) 
						((SatComboBox)fields[i].Control).setDataByTableName(connector, fields[i].ReferenceTableName); 
				}
				hasControl = true;
			}
			else
				MessageBox.Show("The number of controls for table '" + name + "' doesn't match.\n" +
								"Table fileds = " + fields.Count.ToString() + "  VS  Controls count = " + controls.Length.ToString(),
								"Database Error", MessageBoxButtons.OK,
								MessageBoxIcon.Error);	
		}
		public void addButtons(params Button[] buttons)
		{
			if (buttons.Length == 3)
			{
				this.buttons = new List<Button>();
				for (int i = 0; i < 3; i++)
					this.buttons.Add(buttons[i]);
				hasButton = true;
				buttons[0].Click += new EventHandler(buttonNew_Click);
				buttons[1].Click += new EventHandler(buttonSaveFor3ButtonsDesign_Click);
				buttons[2].Click += new EventHandler(buttonDelete_Click);
			}
			else if (buttons.Length == 4)
			{
				this.buttons = new List<Button>();
				for (int i = 0; i < 4; i++)
					this.buttons.Add(buttons[i]);
				hasButton = true;
				buttons[0].Click += new EventHandler(buttonNew_Click);
				buttons[1].Click += new EventHandler(buttonSave_Click);
				buttons[2].Click += new EventHandler(buttonUpdate_Click);
				buttons[3].Click += new EventHandler(buttonDelete_Click);

				buttons[1].Enabled = false;
			}
			else
				MessageBox.Show("The number of buttons for table '" + name + "' must be 4.\n" +
								"Buttons you input are = " + buttons.Length.ToString(),
								"Database Error", MessageBoxButtons.OK,
								MessageBoxIcon.Error);
		}
		public void addListView(SatListView listView, int searchColumnIndex=-1, ListViewSearchMode searchMode= ListViewSearchMode.Hide)
		{
			if (listView.Columns.Count == fields.Count)
			{
				this.listView = listView;
				if (isWeakEnitiy)
					setDataToListViewForWeakEnitity(listView);
				else
				{
                    DataTable dt = new DataTable();
                    SqlDataAdapter da;
                    da = new SqlDataAdapter(
                                "Select Top 1 * From " + name, 
                                connector.Connection);
                    da.Fill(dt);
                    //DataRow dr = dt.Rows[0];

                    string sql = "Select ";
                    for (int i = 0; i < dt.Columns.Count; i++)
                        if (dt.Columns[i].ColumnName.ToLower().Contains("picture"))
                            sql = sql + "'A',";
                        else
                            sql = sql + "[" + dt.Columns[i].ColumnName + "],";
                    sql = sql.Substring(0, sql.Length - 1) + " From "+ name;
                    listView.setData(connector, sql);
				}
				listView.Click += new EventHandler(listView_Click);
                listView.SearchColumnIndex = searchColumnIndex;
                listView.SearchMode = searchMode;
			}
			else
				MessageBox.Show("The number of columns in ListView(" + listView.Name + ") for table '" + name + "' doesn't match.\n" +
								"Table fileds = " + fields.Count.ToString() + "  VS  Columns in ListView = " + listView.Columns.Count.ToString(),
								"Database Error", MessageBoxButtons.OK,
								MessageBoxIcon.Error);	
		}
		public void addSearchTextBox(SatTextBox searchTextBox) {
			this.searchTextBox = searchTextBox;
            if (this.listView != null)
                this.listView.SearchTextBox = searchTextBox;
		}
        public void addpictureBox(PictureBox pictureBox)
        {
            this.pictureBox = pictureBox;
        }
        public void addPictureBrowseLabel(LinkLabel label)
        {
            this.pictureBrowseLabel = label;
            this.pictureBrowseLabel.Click += new EventHandler(pictureBrowseLabel_Click);
        }

		public void buttonNew_Click(object sender, EventArgs e) 
		{
			fields[0].Control.Text = connector.getAutoID(name, fields[0].Name);
			for (int i = 1; i < fields.Count; i++) {
				if(fields[i].Control is TextBox)
					fields[i].Control.Text = "";
			}
            buttons[0].Enabled = false;
            buttons[2].Enabled = false;
            if(pictureBox != null)pictureBox.Image = null;
            fields[1].Control.Focus();
		}
		public void buttonSave_Click(object sender, EventArgs e)
		{
            if(checkEmptyControls())
                return;
			connector.insertTable(name);
			listView.refreshData();
			buttons[0].Enabled = true;
		}
		public void buttonSaveFor3ButtonsDesign_Click(object sender, EventArgs e)
		{
            if (checkEmptyControls())
                return;
            if (buttons[0].Enabled == false)
			{
				connector.insertTable(name);
				buttons[0].Enabled = true;
			}
			else
				connector.updateTableOnePK(name);
			listView.refreshData();
		}
		public void buttonUpdate_Click(object sender, EventArgs e)
		{
			connector.updateTableOnePK(name);
			listView.refreshData();
		}
		public void buttonDelete_Click(object sender, EventArgs e)
		{
			connector.deleteTableByIdWithMsgBox(name);
            if (pictureBox != null) pictureBox.Image = null;
			listView.refreshData();
		}
		public void listView_Click(object sender, EventArgs e)
		{
			if (listView.SelectedItems.Count == 1 ) {
				for (int i = 0; i < fields.Count; i++)
				{
					if (fields[i].Control is TextBox || fields[i].Control is ComboBox || fields[i].Control is DateTimePicker)
						fields[i].Control.Text = listView.SelectedItems[0].SubItems[i].Text;
                    else if (fields[i].Control is PictureBox) {
                        string sql = "Select Picture From Product Where Id=" + fields[0].Control.Text;
                        SqlCommand cmd = new SqlCommand(sql,connector.Connection);
                        Byte[] content = cmd.ExecuteScalar() as Byte[];
                        MemoryStream ms = new MemoryStream(content);
                        try
                        {
                            ((PictureBox)fields[i].Control).Image = Image.FromStream(ms);
                        }
                        catch (ArgumentException)
                        {
                            MessageBox.Show("Cannot Display Picture", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    else if (fields[i].Control is CheckBox)
                    {
                        if (listView.SelectedItems[0].SubItems[i].Text == "1" || listView.SelectedItems[0].SubItems[i].Text.ToLower() == "true")
                            ((CheckBox)fields[i].Control).Checked = true;
                        else
                            ((CheckBox)fields[i].Control).Checked = false;
                    }
				}
                if (buttons != null)
                {
                    if (buttons.Count == 4)
                    {
                        buttons[1].Enabled = false;
                        buttons[2].Enabled = true;
                        buttons[3].Enabled = true;
                    }
                    buttons[0].Enabled = true;
                    if (buttons.Count == 3)
                        buttons[2].Enabled = true;
                }
			}
		}
        public void pictureBrowseLabel_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            openFileDialog1.Filter = "Image Files|*.jpg;*.jpeg;*.png;|All Files|*.*";
            openFileDialog1.FilterIndex = 1;
            openFileDialog1.RestoreDirectory = true;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
                this.pictureBox.Image = System.Drawing.Image.FromFile(openFileDialog1.FileName);
        }    
        
        public bool checkEmptyControls() {
            foreach(SatMsSQLServerTableField f in fields )
                if (f.Control.Text.Length < 1 && !f.AllowNULL)
                {
                    Label label = new Label();
                    label.Text = "Required";
                    label.AutoSize = false;
                    label.Size = new System.Drawing.Size(110, f.Control.Height);
                    label.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
                    label.BackColor = System.Drawing.Color.Red;
                    label.ForeColor = System.Drawing.Color.White;
                    label.Location = new System.Drawing.Point(f.Control.Location.X+ f.Control.Width +5,f.Control.Location.Y);
                    f.Control.Parent.Controls.Add(label);
                    return true;
                }
            return false;
        }

		/// <summary>
		/// Set data to ListView which its Table is Weak Entity Set
		/// </summary>
		/// <param name="listView"></param>
		/// <param name="tableName"></param>
		public void setDataToListViewForWeakEnitity(SatListView listView)
		{
			string sqlSelect = "Select ",
				   sqlFrom = " From " + name + ",",
				   sqlWhere = " Where ";
			foreach (SatMsSQLServerTableField f in this.Fields)
			{
                if(f.Name.ToLower() == "id")
                    sqlSelect += name + "." + f.Name + ",";
				else if (f.KeyType == KeyType.Normal)
					sqlSelect += name + "." + f.Name + ",";
				else if (f.KeyType == KeyType.FK)
				{
					SatMsSQLServerTable t = new SatMsSQLServerTable(f.ReferenceTableName, connector);
					sqlSelect += t.Name + "." + t.Fields[1].Name + ",";
					sqlFrom += t.Name + ",";
					sqlWhere += t.Name + "." + t.Fields[0].Name +
								"=" + this.Name + "." + f.Name + " AND ";
				}
			}
			sqlSelect = sqlSelect.Substring(0, sqlSelect.Length - 1);
			sqlFrom = sqlFrom.Substring(0, sqlFrom.Length - 1);
			if (sqlWhere == " Where ")
				sqlWhere = "";
			else
				sqlWhere = sqlWhere.Substring(0, sqlWhere.Length - 5);
			listView.setData(connector, sqlSelect + sqlFrom + sqlWhere);
		}
        /// <summary>
        /// Return the first PK field if it exists
        /// </summary>
        public SatMsSQLServerTableField getPKField() {
            foreach (SatMsSQLServerTableField f in fields)
                if (f.KeyType == KeyType.PK)
                    return f;
            return null;
        }
    }
}




/* Search for forieng key in a table
SELECT 
    f.name AS ForeignKey,
    OBJECT_NAME(f.parent_object_id) AS TableName,
    COL_NAME(fc.parent_object_id,
    fc.parent_column_id) AS ColumnName,
    OBJECT_NAME (f.referenced_object_id) AS ReferenceTableName,
    COL_NAME(fc.referenced_object_id,
    fc.referenced_column_id) AS ReferenceColumnName
FROM 
    sys.foreign_keys AS f
    INNER JOIN sys.foreign_key_columns AS fc ON f.OBJECT_ID = fc.constraint_object_id
*/
