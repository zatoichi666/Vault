using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextAnalyzer
{
    /// <summary>
    /// Matthew Synborski
    /// CSE-681 Fall 2013 for Dr. Jim Fawcett
    /// 
    /// Description: TextAnalyzer class is a top-level class (executable).
    /// This aggregates a command line processor to take switches from the user's inputs.  The switches are used to populate a job object.  
    /// The job object is of TextAnalyzerJob type.  This job operates on a list of files, looking first for associated metadata files.  
    /// If metadata files are found the job's metadata tags are queried from the file list's metadata.  If the job also contains textQuery 
    /// strings, they are queried from the file list.
    /// 
    /// </summary>
    class TextAnalyzer
    {



        static void Main(string[] args)
        {
            CommandLineProcessor cmd = new CommandLineProcessor(args);
            TextAnalyzerJob _job = new TextAnalyzerJob();

            int errcode = _job.setJobFromSwitches(cmd.getSwitches());

            if (errcode == -1)
            {
                Console.WriteLine("There were errors:");
                foreach (String errString in _job.resultStringList)
                {
                    Console.WriteLine(errString);
                }
                Console.WriteLine("");
                _job.DisplayUsage();
            }
            else if (errcode == -2)
            {
                _job.DisplayUsage();
            }
            else if (errcode == 0)
            {

                IEnumerable<string> fileTable = FileTable.GetFiles(_job.taskPath, _job.extensionList.ToArray(), _job.recursive);
                
                
                /*
                 * /T"String" /T"int" /A /F"C:\School\CSE-681\Project2-TextAnalyzer" /X"txt" /X"cs" /X"doc" /R /M"time" /M"keyword"
                 */
                if (_job.textQueryList.Count > 0)
                {
                    
                    
                    Console.Write("Performing text query for: {");
                    
                    foreach (String s in _job.textQueryList)
                    {
                        Console.Write(s);
                        Console.Write(" ");
                    }
                    Console.Write("} ");
                }

                if (_job.metadataQueryList.Count > 0)
                {
                    Console.Write("Performing metadata query for: {");
                    foreach (String s in _job.metadataQueryList)
                    {
                        Console.Write(s);
                        Console.Write(" ");
                    }
                    Console.WriteLine("}");
                }
                else
                    Console.WriteLine();

                List<XmlSearchResult_c> tq = TextQuery.run(fileTable, _job.textQueryList.ToArray(), _job.mustFindAll, _job.metadataQueryList.ToArray());
                TextQuery.DisplayResults(tq);
                
                
            }
        }
    }
}
