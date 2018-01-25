using MySql.Data.MySqlClient.Properties;
using System;
using System.Collections.Generic;
namespace MySql.Data.MySqlClient
{
	internal sealed class ExceptionInterceptor : Interceptor
	{
		private List<BaseExceptionInterceptor> interceptors = new List<BaseExceptionInterceptor>();
		public ExceptionInterceptor(MySqlConnection connection)
		{
			this.connection = connection;
			base.LoadInterceptors(connection.Settings.ExceptionInterceptors);
			this.interceptors.Add(new StandardExceptionInterceptor());
		}
		protected override void AddInterceptor(object o)
		{
			if (o == null)
			{
				throw new ArgumentException(string.Format("Unable to instantiate ExceptionInterceptor", new object[0]));
			}
			if (!(o is BaseExceptionInterceptor))
			{
				throw new InvalidOperationException(string.Format(Resources.TypeIsNotExceptionInterceptor, o.GetType()));
			}
			(o as BaseExceptionInterceptor).Init(this.connection);
			this.interceptors.Insert(0, (BaseExceptionInterceptor)o);
		}
		public void Throw(Exception exception)
		{
			Exception ex = exception;
			using (List<BaseExceptionInterceptor>.Enumerator enumerator = this.interceptors.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					ex = enumerator.Current.InterceptException(ex);
				}
			}
			throw ex;
		}
		protected override string ResolveType(string nameOrType)
		{
			if (MySqlConfiguration.Settings != null && MySqlConfiguration.Settings.ExceptionInterceptors != null)
			{
				foreach (InterceptorConfigurationElement current in MySqlConfiguration.Settings.ExceptionInterceptors)
				{
					if (string.Compare(current.Name, nameOrType, true) == 0)
					{
						return current.Type;
					}
				}
			}
			return base.ResolveType(nameOrType);
		}
	}
}
