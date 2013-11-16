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
    /// Description: MetadataGen class is a top-level class (executable).
    /// This aggregates a command line processor to take switches from the user's inputs.  The switches are used to populate a job object.  
    /// The job object is of MetadataJob type.  This job operates on a single file to create a metadata file in the same path.  The user's 
    /// Metadata inputs are used to create the metadata file.  
    /// 
    /// </summary>
    class MetadataGen
    {
        static void Main(string[] args)
        {
            CommandLineProcessor cmd = new CommandLineProcessor(args);
            MetadataGenJob _job = new MetadataGenJob();

            int errcode = _job.setJobFromSwitches(cmd.getSwitches());
            /* /F"TextQuery.cs" /T"19810410031414" /D"This is a C# file containing the implementation for the TextQuery class" /E"C:\School\CSE-681\Project2-TextAnalyzer\TextAnalyzer\TextAnalyzer\TextAnalyzerJob.cs>Aggregation" /E"C:\School\CSE-681\Project2-TextAnalyzer\TextAnalyzer\TextAnalyzer\CommandLineProcessor.cs>Aggregation" /E"C:\School\CSE-681\Project2-TextAnalyzer\TextAnalyzer\TextAnalyzer\TextQuery.cs>Aggregation" /E"C:\School\CSE-681\Project2-TextAnalyzer\TextAnalyzer\TextAnalyzer\MetadataQuery.cs>Aggregation" /S"1262" /V"1" /C"prototype" /K"CSharp" /K"code"
             */

            if (errcode == -2)
            {
                _job.DisplayUsage();
            }

            else if (errcode == -1)
            {
                Console.WriteLine("There were errors:");
                foreach (String errString in _job.resultStringList)
                {
                    Console.WriteLine(errString);
                }
                Console.WriteLine("");
                _job.DisplayUsage();
            }
            else
            {
                String filename = _job.taskPath + ".metadata";
                _job.GenerateMetadata().Save(filename);
                Console.WriteLine(_job.taskPath + " => " + filename);
            }
        }
    }
}
