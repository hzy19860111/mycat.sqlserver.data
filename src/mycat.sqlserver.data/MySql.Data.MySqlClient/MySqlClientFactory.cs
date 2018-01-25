using System;
using System.Data.Common;
using System.Reflection;
using System.Security.Permissions;
namespace MySql.Data.MySqlClient
{
	[ReflectionPermission(SecurityAction.Assert, MemberAccess = true)]
	public sealed class MySqlClientFactory : DbProviderFactory, IServiceProvider
	{
		public static MySqlClientFactory Instance = new MySqlClientFactory();
		private Type dbServicesType;
		private FieldInfo mySqlDbProviderServicesInstance;
		public override bool CanCreateDataSourceEnumerator
		{
			get
			{
				return false;
			}
		}
		private Type DbServicesType
		{
			get
			{
				if (this.dbServicesType == null)
				{
					this.dbServicesType = Type.GetType("System.Data.Common.DbProviderServices, System.Data.Entity, \r\n                        Version=3.5.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", false);
				}
				return this.dbServicesType;
			}
		}
		private FieldInfo MySqlDbProviderServicesInstance
		{
			get
			{
				if (this.mySqlDbProviderServicesInstance == null)
				{
					string text = Assembly.GetExecutingAssembly().FullName;
					string arg = text.Replace("MySql.Data", "MySql.Data.Entity");
					string arg2 = text.Replace("MySql.Data", "MySql.Data.Entity.EF5");
					text = string.Format("MySql.Data.MySqlClient.MySqlProviderServices, {0}", arg2);
					Type type = Type.GetType(text, false);
					if (type == null)
					{
						text = string.Format("MySql.Data.MySqlClient.MySqlProviderServices, {0}", arg);
						type = Type.GetType(text, false);
						if (type == null)
						{
							throw new DllNotFoundException(text);
						}
					}
					this.mySqlDbProviderServicesInstance = type.GetField("Instance", BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic);
				}
				return this.mySqlDbProviderServicesInstance;
			}
		}
		public override DbCommandBuilder CreateCommandBuilder()
		{
			return new MySqlCommandBuilder();
		}
		public override DbCommand CreateCommand()
		{
			return new MySqlCommand();
		}
		public override DbConnection CreateConnection()
		{
			return new MySqlConnection();
		}
		public override DbDataAdapter CreateDataAdapter()
		{
			return new MySqlDataAdapter();
		}
		public override DbParameter CreateParameter()
		{
			return new MySqlParameter();
		}
		public override DbConnectionStringBuilder CreateConnectionStringBuilder()
		{
			return new MySqlConnectionStringBuilder();
		}
		object IServiceProvider.GetService(Type serviceType)
		{
			if (serviceType != this.DbServicesType)
			{
				return null;
			}
			if (this.MySqlDbProviderServicesInstance == null)
			{
				return null;
			}
			return this.MySqlDbProviderServicesInstance.GetValue(null);
		}
	}
}
