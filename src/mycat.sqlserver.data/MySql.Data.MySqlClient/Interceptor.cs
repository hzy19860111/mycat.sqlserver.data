using System;
namespace MySql.Data.MySqlClient
{
	internal abstract class Interceptor
	{
		protected MySqlConnection connection;
		protected void LoadInterceptors(string interceptorList)
		{
			if (string.IsNullOrEmpty(interceptorList))
			{
				return;
			}
			string[] array = interceptorList.Split(new char[]
			{
				'|'
			});
			for (int i = 0; i < array.Length; i++)
			{
				string text = array[i];
				if (!string.IsNullOrEmpty(text))
				{
					object o = Activator.CreateInstance(Type.GetType(this.ResolveType(text)));
					this.AddInterceptor(o);
				}
			}
		}
		protected abstract void AddInterceptor(object o);
		protected virtual string ResolveType(string nameOrType)
		{
			return nameOrType;
		}
	}
}
