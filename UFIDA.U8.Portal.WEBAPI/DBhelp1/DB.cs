using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UFIDA.U8.Portal.WEBAPI.DBhelp1
{
    public class DB
    {
        private static string ConnectionString = ConfigurationManager.AppSettings["connectionstringName"].ToString();

        private static string path1 = ConfigurationManager.AppSettings["path"].ToString();
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
                WriteTxt(ex.ToString(), "error");
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
                WriteTxt(ex.ToString(), "error");
            }
            return sqlConnection;
        }

        public static DataTable GetDataTable(string safeSql)
        {
            try
            {
                SqlConnection sqlConnection = iConnection(ConnectionString, safeSql);
                DataTable dataTable = new DataTable();
                SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(safeSql, ConnectionString);
                sqlDataAdapter.Fill(dataTable);
                sqlConnection.Close();
                return dataTable;
            }
            catch (Exception ex)
            {
                WriteTxt((safeSql + "\n" + ex.ToString() + "\n" + ConnectionString) ?? "", "error");
                return null;
            }
        }

        public static DataTable GetDataSet(string safeSql)
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
                WriteTxt(safeSql + "/n" + ex.ToString(), "error");
                return null;
            }
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
                WriteTxt(ex.ToString(), "error");
                return 0;
            }
        }

        public static int ExecuteSqlTran(string safeSql)
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
                WriteTxt(safeSql + "/n" + ex.ToString(), "error");
                return 0;
            }
        }

        public static int ExecuteSqlTranS(List<string> sqllist)
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
                    WriteTxt(sqlCommand.CommandText + "\n" + ex.ToString(), "error");
                    sqlTransaction.Rollback();
                    sqlConnection.Close();
                    return 0;
                }
            }
            catch (Exception ex2)
            {
                WriteTxt(ex2.ToString(), "error");
                return 0;
            }
        }

        public static void WriteTxt(string error)
        {
            DateTime dateTime = default(DateTime);
            dateTime = DateTime.Now;
            string text = "error" + dateTime.Year + dateTime.Month + dateTime.Day + ".txt";
            string path = path1 + text;
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

        public static void WriteTxt2json(string error)
        {
            DateTime dateTime = default(DateTime);
            dateTime = DateTime.Now;
            string text = "json字符串" + dateTime.Year + dateTime.Month + dateTime.Day + ".txt";
            string path = path1 + text;
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

        public static void WriteTxt(string error, string name)
        {
            DateTime dateTime = default(DateTime);
            dateTime = DateTime.Now;
            string text = dateTime.Year.ToString() + dateTime.Month + dateTime.Day;
            string path = AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "\\log\\" + text;
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            path = AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "\\log\\" + text + "\\" + text + name + ".txt";
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

        public static void WriteTxt3(string result, string msgtype)
        {
            DateTime dateTime = default(DateTime);
            string text = msgtype + DateTime.Now.ToString("yyyy-MM-dd") + ".txt";
            string path = path1 + text;
            if (!File.Exists(path))
            {
                DateTime dateTime2 = DateTime.Now.AddMonths(-1);
                string path2 = AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "\\log\\" + msgtype + dateTime2.ToString("yyyy-MM-dd") + ".txt";
                if (File.Exists(path))
                {
                    File.Delete(path2);
                }
                using StreamWriter streamWriter = File.CreateText(path);
                streamWriter.WriteLine("/******************************************************************/");
                streamWriter.WriteLine("当前时间：" + DateTime.Now.ToString());
                streamWriter.WriteLine("信息：" + result);
                streamWriter.WriteLine("/******************************************************************/");
                return;
            }
            StreamReader streamReader = File.OpenText(path);
            string value = streamReader.ReadToEnd();
            streamReader.Close();
            File.Delete(path);
            using StreamWriter streamWriter2 = new StreamWriter(path);
            streamWriter2.WriteLine(value);
            streamWriter2.WriteLine("/******************************************************************/");
            streamWriter2.WriteLine("当前时间：" + DateTime.Now.ToString());
            streamWriter2.WriteLine("信息：" + result);
            streamWriter2.WriteLine("/******************************************************************/");
        }

        public static void CreateDirectoryOrFile()
        {
            DateTime dateTime = default(DateTime);
            dateTime = DateTime.Now;
            string text = dateTime.Year.ToString() + dateTime.Month + dateTime.Day;
            string path = path1 + text;
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        public static DateTime StampToDateTime(string time)
        {
            time = time.Substring(0, 10);
            double value = Convert.ToInt64(time);
            return new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(value).ToLocalTime();
        }

        public static long DateTimeToStamp(DateTime time)
        {
            DateTime dateTime = new DateTime(1970, 1, 1, 8, 0, 0);
            return (long)(time - dateTime).TotalMilliseconds;
        }
    }
}
