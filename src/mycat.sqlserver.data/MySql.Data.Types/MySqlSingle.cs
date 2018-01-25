using MySql.Data.MySqlClient;
using System;
using System.Globalization;
namespace MySql.Data.Types
{
	internal struct MySqlSingle : IMySqlValue
	{
		private float mValue;
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
				return MySqlDbType.Float;
			}
		}
		object IMySqlValue.Value
		{
			get
			{
				return this.mValue;
			}
		}
		public float Value
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
				return typeof(float);
			}
		}
		string IMySqlValue.MySqlTypeName
		{
			get
			{
				return "FLOAT";
			}
		}
		public MySqlSingle(bool isNull)
		{
			this.isNull = isNull;
			this.mValue = 0f;
		}
		public MySqlSingle(float val)
		{
			this.isNull = false;
			this.mValue = val;
		}
		void IMySqlValue.WriteValue(MySqlPacket packet, bool binary, object val, int length)
		{
			float value = (val is float) ? ((float)val) : Convert.ToSingle(val);
			if (binary)
			{
				packet.Write(BitConverter.GetBytes(value));
				return;
			}
			packet.WriteStringNoNull(value.ToString("R", CultureInfo.InvariantCulture));
		}
		IMySqlValue IMySqlValue.ReadValue(MySqlPacket packet, long length, bool nullVal)
		{
			if (nullVal)
			{
				return new MySqlSingle(true);
			}
			if (length == -1L)
			{
				byte[] array = new byte[4];
				packet.Read(array, 0, 4);
				return new MySqlSingle(BitConverter.ToSingle(array, 0));
			}
			return new MySqlSingle(float.Parse(packet.ReadString(length), CultureInfo.InvariantCulture));
		}
		void IMySqlValue.SkipValue(MySqlPacket packet)
		{
			packet.Position += 4;
		}
		internal static void SetDSInfo(MySqlSchemaCollection sc)
		{
			MySqlSchemaRow expr_06 = sc.AddRow();
			expr_06["TypeName"] = "FLOAT";
			expr_06["ProviderDbType"] = MySqlDbType.Float;
			expr_06["ColumnSize"] = 0;
			expr_06["CreateFormat"] = "FLOAT";
			expr_06["CreateParameters"] = null;
			expr_06["DataType"] = "System.Single";
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
