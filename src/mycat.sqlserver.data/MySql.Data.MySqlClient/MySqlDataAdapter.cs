using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
namespace MySql.Data.MySqlClient
{
	[Designer("MySql.Data.MySqlClient.Design.MySqlDataAdapterDesigner,MySqlClient.Design"), DesignerCategory("Code"), ToolboxBitmap(typeof(MySqlDataAdapter), "MySqlClient.resources.dataadapter.bmp")]
	public sealed class MySqlDataAdapter : DbDataAdapter, IDbDataAdapter, IDataAdapter, ICloneable
	{
		private bool loadingDefaults;
		private int updateBatchSize;
		private List<IDbCommand> commandBatch;
		[method: CompilerGenerated]
		[CompilerGenerated]
		public event MySqlRowUpdatingEventHandler RowUpdating;
		[method: CompilerGenerated]
		[CompilerGenerated]
		public event MySqlRowUpdatedEventHandler RowUpdated;
		[Description("Used during Update for deleted rows in Dataset.")]
		public new MySqlCommand DeleteCommand
		{
			get
			{
				return (MySqlCommand)base.DeleteCommand;
			}
			set
			{
				base.DeleteCommand = value;
			}
		}
		[Description("Used during Update for new rows in Dataset.")]
		public new MySqlCommand InsertCommand
		{
			get
			{
				return (MySqlCommand)base.InsertCommand;
			}
			set
			{
				base.InsertCommand = value;
			}
		}
		[Category("Fill"), Description("Used during Fill/FillSchema")]
		public new MySqlCommand SelectCommand
		{
			get
			{
				return (MySqlCommand)base.SelectCommand;
			}
			set
			{
				base.SelectCommand = value;
			}
		}
		[Description("Used during Update for modified rows in Dataset.")]
		public new MySqlCommand UpdateCommand
		{
			get
			{
				return (MySqlCommand)base.UpdateCommand;
			}
			set
			{
				base.UpdateCommand = value;
			}
		}
		internal bool LoadDefaults
		{
			get
			{
				return this.loadingDefaults;
			}
			set
			{
				this.loadingDefaults = value;
			}
		}
		public override int UpdateBatchSize
		{
			get
			{
				return this.updateBatchSize;
			}
			set
			{
				this.updateBatchSize = value;
			}
		}
		public MySqlDataAdapter()
		{
			this.loadingDefaults = true;
			this.updateBatchSize = 1;
		}
		public MySqlDataAdapter(MySqlCommand selectCommand) : this()
		{
			this.SelectCommand = selectCommand;
		}
		public MySqlDataAdapter(string selectCommandText, MySqlConnection connection) : this()
		{
			this.SelectCommand = new MySqlCommand(selectCommandText, connection);
		}
		public MySqlDataAdapter(string selectCommandText, string selectConnString) : this()
		{
			this.SelectCommand = new MySqlCommand(selectCommandText, new MySqlConnection(selectConnString));
		}
		private void OpenConnectionIfClosed(DataRowState state, List<MySqlConnection> openedConnections)
		{
			MySqlCommand mySqlCommand;
			if (state != DataRowState.Added)
			{
				if (state != DataRowState.Deleted)
				{
					if (state != DataRowState.Modified)
					{
						return;
					}
					mySqlCommand = this.UpdateCommand;
				}
				else
				{
					mySqlCommand = this.DeleteCommand;
				}
			}
			else
			{
				mySqlCommand = this.InsertCommand;
			}
			if (mySqlCommand != null && mySqlCommand.Connection != null && mySqlCommand.Connection.connectionState == ConnectionState.Closed)
			{
				mySqlCommand.Connection.Open();
				openedConnections.Add(mySqlCommand.Connection);
			}
		}
		protected override int Update(DataRow[] dataRows, DataTableMapping tableMapping)
		{
			List<MySqlConnection> list = new List<MySqlConnection>();
			int i;
			try
			{
				for (i = 0; i < dataRows.Length; i++)
				{
					DataRow dataRow = dataRows[i];
					this.OpenConnectionIfClosed(dataRow.RowState, list);
				}
				i = base.Update(dataRows, tableMapping);
			}
			finally
			{
				using (List<MySqlConnection>.Enumerator enumerator = list.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						enumerator.Current.Close();
					}
				}
			}
			return i;
		}
		protected override void InitializeBatching()
		{
			this.commandBatch = new List<IDbCommand>();
		}
		protected override int AddToBatch(IDbCommand command)
		{
			MySqlCommand mySqlCommand = (MySqlCommand)command;
			if (mySqlCommand.BatchableCommandText == null)
			{
				mySqlCommand.GetCommandTextForBatching();
			}
			IDbCommand item = (IDbCommand)((ICloneable)command).Clone();
			this.commandBatch.Add(item);
			return this.commandBatch.Count - 1;
		}
		protected override int ExecuteBatch()
		{
			int num = 0;
			int i = 0;
			while (i < this.commandBatch.Count)
			{
				MySqlCommand mySqlCommand = (MySqlCommand)this.commandBatch[i++];
				int j = i;
				while (j < this.commandBatch.Count)
				{
					MySqlCommand mySqlCommand2 = (MySqlCommand)this.commandBatch[j];
					if (mySqlCommand2.BatchableCommandText == null || mySqlCommand2.CommandText != mySqlCommand.CommandText)
					{
						break;
					}
					mySqlCommand.AddToBatch(mySqlCommand2);
					j++;
					i++;
				}
				num += mySqlCommand.ExecuteNonQuery();
			}
			return num;
		}
		protected override void ClearBatch()
		{
			if (this.commandBatch.Count > 0)
			{
				MySqlCommand mySqlCommand = (MySqlCommand)this.commandBatch[0];
				if (mySqlCommand.Batch != null)
				{
					mySqlCommand.Batch.Clear();
				}
			}
			this.commandBatch.Clear();
		}
		protected override void TerminateBatching()
		{
			this.ClearBatch();
			this.commandBatch = null;
		}
		protected override IDataParameter GetBatchedParameter(int commandIdentifier, int parameterIndex)
		{
			return (IDataParameter)this.commandBatch[commandIdentifier].Parameters[parameterIndex];
		}
		protected override RowUpdatedEventArgs CreateRowUpdatedEvent(DataRow dataRow, IDbCommand command, StatementType statementType, DataTableMapping tableMapping)
		{
			return new MySqlRowUpdatedEventArgs(dataRow, command, statementType, tableMapping);
		}
		protected override RowUpdatingEventArgs CreateRowUpdatingEvent(DataRow dataRow, IDbCommand command, StatementType statementType, DataTableMapping tableMapping)
		{
			return new MySqlRowUpdatingEventArgs(dataRow, command, statementType, tableMapping);
		}
		protected override void OnRowUpdating(RowUpdatingEventArgs value)
		{
			if (this.RowUpdating != null)
			{
				this.RowUpdating(this, value as MySqlRowUpdatingEventArgs);
			}
		}
		protected override void OnRowUpdated(RowUpdatedEventArgs value)
		{
			if (this.RowUpdated != null)
			{
				this.RowUpdated(this, value as MySqlRowUpdatedEventArgs);
			}
		}
		public Task<int> FillAsync(DataSet dataSet)
		{
			return this.FillAsync(dataSet, CancellationToken.None);
		}
		public Task<int> FillAsync(DataSet dataSet, CancellationToken cancellationToken)
		{
			TaskCompletionSource<int> taskCompletionSource = new TaskCompletionSource<int>();
			if (cancellationToken == CancellationToken.None || !cancellationToken.IsCancellationRequested)
			{
				try
				{
					int result = base.Fill(dataSet);
					taskCompletionSource.SetResult(result);
					goto IL_3E;
				}
				catch (Exception exception)
				{
					taskCompletionSource.SetException(exception);
					goto IL_3E;
				}
			}
			taskCompletionSource.SetCanceled();
			IL_3E:
			return taskCompletionSource.Task;
		}
		public Task<int> FillAsync(DataTable dataTable)
		{
			return this.FillAsync(dataTable, CancellationToken.None);
		}
		public Task<int> FillAsync(DataTable dataTable, CancellationToken cancellationToken)
		{
			TaskCompletionSource<int> taskCompletionSource = new TaskCompletionSource<int>();
			if (cancellationToken == CancellationToken.None || !cancellationToken.IsCancellationRequested)
			{
				try
				{
					int result = base.Fill(dataTable);
					taskCompletionSource.SetResult(result);
					goto IL_3E;
				}
				catch (Exception exception)
				{
					taskCompletionSource.SetException(exception);
					goto IL_3E;
				}
			}
			taskCompletionSource.SetCanceled();
			IL_3E:
			return taskCompletionSource.Task;
		}
		public Task<int> FillAsync(DataSet dataSet, string srcTable)
		{
			return this.FillAsync(dataSet, srcTable, CancellationToken.None);
		}
		public Task<int> FillAsync(DataSet dataSet, string srcTable, CancellationToken cancellationToken)
		{
			TaskCompletionSource<int> taskCompletionSource = new TaskCompletionSource<int>();
			if (cancellationToken == CancellationToken.None || !cancellationToken.IsCancellationRequested)
			{
				try
				{
					int result = base.Fill(dataSet, srcTable);
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
		public Task<int> FillAsync(DataTable dataTable, IDataReader dataReader)
		{
			return this.FillAsync(dataTable, dataReader, CancellationToken.None);
		}
		public Task<int> FillAsync(DataTable dataTable, IDataReader dataReader, CancellationToken cancellationToken)
		{
			TaskCompletionSource<int> taskCompletionSource = new TaskCompletionSource<int>();
			if (cancellationToken == CancellationToken.None || !cancellationToken.IsCancellationRequested)
			{
				try
				{
					int result = base.Fill(dataTable, dataReader);
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
		public Task<int> FillAsync(DataTable dataTable, IDbCommand command, CommandBehavior behavior)
		{
			return this.FillAsync(dataTable, command, behavior, CancellationToken.None);
		}
		public Task<int> FillAsync(DataTable dataTable, IDbCommand command, CommandBehavior behavior, CancellationToken cancellationToken)
		{
			TaskCompletionSource<int> taskCompletionSource = new TaskCompletionSource<int>();
			if (cancellationToken == CancellationToken.None || !cancellationToken.IsCancellationRequested)
			{
				try
				{
					int result = base.Fill(dataTable, command, behavior);
					taskCompletionSource.SetResult(result);
					goto IL_41;
				}
				catch (Exception exception)
				{
					taskCompletionSource.SetException(exception);
					goto IL_41;
				}
			}
			taskCompletionSource.SetCanceled();
			IL_41:
			return taskCompletionSource.Task;
		}
		public Task<int> FillAsync(int startRecord, int maxRecords, params DataTable[] dataTables)
		{
			return this.FillAsync(startRecord, maxRecords, CancellationToken.None, dataTables);
		}
		public Task<int> FillAsync(int startRecord, int maxRecords, CancellationToken cancellationToken, params DataTable[] dataTables)
		{
			TaskCompletionSource<int> taskCompletionSource = new TaskCompletionSource<int>();
			if (cancellationToken == CancellationToken.None || !cancellationToken.IsCancellationRequested)
			{
				try
				{
					int result = base.Fill(startRecord, maxRecords, dataTables);
					taskCompletionSource.SetResult(result);
					goto IL_41;
				}
				catch (Exception exception)
				{
					taskCompletionSource.SetException(exception);
					goto IL_41;
				}
			}
			taskCompletionSource.SetCanceled();
			IL_41:
			return taskCompletionSource.Task;
		}
		public Task<int> FillAsync(DataSet dataSet, int startRecord, int maxRecords, string srcTable)
		{
			return this.FillAsync(dataSet, startRecord, maxRecords, srcTable, CancellationToken.None);
		}
		public Task<int> FillAsync(DataSet dataSet, int startRecord, int maxRecords, string srcTable, CancellationToken cancellationToken)
		{
			TaskCompletionSource<int> taskCompletionSource = new TaskCompletionSource<int>();
			if (cancellationToken == CancellationToken.None || !cancellationToken.IsCancellationRequested)
			{
				try
				{
					int result = base.Fill(dataSet, startRecord, maxRecords, srcTable);
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
		public Task<int> FillAsync(DataSet dataSet, string srcTable, IDataReader dataReader, int startRecord, int maxRecords)
		{
			return this.FillAsync(dataSet, srcTable, dataReader, startRecord, maxRecords, CancellationToken.None);
		}
		public Task<int> FillAsync(DataSet dataSet, string srcTable, IDataReader dataReader, int startRecord, int maxRecords, CancellationToken cancellationToken)
		{
			TaskCompletionSource<int> taskCompletionSource = new TaskCompletionSource<int>();
			if (cancellationToken == CancellationToken.None || !cancellationToken.IsCancellationRequested)
			{
				try
				{
					int result = base.Fill(dataSet, srcTable, dataReader, startRecord, maxRecords);
					taskCompletionSource.SetResult(result);
					goto IL_45;
				}
				catch (Exception exception)
				{
					taskCompletionSource.SetException(exception);
					goto IL_45;
				}
			}
			taskCompletionSource.SetCanceled();
			IL_45:
			return taskCompletionSource.Task;
		}
		public Task<int> FillAsync(DataTable[] dataTables, int startRecord, int maxRecords, IDbCommand command, CommandBehavior behavior)
		{
			return this.FillAsync(dataTables, startRecord, maxRecords, command, behavior, CancellationToken.None);
		}
		public Task<int> FillAsync(DataTable[] dataTables, int startRecord, int maxRecords, IDbCommand command, CommandBehavior behavior, CancellationToken cancellationToken)
		{
			TaskCompletionSource<int> taskCompletionSource = new TaskCompletionSource<int>();
			if (cancellationToken == CancellationToken.None || !cancellationToken.IsCancellationRequested)
			{
				try
				{
					int result = base.Fill(dataTables, startRecord, maxRecords, command, behavior);
					taskCompletionSource.SetResult(result);
					goto IL_45;
				}
				catch (Exception exception)
				{
					taskCompletionSource.SetException(exception);
					goto IL_45;
				}
			}
			taskCompletionSource.SetCanceled();
			IL_45:
			return taskCompletionSource.Task;
		}
		public Task<int> FillAsync(DataSet dataSet, int startRecord, int maxRecords, string srcTable, IDbCommand command, CommandBehavior behavior)
		{
			return this.FillAsync(dataSet, startRecord, maxRecords, srcTable, command, behavior, CancellationToken.None);
		}
		public Task<int> FillAsync(DataSet dataSet, int startRecord, int maxRecords, string srcTable, IDbCommand command, CommandBehavior behavior, CancellationToken cancellationToken)
		{
			TaskCompletionSource<int> taskCompletionSource = new TaskCompletionSource<int>();
			if (cancellationToken == CancellationToken.None || !cancellationToken.IsCancellationRequested)
			{
				try
				{
					int result = base.Fill(dataSet, startRecord, maxRecords, srcTable, command, behavior);
					taskCompletionSource.SetResult(result);
					goto IL_47;
				}
				catch (Exception exception)
				{
					taskCompletionSource.SetException(exception);
					goto IL_47;
				}
			}
			taskCompletionSource.SetCanceled();
			IL_47:
			return taskCompletionSource.Task;
		}
		public Task<DataTable[]> FillSchemaAsync(DataSet dataSet, SchemaType schemaType)
		{
			return this.FillSchemaAsync(dataSet, schemaType, CancellationToken.None);
		}
		public Task<DataTable[]> FillSchemaAsync(DataSet dataSet, SchemaType schemaType, CancellationToken cancellationToken)
		{
			TaskCompletionSource<DataTable[]> taskCompletionSource = new TaskCompletionSource<DataTable[]>();
			if (cancellationToken == CancellationToken.None || !cancellationToken.IsCancellationRequested)
			{
				try
				{
					DataTable[] result = base.FillSchema(dataSet, schemaType);
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
		public Task<DataTable[]> FillSchemaAsync(DataSet dataSet, SchemaType schemaType, string srcTable)
		{
			return this.FillSchemaAsync(dataSet, schemaType, srcTable, CancellationToken.None);
		}
		public Task<DataTable[]> FillSchemaAsync(DataSet dataSet, SchemaType schemaType, string srcTable, CancellationToken cancellationToken)
		{
			TaskCompletionSource<DataTable[]> taskCompletionSource = new TaskCompletionSource<DataTable[]>();
			if (cancellationToken == CancellationToken.None || !cancellationToken.IsCancellationRequested)
			{
				try
				{
					DataTable[] result = base.FillSchema(dataSet, schemaType, srcTable);
					taskCompletionSource.SetResult(result);
					goto IL_41;
				}
				catch (Exception exception)
				{
					taskCompletionSource.SetException(exception);
					goto IL_41;
				}
			}
			taskCompletionSource.SetCanceled();
			IL_41:
			return taskCompletionSource.Task;
		}
		public Task<DataTable[]> FillSchemaAsync(DataSet dataSet, SchemaType schemaType, string srcTable, IDataReader dataReader)
		{
			return this.FillSchemaAsync(dataSet, schemaType, srcTable, dataReader, CancellationToken.None);
		}
		public Task<DataTable[]> FillSchemaAsync(DataSet dataSet, SchemaType schemaType, string srcTable, IDataReader dataReader, CancellationToken cancellationToken)
		{
			TaskCompletionSource<DataTable[]> taskCompletionSource = new TaskCompletionSource<DataTable[]>();
			if (cancellationToken == CancellationToken.None || !cancellationToken.IsCancellationRequested)
			{
				try
				{
					DataTable[] result = base.FillSchema(dataSet, schemaType, srcTable, dataReader);
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
		public Task<DataTable[]> FillSchemaAsync(DataSet dataSet, SchemaType schemaType, IDbCommand command, string srcTable, CommandBehavior behavior)
		{
			return this.FillSchemaAsync(dataSet, schemaType, command, srcTable, behavior, CancellationToken.None);
		}
		public Task<DataTable[]> FillSchemaAsync(DataSet dataSet, SchemaType schemaType, IDbCommand command, string srcTable, CommandBehavior behavior, CancellationToken cancellationToken)
		{
			TaskCompletionSource<DataTable[]> taskCompletionSource = new TaskCompletionSource<DataTable[]>();
			if (cancellationToken == CancellationToken.None || !cancellationToken.IsCancellationRequested)
			{
				try
				{
					DataTable[] result = base.FillSchema(dataSet, schemaType, command, srcTable, behavior);
					taskCompletionSource.SetResult(result);
					goto IL_45;
				}
				catch (Exception exception)
				{
					taskCompletionSource.SetException(exception);
					goto IL_45;
				}
			}
			taskCompletionSource.SetCanceled();
			IL_45:
			return taskCompletionSource.Task;
		}
		public Task<DataTable> FillSchemaAsync(DataTable dataTable, SchemaType schemaType)
		{
			return this.FillSchemaAsync(dataTable, schemaType, CancellationToken.None);
		}
		public Task<DataTable> FillSchemaAsync(DataTable dataTable, SchemaType schemaType, CancellationToken cancellationToken)
		{
			TaskCompletionSource<DataTable> taskCompletionSource = new TaskCompletionSource<DataTable>();
			if (cancellationToken == CancellationToken.None || !cancellationToken.IsCancellationRequested)
			{
				try
				{
					DataTable result = base.FillSchema(dataTable, schemaType);
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
		public Task<DataTable> FillSchemaAsync(DataTable dataTable, SchemaType schemaType, IDataReader dataReader)
		{
			return this.FillSchemaAsync(dataTable, schemaType, dataReader, CancellationToken.None);
		}
		public Task<DataTable> FillSchemaAsync(DataTable dataTable, SchemaType schemaType, IDataReader dataReader, CancellationToken cancellationToken)
		{
			TaskCompletionSource<DataTable> taskCompletionSource = new TaskCompletionSource<DataTable>();
			if (cancellationToken == CancellationToken.None || !cancellationToken.IsCancellationRequested)
			{
				try
				{
					DataTable result = base.FillSchema(dataTable, schemaType, dataReader);
					taskCompletionSource.SetResult(result);
					goto IL_41;
				}
				catch (Exception exception)
				{
					taskCompletionSource.SetException(exception);
					goto IL_41;
				}
			}
			taskCompletionSource.SetCanceled();
			IL_41:
			return taskCompletionSource.Task;
		}
		public Task<DataTable> FillSchemaAsync(DataTable dataTable, SchemaType schemaType, IDbCommand command, CommandBehavior behavior)
		{
			return this.FillSchemaAsync(dataTable, schemaType, command, behavior, CancellationToken.None);
		}
		public Task<DataTable> FillSchemaAsync(DataTable dataTable, SchemaType schemaType, IDbCommand command, CommandBehavior behavior, CancellationToken cancellationToken)
		{
			TaskCompletionSource<DataTable> taskCompletionSource = new TaskCompletionSource<DataTable>();
			if (cancellationToken == CancellationToken.None || !cancellationToken.IsCancellationRequested)
			{
				try
				{
					DataTable result = base.FillSchema(dataTable, schemaType, command, behavior);
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
		public Task<int> UpdateAsync(DataRow[] dataRows)
		{
			return this.UpdateAsync(dataRows, CancellationToken.None);
		}
		public Task<int> UpdateAsync(DataRow[] dataRows, CancellationToken cancellationToken)
		{
			TaskCompletionSource<int> taskCompletionSource = new TaskCompletionSource<int>();
			if (cancellationToken == CancellationToken.None || !cancellationToken.IsCancellationRequested)
			{
				try
				{
					int result = base.Update(dataRows);
					taskCompletionSource.SetResult(result);
					goto IL_3E;
				}
				catch (Exception exception)
				{
					taskCompletionSource.SetException(exception);
					goto IL_3E;
				}
			}
			taskCompletionSource.SetCanceled();
			IL_3E:
			return taskCompletionSource.Task;
		}
		public Task<int> UpdateAsync(DataSet dataSet)
		{
			return this.UpdateAsync(dataSet, CancellationToken.None);
		}
		public Task<int> UpdateAsync(DataSet dataSet, CancellationToken cancellationToken)
		{
			TaskCompletionSource<int> taskCompletionSource = new TaskCompletionSource<int>();
			if (cancellationToken == CancellationToken.None || !cancellationToken.IsCancellationRequested)
			{
				try
				{
					int result = base.Update(dataSet);
					taskCompletionSource.SetResult(result);
					goto IL_3E;
				}
				catch (Exception exception)
				{
					taskCompletionSource.SetException(exception);
					goto IL_3E;
				}
			}
			taskCompletionSource.SetCanceled();
			IL_3E:
			return taskCompletionSource.Task;
		}
		public Task<int> UpdateAsync(DataTable dataTable)
		{
			return this.UpdateAsync(dataTable, CancellationToken.None);
		}
		public Task<int> UpdateAsync(DataTable dataTable, CancellationToken cancellationToken)
		{
			TaskCompletionSource<int> taskCompletionSource = new TaskCompletionSource<int>();
			if (cancellationToken == CancellationToken.None || !cancellationToken.IsCancellationRequested)
			{
				try
				{
					int result = base.Update(dataTable);
					taskCompletionSource.SetResult(result);
					goto IL_3E;
				}
				catch (Exception exception)
				{
					taskCompletionSource.SetException(exception);
					goto IL_3E;
				}
			}
			taskCompletionSource.SetCanceled();
			IL_3E:
			return taskCompletionSource.Task;
		}
		public Task<int> UpdateAsync(DataRow[] dataRows, DataTableMapping tableMapping)
		{
			return this.UpdateAsync(dataRows, tableMapping, CancellationToken.None);
		}
		public Task<int> UpdateAsync(DataRow[] dataRows, DataTableMapping tableMapping, CancellationToken cancellationToken)
		{
			TaskCompletionSource<int> taskCompletionSource = new TaskCompletionSource<int>();
			if (cancellationToken == CancellationToken.None || !cancellationToken.IsCancellationRequested)
			{
				try
				{
					int result = base.Update(dataRows, tableMapping);
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
		public Task<int> UpdateAsync(DataSet dataSet, string srcTable)
		{
			return this.UpdateAsync(dataSet, srcTable, CancellationToken.None);
		}
		public Task<int> UpdateAsync(DataSet dataSet, string srcTable, CancellationToken cancellationToken)
		{
			TaskCompletionSource<int> taskCompletionSource = new TaskCompletionSource<int>();
			if (cancellationToken == CancellationToken.None || !cancellationToken.IsCancellationRequested)
			{
				try
				{
					int result = base.Update(dataSet, srcTable);
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
