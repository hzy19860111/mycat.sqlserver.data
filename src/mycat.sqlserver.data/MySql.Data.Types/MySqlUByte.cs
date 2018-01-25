using MySql.Data.MySqlClient;
using System;
namespace MySql.Data.Types
{
	internal struct MySqlUByte : IMySqlValue
	{
		private byte mValue;
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
				return MySqlDbType.UByte;
			}
		}
		object IMySqlValue.Value
		{
			get
			{
				return this.mValue;
			}
		}
		public byte Value
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
				return typeof(byte);
			}
		}
		string IMySqlValue.MySqlTypeName
		{
			get
			{
				return "TINYINT";
			}
		}
		public MySqlUByte(bool isNull)
		{
			this.isNull = isNull;
			this.mValue = 0;
		}
		public MySqlUByte(byte val)
		{
			this.isNull = false;
			this.mValue = val;
		}
		void IMySqlValue.WriteValue(MySqlPacket packet, bool binary, object val, int length)
		{
			byte b = (val is byte) ? ((byte)val) : Convert.ToByte(val);
			if (binary)
			{
				packet.WriteByte(b);
				return;
			}
			packet.WriteStringNoNull(b.ToString());
		}
		IMySqlValue IMySqlValue.ReadValue(MySqlPacket packet, long length, bool nullVal)
		{
			if (nullVal)
			{
				return new MySqlUByte(true);
			}
			if (length == -1L)
			{
				return new MySqlUByte(packet.ReadByte());
			}
			return new MySqlUByte(byte.Parse(packet.ReadString(length)));
		}
		void IMySqlValue.SkipValue(MySqlPacket packet)
		{
			packet.ReadByte();
		}
		internal static void SetDSInfo(MySqlSchemaCollection sc)
		{
			MySqlSchemaRow expr_06 = sc.AddRow();
			expr_06["TypeName"] = "TINY INT";
			expr_06["ProviderDbType"] = MySqlDbType.UByte;
			expr_06["ColumnSize"] = 0;
			expr_06["CreateFormat"] = "TINYINT UNSIGNED";
			expr_06["CreateParameters"] = null;
			expr_06["DataType"] = "System.Byte";
			expr_06["IsAutoincrementable"] = true;
			expr_06["IsBestMatch"] = true;
			expr_06["IsCaseSensitive"] = false;
			expr_06["IsFixedLength"] = true;
			expr_06["IsFixedPrecisionScale"] = true;
			expr_06["IsLong"] = false;
			expr_06["IsNullable"] = true;
			expr_06["IsSearchable"] = true;
			expr_06["IsSearchableWithLike"] = false;
			expr_06["IsUnsigned"] = true;
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
