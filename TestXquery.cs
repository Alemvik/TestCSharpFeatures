/*
LINQ to XML is implemented on top of XmlReader, and they're tightly integrated. However, you can also use
XmlReader directly. For example, suppose you're building a Web service that will parse hundreds of XML 
documents per second, and the documents have the same structure, meaning that you only have to write one
implementation of the code to parse the XML. In this case, you'd probably want to use XmlReader directly.

In contrast, if you're building a system that parses many smaller XML documents, and each one is different,
you'd want to take advantage of the productivity improvements that LINQ to XML provides.

LINQ to XML isn't intended to replace XSLT. XSLT is still the tool of choice for complicated and document-centric
XML transformations, especially if the structure of the document isn't well defined.

You can use axis methods to retrieve attributes, child elements, descendant elements, and ancestor elements.
LINQ to XML queries operate on axis methods, and provide several flexible and powerful ways to navigate through
and process an XML tree. See https://docs.microsoft.com/en-us/dotnet/standard/linq/linq-xml-axes-overview

XDocument vs XElement:
XDocument represents a whole XML document. It is normally composed of a number of elements.
XElement represents an XML element (with attributes, children etc). It is part of a larger document.

Use XDocument when working with a whole XML document, XElement when working with an XML element.

For example - XElement has a HasAttributes property indicating whether any attributes exist on the element,
but an XDocument doesn't, as such a property is meaningless in the context of a whole XML Document.

Note that you only have to create XDocument objects if you require the specific functionality provided by the
XDocument class. In many circumstances, you can work directly with XElement. Working directly with XElement is
a simpler programming model. XDocument derives from XContainer. Therefore, it can contain child nodes. However,
XDocument objects can have only one child XElement node. This reflects the XML standard that there can be only
one root element in an XML document.

https://docs.microsoft.com/en-us/dotnet/standard/linq/linq-xml-vs-xml-technologies
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Xml.Xsl;

using Emvie;

namespace TestXquery {
	static class Tester {
		public static void Go()
		{
			var msg = "TestXquery";
			Console.WriteLine($"\n--- {msg} {new String('-', Math.Max(65-msg.Length,3))}\n");

			XDocument srcTree = new XDocument(
				new XComment("This is a comment"),
				new XElement("Root",
					new XElement("Child1", "data1"),
					new XElement("Child2", "data2"),
					new XElement("Child3", "data3"),
					new XElement("Child2", "data4"),
					new XElement("Info5", "info5"),
					new XElement("Info6", "info6"),
					new XElement("Info7", "info7"),
					new XElement("Info8", "info8")
				)
			);

			XDocument doc = new XDocument(
				new XComment("This is a comment"),
				new XElement("Root",
					from el in srcTree.Element("Root").Elements()
					where ((string)el).StartsWith("data")
					select el
				)
			);
			Console.WriteLine(doc);

			XElement root = XElement.Load("ExampleExcel2003FormatSmaller.xml");
			//XDocument root = XDocument.Load("ExampleExcel2003FormatSmaller.xml");
			XNamespace urn="urn:schemas-microsoft-com:office:spreadsheet"; 
			XElement sheet = root.Element(urn+"Worksheet");
    		//Console.WriteLine(sheet);

			IEnumerable<XElement> elems =	root.Descendants(urn+"Worksheet").Where(x =>
				x.Attribute(urn+"Name")?.Value.ToLower() == "report"
			);

			Console.WriteLine(TriOptimaFxRateToCsv("ExampleExcel2003FormatSmaller.xml"));

			//var sheet = (from e in root.Element("Worksheet").Elements()select e);
			//Console.WriteLine(sheet);
			//Console.WriteLine(root);
		}

		// Example: var csv = XmlToCsv(XElement.Load("Excel2003Format.xml"));
		// public static string XmlToCsv(XElement element_a, string path_a, int colCount_a=0, string nameSpace_a="")
		// {
		// 	XNamespace urn = nameSpace_a;

		// }

		static (int count, string csv) TriOptimaFxRateToCsv(string pathFilename_a)
		{
			XElement root = XElement.Load(pathFilename_a);
			XNamespace urn="urn:schemas-microsoft-com:office:spreadsheet"; 
			XElement sheet = root.Element(urn+"Worksheet");

			// Under Worksheet(Name=Report) / Table, get all rows that has only 4 columns
			var rows = root.Descendants(urn+"Worksheet").Where(x => // Descendants looks in all levels
					x.Attribute(urn+"Name")?.Value.ToLower() == "report"
				)
				.SingleOrDefault()
				.Elements(urn+"Table") // Elements finds only those elements that are direct descendents
				.SingleOrDefault()
				.Elements(urn+"Row")
				.Where(x => x.Elements(urn+"Cell").Count() == 4);

			var csv = new StringBuilder();
			foreach(XElement row in rows) {
				var columns = row.Elements(urn+"Cell").Elements(urn+"Data");
				foreach(XElement col in columns) if (col.Value.Contains(',')) {
					csv.Append($"\"{col.Value.Replace("\"","\"\"")}\",");
				} else csv.Append(col.Value+',');
				csv[csv.Length-1]='\n'; // replace last comma by a newline
			}

			return (rows.Count()-1,csv.ToString());
		}
	}
}