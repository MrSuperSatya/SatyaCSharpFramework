using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.OleDb;
using System.Data.Common;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Data.SqlClient;

namespace Satya
{
	public interface SatIDBConnector
	{
		bool isOpen();
		bool openConnection(string connectionString);
		void closeConnection();

		int executeNonQuery(string sql);
		object executeScalar(string sql);
		
		string getAutoID(string tableName, string IDFieldName = "id");
		string getIdOf(string tableName, string fieldName, string fieldValue, DBDataType dataType);
				
		void insert(string fieldValue, DBDataType dataType = DBDataType.Num);
		int endInsert();

		void startUpdate(string tableName);
		void update(string fieldName, string fieldValue, DBDataType dataType = DBDataType.Num);
		int endUpdateOnePK(string idVal, DBDataType dataType= DBDataType.Num, string IDFieldName="Id");
		int endUpdateMultiPK(string whereClause);

		void deleteByID(string tableName, string IDFieldValue, string IDFieldName="Id");
		void deleteByWhereClause(string tableName, string whereClause);

		void validateString(ref string s, DBDataType t);

		void setRowSource(Dictionary<string, string> dictionary, string sql);
		void setRowSource(ListView listView, string sql, int columnCount);		
		void setRowSource(ListView listView, string sql);
		void setRowSource(ListBox listBox, string sql);
		void setRowSource(ComboBox comboBox, string sql);
		void setRowSource(ComboBox comboBox, string fieldName, string tableName);
		void setRowSource(SatComboBox comboBox, string sql);
		void setRowSource(SatComboBox comboBox, string fieldIdName, string filedDisplayName, string tableName);
		void setRowSource(List<string> list, string sql);

		void showDbErrorMsg(string sql, string errorMessage);
		void showDataErrorMsg(string dataValue, string dataType);
	}
}
