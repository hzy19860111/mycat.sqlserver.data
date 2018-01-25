using MySql.Data.MySqlClient;
using System;
using System.Globalization;
namespace MySql.Data.Types
{
	internal struct MySqlByte : IMySqlValue
	{
		private sbyte mValue;
		private bool isNull;
		private bool treatAsBool;
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
				return MySqlDbType.Byte;
			}
		}
		object IMySqlValue.Value
		{
			get
			{
				if (this.TreatAsBoolean)
				{
					return Convert.ToBoolean(this.mValue);
				}
				return this.mValue;
			}
		}
		public sbyte Value
		{
			get
			{
				return this.mValue;
			}
			set
			{
				this.mValue = value;
			}
		}
		Type IMySqlValue.SystemType
		{
			get
			{
				if (this.TreatAsBoolean)
				{
					return typeof(bool);
				}
				return typeof(sbyte);
			}
		}
		string IMySqlValue.MySqlTypeName
		{
			get
			{
				return "TINYINT";
			}
		}
		internal bool TreatAsBoolean
		{
			get
			{
				return this.treatAsBool;
			}
			set
			{
				this.treatAsBool = value;
			}
		}
		public MySqlByte(bool isNull)
		{
			this.isNull = isNull;
			this.mValue = 0;
			this.treatAsBool = false;
		}
		public MySqlByte(sbyte val)
		{
			this.isNull = false;
			this.mValue = val;
			this.treatAsBool = false;
		}
		void IMySqlValue.WriteValue(MySqlPacket packet, bool binary, object val, int length)
		{
			sbyte b = (val is sbyte) ? ((sbyte)val) : Convert.ToSByte(val);
			if (binary)
			{
				packet.WriteByte((byte)b);
				return;
			}
			packet.WriteStringNoNull(b.ToString());
		}
		IMySqlValue IMySqlValue.ReadValue(MySqlPacket packet, long length, bool nullVal)
		{
			if (nullVal)
			{
				return new MySqlByte(true);
			}
			if (length == -1L)
			{
				return new MySqlByte((sbyte)packet.ReadByte());
			}
			string s = packet.ReadString(length);
			return new MySqlByte(sbyte.Parse(s, NumberStyles.Any, CultureInfo.InvariantCulture))
			{
				TreatAsBoolean = this.TreatAsBoolean
			};
		}
		void IMySqlValue.SkipValue(MySqlPacket packet)
		{
			packet.ReadByte();
		}
		internal static void SetDSInfo(MySqlSchemaCollection sc)
		{
			MySqlSchemaRow expr_06 = sc.AddRow();
			expr_06["TypeName"] = "TINYINT";
			expr_06["ProviderDbType"] = MySqlDbType.Byte;
			expr_06["ColumnSize"] = 0;
			expr_06["CreateFormat"] = "TINYINT";
			expr_06["CreateParameters"] = null;
			expr_06["DataType"] = "System.SByte";
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
