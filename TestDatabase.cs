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

			{  // Register the ADO.net adapter for MySql database
            DbProviderFactories.RegisterFactory("MySql", MySql.Data.MySqlClient.MySqlClientFactory.Instance);

				Db.Init(new Emvie.DataSource[] {new ("MySqlA","MySql","DataSource=localhost;port=3306;Database=Skillango;uid=root;pwd=1111qqqq")});

				//var db = Db.Instance("MySql.Data.MySqlClient","DataSource=localhost;port=3306;Database=Skillango;uid=root;pwd=1111qqqq");
				//Console.WriteLine(db.ServerAndDbName);
            var tbl = Db.ToTbl("MySqlA","select * from User where name like '%al%' order by Name limit 10;");
            foreach (DataRow dr in tbl.Rows) Console.WriteLine($"{dr[0]}{new string(' ',35-dr[0].ToString().Length)}{dr[1]}");
			}
		}
	}
}