/*
Parallelism broadly means achieving concurrency by distributing work across multiple CPUs.

The subtle difference betwwen concurrency and parallelism is that concurrency means that the system
is able to advance multiple tasks indipendently while parallelism advance them at the same exact time.

So, concurrency is about dealing with lots of things at once while parallelism is about doing lots of things at once.

Async describes how individual threads are used. Async is a programming model. It can be implemented without threads,
I believe .NET implements with threading, Node.js for example uses a single thread with an event loop to achieve async.

Let me summary them in a story:

∙ Bob started a restaurant and he does all the thing: Being a chef, being a waiter and cashier. this kind of system is non concurrency.

∙ More and more customers come. Bob decided to hire 1 Chef, 1 Waiter and 1 Cashier. He just enjoys to do the management things.
  Now, at the same time, Waiter gets order while Cashier collects payment from another customer: Now we have a *concurrency * system.

∙ The number of customer continues being increase. 1 waiter is not enough. Bob decided to get another waiter. 
  Waiter 1 gets order from customer table number 1 to 10, waiter 2 gets order from table number 11 to 20.
  Getting order is now divided between 2 waiter. Now we have *Parallel * system.

∙ Waiter 1 after getting the order, he bring the order to the kitchen and wait for the food. He just standing there and keep waiting
  for the Chef. After the food is ready, he bring the food to his customer and getting next order. Bob doesn't want to do this way, he
  ask waiter 1 stop waiting, After the food is ready, the chef will inform and either waiter 1 or waiter 2 can bring it to the customer.
  Now we have Async.

Concurrency scales better when you are waiting for things to complete (typically IO), so you can yield threads to do other work while
that waiting is happening instead of just sitting idle. Parallelism (multi-threading) scales better when you have computationally
expensive work where you cannot yield the threads back.

https://www.youtube.com/watch?v=lHuyl_WTpME

*/
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;

using Test;

namespace TestAsync {
	public static class Tester {
		static Stopwatch _sw;
		public static void Go(string directory_a)
		{
			Console.WriteLine($"\n--- TestAsync {new String('-', 50)}\n");

			_sw = Stopwatch.StartNew();
			var taskA = GrindCoffeeTask();
			var taskB = BoilWaterTask();
			var taskC = PrepareToastsTask();
			try {
				Task.WaitAll(taskA, taskB, taskC);
			} catch (AggregateException ae) {
				Console.ForegroundColor = ConsoleColor.Red;
				ae.Handle((x) => {
					Console.WriteLine($"{_sw.Elapsed.TotalSeconds:0.000}: {x.Message}");
					return true; // Handeled
				});
			} catch (Exception e) {
				Console.WriteLine($"Exception: {e.Message}");
			} finally {
				Console.ForegroundColor = (ConsoleColor)Enum.Parse(typeof(ConsoleColor), Program.config.GetValue<string>("ConsoleForegroundColor"), true);
				//Console.ResetColor();
				var results = new List<string>();
				Console.Write($"{_sw.Elapsed.TotalSeconds:0.000}: ");
				if (taskA.IsCompletedSuccessfully) results.Add(taskA.Result.ToString());
				if (taskB.IsCompletedSuccessfully) results.Add(taskB.Result.ToString());
				if (taskC.IsCompletedSuccessfully) results.Add(taskC.Result.ToString());
				if (results.Count > 0) Console.WriteLine(String.Join(", ", results.ToArray()));
				else Console.WriteLine("No breakfast today");
			}

			// https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/async/
			Task task = new Task(CallMethod);
			task.Start();
			task.Wait();

			Console.WriteLine($"\n--- TestParallelism {new String('-', 50)}\n");
			if (!Directory.Exists(directory_a)) {
         	Console.WriteLine($"The directory {directory_a} does not exist.");
         	return;
      	}

			String[] files = Directory.GetFiles(directory_a, "*.cs");
			long totalSize = 0;
			Parallel.For (0, files.Length, index => { FileInfo fi = new FileInfo(files[index]);
				long size = fi.Length;
				Interlocked.Add(ref totalSize, size);
			});
			Console.WriteLine("Directory '{0}':", directory_a);
			Console.WriteLine("{0:N0} files, {1:N0} bytes", files.Length, totalSize);
		}
		static async Task<uint> GrindCoffeeTask()
		{
			await Task.Delay(1000);
			throw new Exception("Grinder is broken!");
			await Task.Delay(1000);
			Console.WriteLine($"{_sw.Elapsed.TotalSeconds:0.000}: Grind coffee task is done");
			return 1u;
		}
		static async Task<string> BoilWaterTask()
		{
			//throw new Exception("No water to be boiled!");
			await Task.Delay(3000);
			Console.WriteLine($"{_sw.Elapsed.TotalSeconds:0.000}: Boil water task is done");
			return "Water is now at 96 °C";
		}
		static async Task<string> PrepareToastsTask()
		{
			//throw new Exception("No bread!");
			await Task.Delay(4000);
			Console.WriteLine($"{_sw.Elapsed.TotalSeconds:0.000}: Prepare toasts task is done");
			return "Two toasts are ready";
		}
		static async void CallMethod()
		{
			string filePath = "Program.cs";
			Task<int> task = ReadFile(filePath);

			Console.WriteLine("Other Work 1");
			Console.WriteLine("Other Work 2");
			Console.WriteLine("Other Work 3");

			int length = await task;
			Console.WriteLine($"Total length: {length}");
			Console.WriteLine("After work 1");
			Console.WriteLine("After work 2");
		}

		static async Task<int> ReadFile(string file)
		{
			int length = 0;

			Console.WriteLine($"Reading {file}");
			using (StreamReader reader = new StreamReader(file)) {
				string s = await reader.ReadToEndAsync();
				length = s.Length;
			}
			Console.WriteLine("File reading is completed");
			return length;
		}
	}
}
