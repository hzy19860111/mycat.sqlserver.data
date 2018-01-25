using MySql.Data.MySqlClient;
using System;
namespace MySql.Data.Types
{
	internal struct MySqlInt16 : IMySqlValue
	{
		private short mValue;
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
				return MySqlDbType.Int16;
			}
		}
		object IMySqlValue.Value
		{
			get
			{
				return this.mValue;
			}
		}
		public short Value
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
				return typeof(short);
			}
		}
		string IMySqlValue.MySqlTypeName
		{
			get
			{
				return "SMALLINT";
			}
		}
		public MySqlInt16(bool isNull)
		{
			this.isNull = isNull;
			this.mValue = 0;
		}
		public MySqlInt16(short val)
		{
			this.isNull = false;
			this.mValue = val;
		}
		void IMySqlValue.WriteValue(MySqlPacket packet, bool binary, object val, int length)
		{
			int num = (val is int) ? ((int)val) : Convert.ToInt32(val);
			if (binary)
			{
				packet.WriteInteger((long)num, 2);
				return;
			}
			packet.WriteStringNoNull(num.ToString());
		}
		IMySqlValue IMySqlValue.ReadValue(MySqlPacket packet, long length, bool nullVal)
		{
			if (nullVal)
			{
				return new MySqlInt16(true);
			}
			if (length == -1L)
			{
				return new MySqlInt16((short)packet.ReadInteger(2));
			}
			return new MySqlInt16(short.Parse(packet.ReadString(length)));
		}
		void IMySqlValue.SkipValue(MySqlPacket packet)
		{
			packet.Position += 2;
		}
		internal static void SetDSInfo(MySqlSchemaCollection sc)
		{
			MySqlSchemaRow expr_06 = sc.AddRow();
			expr_06["TypeName"] = "SMALLINT";
			expr_06["ProviderDbType"] = MySqlDbType.Int16;
			expr_06["ColumnSize"] = 0;
			expr_06["CreateFormat"] = "SMALLINT";
			expr_06["CreateParameters"] = null;
			expr_06["DataType"] = "System.Int16";
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
