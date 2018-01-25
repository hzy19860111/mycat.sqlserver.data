using MySql.Data.MySqlClient;
using System;
namespace MySql.Data.Types
{
	internal struct MySqlBit : IMySqlValue
	{
		private ulong mValue;
		private bool isNull;
		private bool readAsString;
		public bool ReadAsString
		{
			get
			{
				return this.readAsString;
			}
			set
			{
				this.readAsString = value;
			}
		}
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
				return MySqlDbType.Bit;
			}
		}
		object IMySqlValue.Value
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
				return typeof(ulong);
			}
		}
		string IMySqlValue.MySqlTypeName
		{
			get
			{
				return "BIT";
			}
		}
		public MySqlBit(bool isnull)
		{
			this.mValue = 0uL;
			this.isNull = isnull;
			this.readAsString = false;
		}
		public void WriteValue(MySqlPacket packet, bool binary, object value, int length)
		{
			ulong v = (value is ulong) ? ((ulong)value) : Convert.ToUInt64(value);
			if (binary)
			{
				packet.WriteInteger((long)v, 8);
				return;
			}
			packet.WriteStringNoNull(v.ToString());
		}
		public IMySqlValue ReadValue(MySqlPacket packet, long length, bool isNull)
		{
			this.isNull = isNull;
			if (isNull)
			{
				return this;
			}
			if (length == -1L)
			{
				length = packet.ReadFieldLength();
			}
			if (this.ReadAsString)
			{
				this.mValue = ulong.Parse(packet.ReadString(length));
			}
			else
			{
				this.mValue = packet.ReadBitValue((int)length);
			}
			return this;
		}
		public void SkipValue(MySqlPacket packet)
		{
			int num = (int)packet.ReadFieldLength();
			packet.Position += num;
		}
		internal static void SetDSInfo(MySqlSchemaCollection sc)
		{
			MySqlSchemaRow expr_06 = sc.AddRow();
			expr_06["TypeName"] = "BIT";
			expr_06["ProviderDbType"] = MySqlDbType.Bit;
			expr_06["ColumnSize"] = 64;
			expr_06["CreateFormat"] = "BIT";
			expr_06["CreateParameters"] = DBNull.Value;
			expr_06["DataType"] = typeof(ulong).ToString();
			expr_06["IsAutoincrementable"] = false;
			expr_06["IsBestMatch"] = true;
			expr_06["IsCaseSensitive"] = false;
			expr_06["IsFixedLength"] = false;
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
			expr_06["LiteralPrefix"] = DBNull.Value;
			expr_06["LiteralSuffix"] = DBNull.Value;
			expr_06["NativeDataType"] = DBNull.Value;
		}
	}
}
