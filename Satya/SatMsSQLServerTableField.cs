using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Satya
{
	public enum KeyType
	{
	    Normal,
		PK,
		FK
	}
	public class SatMsSQLServerTableField : Object
	{
		private string name;
		private Control control;
		private DBDataType dataType;
		private KeyType keyType = KeyType.Normal;
        private bool allowNULL;       
		private string referenceTableName; //For FK only

		public string ReferenceTableName
		{
			get { return referenceTableName; }
			set { referenceTableName = value; }
		}
		public KeyType KeyType
		{
			get { return keyType; }
            set { keyType = value; }
		}
        public bool AllowNULL
        {
            get { return allowNULL; }
        }
		public string Name
		{
			get { return name; }
		}
		public DBDataType DataType
		{
			get { return dataType; }
		}
		public Control Control
		{
			get { return control; }
            set { control = value; }
		}

		public SatMsSQLServerTableField(string name, DBDataType dataType, bool allowNULL=true, KeyType keyType = KeyType.Normal)
		{
			this.name = name;
			this.dataType = dataType;
			this.keyType = keyType;
            this.allowNULL = allowNULL;
		}
	}
}
