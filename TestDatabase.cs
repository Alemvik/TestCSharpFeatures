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

			{  // Register the ADO.net adapter for MySql (using MySql namespace)
            DbProviderFactories.RegisterFactory("MySql", MySql.Data.MySqlClient.MySqlClientFactory.Instance);
				// Register the ADO.net adapter for Microsoft Sql Server
				// DbProviderFactories.RegisterFactory("SqlSrv", System.Data.SqlClient.SqlClientFactory.Instance);
				// Register the ADO.net adapter for Oracle
				// DbProviderFactories.RegisterFactory("Oracle", Oracle.ManageDataAccess.Client.MySqlClientFactory.Instance);

         	Db.Init(new Emvie.DataSource[] {new ("MySql","MySql","DataSourcE=localhost;port=3306;Database=Skillango;uid=root;pwd=1111qqqq")});
            var tbl = Db.ToTbl("MySql","select Name, Manager, Title from Skillango.User order by Name limit 15;");
            foreach (DataRow dr in tbl.Rows) Console.WriteLine($"{dr[0]}{new String(' ', Math.Max(40-((string)dr[0]).Length,3))}{dr[1]}{new String(' ', Math.Max(30-((string)dr[1]).Length,3))}{dr[2]}");

            Db.BeginTransaction("MySql");
            int i = (int)Db.ExecNonQry("MySql",0,"update Skillango.User set Title='Cyber Security Specialist' where Name='Alain Trépanier (6036527)';",0);
            Db.CommitTransaction("MySql");

            long count = (long)Db.ExecScalar("MySql",0,"select count(*) from Skillango.User where Name like @NameMask;",0,"@NameMask","a%");

            Console.WriteLine($"\n{i};{Db.ServerAndDbName("MySql")};count={count}");
			}
		}
	}
}