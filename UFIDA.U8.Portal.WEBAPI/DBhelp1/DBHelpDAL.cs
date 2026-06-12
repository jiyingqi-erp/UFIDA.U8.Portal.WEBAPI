using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UFIDA.U8.Portal.WEBAPI.DBhelp1
{
    public class DBHelpDAL
    {
        public static SqlConnection iConnection(string connectionString, string safeSql)
        {
            SqlConnection sqlConnection = null;
            try
            {
                if (sqlConnection == null)
                {
                    sqlConnection = new SqlConnection(connectionString);
                    sqlConnection.Open();
                }
                else if (sqlConnection.State == ConnectionState.Closed)
                {
                    sqlConnection.Open();
                }
                else if (sqlConnection.State == ConnectionState.Broken)
                {
                    sqlConnection.Close();
                    sqlConnection.Open();
                }
            }
            catch (Exception ex)
            {
                WriteTxt(ex.ToString());
            }
            return sqlConnection;
        }

        public static SqlConnection iConnection(string connectionString)
        {
            SqlConnection sqlConnection = null;
            try
            {
                if (sqlConnection == null)
                {
                    sqlConnection = new SqlConnection(connectionString);
                    sqlConnection.Open();
                }
                else if (sqlConnection.State == ConnectionState.Closed)
                {
                    sqlConnection.Open();
                }
                else if (sqlConnection.State == ConnectionState.Broken)
                {
                    sqlConnection.Close();
                    sqlConnection.Open();
                }
            }
            catch (Exception ex)
            {
                WriteTxt(ex.ToString());
            }
            return sqlConnection;
        }

        public static int ExecuteCommand(string ConnectionString, params SqlParameter[] values)
        {
            try
            {
                SqlConnection sqlConnection = iConnection(ConnectionString);
                SqlCommand sqlCommand = new SqlCommand();
                sqlCommand.Connection = sqlConnection;
                sqlCommand.CommandText = "Pro_UpdateBooksCatagory";
                sqlCommand.CommandType = CommandType.StoredProcedure;
                sqlCommand.Parameters.AddRange(values);
                int result = sqlCommand.ExecuteNonQuery();
                sqlConnection.Close();
                return result;
            }
            catch (Exception ex)
            {
                WriteTxt(ex.ToString());
                return 0;
            }
        }

        public static int ExecuteCommand(string ConnectionString, string safeSql)
        {
            try
            {
                SqlConnection sqlConnection = iConnection(ConnectionString, safeSql);
                SqlCommand sqlCommand = new SqlCommand(safeSql, sqlConnection);
                int result = sqlCommand.ExecuteNonQuery();
                sqlConnection.Close();
                return result;
            }
            catch (Exception ex)
            {
                WriteTxt(safeSql + "/n" + ex.ToString());
                return 0;
            }
        }

        public static int ExecuteSqlTran(string ConnectionString, List<string> sqllist)
        {
            try
            {
                SqlConnection sqlConnection = iConnection(ConnectionString);
                SqlCommand sqlCommand = new SqlCommand();
                SqlTransaction sqlTransaction = sqlConnection.BeginTransaction();
                sqlCommand.Connection = sqlConnection;
                sqlCommand.Transaction = sqlTransaction;
                try
                {
                    foreach (string item in sqllist)
                    {
                        sqlCommand.CommandText = item;
                        sqlCommand.ExecuteNonQuery();
                    }
                    sqlTransaction.Commit();
                    sqlConnection.Close();
                    return 1;
                }
                catch (Exception ex)
                {
                    WriteTxt(sqlCommand.CommandText + "/n" + ex.ToString());
                    sqlTransaction.Rollback();
                    sqlConnection.Close();
                    return 0;
                }
            }
            catch (Exception ex2)
            {
                WriteTxt(ex2.ToString());
                return 0;
            }
        }

        public static int ExecuteCommand(string ConnectionString, string sql, params SqlParameter[] values)
        {
            try
            {
                SqlConnection sqlConnection = iConnection(ConnectionString);
                SqlCommand sqlCommand = new SqlCommand(sql, sqlConnection);
                sqlCommand.Parameters.AddRange(values);
                int result = sqlCommand.ExecuteNonQuery();
                sqlConnection.Close();
                return result;
            }
            catch (Exception ex)
            {
                WriteTxt(sql + "/n" + ex.ToString());
                return 0;
            }
        }

        public static int GetScalar(string ConnectionString, string safeSql)
        {
            try
            {
                SqlConnection sqlConnection = iConnection(ConnectionString);
                SqlCommand sqlCommand = new SqlCommand(safeSql, sqlConnection);
                int result = Convert.ToInt32(sqlCommand.ExecuteScalar());
                sqlConnection.Close();
                return result;
            }
            catch (Exception ex)
            {
                WriteTxt(safeSql + "/n" + ex.ToString());
                return 0;
            }
        }

        public static int GetScalar(string ConnectionString, params SqlParameter[] values)
        {
            try
            {
                SqlConnection sqlConnection = iConnection(ConnectionString);
                SqlCommand sqlCommand = new SqlCommand();
                sqlCommand.Connection = sqlConnection;
                sqlCommand.CommandText = "Pro_InsertOrder";
                sqlCommand.CommandType = CommandType.StoredProcedure;
                sqlCommand.Parameters.AddRange(values);
                int result = Convert.ToInt32(sqlCommand.ExecuteScalar());
                sqlConnection.Close();
                return result;
            }
            catch (Exception ex)
            {
                WriteTxt(ex.ToString());
                return 0;
            }
        }

        public static int GetScalar(string ConnectionString, string sql, params SqlParameter[] values)
        {
            try
            {
                SqlConnection sqlConnection = iConnection(ConnectionString);
                SqlCommand sqlCommand = new SqlCommand(sql, sqlConnection);
                sqlCommand.Parameters.AddRange(values);
                int result = Convert.ToInt32(sqlCommand.ExecuteScalar());
                sqlConnection.Close();
                return result;
            }
            catch (Exception ex)
            {
                WriteTxt(sql + "/n" + ex.ToString());
                return 0;
            }
        }

        public static SqlDataReader GetReader(string ConnectionString, string safeSql)
        {
            try
            {
                SqlConnection sqlConnection = iConnection(ConnectionString);
                SqlCommand sqlCommand = new SqlCommand(safeSql, sqlConnection);
                SqlDataReader result = sqlCommand.ExecuteReader();
                sqlConnection.Close();
                return result;
            }
            catch (Exception ex)
            {
                WriteTxt(safeSql + "/n" + ex.ToString());
                return null;
            }
        }

        public static SqlDataReader GetReader(string ConnectionString, string sql, params SqlParameter[] values)
        {
            try
            {
                SqlConnection sqlConnection = iConnection(ConnectionString);
                SqlCommand sqlCommand = new SqlCommand(sql, sqlConnection);
                sqlCommand.Parameters.AddRange(values);
                SqlDataReader result = sqlCommand.ExecuteReader();
                sqlConnection.Close();
                return result;
            }
            catch (Exception ex)
            {
                WriteTxt(sql + "/n" + ex.ToString());
                return null;
            }
        }

        public static DataTable GetDataTable(string ConnectionString, string safeSql)
        {
            try
            {
                SqlConnection sqlConnection = iConnection(ConnectionString, safeSql);
                DataSet dataSet = new DataSet();
                SqlCommand selectCommand = new SqlCommand(safeSql, sqlConnection);
                SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(selectCommand);
                sqlDataAdapter.Fill(dataSet);
                sqlConnection.Close();
                return dataSet.Tables[0];
            }
            catch (Exception ex)
            {
                WriteTxt(safeSql + "/n" + ex.ToString());
                return null;
            }
        }

        public static DataTable GetDataTable(string ConnectionString, CommandType cmdType, string cmdText, params SqlParameter[] parameters)
        {
            try
            {
                DataTable dataTable = new DataTable();
                using (SqlConnection sqlConnection = new SqlConnection(ConnectionString))
                {
                    sqlConnection.Open();
                    using SqlCommand sqlCommand = sqlConnection.CreateCommand();
                    sqlCommand.CommandTimeout = 0;
                    sqlCommand.CommandType = cmdType;
                    sqlCommand.CommandText = cmdText;
                    foreach (SqlParameter value in parameters)
                    {
                        sqlCommand.Parameters.Add(value);
                    }
                    SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(sqlCommand);
                    sqlDataAdapter.Fill(dataTable);
                }
                return dataTable;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public static DataTable GetDataTable(string ConnectionString, string sql, params SqlParameter[] values)
        {
            try
            {
                SqlConnection sqlConnection = iConnection(ConnectionString);
                DataSet dataSet = new DataSet();
                SqlCommand sqlCommand = new SqlCommand(sql, sqlConnection);
                sqlCommand.Parameters.AddRange(values);
                SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(sqlCommand);
                sqlDataAdapter.Fill(dataSet);
                sqlConnection.Close();
                return dataSet.Tables[0];
            }
            catch (Exception ex)
            {
                WriteTxt(sql + "/n" + ex.ToString());
                return null;
            }
        }

        public static string GetString(string ConnectionString, string safeSql)
        {
            try
            {
                SqlConnection sqlConnection = iConnection(ConnectionString);
                DataSet dataSet = new DataSet();
                SqlCommand selectCommand = new SqlCommand(safeSql, sqlConnection);
                SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(selectCommand);
                sqlDataAdapter.Fill(dataSet);
                DataTable dataTable = dataSet.Tables[0];
                string empty = string.Empty;
                empty = ((dataTable == null || dataTable.Rows.Count == 0) ? "" : dataTable.Rows[0][0].ToString());
                sqlConnection.Close();
                return empty;
            }
            catch (Exception ex)
            {
                WriteTxt(safeSql + "/n" + ex.ToString());
                return null;
            }
        }

        public static DataTable GetPaging(string ConnectionString, string ConnString, string ProcName, string Tables, string PrimaryKey, string Sort, int CurrentPage, int PageSize, string Fields, string Filter, string Group, out int TotalPage, out int intResult)
        {
            try
            {
                SqlConnection sqlConnection = iConnection(ConnectionString);
                DataSet dataSet = new DataSet();
                SqlCommand sqlCommand = new SqlCommand(ProcName, sqlConnection);
                sqlCommand.CommandType = CommandType.StoredProcedure;
                sqlCommand.Parameters.Add("@Tables", SqlDbType.VarChar, 1000).Value = Tables;
                sqlCommand.Parameters.Add("@PrimaryKey", SqlDbType.VarChar, 100).Value = PrimaryKey;
                sqlCommand.Parameters.Add("@Sort", SqlDbType.VarChar, 200).Value = Sort;
                sqlCommand.Parameters.Add("@CurrentPage", SqlDbType.Int).Value = CurrentPage;
                sqlCommand.Parameters.Add("@PageSize", SqlDbType.Int).Value = PageSize;
                sqlCommand.Parameters.Add("@Fields", SqlDbType.VarChar, 1000).Value = Fields;
                sqlCommand.Parameters.Add("@Filter", SqlDbType.VarChar, 1000).Value = Filter;
                sqlCommand.Parameters.Add("@Group", SqlDbType.VarChar, 1000).Value = Group;
                SqlParameter sqlParameter = new SqlParameter("@TotalPage", SqlDbType.Int);
                sqlParameter.Direction = ParameterDirection.Output;
                sqlCommand.Parameters.Add(sqlParameter);
                SqlParameter sqlParameter2 = new SqlParameter("@intResult", SqlDbType.Int);
                sqlParameter2.Direction = ParameterDirection.Output;
                sqlCommand.Parameters.Add(sqlParameter2);
                new SqlDataAdapter(sqlCommand).Fill(dataSet);
                TotalPage = Convert.ToInt32(sqlParameter.Value);
                intResult = Convert.ToInt32(sqlParameter2.Value);
                sqlConnection.Close();
                return dataSet.Tables[0];
            }
            catch (Exception ex)
            {
                WriteTxt(ex.ToString());
                TotalPage = 0;
                intResult = 0;
                return null;
            }
        }

        public static void WriteTxt(string error)
        {
            DateTime dateTime = default(DateTime);
            dateTime = DateTime.Now;
            string text = "error" + dateTime.Year + dateTime.Month + dateTime.Day + ".txt";
            string path = "D:\\log\\" + text;
            if (!File.Exists(path))
            {
                using (StreamWriter streamWriter = File.CreateText(path))
                {
                    streamWriter.WriteLine("/******************************************************************/");
                    streamWriter.WriteLine("/* " + dateTime.ToString());
                    streamWriter.WriteLine(error);
                    streamWriter.WriteLine("/******************************************************************/");
                    return;
                }
            }
            StreamReader streamReader = File.OpenText(path);
            string value = streamReader.ReadToEnd();
            streamReader.Close();
            File.Delete(path);
            using StreamWriter streamWriter2 = new StreamWriter(path);
            streamWriter2.WriteLine(value);
            streamWriter2.WriteLine("/******************************************************************/");
            streamWriter2.WriteLine("/* " + dateTime.ToString());
            streamWriter2.WriteLine(error);
            streamWriter2.WriteLine("/******************************************************************/");
        }
    }
}
