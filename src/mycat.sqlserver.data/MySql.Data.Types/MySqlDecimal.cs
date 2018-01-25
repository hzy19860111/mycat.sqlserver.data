using MySql.Data.MySqlClient;
using System;
using System.Globalization;
namespace MySql.Data.Types
{
	public struct MySqlDecimal : IMySqlValue
	{
		private byte precision;
		private byte scale;
		private string mValue;
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
				return MySqlDbType.Decimal;
			}
		}
		public byte Precision
		{
			get
			{
				return this.precision;
			}
			set
			{
				this.precision = value;
			}
		}
		public byte Scale
		{
			get
			{
				return this.scale;
			}
			set
			{
				this.scale = value;
			}
		}
		object IMySqlValue.Value
		{
			get
			{
				return this.Value;
			}
		}
		public decimal Value
		{
			get
			{
				return Convert.ToDecimal(this.mValue, CultureInfo.InvariantCulture);
			}
		}
		Type IMySqlValue.SystemType
		{
			get
			{
				return typeof(decimal);
			}
		}
		string IMySqlValue.MySqlTypeName
		{
			get
			{
				return "DECIMAL";
			}
		}
		internal MySqlDecimal(bool isNull)
		{
			this.isNull = isNull;
			this.mValue = null;
			this.precision = (this.scale = 0);
		}
		internal MySqlDecimal(string val)
		{
			this.isNull = false;
			this.precision = (this.scale = 0);
			this.mValue = val;
		}
		public double ToDouble()
		{
			return double.Parse(this.mValue);
		}
		public override string ToString()
		{
			return this.mValue;
		}
		void IMySqlValue.WriteValue(MySqlPacket packet, bool binary, object val, int length)
		{
			string text = ((val is decimal) ? ((decimal)val) : Convert.ToDecimal(val)).ToString(CultureInfo.InvariantCulture);
			if (binary)
			{
				packet.WriteLenString(text);
				return;
			}
			packet.WriteStringNoNull(text);
		}
		IMySqlValue IMySqlValue.ReadValue(MySqlPacket packet, long length, bool nullVal)
		{
			if (nullVal)
			{
				return new MySqlDecimal(true);
			}
			string val = string.Empty;
			if (length == -1L)
			{
				val = packet.ReadLenString();
			}
			else
			{
				val = packet.ReadString(length);
			}
			return new MySqlDecimal(val);
		}
		void IMySqlValue.SkipValue(MySqlPacket packet)
		{
			int num = (int)packet.ReadFieldLength();
			packet.Position += num;
		}
		internal static void SetDSInfo(MySqlSchemaCollection sc)
		{
			MySqlSchemaRow expr_06 = sc.AddRow();
			expr_06["TypeName"] = "DECIMAL";
			expr_06["ProviderDbType"] = MySqlDbType.NewDecimal;
			expr_06["ColumnSize"] = 0;
			expr_06["CreateFormat"] = "DECIMAL({0},{1})";
			expr_06["CreateParameters"] = "precision,scale";
			expr_06["DataType"] = "System.Decimal";
			expr_06["IsAutoincrementable"] = false;
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
