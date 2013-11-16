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
    /// Description: Switch_c class is used for storing a pair of strings.  
    /// The switchChar contains a single character string.  
    /// The payload contains the payload passed along with the switch
    /// </summary>
    public class Switch_c
    {
        static void Main()
        {

            Console.WriteLine("Testing Switch_c class");

            Switch_c test = new Switch_c();

            test.payload = "payload";
            test.switchChar = "R";
            
            Console.WriteLine("Expected values: payload = \"payload\" ");
            Console.WriteLine("Expected values: switchChar = \"R\" ");
            Console.WriteLine("Actual values: payload = " + test.payload);
            Console.WriteLine("Actual values: switchChar = " + test.switchChar);
                
        }

        public String payload { get; set; }
        public String switchChar { get; set; }

    }
}
