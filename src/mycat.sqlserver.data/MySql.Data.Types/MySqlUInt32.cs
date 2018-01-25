using MySql.Data.MySqlClient;
using System;
using System.Globalization;
namespace MySql.Data.Types
{
	internal struct MySqlUInt32 : IMySqlValue
	{
		private uint mValue;
		private bool isNull;
		private bool is24Bit;
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
				return MySqlDbType.UInt32;
			}
		}
		object IMySqlValue.Value
		{
			get
			{
				return this.mValue;
			}
		}
		public uint Value
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
				return typeof(uint);
			}
		}
		string IMySqlValue.MySqlTypeName
		{
			get
			{
				if (!this.is24Bit)
				{
					return "INT";
				}
				return "MEDIUMINT";
			}
		}
		private MySqlUInt32(MySqlDbType type)
		{
			this.is24Bit = (type == MySqlDbType.Int24);
			this.isNull = true;
			this.mValue = 0u;
		}
		public MySqlUInt32(MySqlDbType type, bool isNull)
		{
			this = new MySqlUInt32(type);
			this.isNull = isNull;
		}
		public MySqlUInt32(MySqlDbType type, uint val)
		{
			this = new MySqlUInt32(type);
			this.isNull = false;
			this.mValue = val;
		}
		void IMySqlValue.WriteValue(MySqlPacket packet, bool binary, object v, int length)
		{
			uint num = (v is uint) ? ((uint)v) : Convert.ToUInt32(v);
			if (binary)
			{
				packet.WriteInteger((long)((ulong)num), this.is24Bit ? 3 : 4);
				return;
			}
			packet.WriteStringNoNull(num.ToString());
		}
		IMySqlValue IMySqlValue.ReadValue(MySqlPacket packet, long length, bool nullVal)
		{
			if (nullVal)
			{
				return new MySqlUInt32(((IMySqlValue)this).MySqlDbType, true);
			}
			if (length == -1L)
			{
				return new MySqlUInt32(((IMySqlValue)this).MySqlDbType, (uint)packet.ReadInteger(4));
			}
			return new MySqlUInt32(((IMySqlValue)this).MySqlDbType, uint.Parse(packet.ReadString(length), NumberStyles.Any, CultureInfo.InvariantCulture));
		}
		void IMySqlValue.SkipValue(MySqlPacket packet)
		{
			packet.Position += 4;
		}
		internal static void SetDSInfo(MySqlSchemaCollection sc)
		{
			string[] array = new string[]
			{
				"MEDIUMINT",
				"INT"
			};
			MySqlDbType[] array2 = new MySqlDbType[]
			{
				MySqlDbType.UInt24,
				MySqlDbType.UInt32
			};
			for (int i = 0; i < array.Length; i++)
			{
				MySqlSchemaRow expr_3B = sc.AddRow();
				expr_3B["TypeName"] = array[i];
				expr_3B["ProviderDbType"] = array2[i];
				expr_3B["ColumnSize"] = 0;
				expr_3B["CreateFormat"] = array[i] + " UNSIGNED";
				expr_3B["CreateParameters"] = null;
				expr_3B["DataType"] = "System.UInt32";
				expr_3B["IsAutoincrementable"] = true;
				expr_3B["IsBestMatch"] = true;
				expr_3B["IsCaseSensitive"] = false;
				expr_3B["IsFixedLength"] = true;
				expr_3B["IsFixedPrecisionScale"] = true;
				expr_3B["IsLong"] = false;
				expr_3B["IsNullable"] = true;
				expr_3B["IsSearchable"] = true;
				expr_3B["IsSearchableWithLike"] = false;
				expr_3B["IsUnsigned"] = true;
				expr_3B["MaximumScale"] = 0;
				expr_3B["MinimumScale"] = 0;
				expr_3B["IsConcurrencyType"] = DBNull.Value;
				expr_3B["IsLiteralSupported"] = false;
				expr_3B["LiteralPrefix"] = null;
				expr_3B["LiteralSuffix"] = null;
				expr_3B["NativeDataType"] = null;
			}
		}
	}
}
