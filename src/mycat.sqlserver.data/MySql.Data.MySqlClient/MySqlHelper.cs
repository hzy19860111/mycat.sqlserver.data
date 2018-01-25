using System;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
namespace MySql.Data.MySqlClient
{
	public sealed class MySqlHelper
	{
		private enum CharClass : byte
		{
			None,
			Quote,
			Backslash
		}
		private static string stringOfBackslashChars = "\\¥Š₩∖﹨＼";
		private static string stringOfQuoteChars = "\"'`´ʹʺʻʼˈˊˋ˙̀́‘’‚′‵❛❜＇";
		private static MySqlHelper.CharClass[] charClassArray = MySqlHelper.makeCharClassArray();
		private MySqlHelper()
		{
		}
		public static int ExecuteNonQuery(MySqlConnection connection, string commandText, params MySqlParameter[] commandParameters)
		{
			MySqlCommand mySqlCommand = new MySqlCommand();
			mySqlCommand.Connection = connection;
			mySqlCommand.CommandText = commandText;
			mySqlCommand.CommandType = CommandType.Text;
			if (commandParameters != null)
			{
				for (int i = 0; i < commandParameters.Length; i++)
				{
					MySqlParameter value = commandParameters[i];
					mySqlCommand.Parameters.Add(value);
				}
			}
			int result = mySqlCommand.ExecuteNonQuery();
			mySqlCommand.Parameters.Clear();
			return result;
		}
		public static int ExecuteNonQuery(string connectionString, string commandText, params MySqlParameter[] parms)
		{
			int result;
			using (MySqlConnection mySqlConnection = new MySqlConnection(connectionString))
			{
				mySqlConnection.Open();
				result = MySqlHelper.ExecuteNonQuery(mySqlConnection, commandText, parms);
			}
			return result;
		}
		public static DataRow ExecuteDataRow(string connectionString, string commandText, params MySqlParameter[] parms)
		{
			DataSet dataSet = MySqlHelper.ExecuteDataset(connectionString, commandText, parms);
			if (dataSet == null)
			{
				return null;
			}
			if (dataSet.Tables.Count == 0)
			{
				return null;
			}
			if (dataSet.Tables[0].Rows.Count == 0)
			{
				return null;
			}
			return dataSet.Tables[0].Rows[0];
		}
		public static DataSet ExecuteDataset(string connectionString, string commandText)
		{
			return MySqlHelper.ExecuteDataset(connectionString, commandText, null);
		}
		public static DataSet ExecuteDataset(string connectionString, string commandText, params MySqlParameter[] commandParameters)
		{
			DataSet result;
			using (MySqlConnection mySqlConnection = new MySqlConnection(connectionString))
			{
				mySqlConnection.Open();
				result = MySqlHelper.ExecuteDataset(mySqlConnection, commandText, commandParameters);
			}
			return result;
		}
		public static DataSet ExecuteDataset(MySqlConnection connection, string commandText)
		{
			return MySqlHelper.ExecuteDataset(connection, commandText, null);
		}
		public static DataSet ExecuteDataset(MySqlConnection connection, string commandText, params MySqlParameter[] commandParameters)
		{
			MySqlCommand mySqlCommand = new MySqlCommand();
			mySqlCommand.Connection = connection;
			mySqlCommand.CommandText = commandText;
			mySqlCommand.CommandType = CommandType.Text;
			if (commandParameters != null)
			{
				for (int i = 0; i < commandParameters.Length; i++)
				{
					MySqlParameter value = commandParameters[i];
					mySqlCommand.Parameters.Add(value);
				}
			}
			DataAdapter arg_4E_0 = new MySqlDataAdapter(mySqlCommand);
			DataSet dataSet = new DataSet();
			arg_4E_0.Fill(dataSet);
			mySqlCommand.Parameters.Clear();
			return dataSet;
		}
		public static void UpdateDataSet(string connectionString, string commandText, DataSet ds, string tablename)
		{
			MySqlConnection mySqlConnection = new MySqlConnection(connectionString);
			mySqlConnection.Open();
			MySqlDataAdapter expr_14 = new MySqlDataAdapter(commandText, mySqlConnection);
			new MySqlCommandBuilder(expr_14).ToString();
			expr_14.Update(ds, tablename);
			mySqlConnection.Close();
		}
		private static MySqlDataReader ExecuteReader(MySqlConnection connection, MySqlTransaction transaction, string commandText, MySqlParameter[] commandParameters, bool ExternalConn)
		{
			MySqlCommand mySqlCommand = new MySqlCommand();
			mySqlCommand.Connection = connection;
			mySqlCommand.Transaction = transaction;
			mySqlCommand.CommandText = commandText;
			mySqlCommand.CommandType = CommandType.Text;
			if (commandParameters != null)
			{
				for (int i = 0; i < commandParameters.Length; i++)
				{
					MySqlParameter value = commandParameters[i];
					mySqlCommand.Parameters.Add(value);
				}
			}
			MySqlDataReader result;
			if (ExternalConn)
			{
				result = mySqlCommand.ExecuteReader();
			}
			else
			{
				result = mySqlCommand.ExecuteReader(CommandBehavior.CloseConnection);
			}
			mySqlCommand.Parameters.Clear();
			return result;
		}
		public static MySqlDataReader ExecuteReader(string connectionString, string commandText)
		{
			return MySqlHelper.ExecuteReader(connectionString, commandText, null);
		}
		public static MySqlDataReader ExecuteReader(MySqlConnection connection, string commandText)
		{
			return MySqlHelper.ExecuteReader(connection, null, commandText, null, true);
		}
		public static MySqlDataReader ExecuteReader(string connectionString, string commandText, params MySqlParameter[] commandParameters)
		{
			MySqlConnection expr_06 = new MySqlConnection(connectionString);
			expr_06.Open();
			return MySqlHelper.ExecuteReader(expr_06, null, commandText, commandParameters, false);
		}
		public static MySqlDataReader ExecuteReader(MySqlConnection connection, string commandText, params MySqlParameter[] commandParameters)
		{
			return MySqlHelper.ExecuteReader(connection, null, commandText, commandParameters, true);
		}
		public static object ExecuteScalar(string connectionString, string commandText)
		{
			return MySqlHelper.ExecuteScalar(connectionString, commandText, null);
		}
		public static object ExecuteScalar(string connectionString, string commandText, params MySqlParameter[] commandParameters)
		{
			object result;
			using (MySqlConnection mySqlConnection = new MySqlConnection(connectionString))
			{
				mySqlConnection.Open();
				result = MySqlHelper.ExecuteScalar(mySqlConnection, commandText, commandParameters);
			}
			return result;
		}
		public static object ExecuteScalar(MySqlConnection connection, string commandText)
		{
			return MySqlHelper.ExecuteScalar(connection, commandText, null);
		}
		public static object ExecuteScalar(MySqlConnection connection, string commandText, params MySqlParameter[] commandParameters)
		{
			MySqlCommand mySqlCommand = new MySqlCommand();
			mySqlCommand.Connection = connection;
			mySqlCommand.CommandText = commandText;
			mySqlCommand.CommandType = CommandType.Text;
			if (commandParameters != null)
			{
				for (int i = 0; i < commandParameters.Length; i++)
				{
					MySqlParameter value = commandParameters[i];
					mySqlCommand.Parameters.Add(value);
				}
			}
			object result = mySqlCommand.ExecuteScalar();
			mySqlCommand.Parameters.Clear();
			return result;
		}
		private static MySqlHelper.CharClass[] makeCharClassArray()
		{
			MySqlHelper.CharClass[] array = new MySqlHelper.CharClass[65536];
			string text = MySqlHelper.stringOfBackslashChars;
			for (int i = 0; i < text.Length; i++)
			{
				char c = text[i];
				array[(int)c] = MySqlHelper.CharClass.Backslash;
			}
			text = MySqlHelper.stringOfQuoteChars;
			for (int i = 0; i < text.Length; i++)
			{
				char c2 = text[i];
				array[(int)c2] = MySqlHelper.CharClass.Quote;
			}
			return array;
		}
		private static bool needsQuoting(string s)
		{
			for (int i = 0; i < s.Length; i++)
			{
				char c = s[i];
				if (MySqlHelper.charClassArray[(int)c] != MySqlHelper.CharClass.None)
				{
					return true;
				}
			}
			return false;
		}
		public static string EscapeString(string value)
		{
			if (!MySqlHelper.needsQuoting(value))
			{
				return value;
			}
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < value.Length; i++)
			{
				char c = value[i];
				if (MySqlHelper.charClassArray[(int)c] != MySqlHelper.CharClass.None)
				{
					stringBuilder.Append("\\");
				}
				stringBuilder.Append(c);
			}
			return stringBuilder.ToString();
		}
		public static string DoubleQuoteString(string value)
		{
			if (!MySqlHelper.needsQuoting(value))
			{
				return value;
			}
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < value.Length; i++)
			{
				char c = value[i];
				MySqlHelper.CharClass charClass = MySqlHelper.charClassArray[(int)c];
				if (charClass == MySqlHelper.CharClass.Quote)
				{
					stringBuilder.Append(c);
				}
				else if (charClass == MySqlHelper.CharClass.Backslash)
				{
					stringBuilder.Append("\\");
				}
				stringBuilder.Append(c);
			}
			return stringBuilder.ToString();
		}
		public static Task<DataRow> ExecuteDataRowAsync(string connectionString, string commandText, params MySqlParameter[] parms)
		{
			return MySqlHelper.ExecuteDataRowAsync(connectionString, commandText, CancellationToken.None, parms);
		}
		public static Task<DataRow> ExecuteDataRowAsync(string connectionString, string commandText, CancellationToken cancellationToken, params MySqlParameter[] parms)
		{
			TaskCompletionSource<DataRow> taskCompletionSource = new TaskCompletionSource<DataRow>();
			if (cancellationToken == CancellationToken.None || !cancellationToken.IsCancellationRequested)
			{
				try
				{
					DataRow result = MySqlHelper.ExecuteDataRow(connectionString, commandText, parms);
					taskCompletionSource.SetResult(result);
					goto IL_3F;
				}
				catch (Exception exception)
				{
					taskCompletionSource.SetException(exception);
					goto IL_3F;
				}
			}
			taskCompletionSource.SetCanceled();
			IL_3F:
			return taskCompletionSource.Task;
		}
		public static Task<int> ExecuteNonQueryAsync(MySqlConnection connection, string commandText, params MySqlParameter[] commandParameters)
		{
			return MySqlHelper.ExecuteNonQueryAsync(connection, commandText, CancellationToken.None, commandParameters);
		}
		public static Task<int> ExecuteNonQueryAsync(MySqlConnection connection, string commandText, CancellationToken cancellationToken, params MySqlParameter[] commandParameters)
		{
			TaskCompletionSource<int> taskCompletionSource = new TaskCompletionSource<int>();
			if (cancellationToken == CancellationToken.None || !cancellationToken.IsCancellationRequested)
			{
				try
				{
					int result = MySqlHelper.ExecuteNonQuery(connection, commandText, commandParameters);
					taskCompletionSource.SetResult(result);
					goto IL_3F;
				}
				catch (Exception exception)
				{
					taskCompletionSource.SetException(exception);
					goto IL_3F;
				}
			}
			taskCompletionSource.SetCanceled();
			IL_3F:
			return taskCompletionSource.Task;
		}
		public static Task<int> ExecuteNonQueryAsync(string connectionString, string commandText, params MySqlParameter[] commandParameters)
		{
			return MySqlHelper.ExecuteNonQueryAsync(connectionString, commandText, CancellationToken.None, commandParameters);
		}
		public static Task<int> ExecuteNonQueryAsync(string connectionString, string commandText, CancellationToken cancellationToken, params MySqlParameter[] commandParameters)
		{
			TaskCompletionSource<int> taskCompletionSource = new TaskCompletionSource<int>();
			if (cancellationToken == CancellationToken.None || !cancellationToken.IsCancellationRequested)
			{
				try
				{
					int result = MySqlHelper.ExecuteNonQuery(connectionString, commandText, commandParameters);
					taskCompletionSource.SetResult(result);
					goto IL_3F;
				}
				catch (Exception exception)
				{
					taskCompletionSource.SetException(exception);
					goto IL_3F;
				}
			}
			taskCompletionSource.SetCanceled();
			IL_3F:
			return taskCompletionSource.Task;
		}
		public static Task<DataSet> ExecuteDatasetAsync(string connectionString, string commandText)
		{
			return MySqlHelper.ExecuteDatasetAsync(connectionString, commandText, CancellationToken.None, null);
		}
		public static Task<DataSet> ExecuteDatasetAsync(string connectionString, string commandText, CancellationToken cancellationToken)
		{
			return MySqlHelper.ExecuteDatasetAsync(connectionString, commandText, cancellationToken, null);
		}
		public static Task<DataSet> ExecuteDatasetAsync(string connectionString, string commandText, params MySqlParameter[] commandParameters)
		{
			return MySqlHelper.ExecuteDatasetAsync(connectionString, commandText, CancellationToken.None, commandParameters);
		}
		public static Task<DataSet> ExecuteDatasetAsync(string connectionString, string commandText, CancellationToken cancellationToken, params MySqlParameter[] commandParameters)
		{
			TaskCompletionSource<DataSet> taskCompletionSource = new TaskCompletionSource<DataSet>();
			if (cancellationToken == CancellationToken.None || !cancellationToken.IsCancellationRequested)
			{
				try
				{
					DataSet result = MySqlHelper.ExecuteDataset(connectionString, commandText, commandParameters);
					taskCompletionSource.SetResult(result);
					goto IL_3F;
				}
				catch (Exception exception)
				{
					taskCompletionSource.SetException(exception);
					goto IL_3F;
				}
			}
			taskCompletionSource.SetCanceled();
			IL_3F:
			return taskCompletionSource.Task;
		}
		public static Task<DataSet> ExecuteDatasetAsync(MySqlConnection connection, string commandText)
		{
			return MySqlHelper.ExecuteDatasetAsync(connection, commandText, CancellationToken.None, null);
		}
		public static Task<DataSet> ExecuteDatasetAsync(MySqlConnection connection, string commandText, CancellationToken cancellationToken)
		{
			return MySqlHelper.ExecuteDatasetAsync(connection, commandText, cancellationToken, null);
		}
		public static Task<DataSet> ExecuteDatasetAsync(MySqlConnection connection, string commandText, params MySqlParameter[] commandParameters)
		{
			return MySqlHelper.ExecuteDatasetAsync(connection, commandText, CancellationToken.None, commandParameters);
		}
		public static Task<DataSet> ExecuteDatasetAsync(MySqlConnection connection, string commandText, CancellationToken cancellationToken, params MySqlParameter[] commandParameters)
		{
			TaskCompletionSource<DataSet> taskCompletionSource = new TaskCompletionSource<DataSet>();
			if (cancellationToken == CancellationToken.None || !cancellationToken.IsCancellationRequested)
			{
				try
				{
					DataSet result = MySqlHelper.ExecuteDataset(connection, commandText, commandParameters);
					taskCompletionSource.SetResult(result);
					goto IL_3F;
				}
				catch (Exception exception)
				{
					taskCompletionSource.SetException(exception);
					goto IL_3F;
				}
			}
			taskCompletionSource.SetCanceled();
			IL_3F:
			return taskCompletionSource.Task;
		}
		public static Task UpdateDataSetAsync(string connectionString, string commandText, DataSet ds, string tablename)
		{
			return MySqlHelper.UpdateDataSetAsync(connectionString, commandText, ds, tablename, CancellationToken.None);
		}
		public static Task UpdateDataSetAsync(string connectionString, string commandText, DataSet ds, string tablename, CancellationToken cancellationToken)
		{
			TaskCompletionSource<bool> taskCompletionSource = new TaskCompletionSource<bool>();
			if (cancellationToken == CancellationToken.None || !cancellationToken.IsCancellationRequested)
			{
				try
				{
					MySqlHelper.UpdateDataSet(connectionString, commandText, ds, tablename);
					taskCompletionSource.SetResult(true);
					goto IL_40;
				}
				catch (Exception exception)
				{
					taskCompletionSource.SetException(exception);
					goto IL_40;
				}
			}
			taskCompletionSource.SetCanceled();
			IL_40:
			return taskCompletionSource.Task;
		}
		private static Task<MySqlDataReader> ExecuteReaderAsync(MySqlConnection connection, MySqlTransaction transaction, string commandText, MySqlParameter[] commandParameters, bool ExternalConn)
		{
			return MySqlHelper.ExecuteReaderAsync(connection, transaction, commandText, commandParameters, ExternalConn, CancellationToken.None);
		}
		private static Task<MySqlDataReader> ExecuteReaderAsync(MySqlConnection connection, MySqlTransaction transaction, string commandText, MySqlParameter[] commandParameters, bool ExternalConn, CancellationToken cancellationToken)
		{
			TaskCompletionSource<MySqlDataReader> taskCompletionSource = new TaskCompletionSource<MySqlDataReader>();
			if (cancellationToken == CancellationToken.None || !cancellationToken.IsCancellationRequested)
			{
				try
				{
					MySqlDataReader result = MySqlHelper.ExecuteReader(connection, transaction, commandText, commandParameters, ExternalConn);
					taskCompletionSource.SetResult(result);
					goto IL_43;
				}
				catch (Exception exception)
				{
					taskCompletionSource.SetException(exception);
					goto IL_43;
				}
			}
			taskCompletionSource.SetCanceled();
			IL_43:
			return taskCompletionSource.Task;
		}
		public static Task<MySqlDataReader> ExecuteReaderAsync(string connectionString, string commandText)
		{
			return MySqlHelper.ExecuteReaderAsync(connectionString, commandText, CancellationToken.None, null);
		}
		public static Task<MySqlDataReader> ExecuteReaderAsync(string connectionString, string commandText, CancellationToken cancellationToken)
		{
			return MySqlHelper.ExecuteReaderAsync(connectionString, commandText, cancellationToken, null);
		}
		public static Task<MySqlDataReader> ExecuteReaderAsync(MySqlConnection connection, string commandText)
		{
			return MySqlHelper.ExecuteReaderAsync(connection, null, commandText, null, true, CancellationToken.None);
		}
		public static Task<MySqlDataReader> ExecuteReaderAsync(MySqlConnection connection, string commandText, CancellationToken cancellationToken)
		{
			return MySqlHelper.ExecuteReaderAsync(connection, null, commandText, null, true, cancellationToken);
		}
		public static Task<MySqlDataReader> ExecuteReaderAsync(string connectionString, string commandText, params MySqlParameter[] commandParameters)
		{
			return MySqlHelper.ExecuteReaderAsync(connectionString, commandText, CancellationToken.None, commandParameters);
		}
		public static Task<MySqlDataReader> ExecuteReaderAsync(string connectionString, string commandText, CancellationToken cancellationToken, params MySqlParameter[] commandParameters)
		{
			TaskCompletionSource<MySqlDataReader> taskCompletionSource = new TaskCompletionSource<MySqlDataReader>();
			if (cancellationToken == CancellationToken.None || !cancellationToken.IsCancellationRequested)
			{
				try
				{
					MySqlDataReader result = MySqlHelper.ExecuteReader(connectionString, commandText, commandParameters);
					taskCompletionSource.SetResult(result);
					goto IL_3F;
				}
				catch (Exception exception)
				{
					taskCompletionSource.SetException(exception);
					goto IL_3F;
				}
			}
			taskCompletionSource.SetCanceled();
			IL_3F:
			return taskCompletionSource.Task;
		}
		public static Task<MySqlDataReader> ExecuteReaderAsync(MySqlConnection connection, string commandText, params MySqlParameter[] commandParameters)
		{
			return MySqlHelper.ExecuteReaderAsync(connection, null, commandText, commandParameters, true, CancellationToken.None);
		}
		public static Task<MySqlDataReader> ExecuteReaderAsync(MySqlConnection connection, string commandText, CancellationToken cancellationToken, params MySqlParameter[] commandParameters)
		{
			return MySqlHelper.ExecuteReaderAsync(connection, null, commandText, commandParameters, true, cancellationToken);
		}
		public static Task<object> ExecuteScalarAsync(string connectionString, string commandText)
		{
			return MySqlHelper.ExecuteScalarAsync(connectionString, commandText, CancellationToken.None, null);
		}
		public static Task<object> ExecuteScalarAsync(string connectionString, string commandText, CancellationToken cancellationToken)
		{
			return MySqlHelper.ExecuteScalarAsync(connectionString, commandText, cancellationToken, null);
		}
		public static Task<object> ExecuteScalarAsync(string connectionString, string commandText, params MySqlParameter[] commandParameters)
		{
			return MySqlHelper.ExecuteScalarAsync(connectionString, commandText, CancellationToken.None, commandParameters);
		}
		public static Task<object> ExecuteScalarAsync(string connectionString, string commandText, CancellationToken cancellationToken, params MySqlParameter[] commandParameters)
		{
			TaskCompletionSource<object> taskCompletionSource = new TaskCompletionSource<object>();
			if (cancellationToken == CancellationToken.None || !cancellationToken.IsCancellationRequested)
			{
				try
				{
					object result = MySqlHelper.ExecuteScalar(connectionString, commandText, commandParameters);
					taskCompletionSource.SetResult(result);
					goto IL_3F;
				}
				catch (Exception exception)
				{
					taskCompletionSource.SetException(exception);
					goto IL_3F;
				}
			}
			taskCompletionSource.SetCanceled();
			IL_3F:
			return taskCompletionSource.Task;
		}
		public static Task<object> ExecuteScalarAsync(MySqlConnection connection, string commandText)
		{
			return MySqlHelper.ExecuteScalarAsync(connection, commandText, CancellationToken.None, null);
		}
		public static Task<object> ExecuteScalarAsync(MySqlConnection connection, string commandText, CancellationToken cancellationToken)
		{
			return MySqlHelper.ExecuteScalarAsync(connection, commandText, cancellationToken, null);
		}
		public static Task<object> ExecuteScalarAsync(MySqlConnection connection, string commandText, params MySqlParameter[] commandParameters)
		{
			return MySqlHelper.ExecuteScalarAsync(connection, commandText, CancellationToken.None, commandParameters);
		}
		public static Task<object> ExecuteScalarAsync(MySqlConnection connection, string commandText, CancellationToken cancellationToken, params MySqlParameter[] commandParameters)
		{
			TaskCompletionSource<object> taskCompletionSource = new TaskCompletionSource<object>();
			if (cancellationToken == CancellationToken.None || !cancellationToken.IsCancellationRequested)
			{
				try
				{
					object result = MySqlHelper.ExecuteScalar(connection, commandText, commandParameters);
					taskCompletionSource.SetResult(result);
					goto IL_3F;
				}
				catch (Exception exception)
				{
					taskCompletionSource.SetException(exception);
					goto IL_3F;
				}
			}
			taskCompletionSource.SetCanceled();
			IL_3F:
			return taskCompletionSource.Task;
		}
	}
}
