using MySql.Data.MySqlClient.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Timers;
namespace MySql.Data.MySqlClient.Replication
{
	public abstract class ReplicationServerGroup
	{
		protected List<ReplicationServer> servers = new List<ReplicationServer>();
		public string Name
		{
			get;
			protected set;
		}
		public int RetryTime
		{
			get;
			protected set;
		}
		protected IList<ReplicationServer> Servers
		{
			get;
			private set;
		}
		public ReplicationServerGroup(string name, int retryTime)
		{
			this.Servers = this.servers;
			this.Name = name;
			this.RetryTime = retryTime;
		}
		protected internal ReplicationServer AddServer(string name, bool isMaster, string connectionString)
		{
			ReplicationServer replicationServer = new ReplicationServer(name, isMaster, connectionString);
			this.servers.Add(replicationServer);
			return replicationServer;
		}
		protected internal void RemoveServer(string name)
		{
			ReplicationServer server = this.GetServer(name);
			if (server == null)
			{
				throw new MySqlException(string.Format(Resources.ReplicationServerNotFound, name));
			}
			this.servers.Remove(server);
		}
		protected internal ReplicationServer GetServer(string name)
		{
			foreach (ReplicationServer current in this.servers)
			{
				if (string.Compare(name, current.Name, StringComparison.OrdinalIgnoreCase) == 0)
				{
					return current;
				}
			}
			return null;
		}
		protected internal abstract ReplicationServer GetServer(bool isMaster);
		protected internal virtual ReplicationServer GetServer(bool isMaster, MySqlConnectionStringBuilder settings)
		{
			return this.GetServer(isMaster);
		}
		protected internal virtual void HandleFailover(ReplicationServer server)
		{
			BackgroundWorker expr_19 = new BackgroundWorker();
			expr_19.DoWork += delegate(object sender, DoWorkEventArgs e)
			{
				bool isRunning = false;
				ReplicationServer server1 = e.Argument as ReplicationServer;
				Timer timer = new Timer((double)this.RetryTime * 1000.0);
				ElapsedEventHandler elapsedEventHandler = delegate(object sender1, ElapsedEventArgs e1)
				{
					if (isRunning)
					{
						return;
					}
					try
					{
						isRunning = true;
						using (MySqlConnection mySqlConnection = new MySqlConnection(server.ConnectionString))
						{
							mySqlConnection.Open();
							server1.IsAvailable = true;
							timer.Stop();
						}
					}
					catch
					{
						MySqlTrace.LogWarning(0, string.Format(Resources.Replication_ConnectionAttemptFailed, server1.Name));
					}
					finally
					{
						isRunning = false;
					}
				};
				timer.Elapsed += elapsedEventHandler;
				timer.Start();
				elapsedEventHandler(sender, null);
			};
			expr_19.RunWorkerAsync(server);
		}
		protected internal virtual void HandleFailover(ReplicationServer server, Exception exception)
		{
			this.HandleFailover(server);
		}
	}
}
