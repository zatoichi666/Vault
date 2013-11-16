using System;
using System.Collections.Generic;

namespace TextAnalyzer
{
    /// <summary>
    /// Matthew Synborski
    /// CSE-681 Fall 2013 for Dr. Jim Fawcett
    /// 
    /// Description: CommandLineProcessor class is used for taking in the arguments from the command line input
    /// and packaging them into a container of String_s objects, one for each user-specified switch.  
    /// </summary>
    public class CommandLineProcessor
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Demonstrating CommandLineProcessor Class");
            Console.WriteLine("=============================");
            Console.WriteLine();
            Console.WriteLine("The user specified " + args.Length + " arguments");
            Console.WriteLine("The list of arguments is as follows:");
            foreach (String s in args)
            {
                Console.WriteLine(s);
            }
            Console.WriteLine();

            Console.WriteLine("Demonstrating CommandLineProcessor Class");
            CommandLineProcessor clExample = new CommandLineProcessor(args);

            Console.WriteLine("Printing switches and payloads of each argument:");

            foreach (Switch_c sw in clExample.getSwitches())
            {
                Console.WriteLine("Switch: /" + sw.switchChar + " Payload: " + sw.payload);
            }
        }

        private List<Switch_c> switches = new List<Switch_c>();

        public List<Switch_c> getSwitches()
        {
            return switches;
        }

        /// <summary>
        /// Check for the form of the arguments from the user.  
        /// Ensure:
        /// 1. There are arguments, otherwise show the usage statement.
        /// 2. There are entries, consisting of a single character "switch character" 
        ///     and a payload.  Note, the payload can be empty, so it can have any form.
        /// </summary>
        /// <param name="args"></param>
        public CommandLineProcessor(String[] args)
        {

            // Populate the switches object
            for (int i = 0; i < args.Length; i++)
            {
                try
                {
                    Switch_c entry = new Switch_c();
                    if (args[i].IndexOf("/") == 0)
                    {
                        entry.payload = args[i].Substring(args[i].IndexOf("/") + 2, args[i].Length - args[i].IndexOf("/") - 2);
                        entry.switchChar = args[i].Substring(args[i].IndexOf("/") + 1, 1);
                    }
                    else
                    {
                        entry.payload = args[i];
                        entry.switchChar = "";
                    }
                    switches.Add(entry);
                }
                catch (Exception e)
                {
                    Console.WriteLine("{0} Exception caught.", e);
                }
            }



        }

    }


}
