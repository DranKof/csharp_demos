/*
	Multithreading demo by Darren Stults

	This demonstrates simple single-threaded processes against similar
	multithreaded calls and then demonstrates how to increase the complexity
	of the multithreaded calls in increasingly realistic scenarios.

	This was originally based off the demo by Mark J. Price:
	https://github.com/markjprice/cs8dotnetcore3/blob/master/Chapter13/WorkingWithTasks/Program.cs

	I refactored most of it and expanded it to demonstrate a broader scope.
	Namely I wanted to make particularly clear the differences between the standard vs. lambda calls
	for anyone that wasn't versed in those and then also show greater clarity that multi-tasking is
	indeed occuring with the final "real world" chained process example, which furthermore requires
	passing individually declared parameters when tasks are simultaneously initialized in the same
	for-loop as they would not be called one-by-one as they are in the simplified versions.
*/

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using static System.Console;

namespace Multitasking
{
	class Program
	{

		static char sampleMethodName = 'C'; // used to cycle between names in no-arg demo
		static void SampleMethod()
		{
			int msTime = 0;
			switch (sampleMethodName)
			{
				case 'C':
					sampleMethodName = 'A';
					msTime = 3000;
					break;
				case 'A':
					sampleMethodName = 'B';
					msTime = 2000;
					break;
				case 'B':
					sampleMethodName = 'C';
					msTime = 1000;
					break;
			}
			SampleMethod(sampleMethodName, msTime);
		}

		static void SampleMethod(char name, int sleepTime)
		{
			Write($" [START {name}]");
			Thread.Sleep(sleepTime); // simulate three seconds of work 
			Write($" [FINISH {name}]");
		}

		static decimal CallWebService()
		{
			return CallWebService(1);
		}
		static decimal CallWebService(int num)
		{
			WriteLine($"Starting call to Web Service {num}...");
			Thread.Sleep((new Random()).Next(2000, 4000));
			WriteLine($"Finished call to Web Service {num}.");
			return ((int)(10000 * (new Random()).NextDouble()) / 100);
		}

		static string CallStoredProcedure(int num, decimal amount)
		{
			WriteLine($"Starting call to Stored Procedure {num}...");
			Thread.Sleep((new Random()).Next(2000, 4000));
			WriteLine($"Finished call to stored procedure {num}.");
			return $"{(new Random()).Next(5, 20)} products cost more than {amount:C}.";
		}

		static void Main(string[] args)
		{
			WriteLine("Multitasking Demo");
			WriteLine($"=======================");

			WriteLine();
			WriteLine("1) First we will see 3 tasks running one after another.");
			WriteLine("-----------------------");
			var timer = Stopwatch.StartNew();
			TestSingleThreading();
			WriteLine();
			WriteLine($"{timer.ElapsedMilliseconds:#,##0}ms elapsed.");

			WriteLine();
			WriteLine("2) Same three tasks running simultaneously, without arguments in method calls.");
			WriteLine("-----------------------");
			timer.Restart();
			TestMultithreadingNoArgs();
			WriteLine();
			WriteLine($"{timer.ElapsedMilliseconds:#,##0}ms elapsed.");

			WriteLine();
			WriteLine("3) Same three tasks running simultaneously, but with arguments (requires lambda expressions).");
			WriteLine("-----------------------");
			timer.Restart();
			TestMultithreadingWithArgs();
			WriteLine();
			WriteLine($"{timer.ElapsedMilliseconds:#,##0}ms elapsed.");

			WriteLine();
			WriteLine("4) REAL WORLD EMULATION - arrays of tasks that spawn tasks:");
			WriteLine("   This emulates tasks that immediately chain secondary processes when the first are done.");
			WriteLine("-----------------------");
			timer.Restart();
			ShowTaskProcessChainingDemo();
			WriteLine();
			WriteLine($"{timer.ElapsedMilliseconds:#,##0}ms elapsed.");

		}

		static void TestSingleThreading()
		{
			SampleMethod('A', 3000);
			SampleMethod('B', 2000);
			SampleMethod('C', 1000);
		}

		static void TestMultithreadingNoArgs()
		{
			// THREE WAYS TO DO THE EXACT SAME THING
			// 1
			Task taskA = new Task(SampleMethod);
			taskA.Start();
			// 2
			Task taskB = Task.Factory.StartNew(SampleMethod);
			// 3
			Task taskC = Task.Run(new Action(SampleMethod));
			// create array to store all tasks
			Task[] tasks = { taskA, taskB, taskC };
			// wait for all tasks to complete
			Task.WaitAll(tasks);
		}

		static void TestMultithreadingWithArgs()
		{
			// SAME THREE WAYS, BUT WITH LAMBDA EXPRESSION ARGUMENTS
			Task taskA = new Task(() => SampleMethod('A', 3000));
			taskA.Start();
			Task taskB = Task.Factory.StartNew(() => SampleMethod('B', 2000));
			Task taskC = Task.Run(new Action(() => SampleMethod('C', 1000)));
			Task[] tasks = { taskA, taskB, taskC };
			Task.WaitAll(tasks);
		}

		static void ShowTaskProcessChainingDemo()
		{

			// Create a task array with final return value string returned
			//     from the final method run in process chain
			Task<string>[] tasks = new Task<string>[5];

			// Sample with no parameters in StartNew
			tasks[0] = Task.Factory.StartNew(CallWebService)
					.ContinueWith(previousTask => CallStoredProcedure(1, previousTask.Result));

			// Sample with parameters in first StartNew
			for (int i = 1; i < 5; i++)
			{
				int t = i + 1; // Factory tasks won't pass values until the iterator is done, so we have to create unique
					       // pointers for each task during the for loop, otherwise they will all be "Task 5".
				tasks[i] = Task.Factory.StartNew(() => CallWebService(t))
					 .ContinueWith(previousTask => CallStoredProcedure(t, previousTask.Result));
			}

			Task.WaitAll(tasks);
			for (int i = 0; i < 5; i++)
			{
				WriteLine($"Results from {i + 1}: {tasks[i].Result}");
			}

		}

	}
}
