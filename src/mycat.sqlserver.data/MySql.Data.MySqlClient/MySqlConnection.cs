using MySql.Data.Common;
using MySql.Data.MySqlClient.Properties;
using MySql.Data.MySqlClient.Replication;
using System;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Drawing;
using System.Drawing.Design;
using System.Runtime.CompilerServices;
using System.Security;
using System.Security.Permissions;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
namespace MySql.Data.MySqlClient
{
	[DesignerCategory("Code"), ToolboxItem(true), ToolboxBitmap(typeof(MySqlConnection), "MySqlClient.resources.connection.bmp")]
	public sealed class MySqlConnection : DbConnection, IDisposable, ICloneable
	{
		internal ConnectionState connectionState;
		internal Driver driver;
		internal bool hasBeenOpen;
		private SchemaProvider schemaProvider;
		private ProcedureCache procedureCache;
		private bool isInUse;
		private PerformanceMonitor perfMonitor;
		private ExceptionInterceptor exceptionInterceptor;
		internal CommandInterceptor commandInterceptor;
		private bool isKillQueryConnection;
		private string database;
		private int commandTimeout;
		private static Cache<string, MySqlConnectionStringBuilder> connectionStringCache = new Cache<string, MySqlConnectionStringBuilder>(0, 25);
		[method: CompilerGenerated]
		[CompilerGenerated]
		public event MySqlInfoMessageEventHandler InfoMessage;
		internal PerformanceMonitor PerfMonitor
		{
			get
			{
				return this.perfMonitor;
			}
		}
		internal ProcedureCache ProcedureCache
		{
			get
			{
				return this.procedureCache;
			}
		}
		internal MySqlConnectionStringBuilder Settings
		{
			get;
			private set;
		}
		internal MySqlDataReader Reader
		{
			get
			{
				if (this.driver == null)
				{
					return null;
				}
				return this.driver.reader;
			}
			set
			{
				this.driver.reader = value;
				this.isInUse = (this.driver.reader != null);
			}
		}
		internal bool SoftClosed
		{
			get
			{
				return this.State == ConnectionState.Closed && this.driver != null && this.driver.CurrentTransaction != null;
			}
		}
		internal bool IsInUse
		{
			get
			{
				return this.isInUse;
			}
			set
			{
				this.isInUse = value;
			}
		}
		[Browsable(false)]
		public int ServerThread
		{
			get
			{
				return this.driver.ThreadID;
			}
		}
		[Browsable(true)]
		public override string DataSource
		{
			get
			{
				return this.Settings.Server;
			}
		}
		[Browsable(true)]
		public override int ConnectionTimeout
		{
			get
			{
				return (int)this.Settings.ConnectionTimeout;
			}
		}
		[Browsable(true)]
		public override string Database
		{
			get
			{
				return this.database;
			}
		}
		[Browsable(false)]
		public bool UseCompression
		{
			get
			{
				return this.Settings.UseCompression;
			}
		}
		[Browsable(false)]
		public override ConnectionState State
		{
			get
			{
				return this.connectionState;
			}
		}
		[Browsable(false)]
		public override string ServerVersion
		{
			get
			{
				return this.driver.Version.ToString();
			}
		}
		[Browsable(true), Category("Data"), Description("Information used to connect to a DataSource, such as 'Server=xxx;UserId=yyy;Password=zzz;Database=dbdb'."), Editor("MySql.Data.MySqlClient.Design.ConnectionStringTypeEditor,MySqlClient.Design", typeof(UITypeEditor))]
		public override string ConnectionString
		{
			get
			{
				return this.Settings.GetConnectionString(!this.hasBeenOpen || this.Settings.PersistSecurityInfo);
			}
			set
			{
				if (this.State != ConnectionState.Closed)
				{
					this.Throw(new MySqlException("Not allowed to change the 'ConnectionString' property while the connection (state=" + this.State + ")."));
				}
				Cache<string, MySqlConnectionStringBuilder> obj = MySqlConnection.connectionStringCache;
				MySqlConnectionStringBuilder mySqlConnectionStringBuilder;
				lock (obj)
				{
					if (value == null)
					{
						mySqlConnectionStringBuilder = new MySqlConnectionStringBuilder();
					}
					else
					{
						mySqlConnectionStringBuilder = MySqlConnection.connectionStringCache[value];
						if (mySqlConnectionStringBuilder == null)
						{
							mySqlConnectionStringBuilder = new MySqlConnectionStringBuilder(value);
							MySqlConnection.connectionStringCache.Add(value, mySqlConnectionStringBuilder);
						}
					}
				}
				this.Settings = mySqlConnectionStringBuilder;
				if (this.Settings.Database != null && this.Settings.Database.Length > 0)
				{
					this.database = this.Settings.Database;
				}
				if (this.driver != null)
				{
					this.driver.Settings = mySqlConnectionStringBuilder;
				}
			}
		}
		protected override DbProviderFactory DbProviderFactory
		{
			get
			{
				return MySqlClientFactory.Instance;
			}
		}
		public bool IsPasswordExpired
		{
			get
			{
				return this.driver.IsPasswordExpired;
			}
		}
		public MySqlConnection()
		{
			this.Settings = new MySqlConnectionStringBuilder();
			this.database = string.Empty;
		}
		public MySqlConnection(string connectionString) : this()
		{
			this.ConnectionString = connectionString;
		}
		~MySqlConnection()
		{
			this.Dispose(false);
		}
		internal void OnInfoMessage(MySqlInfoMessageEventArgs args)
		{
			if (this.InfoMessage != null)
			{
				this.InfoMessage(this, args);
			}
		}
		private void AssertPermissions()
		{
			if (this.Settings.IncludeSecurityAsserts)
			{
				PermissionSet expr_13 = new PermissionSet(PermissionState.None);
				expr_13.AddPermission(new MySqlClientPermission(this.ConnectionString));
				expr_13.Demand();
				MySqlSecurityPermission.CreatePermissionSet(true).Assert();
			}
		}
		public override void EnlistTransaction(Transaction transaction)
		{
			if (transaction == null)
			{
				return;
			}
			if (this.driver.CurrentTransaction != null)
			{
				if (this.driver.CurrentTransaction.BaseTransaction == transaction)
				{
					return;
				}
				this.Throw(new MySqlException("Already enlisted"));
			}
			Driver driverInTransaction = DriverTransactionManager.GetDriverInTransaction(transaction);
			if (driverInTransaction != null)
			{
				if (driverInTransaction.IsInActiveUse)
				{
					this.Throw(new NotSupportedException(Resources.MultipleConnectionsInTransactionNotSupported));
				}
				string arg_7B_0 = driverInTransaction.Settings.ConnectionString;
				string connectionString = this.Settings.ConnectionString;
				if (string.Compare(arg_7B_0, connectionString, true) != 0)
				{
					this.Throw(new NotSupportedException(Resources.MultipleConnectionsInTransactionNotSupported));
				}
				this.CloseFully();
				this.driver = driverInTransaction;
			}
			if (this.driver.CurrentTransaction == null)
			{
				MySqlPromotableTransaction mySqlPromotableTransaction = new MySqlPromotableTransaction(this, transaction);
				if (!transaction.EnlistPromotableSinglePhase(mySqlPromotableTransaction))
				{
					this.Throw(new NotSupportedException(Resources.DistributedTxnNotSupported));
				}
				this.driver.CurrentTransaction = mySqlPromotableTransaction;
				DriverTransactionManager.SetDriverInTransaction(this.driver);
				this.driver.IsInActiveUse = true;
			}
		}
		public new MySqlTransaction BeginTransaction()
		{
			return this.BeginTransaction(System.Data.IsolationLevel.RepeatableRead);
		}
		public new MySqlTransaction BeginTransaction(System.Data.IsolationLevel iso)
		{
			if (this.State != ConnectionState.Open)
			{
				this.Throw(new InvalidOperationException(Resources.ConnectionNotOpen));
			}
			if (this.driver.HasStatus(ServerStatusFlags.InTransaction))
			{
				this.Throw(new InvalidOperationException(Resources.NoNestedTransactions));
			}
			MySqlTransaction result = new MySqlTransaction(this, iso);
			MySqlCommand mySqlCommand = new MySqlCommand("", this);
			mySqlCommand.CommandText = "SET SESSION TRANSACTION ISOLATION LEVEL ";
			if (iso <= System.Data.IsolationLevel.ReadCommitted)
			{
				if (iso != System.Data.IsolationLevel.Chaos)
				{
					if (iso != System.Data.IsolationLevel.ReadUncommitted)
					{
						if (iso == System.Data.IsolationLevel.ReadCommitted)
						{
							MySqlCommand expr_99 = mySqlCommand;
							expr_99.CommandText += "READ COMMITTED";
						}
					}
					else
					{
						MySqlCommand expr_B1 = mySqlCommand;
						expr_B1.CommandText += "READ UNCOMMITTED";
					}
				}
				else
				{
					this.Throw(new NotSupportedException(Resources.ChaosNotSupported));
				}
			}
			else if (iso != System.Data.IsolationLevel.RepeatableRead)
			{
				if (iso != System.Data.IsolationLevel.Serializable)
				{
					if (iso == System.Data.IsolationLevel.Snapshot)
					{
						this.Throw(new NotSupportedException(Resources.SnapshotNotSupported));
					}
				}
				else
				{
					MySqlCommand expr_E1 = mySqlCommand;
					expr_E1.CommandText += "SERIALIZABLE";
				}
			}
			else
			{
				MySqlCommand expr_C9 = mySqlCommand;
				expr_C9.CommandText += "REPEATABLE READ";
			}
			mySqlCommand.ExecuteNonQuery();
			mySqlCommand.CommandText = "BEGIN";
			mySqlCommand.ExecuteNonQuery();
			return result;
		}
		public override void ChangeDatabase(string databaseName)
		{
			if (databaseName == null || databaseName.Trim().Length == 0)
			{
				this.Throw(new ArgumentException(Resources.ParameterIsInvalid, "databaseName"));
			}
			if (this.State != ConnectionState.Open)
			{
				this.Throw(new InvalidOperationException(Resources.ConnectionNotOpen));
			}
			Driver obj = this.driver;
			lock (obj)
			{
				if (Transaction.Current != null && Transaction.Current.TransactionInformation.Status == TransactionStatus.Aborted)
				{
					this.Throw(new TransactionAbortedException());
				}
				using (new CommandTimer(this, (int)this.Settings.DefaultCommandTimeout))
				{
					this.driver.SetDatabase(databaseName);
				}
			}
			this.database = databaseName;
		}
		internal void SetState(ConnectionState newConnectionState, bool broadcast)
		{
			if (newConnectionState == this.connectionState && !broadcast)
			{
				return;
			}
			ConnectionState originalState = this.connectionState;
			this.connectionState = newConnectionState;
			if (broadcast)
			{
				this.OnStateChange(new StateChangeEventArgs(originalState, this.connectionState));
			}
		}
		public bool Ping()
		{
			if (this.Reader != null)
			{
				this.Throw(new MySqlException(Resources.DataReaderOpen));
			}
			if (this.driver != null && this.driver.Ping())
			{
				return true;
			}
			this.driver = null;
			this.SetState(ConnectionState.Closed, true);
			return false;
		}
		public override void Open()
		{
			if (this.State == ConnectionState.Open)
			{
				this.Throw(new InvalidOperationException(Resources.ConnectionAlreadyOpen));
			}
			this.exceptionInterceptor = new ExceptionInterceptor(this);
			this.commandInterceptor = new CommandInterceptor(this);
			this.SetState(ConnectionState.Connecting, true);
			this.AssertPermissions();
			if (this.Settings.AutoEnlist && Transaction.Current != null)
			{
				this.driver = DriverTransactionManager.GetDriverInTransaction(Transaction.Current);
				if (this.driver != null && (this.driver.IsInActiveUse || !this.driver.Settings.EquivalentTo(this.Settings)))
				{
					this.Throw(new NotSupportedException(Resources.MultipleConnectionsInTransactionNotSupported));
				}
			}
			try
			{
				MySqlConnectionStringBuilder settings = this.Settings;
				if (ReplicationManager.IsReplicationGroup(this.Settings.Server))
				{
					if (this.driver == null)
					{
						ReplicationManager.GetNewConnection(this.Settings.Server, false, this);
					}
					else
					{
						settings = this.driver.Settings;
					}
				}
				if (this.Settings.Pooling)
				{
					MySqlPool pool = MySqlPoolManager.GetPool(settings);
					if (this.driver == null || !this.driver.IsOpen)
					{
						this.driver = pool.GetConnection();
					}
					this.procedureCache = pool.ProcedureCache;
				}
				else
				{
					if (this.driver == null || !this.driver.IsOpen)
					{
						this.driver = Driver.Create(settings);
					}
					this.procedureCache = new ProcedureCache((int)this.Settings.ProcedureCacheSize);
				}
			}
			catch (Exception)
			{
				this.SetState(ConnectionState.Closed, true);
				throw;
			}
			if (this.driver.Settings.UseOldSyntax)
			{
				MySqlTrace.LogWarning(this.ServerThread, "You are using old syntax that will be removed in future versions");
			}
			this.SetState(ConnectionState.Open, false);
			this.driver.Configure(this);
			if ((!this.driver.SupportsPasswordExpiration || !this.driver.IsPasswordExpired) && this.Settings.Database != null && this.Settings.Database != string.Empty)
			{
				this.ChangeDatabase(this.Settings.Database);
			}
			this.schemaProvider = new ISSchemaProvider(this);
			this.perfMonitor = new PerformanceMonitor(this);
			if (Transaction.Current != null && this.Settings.AutoEnlist)
			{
				this.EnlistTransaction(Transaction.Current);
			}
			this.hasBeenOpen = true;
			this.SetState(ConnectionState.Open, true);
		}
		public new MySqlCommand CreateCommand()
		{
			return new MySqlCommand
			{
				Connection = this
			};
		}
		public object Clone()
		{
			MySqlConnection mySqlConnection = new MySqlConnection();
			string connectionString = this.Settings.ConnectionString;
			if (connectionString != null)
			{
				mySqlConnection.ConnectionString = connectionString;
			}
			return mySqlConnection;
		}
		internal void Abort()
		{
			try
			{
				this.driver.Close();
			}
			catch (Exception ex)
			{
				MySqlTrace.LogWarning(this.ServerThread, "Error occurred aborting the connection. Exception was: " + ex.Message);
			}
			finally
			{
				this.isInUse = false;
			}
			this.SetState(ConnectionState.Closed, true);
		}
		internal void CloseFully()
		{
			if (this.Settings.Pooling && this.driver.IsOpen)
			{
				if (this.driver.HasStatus(ServerStatusFlags.InTransaction))
				{
					new MySqlTransaction(this, System.Data.IsolationLevel.Unspecified).Rollback();
				}
				MySqlPoolManager.ReleaseConnection(this.driver);
			}
			else
			{
				this.driver.Close();
			}
			this.driver = null;
		}
		public override void Close()
		{
			if (this.driver != null)
			{
				this.driver.IsPasswordExpired = false;
			}
			if (this.State == ConnectionState.Closed)
			{
				return;
			}
			if (this.Reader != null)
			{
				this.Reader.Close();
			}
			if (this.driver != null)
			{
				if (this.driver.CurrentTransaction == null)
				{
					this.CloseFully();
				}
				else
				{
					this.driver.IsInActiveUse = false;
				}
			}
			this.SetState(ConnectionState.Closed, true);
		}
		internal string CurrentDatabase()
		{
			if (this.Database != null && this.Database.Length > 0)
			{
				return this.Database;
			}
			return new MySqlCommand("SELECT database()", this).ExecuteScalar().ToString();
		}
		internal void HandleTimeoutOrThreadAbort(Exception ex)
		{
			bool isFatal = false;
			if (this.isKillQueryConnection)
			{
				this.Abort();
				if (!(ex is TimeoutException))
				{
					return;
				}
				this.Throw(new MySqlException(Resources.Timeout, true, ex));
			}
			try
			{
				this.CancelQuery(5);
				this.driver.ResetTimeout(5000);
				if (this.Reader != null)
				{
					this.Reader.Close();
					this.Reader = null;
				}
			}
			catch (Exception ex2)
			{
				MySqlTrace.LogWarning(this.ServerThread, "Could not kill query,  aborting connection. Exception was " + ex2.Message);
				this.Abort();
				isFatal = true;
			}
			if (ex is TimeoutException)
			{
				this.Throw(new MySqlException(Resources.Timeout, isFatal, ex));
			}
		}
		public void CancelQuery(int timeout)
		{
			using (MySqlConnection mySqlConnection = new MySqlConnection(new MySqlConnectionStringBuilder(this.Settings.ConnectionString)
			{
				Pooling = false,
				AutoEnlist = false,
				ConnectionTimeout = (uint)timeout
			}.ConnectionString))
			{
				mySqlConnection.isKillQueryConnection = true;
				mySqlConnection.Open();
				new MySqlCommand("KILL QUERY " + this.ServerThread, mySqlConnection)
				{
					CommandTimeout = timeout
				}.ExecuteNonQuery();
			}
		}
		internal bool SetCommandTimeout(int value)
		{
			if (!this.hasBeenOpen)
			{
				return false;
			}
			if (this.commandTimeout != 0)
			{
				return false;
			}
			if (this.driver == null)
			{
				return false;
			}
			this.commandTimeout = value;
			this.driver.ResetTimeout(this.commandTimeout * 1000);
			return true;
		}
		internal void ClearCommandTimeout()
		{
			if (!this.hasBeenOpen)
			{
				return;
			}
			this.commandTimeout = 0;
			if (this.driver != null)
			{
				this.driver.ResetTimeout(0);
			}
		}
		public MySqlSchemaCollection GetSchemaCollection(string collectionName, string[] restrictionValues)
		{
			if (collectionName == null)
			{
				collectionName = SchemaProvider.MetaCollection;
			}
			string[] restrictions = this.schemaProvider.CleanRestrictions(restrictionValues);
			return this.schemaProvider.GetSchema(collectionName, restrictions);
		}
		public static void ClearPool(MySqlConnection connection)
		{
			MySqlPoolManager.ClearPool(connection.Settings);
		}
		public static void ClearAllPools()
		{
			MySqlPoolManager.ClearAllPools();
		}
		internal void Throw(Exception ex)
		{
			if (this.exceptionInterceptor == null)
			{
				throw ex;
			}
			this.exceptionInterceptor.Throw(ex);
		}
		public new void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}
		public Task<MySqlTransaction> BeginTransactionAsync()
		{
			return this.BeginTransactionAsync(System.Data.IsolationLevel.RepeatableRead, CancellationToken.None);
		}
		public Task<MySqlTransaction> BeginTransactionAsync(CancellationToken cancellationToken)
		{
			return this.BeginTransactionAsync(System.Data.IsolationLevel.RepeatableRead, cancellationToken);
		}
		public Task<MySqlTransaction> BeginTransactionAsync(System.Data.IsolationLevel iso)
		{
			return this.BeginTransactionAsync(iso, CancellationToken.None);
		}
		public Task<MySqlTransaction> BeginTransactionAsync(System.Data.IsolationLevel iso, CancellationToken cancellationToken)
		{
			TaskCompletionSource<MySqlTransaction> taskCompletionSource = new TaskCompletionSource<MySqlTransaction>();
			if (cancellationToken == CancellationToken.None || !cancellationToken.IsCancellationRequested)
			{
				try
				{
					MySqlTransaction result = this.BeginTransaction(iso);
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
		public Task ChangeDataBaseAsync(string databaseName)
		{
			return this.ChangeDataBaseAsync(databaseName, CancellationToken.None);
		}
		public Task ChangeDataBaseAsync(string databaseName, CancellationToken cancellationToken)
		{
			TaskCompletionSource<bool> taskCompletionSource = new TaskCompletionSource<bool>();
			if (cancellationToken == CancellationToken.None || !cancellationToken.IsCancellationRequested)
			{
				try
				{
					this.ChangeDatabase(databaseName);
					taskCompletionSource.SetResult(true);
				}
				catch (Exception exception)
				{
					taskCompletionSource.SetException(exception);
				}
			}
			return taskCompletionSource.Task;
		}
		public Task CloseAsync()
		{
			return this.CloseAsync(CancellationToken.None);
		}
		public Task CloseAsync(CancellationToken cancellationToken)
		{
			TaskCompletionSource<bool> taskCompletionSource = new TaskCompletionSource<bool>();
			if (cancellationToken == CancellationToken.None || !cancellationToken.IsCancellationRequested)
			{
				try
				{
					this.Close();
					taskCompletionSource.SetResult(true);
					goto IL_3C;
				}
				catch (Exception exception)
				{
					taskCompletionSource.SetException(exception);
					goto IL_3C;
				}
			}
			taskCompletionSource.SetCanceled();
			IL_3C:
			return taskCompletionSource.Task;
		}
		public Task ClearPoolAsync(MySqlConnection connection)
		{
			return this.ClearPoolAsync(connection, CancellationToken.None);
		}
		public Task ClearPoolAsync(MySqlConnection connection, CancellationToken cancellationToken)
		{
			TaskCompletionSource<bool> taskCompletionSource = new TaskCompletionSource<bool>();
			if (cancellationToken == CancellationToken.None || !cancellationToken.IsCancellationRequested)
			{
				try
				{
					MySqlConnection.ClearPool(connection);
					taskCompletionSource.SetResult(true);
					goto IL_3C;
				}
				catch (Exception exception)
				{
					taskCompletionSource.SetException(exception);
					goto IL_3C;
				}
			}
			taskCompletionSource.SetCanceled();
			IL_3C:
			return taskCompletionSource.Task;
		}
		public Task ClearAllPoolsAsync()
		{
			return this.ClearAllPoolsAsync(CancellationToken.None);
		}
		public Task ClearAllPoolsAsync(CancellationToken cancellationToken)
		{
			TaskCompletionSource<bool> taskCompletionSource = new TaskCompletionSource<bool>();
			if (cancellationToken == CancellationToken.None || !cancellationToken.IsCancellationRequested)
			{
				try
				{
					MySqlConnection.ClearAllPools();
					taskCompletionSource.SetResult(true);
					goto IL_3B;
				}
				catch (Exception exception)
				{
					taskCompletionSource.SetException(exception);
					goto IL_3B;
				}
			}
			taskCompletionSource.SetCanceled();
			IL_3B:
			return taskCompletionSource.Task;
		}
		public Task<MySqlSchemaCollection> GetSchemaCollectionAsync(string collectionName, string[] restrictionValues)
		{
			return this.GetSchemaCollectionAsync(collectionName, restrictionValues, CancellationToken.None);
		}
		public Task<MySqlSchemaCollection> GetSchemaCollectionAsync(string collectionName, string[] restrictionValues, CancellationToken cancellationToken)
		{
			TaskCompletionSource<MySqlSchemaCollection> taskCompletionSource = new TaskCompletionSource<MySqlSchemaCollection>();
			if (cancellationToken == CancellationToken.None || !cancellationToken.IsCancellationRequested)
			{
				try
				{
					MySqlSchemaCollection schemaCollection = this.GetSchemaCollection(collectionName, restrictionValues);
					taskCompletionSource.SetResult(schemaCollection);
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
		public override DataTable GetSchema()
		{
			return this.GetSchema(null);
		}
		public override DataTable GetSchema(string collectionName)
		{
			if (collectionName == null)
			{
				collectionName = SchemaProvider.MetaCollection;
			}
			return this.GetSchema(collectionName, null);
		}
		public override DataTable GetSchema(string collectionName, string[] restrictionValues)
		{
			if (collectionName == null)
			{
				collectionName = SchemaProvider.MetaCollection;
			}
			string[] restrictions = this.schemaProvider.CleanRestrictions(restrictionValues);
			return this.schemaProvider.GetSchema(collectionName, restrictions).AsDataTable();
		}
		protected override DbTransaction BeginDbTransaction(System.Data.IsolationLevel isolationLevel)
		{
			if (isolationLevel == System.Data.IsolationLevel.Unspecified)
			{
				return this.BeginTransaction();
			}
			return this.BeginTransaction(isolationLevel);
		}
		protected override DbCommand CreateDbCommand()
		{
			return this.CreateCommand();
		}
		protected override void Dispose(bool disposing)
		{
			if (this.State == ConnectionState.Open)
			{
				this.Close();
			}
			base.Dispose(disposing);
		}
	}
}
