using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;


namespace UFIDA.U8.Portal.WEBAPI.DBhelp1
{
    public class U8SqlDBHelper
    {
        public static string constr = ConfigurationManager.AppSettings["connectionstringName"].ToString();

        private static SqlConnection con;

        public static int AddReturnNewID(string safeSql)
        {
            try
            {
                return AddReturnNewID(safeSql, (SqlParameter[])null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static int AddReturnNewID(string safeSql, params SqlParameter[] paras)
        {
            try
            {
                string safeSql2 = safeSql + " select SCOPE_IDENTITY()";
                return Convert.ToInt32(ExecuteScalar(safeSql2, paras));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static int ExecuteSql(string safeSql)
        {
            try
            {
                return ExecuteSql(safeSql, (SqlParameter[])null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static int ExecuteSql(string safeSql, params SqlParameter[] paras)
        {
            using SqlConnection sqlConnection = new SqlConnection(constr);
            using SqlCommand sqlCommand = new SqlCommand();
            try
            {
                sqlConnection.Open();
                SetCmdParam(sqlConnection, sqlCommand, CommandType.Text, safeSql, paras);
                return sqlCommand.ExecuteNonQuery();
            }
            catch (SqlException ex)
            {
                LogException.WriteLog(ex, safeSql);
                sqlCommand.Dispose();
                sqlConnection.Close();
                throw ex;
            }
        }

        public static object ExecuteScalar(CommandType cmdType, string cmdText, params SqlParameter[] parameters)
        {
            try
            {
                object result;
                using (SqlConnection sqlConnection = new SqlConnection(constr))
                {
                    sqlConnection.Open();
                    using SqlCommand sqlCommand = sqlConnection.CreateCommand();
                    sqlCommand.CommandType = cmdType;
                    sqlCommand.CommandText = cmdText;
                    sqlCommand.Parameters.AddRange(parameters);
                    sqlCommand.CommandTimeout = 0;
                    result = sqlCommand.ExecuteScalar();
                }
                return result;
            }
            catch (Exception ex)
            {
                LogException.WriteLog(ex, cmdText);
                return null;
            }
        }

        public static DataSet ExecuteSqls(List<string> SQLStringList)
        {
            return ExecuteSqls(SQLStringList, null);
        }

        public static DataSet ExecuteSqls(List<string> SQLStringList, List<SqlParameter[]> LstParas)
        {
            using SqlConnection sqlConnection = new SqlConnection(constr);
            string text = "";
            sqlConnection.Open();
            SqlCommand sqlCommand = new SqlCommand();
            sqlCommand.Connection = sqlConnection;
            DataSet dataSet = new DataSet();
            try
            {
                for (int i = 0; i < SQLStringList.Count; i++)
                {
                    text = SQLStringList[i];
                    SetCmdParam(sqlConnection, sqlCommand, CommandType.Text, text, (SqlParameter[])null);
                    DataTable dataTable = new DataTable();
                    SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(sqlCommand);
                    sqlDataAdapter.Fill(dataTable);
                    dataSet.Tables.Add(dataTable);
                    sqlCommand.Parameters.Clear();
                }
                return dataSet;
            }
            catch (Exception ex)
            {
                LogException.WriteLog(ex, text);
                return null;
            }
        }

        public static int ExecuteSqlTran(List<string> SQLStringList)
        {
            return ExecuteSqlTran(SQLStringList, null);
        }

        public static int ExecuteSqlTran(List<string> SQLStringList, List<SqlParameter[]> LstParas)
        {
            using SqlConnection sqlConnection = new SqlConnection(constr);
            sqlConnection.Open();
            SqlCommand sqlCommand = new SqlCommand();
            sqlCommand.Connection = sqlConnection;
            SqlTransaction sqlTransaction = (sqlCommand.Transaction = sqlConnection.BeginTransaction());
            string text = "";
            try
            {
                int num = 0;
                for (int i = 0; i < SQLStringList.Count; i++)
                {
                    text = SQLStringList[i];
                    if (text.Trim().Length > 1)
                    {
                        if (LstParas == null)
                        {
                            SetCmdParam(sqlConnection, sqlCommand, text);
                        }
                        else
                        {
                            SetCmdParam(sqlConnection, sqlCommand, text, LstParas[i]);
                        }
                        num += sqlCommand.ExecuteNonQuery();
                        sqlCommand.Parameters.Clear();
                    }
                }
                sqlTransaction.Commit();
                return num;
            }
            catch (Exception ex)
            {
                LogException.WriteLog(ex, text);
                sqlTransaction.Rollback();
                return 0;
            }
        }

        public static int ExecuteSqlTran(List<string> SQLStringList, ref string returnValue)
        {
            returnValue = string.Empty;
            using SqlConnection sqlConnection = new SqlConnection(constr);
            sqlConnection.Open();
            SqlCommand sqlCommand = new SqlCommand();
            sqlCommand.Connection = sqlConnection;
            SqlTransaction sqlTransaction = (sqlCommand.Transaction = sqlConnection.BeginTransaction());
            string text = "";
            try
            {
                int num = 0;
                for (int i = 0; i < SQLStringList.Count; i++)
                {
                    text = SQLStringList[i];
                    if (text.Trim().Length > 1)
                    {
                        SetCmdParam(sqlConnection, sqlCommand, text);
                        num += sqlCommand.ExecuteNonQuery();
                        sqlCommand.Parameters.Clear();
                    }
                }
                sqlTransaction.Commit();
                return num;
            }
            catch (Exception ex)
            {
                LogException.WriteLog(ex, text);
                sqlTransaction.Rollback();
                return 0;
            }
        }

        public static object ExecuteScalar(string safeSql)
        {
            return ExecuteScalar(safeSql, (SqlParameter[])null);
        }

        public static object ExecuteScalar(string safeSql, params SqlParameter[] paras)
        {
            using SqlConnection sqlConnection = new SqlConnection(constr);
            using SqlCommand sqlCommand = new SqlCommand();
            try
            {
                sqlConnection.Open();
                SetCmdParam(sqlConnection, sqlCommand, CommandType.Text, safeSql, paras);
                object obj = sqlCommand.ExecuteScalar();
                if (object.Equals(obj, null) || object.Equals(obj, DBNull.Value))
                {
                    return null;
                }
                return obj;
            }
            catch (SqlException ex)
            {
                sqlCommand.Dispose();
                sqlConnection.Close();
                throw ex;
            }
        }

        public static int ExecuteProcedure(string storedProcName, out int rowsAffected)
        {
            return ExecuteProcedure(storedProcName, null, out rowsAffected);
        }

        public static int ExecuteProcedure(string storedProcName, SqlParameter[] paras, out int rowsAffected)
        {
            using SqlConnection sqlConnection = new SqlConnection(constr);
            try
            {
                sqlConnection.Open();
                SqlCommand sqlCommand = BuildIntCommand(sqlConnection, storedProcName, paras);
                rowsAffected = sqlCommand.ExecuteNonQuery();
                return (int)sqlCommand.Parameters["ReturnValue"].Value;
            }
            catch (SqlException ex)
            {
                LogException.WriteLog(ex, storedProcName);
                sqlConnection.Close();
                rowsAffected = 0;
                return 0;
            }
        }

        public static DataTable ExecuteProcedure(string storedProcName)
        {
            return ExecuteProcedure(storedProcName, (SqlParameter[])null);
        }

        public static DataTable ExecuteProcedure(string storedProcName, params SqlParameter[] paras)
        {
            using SqlConnection sqlConnection = new SqlConnection(constr);
            try
            {
                DataTable dataTable = new DataTable();
                sqlConnection.Open();
                SqlDataAdapter sqlDataAdapter = new SqlDataAdapter();
                sqlDataAdapter.SelectCommand = BuildQueryCommand(sqlConnection, storedProcName, paras);
                sqlDataAdapter.SelectCommand.CommandTimeout = 0;
                sqlDataAdapter.Fill(dataTable);
                sqlConnection.Close();
                return dataTable;
            }
            catch (SqlException ex)
            {
                LogException.WriteLog(ex, storedProcName);
                sqlConnection.Close();
                return null;
            }
        }

        public static DataTable GetDataTable(string safeSql)
        {
            try
            {
                return GetDataTable(safeSql, (SqlParameter[])null);
            }
            catch (SqlException ex)
            {
                LogException.WriteLog(ex, safeSql);
                con.Close();
                return null;
            }
        }

        public static string GetString(string safeSql)
        {
            try
            {
                DataTable dataTable = GetDataTable(safeSql, (SqlParameter[])null);
                if (dataTable.Rows.Count == 0)
                {
                    return "";
                }
                return dataTable.Rows[0][0].ToString();
            }
            catch (SqlException ex)
            {
                LogException.WriteLog(ex, safeSql);
                con.Close();
                return "";
            }
        }

        public static DataTable GetDataTable(string safeSql, params SqlParameter[] paras)
        {
            try
            {
                return GetDataTable(safeSql, CommandType.Text, paras);
            }
            catch (SqlException ex)
            {
                LogException.WriteLog(ex, safeSql);
                con.Close();
                return null;
            }
        }

        public static int GetExecute(string sqlStr)
        {
            InitConnection();
            SqlCommand sqlCommand = new SqlCommand(sqlStr, con);
            int result = sqlCommand.ExecuteNonQuery();
            con.Close();
            return result;
        }

        public static SqlDataReader GetDataReader(string sqlStr)
        {
            InitConnection();
            SqlCommand sqlCommand = new SqlCommand(sqlStr, con);
            return sqlCommand.ExecuteReader(CommandBehavior.CloseConnection);
        }

        public static DataTable GetDataTable(string cmdText, CommandType cmdType, params SqlParameter[] paras)
        {
            using SqlConnection sqlConnection = new SqlConnection(constr);
            using SqlCommand sqlCommand = new SqlCommand();
            try
            {
                sqlConnection.Open();
                SetCmdParam(sqlConnection, sqlCommand, cmdType, cmdText, paras);
                DataTable dataTable = new DataTable();
                SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(sqlCommand);
                sqlDataAdapter.Fill(dataTable);
                return dataTable;
            }
            catch (SqlException ex)
            {
                LogException.WriteLog(ex, cmdText);
                sqlCommand.Dispose();
                sqlConnection.Close();
                return null;
            }
        }

        public static DataSet GetDataSet(string cmdText, CommandType cmdType, params SqlParameter[] paras)
        {
            using SqlConnection sqlConnection = new SqlConnection(constr);
            using SqlCommand sqlCommand = new SqlCommand();
            try
            {
                sqlConnection.Open();
                SetCmdParam(sqlConnection, sqlCommand, cmdType, cmdText, paras);
                DataSet dataSet = new DataSet();
                SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(sqlCommand);
                sqlDataAdapter.Fill(dataSet);
                return dataSet;
            }
            catch (SqlException ex)
            {
                LogException.WriteLog(ex, cmdText);
                sqlCommand.Dispose();
                sqlConnection.Close();
                return null;
            }
        }

        private static SqlConnection InitConnection()
        {
            if (con == null)
            {
                con = new SqlConnection(constr);
                con.Open();
            }
            else if (con.State == ConnectionState.Closed)
            {
                con.Open();
            }
            else if (con.State == ConnectionState.Broken)
            {
                con.Close();
                con.Open();
            }
            return con;
        }

        private static void SetCmdParam(SqlConnection con, SqlCommand cmd, string cmdText)
        {
            SetCmdParam(con, cmd, CommandType.Text, cmdText, (SqlParameter[])null);
        }

        private static void SetCmdParam(SqlConnection con, SqlCommand cmd, string cmdText, SqlParameter[] paras)
        {
            SetCmdParam(con, cmd, CommandType.Text, cmdText, paras);
        }

        private static void SetCmdParam(SqlConnection con, SqlCommand cmd, CommandType cmdType, string cmdText, params SqlParameter[] paras)
        {
            if (con.State == ConnectionState.Closed)
            {
                con.Open();
            }
            cmd.Connection = con;
            cmd.CommandText = cmdText;
            cmd.CommandType = cmdType;
            if (paras == null)
            {
                return;
            }
            foreach (SqlParameter sqlParameter in paras)
            {
                if ((sqlParameter.Direction == ParameterDirection.InputOutput || sqlParameter.Direction == ParameterDirection.Input) && sqlParameter.Value == DBNull.Value)
                {
                    sqlParameter.Value = DBNull.Value;
                }
                cmd.Parameters.Add(sqlParameter);
            }
        }

        public static DataTable RunProcedure(string storedProcName, IDataParameter[] parameters, ref Dictionary<string, string> dic)
        {
            DataTable dataTable = new DataTable();
            using SqlConnection sqlConnection = new SqlConnection(constr);
            SqlCommand sqlCommand = BuildQueryCommand(sqlConnection, storedProcName, parameters);
            SqlDataAdapter sqlDataAdapter = new SqlDataAdapter
            {
                SelectCommand = sqlCommand
            };
            sqlDataAdapter.SelectCommand.CommandTimeout = 0;
            sqlDataAdapter.Fill(dataTable);
            sqlConnection.Close();
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            foreach (KeyValuePair<string, string> item in dic)
            {
                try
                {
                    dictionary.Add(item.Key, sqlCommand.Parameters[item.Key].Value.ToString());
                }
                catch (Exception)
                {
                }
            }
            dic = dictionary;
            return dataTable;
        }

        private static SqlCommand BuildIntCommand(SqlConnection connection, string storedProcName, IDataParameter[] parameters)
        {
            SqlCommand sqlCommand = BuildQueryCommand(connection, storedProcName, parameters);
            sqlCommand.Parameters.Add(new SqlParameter("ReturnValue", SqlDbType.Int, 4, ParameterDirection.ReturnValue, isNullable: false, 0, 0, string.Empty, DataRowVersion.Default, null));
            return sqlCommand;
        }

        private static SqlCommand BuildQueryCommand(SqlConnection con, string storedProcName, IDataParameter[] paras)
        {
            if (con.State == ConnectionState.Closed)
            {
                con.Open();
            }
            SqlCommand sqlCommand = new SqlCommand(storedProcName, con);
            sqlCommand.CommandType = CommandType.StoredProcedure;
            if (paras == null || paras.Length == 0)
            {
                return sqlCommand;
            }
            for (int i = 0; i < paras.Length; i++)
            {
                SqlParameter sqlParameter = (SqlParameter)paras[i];
                if (sqlParameter != null)
                {
                    if ((sqlParameter.Direction == ParameterDirection.InputOutput || sqlParameter.Direction == ParameterDirection.Input) && sqlParameter.Value == null)
                    {
                        sqlParameter.Value = DBNull.Value;
                    }
                    sqlCommand.Parameters.Add(sqlParameter);
                }
            }
            return sqlCommand;
        }

        public static int GetDatasetBy(string sqlString, string connectionString, out DataTable dts, out string errMsg)
        {
            errMsg = "";
            dts = new DataTable();
            SqlCommand sqlCommand = new SqlCommand();
            SqlConnection sqlConnection = new SqlConnection();
            SqlDataAdapter sqlDataAdapter = new SqlDataAdapter();
            sqlConnection.ConnectionString = connectionString;
            try
            {
                sqlConnection.Open();
            }
            catch (Exception ex)
            {
                errMsg = ex.Message;
                return -1;
            }
            sqlCommand.Connection = sqlConnection;
            sqlCommand.CommandText = sqlString;
            sqlDataAdapter.SelectCommand = sqlCommand;
            try
            {
                sqlDataAdapter.Fill(dts);
                return 0;
            }
            catch (Exception ex2)
            {
                errMsg = ex2.Message;
                return -2;
            }
            finally
            {
                if (sqlConnection.State != ConnectionState.Closed)
                {
                    sqlConnection.Close();
                    sqlConnection = null;
                }
            }
        }

        public static int GetDataset(string sqlString, string connectionString, out DataSet result, out string errMsg)
        {
            errMsg = "";
            result = new DataSet();
            SqlCommand sqlCommand = new SqlCommand();
            SqlConnection sqlConnection = new SqlConnection();
            SqlDataAdapter sqlDataAdapter = new SqlDataAdapter();
            sqlConnection.ConnectionString = connectionString;
            try
            {
                sqlConnection.Open();
            }
            catch (Exception ex)
            {
                errMsg = ex.Message;
                return -1;
            }
            sqlCommand.Connection = sqlConnection;
            sqlCommand.CommandText = sqlString;
            sqlDataAdapter.SelectCommand = sqlCommand;
            try
            {
                sqlDataAdapter.Fill(result);
                return 0;
            }
            catch (Exception ex2)
            {
                errMsg = ex2.Message;
                return -2;
            }
            finally
            {
                if (sqlConnection.State != ConnectionState.Closed)
                {
                    sqlConnection.Close();
                    sqlConnection = null;
                }
            }
        }

        public static int ExecuteNonQuery(string cmdText)
        {
            try
            {
                int result = 0;
                using (SqlConnection sqlConnection = new SqlConnection(constr))
                {
                    sqlConnection.Open();
                    using SqlCommand sqlCommand = sqlConnection.CreateCommand();
                    sqlCommand.CommandText = cmdText;
                    result = sqlCommand.ExecuteNonQuery();
                }
                return result;
            }
            catch (Exception ex)
            {
                LogException.WriteLog(ex, cmdText);
                con.Close();
                return 0;
            }
        }

        public static int ExecuteNonQuery(string strsql, SqlParameter[] para)
        {
            SqlConnection sqlConnection = new SqlConnection(constr);
            SqlCommand sqlCommand = new SqlCommand(strsql, sqlConnection);
            sqlConnection.Open();
            if (para != null)
            {
                sqlCommand.Parameters.AddRange(para);
            }
            int result = 0;
            try
            {
                result = sqlCommand.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                LogException.WriteLog(ex, strsql);
            }
            finally
            {
                sqlConnection.Close();
            }
            return result;
        }

        public static int SpExecuteNonQuery(string cmdText)
        {
            try
            {
                int result = 0;
                using (SqlConnection sqlConnection = new SqlConnection(constr))
                {
                    sqlConnection.Open();
                    using SqlCommand sqlCommand = sqlConnection.CreateCommand();
                    sqlCommand.CommandText = cmdText;
                    sqlCommand.CommandType = CommandType.StoredProcedure;
                    result = sqlCommand.ExecuteNonQuery();
                }
                return result;
            }
            catch (Exception ex)
            {
                LogException.WriteLog(ex, cmdText);
                con.Close();
                return 0;
            }
        }

        public static bool Exists(string cmdText, params SqlParameter[] paras)
        {
            try
            {
                DataTable dataTable = GetDataTable(cmdText, paras);
                if (Convert.ToInt32(dataTable.Rows[0][0].ToString()) == 0)
                {
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                LogException.WriteLog(ex, cmdText);
                con.Close();
                return false;
            }
        }

        public static void SqlBulkCopyByDatatable(string TableName, DataTable dt)
        {
            using (new SqlConnection(constr))
            {
                using SqlBulkCopy sqlBulkCopy = new SqlBulkCopy(constr, SqlBulkCopyOptions.UseInternalTransaction);
                try
                {
                    sqlBulkCopy.DestinationTableName = TableName;
                    for (int i = 0; i < dt.Columns.Count; i++)
                    {
                        sqlBulkCopy.ColumnMappings.Add(dt.Columns[i].ColumnName, dt.Columns[i].ColumnName);
                    }
                    sqlBulkCopy.WriteToServer(dt);
                }
                catch (Exception ex)
                {
                    LogException.WriteLog(ex, TableName);
                }
            }
        }

        public static string GetScalar(string safeSql)
        {
            using SqlConnection sqlConnection = new SqlConnection(constr);
            sqlConnection.Open();
            SqlCommand sqlCommand = new SqlCommand(safeSql, sqlConnection);
            return Convert.ToString(sqlCommand.ExecuteScalar());
        }

        public static string GetScalar(string sql, params SqlParameter[] values)
        {
            using SqlConnection sqlConnection = new SqlConnection(constr);
            sqlConnection.Open();
            SqlCommand sqlCommand = new SqlCommand(sql, sqlConnection);
            sqlCommand.Parameters.AddRange(values);
            return Convert.ToString(sqlCommand.ExecuteScalar());
        }
    }
}
