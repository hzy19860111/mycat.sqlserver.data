using MySql.Data.MySqlClient.Properties;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
namespace MySql.Data.MySqlClient
{
	public class MySqlScript
	{
		private MySqlConnection connection;
		private string query;
		private string delimiter;
		[method: CompilerGenerated]
		[CompilerGenerated]
		public event MySqlStatementExecutedEventHandler StatementExecuted;
		[method: CompilerGenerated]
		[CompilerGenerated]
		public event MySqlScriptErrorEventHandler Error;
		[method: CompilerGenerated]
		[CompilerGenerated]
		public event EventHandler ScriptCompleted;
		public MySqlConnection Connection
		{
			get
			{
				return this.connection;
			}
			set
			{
				this.connection = value;
			}
		}
		public string Query
		{
			get
			{
				return this.query;
			}
			set
			{
				this.query = value;
			}
		}
		public string Delimiter
		{
			get
			{
				return this.delimiter;
			}
			set
			{
				this.delimiter = value;
			}
		}
		public MySqlScript()
		{
			this.Delimiter = ";";
		}
		public MySqlScript(MySqlConnection connection) : this()
		{
			this.connection = connection;
		}
		public MySqlScript(string query) : this()
		{
			this.query = query;
		}
		public MySqlScript(MySqlConnection connection, string query) : this()
		{
			this.connection = connection;
			this.query = query;
		}
		public int Execute()
		{
			bool flag = false;
			if (this.connection == null)
			{
				throw new InvalidOperationException(Resources.ConnectionNotSet);
			}
			if (this.query == null || this.query.Length == 0)
			{
				return 0;
			}
			if (this.connection.State != ConnectionState.Open)
			{
				flag = true;
				this.connection.Open();
			}
			bool allowUserVariables = this.connection.Settings.AllowUserVariables;
			this.connection.Settings.AllowUserVariables = true;
			int result;
			try
			{
				string expr_83 = StringUtility.ToUpperInvariant(this.connection.driver.Property("sql_mode"));
				bool ansiQuotes = expr_83.IndexOf("ANSI_QUOTES") != -1;
				bool noBackslashEscapes = expr_83.IndexOf("NO_BACKSLASH_ESCAPES") != -1;
				List<ScriptStatement> arg_BF_0 = this.BreakIntoStatements(ansiQuotes, noBackslashEscapes);
				int num = 0;
				MySqlCommand mySqlCommand = new MySqlCommand(null, this.connection);
				foreach (ScriptStatement current in arg_BF_0)
				{
					if (!string.IsNullOrEmpty(current.text))
					{
						mySqlCommand.CommandText = current.text;
						try
						{
							mySqlCommand.ExecuteNonQuery();
							num++;
							this.OnQueryExecuted(current);
						}
						catch (Exception ex)
						{
							if (this.Error == null)
							{
								throw;
							}
							if (!this.OnScriptError(ex))
							{
								break;
							}
						}
					}
				}
				this.OnScriptCompleted();
				result = num;
			}
			finally
			{
				this.connection.Settings.AllowUserVariables = allowUserVariables;
				if (flag)
				{
					this.connection.Close();
				}
			}
			return result;
		}
		private void OnQueryExecuted(ScriptStatement statement)
		{
			if (this.StatementExecuted != null)
			{
				MySqlScriptEventArgs mySqlScriptEventArgs = new MySqlScriptEventArgs();
				mySqlScriptEventArgs.Statement = statement;
				this.StatementExecuted(this, mySqlScriptEventArgs);
			}
		}
		private void OnScriptCompleted()
		{
			if (this.ScriptCompleted != null)
			{
				this.ScriptCompleted(this, EventArgs.Empty);
			}
		}
		private bool OnScriptError(Exception ex)
		{
			if (this.Error != null)
			{
				MySqlScriptErrorEventArgs mySqlScriptErrorEventArgs = new MySqlScriptErrorEventArgs(ex);
				this.Error(this, mySqlScriptErrorEventArgs);
				return mySqlScriptErrorEventArgs.Ignore;
			}
			return false;
		}
		private List<int> BreakScriptIntoLines()
		{
			List<int> list = new List<int>();
			StringReader stringReader = new StringReader(this.query);
			string text = stringReader.ReadLine();
			int num = 0;
			while (text != null)
			{
				list.Add(num);
				num += text.Length;
				text = stringReader.ReadLine();
			}
			return list;
		}
		private static int FindLineNumber(int position, List<int> lineNumbers)
		{
			int num = 0;
			while (num < lineNumbers.Count && position < lineNumbers[num])
			{
				num++;
			}
			return num;
		}
		private List<ScriptStatement> BreakIntoStatements(bool ansiQuotes, bool noBackslashEscapes)
		{
			string text = this.Delimiter;
			int num = 0;
			List<ScriptStatement> list = new List<ScriptStatement>();
			List<int> list2 = this.BreakScriptIntoLines();
			MySqlTokenizer mySqlTokenizer = new MySqlTokenizer(this.query);
			mySqlTokenizer.AnsiQuotes = ansiQuotes;
			mySqlTokenizer.BackslashEscapes = !noBackslashEscapes;
			for (string text2 = mySqlTokenizer.NextToken(); text2 != null; text2 = mySqlTokenizer.NextToken())
			{
				if (!mySqlTokenizer.Quoted)
				{
					if (text2.ToLower(CultureInfo.InvariantCulture) == "delimiter")
					{
						mySqlTokenizer.NextToken();
						this.AdjustDelimiterEnd(mySqlTokenizer);
						text = this.query.Substring(mySqlTokenizer.StartIndex, mySqlTokenizer.StopIndex - mySqlTokenizer.StartIndex).Trim();
						num = mySqlTokenizer.StopIndex;
					}
					else
					{
						if (text.StartsWith(text2, StringComparison.OrdinalIgnoreCase) && mySqlTokenizer.StartIndex + text.Length <= this.query.Length && this.query.Substring(mySqlTokenizer.StartIndex, text.Length) == text)
						{
							text2 = text;
							mySqlTokenizer.Position = mySqlTokenizer.StartIndex + text.Length;
							mySqlTokenizer.StopIndex = mySqlTokenizer.Position;
						}
						int num2 = text2.IndexOf(text, StringComparison.OrdinalIgnoreCase);
						if (num2 != -1)
						{
							int num3 = mySqlTokenizer.StopIndex - text2.Length + num2;
							if (mySqlTokenizer.StopIndex == this.query.Length - 1)
							{
								num3++;
							}
							string text3 = this.query.Substring(num, num3 - num);
							ScriptStatement scriptStatement = default(ScriptStatement);
							scriptStatement.text = text3.Trim();
							scriptStatement.line = MySqlScript.FindLineNumber(num, list2);
							scriptStatement.position = num - list2[scriptStatement.line];
							list.Add(scriptStatement);
							num = num3 + text.Length;
						}
					}
				}
			}
			if (num < this.query.Length - 1)
			{
				string text4 = this.query.Substring(num).Trim();
				if (!string.IsNullOrEmpty(text4))
				{
					ScriptStatement scriptStatement2 = default(ScriptStatement);
					scriptStatement2.text = text4;
					scriptStatement2.line = MySqlScript.FindLineNumber(num, list2);
					scriptStatement2.position = num - list2[scriptStatement2.line];
					list.Add(scriptStatement2);
				}
			}
			return list;
		}
		private void AdjustDelimiterEnd(MySqlTokenizer tokenizer)
		{
			if (tokenizer.StopIndex < this.query.Length)
			{
				int num = tokenizer.StopIndex;
				char c = this.query[num];
				while (!char.IsWhiteSpace(c) && num < this.query.Length - 1)
				{
					c = this.query[++num];
				}
				tokenizer.StopIndex = num;
				tokenizer.Position = num;
			}
		}
		public Task<int> ExecuteAsync()
		{
			return this.ExecuteAsync(CancellationToken.None);
		}
		public Task<int> ExecuteAsync(CancellationToken cancellationToken)
		{
			TaskCompletionSource<int> taskCompletionSource = new TaskCompletionSource<int>();
			if (cancellationToken == CancellationToken.None || !cancellationToken.IsCancellationRequested)
			{
				try
				{
					int result = this.Execute();
					taskCompletionSource.SetResult(result);
					goto IL_3D;
				}
				catch (Exception exception)
				{
					taskCompletionSource.SetException(exception);
					goto IL_3D;
				}
			}
			taskCompletionSource.SetCanceled();
			IL_3D:
			return taskCompletionSource.Task;
		}
	}
}
