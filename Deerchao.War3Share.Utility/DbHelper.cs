using System;
using System.Data;
using System.Data.Common;

namespace Deerchao.War3Share.Utility
{
    public static class DbHelper
    {
        #region Run command, and retrieve data
        //ado.net 底层提供了 connection pool, 因此频繁地打开与关闭数据库链接对性能的影响有限
        public static int GetInt(DbCommand cmd)
        {
            return Convert.ToInt32(GetObject(cmd));
        }

        public static bool GetBool(DbCommand cmd)
        {
            return (bool)GetObject(cmd);
        }

        public static Guid GetGuid(DbCommand cmd)
        {
            return (Guid)GetObject(cmd);
        }

        public static object GetObject(DbCommand cmd)
        {
            object result;
            try
            {
                cmd.Connection.Open();
                result = cmd.ExecuteScalar();
            }
            finally { cmd.Connection.Close(); }
            return result;
        }

        public static DateTime GetDateTime(DbCommand cmd)
        {
            return (DateTime)GetObject(cmd);
        }

        public static string GetString(DbCommand cmd)
        {
            return (string)GetObject(cmd);
        }

        public static DataTable GetDataTable(DbCommand cmd)
        {
            DataSet ds=new DataSet();
            DataTable tb = new DataTable();
            ds.Tables.Add(tb);
            ds.EnforceConstraints = false;
            try
            {
                cmd.Connection.Open();
                tb.Load(cmd.ExecuteReader());
            }
            finally { cmd.Connection.Close(); }
            return tb;
        }

        public static DataRow GetDataRow(DbCommand cmd)
        {
            DataTable tb = GetDataTable(cmd);
            if (tb.Rows.Count > 0)
                return tb.Rows[0];
            return null;
        }
        #endregion

        public static void Run(DbCommand cmd)
        {
            try
            {
                cmd.Connection.Open();
                cmd.ExecuteNonQuery();
            }
            finally { cmd.Connection.Close(); }
        }

        #region Get Typed value from object
        public static string GetDbString(object value)
        {
            if (value is string)
            {
                return (string)value;
            }
            return null;
        }

        public static int GetDbInt(object value)
        {
            if (value is Int32)
                return (int)value;
            return 0;
        }

        public static bool GetDbBool(object value)
        {
            if (value is bool)
                return (bool)value;
            return false;
        }

        public static DateTime GetDbDateTime(object value)
        {
            if (value is DateTime)
                return (DateTime)value;
            return DateTime.MinValue;
        }

        public static Guid GetDbGuid(object value)
        {
            if (value is Guid)
                return (Guid)value;
            return Guid.Empty;
        }

        public static decimal GetDbDecimal(object value)
        {
            if (value is decimal)
                return (decimal)value;
            return 0;
        }

        public static double GetDbFloat(object value)
        {
            if (value is double)
                return (double)value;
            return 0;
        }
        #endregion


        #region Actions on rows in DataTable
        public static DataRow FindRow(DataTable tb, string colName, object value)
        {
            foreach (DataRow row in tb.Rows)
            {
                if (Equals(row[colName], value))
                    return row;
            }
            return null;
        }
        #endregion

        public static bool HasColumn(DataTable tb, string colName)
        {
            return tb.Columns.Contains(colName);
        }
    }
}