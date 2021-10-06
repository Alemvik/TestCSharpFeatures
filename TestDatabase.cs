// https://www.chubbydeveloper.com/ado-net-vs-entity-framework/
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;

using Emvie;

namespace TestDatabase {
	class Tester {
		public static void Go()
		{
			var msg = "TestDatabase";
			Console.WriteLine($"\n--- {msg} {new String('-', Math.Max(65-msg.Length,3))}\n");

			{  // MySql database
            DbProviderFactories.RegisterFactory("MySql.Data.MySqlClient", MySql.Data.MySqlClient.MySqlClientFactory.Instance);
            //var tbl = Db.Exec2Dt("select * from $User$");
            //foreach (DataRow dr in tbl.Rows) Console.WriteLine($"{dr[0]}\t\t{dr[1]}");
			}
		}
	}
}