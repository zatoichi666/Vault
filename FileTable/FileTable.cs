///////////////////////////////////////////////////////////////////////////////
// FileTable.cs - Utility class for collecting a list of files, using        //
// recursion.                                                                 //
//                                                                           //
// Matthew Synborski, CSE681 - Software Modeling and Analysis, Fall 2013     //
///////////////////////////////////////////////////////////////////////////////
/*
 *
 * Required References:
 * - System.Threading.Tasks
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace TextAnalyzer
{
    /// <summary>
    /// Matthew Synborski
    /// CSE-681 Fall 2013 for Dr. Jim Fawcett
    /// 
    /// Description: FileTable class is used for finding files within a single directory, 
    /// either recursively or non-recursively with a list of file extensions.  The file 
    /// extension will be made case-insensitive, so specifying ".xml" is the same as ".XML"
    /// 
    /// A IEnumerable<string> is returned containing the fully qualified path of the found files.
    /// </summary>
    public static class FileTable
    {

        static void Main()
        {
            Console.WriteLine("Demonstrating FileTable Class");
            Console.WriteLine("=============================");
            Console.WriteLine();
            Directory.GetCurrentDirectory();
            String[] fileExt = {"*.cs", "*.csproj"};
            Directory.SetCurrentDirectory(Directory.GetCurrentDirectory() + "/../..");
            String currDir = Directory.GetCurrentDirectory();

            IEnumerable<String> fTab = FileTable.GetFiles(currDir, fileExt, true);

            Console.WriteLine("Printing out recursively retrieved file table relative to: " + currDir);
            foreach (String s in fTab)
            {
                Console.WriteLine(s);
            }
            Console.WriteLine();

            fTab = FileTable.GetFiles(currDir, fileExt, false);

            Console.WriteLine("Printing out non-recursively retrieved file table relative to: " + currDir);
            foreach (String s in fTab)
            {
                Console.WriteLine(s);
            }

        }
        /// <summary>
        /// Appends the correct file extension characters if they are missing.
        /// </summary>
        /// <param name="ext"></param>
        /// <returns>A list of file extensions containing the correct file extension form, if "*." is missing it appends that.</returns>
        private static string[] correctExtensionArray(string[] ext)
        {
            List<String> extCorr = new List<String>();

            foreach (string s in ext)
            {
                if (!s.Contains("*"))
                {
                    extCorr.Add(String.Concat("*.", s));
                }
                else if (!s.Contains("."))
                {
                    extCorr.Add(String.Concat(".", s));
                }
                else
                {
                    extCorr.Add(s);
                }

            }
            return extCorr.ToArray();
        }

        /// <summary>
        /// Performs the file search and returns an IEnumerable list of the fully qualified found files.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="fileExtensions"></param>
        /// <param name="isRecursive"></param>
        /// <returns></returns>
        public static IEnumerable<string> GetFiles(string path, string[] fileExtensions, bool isRecursive)
        {
            fileExtensions = correctExtensionArray(fileExtensions);
            SearchOption searchOption;
            if (isRecursive)
            {
                searchOption = SearchOption.AllDirectories;
            }
            else
            {
                searchOption = SearchOption.TopDirectoryOnly;
            }

            return fileExtensions.AsParallel().SelectMany(searchPattern => Directory.EnumerateFiles(path, searchPattern, searchOption));
        }
    }
}
