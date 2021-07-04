using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;

using Test;

namespace TestAsync {
    public static class Tester {
        static Stopwatch _sw;
        public static void Go() {
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
        }
        static async Task<uint> GrindCoffeeTask() {
            await Task.Delay(1000);
            throw new Exception("Grinder is broken!");
            await Task.Delay(1000);
            Console.WriteLine($"{_sw.Elapsed.TotalSeconds:0.000}: Grind coffee task is done");
            return 1u;
        }
        static async Task<string> BoilWaterTask() {
            //throw new Exception("No water to be boiled!");
            await Task.Delay(3000);
            Console.WriteLine($"{_sw.Elapsed.TotalSeconds:0.000}: Boil water task is done");
            return "Water is now at 96 °C";
        }
        static async Task<string> PrepareToastsTask() {
            //throw new Exception("No bread!");
            await Task.Delay(4000);
            Console.WriteLine($"{_sw.Elapsed.TotalSeconds:0.000}: Prepare toasts task is done");
            return "Two toasts are ready";
        }
        static async void CallMethod() {
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

        static async Task<int> ReadFile(string file) {
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
