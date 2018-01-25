using MySql.Data.Common;
using MySql.Data.MySqlClient.Properties;
using MySql.Data.Types;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
namespace MySql.Data.MySqlClient
{
    internal class SchemaProvider
    {
        protected MySqlConnection connection;
        public static string MetaCollection = "MetaDataCollections";
        public SchemaProvider(MySqlConnection connectionToUse)
        {
            this.connection = connectionToUse;
        }
        public virtual MySqlSchemaCollection GetSchema(string collection, string[] restrictions)
        {
            if (this.connection.State != ConnectionState.Open)
            {
                throw new MySqlException("GetSchema can only be called on an open connection.");
            }
            collection = StringUtility.ToUpperInvariant(collection);
            MySqlSchemaCollection expr_29 = this.GetSchemaInternal(collection, restrictions);
            if (expr_29 == null)
            {
                throw new ArgumentException("Invalid collection name");
            }
            return expr_29;
        }
        public virtual MySqlSchemaCollection GetDatabases(string[] restrictions)
        {
            Regex regex = null;
            int arg_22_0 = int.Parse(this.connection.driver.Property("lower_case_table_names"));
            string text = "SHOW DATABASES";
            if (arg_22_0 == 0 && restrictions != null && restrictions.Length >= 1)
            {
                text = text + " LIKE '" + restrictions[0] + "'";
            }
            MySqlSchemaCollection mySqlSchemaCollection = this.QueryCollection("Databases", text);
            if (arg_22_0 != 0 && restrictions != null && restrictions.Length >= 1 && restrictions[0] != null)
            {
                regex = new Regex(restrictions[0], RegexOptions.IgnoreCase);
            }
            MySqlSchemaCollection mySqlSchemaCollection2 = new MySqlSchemaCollection("Databases");
            mySqlSchemaCollection2.AddColumn("CATALOG_NAME", typeof(string));
            mySqlSchemaCollection2.AddColumn("SCHEMA_NAME", typeof(string));
            foreach (MySqlSchemaRow current in mySqlSchemaCollection.Rows)
            {
                if (regex == null || regex.Match(current[0].ToString()).Success)
                {
                    mySqlSchemaCollection2.AddRow()[1] = current[0];
                }
            }
            return mySqlSchemaCollection2;
        }
        public virtual MySqlSchemaCollection GetTables(string[] restrictions)
        {
            MySqlSchemaCollection mySqlSchemaCollection = new MySqlSchemaCollection("Tables");
            mySqlSchemaCollection.AddColumn("TABLE_CATALOG", typeof(string));
            mySqlSchemaCollection.AddColumn("TABLE_SCHEMA", typeof(string));
            mySqlSchemaCollection.AddColumn("TABLE_NAME", typeof(string));
            mySqlSchemaCollection.AddColumn("TABLE_TYPE", typeof(string));
            mySqlSchemaCollection.AddColumn("ENGINE", typeof(string));
            mySqlSchemaCollection.AddColumn("VERSION", typeof(ulong));
            mySqlSchemaCollection.AddColumn("ROW_FORMAT", typeof(string));
            mySqlSchemaCollection.AddColumn("TABLE_ROWS", typeof(ulong));
            mySqlSchemaCollection.AddColumn("AVG_ROW_LENGTH", typeof(ulong));
            mySqlSchemaCollection.AddColumn("DATA_LENGTH", typeof(ulong));
            mySqlSchemaCollection.AddColumn("MAX_DATA_LENGTH", typeof(ulong));
            mySqlSchemaCollection.AddColumn("INDEX_LENGTH", typeof(ulong));
            mySqlSchemaCollection.AddColumn("DATA_FREE", typeof(ulong));
            mySqlSchemaCollection.AddColumn("AUTO_INCREMENT", typeof(ulong));
            mySqlSchemaCollection.AddColumn("CREATE_TIME", typeof(DateTime));
            mySqlSchemaCollection.AddColumn("UPDATE_TIME", typeof(DateTime));
            mySqlSchemaCollection.AddColumn("CHECK_TIME", typeof(DateTime));
            mySqlSchemaCollection.AddColumn("TABLE_COLLATION", typeof(string));
            mySqlSchemaCollection.AddColumn("CHECKSUM", typeof(ulong));
            mySqlSchemaCollection.AddColumn("CREATE_OPTIONS", typeof(string));
            mySqlSchemaCollection.AddColumn("TABLE_COMMENT", typeof(string));
            string[] array = new string[4];
            if (restrictions != null && restrictions.Length >= 2)
            {
                array[0] = restrictions[1];
            }
            MySqlSchemaCollection arg_20B_0 = this.GetDatabases(array);
            if (restrictions != null)
            {
                Array.Copy(restrictions, array, Math.Min(array.Length, restrictions.Length));
            }
            foreach (MySqlSchemaRow current in arg_20B_0.Rows)
            {
                array[1] = current["SCHEMA_NAME"].ToString();
                this.FindTables(mySqlSchemaCollection, array);
            }
            return mySqlSchemaCollection;
        }
        protected void QuoteDefaultValues(MySqlSchemaCollection schemaCollection)
        {
            if (schemaCollection == null)
            {
                return;
            }
            if (!schemaCollection.ContainsColumn("COLUMN_DEFAULT"))
            {
                return;
            }
            foreach (MySqlSchemaRow current in schemaCollection.Rows)
            {
                object arg = current["COLUMN_DEFAULT"];
                if (MetaData.IsTextType(current["DATA_TYPE"].ToString()))
                {
                    current["COLUMN_DEFAULT"] = string.Format("{0}", arg);
                }
            }
        }
        public virtual MySqlSchemaCollection GetColumns(string[] restrictions)
        {
            MySqlSchemaCollection mySqlSchemaCollection = new MySqlSchemaCollection("Columns");
            mySqlSchemaCollection.AddColumn("TABLE_CATALOG", typeof(string));
            mySqlSchemaCollection.AddColumn("TABLE_SCHEMA", typeof(string));
            mySqlSchemaCollection.AddColumn("TABLE_NAME", typeof(string));
            mySqlSchemaCollection.AddColumn("COLUMN_NAME", typeof(string));
            mySqlSchemaCollection.AddColumn("ORDINAL_POSITION", typeof(ulong));
            mySqlSchemaCollection.AddColumn("COLUMN_DEFAULT", typeof(string));
            mySqlSchemaCollection.AddColumn("IS_NULLABLE", typeof(string));
            mySqlSchemaCollection.AddColumn("DATA_TYPE", typeof(string));
            mySqlSchemaCollection.AddColumn("CHARACTER_MAXIMUM_LENGTH", typeof(ulong));
            mySqlSchemaCollection.AddColumn("CHARACTER_OCTET_LENGTH", typeof(ulong));
            mySqlSchemaCollection.AddColumn("NUMERIC_PRECISION", typeof(ulong));
            mySqlSchemaCollection.AddColumn("NUMERIC_SCALE", typeof(ulong));
            mySqlSchemaCollection.AddColumn("CHARACTER_SET_NAME", typeof(string));
            mySqlSchemaCollection.AddColumn("COLLATION_NAME", typeof(string));
            mySqlSchemaCollection.AddColumn("COLUMN_TYPE", typeof(string));
            mySqlSchemaCollection.AddColumn("COLUMN_KEY", typeof(string));
            mySqlSchemaCollection.AddColumn("EXTRA", typeof(string));
            mySqlSchemaCollection.AddColumn("PRIVILEGES", typeof(string));
            mySqlSchemaCollection.AddColumn("COLUMN_COMMENT", typeof(string));
            mySqlSchemaCollection.AddColumn("GENERATION_EXPRESSION", typeof(string));
            string columnRestriction = null;
            if (restrictions != null && restrictions.Length == 4)
            {
                columnRestriction = restrictions[3];
                restrictions[3] = null;
            }
            foreach (MySqlSchemaRow current in this.GetTables(restrictions).Rows)
            {
                this.LoadTableColumns(mySqlSchemaCollection, current["TABLE_SCHEMA"].ToString(), current["TABLE_NAME"].ToString(), columnRestriction);
            }
            this.QuoteDefaultValues(mySqlSchemaCollection);
            return mySqlSchemaCollection;
        }
        private void LoadTableColumns(MySqlSchemaCollection schemaCollection, string schema, string tableName, string columnRestriction)
        {
            MySqlCommand arg_19_0 = new MySqlCommand(string.Format("SHOW FULL COLUMNS FROM `{0}`.`{1}`", schema, tableName), this.connection);
            int num = 1;
            using (MySqlDataReader mySqlDataReader = arg_19_0.ExecuteReader())
            {
                while (mySqlDataReader.Read())
                {
                    string @string = mySqlDataReader.GetString(0);
                    if (columnRestriction == null || !(@string != columnRestriction))
                    {
                        MySqlSchemaRow mySqlSchemaRow = schemaCollection.AddRow();
                        mySqlSchemaRow["TABLE_CATALOG"] = DBNull.Value;
                        mySqlSchemaRow["TABLE_SCHEMA"] = schema;
                        mySqlSchemaRow["TABLE_NAME"] = tableName;
                        mySqlSchemaRow["COLUMN_NAME"] = @string;
                        mySqlSchemaRow["ORDINAL_POSITION"] = num++;
                        mySqlSchemaRow["COLUMN_DEFAULT"] = mySqlDataReader.GetValue(5);
                        mySqlSchemaRow["IS_NULLABLE"] = mySqlDataReader.GetString(3);
                        mySqlSchemaRow["DATA_TYPE"] = mySqlDataReader.GetString(1);
                        mySqlSchemaRow["CHARACTER_MAXIMUM_LENGTH"] = DBNull.Value;
                        mySqlSchemaRow["CHARACTER_OCTET_LENGTH"] = DBNull.Value;
                        mySqlSchemaRow["NUMERIC_PRECISION"] = DBNull.Value;
                        mySqlSchemaRow["NUMERIC_SCALE"] = DBNull.Value;
                        mySqlSchemaRow["CHARACTER_SET_NAME"] = mySqlDataReader.GetValue(2);
                        mySqlSchemaRow["COLLATION_NAME"] = mySqlSchemaRow["CHARACTER_SET_NAME"];
                        mySqlSchemaRow["COLUMN_TYPE"] = mySqlDataReader.GetString(1);
                        mySqlSchemaRow["COLUMN_KEY"] = mySqlDataReader.GetString(4);
                        mySqlSchemaRow["EXTRA"] = mySqlDataReader.GetString(6);
                        mySqlSchemaRow["PRIVILEGES"] = mySqlDataReader.GetString(7);
                        mySqlSchemaRow["COLUMN_COMMENT"] = mySqlDataReader.GetString(8);
                        mySqlSchemaRow["GENERATION_EXPRESION"] = (mySqlDataReader.GetString(6).Contains("VIRTUAL") ? mySqlDataReader.GetString(9) : string.Empty);
                        SchemaProvider.ParseColumnRow(mySqlSchemaRow);
                    }
                }
            }
        }
        private static void ParseColumnRow(MySqlSchemaRow row)
        {
            string text = row["CHARACTER_SET_NAME"].ToString();
            int num = text.IndexOf('_');
            if (num != -1)
            {
                row["CHARACTER_SET_NAME"] = text.Substring(0, num);
            }
            string text2 = row["DATA_TYPE"].ToString();
            num = text2.IndexOf('(');
            if (num == -1)
            {
                return;
            }
            row["DATA_TYPE"] = text2.Substring(0, num);
            int num2 = text2.IndexOf(')', num);
            string text3 = text2.Substring(num + 1, num2 - (num + 1));
            string a = row["DATA_TYPE"].ToString().ToLower();
            if (a == "char" || a == "varchar")
            {
                row["CHARACTER_MAXIMUM_LENGTH"] = text3;
                return;
            }
            if (a == "real" || a == "decimal")
            {
                string[] array = text3.Split(new char[]
				{
					','
				});
                row["NUMERIC_PRECISION"] = array[0];
                if (array.Length == 2)
                {
                    row["NUMERIC_SCALE"] = array[1];
                }
            }
        }
        public virtual MySqlSchemaCollection GetIndexes(string[] restrictions)
        {
            MySqlSchemaCollection mySqlSchemaCollection = new MySqlSchemaCollection("Indexes");
            mySqlSchemaCollection.AddColumn("INDEX_CATALOG", typeof(string));
            mySqlSchemaCollection.AddColumn("INDEX_SCHEMA", typeof(string));
            mySqlSchemaCollection.AddColumn("INDEX_NAME", typeof(string));
            mySqlSchemaCollection.AddColumn("TABLE_NAME", typeof(string));
            mySqlSchemaCollection.AddColumn("UNIQUE", typeof(bool));
            mySqlSchemaCollection.AddColumn("PRIMARY", typeof(bool));
            mySqlSchemaCollection.AddColumn("TYPE", typeof(string));
            mySqlSchemaCollection.AddColumn("COMMENT", typeof(string));
            string[] array = new string[Math.Max((restrictions == null) ? 4 : restrictions.Length, 4)];
            if (restrictions != null)
            {
                restrictions.CopyTo(array, 0);
            }
            array[3] = "BASE TABLE";
            foreach (MySqlSchemaRow current in this.GetTables(array).Rows)
            {
                string sql = string.Format("SHOW INDEX FROM `{0}`.`{1}`", MySqlHelper.DoubleQuoteString((string)current["TABLE_SCHEMA"]), MySqlHelper.DoubleQuoteString((string)current["TABLE_NAME"]));
                foreach (MySqlSchemaRow current2 in this.QueryCollection("indexes", sql).Rows)
                {
                    if ((long)current2["SEQ_IN_INDEX"] == 1L && (restrictions == null || restrictions.Length != 4 || restrictions[3] == null || current2["KEY_NAME"].Equals(restrictions[3])))
                    {
                        MySqlSchemaRow expr_1A3 = mySqlSchemaCollection.AddRow();
                        expr_1A3["INDEX_CATALOG"] = null;
                        expr_1A3["INDEX_SCHEMA"] = current["TABLE_SCHEMA"];
                        expr_1A3["INDEX_NAME"] = current2["KEY_NAME"];
                        expr_1A3["TABLE_NAME"] = current2["TABLE"];
                        expr_1A3["UNIQUE"] = ((long)current2["NON_UNIQUE"] == 0L);
                        expr_1A3["PRIMARY"] = current2["KEY_NAME"].Equals("PRIMARY");
                        expr_1A3["TYPE"] = current2["INDEX_TYPE"];
                        expr_1A3["COMMENT"] = current2["COMMENT"];
                    }
                }
            }
            return mySqlSchemaCollection;
        }
        public virtual MySqlSchemaCollection GetIndexColumns(string[] restrictions)
        {
            MySqlSchemaCollection mySqlSchemaCollection = new MySqlSchemaCollection("IndexColumns");
            mySqlSchemaCollection.AddColumn("INDEX_CATALOG", typeof(string));
            mySqlSchemaCollection.AddColumn("INDEX_SCHEMA", typeof(string));
            mySqlSchemaCollection.AddColumn("INDEX_NAME", typeof(string));
            mySqlSchemaCollection.AddColumn("TABLE_NAME", typeof(string));
            mySqlSchemaCollection.AddColumn("COLUMN_NAME", typeof(string));
            mySqlSchemaCollection.AddColumn("ORDINAL_POSITION", typeof(int));
            mySqlSchemaCollection.AddColumn("SORT_ORDER", typeof(string));
            string[] array = new string[Math.Max((restrictions == null) ? 4 : restrictions.Length, 4)];
            if (restrictions != null)
            {
                restrictions.CopyTo(array, 0);
            }
            array[3] = "BASE TABLE";
            foreach (MySqlSchemaRow current in this.GetTables(array).Rows)
            {
                using (MySqlDataReader mySqlDataReader = new MySqlCommand(string.Format("SHOW INDEX FROM `{0}`.`{1}`", current["TABLE_SCHEMA"], current["TABLE_NAME"]), this.connection).ExecuteReader())
                {
                    while (mySqlDataReader.Read())
                    {
                        string @string = SchemaProvider.GetString(mySqlDataReader, mySqlDataReader.GetOrdinal("KEY_NAME"));
                        string string2 = SchemaProvider.GetString(mySqlDataReader, mySqlDataReader.GetOrdinal("COLUMN_NAME"));
                        if (restrictions == null || ((restrictions.Length < 4 || restrictions[3] == null || !(@string != restrictions[3])) && (restrictions.Length < 5 || restrictions[4] == null || !(string2 != restrictions[4]))))
                        {
                            MySqlSchemaRow expr_189 = mySqlSchemaCollection.AddRow();
                            expr_189["INDEX_CATALOG"] = null;
                            expr_189["INDEX_SCHEMA"] = current["TABLE_SCHEMA"];
                            expr_189["INDEX_NAME"] = @string;
                            expr_189["TABLE_NAME"] = SchemaProvider.GetString(mySqlDataReader, mySqlDataReader.GetOrdinal("TABLE"));
                            expr_189["COLUMN_NAME"] = string2;
                            expr_189["ORDINAL_POSITION"] = mySqlDataReader.GetValue(mySqlDataReader.GetOrdinal("SEQ_IN_INDEX"));
                            expr_189["SORT_ORDER"] = mySqlDataReader.GetString("COLLATION");
                        }
                    }
                }
            }
            return mySqlSchemaCollection;
        }
        public virtual MySqlSchemaCollection GetForeignKeys(string[] restrictions)
        {
            MySqlSchemaCollection mySqlSchemaCollection = new MySqlSchemaCollection("Foreign Keys");
            mySqlSchemaCollection.AddColumn("CONSTRAINT_CATALOG", typeof(string));
            mySqlSchemaCollection.AddColumn("CONSTRAINT_SCHEMA", typeof(string));
            mySqlSchemaCollection.AddColumn("CONSTRAINT_NAME", typeof(string));
            mySqlSchemaCollection.AddColumn("TABLE_CATALOG", typeof(string));
            mySqlSchemaCollection.AddColumn("TABLE_SCHEMA", typeof(string));
            mySqlSchemaCollection.AddColumn("TABLE_NAME", typeof(string));
            mySqlSchemaCollection.AddColumn("MATCH_OPTION", typeof(string));
            mySqlSchemaCollection.AddColumn("UPDATE_RULE", typeof(string));
            mySqlSchemaCollection.AddColumn("DELETE_RULE", typeof(string));
            mySqlSchemaCollection.AddColumn("REFERENCED_TABLE_CATALOG", typeof(string));
            mySqlSchemaCollection.AddColumn("REFERENCED_TABLE_SCHEMA", typeof(string));
            mySqlSchemaCollection.AddColumn("REFERENCED_TABLE_NAME", typeof(string));
            string filterName = null;
            if (restrictions != null && restrictions.Length >= 4)
            {
                filterName = restrictions[3];
                restrictions[3] = null;
            }
            foreach (MySqlSchemaRow current in this.GetTables(restrictions).Rows)
            {
                this.GetForeignKeysOnTable(mySqlSchemaCollection, current, filterName, false);
            }
            return mySqlSchemaCollection;
        }
        public virtual MySqlSchemaCollection GetForeignKeyColumns(string[] restrictions)
        {
            MySqlSchemaCollection mySqlSchemaCollection = new MySqlSchemaCollection("Foreign Keys");
            mySqlSchemaCollection.AddColumn("CONSTRAINT_CATALOG", typeof(string));
            mySqlSchemaCollection.AddColumn("CONSTRAINT_SCHEMA", typeof(string));
            mySqlSchemaCollection.AddColumn("CONSTRAINT_NAME", typeof(string));
            mySqlSchemaCollection.AddColumn("TABLE_CATALOG", typeof(string));
            mySqlSchemaCollection.AddColumn("TABLE_SCHEMA", typeof(string));
            mySqlSchemaCollection.AddColumn("TABLE_NAME", typeof(string));
            mySqlSchemaCollection.AddColumn("COLUMN_NAME", typeof(string));
            mySqlSchemaCollection.AddColumn("ORDINAL_POSITION", typeof(int));
            mySqlSchemaCollection.AddColumn("REFERENCED_TABLE_CATALOG", typeof(string));
            mySqlSchemaCollection.AddColumn("REFERENCED_TABLE_SCHEMA", typeof(string));
            mySqlSchemaCollection.AddColumn("REFERENCED_TABLE_NAME", typeof(string));
            mySqlSchemaCollection.AddColumn("REFERENCED_COLUMN_NAME", typeof(string));
            string filterName = null;
            if (restrictions != null && restrictions.Length >= 4)
            {
                filterName = restrictions[3];
                restrictions[3] = null;
            }
            foreach (MySqlSchemaRow current in this.GetTables(restrictions).Rows)
            {
                this.GetForeignKeysOnTable(mySqlSchemaCollection, current, filterName, true);
            }
            return mySqlSchemaCollection;
        }
        private string GetSqlMode()
        {
            return new MySqlCommand("SELECT @@SQL_MODE", this.connection).ExecuteScalar().ToString();
        }
        private void GetForeignKeysOnTable(MySqlSchemaCollection fkTable, MySqlSchemaRow tableToParse, string filterName, bool includeColumns)
        {
            string sqlMode = this.GetSqlMode();
            if (filterName != null)
            {
                filterName = StringUtility.ToLowerInvariant(filterName);
            }
            string arg_3A_0 = string.Format("SHOW CREATE TABLE `{0}`.`{1}`", tableToParse["TABLE_SCHEMA"], tableToParse["TABLE_NAME"]);
            string input = null;
            using (MySqlDataReader mySqlDataReader = new MySqlCommand(arg_3A_0, this.connection).ExecuteReader())
            {
                mySqlDataReader.Read();
                input = StringUtility.ToLowerInvariant(mySqlDataReader.GetString(1));
            }
            MySqlTokenizer mySqlTokenizer = new MySqlTokenizer(input);
            mySqlTokenizer.AnsiQuotes = (sqlMode.IndexOf("ANSI_QUOTES") != -1);
            mySqlTokenizer.BackslashEscapes = (sqlMode.IndexOf("NO_BACKSLASH_ESCAPES") != -1);
            while (true)
            {
                string text = mySqlTokenizer.NextToken();
                while (text != null && (text != "constraint" || mySqlTokenizer.Quoted))
                {
                    text = mySqlTokenizer.NextToken();
                }
                if (text == null)
                {
                    break;
                }
                SchemaProvider.ParseConstraint(fkTable, tableToParse, mySqlTokenizer, includeColumns);
            }
        }
        private static void ParseConstraint(MySqlSchemaCollection fkTable, MySqlSchemaRow table, MySqlTokenizer tokenizer, bool includeColumns)
        {
            string text = tokenizer.NextToken();
            MySqlSchemaRow mySqlSchemaRow = fkTable.AddRow();
            string a = tokenizer.NextToken();
            if (a != "foreign" || tokenizer.Quoted)
            {
                return;
            }
            tokenizer.NextToken();
            tokenizer.NextToken();
            mySqlSchemaRow["CONSTRAINT_CATALOG"] = table["TABLE_CATALOG"];
            mySqlSchemaRow["CONSTRAINT_SCHEMA"] = table["TABLE_SCHEMA"];
            mySqlSchemaRow["TABLE_CATALOG"] = table["TABLE_CATALOG"];
            mySqlSchemaRow["TABLE_SCHEMA"] = table["TABLE_SCHEMA"];
            mySqlSchemaRow["TABLE_NAME"] = table["TABLE_NAME"];
            mySqlSchemaRow["REFERENCED_TABLE_CATALOG"] = null;
            mySqlSchemaRow["CONSTRAINT_NAME"] = text.Trim(new char[]
			{
				'\'',
				'`'
			});
            List<string> srcColumns = includeColumns ? SchemaProvider.ParseColumns(tokenizer) : null;
            while (a != "references" || tokenizer.Quoted)
            {
                a = tokenizer.NextToken();
            }
            string text2 = tokenizer.NextToken();
            string text3 = tokenizer.NextToken();
            if (text3.StartsWith(".", StringComparison.Ordinal))
            {
                mySqlSchemaRow["REFERENCED_TABLE_SCHEMA"] = text2;
                mySqlSchemaRow["REFERENCED_TABLE_NAME"] = text3.Substring(1).Trim(new char[]
				{
					'\'',
					'`'
				});
                tokenizer.NextToken();
            }
            else
            {
                mySqlSchemaRow["REFERENCED_TABLE_SCHEMA"] = table["TABLE_SCHEMA"];
                mySqlSchemaRow["REFERENCED_TABLE_NAME"] = text2.Substring(1).Trim(new char[]
				{
					'\'',
					'`'
				});
            }
            List<string> targetColumns = includeColumns ? SchemaProvider.ParseColumns(tokenizer) : null;
            if (includeColumns)
            {
                SchemaProvider.ProcessColumns(fkTable, mySqlSchemaRow, srcColumns, targetColumns);
                return;
            }
            fkTable.Rows.Add(mySqlSchemaRow);
        }
        private static List<string> ParseColumns(MySqlTokenizer tokenizer)
        {
            List<string> list = new List<string>();
            string text = tokenizer.NextToken();
            while (text != ")")
            {
                if (text != ",")
                {
                    list.Add(text);
                }
                text = tokenizer.NextToken();
            }
            return list;
        }
        private static void ProcessColumns(MySqlSchemaCollection fkTable, MySqlSchemaRow row, List<string> srcColumns, List<string> targetColumns)
        {
            for (int i = 0; i < srcColumns.Count; i++)
            {
                MySqlSchemaRow mySqlSchemaRow = fkTable.AddRow();
                row.CopyRow(mySqlSchemaRow);
                mySqlSchemaRow["COLUMN_NAME"] = srcColumns[i];
                mySqlSchemaRow["ORDINAL_POSITION"] = i;
                mySqlSchemaRow["REFERENCED_COLUMN_NAME"] = targetColumns[i];
                fkTable.Rows.Add(mySqlSchemaRow);
            }
        }
        public virtual MySqlSchemaCollection GetUsers(string[] restrictions)
        {
            StringBuilder stringBuilder = new StringBuilder("SELECT Host, User FROM mysql.user");
            if (restrictions != null && restrictions.Length != 0)
            {
                stringBuilder.AppendFormat(CultureInfo.InvariantCulture, " WHERE User LIKE '{0}'", new object[]
				{
					restrictions[0]
				});
            }
            MySqlSchemaCollection expr_40 = this.QueryCollection("Users", stringBuilder.ToString());
            expr_40.Columns[0].Name = "HOST";
            expr_40.Columns[1].Name = "USERNAME";
            return expr_40;
        }
        public virtual MySqlSchemaCollection GetProcedures(string[] restrictions)
        {
            MySqlSchemaCollection mySqlSchemaCollection = new MySqlSchemaCollection("Procedures");
            mySqlSchemaCollection.AddColumn("SPECIFIC_NAME", typeof(string));
            mySqlSchemaCollection.AddColumn("ROUTINE_CATALOG", typeof(string));
            mySqlSchemaCollection.AddColumn("ROUTINE_SCHEMA", typeof(string));
            mySqlSchemaCollection.AddColumn("ROUTINE_NAME", typeof(string));
            mySqlSchemaCollection.AddColumn("ROUTINE_TYPE", typeof(string));
            mySqlSchemaCollection.AddColumn("DTD_IDENTIFIER", typeof(string));
            mySqlSchemaCollection.AddColumn("ROUTINE_BODY", typeof(string));
            mySqlSchemaCollection.AddColumn("ROUTINE_DEFINITION", typeof(string));
            mySqlSchemaCollection.AddColumn("EXTERNAL_NAME", typeof(string));
            mySqlSchemaCollection.AddColumn("EXTERNAL_LANGUAGE", typeof(string));
            mySqlSchemaCollection.AddColumn("PARAMETER_STYLE", typeof(string));
            mySqlSchemaCollection.AddColumn("IS_DETERMINISTIC", typeof(string));
            mySqlSchemaCollection.AddColumn("SQL_DATA_ACCESS", typeof(string));
            mySqlSchemaCollection.AddColumn("SQL_PATH", typeof(string));
            mySqlSchemaCollection.AddColumn("SECURITY_TYPE", typeof(string));
            mySqlSchemaCollection.AddColumn("CREATED", typeof(DateTime));
            mySqlSchemaCollection.AddColumn("LAST_ALTERED", typeof(DateTime));
            mySqlSchemaCollection.AddColumn("SQL_MODE", typeof(string));
            mySqlSchemaCollection.AddColumn("ROUTINE_COMMENT", typeof(string));
            mySqlSchemaCollection.AddColumn("DEFINER", typeof(string));
            StringBuilder stringBuilder = new StringBuilder("SELECT * FROM mysql.proc WHERE 1=1");
            if (restrictions != null)
            {
                if (restrictions.Length >= 2 && restrictions[1] != null)
                {
                    stringBuilder.AppendFormat(CultureInfo.InvariantCulture, " AND db LIKE '{0}'", new object[]
					{
						restrictions[1]
					});
                }
                if (restrictions.Length >= 3 && restrictions[2] != null)
                {
                    stringBuilder.AppendFormat(CultureInfo.InvariantCulture, " AND name LIKE '{0}'", new object[]
					{
						restrictions[2]
					});
                }
                if (restrictions.Length >= 4 && restrictions[3] != null)
                {
                    stringBuilder.AppendFormat(CultureInfo.InvariantCulture, " AND type LIKE '{0}'", new object[]
					{
						restrictions[3]
					});
                }
            }
            using (MySqlDataReader mySqlDataReader = new MySqlCommand(stringBuilder.ToString(), this.connection).ExecuteReader())
            {
                while (mySqlDataReader.Read())
                {
                    MySqlSchemaRow expr_26B = mySqlSchemaCollection.AddRow();
                    expr_26B["SPECIFIC_NAME"] = mySqlDataReader.GetString("specific_name");
                    expr_26B["ROUTINE_CATALOG"] = DBNull.Value;
                    expr_26B["ROUTINE_SCHEMA"] = mySqlDataReader.GetString("db");
                    expr_26B["ROUTINE_NAME"] = mySqlDataReader.GetString("name");
                    string @string = mySqlDataReader.GetString("type");
                    expr_26B["ROUTINE_TYPE"] = @string;
                    expr_26B["DTD_IDENTIFIER"] = ((StringUtility.ToLowerInvariant(@string) == "function") ? mySqlDataReader.GetString("returns") : DBNull.Value.ToString());
                    expr_26B["ROUTINE_BODY"] = "SQL";
                    expr_26B["ROUTINE_DEFINITION"] = mySqlDataReader.GetString("body");
                    expr_26B["EXTERNAL_NAME"] = DBNull.Value;
                    expr_26B["EXTERNAL_LANGUAGE"] = DBNull.Value;
                    expr_26B["PARAMETER_STYLE"] = "SQL";
                    expr_26B["IS_DETERMINISTIC"] = mySqlDataReader.GetString("is_deterministic");
                    expr_26B["SQL_DATA_ACCESS"] = mySqlDataReader.GetString("sql_data_access");
                    expr_26B["SQL_PATH"] = DBNull.Value;
                    expr_26B["SECURITY_TYPE"] = mySqlDataReader.GetString("security_type");
                    expr_26B["CREATED"] = mySqlDataReader.GetDateTime("created");
                    expr_26B["LAST_ALTERED"] = mySqlDataReader.GetDateTime("modified");
                    expr_26B["SQL_MODE"] = mySqlDataReader.GetString("sql_mode");
                    expr_26B["ROUTINE_COMMENT"] = mySqlDataReader.GetString("comment");
                    expr_26B["DEFINER"] = mySqlDataReader.GetString("definer");
                }
            }
            return mySqlSchemaCollection;
        }
        protected virtual MySqlSchemaCollection GetCollections()
        {
            object[][] data = new object[][]
			{
				new object[]
				{
					"MetaDataCollections",
					0,
					0
				},
				new object[]
				{
					"DataSourceInformation",
					0,
					0
				},
				new object[]
				{
					"DataTypes",
					0,
					0
				},
				new object[]
				{
					"Restrictions",
					0,
					0
				},
				new object[]
				{
					"ReservedWords",
					0,
					0
				},
				new object[]
				{
					"Databases",
					1,
					1
				},
				new object[]
				{
					"Tables",
					4,
					2
				},
				new object[]
				{
					"Columns",
					4,
					4
				},
				new object[]
				{
					"Users",
					1,
					1
				},
				new object[]
				{
					"Foreign Keys",
					4,
					3
				},
				new object[]
				{
					"IndexColumns",
					5,
					4
				},
				new object[]
				{
					"Indexes",
					4,
					3
				},
				new object[]
				{
					"Foreign Key Columns",
					4,
					3
				},
				new object[]
				{
					"UDF",
					1,
					1
				}
			};
            MySqlSchemaCollection expr_201 = new MySqlSchemaCollection("MetaDataCollections");
            expr_201.AddColumn("CollectionName", typeof(string));
            expr_201.AddColumn("NumberOfRestrictions", typeof(int));
            expr_201.AddColumn("NumberOfIdentifierParts", typeof(int));
            SchemaProvider.FillTable(expr_201, data);
            return expr_201;
        }
        private MySqlSchemaCollection GetDataSourceInformation()
        {
            MySqlSchemaCollection expr_0A = new MySqlSchemaCollection("DataSourceInformation");
            expr_0A.AddColumn("CompositeIdentifierSeparatorPattern", typeof(string));
            expr_0A.AddColumn("DataSourceProductName", typeof(string));
            expr_0A.AddColumn("DataSourceProductVersion", typeof(string));
            expr_0A.AddColumn("DataSourceProductVersionNormalized", typeof(string));
            expr_0A.AddColumn("GroupByBehavior", typeof(GroupByBehavior));
            expr_0A.AddColumn("IdentifierPattern", typeof(string));
            expr_0A.AddColumn("IdentifierCase", typeof(IdentifierCase));
            expr_0A.AddColumn("OrderByColumnsInSelect", typeof(bool));
            expr_0A.AddColumn("ParameterMarkerFormat", typeof(string));
            expr_0A.AddColumn("ParameterMarkerPattern", typeof(string));
            expr_0A.AddColumn("ParameterNameMaxLength", typeof(int));
            expr_0A.AddColumn("ParameterNamePattern", typeof(string));
            expr_0A.AddColumn("QuotedIdentifierPattern", typeof(string));
            expr_0A.AddColumn("QuotedIdentifierCase", typeof(IdentifierCase));
            expr_0A.AddColumn("StatementSeparatorPattern", typeof(string));
            expr_0A.AddColumn("StringLiteralPattern", typeof(string));
            expr_0A.AddColumn("SupportedJoinOperators", typeof(SupportedJoinOperators));
            DBVersion version = this.connection.driver.Version;
            string value = string.Format("{0:0}.{1:0}.{2:0}", version.Major, version.Minor, version.Build);
            MySqlSchemaRow mySqlSchemaRow = expr_0A.AddRow();
            mySqlSchemaRow["CompositeIdentifierSeparatorPattern"] = "\\.";
            mySqlSchemaRow["DataSourceProductName"] = "MySQL";
            mySqlSchemaRow["DataSourceProductVersion"] = this.connection.ServerVersion;
            mySqlSchemaRow["DataSourceProductVersionNormalized"] = value;
            mySqlSchemaRow["GroupByBehavior"] = GroupByBehavior.Unrelated;
            mySqlSchemaRow["IdentifierPattern"] = "(^\\`\\p{Lo}\\p{Lu}\\p{Ll}_@#][\\p{Lo}\\p{Lu}\\p{Ll}\\p{Nd}@$#_]*$)|(^\\`[^\\`\\0]|\\`\\`+\\`$)|(^\\\" + [^\\\"\\0]|\\\"\\\"+\\\"$)";
            mySqlSchemaRow["IdentifierCase"] = IdentifierCase.Insensitive;
            mySqlSchemaRow["OrderByColumnsInSelect"] = false;
            mySqlSchemaRow["ParameterMarkerFormat"] = "{0}";
            mySqlSchemaRow["ParameterMarkerPattern"] = "(@[A-Za-z0-9_$#]*)";
            mySqlSchemaRow["ParameterNameMaxLength"] = 128;
            mySqlSchemaRow["ParameterNamePattern"] = "^[\\p{Lo}\\p{Lu}\\p{Ll}\\p{Lm}_@#][\\p{Lo}\\p{Lu}\\p{Ll}\\p{Lm}\\p{Nd}\\uff3f_@#\\$]*(?=\\s+|$)";
            mySqlSchemaRow["QuotedIdentifierPattern"] = "(([^\\`]|\\`\\`)*)";
            mySqlSchemaRow["QuotedIdentifierCase"] = IdentifierCase.Sensitive;
            mySqlSchemaRow["StatementSeparatorPattern"] = ";";
            mySqlSchemaRow["StringLiteralPattern"] = "'(([^']|'')*)'";
            mySqlSchemaRow["SupportedJoinOperators"] = 15;
            expr_0A.Rows.Add(mySqlSchemaRow);
            return expr_0A;
        }
        private static MySqlSchemaCollection GetDataTypes()
        {
            MySqlSchemaCollection expr_0A = new MySqlSchemaCollection("DataTypes");
            expr_0A.AddColumn("TypeName", typeof(string));
            expr_0A.AddColumn("ProviderDbType", typeof(int));
            expr_0A.AddColumn("ColumnSize", typeof(long));
            expr_0A.AddColumn("CreateFormat", typeof(string));
            expr_0A.AddColumn("CreateParameters", typeof(string));
            expr_0A.AddColumn("DataType", typeof(string));
            expr_0A.AddColumn("IsAutoincrementable", typeof(bool));
            expr_0A.AddColumn("IsBestMatch", typeof(bool));
            expr_0A.AddColumn("IsCaseSensitive", typeof(bool));
            expr_0A.AddColumn("IsFixedLength", typeof(bool));
            expr_0A.AddColumn("IsFixedPrecisionScale", typeof(bool));
            expr_0A.AddColumn("IsLong", typeof(bool));
            expr_0A.AddColumn("IsNullable", typeof(bool));
            expr_0A.AddColumn("IsSearchable", typeof(bool));
            expr_0A.AddColumn("IsSearchableWithLike", typeof(bool));
            expr_0A.AddColumn("IsUnsigned", typeof(bool));
            expr_0A.AddColumn("MaximumScale", typeof(short));
            expr_0A.AddColumn("MinimumScale", typeof(short));
            expr_0A.AddColumn("IsConcurrencyType", typeof(bool));
            expr_0A.AddColumn("IsLiteralSupported", typeof(bool));
            expr_0A.AddColumn("LiteralPrefix", typeof(string));
            expr_0A.AddColumn("LiteralSuffix", typeof(string));
            expr_0A.AddColumn("NativeDataType", typeof(string));
            MySqlBit.SetDSInfo(expr_0A);
            MySqlBinary.SetDSInfo(expr_0A);
            MySqlDateTime.SetDSInfo(expr_0A);
            MySqlTimeSpan.SetDSInfo(expr_0A);
            MySqlString.SetDSInfo(expr_0A);
            MySqlDouble.SetDSInfo(expr_0A);
            MySqlSingle.SetDSInfo(expr_0A);
            MySqlByte.SetDSInfo(expr_0A);
            MySqlInt16.SetDSInfo(expr_0A);
            MySqlInt32.SetDSInfo(expr_0A);
            MySqlInt64.SetDSInfo(expr_0A);
            MySqlDecimal.SetDSInfo(expr_0A);
            MySqlUByte.SetDSInfo(expr_0A);
            MySqlUInt16.SetDSInfo(expr_0A);
            MySqlUInt32.SetDSInfo(expr_0A);
            MySqlUInt64.SetDSInfo(expr_0A);
            return expr_0A;
        }
        protected virtual MySqlSchemaCollection GetRestrictions()
        {
            object[][] data = new object[][]
			{
				new object[]
				{
					"Users",
					"Name",
					"",
					0
				},
				new object[]
				{
					"Databases",
					"Name",
					"",
					0
				},
				new object[]
				{
					"Tables",
					"Database",
					"",
					0
				},
				new object[]
				{
					"Tables",
					"Schema",
					"",
					1
				},
				new object[]
				{
					"Tables",
					"Table",
					"",
					2
				},
				new object[]
				{
					"Tables",
					"TableType",
					"",
					3
				},
				new object[]
				{
					"Columns",
					"Database",
					"",
					0
				},
				new object[]
				{
					"Columns",
					"Schema",
					"",
					1
				},
				new object[]
				{
					"Columns",
					"Table",
					"",
					2
				},
				new object[]
				{
					"Columns",
					"Column",
					"",
					3
				},
				new object[]
				{
					"Indexes",
					"Database",
					"",
					0
				},
				new object[]
				{
					"Indexes",
					"Schema",
					"",
					1
				},
				new object[]
				{
					"Indexes",
					"Table",
					"",
					2
				},
				new object[]
				{
					"Indexes",
					"Name",
					"",
					3
				},
				new object[]
				{
					"IndexColumns",
					"Database",
					"",
					0
				},
				new object[]
				{
					"IndexColumns",
					"Schema",
					"",
					1
				},
				new object[]
				{
					"IndexColumns",
					"Table",
					"",
					2
				},
				new object[]
				{
					"IndexColumns",
					"ConstraintName",
					"",
					3
				},
				new object[]
				{
					"IndexColumns",
					"Column",
					"",
					4
				},
				new object[]
				{
					"Foreign Keys",
					"Database",
					"",
					0
				},
				new object[]
				{
					"Foreign Keys",
					"Schema",
					"",
					1
				},
				new object[]
				{
					"Foreign Keys",
					"Table",
					"",
					2
				},
				new object[]
				{
					"Foreign Keys",
					"Constraint Name",
					"",
					3
				},
				new object[]
				{
					"Foreign Key Columns",
					"Catalog",
					"",
					0
				},
				new object[]
				{
					"Foreign Key Columns",
					"Schema",
					"",
					1
				},
				new object[]
				{
					"Foreign Key Columns",
					"Table",
					"",
					2
				},
				new object[]
				{
					"Foreign Key Columns",
					"Constraint Name",
					"",
					3
				},
				new object[]
				{
					"UDF",
					"Name",
					"",
					0
				}
			};
            MySqlSchemaCollection expr_4BD = new MySqlSchemaCollection("Restrictions");
            expr_4BD.AddColumn("CollectionName", typeof(string));
            expr_4BD.AddColumn("RestrictionName", typeof(string));
            expr_4BD.AddColumn("RestrictionDefault", typeof(string));
            expr_4BD.AddColumn("RestrictionNumber", typeof(int));
            SchemaProvider.FillTable(expr_4BD, data);
            return expr_4BD;
        }
        private static MySqlSchemaCollection GetReservedWords()
        {
            MySqlSchemaCollection mySqlSchemaCollection = new MySqlSchemaCollection("ReservedWords");
            mySqlSchemaCollection.AddColumn(DbMetaDataColumnNames.ReservedWord, typeof(string));
            Stream manifestResourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("MySql.Data.MySqlClient.Properties.ReservedWords.txt");
            StreamReader streamReader = new StreamReader(manifestResourceStream);
            for (string text = streamReader.ReadLine(); text != null; text = streamReader.ReadLine())
            {
                string[] array = text.Split(new char[]
				{
					' '
				});
                for (int i = 0; i < array.Length; i++)
                {
                    string value = array[i];
                    if (!string.IsNullOrEmpty(value))
                    {
                        mySqlSchemaCollection.AddRow()[0] = value;
                    }
                }
            }
            streamReader.Dispose();
            manifestResourceStream.Close();
            return mySqlSchemaCollection;
        }
        protected static void FillTable(MySqlSchemaCollection dt, object[][] data)
        {
            for (int i = 0; i < data.Length; i++)
            {
                object[] array = data[i];
                MySqlSchemaRow mySqlSchemaRow = dt.AddRow();
                for (int j = 0; j < array.Length; j++)
                {
                    mySqlSchemaRow[j] = array[j];
                }
            }
        }
        private void FindTables(MySqlSchemaCollection schema, string[] restrictions)
        {
            StringBuilder stringBuilder = new StringBuilder();
            StringBuilder stringBuilder2 = new StringBuilder();
            stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "SHOW TABLE STATUS FROM `{0}`", new object[]
			{
				restrictions[1]
			});
            if (restrictions != null && restrictions.Length >= 3 && restrictions[2] != null)
            {
                stringBuilder2.AppendFormat(CultureInfo.InvariantCulture, " LIKE '{0}'", new object[]
				{
					restrictions[2]
				});
            }
            stringBuilder.Append(stringBuilder2.ToString());
            string value = (restrictions[1].ToLower() == "information_schema") ? "SYSTEM VIEW" : "BASE TABLE";
            using (MySqlDataReader mySqlDataReader = new MySqlCommand(stringBuilder.ToString(), this.connection).ExecuteReader())
            {
                while (mySqlDataReader.Read())
                {
                    MySqlSchemaRow expr_A4 = schema.AddRow();
                    expr_A4["TABLE_CATALOG"] = null;
                    expr_A4["TABLE_SCHEMA"] = restrictions[1];
                    expr_A4["TABLE_NAME"] = mySqlDataReader.GetString(0);
                    expr_A4["TABLE_TYPE"] = value;
                    expr_A4["ENGINE"] = SchemaProvider.GetString(mySqlDataReader, 1);
                    expr_A4["VERSION"] = mySqlDataReader.GetValue(2);
                    expr_A4["ROW_FORMAT"] = SchemaProvider.GetString(mySqlDataReader, 3);
                    expr_A4["TABLE_ROWS"] = mySqlDataReader.GetValue(4);
                    expr_A4["AVG_ROW_LENGTH"] = mySqlDataReader.GetValue(5);
                    expr_A4["DATA_LENGTH"] = mySqlDataReader.GetValue(6);
                    expr_A4["MAX_DATA_LENGTH"] = mySqlDataReader.GetValue(7);
                    expr_A4["INDEX_LENGTH"] = mySqlDataReader.GetValue(8);
                    expr_A4["DATA_FREE"] = mySqlDataReader.GetValue(9);
                    expr_A4["AUTO_INCREMENT"] = mySqlDataReader.GetValue(10);
                    expr_A4["CREATE_TIME"] = mySqlDataReader.GetValue(11);
                    expr_A4["UPDATE_TIME"] = mySqlDataReader.GetValue(12);
                    expr_A4["CHECK_TIME"] = mySqlDataReader.GetValue(13);
                    expr_A4["TABLE_COLLATION"] = SchemaProvider.GetString(mySqlDataReader, 14);
                    expr_A4["CHECKSUM"] = mySqlDataReader.GetValue(15);
                    expr_A4["CREATE_OPTIONS"] = SchemaProvider.GetString(mySqlDataReader, 16);
                    expr_A4["TABLE_COMMENT"] = SchemaProvider.GetString(mySqlDataReader, 17);
                }
            }
        }
        private static string GetString(MySqlDataReader reader, int index)
        {
            if (reader.IsDBNull(index))
            {
                return null;
            }
            return reader.GetString(index);
        }
        public virtual MySqlSchemaCollection GetUDF(string[] restrictions)
        {
            string text = "SELECT name,ret,dl FROM mysql.func";
            if (restrictions != null && restrictions.Length >= 1 && !string.IsNullOrEmpty(restrictions[0]))
            {
                text += string.Format(" WHERE name LIKE '{0}'", restrictions[0]);
            }
            MySqlSchemaCollection mySqlSchemaCollection = new MySqlSchemaCollection("User-defined Functions");
            mySqlSchemaCollection.AddColumn("NAME", typeof(string));
            mySqlSchemaCollection.AddColumn("RETURN_TYPE", typeof(int));
            mySqlSchemaCollection.AddColumn("LIBRARY_NAME", typeof(string));
            MySqlCommand mySqlCommand = new MySqlCommand(text, this.connection);
            try
            {
                using (MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader())
                {
                    while (mySqlDataReader.Read())
                    {
                        MySqlSchemaRow expr_96 = mySqlSchemaCollection.AddRow();
                        expr_96[0] = mySqlDataReader.GetString(0);
                        expr_96[1] = mySqlDataReader.GetInt32(1);
                        expr_96[2] = mySqlDataReader.GetString(2);
                    }
                }
            }
            catch (MySqlException ex)
            {
                if (ex.Number != 1142)
                {
                    throw;
                }
                throw new MySqlException(Resources.UnableToEnumerateUDF, ex);
            }
            return mySqlSchemaCollection;
        }
        protected virtual MySqlSchemaCollection GetSchemaInternal(string collection, string[] restrictions)
		{
            switch (collection)
            {
                // common collections
                case "METADATACOLLECTIONS":
                    return GetCollections();
                case "DATASOURCEINFORMATION":
                    return GetDataSourceInformation();
                case "DATATYPES":
                    return GetDataTypes();
                case "RESTRICTIONS":
                    return GetRestrictions();
                case "RESERVEDWORDS":
                    return GetReservedWords();

                // collections specific to our provider
                case "USERS":
                    return GetUsers(restrictions);
                case "DATABASES":
                    return GetDatabases(restrictions);
                case "UDF":
                    return GetUDF(restrictions);
            }

            // if we have a current database and our users have
            // not specified a database, then default to the currently
            // selected one.
            if (restrictions == null)
                restrictions = new string[2];
            if (connection != null &&
              connection.Database != null &&
              connection.Database.Length > 0 &&
              restrictions.Length > 1 &&
              restrictions[1] == null)
                restrictions[1] = connection.Database;

            switch (collection)
            {
                case "TABLES":
                    return GetTables(restrictions);
                case "COLUMNS":
                    return GetColumns(restrictions);
                case "INDEXES":
                    return GetIndexes(restrictions);
                case "INDEXCOLUMNS":
                    return GetIndexColumns(restrictions);
                case "FOREIGN KEYS":
                    return GetForeignKeys(restrictions);
                case "FOREIGN KEY COLUMNS":
                    return GetForeignKeyColumns(restrictions);
            }
            return null;
		}
        internal string[] CleanRestrictions(string[] restrictionValues)
        {
            string[] array = null;
            if (restrictionValues != null)
            {
                array = (string[])restrictionValues.Clone();
                for (int i = 0; i < array.Length; i++)
                {
                    string text = array[i];
                    if (text != null)
                    {
                        array[i] = text.Trim(new char[]
						{
							'`'
						});
                    }
                }
            }
            return array;
        }
        protected MySqlSchemaCollection QueryCollection(string name, string sql)
        {
            MySqlSchemaCollection mySqlSchemaCollection = new MySqlSchemaCollection(name);
            MySqlDataReader mySqlDataReader = new MySqlCommand(sql, this.connection).ExecuteReader();
            for (int i = 0; i < mySqlDataReader.FieldCount; i++)
            {
                mySqlSchemaCollection.AddColumn(mySqlDataReader.GetName(i), mySqlDataReader.GetFieldType(i));
            }
            using (mySqlDataReader)
            {
                while (mySqlDataReader.Read())
                {
                    MySqlSchemaRow mySqlSchemaRow = mySqlSchemaCollection.AddRow();
                    for (int j = 0; j < mySqlDataReader.FieldCount; j++)
                    {
                        mySqlSchemaRow[j] = mySqlDataReader.GetValue(j);
                    }
                }
            }
            return mySqlSchemaCollection;
        }
    }
}
