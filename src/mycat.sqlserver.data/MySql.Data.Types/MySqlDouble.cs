using MySql.Data.MySqlClient;
using System;
using System.Globalization;
namespace MySql.Data.Types
{
	internal struct MySqlDouble : IMySqlValue
	{
		private double mValue;
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
				return MySqlDbType.Double;
			}
		}
		object IMySqlValue.Value
		{
			get
			{
				return this.mValue;
			}
		}
		public double Value
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
				return typeof(double);
			}
		}
		string IMySqlValue.MySqlTypeName
		{
			get
			{
				return "DOUBLE";
			}
		}
		public MySqlDouble(bool isNull)
		{
			this.isNull = isNull;
			this.mValue = 0.0;
		}
		public MySqlDouble(double val)
		{
			this.isNull = false;
			this.mValue = val;
		}
		void IMySqlValue.WriteValue(MySqlPacket packet, bool binary, object val, int length)
		{
			double value = (val is double) ? ((double)val) : Convert.ToDouble(val);
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
				return new MySqlDouble(true);
			}
			if (length == -1L)
			{
				byte[] array = new byte[8];
				packet.Read(array, 0, 8);
				return new MySqlDouble(BitConverter.ToDouble(array, 0));
			}
			string text = packet.ReadString(length);
			double val;
			try
			{
				val = double.Parse(text, CultureInfo.InvariantCulture);
			}
			catch (OverflowException)
			{
				if (text.StartsWith("-", StringComparison.Ordinal))
				{
					val = -1.7976931348623157E+308;
				}
				else
				{
					val = 1.7976931348623157E+308;
				}
			}
			return new MySqlDouble(val);
		}
		void IMySqlValue.SkipValue(MySqlPacket packet)
		{
			packet.Position += 8;
		}
		internal static void SetDSInfo(MySqlSchemaCollection sc)
		{
			MySqlSchemaRow expr_06 = sc.AddRow();
			expr_06["TypeName"] = "DOUBLE";
			expr_06["ProviderDbType"] = MySqlDbType.Double;
			expr_06["ColumnSize"] = 0;
			expr_06["CreateFormat"] = "DOUBLE";
			expr_06["CreateParameters"] = null;
			expr_06["DataType"] = "System.Double";
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
