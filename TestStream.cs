using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

/*
Streams only deal with byte[] data while TextReader/TextWriter deal with characters (some been one byte and some been two bytes)
*/
namespace TestStream {
	static class Tester {
		public static void Go()
		{
			var msg = "TestStream";
			Console.WriteLine($"\n--- {msg} {new String('-', Math.Max(65-msg.Length,3))}\n");

			string test = "Je m'appel Éléonore 😜";

			// You could also comment out the following block and use preset encodings instead e.g. Encoding.ASCII, Encoding.DEFAULT, Encoding.UTF8, etc.
			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
         Encoding encoding = Encoding.GetEncoding("windows-1252");

			// Convert string to stream without using StreamWriter
			byte[] byteArray = encoding.GetBytes(test); //byte[] byteArray = Encoding.Default.GetBytes(test);
			var ms = new MemoryStream(byteArray); // <= this MemoryStream is not expandable !

			// Convert string to stream using StreamWriter (it uses System.Text.Encoding.Default.GetBytes(str))
			ms = new MemoryStream(500); // 500 bytes capacity
			var sw = new StreamWriter(ms, encoding);
			sw.Write(test);
			sw.Flush();

			// Convert a stream to a string
			ms.Position = 0;
			var sr = new StreamReader(ms,encoding);
			string text = sr.ReadToEnd();
			Console.WriteLine($"text=\"{text}\"");

			// Save stream to a file
			ms.Position = 0;
			string fileLocationAndName = "./TestStream.txt";
			using(FileStream fs = new FileStream(fileLocationAndName, FileMode.Create)) 
  				ms.CopyTo(fs);

			// Read it back to ms at current ms position (for twice the text)
			using(FileStream fs = new FileStream(fileLocationAndName, FileMode.Open, FileAccess.Read)) 
				fs.CopyTo(ms);
			Console.WriteLine($"Content of {fileLocationAndName}: \"{ms.ToString(encoding)}\"");

			// Delete the file
			File.Delete(fileLocationAndName);
		}

		public static string ToString(this Stream stream_a, Encoding encoding_a=null)
		{
			if (encoding_a is null) encoding_a = Encoding.Default;
			stream_a.Position = 0;
			var sr = new StreamReader(stream_a,encoding_a);
			return sr.ReadToEnd();
		}
	}
}