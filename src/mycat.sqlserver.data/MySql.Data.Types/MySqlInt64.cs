using MySql.Data.MySqlClient;
using System;
namespace MySql.Data.Types
{
	internal struct MySqlInt64 : IMySqlValue
	{
		private long mValue;
		private bool isNull;
		public bool IsNull
		{
			get
			{
				return this.isNull;
			}
		}
		MySqlDbType IMySqlValue.MySqlDbType
		{
			get
			{
				return MySqlDbType.Int64;
			}
		}
		object IMySqlValue.Value
		{
			get
			{
				return this.mValue;
			}
		}
		public long Value
		{
			get
			{
				return this.mValue;
			}
		}
		Type IMySqlValue.SystemType
		{
			get
			{
				return typeof(long);
			}
		}
		string IMySqlValue.MySqlTypeName
		{
			get
			{
				return "BIGINT";
			}
		}
		public MySqlInt64(bool isNull)
		{
			this.isNull = isNull;
			this.mValue = 0L;
		}
		public MySqlInt64(long val)
		{
			this.isNull = false;
			this.mValue = val;
		}
		void IMySqlValue.WriteValue(MySqlPacket packet, bool binary, object val, int length)
		{
			long v = (val is long) ? ((long)val) : Convert.ToInt64(val);
			if (binary)
			{
				packet.WriteInteger(v, 8);
				return;
			}
			packet.WriteStringNoNull(v.ToString());
		}
		IMySqlValue IMySqlValue.ReadValue(MySqlPacket packet, long length, bool nullVal)
		{
			if (nullVal)
			{
				return new MySqlInt64(true);
			}
			if (length == -1L)
			{
				return new MySqlInt64((long)packet.ReadULong(8));
			}
			return new MySqlInt64(long.Parse(packet.ReadString(length)));
		}
		void IMySqlValue.SkipValue(MySqlPacket packet)
		{
			packet.Position += 8;
		}
		internal static void SetDSInfo(MySqlSchemaCollection sc)
		{
			MySqlSchemaRow expr_06 = sc.AddRow();
			expr_06["TypeName"] = "BIGINT";
			expr_06["ProviderDbType"] = MySqlDbType.Int64;
			expr_06["ColumnSize"] = 0;
			expr_06["CreateFormat"] = "BIGINT";
			expr_06["CreateParameters"] = null;
			expr_06["DataType"] = "System.Int64";
			expr_06["IsAutoincrementable"] = true;
			expr_06["IsBestMatch"] = true;
			expr_06["IsCaseSensitive"] = false;
			expr_06["IsFixedLength"] = true;
			expr_06["IsFixedPrecisionScale"] = true;
			expr_06["IsLong"] = false;
			expr_06["IsNullable"] = true;
			expr_06["IsSearchable"] = true;
			expr_06["IsSearchableWithLike"] = false;
			expr_06["IsUnsigned"] = false;
			expr_06["MaximumScale"] = 0;
			expr_06["MinimumScale"] = 0;
			expr_06["IsConcurrencyType"] = DBNull.Value;
			expr_06["IsLiteralSupported"] = false;
			expr_06["LiteralPrefix"] = null;
			expr_06["LiteralSuffix"] = null;
			expr_06["NativeDataType"] = null;
		}
	}
}
