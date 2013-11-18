///////////////////////////////////////////////////////////////////////////////
// Job.cs - Utility class for collecting a list of inputs for the            //
// TextAnalyzer                                                              //
//                                                                           //
// Matthew Synborski, CSE681 - Software Modeling and Analysis, Fall 2013     //
///////////////////////////////////////////////////////////////////////////////
/*
 *
 * Required References:
 * - System.Collections.Generic;
 * - System.Xml.Linq
 */

using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Xml;
using System.Xml.Linq;

namespace TextAnalyzer
{
    public interface IJob
    {
        List<String> categoryList { get; set; }
        List<String> keywordList { get; set; }
        List<String> resultStringList { get; set; }
        void DisplayUsage();
        int setJobFromSwitches(List<Switch_c> _switches);
    }

    /// <summary>    
    /// Matthew Synborski
    /// CSE-681 Fall 2013 for Dr. Jim Fawcett
    /// 
    /// Description: MetadataGenJob Class - Container for MetadataGen Jobs.  
    /// This container class contains the processed contents of the user's input, 
    /// and any pertinent data read by the MetadataQuery
    /// </summary>    
    public class MetadataGenJob : IJob
    {
        public static void Main()
        {
            MetadataGenJob mg = new MetadataGenJob();
            List<Switch_c> switches = new List<Switch_c>();

            Console.WriteLine("Demonstrating MetadataGen Class");
            Console.WriteLine("===============================");
            Console.WriteLine();

            Console.WriteLine("Populating a list of Switch_c objects\n");

            Directory.SetCurrentDirectory(Directory.GetCurrentDirectory() + "/../..");
            String currDir = Directory.GetCurrentDirectory();

            Switch_c sw1 = new Switch_c(); sw1.payload = "TextQuery.cs"; sw1.switchChar = "F"; switches.Add(sw1);
            Switch_c sw2 = new Switch_c(); sw2.payload = "19810410031414"; sw2.switchChar = "T"; switches.Add(sw2);
            Switch_c sw3 = new Switch_c(); sw3.payload = "This is a C# file containing the implementation for the TextQuery class"; sw3.switchChar = "D"; switches.Add(sw3);
            Switch_c sw4 = new Switch_c(); sw4.payload = "TextAnalyzerJob.cs>Aggregation"; sw4.switchChar = "E"; switches.Add(sw4);
            Switch_c sw5 = new Switch_c(); sw5.payload = "CommandLineProcessor.cs>Aggregation"; sw5.switchChar = "E"; switches.Add(sw5);
            Switch_c sw6 = new Switch_c(); sw6.payload = "TextQuery.cs>Aggregation"; sw6.switchChar = "E"; switches.Add(sw6);
            Switch_c sw7 = new Switch_c(); sw7.payload = "MetadataQuery.cs>Aggregation"; sw7.switchChar = "E"; switches.Add(sw7);
            Switch_c sw8 = new Switch_c(); sw8.payload = "1262"; sw8.switchChar = "S"; switches.Add(sw8);
            Switch_c sw9 = new Switch_c(); sw9.payload = "1"; sw9.switchChar = "V"; switches.Add(sw9);
            Switch_c sw10 = new Switch_c(); sw10.payload = "prototype"; sw10.switchChar = "C"; switches.Add(sw10);
            Switch_c sw11 = new Switch_c(); sw11.payload = "CSharp"; sw11.switchChar = "K"; switches.Add(sw11);
            Switch_c sw12 = new Switch_c(); sw12.payload = "code"; sw12.switchChar = "K"; switches.Add(sw12);

            Console.WriteLine("Populating the MetadataGenJob object\n");
            mg.setJobFromSwitches(switches);

            Console.WriteLine("Accessing the object, outputting resulting Metadata");
            Console.Write(mg.GenerateMetadata().OuterXml);
        }



        private bool _recursive = false;
        private bool _mustFindAll = false;
        private String _queryType;
        private String _taskPath;
        private List<String> _extensionList = new List<String>();
        private List<String> _categoryList = new List<String>();
        private List<String> _keywordList = new List<String>();
        private List<String> _textQueryList = new List<String>();
        private List<String> _metadataQueryList = new List<String>();
        private List<String> _resultStringList = new List<String>();
        private int _size = 0;
        private int _version = 0;
        private Dictionary<String, String> _dependencyList = new Dictionary<String, String>();
        private String _description;
        private DateTime _time;

        bool _switchF { get; set; }
        bool _switchV { get; set; }
        bool _switchT { get; set; }
        bool _switchD { get; set; }
        bool _switchS { get; set; }

        public XmlDocument GenerateMetadata()
        {
            XmlDocument doc = new XmlDocument();
            XmlDeclaration xd = doc.CreateXmlDeclaration("1.0", null, null);

            XElement xe = new XElement("metadata",
                new XElement("filename", _taskPath),
                new XElement("version", _version),
                new XElement("time", _time.ToString("yyyyMMddhhmmss")),
                new XElement("description", _description),
                new XElement("size", _size)
                );

            foreach (String category in this._categoryList)
            {
                xe.Add(new XElement("category", category));
            }

            foreach (String keyword in this._keywordList)
            {
                xe.Add(new XElement("keyword", keyword));
            }

            foreach (KeyValuePair<String, String> dependency in this._dependencyList)
            {
                StringBuilder sb = new StringBuilder(dependency.Key + ">" + dependency.Value);
                xe.Add(new XElement("dependency", sb.ToString()));
            }

            doc.LoadXml(xe.ToString());
            doc.InsertBefore(xd, doc.FirstChild);
            return doc;
        }

        public void DisplayUsage()
        {
            Console.WriteLine("MetadataGen by Matthew Synborski");
            Console.WriteLine("Usage: MetadataGen [OPTIONS]");
            Console.WriteLine("   or: MetadataGen path [OPTIONS]");
            Console.WriteLine("");
            Console.WriteLine("/F          Filename");
            Console.WriteLine("/V          Version");
            Console.WriteLine("/T          Time/Date");
            Console.WriteLine("/D          Description");
            Console.WriteLine("/S          Size");
            Console.WriteLine("/C          Category (optional)");
            Console.WriteLine("/E          Dependencies (optional)");
            Console.WriteLine("/K          Keywords (optional)");
        }

        public String taskPath
        {
            get
            {
                return _taskPath;
            }

            set
            {
                _taskPath = value;
            }
        }

        // Property implementation: 
        public bool recursive
        {
            get
            {
                return _recursive;
            }

            set
            {
                _recursive = value;
            }
        }

        // Property implementation: 
        public String queryType
        {
            get
            {
                return _queryType;
            }

            set
            {
                _queryType = value;
            }
        }

        // Property implementation: 
        public List<String> extensionList
        {
            get
            {
                return _extensionList;
            }

            set
            {
                _extensionList = value;
            }
        }

        // Property implementation: 
        public List<String> categoryList
        {
            get
            {
                return _categoryList;
            }

            set
            {
                _categoryList = value;
            }
        }

        // Property implementation: 
        public List<String> keywordList
        {
            get
            {
                return _keywordList;
            }

            set
            {
                _keywordList = value;
            }
        }

        // Property implementation: 
        public List<String> textQueryList
        {
            get
            {
                return _textQueryList;
            }

            set
            {
                _textQueryList = value;
            }
        }

        // Property implementation: 
        public List<String> metadataQueryList
        {
            get
            {
                return _metadataQueryList;
            }

            set
            {
                _metadataQueryList = value;
            }
        }

        // Property implementation: 
        public bool mustFindAll
        {
            get
            {
                return _mustFindAll;
            }

            set
            {
                _mustFindAll = value;
            }
        }

        // Property implementation: 
        public List<String> resultStringList
        {
            get
            {
                return _resultStringList;
            }

            set
            {
                _resultStringList = value;
            }
        }

        private void ShowElement(Switch_c element)
        {
            Console.Write("/");
            Console.Write(element.switchChar);
            Console.Write("\"");
            Console.Write(element.payload);
            Console.WriteLine("\"");
        }
        private int handleSwitchC(Switch_c element)
        {
            _categoryList.Add(element.payload);
            return 0;
        }

        private int handleSwitchE(Switch_c element)
        {
            char[] delimiterChars = { '>' };
            String[] dependency = element.payload.Split(delimiterChars);
            if (dependency.Count() != 2)
            {
                return -1;
            }
            else
            {
                _dependencyList.Add(dependency[0], dependency[1]);
                return 0;
            }
        }

        private int handleSwitchK(Switch_c element)
        {
            _keywordList.Add(element.payload);
            return 0;
        }
        private int handleSwitchF(Switch_c element)
        {
            int returncode = 0;
            if (_switchF == true)
            {
                returncode = -1;
                _resultStringList.Add("Specified multiple filename option /F");

            }
            else
            {
                // verify a file exists, if not, throw an error            
                if (File.Exists(element.payload))
                {
                    _taskPath = element.payload;
                    _switchF = true;
                    returncode = 0;
                }
                else
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append("File: ");
                    sb.Append(element.payload);
                    sb.Append(" was missing from the file system");
                    _resultStringList.Add(sb.ToString());
                    returncode = -1;
                }
            }
            return returncode;
        }
        private int handleSwitchV(Switch_c element)
        {
            int returncode = 0;
            if (_switchV == true)
            {
                _resultStringList.Add("Specified multiple version option /V");
                returncode = -1;

            }
            else
            {
                _version = Convert.ToInt32(element.payload);
                _switchV = true;
                returncode = 0;
            }
            return returncode;
        }
        private int handleSwitchT(Switch_c element)
        {
            DateTime dt;
            CultureInfo provider = CultureInfo.InvariantCulture;
            String format = "yyyyMMddhhmmss";
            dt = DateTime.ParseExact(element.payload, format, provider);
            _time = dt;
            return 0;
        }

        private int handleSwitchD(Switch_c element)
        {
            int returncode = 0;
            if (_switchD == true)
            {
                _resultStringList.Add("Specified multiple description option /D");
                returncode = -1;
            }
            else
            {
                _description = element.payload;
                _switchD = true;
                returncode = 0;
            }
            return returncode;
        }

        private int handleSwitchS(Switch_c element)
        {
            int returncode = 0;
            if (_switchS == true)
            {
                _resultStringList.Add("Specified multiple size option /S");
                returncode = -1;
            }
            else
            {
                _size = Convert.ToInt32(element.payload);
                _switchS = true;
                returncode = 0;
            }
            return returncode;
        }

        private int processSwitches(List<Switch_c> _switches)
        {
            int returncode = -2;
            foreach (Switch_c element in _switches)
            {
                // Handle Category switches
                if (element.switchChar == "C")
                {
                    returncode = handleSwitchC(element);
                }
                // Handle dEpendency switches
                else if (element.switchChar == "E")
                {
                    returncode = handleSwitchE(element);
                }
                // Handle file switch
                else if (element.switchChar == "F")
                {
                    returncode = handleSwitchF(element);
                }
                // Handle Version switch
                else if (element.switchChar == "V")
                {
                    returncode = handleSwitchV(element);
                }
                // Handle Time/Date switch
                else if (element.switchChar == "T")
                {
                    returncode = handleSwitchT(element);
                }
                // Handle Description switch
                else if (element.switchChar == "D")
                {
                    returncode = handleSwitchD(element);

                }
                // Handle Size switch
                else if (element.switchChar == "S")
                {
                    returncode = handleSwitchS(element);
                }
                else
                {
                    returncode = -1;
                    // If user enters a bogus switch, stop evaluating switches and 
                    StringBuilder sb = new StringBuilder();
                    sb.Append("Bad option: ");
                    sb.Append(element.switchChar);
                    sb.Append(" ");
                    sb.Append(element.payload);
                    _resultStringList.Add(sb.ToString());                    

                }
                if (returncode != 0)
                {
                    // If user enters a bogus switch, stop evaluating switches and 
                    return returncode;
                }
                 
            }
            return returncode;
        }

        public int setJobFromSwitches(List<Switch_c> _switches)
        {
            int returncode = 0;

            List<Switch_c> switches = new List<Switch_c>();
            switches = _switches;

            if (switches.Count == 0)
            {

                return -2;
            }

            returncode = processSwitches(_switches);

            return returncode;
        }


    }
    /// <summary>    
    /// Matthew Synborski
    /// CSE-681 Fall 2013 for Dr. Jim Fawcett
    /// 
    /// Description: TextAnalyzerJob Class - Container for TextAnalyzer Jobs.  
    /// This container class contains the processed contents of the user's input, 
    /// and any pertinent data read by the TextQuery
    /// </summary>
    public class TextAnalyzerJob : IJob
    {

        // Fields:
        private bool _recursive = false;
        private bool _mustFindAll = false;
        private String _queryType;
        private String _taskPath;
        private List<String> _extensionList = new List<String>();
        private List<String> _categoryList = new List<String>();
        private List<String> _keywordList = new List<String>();
        private List<String> _textQueryList = new List<String>();
        private List<String> _metadataQueryList = new List<String>();
        private List<String> _resultStringList = new List<String>();


        bool _switchO { get; set; }
        bool _switchA { get; set; }
        bool _switchT { get; set; }
        bool _switchM { get; set; }
        bool _switchX { get; set; }
        bool _switchF { get; set; }

        public static void Main()
        {
            TextAnalyzerJob tj = new TextAnalyzerJob();
            List<Switch_c> switches = new List<Switch_c>();

            Console.WriteLine("Demonstrating TextAnalyzerJob Class");
            Console.WriteLine("===================================");
            Console.WriteLine();

            Console.WriteLine("Populating a list of Switch_c objects\n");

            Directory.SetCurrentDirectory(Directory.GetCurrentDirectory() + "/../..");
            String currDir = Directory.GetCurrentDirectory();

            Switch_c sw1 = new Switch_c(); sw1.payload = "String"; sw1.switchChar = "T"; switches.Add(sw1);
            Switch_c sw2 = new Switch_c(); sw2.payload = "int"; sw2.switchChar = "T"; switches.Add(sw2);
            Switch_c sw3 = new Switch_c(); sw3.payload = ""; sw3.switchChar = "A"; switches.Add(sw3);
            Switch_c sw4 = new Switch_c(); sw4.payload = currDir; sw4.switchChar = "F"; switches.Add(sw4);
            Switch_c sw5 = new Switch_c(); sw5.payload = "txt"; sw5.switchChar = "X"; switches.Add(sw5);
            Switch_c sw6 = new Switch_c(); sw6.payload = "cs"; sw6.switchChar = "X"; switches.Add(sw6);
            Switch_c sw7 = new Switch_c(); sw7.payload = "doc"; sw7.switchChar = "X"; switches.Add(sw7);
            Switch_c sw8 = new Switch_c(); sw8.payload = ""; sw8.switchChar = "R"; switches.Add(sw8);

            Console.WriteLine("Populating the TextAnalyzerJob object\n");
            tj.setJobFromSwitches(switches);

            Console.WriteLine("Accessing the object\n");

            Console.Write("The file extensions to search are: { ");
            foreach (String s in tj.extensionList)
            {
                Console.Write(s + " ");
            }
            Console.WriteLine("}");

            Console.Write("The TextQueryJob is ");
            if (tj.recursive)
                Console.WriteLine("recursive.");
            else
                Console.WriteLine("not recursive.");

            Console.Write("The search terms to search are: { ");
            foreach (String s in tj.textQueryList)
            {
                Console.Write(s + " ");
            }
            Console.WriteLine("}");
        }



        public void DisplayUsage()
        {
            Console.WriteLine("TextAnalyzer by Matthew Synborski");
            Console.WriteLine("Usage: TextAnalyzer [OPTIONS]");
            Console.WriteLine("   or: TextAnalyzer path [OPTIONS]");
            Console.WriteLine("");
            Console.WriteLine("/R,           Recursively search");
            Console.WriteLine("/T,           Text query");
            Console.WriteLine("    /O,       -must contain one of text strings: default");
            Console.WriteLine("    /A,       -must contain all text strings");
            Console.WriteLine("/M,           Metadata query");
            Console.WriteLine("/C,           Category query");
            Console.WriteLine("/X,           File Extension (one /X for each extension)");
        }

        // Property implementation: 
        public String taskPath
        {
            get
            {
                return _taskPath;
            }

            set
            {
                _taskPath = value;
            }
        }

        // Property implementation: 
        public bool recursive
        {
            get
            {
                return _recursive;
            }

            set
            {
                _recursive = value;
            }
        }

        // Property implementation: 
        public String queryType
        {
            get
            {
                return _queryType;
            }

            set
            {
                _queryType = value;
            }
        }

        // Property implementation: 
        public List<String> extensionList
        {
            get
            {
                return _extensionList;
            }

            set
            {
                _extensionList = value;
            }
        }

        // Property implementation: 
        public List<String> categoryList
        {
            get
            {
                return _categoryList;
            }

            set
            {
                _categoryList = value;
            }
        }

        // Property implementation: 
        public List<String> keywordList
        {
            get
            {
                return _keywordList;
            }

            set
            {
                _keywordList = value;
            }
        }

        // Property implementation: 
        public List<String> textQueryList
        {
            get
            {
                return _textQueryList;
            }

            set
            {
                _textQueryList = value;
            }
        }

        // Property implementation: 
        public List<String> metadataQueryList
        {
            get
            {
                return _metadataQueryList;
            }

            set
            {
                _metadataQueryList = value;
            }
        }

        // Property implementation: 
        public bool mustFindAll
        {
            get
            {
                return _mustFindAll;
            }

            set
            {
                _mustFindAll = value;
            }
        }

        // Property implementation: 
        public List<String> resultStringList
        {
            get
            {
                return _resultStringList;
            }

            set
            {
                _resultStringList = value;
            }
        }

        private int handleSwitchO(Switch_c element)
        {
            int returncode = 0;
            if (_switchA == true)
            {
                returncode = -1;
                _resultStringList.Add("Specified conflicting find all/one option, /O and /A");
            }

            /*
            if (_switchM == true)
            {
                returncode = -1;
                _resultStringList.Add("Specified conflicting text find option /O or /A with metadata query switch /M");
            }
             */
            _mustFindAll = false;
            _switchO = true;
            return returncode;
        }

        private int handleSwitchA(Switch_c element)
        {
            int returncode = 0;
            if (_switchO == true)
            {
                returncode = -1;
                _resultStringList.Add("Specified conflicting find all/one option, /O and /A");
            }


            _mustFindAll = true;
            _switchA = true;
            return returncode;
        }


        private int handleSwitchF(Switch_c element)
        {
            int returncode = 0;
            if (_switchF == false)
            {
                _switchF = true;
            }
            _taskPath = element.payload;
            return returncode;
        }

        private int handleSwitchX(Switch_c element)
        {
            int returncode = 0;
            if (_switchX == false)
            {
                _extensionList.Clear();
            }
            _switchX = true;
            _extensionList.Add(element.payload);
            return returncode;
        }

        private int handleSwitchM(Switch_c element)
        {
            int returncode = 0;
            _queryType = "metadata";
            if (_metadataQueryList == null)
            {
                _metadataQueryList = new List<String>();
            }
            _metadataQueryList.Add(element.payload);
            _switchM = true;

            return returncode;
        }

        private int handleSwitchT(Switch_c element)
        {
            int returncode = 0;

            if (_switchT == false)
            {
                _extensionList.Clear();
                _extensionList.Add("cs");
                _extensionList.Add("txt");
            }

            _queryType = "text";
            if (_textQueryList == null)
            {
                _textQueryList = new List<String>();
            }
            _textQueryList.Add(element.payload);

            _switchT = true;
            return returncode;
        }


        public int setJobFromSwitches(List<Switch_c> _switches)
        {
            int returncode = -2;
            List<Switch_c> switches = new List<Switch_c>();
            switches = _switches;
                        
            foreach (Switch_c element in _switches)
            {

                // Handle query type switches
                if (element.switchChar == "T")
                {
                    returncode = handleSwitchT(element);
                }
                else if (element.switchChar == "M")
                {
                    returncode = handleSwitchM(element);
                }
                // Handle recursion switch
                else if (element.switchChar == "R")
                {
                    _recursive = true;
                }
                // Handle all/one options
                else if (element.switchChar == "O")
                {
                    returncode = handleSwitchO(element);
                }
                else if (element.switchChar == "A")
                {
                    returncode = handleSwitchA(element);
                }
                else if (element.switchChar == "X")
                {
                    returncode = handleSwitchX(element);
                }
                else if (element.switchChar == "F")
                {
                    returncode = handleSwitchF(element);
                }
                else
                {
                    returncode = -1;
                    StringBuilder sb = new StringBuilder();
                    sb.Append("Bad option: ");
                    sb.Append(element.switchChar);
                    sb.Append(" ");
                    sb.Append(element.payload);
                    _resultStringList.Add(sb.ToString());
                }

                if (returncode != 0)
                {
                    return returncode;
                }
            }
            return returncode;
        }
    }
}
