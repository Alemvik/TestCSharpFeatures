// https://channel9.msdn.com/Blogs/dotnet/Get-started-VSCode-Csharp-NET-Core-Windows
using System;
using System.Collections.Generic;
using System.Globalization;
//using System.Runtime.InteropServices;

using Alemvik;

namespace TestBestPath;

class Tester {
	public static void Go(int width=10, int height=10)
	{
		var msg = "TestBestPath";
		Console.WriteLine($"\n--- {msg} {new String('-', Math.Max(65-msg.Length,3))}\n");

			int[,] matrix;

			matrix = new int[width,height];
			Random rnd = new();
			for (int y=0; y<width ;y++) for (int x=0; x<height ;x++) matrix[y,x] = rnd.Next(10); // random int betwwen 0 inclusively and 9 inclusively

			/*var sw2 = Stopwatch.StartNew();
			var (risk2, path2) = FindBestPath2(matrix,start);
			sw2.Stop();
			Console.Write('\n');PrintMatrix(matrix,risk2,path2,sw2);*/

			(int y,int x) srcLoc = (0,0);
			(int y,int x) dstLoc = (matrix.GetLength(0)-1,matrix.GetLength(1)-1);
			var sw1 = Stopwatch.StartNew();
			var (risk, path) = FindBestPath(matrix, srcLoc, dstLoc);

			//if (int.TryParse(args[0], out int ghr) && (risk2 - risk < 3 /*|| path.Count == path2.Count*/)) goto A;

			Console.Write('\n'); PrintMatrix(matrix,risk,path,sw1);
	}

	public static void Go(string fileName)
	{
		var msg = "TestBestPath";
		Console.WriteLine($"\n--- {msg} {new String('-', Math.Max(65-msg.Length,3))}\n");

		int[,] matrix;

		var lines = System.IO.File.ReadAllText(fileName).Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
		int height = lines.Length;
		int width = lines[0].Length;
		matrix = new int[height,width];
		unsafe { fixed (int* p = matrix) new Span<int>(p, matrix.Length).Fill(int.MaxValue);} // requires to be inside unsafe methods or unsabe block
		for (int y=0; y<height ;y++) for (int x=0; x<width ;x++) if (lines[y][x]>='0' && lines[y][x]<='9') matrix[y,x] = lines[y][x] - '0';

		var matrix5 = new int[height*5,width*5];
		for (int y=0; y<height ;y++) for (int x=0; x<width ;x++) matrix5[y,x] = matrix[y,x];
		for (int i=0; i<4 ;i++) for (int y=0; y<height ;y++) for (int x=0; x<width ;x++) matrix5[(i+1)*height+y,x] = matrix5[i*height+y,x] + 1  == 10 ? 1 : matrix5[i*height+y,x] + 1;
		for (int i=0; i<5 ;i++) for (int j=0; j<4 ;j++) for (int y=0; y<height ;y++) for (int x=0; x<width ;x++) matrix5[i*height+y,(j+1)*width+x] = matrix5[i*height+y,j*width+x] + 1 == 10 ? 1 : matrix5[i*height+y,j*width+x] + 1;
		//matrix = matrix5;
		/*for (int y=0; y<height*5 ;y++) {
			for (int x=0; x<width*5 ;x++) Console.Write(matrix5[y,x]);
			Console.Write('\n');
		}*/

		/*var sw2 = Stopwatch.StartNew();
		var (risk2, path2) = FindBestPath2(matrix,start);
		sw2.Stop();
		Console.Write('\n');PrintMatrix(matrix,risk2,path2,sw2);*/

		(int y,int x) srcLoc = (0,0);
		(int y,int x) dstLoc = (matrix.GetLength(0)-1,matrix.GetLength(1)-1);
		var sw1 = Stopwatch.StartNew();
		var (risk, path) = FindBestPath(matrix, srcLoc, dstLoc);

		//if (int.TryParse(args[0], out int ghr) && (risk2 - risk < 3 /*|| path.Count == path2.Count*/)) goto A;

		Console.Write('\n'); PrintMatrix(matrix,risk,path,sw1);
	}

	/*
	static (int risk, List<(int y,int x)> path) FindBestPath2(int[,] matrix, int start=0)
	{
		int wh = matrix.GetLength(0);
		if (wh != matrix.GetLength(1)) throw new ArgumentException("matrix must be square");
		if (!start.Between(-wh+1,wh-1)) throw new ArgumentException($"start must be between {-wh+1} inclusively and {wh-1} inclusively");

		//var sw = Stopwatch.StartNew();
		var perimeters = new (List<(int y,int x)> path, int risk)[3,2*wh-1]; //Array.Fill(horA,(null,0));
		var innerPerimeterIx = 0;
		var outterPerimeterIx = 1;
		var outterCopyIx = 2;
		perimeters[innerPerimeterIx,0] = (new List<(int y,int x)>() {(wh-1,wh-1)},matrix[wh-1,wh-1]);
		for (int x=0; x<wh-1 ;x++) {// with wh=5: 0123
			int innerPerimeterLength = 2 * x + 1; // with wh=5: 1,3,5,7
			int outterPerimeterLength = innerPerimeterLength + 2;
			int innerPerimeterWidth = (innerPerimeterLength + 1) / 2;
			int outterPerimeterWidth = (outterPerimeterLength + 1) / 2;
			(int y, int x) innerPerimeterLocation = (wh-innerPerimeterWidth,wh-innerPerimeterWidth);
			(int y, int x) outterPerimeterLocation = (innerPerimeterLocation.y-1,innerPerimeterLocation.x-1);

			for (int i=0; i<outterPerimeterWidth ;i++) perimeters[outterCopyIx,i].risk = matrix[wh-1-i,wh-x-2];
			for (int i=0; i<x+1 ;i++) perimeters[outterCopyIx,outterPerimeterWidth+i].risk = matrix[wh-outterPerimeterWidth,wh-1-x+i];

			var io = new int[outterPerimeterLength];
			io[outterPerimeterWidth] = io[outterPerimeterWidth-1] = io[outterPerimeterWidth-2] = innerPerimeterWidth-1;
			for (int i=outterPerimeterWidth+1; i<outterPerimeterLength ;i++) io[i] = io[i-1]+1;
			for (int i=outterPerimeterWidth-3; i>=0 ;i--) io[i] = io[i+1]-1;

			for (int o=0; o<outterPerimeterLength ;o++) { // botomleft to upperright
				perimeters[outterPerimeterIx,o].risk = int.MaxValue;
				int ii=0;
				for (int i=0; i<innerPerimeterLength ;i++) {
					int r = perimeters[outterCopyIx,o].risk + perimeters[innerPerimeterIx,i].risk;
					var path = new List<(int y, int x)> {
						Location(outterPerimeterLocation, outterPerimeterWidth, o)
					};

					// Reached the two lefttop corners
					if (i==innerPerimeterWidth-1 && o==outterPerimeterWidth-1) {
						if (perimeters[outterCopyIx,o-1].risk <= perimeters[outterCopyIx,o+1].risk) {
							r += perimeters[outterCopyIx,o-1].risk;
							path.Add((outterPerimeterLocation.y+1,outterPerimeterLocation.x));
						} else {
							r += perimeters[outterCopyIx,o+1].risk;
							path.Add((outterPerimeterLocation.y,outterPerimeterLocation.x+1));
						}
					} else {
						int l = (o>=outterPerimeterWidth ? Array.LastIndexOf(io,i) : Array.IndexOf(io,i)) - o;
						if (l>0) for (int k=0; k<l ;k++) {r += perimeters[outterCopyIx,o+k+1].risk; path.Add(Location(outterPerimeterLocation,outterPerimeterWidth,o+k+1));}
						else for (int k=0; k<-l ;k++) {r += perimeters[outterCopyIx,o-k-1].risk; path.Add(Location(outterPerimeterLocation,outterPerimeterWidth,o-k-1));}
					}

					if (r < perimeters[outterPerimeterIx,o].risk || (r == perimeters[outterPerimeterIx,o].risk && path.Count < perimeters[outterPerimeterIx,o].path.Count)) {
						perimeters[outterPerimeterIx,o].risk = r;
						perimeters[outterPerimeterIx,o].path = path;
						ii=i;
					}
				}

				perimeters[outterPerimeterIx,o].path.AddRange(perimeters[innerPerimeterIx,ii].path);
			}

// for (int k=0; k<innerPerimeterLength ;k++) Console.Write($"{perimeters[innerPerimeterIx,k].risk} ");Console.Write(" ==> ");
// for (int k=0; k<outterPerimeterLength ;k++) Console.Write($"{perimeters[outterCopyIx,k].risk} ");Console.Write(" ==> ");
// for (int k=0; k<outterPerimeterLength ;k++) Console.Write($"{perimeters[outterPerimeterIx,k].risk} ");Console.Write('\n');

			innerPerimeterIx ^= 1;
			outterPerimeterIx ^= 1;
		}

		var startIx = start<0 ? wh+start-1 : wh+start-1;
/ *		for (int y=0; y<wh ;y++) {
			for (int x=0; x<wh ;x++) if (matrix[y,x]==int.MaxValue) Console.Write("￭"); else if (perimeters[innerPerimeterIx,startIx].path.Contains((y,x))) {if (Console.IsOutputRedirected) Console.Write('●'); else {Console.ForegroundColor = ConsoleColor.Cyan; Console.Write($"{matrix[y,x],1}"); Console.ForegroundColor = ConsoleColor.White;}} else Console.Write($"{matrix[y,x],1}"); // https://docs.microsoft.com/en-us/dotnet/standard/base-types/composite-formatting
			Console.Write('\n');
		}
		Console.WriteLine($"\n{DateTime.Now:yyyy-MM-dd (HH\\hmm)}: Duration for {wh} x {wh} is {sw.Elapsed.ToString(@"hh\:mm\:ss\.fff")}; LowestRisk={perimeters[innerPerimeterIx,startIx].risk-perimeters[outterCopyIx,startIx].risk} ({perimeters[innerPerimeterIx,startIx].path.Count-1} moves); start={start}\n");
* /
		return (perimeters[innerPerimeterIx,startIx].risk-perimeters[outterCopyIx,startIx].risk,perimeters[innerPerimeterIx,startIx].path);

		static (int y,int x) Location((int y,int x) location, int width, int x) {
			if (x<width) return (location.y+(width-x-1),location.x);
			return (location.y,location.x+(x+1-width));
		}
	} */

	static (int risk, List<(int y,int x)> path) FindBestPath(int [,] matrix, (int y,int x) src, (int y,int x) dst, uint maxBackSteps=uint.MaxValue)
	{
		int height = matrix.GetLength(0), width = matrix.GetLength(1);
		int lowestRiskSoFar = int.MaxValue, iterationCount = 0, pathCount = 0;
		var shortestPath = new List<(int y,int x)>();

		if (matrix[src.y,src.x] == int.MaxValue) throw new ArgumentException("Invalid source location - a wall !");

		int minimalRislLocation = matrix.Cast<int>().Min();

		//var archives = new Dictionary<(int y,int x), (int risk, List<(int y,int x)> path, (int y, int x) from)>();
		var archives = new Dictionary<(int y,int x), (int risk, List<(int y,int x)> path)>[4] {
			new Dictionary<(int y,int x), (int risk, List<(int y,int x)> path)>(), // right
			new Dictionary<(int y,int x), (int risk, List<(int y,int x)> path)>(), // down
			new Dictionary<(int y,int x), (int risk, List<(int y,int x)> path)>(), // left
			new Dictionary<(int y,int x), (int risk, List<(int y,int x)> path)>()  // up
		};
		return MoveTo(src,-matrix[src.y,src.x],(new List<(int y,int x)>(),(0,0))); // Do not consider the risk of the starting location unless you move there

		(int risk, List<(int y,int x)> path) MoveTo((int y, int x) location, int risk, (List<(int y,int x)> path, (int y,int x) max) path, int direction=0) {
			if (location.y<0 || location.y>=height || location.x<0 || location.x>=width || matrix[location.y,location.x]==int.MaxValue || 
				path.path.Contains((location.y,location.x)) || 
				risk + matrix[dst.y,dst.x] + minimalRislLocation * (Math.Abs(location.y-dst.y)+Math.Abs(location.x-dst.x)-1) > lowestRiskSoFar) return (int.MaxValue,null);

			iterationCount++;
			risk += matrix[location.y,location.x];
			path.path.Add(location);
			path.max.y = Math.Max(path.max.y,location.y);
			path.max.x = Math.Max(path.max.x,location.x);

			var loc = location;
			(int risk, List<(int y,int x)> path) archiveValue = (int.MaxValue,null);
			if (direction>0 && archives[direction-1].TryGetValue(location, out archiveValue)) {
				//if (archiveValue.risk == int.MaxValue) return (int.MaxValue,null);
				if (risk+archiveValue.risk > lowestRiskSoFar) return (int.MaxValue,null);
				risk += archiveValue.risk;
				path.path.AddRange(archiveValue.path);
				//Console.WriteLine($"{location}: {archiveValue.risk}");
				location = dst;
			}

			if (location == dst) {
				pathCount++;
				if (risk <= lowestRiskSoFar) {
					if (risk < lowestRiskSoFar || path.path.Count < shortestPath.Count)  {
						shortestPath = path.path;
						if (archiveValue.risk != int.MaxValue) {
							Console.Beep(); Console.Write($"\n{DateTime.Now:HH\\hmm} {loc} {risk} ({shortestPath.Count}): "); for (int i=0; i<Math.Min(20,path.path.Count) ;i++) Console.Write($"({path.path[i].y},{path.path[i].x}) "); Console.Write('\n');
						}
					}
					lowestRiskSoFar = risk;
					//return (risk,path.path);
				}

				return (risk,path.path);
			}

			//if (risk + matrix[dst.y,dst.x] > lowestRiskSoFar) return (int.MaxValue,null); // no need to go further since we already have found better path than this path segment

			var right = MoveTo((location.y,location.x+1), risk, (new List<(int y,int x)>(path.path),path.max),1);
			if (right.risk != int.MaxValue && right.path[^1]==dst) {
				var segment = right.path.SkipWhile(l => l!=location).Skip(1).ToList();
				int segmentRisk = segment.Sum(l=>matrix[l.y,l.x]);

				if (archives[0].TryGetValue(location, out archiveValue) && (archiveValue.risk != segmentRisk || archiveValue.path.Count != segment.Count)) {
					Console.Write($"{location} ({segmentRisk} was {archiveValue.risk}): "); for (int i=0; i<segment.Count ;i++) Console.Write(segment[i]); Console.Write('\n');
				} else {
					Console.Write($"{location} ({segmentRisk}): "); for (int i=0; i<segment.Count ;i++) Console.Write(segment[i]); Console.Write('\n');
				}

				archives[0][location] = (segmentRisk,segment);
			} //else if (archives[0].TryGetValue(location, out archiveValue)) Console.Write($"{location} (void was {archiveValue.risk})\n"); //archives[0].Remove(location); //] = (int.MaxValue,null);

			var down = MoveTo((location.y+1,location.x), risk, (new List<(int y,int x)>(path.path),path.max),2);

			//if (Math.Max(0,path.max.y-path.path[^1].y) + Math.Max(0,path.max.x-path.path[^1].x) > maxBackSteps) return (int.MaxValue,null);
			var left = MoveTo((location.y,location.x-1), risk, (new List<(int y,int x)>(path.path),path.max),3);
			var up = MoveTo((location.y-1,location.x), risk, (new List<(int y,int x)>(path.path),path.max),4);

			//var min = new (int risk, List<(int y,int x)> path)[] {right,down,left,up}.OrderBy(m => m.risk).First();//.Min();
			var min = new (int risk, List<(int y,int x)> path)[] {right,down,left,up}.MinBy(x => x.risk);

			/*if (min.risk != int.MaxValue) {
				var segment = min.path.SkipWhile(l => l!=location).Skip(1).ToList();
				var fromLoc = min.path[^(segment.Count+1)];
				archives[location] = (min.risk-risk,segment,fromLoc);
				if (location == (3,11) || location == (666,4)) {
					Console.Write($"\nMinimum risk from {location} arriving from ({fromLoc}) ({min.risk-risk}/{lowestRiskSoFar}): ");
					for (int i=0; i<segment.Count ;i++) Console.Write(segment[i]);Console.Write(";");
					for (int i=0; i<min.path.Count ;i++) Console.Write(min.path[i]);
					Console.Write("\n");
				}
			} else {
				if (location == (3,11) && archives.ContainsKey(location) && archives[location].risk!=int.MaxValue) {
					Console.Write($"\nMinimum risk from {location} ({archives[location].risk}/{lowestRiskSoFar}): Voided: ");
					for (int i=0; i<archives[location].path.Count ;i++) Console.Write(archives[location].path[i]);
					Console.Write("\n");
				}
				archives[location] = (int.MaxValue,null,(0,0));
			}*/

			return min;
		}
	}

	static void PrintMatrix(int[,] matrix, int risk, List<(int y,int x)> path, Stopwatch sw)
	{
		for (int y=0; y<matrix.GetLength(0) ;y++) {
			for (int x=0; x<matrix.GetLength(1) ;x++) {
				char v = matrix[y,x]==int.MaxValue ? '#' : (char)(matrix[y,x] + '0');
				if (path.Contains((y,x))) if (Console.IsOutputRedirected) Console.Write('*');
				else {
					Console.ForegroundColor = ConsoleColor.Cyan;
					Console.Write(v);
					Console.ForegroundColor = ConsoleColor.White;
				} else Console.Write(v);
			}
			Console.Write('\n');
		}
		//for (int i=0; i<path.Count ;i++) Console.Write(path[i]);
		string value = $"\n{DateTime.Now:yyyy-MM-dd (HH\\hmm)}: Duration for {matrix.GetLength(0)} rows per {matrix.GetLength(1)} columns is {sw.Elapsed:hh\\:mm\\:ss\\.fff}; Risk={risk} ({path.Count - 1} moves); srcLoc=({path[0].y},{path[0].x}); dstLoc=({path[^1].y},{path[^1].x})\n";
		Console.WriteLine(value);
	}

	/*public static unsafe void FillMultiDimArray<T>(this Array array, T value)
	{
		fixed (int* a = Array.Fill<int>() {
			var span = new Span<T>(array, array.Length);
			span.Fill(value);
		}
	}*/
}
