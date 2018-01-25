using System;
using System.Data;
using System.Data.Common;
namespace MySql.Data.MySqlClient
{
	public sealed class MySqlTransaction : DbTransaction, IDisposable
	{
		private IsolationLevel level;
		private MySqlConnection conn;
		private bool open;
		private bool disposed;
		protected override DbConnection DbConnection
		{
			get
			{
				return this.conn;
			}
		}
		public new MySqlConnection Connection
		{
			get
			{
				return this.conn;
			}
		}
		public override IsolationLevel IsolationLevel
		{
			get
			{
				return this.level;
			}
		}
		internal MySqlTransaction(MySqlConnection c, IsolationLevel il)
		{
			this.conn = c;
			this.level = il;
			this.open = true;
		}
		~MySqlTransaction()
		{
			this.Dispose(false);
		}
		public new void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}
		protected override void Dispose(bool disposing)
		{
			if (this.disposed)
			{
				return;
			}
			base.Dispose(disposing);
			if (disposing && ((this.conn != null && this.conn.State == ConnectionState.Open) || this.conn.SoftClosed) && this.open)
			{
				this.Rollback();
			}
			this.disposed = true;
		}
		public override void Commit()
		{
			if (this.conn == null || (this.conn.State != ConnectionState.Open && !this.conn.SoftClosed))
			{
				throw new InvalidOperationException("Connection must be valid and open to commit transaction");
			}
			if (!this.open)
			{
				throw new InvalidOperationException("Transaction has already been committed or is not pending");
			}
			new MySqlCommand("COMMIT", this.conn).ExecuteNonQuery();
			this.open = false;
		}
		public override void Rollback()
		{
			if (this.conn == null || (this.conn.State != ConnectionState.Open && !this.conn.SoftClosed))
			{
				throw new InvalidOperationException("Connection must be valid and open to rollback transaction");
			}
			if (!this.open)
			{
				throw new InvalidOperationException("Transaction has already been rolled back or is not pending");
			}
			new MySqlCommand("ROLLBACK", this.conn).ExecuteNonQuery();
			this.open = false;
		}
	}
}
