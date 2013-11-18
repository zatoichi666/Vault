/// <summary>    
/// Matthew Synborski
/// CSE-681 Fall 2013 for Dr. Jim Fawcett
/// 
/// Description: TextQuery Class - Performs Text Query on a list of fully qualified filenames
/// This Query iterates through each filename, parsing the file completely, looking for any 
/// or all of the search strings.
/// 
/// </summary>
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Office.Interop.Word;

namespace TextAnalyzer
{

    public static class TextQuery
    {
        /// <summary>
        /// Demonstration entry point for TextQuery
        /// </summary>
        static void Main()
        {
            Console.WriteLine("Demonstrating TextQuery Class");
            Console.WriteLine("=============================");

            Console.WriteLine("Demonstrates find all vs. find one TextQuery, using the same file table.");
            Console.WriteLine("Expected Result: The \"find one\" query should return more results.");
            Console.WriteLine();

            Directory.SetCurrentDirectory(Directory.GetCurrentDirectory() + "/../..");
            String currDir = Directory.GetCurrentDirectory();

            String[] extArray = { "*.cs" };
            String[] searchStringArray = { "String", "Synborski", "int" };
            String[] xmlTagArray = { "time", "keyword" };

            Console.WriteLine("Working in " + currDir);
            Console.Write("Looking for {");
            foreach (String s in searchStringArray)
            {
                Console.Write("\"" + s + "\" ");

            }
            Console.WriteLine("}");
            Console.WriteLine();

            IEnumerable<String> ft = FileTable.GetFiles(currDir, extArray, true);

            Console.WriteLine("Printing contents of file set (using FileTable)");
            foreach (String s in ft)
            {
                Console.WriteLine(s);
            }
            Console.WriteLine();

            List<XmlSearchResult_c> res = TextQuery.run(ft, searchStringArray, true, xmlTagArray);
            Console.WriteLine("Printing results of \"find all\" TextQuery");
            foreach (XmlSearchResult_c s in res)
            {
                if (s.textQueryFound)
                    Console.WriteLine(s.filename);
            }
            Console.WriteLine();

            List<XmlSearchResult_c> res2 = TextQuery.run(ft, searchStringArray, false, xmlTagArray);
            Console.WriteLine("Printing results of \"find one\" TextQuery");
            foreach (XmlSearchResult_c s in res2)
            {
                if (s.textQueryFound)
                    Console.WriteLine(s.filename);
            }
            Console.WriteLine();

        }

        private static bool textQuery(String filename, String[] searchStringList, bool mustContainAll)
        {
            String stringToCheck = "";
            Boolean result = false;

            if (filename.EndsWith(".doc") && (!filename.Contains("~")))
            {
                Application ap = new Application();
                Document document = ap.Documents.Open(filename); // Open the .doc using MSWord
                stringToCheck = document.Content.Text;
                ((_Application)ap).Quit(); // Close MSWord
            }
            else
            {
                stringToCheck = File.ReadAllText(filename);
            }

            if (mustContainAll)
            {
                if (searchStringList.All(s => stringToCheck.Contains(s)))
                {
                    result = true;

                }
            }
            else
            {
                if (searchStringList.Any(stringToCheck.Contains))
                {
                    result = true;

                }
            }
            return result;
        }


        private static XmlSearchResult_c queryXDocumentMetadata(XDocument xD, String filename, String[] searchTagList)
        {

            XmlSearchResult_c entry = new XmlSearchResult_c();
            entry.filename = filename;

            foreach (String tag in searchTagList)
            {
                var q = from x in
                            xD.Elements("metadata")
                            .Descendants(tag)
                        select x;

                foreach (var elem in q)
                {
                    entry.tagAndValue.Add(elem.Name.ToString() + ":" + elem.Value.ToString());
                }
            }
            return entry;
        }

        private static XmlSearchResult_c metadataQuery(String filename, String[] metadataTagList)
        {
            XmlSearchResult_c xmlResult = new XmlSearchResult_c();
            xmlResult.filename = filename;
            
            if (File.Exists(filename + ".metadata"))
            {
                
                // for each file in the file table, read in the XML content
                try
                {
                    XmlDocument xd = new XmlDocument();
                    xd.Load(filename + ".metadata");
                                       

                    XDocument xD = XDocument.Parse(xd.OuterXml.ToString());
                    // Test if the root node is "metadata"                    
                    if (xD.Root.Name.ToString() == "metadata")
                    {
                        xmlResult = queryXDocumentMetadata(xD, filename, metadataTagList);
                        xmlResult.metadataFileFound = true;
                    }
                }
                catch
                {
                    Console.Write("Xml Exception found\n");
                }
            }
            else
            {
                xmlResult.metadataFileFound = false;
                //Console.WriteLine("Error: metadata file not found for " + filename);
            }
            return xmlResult;
        }

        public static void DisplayResults(List<XmlSearchResult_c> queryResults)
        {
            foreach (XmlSearchResult_c xsr in queryResults)
            {

                StringBuilder sb1 = new StringBuilder();
                sb1.Append("Filename: " + xsr.filename + "\n");
                sb1.Append('=', 10 + xsr.filename.Length);
                sb1.Append("\n");
                Console.Write(sb1.ToString());

                if (xsr.metadataFileFound)
                {


                    if (xsr.metadataQueryAttempted)
                    {

                        StringBuilder sb2 = new StringBuilder();
                        sb2.Append("   MetadataQuery Results\n");
                        sb2.Append("   ");
                        sb2.Append('-', 21);
                        sb2.Append("\n");
                        Console.Write(sb2.ToString());

                        foreach (String s in xsr.tagAndValue)
                        {
                            Console.WriteLine("   " + s);
                        }
                        Console.WriteLine();
                    }
                }
                else
                {
                    Console.WriteLine("   (!) Error: No metadata found\n");
                }

                if (xsr.textQueryAttempted)
                {

                    if (xsr.textQueryFound)
                    {
                        StringBuilder sb3 = new StringBuilder();
                        sb3.Append("   TextQuery Results\n");
                        sb3.Append("   ");
                        sb3.Append('-', 20);
                        sb3.Append("\n");
                        Console.Write(sb3.ToString());
                        Console.WriteLine("   Found text in " + xsr.filename);
                    }

                    Console.WriteLine();
                }

            }

        }


        /// <summary>
        /// run is the static method in the TextQuery class.  
        /// This takes an IEnumerable of strings containing the fully qualified filenames 
        /// of the files to be Queried.  This query returns the list of filenames that contain
        /// any or all of the strings within the searchStringList array.
        /// 
        /// If the fileTable entry being searched ends with ".doc", Microsoft.Office.Interop.Word 
        /// will be used to open the file, extract the text content and perform the text Query thereon.
        /// </summary>
        /// <param name="fileTable"></param>
        /// <param name="searchStringList"></param>
        /// <param name="mustContainAll"></param>
        /// <returns></returns>
        public static List<XmlSearchResult_c> run(IEnumerable<string> fileTable, String[] searchStringList, bool mustContainAll, String[] metadataTagList)
        {
            List<XmlSearchResult_c> xmlSearchResults = new List<XmlSearchResult_c>();
            IEnumerable<String> strings = searchStringList;

            foreach (String filename in fileTable)
            {
                XmlSearchResult_c xsr = new XmlSearchResult_c();
                
                xsr = metadataQuery(filename, metadataTagList);
                if (metadataTagList.Length > 0)
                {                    
                    xsr.metadataQueryAttempted = true;
                }
                
                if (searchStringList.Length > 0)
                {
                    xsr.textQueryAttempted = true;
                    if (textQuery(filename, searchStringList, mustContainAll))
                    {
                        xsr.textQueryFound = true;
                    }
                }
                xmlSearchResults.Add(xsr);
            }

            return xmlSearchResults;
        }

    }
}
