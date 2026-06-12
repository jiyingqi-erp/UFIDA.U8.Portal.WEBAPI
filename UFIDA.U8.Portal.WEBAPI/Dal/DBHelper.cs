using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace UFIDA.U8.Portal.WEBAPI.Dal
{

public class DBHelper
{
	public static string constr = ConfigurationManager.AppSettings["connectionstringName"].ToString();

	private SqlConnection conn1;

	public SqlConnection conn
	{
		get
		{
			if (conn1 == null)
			{
				conn1 = new SqlConnection(constr);
			}
			return conn1;
		}
	}

	public object SelectOne(string sql)
	{
		conn.Open();
		SqlCommand sqlCommand = new SqlCommand(sql, conn);
		object result = sqlCommand.ExecuteScalar();
		conn.Close();
		return result;
	}

	public object SelectOne(string sql, SqlParameter[] sqlparams)
	{
		conn.Open();
		SqlCommand sqlCommand = new SqlCommand(sql, conn);
		sqlCommand.Parameters.AddRange(sqlparams);
		object result = sqlCommand.ExecuteScalar();
		conn.Close();
		return result;
	}

	public SqlDataReader SelectList(string sql)
	{
		conn.Open();
		SqlCommand sqlCommand = new SqlCommand(sql, conn);
		return sqlCommand.ExecuteReader(CommandBehavior.CloseConnection);
	}

	public SqlDataReader SelectList(string sql, SqlParameter[] paramter)
	{
		conn.Open();
		SqlCommand sqlCommand = new SqlCommand(sql, conn);
		sqlCommand.Parameters.AddRange(paramter);
		return sqlCommand.ExecuteReader(CommandBehavior.CloseConnection);
	}

	public int SelectNonQuery(string sql)
	{
		conn.Open();
		SqlCommand sqlCommand = new SqlCommand(sql, conn);
		int result = sqlCommand.ExecuteNonQuery();
		conn.Close();
		return result;
	}

	public int SelectNonQuery(string sql, SqlParameter[] parameters)
	{
		conn.Open();
		SqlCommand sqlCommand = new SqlCommand(sql, conn);
		sqlCommand.Parameters.AddRange(parameters);
		int result = sqlCommand.ExecuteNonQuery();
		conn.Close();
		return result;
	}

	public DataTable SelectDataTable(string sql)
	{
		SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(sql, conn);
		DataSet dataSet = new DataSet();
		sqlDataAdapter.Fill(dataSet);
		DataTable dataTable = new DataTable();
		return dataSet.Tables[0];
	}

	public bool ExcuteTransaction(string[] sqls)
	{
		bool result = false;
		conn.Open();
		SqlTransaction sqlTransaction = conn.BeginTransaction();
		try
		{
			SqlCommand sqlCommand = new SqlCommand();
			sqlCommand.Connection = conn;
			for (int i = 0; i < sqls.Length; i++)
			{
				sqlCommand.CommandText = sqls[i];
				sqlCommand.Transaction = sqlTransaction;
				int rowsAffected = sqlCommand.ExecuteNonQuery();
			}
			sqlTransaction.Commit();
			result = true;
		}
		catch (Exception)
		{
			sqlTransaction.Rollback();
		}
		finally
		{
			conn.Close();
		}
		return result;
	}

	public int ExcuteProc(string procName, SqlParameter[] sqlparams)
	{
		conn.Open();
		SqlCommand sqlCommand = new SqlCommand();
		sqlCommand.CommandType = CommandType.StoredProcedure;
		sqlCommand.CommandText = procName;
		sqlCommand.Connection = conn;
		sqlCommand.Parameters.AddRange(sqlparams);
		int result = sqlCommand.ExecuteNonQuery();
		conn.Close();
		return result;
	}
}
}
