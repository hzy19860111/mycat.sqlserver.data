using MySql.Data.MySqlClient;
using System;
using System.Globalization;
namespace MySql.Data.Types
{
	internal struct MySqlInt32 : IMySqlValue
	{
		private int mValue;
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
				return MySqlDbType.Int32;
			}
		}
		object IMySqlValue.Value
		{
			get
			{
				return this.mValue;
			}
		}
		public int Value
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
				return typeof(int);
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
		private MySqlInt32(MySqlDbType type)
		{
			this.is24Bit = (type == MySqlDbType.Int24);
			this.isNull = true;
			this.mValue = 0;
		}
		public MySqlInt32(MySqlDbType type, bool isNull)
		{
			this = new MySqlInt32(type);
			this.isNull = isNull;
		}
		public MySqlInt32(MySqlDbType type, int val)
		{
			this = new MySqlInt32(type);
			this.isNull = false;
			this.mValue = val;
		}
		void IMySqlValue.WriteValue(MySqlPacket packet, bool binary, object val, int length)
		{
			int num = (val is int) ? ((int)val) : Convert.ToInt32(val);
			if (binary)
			{
				packet.WriteInteger((long)num, this.is24Bit ? 3 : 4);
				return;
			}
			packet.WriteStringNoNull(num.ToString());
		}
		IMySqlValue IMySqlValue.ReadValue(MySqlPacket packet, long length, bool nullVal)
		{
			if (nullVal)
			{
				return new MySqlInt32(((IMySqlValue)this).MySqlDbType, true);
			}
			if (length == -1L)
			{
				return new MySqlInt32(((IMySqlValue)this).MySqlDbType, packet.ReadInteger(4));
			}
			return new MySqlInt32(((IMySqlValue)this).MySqlDbType, int.Parse(packet.ReadString(length), CultureInfo.InvariantCulture));
		}
		void IMySqlValue.SkipValue(MySqlPacket packet)
		{
			packet.Position += 4;
		}
		internal static void SetDSInfo(MySqlSchemaCollection sc)
		{
			string[] array = new string[]
			{
				"INT",
				"YEAR",
				"MEDIUMINT"
			};
			MySqlDbType[] array2 = new MySqlDbType[]
			{
				MySqlDbType.Int32,
				MySqlDbType.Year,
				MySqlDbType.Int24
			};
			for (int i = 0; i < array.Length; i++)
			{
				MySqlSchemaRow expr_3E = sc.AddRow();
				expr_3E["TypeName"] = array[i];
				expr_3E["ProviderDbType"] = array2[i];
				expr_3E["ColumnSize"] = 0;
				expr_3E["CreateFormat"] = array[i];
				expr_3E["CreateParameters"] = null;
				expr_3E["DataType"] = "System.Int32";
				expr_3E["IsAutoincrementable"] = (array2[i] != MySqlDbType.Year);
				expr_3E["IsBestMatch"] = true;
				expr_3E["IsCaseSensitive"] = false;
				expr_3E["IsFixedLength"] = true;
				expr_3E["IsFixedPrecisionScale"] = true;
				expr_3E["IsLong"] = false;
				expr_3E["IsNullable"] = true;
				expr_3E["IsSearchable"] = true;
				expr_3E["IsSearchableWithLike"] = false;
				expr_3E["IsUnsigned"] = false;
				expr_3E["MaximumScale"] = 0;
				expr_3E["MinimumScale"] = 0;
				expr_3E["IsConcurrencyType"] = DBNull.Value;
				expr_3E["IsLiteralSupported"] = false;
				expr_3E["LiteralPrefix"] = null;
				expr_3E["LiteralSuffix"] = null;
				expr_3E["NativeDataType"] = null;
			}
		}
	}
}
