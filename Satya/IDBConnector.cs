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
	public interface IDBConnector
	{
		bool isOpen();
		bool openConnection(string connectionString);
		void closeConnection();

		int executeNonQuery(string sql);
		string executeScalar(string sql);
		//SqlDataReader executeReader(string sql);

		string getAutoID(string tableName, string IDFieldName = "id");
		string getIdOf(string tableName, string fieldName, string fieldValue, DBDataType dataType);

		//SqlDataReader selectAll(string tableName);
		//public SqlDataReader select(params string[] fields);

		void startInsertOnePK(string tableName, string PKFieldName);
		void startInsertMultiPK(string tableName);
		void setInsert(string fieldValue, DBDataType dataType = DBDataType.Num);
		void endInsert();

		void startUpdate(string tableName);
		void setUpdate(string fieldName, string fieldValue, DBDataType dataType = DBDataType.Num);
		int endUpdateOnePK(string idVal, DBDataType dataType= DBDataType.Num, string IDFieldName="Id");
		int endUpdateMultiPK(string whereClause);

		void deleteByID(string tableName, string IDFieldValue, string IDFieldName="Id");
		void deleteByWhereClause(string tableName, string whereClause);

		void validateString(ref string s, DBDataType t);

		void setRowSource(ref Dictionary<string, string> dic, string sql);
		void setRowSource(ref ListView lv, string sql, int columnCount);
		void setRowSource(ref ComboBox cb, string sql);
		void setRowSource(ref ComboBox cb, string tableName, string fieldName);

		void showDbErrorMsg(string sql, string errorMessage);
		void showDataErrorMsg(string dataValue, string dataType);
	}
}
