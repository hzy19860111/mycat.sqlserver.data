using System;
using System.Collections.Generic;
namespace MySql.Data.MySqlClient
{
	public class MySqlSchemaRow
	{
		private Dictionary<int, object> data;
		internal MySqlSchemaCollection Collection
		{
			get;
			private set;
		}
		internal object this[string s]
		{
			get
			{
				return this.GetValueForName(s);
			}
			set
			{
				this.SetValueForName(s, value);
			}
		}
		internal object this[int i]
		{
			get
			{
				int key = this.Collection.LogicalMappings[i];
				if (!this.data.ContainsKey(key))
				{
					this.data[key] = null;
				}
				return this.data[key];
			}
			set
			{
				this.data[this.Collection.LogicalMappings[i]] = value;
			}
		}
		public MySqlSchemaRow(MySqlSchemaCollection c)
		{
			this.Collection = c;
			this.InitMetadata();
		}
		internal void InitMetadata()
		{
			this.data = new Dictionary<int, object>();
		}
		private void SetValueForName(string colName, object value)
		{
			int i = this.Collection.Mapping[colName];
			this[i] = value;
		}
		private object GetValueForName(string colName)
		{
			int num = this.Collection.Mapping[colName];
			if (!this.data.ContainsKey(num))
			{
				this.data[num] = null;
			}
			return this[num];
		}
		internal void CopyRow(MySqlSchemaRow row)
		{
			if (this.Collection.Columns.Count != row.Collection.Columns.Count)
			{
				throw new InvalidOperationException("column count doesn't match");
			}
			for (int i = 0; i < this.Collection.Columns.Count; i++)
			{
				row[i] = this[i];
			}
		}
	}
}
