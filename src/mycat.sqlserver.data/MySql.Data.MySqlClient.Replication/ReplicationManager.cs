using MySql.Data.MySqlClient.Properties;
using System;
using System.Collections.Generic;
namespace MySql.Data.MySqlClient.Replication
{
	internal static class ReplicationManager
	{
		private static List<ReplicationServerGroup> groups;
		private static object thisLock;
		internal static IList<ReplicationServerGroup> Groups
		{
			get;
			private set;
		}
		static ReplicationManager()
		{
			ReplicationManager.groups = new List<ReplicationServerGroup>();
			ReplicationManager.thisLock = new object();
			ReplicationManager.Groups = ReplicationManager.groups;
			if (MySqlConfiguration.Settings == null)
			{
				return;
			}
			foreach (ReplicationServerGroupConfigurationElement current in MySqlConfiguration.Settings.Replication.ServerGroups)
			{
				ReplicationServerGroup replicationServerGroup = ReplicationManager.AddGroup(current.Name, current.GroupType, current.RetryTime);
				foreach (ReplicationServerConfigurationElement current2 in current.Servers)
				{
					replicationServerGroup.AddServer(current2.Name, current2.IsMaster, current2.ConnectionString);
				}
			}
		}
		internal static ReplicationServerGroup AddGroup(string name, int retryTime)
		{
			return ReplicationManager.AddGroup(name, null, retryTime);
		}
		internal static ReplicationServerGroup AddGroup(string name, string groupType, int retryTime)
		{
			if (string.IsNullOrEmpty(groupType))
			{
				groupType = "MySql.Data.MySqlClient.Replication.ReplicationRoundRobinServerGroup";
			}
			ReplicationServerGroup replicationServerGroup = (ReplicationServerGroup)Activator.CreateInstance(Type.GetType(groupType), new object[]
			{
				name,
				retryTime
			});
			ReplicationManager.groups.Add(replicationServerGroup);
			return replicationServerGroup;
		}
		internal static ReplicationServer GetServer(string groupName, bool isMaster)
		{
			return ReplicationManager.GetGroup(groupName).GetServer(isMaster);
		}
		internal static ReplicationServerGroup GetGroup(string groupName)
		{
			ReplicationServerGroup replicationServerGroup = null;
			foreach (ReplicationServerGroup current in ReplicationManager.groups)
			{
				if (string.Compare(current.Name, groupName, StringComparison.OrdinalIgnoreCase) == 0)
				{
					replicationServerGroup = current;
					break;
				}
			}
			if (replicationServerGroup == null)
			{
				throw new MySqlException(string.Format(Resources.ReplicationGroupNotFound, groupName));
			}
			return replicationServerGroup;
		}
		internal static bool IsReplicationGroup(string groupName)
		{
			using (List<ReplicationServerGroup>.Enumerator enumerator = ReplicationManager.groups.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (string.Compare(enumerator.Current.Name, groupName, StringComparison.OrdinalIgnoreCase) == 0)
					{
						return true;
					}
				}
			}
			return false;
		}
		internal static void GetNewConnection(string groupName, bool master, MySqlConnection connection)
		{
			while (true)
			{
				object obj = ReplicationManager.thisLock;
				lock (obj)
				{
					if (ReplicationManager.IsReplicationGroup(groupName))
					{
						ReplicationServerGroup group = ReplicationManager.GetGroup(groupName);
						ReplicationServer server = group.GetServer(master, connection.Settings);
						if (server == null)
						{
							throw new MySqlException(Resources.Replication_NoAvailableServer);
						}
						try
						{
							bool flag2 = false;
							if (connection.driver == null || !connection.driver.IsOpen)
							{
								flag2 = true;
							}
							else if (!new MySqlConnectionStringBuilder(server.ConnectionString).Equals(connection.driver.Settings))
							{
								flag2 = true;
							}
							if (flag2)
							{
								Driver driver = Driver.Create(new MySqlConnectionStringBuilder(server.ConnectionString));
								connection.driver = driver;
							}
						}
						catch (MySqlException ex)
						{
							connection.driver = null;
							server.IsAvailable = false;
							MySqlTrace.LogError(ex.Number, ex.ToString());
							if (ex.Number == 1042)
							{
								group.HandleFailover(server, ex);
								continue;
							}
							throw;
						}
					}
				}
				break;
			}
		}
	}
}
