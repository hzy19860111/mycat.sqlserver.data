using MySql.Data.MySqlClient.Properties;
using MySql.Data.Types;
using System;
using System.Collections;
using System.Data;
using System.Data.Common;
using System.Data.SqlTypes;
using System.Globalization;
using System.IO;
using System.Threading;
namespace MySql.Data.MySqlClient
{
	public sealed class MySqlDataReader : DbDataReader, IDisposable, IDataReader, IDataRecord
	{
		private bool isOpen = true;
		private CommandBehavior commandBehavior;
		private MySqlCommand command;
		internal long affectedRows;
		internal Driver driver;
		private PreparableStatement statement;
		private ResultSet resultSet;
		private bool disableZeroAffectedRows;
		private MySqlConnection connection;
		internal PreparableStatement Statement
		{
			get
			{
				return this.statement;
			}
		}
		internal MySqlCommand Command
		{
			get
			{
				return this.command;
			}
		}
		internal ResultSet ResultSet
		{
			get
			{
				return this.resultSet;
			}
		}
		internal CommandBehavior CommandBehavior
		{
			get
			{
				return this.commandBehavior;
			}
		}
		public override int FieldCount
		{
			get
			{
				if (this.resultSet != null)
				{
					return this.resultSet.Size;
				}
				return 0;
			}
		}
		public override bool HasRows
		{
			get
			{
				return this.resultSet != null && this.resultSet.HasRows;
			}
		}
		public override bool IsClosed
		{
			get
			{
				return !this.isOpen;
			}
		}
		public override int RecordsAffected
		{
			get
			{
				if (this.disableZeroAffectedRows && this.affectedRows == 0L)
				{
					return -1;
				}
				return (int)this.affectedRows;
			}
		}
		public override object this[int i]
		{
			get
			{
				return this.GetValue(i);
			}
		}
		public override object this[string name]
		{
			get
			{
				return this[this.GetOrdinal(name)];
			}
		}
		public override int Depth
		{
			get
			{
				return 0;
			}
		}
		internal MySqlDataReader(MySqlCommand cmd, PreparableStatement statement, CommandBehavior behavior)
		{
			this.command = cmd;
			this.connection = this.command.Connection;
			this.commandBehavior = behavior;
			this.driver = this.connection.driver;
			this.affectedRows = -1L;
			this.statement = statement;
			if (cmd.CommandType == CommandType.StoredProcedure && cmd.UpdatedRowSource == UpdateRowSource.FirstReturnedRecord)
			{
				this.disableZeroAffectedRows = true;
			}
		}
		public override void Close()
		{
			if (!this.isOpen)
			{
				return;
			}
			bool flag = (this.commandBehavior & CommandBehavior.CloseConnection) > CommandBehavior.Default;
			CommandBehavior commandBehavior = this.commandBehavior;
			try
			{
				if (!commandBehavior.Equals(CommandBehavior.SchemaOnly))
				{
					this.commandBehavior = CommandBehavior.Default;
				}
				while (this.NextResult())
				{
				}
			}
			catch (MySqlException ex)
			{
				if (!ex.IsQueryAborted)
				{
					bool flag2 = false;
					for (Exception ex2 = ex; ex2 != null; ex2 = ex2.InnerException)
					{
						if (ex2 is IOException)
						{
							flag2 = true;
							break;
						}
					}
					if (!flag2)
					{
						throw;
					}
				}
			}
			catch (IOException)
			{
			}
			finally
			{
				this.connection.Reader = null;
				this.commandBehavior = commandBehavior;
			}
			this.command.Close(this);
			this.commandBehavior = CommandBehavior.Default;
			if (this.command.Canceled && this.connection.driver.Version.isAtLeast(5, 1, 0))
			{
				this.ClearKillFlag();
			}
			if (flag)
			{
				this.connection.Close();
			}
			this.command = null;
			this.connection.IsInUse = false;
			this.connection = null;
			this.isOpen = false;
		}
		public bool GetBoolean(string name)
		{
			return this.GetBoolean(this.GetOrdinal(name));
		}
		public override bool GetBoolean(int i)
		{
			return Convert.ToBoolean(this.GetValue(i));
		}
		public byte GetByte(string name)
		{
			return this.GetByte(this.GetOrdinal(name));
		}
		public override byte GetByte(int i)
		{
			IMySqlValue fieldValue = this.GetFieldValue(i, false);
			if (fieldValue is MySqlUByte)
			{
				return ((MySqlUByte)fieldValue).Value;
			}
			return (byte)((MySqlByte)fieldValue).Value;
		}
		public sbyte GetSByte(string name)
		{
			return this.GetSByte(this.GetOrdinal(name));
		}
		public sbyte GetSByte(int i)
		{
			IMySqlValue fieldValue = this.GetFieldValue(i, false);
			//object arg_0F_0 = fieldValue as MySqlByte;
			return ((MySqlByte)fieldValue).Value;
		}
		public override long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
		{
			if (i >= this.FieldCount)
			{
				this.Throw(new IndexOutOfRangeException());
			}
			IMySqlValue fieldValue = this.GetFieldValue(i, false);
			if (!(fieldValue is MySqlBinary) && !(fieldValue is MySqlGuid))
			{
				this.Throw(new MySqlException("GetBytes can only be called on binary or guid columns"));
			}
			byte[] array;
			if (fieldValue is MySqlBinary)
			{
				array = ((MySqlBinary)fieldValue).Value;
			}
			else
			{
				array = ((MySqlGuid)fieldValue).Bytes;
			}
			if (buffer == null)
			{
				return (long)array.Length;
			}
			if (bufferoffset >= buffer.Length || bufferoffset < 0)
			{
				this.Throw(new IndexOutOfRangeException("Buffer index must be a valid index in buffer"));
			}
			if (buffer.Length < bufferoffset + length)
			{
				this.Throw(new ArgumentException("Buffer is not large enough to hold the requested data"));
			}
			if (fieldOffset < 0L || (fieldOffset >= (long)array.Length && (long)array.Length != 0L))
			{
				this.Throw(new IndexOutOfRangeException("Data index must be a valid index in the field"));
			}
			if ((long)array.Length < fieldOffset + (long)length)
			{
				length = (int)((long)array.Length - fieldOffset);
			}
			Buffer.BlockCopy(array, (int)fieldOffset, buffer, bufferoffset, length);
			return (long)length;
		}
		private object ChangeType(IMySqlValue value, int fieldIndex, Type newType)
		{
			this.resultSet.Fields[fieldIndex].AddTypeConversion(newType);
			return Convert.ChangeType(value.Value, newType, CultureInfo.InvariantCulture);
		}
		public char GetChar(string name)
		{
			return this.GetChar(this.GetOrdinal(name));
		}
		public override char GetChar(int i)
		{
			return this.GetString(i)[0];
		}
		public override long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
		{
			if (i >= this.FieldCount)
			{
				this.Throw(new IndexOutOfRangeException());
			}
			string @string = this.GetString(i);
			if (buffer == null)
			{
				return (long)@string.Length;
			}
			if (bufferoffset >= buffer.Length || bufferoffset < 0)
			{
				this.Throw(new IndexOutOfRangeException("Buffer index must be a valid index in buffer"));
			}
			if (buffer.Length < bufferoffset + length)
			{
				this.Throw(new ArgumentException("Buffer is not large enough to hold the requested data"));
			}
			if (fieldoffset < 0L || fieldoffset >= (long)@string.Length)
			{
				this.Throw(new IndexOutOfRangeException("Field offset must be a valid index in the field"));
			}
			if (@string.Length < length)
			{
				length = @string.Length;
			}
			@string.CopyTo((int)fieldoffset, buffer, bufferoffset, length);
			return (long)length;
		}
		public override string GetDataTypeName(int i)
		{
			if (!this.isOpen)
			{
				this.Throw(new Exception("No current query in data reader"));
			}
			if (i >= this.FieldCount)
			{
				this.Throw(new IndexOutOfRangeException());
			}
			return this.resultSet.Values[i].MySqlTypeName;
		}
		public MySqlDateTime GetMySqlDateTime(string column)
		{
			return this.GetMySqlDateTime(this.GetOrdinal(column));
		}
		public MySqlDateTime GetMySqlDateTime(int column)
		{
			return (MySqlDateTime)this.GetFieldValue(column, true);
		}
		public DateTime GetDateTime(string column)
		{
			return this.GetDateTime(this.GetOrdinal(column));
		}
		public override DateTime GetDateTime(int i)
		{
			IMySqlValue fieldValue = this.GetFieldValue(i, true);
			MySqlDateTime mySqlDateTime;
			if (fieldValue is MySqlDateTime)
			{
				mySqlDateTime = (MySqlDateTime)fieldValue;
			}
			else
			{
				mySqlDateTime = MySqlDateTime.Parse(this.GetString(i));
			}
			mySqlDateTime.TimezoneOffset = this.driver.timeZoneOffset;
			if (this.connection.Settings.ConvertZeroDateTime && !mySqlDateTime.IsValidDateTime)
			{
				return DateTime.MinValue;
			}
			return mySqlDateTime.GetDateTime();
		}
		public MySqlDecimal GetMySqlDecimal(string column)
		{
			return this.GetMySqlDecimal(this.GetOrdinal(column));
		}
		public MySqlDecimal GetMySqlDecimal(int i)
		{
			return (MySqlDecimal)this.GetFieldValue(i, false);
		}
		public decimal GetDecimal(string column)
		{
			return this.GetDecimal(this.GetOrdinal(column));
		}
		public override decimal GetDecimal(int i)
		{
			IMySqlValue fieldValue = this.GetFieldValue(i, true);
			if (fieldValue is MySqlDecimal)
			{
				return ((MySqlDecimal)fieldValue).Value;
			}
			return Convert.ToDecimal(fieldValue.Value);
		}
		public double GetDouble(string column)
		{
			return this.GetDouble(this.GetOrdinal(column));
		}
		public override double GetDouble(int i)
		{
			IMySqlValue fieldValue = this.GetFieldValue(i, true);
			if (fieldValue is MySqlDouble)
			{
				return ((MySqlDouble)fieldValue).Value;
			}
			return Convert.ToDouble(fieldValue.Value);
		}
		public Type GetFieldType(string column)
		{
			return this.GetFieldType(this.GetOrdinal(column));
		}
		public override Type GetFieldType(int i)
		{
			if (!this.isOpen)
			{
				this.Throw(new Exception("No current query in data reader"));
			}
			if (i >= this.FieldCount)
			{
				this.Throw(new IndexOutOfRangeException());
			}
			IMySqlValue mySqlValue = this.resultSet.Values[i];
			if (!(mySqlValue is MySqlDateTime))
			{
				return mySqlValue.SystemType;
			}
			if (!this.connection.Settings.AllowZeroDateTime)
			{
				return typeof(DateTime);
			}
			return typeof(MySqlDateTime);
		}
		public float GetFloat(string column)
		{
			return this.GetFloat(this.GetOrdinal(column));
		}
		public override float GetFloat(int i)
		{
			IMySqlValue fieldValue = this.GetFieldValue(i, true);
			if (fieldValue is MySqlSingle)
			{
				return ((MySqlSingle)fieldValue).Value;
			}
			return Convert.ToSingle(fieldValue.Value);
		}
		public Guid GetGuid(string column)
		{
			return this.GetGuid(this.GetOrdinal(column));
		}
		public override Guid GetGuid(int i)
		{
			object value = this.GetValue(i);
			if (value is Guid)
			{
				return (Guid)value;
			}
			if (value is string)
			{
				return new Guid(value as string);
			}
			if (value is byte[])
			{
				byte[] array = (byte[])value;
				if (array.Length == 16)
				{
					return new Guid(array);
				}
			}
			this.Throw(new MySqlException(Resources.ValueNotSupportedForGuid));
			return Guid.Empty;
		}
		public short GetInt16(string column)
		{
			return this.GetInt16(this.GetOrdinal(column));
		}
		public override short GetInt16(int i)
		{
			IMySqlValue fieldValue = this.GetFieldValue(i, true);
			if (fieldValue is MySqlInt16)
			{
				return ((MySqlInt16)fieldValue).Value;
			}
			return (short)this.ChangeType(fieldValue, i, typeof(short));
		}
		public int GetInt32(string column)
		{
			return this.GetInt32(this.GetOrdinal(column));
		}
		public override int GetInt32(int i)
		{
			IMySqlValue fieldValue = this.GetFieldValue(i, true);
			if (fieldValue is MySqlInt32)
			{
				return ((MySqlInt32)fieldValue).Value;
			}
			return (int)this.ChangeType(fieldValue, i, typeof(int));
		}
		public long GetInt64(string column)
		{
			return this.GetInt64(this.GetOrdinal(column));
		}
		public override long GetInt64(int i)
		{
			IMySqlValue fieldValue = this.GetFieldValue(i, true);
			if (fieldValue is MySqlInt64)
			{
				return ((MySqlInt64)fieldValue).Value;
			}
			return (long)this.ChangeType(fieldValue, i, typeof(long));
		}
		public override string GetName(int i)
		{
			if (!this.isOpen)
			{
				this.Throw(new Exception("No current query in data reader"));
			}
			if (i >= this.FieldCount)
			{
				this.Throw(new IndexOutOfRangeException());
			}
			return this.resultSet.Fields[i].ColumnName;
		}
		public override int GetOrdinal(string name)
		{
			if (!this.isOpen || this.resultSet == null)
			{
				this.Throw(new Exception("No current query in data reader"));
			}
			return this.resultSet.GetOrdinal(name);
		}
		public string GetString(string column)
		{
			return this.GetString(this.GetOrdinal(column));
		}
		public override string GetString(int i)
		{
			IMySqlValue fieldValue = this.GetFieldValue(i, true);
			if (fieldValue is MySqlBinary)
			{
				byte[] value = ((MySqlBinary)fieldValue).Value;
				return this.resultSet.Fields[i].Encoding.GetString(value, 0, value.Length);
			}
			return fieldValue.Value.ToString();
		}
		public TimeSpan GetTimeSpan(string column)
		{
			return this.GetTimeSpan(this.GetOrdinal(column));
		}
		public TimeSpan GetTimeSpan(int column)
		{
			return ((MySqlTimeSpan)this.GetFieldValue(column, true)).Value;
		}
		public override object GetValue(int i)
		{
			if (!this.isOpen)
			{
				this.Throw(new Exception("No current query in data reader"));
			}
			if (i >= this.FieldCount)
			{
				this.Throw(new IndexOutOfRangeException());
			}
			IMySqlValue fieldValue = this.GetFieldValue(i, false);
			if (fieldValue.IsNull)
			{
				return DBNull.Value;
			}
			if (!(fieldValue is MySqlDateTime))
			{
				return fieldValue.Value;
			}
			MySqlDateTime mySqlDateTime = (MySqlDateTime)fieldValue;
			if (!mySqlDateTime.IsValidDateTime && this.connection.Settings.ConvertZeroDateTime)
			{
				return DateTime.MinValue;
			}
			if (this.connection.Settings.AllowZeroDateTime)
			{
				return fieldValue;
			}
			return mySqlDateTime.GetDateTime();
		}
		public override int GetValues(object[] values)
		{
			int num = Math.Min(values.Length, this.FieldCount);
			for (int i = 0; i < num; i++)
			{
				values[i] = this.GetValue(i);
			}
			return num;
		}
		public ushort GetUInt16(string column)
		{
			return this.GetUInt16(this.GetOrdinal(column));
		}
		public ushort GetUInt16(int column)
		{
			IMySqlValue fieldValue = this.GetFieldValue(column, true);
			if (fieldValue is MySqlUInt16)
			{
				return ((MySqlUInt16)fieldValue).Value;
			}
			return (ushort)this.ChangeType(fieldValue, column, typeof(ushort));
		}
		public uint GetUInt32(string column)
		{
			return this.GetUInt32(this.GetOrdinal(column));
		}
		public uint GetUInt32(int column)
		{
			IMySqlValue fieldValue = this.GetFieldValue(column, true);
			if (fieldValue is MySqlUInt32)
			{
				return ((MySqlUInt32)fieldValue).Value;
			}
			return (uint)this.ChangeType(fieldValue, column, typeof(uint));
		}
		public ulong GetUInt64(string column)
		{
			return this.GetUInt64(this.GetOrdinal(column));
		}
		public ulong GetUInt64(int column)
		{
			IMySqlValue fieldValue = this.GetFieldValue(column, true);
			if (fieldValue is MySqlUInt64)
			{
				return ((MySqlUInt64)fieldValue).Value;
			}
			return (ulong)this.ChangeType(fieldValue, column, typeof(ulong));
		}
		IDataReader IDataRecord.GetData(int i)
		{
			return base.GetData(i);
		}
		public override bool IsDBNull(int i)
		{
			return DBNull.Value == this.GetValue(i);
		}
		public override bool NextResult()
		{
			if (!this.isOpen)
			{
				this.Throw(new MySqlException(Resources.NextResultIsClosed));
			}
			bool flag = this.command.CommandType == CommandType.TableDirect && this.command.EnableCaching && (this.commandBehavior & CommandBehavior.SequentialAccess) == CommandBehavior.Default;
			if (this.resultSet != null)
			{
				this.resultSet.Close();
				if (flag)
				{
					TableCache.AddToCache(this.command.CommandText, this.resultSet);
				}
			}
			if (this.resultSet != null && ((this.commandBehavior & CommandBehavior.SingleResult) > CommandBehavior.Default | flag))
			{
				return false;
			}
			bool result;
			try
			{
				while (true)
				{
					this.resultSet = null;
					if (flag)
					{
						this.resultSet = TableCache.RetrieveFromCache(this.command.CommandText, this.command.CacheAge);
					}
					if (this.resultSet == null)
					{
						this.resultSet = this.driver.NextResult(this.Statement.StatementId, false);
						if (this.resultSet == null)
						{
							break;
						}
						if (this.resultSet.IsOutputParameters && this.command.CommandType == CommandType.StoredProcedure)
						{
							StoredProcedure expr_115 = this.statement as StoredProcedure;
							expr_115.ProcessOutputParameters(this);
							this.resultSet.Close();
							if (!expr_115.ServerProvidingOutputParameters)
							{
								goto Block_14;
							}
							this.resultSet = this.driver.NextResult(this.Statement.StatementId, true);
						}
						this.resultSet.Cached = flag;
					}
					if (this.resultSet.Size == 0)
					{
						this.Command.lastInsertedId = this.resultSet.InsertedId;
						if (this.affectedRows == -1L)
						{
							this.affectedRows = (long)this.resultSet.AffectedRows;
						}
						else
						{
							this.affectedRows += (long)this.resultSet.AffectedRows;
						}
					}
					if (this.resultSet.Size != 0)
					{
						goto Block_17;
					}
				}
				result = false;
				return result;
				Block_14:
				result = false;
				return result;
				Block_17:
				result = true;
			}
			catch (MySqlException ex)
			{
				if (ex.IsFatal)
				{
					this.connection.Abort();
				}
				if (ex.Number == 0)
				{
					throw new MySqlException(Resources.FatalErrorReadingResult, ex);
				}
				if ((this.commandBehavior & CommandBehavior.CloseConnection) != CommandBehavior.Default)
				{
					this.Close();
				}
				throw;
			}
			return result;
		}
		public override bool Read()
		{
			if (!this.isOpen)
			{
				this.Throw(new MySqlException("Invalid attempt to Read when reader is closed."));
			}
			if (this.resultSet == null)
			{
				return false;
			}
			bool result;
			try
			{
				result = this.resultSet.NextRow(this.commandBehavior);
			}
			catch (TimeoutException ex)
			{
				this.connection.HandleTimeoutOrThreadAbort(ex);
				throw;
			}
			catch (ThreadAbortException ex2)
			{
				this.connection.HandleTimeoutOrThreadAbort(ex2);
				throw;
			}
			catch (MySqlException ex3)
			{
				if (ex3.IsFatal)
				{
					this.connection.Abort();
				}
				if (ex3.IsQueryAborted)
				{
					throw;
				}
				throw new MySqlException(Resources.FatalErrorDuringRead, ex3);
			}
			return result;
		}
		private IMySqlValue GetFieldValue(int index, bool checkNull)
		{
			if (index < 0 || index >= this.FieldCount)
			{
				this.Throw(new ArgumentException(Resources.InvalidColumnOrdinal));
			}
			IMySqlValue mySqlValue = this.resultSet[index];
			if (checkNull && mySqlValue.IsNull)
			{
				throw new SqlNullValueException();
			}
			return mySqlValue;
		}
		private void ClearKillFlag()
		{
			MySqlCommand mySqlCommand = new MySqlCommand("SELECT * FROM bogus_table LIMIT 0", this.connection);
			mySqlCommand.InternallyCreated = true;
			try
			{
				mySqlCommand.ExecuteReader();
			}
			catch (MySqlException arg_21_0)
			{
				if (arg_21_0.Number != 1146)
				{
					throw;
				}
			}
		}
		private void ProcessOutputParameters()
		{
			if (!this.driver.SupportsOutputParameters || !this.command.IsPrepared)
			{
				this.AdjustOutputTypes();
			}
			if ((this.commandBehavior & CommandBehavior.SchemaOnly) != CommandBehavior.Default)
			{
				return;
			}
			this.resultSet.NextRow(this.commandBehavior);
			string text = "@_cnet_param_";
			for (int i = 0; i < this.FieldCount; i++)
			{
				string text2 = this.GetName(i);
				if (text2.StartsWith(text))
				{
					text2 = text2.Remove(0, text.Length);
				}
				this.command.Parameters.GetParameterFlexible(text2, true).Value = this.GetValue(i);
			}
		}
		private void AdjustOutputTypes()
		{
			for (int i = 0; i < this.FieldCount; i++)
			{
				string text = this.GetName(i);
				text = text.Remove(0, "_cnet_param_".Length + 1);
				IMySqlValue iMySqlValue = MySqlField.GetIMySqlValue(this.command.Parameters.GetParameterFlexible(text, true).MySqlDbType);
				if (iMySqlValue is MySqlBit)
				{
					MySqlBit mySqlBit = (MySqlBit)iMySqlValue;
					mySqlBit.ReadAsString = true;
					this.resultSet.SetValueObject(i, mySqlBit);
				}
				else
				{
					this.resultSet.SetValueObject(i, iMySqlValue);
				}
			}
		}
		private void Throw(Exception ex)
		{
			if (this.connection != null)
			{
				this.connection.Throw(ex);
			}
			throw ex;
		}
		public new void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}
		internal new void Dispose(bool disposing)
		{
			if (disposing)
			{
				this.Close();
			}
		}
		~MySqlDataReader()
		{
			this.Dispose(false);
		}
		public MySqlGeometry GetMySqlGeometry(int i)
		{
			try
			{
				IMySqlValue fieldValue = this.GetFieldValue(i, false);
				if (fieldValue is MySqlGeometry || fieldValue is MySqlBinary)
				{
					return new MySqlGeometry(MySqlDbType.Geometry, (byte[])fieldValue.Value);
				}
			}
			catch
			{
				this.Throw(new Exception("Can't get MySqlGeometry from value"));
			}
			return new MySqlGeometry(true);
		}
		public MySqlGeometry GetMySqlGeometry(string column)
		{
			return this.GetMySqlGeometry(this.GetOrdinal(column));
		}
		public override DataTable GetSchemaTable()
		{
			if (this.FieldCount == 0)
			{
				return null;
			}
			DataTable dataTable = new DataTable("SchemaTable");
			dataTable.Columns.Add("ColumnName", typeof(string));
			dataTable.Columns.Add("ColumnOrdinal", typeof(int));
			dataTable.Columns.Add("ColumnSize", typeof(int));
			dataTable.Columns.Add("NumericPrecision", typeof(int));
			dataTable.Columns.Add("NumericScale", typeof(int));
			dataTable.Columns.Add("IsUnique", typeof(bool));
			dataTable.Columns.Add("IsKey", typeof(bool));
			dataTable.Columns["IsKey"].AllowDBNull = true;
			dataTable.Columns.Add("BaseCatalogName", typeof(string));
			dataTable.Columns.Add("BaseColumnName", typeof(string));
			dataTable.Columns.Add("BaseSchemaName", typeof(string));
			dataTable.Columns.Add("BaseTableName", typeof(string));
			dataTable.Columns.Add("DataType", typeof(Type));
			dataTable.Columns.Add("AllowDBNull", typeof(bool));
			dataTable.Columns.Add("ProviderType", typeof(int));
			dataTable.Columns.Add("IsAliased", typeof(bool));
			dataTable.Columns.Add("IsExpression", typeof(bool));
			dataTable.Columns.Add("IsIdentity", typeof(bool));
			dataTable.Columns.Add("IsAutoIncrement", typeof(bool));
			dataTable.Columns.Add("IsRowVersion", typeof(bool));
			dataTable.Columns.Add("IsHidden", typeof(bool));
			dataTable.Columns.Add("IsLong", typeof(bool));
			dataTable.Columns.Add("IsReadOnly", typeof(bool));
			int num = 1;
			for (int i = 0; i < this.FieldCount; i++)
			{
				MySqlField mySqlField = this.resultSet.Fields[i];
				DataRow dataRow = dataTable.NewRow();
				dataRow["ColumnName"] = mySqlField.ColumnName;
				dataRow["ColumnOrdinal"] = num++;
				dataRow["ColumnSize"] = (mySqlField.IsTextField ? (mySqlField.ColumnLength / mySqlField.MaxLength) : mySqlField.ColumnLength);
				int precision = (int)mySqlField.Precision;
				int scale = (int)mySqlField.Scale;
				if (precision != -1)
				{
					dataRow["NumericPrecision"] = (short)precision;
				}
				if (scale != -1)
				{
					dataRow["NumericScale"] = (short)scale;
				}
				dataRow["DataType"] = this.GetFieldType(i);
				dataRow["ProviderType"] = (int)mySqlField.Type;
				dataRow["IsLong"] = (mySqlField.IsBlob && mySqlField.ColumnLength > 255);
				dataRow["AllowDBNull"] = mySqlField.AllowsNull;
				dataRow["IsReadOnly"] = false;
				dataRow["IsRowVersion"] = false;
				dataRow["IsUnique"] = false;
				dataRow["IsKey"] = mySqlField.IsPrimaryKey;
				dataRow["IsAutoIncrement"] = mySqlField.IsAutoIncrement;
				dataRow["BaseSchemaName"] = mySqlField.DatabaseName;
				dataRow["BaseCatalogName"] = null;
				dataRow["BaseTableName"] = mySqlField.RealTableName;
				dataRow["BaseColumnName"] = mySqlField.OriginalColumnName;
				dataTable.Rows.Add(dataRow);
			}
			return dataTable;
		}
		public override IEnumerator GetEnumerator()
		{
			return new DbEnumerator(this, (this.commandBehavior & CommandBehavior.CloseConnection) > CommandBehavior.Default);
		}
	}
}
