using MySql.Data.MySqlClient;
using System;
namespace MySql.Data.Types
{
	internal struct MySqlString : IMySqlValue
	{
		private string mValue;
		private bool isNull;
		private MySqlDbType type;
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
				return this.type;
			}
		}
		object IMySqlValue.Value
		{
			get
			{
				return this.mValue;
			}
		}
		public string Value
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
				return typeof(string);
			}
		}
		string IMySqlValue.MySqlTypeName
		{
			get
			{
				if (this.type == MySqlDbType.Set)
				{
					return "SET";
				}
				if (this.type != MySqlDbType.Enum)
				{
					return "VARCHAR";
				}
				return "ENUM";
			}
		}
		public MySqlString(MySqlDbType type, bool isNull)
		{
			this.type = type;
			this.isNull = isNull;
			this.mValue = string.Empty;
		}
		public MySqlString(MySqlDbType type, string val)
		{
			this.type = type;
			this.isNull = false;
			this.mValue = val;
		}
		void IMySqlValue.WriteValue(MySqlPacket packet, bool binary, object val, int length)
		{
			string text = val.ToString();
			if (length > 0)
			{
				length = Math.Min(length, text.Length);
				text = text.Substring(0, length);
			}
			if (binary)
			{
				packet.WriteLenString(text);
				return;
			}
			packet.WriteStringNoNull("'" + MySqlHelper.EscapeString(text) + "'");
		}
		IMySqlValue IMySqlValue.ReadValue(MySqlPacket packet, long length, bool nullVal)
		{
			if (nullVal)
			{
				return new MySqlString(this.type, true);
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
			return new MySqlString(this.type, val);
		}
		void IMySqlValue.SkipValue(MySqlPacket packet)
		{
			int num = (int)packet.ReadFieldLength();
			packet.Position += num;
		}
		internal static void SetDSInfo(MySqlSchemaCollection sc)
		{
			string[] array = new string[]
			{
				"CHAR",
				"NCHAR",
				"VARCHAR",
				"NVARCHAR",
				"SET",
				"ENUM",
				"TINYTEXT",
				"TEXT",
				"MEDIUMTEXT",
				"LONGTEXT"
			};
			MySqlDbType[] array2 = new MySqlDbType[]
			{
				MySqlDbType.String,
				MySqlDbType.String,
				MySqlDbType.VarChar,
				MySqlDbType.VarChar,
				MySqlDbType.Set,
				MySqlDbType.Enum,
				MySqlDbType.TinyText,
				MySqlDbType.Text,
				MySqlDbType.MediumText,
				MySqlDbType.LongText
			};
			for (int i = 0; i < array.Length; i++)
			{
				MySqlSchemaRow expr_79 = sc.AddRow();
				expr_79["TypeName"] = array[i];
				expr_79["ProviderDbType"] = array2[i];
				expr_79["ColumnSize"] = 0;
				expr_79["CreateFormat"] = ((i < 4) ? (array[i] + "({0})") : array[i]);
				expr_79["CreateParameters"] = ((i < 4) ? "size" : null);
				expr_79["DataType"] = "System.String";
				expr_79["IsAutoincrementable"] = false;
				expr_79["IsBestMatch"] = true;
				expr_79["IsCaseSensitive"] = false;
				expr_79["IsFixedLength"] = false;
				expr_79["IsFixedPrecisionScale"] = true;
				expr_79["IsLong"] = false;
				expr_79["IsNullable"] = true;
				expr_79["IsSearchable"] = true;
				expr_79["IsSearchableWithLike"] = true;
				expr_79["IsUnsigned"] = false;
				expr_79["MaximumScale"] = 0;
				expr_79["MinimumScale"] = 0;
				expr_79["IsConcurrencyType"] = DBNull.Value;
				expr_79["IsLiteralSupported"] = false;
				expr_79["LiteralPrefix"] = null;
				expr_79["LiteralSuffix"] = null;
				expr_79["NativeDataType"] = null;
			}
		}
	}
}
