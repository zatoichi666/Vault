///////////////////////////////////////////////////////////////////////////////
// Server.cs - Document Vault Server prototype                               //
//                                                                           //
// Jim Fawcett, CSE681 - Software Modeling and Analysis, Fall 2013           //
///////////////////////////////////////////////////////////////////////////////
/*
 *  Package Contents:
 *  -----------------
 *  This package defines four classes:
 *  Server:
 *    Provides prototype behavior for the DocumentVault server.
 *  EchoCommunicator:
 *    Simply diplays its messages on the server Console.
 *  QueryCommunicator:
 *    Serves as a placeholder for query processing.  You should be able to
 *    invoke your Project #2 query processing from the ProcessMessages function.
 *  NavigationCommunicator:
 *    Serves as a placeholder for navigation processing.  You should be able to
 *    invoke your navigation processing from the ProcessMessages function.
 * 
 *  Required Files:
 *  - Server:      Server.cs, Sender.cs, Receiver.cs
 *  - Components:  ICommLib, AbstractCommunicator, BlockingQueue
 *  - CommService: ICommService, CommService
 *
 *  Required References:
 *  - System.ServiceModel
 *  - System.RuntimeSerialization
 *
 *  Build Command:  devenv Project4HelpF13.sln /rebuild debug
 *
 *  Maintenace History:
 *  ver 2.1 : Nov 7, 2013
 *  - replaced ServerSender with a merged Sender class
 *  ver 2.0 : Nov 5, 2013
 *  - fixed bugs in the message routing process
 *  ver 1.0 : Oct 29, 2013
 *  - first release
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using TextAnalyzer;
using System.Xml.Linq;
using System.IO;
using System.Collections.Generic;


namespace DocumentVault
{
    // Echo Communicator

    class EchoCommunicator : AbstractCommunicator
    {
        protected override void ProcessMessages()
        {
            while (true)
            {
                ServiceMessage msg = bq.deQ();
                Console.Write("\n  {0} Recieved Message:\n", msg.TargetCommunicator);
                msg.ShowMessage();
                Console.Write("\n  Echo processing completed\n");
                if (msg.Contents == "quit")
                    break;
            }
        }
    }
    // Query Communicator

    class QueryCommunicator : AbstractCommunicator
    {
        IEnumerable<String> fTab;

        private TextAnalyzerJob DecodeQueryMessage(String QueryMessage)
        {
            TextAnalyzerJob tj = new TextAnalyzerJob();
            XDocument xd = XDocument.Parse(QueryMessage);
            IEnumerable<XElement> tq = xd.Element("Query").Elements("TextQuery");
            IEnumerable<XElement> mq = xd.Element("Query").Elements("MetadataQuery");
            String textQuery_s = "";
            foreach (XElement xe in tq)
            {
                Console.WriteLine("TextQuery terms: {0}", xe.Value);
                textQuery_s += xe.Value;
            }
            String[] separators = new String[] { ",", " " };
            String[] tqList = textQuery_s.Split(separators, StringSplitOptions.None);
            tqList = tqList.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToArray();

            String metadataQuery_s = "";
            foreach (XElement xe in mq)
            {
                Console.WriteLine("MetadataQuery terms: {0}", xe.Value);
                metadataQuery_s += xe.Value;
            }
            String[] mqList = metadataQuery_s.Split(separators, StringSplitOptions.None);
            mqList = mqList.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToArray();

            tj.textQueryList = tqList.ToList();
            tj.metadataQueryList = mqList.ToList();
            tj.mustFindAll = xd.Element("Query").Element("FindAll").Value == "true";
            tj.recursive = xd.Element("Query").Element("Recursive").Value == "true";

            Directory.SetCurrentDirectory(Directory.GetCurrentDirectory());
            String currDir = Directory.GetCurrentDirectory();
            tj.taskPath = currDir;
            Console.WriteLine("MetadataQuery terms: {0}", tj.taskPath);
            return tj;
        }
        /// <summary>
        /// Performs the query as requested by the user, returns a response containing hte results of the query
        /// Text Query will be performed regardless of the results of the Metadata Query
        /// Metadata Query will be performed regardless of the results of the Text Query
        /// Results will be combined for instances where a file contains a given metadata tag in the search while
        /// also containing text in the search
        /// </summary>
        protected override void ProcessMessages()
        {
            while (true)
            {
                ServiceMessage msg = bq.deQ();
                if (msg.Contents == "quit")
                    break;
                String[] fileExt = { "*.*" };
                Directory.SetCurrentDirectory(Directory.GetCurrentDirectory());
                String currDir = Directory.GetCurrentDirectory();
                TextAnalyzerJob tj = DecodeQueryMessage(msg.Contents);
                fTab = FileTable.GetFiles(currDir, fileExt, tj.recursive);
                foreach (String s in fTab) { Console.Write("{0}\n", s); }
                List<XmlSearchResult_c> tq = TextQuery.run(fTab, tj.textQueryList.ToArray(), tj.mustFindAll, tj.metadataQueryList.ToArray());
                XDocument xr = new XDocument(new XElement("Query"));
                foreach (XmlSearchResult_c s in tq)
                {
                    String filename = Path.GetFileName(s.filename);
                    XElement xe = new XElement("File", Path.GetFileName(filename));
                    if (s.textQueryFound)
                    {
                        String[] ls = s.tagAndValue.ToArray();
                        String[] sep = { ":" };
                        xe.Add(new XElement("TextQuerySuccess"));
                    }
                    if (s.tagAndValue.Count > 0)
                    {
                        String[] sep = { ":" };
                        foreach (String t in s.tagAndValue)
                        {
                            xe.Add(new XElement(
                            t.Split(sep, StringSplitOptions.None)[0],
                            t.Split(sep, StringSplitOptions.None)[1])
                            );
                        }
                    }
                    if (s.tagAndValue.Count > 0 || s.textQueryFound)
                    {
                        xr.Element("Query").Add(xe);
                    }
                }
                ServiceMessage reply = ServiceMessage.MakeMessage("query-echo", "query", xr.ToString());
                reply.TargetUrl = msg.SourceUrl;
                reply.SourceUrl = msg.TargetUrl;
                AbstractMessageDispatcher dispatcher = AbstractMessageDispatcher.GetInstance();
                dispatcher.PostMessage(reply);
            }
        }
    }
    // Navigate Communicator
    class NavigationCommunicator : AbstractCommunicator
    {
        private List<String> GetParents(String childFile)
        {
            List<String> Parents = new List<String>();
            List<String> fl = GetFileList("");
            List<String> metadataTerms = new List<String>();
            metadataTerms.Add("dependency");
            List<String> emptyText = new List<String>();
            List<XmlSearchResult_c> tq = TextQuery.run(fl, emptyText.ToArray(), false, metadataTerms.ToArray());
            String[] separators = { ":", ";", "dependency" };
            foreach (XmlSearchResult_c xsr in tq)
            {
                foreach (String s in xsr.tagAndValue)
                {
                    String[] cat2 = s.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                    foreach (String s2 in cat2)
                    {
                        if (childFile == s2)
                        {
                            Parents.Add(xsr.filename);
                        }
                    }
                }
            }
            return Parents;
        }

        /// <summary>
        /// Provides the list of files on the filesystem of the Server that have an associated .metadata file at the same level of the filesystem
        /// </summary>
        /// <param name="Category"></param> Pass an empty string ot get the list of files unfiltered by category.  Pass a string to filter by a single category
        /// <returns></returns>
        private List<String> GetFileList(String Category)
        {
            String[] fileExt = { "*.*" };
            Directory.SetCurrentDirectory(Directory.GetCurrentDirectory());
            String currDir = Directory.GetCurrentDirectory();
            IEnumerable<String> fTab = FileTable.GetFiles(currDir, fileExt, true);
            List<String> FileList = new List<String>();
            foreach (String s in fTab)
            {
                if (File.Exists(s + ".metadata"))
                {
                    FileList.Add(Path.GetFileName(s));
                }
            }
            // Filter the FileList by the category provided
            if (Category.Length > 0)
            {
                List<String> categoryInList = new List<String>();
                categoryInList.Add("categories");
                List<String> emptyText = new List<String>();
                List<XmlSearchResult_c> tq = TextQuery.run(FileList, emptyText.ToArray(), false, categoryInList.ToArray());
                FileList.Clear();
                foreach (XmlSearchResult_c xsr in tq)
                {
                    foreach (String s in xsr.tagAndValue)
                    {
                        String[] l1separators = { ":" };
                        String[] l2separators = { ";" };
                        String cat = s.Split(l1separators, StringSplitOptions.None)[1];
                        String[] categories = cat.Split(l2separators, StringSplitOptions.None);
                        foreach (String c in categories)
                        {
                            if (c == Category)
                                FileList.Add(xsr.filename);
                        }                        
                    }
                }
                XDocument xr = new XDocument(new XElement("Query"));
            }
            return FileList;
        }

        private List<String> GetCategoryList()
        {
            // Get the files without filtering by category
            List<String> fl = GetFileList("");
            foreach (String s in fl) { Console.Write("{0}\n", s); }
            List<String> metadataTerms = new List<String>();
            metadataTerms.Add("categories");

            List<String> emptyText = new List<String>();
            List<XmlSearchResult_c> tq = TextQuery.run(fl, emptyText.ToArray(), false, metadataTerms.ToArray());
            List<String> categories = new List<String>();
            foreach (XmlSearchResult_c xsr in tq)
            {
                //xsr.tagAndValue
                foreach (String s in xsr.tagAndValue)
                {
                    String[] separators = { ":" };
                    String[] separatorsl2 = { ";" };
                    String cat = s.Split(separators, StringSplitOptions.None)[1];
                    String[] cat2 = cat.Split(separatorsl2, StringSplitOptions.None);
                    foreach (String c in cat2)
                    {
                        if (!categories.Contains(c))
                        {
                            categories.Add(c);
                        }
                    }
                }
            }
            return categories;
        }

        // GetChildren() is implemented inline in the formation of the XDocument object

        protected override void ProcessMessages()
        {
            while (true)
            {
                ServiceMessage msg = bq.deQ();
                //Console.Write("\n  {0} Recieved Message:\n", msg.TargetCommunicator);
                msg.ShowMessage();

                if (msg.Contents == "quit")
                    break;

                XDocument xd = XDocument.Parse(msg.Contents);
                String CategorySelected = "";
                String FileNameSelected = "";

                var q1 = from x in
                             xd.Elements("ContentRequest")
                             .Descendants("Category")
                         select x;
                foreach (var elem in q1)
                {
                    CategorySelected = elem.Value;
                }

                var q2 = from x in
                             xd.Elements("ContentRequest")
                             .Descendants("File")

                         select x;
                foreach (var elem in q2)
                {
                    FileNameSelected = elem.Value;
                }

                // Filter the filelist by category
                
                String fileList = String.Join(";", GetFileList(CategorySelected));
                String categoryList = String.Join(";", GetCategoryList());



                // Get the File Content
                String FileContent = "";
                String MetadataContent = "";
                if (File.Exists(FileNameSelected))
                    FileContent = File.ReadAllText(FileNameSelected);

                if (File.Exists(FileNameSelected + ".metadata"))
                    MetadataContent = File.ReadAllText(FileNameSelected + ".metadata");

                String FileContent64enc = Convert.ToBase64String(Encoding.UTF8.GetBytes(FileContent));
                String Metadata64enc = Convert.ToBase64String(Encoding.UTF8.GetBytes(MetadataContent));

                List<String> ParentList = GetParents(FileNameSelected);

                String ChildList = "";
                // Get the dependencies
                try
                {
                    if (MetadataContent.Length > 0)
                    {

                        XDocument md = XDocument.Parse(MetadataContent);


                        var q4 = from x in
                                     md.Elements("metadata")
                                     .Descendants("dependency")

                                 select x;
                        foreach (var elem in q4)
                        {
                            ChildList += Path.GetFileName(elem.Value) + ';';
                        }
                    }
                }
                catch (InvalidDataException e)
                {

                }

                XDocument xr = new XDocument(
                    new XDeclaration("1.0", "utf-8", null),
                    new XElement("Navigation",
                    new XElement("FileList", fileList),
                    new XElement("CategoryList", categoryList),
                    new XElement("FileContent", FileContent64enc),
                    new XElement("MetadataContent", Metadata64enc),
                    new XElement("ChildList", ChildList),
                    new XElement("ParentList", String.Join(";", ParentList))
                ));

                Console.WriteLine(xr.ToString());

                ServiceMessage reply = ServiceMessage.MakeMessage("nav-echo", "nav", xr.ToString());
                reply.TargetUrl = msg.SourceUrl;
                reply.SourceUrl = msg.TargetUrl;
                AbstractMessageDispatcher dispatcher = AbstractMessageDispatcher.GetInstance();
                dispatcher.PostMessage(reply);
            }
        }
    }
    // Server


    class SubmissionCommunicator : AbstractCommunicator
    {
        bool SubmitFileFromSubmissionMessage(ServiceMessage SubmissionMessage)
        {
            // Extract the file content and the XML file content
            string[] separators = new string[] { "[", "]" };
            string[] terms = SubmissionMessage.Contents.Split(separators, StringSplitOptions.None);

            try
            {
                String XmlString = terms[0];
                String FileContent = terms[1];

                // Create an XDocument from the xml content
                XDocument xd = XDocument.Parse(XmlString);

                var data = from item in
                               xd.Descendants("metadata")
                               .Elements("filename")
                           select item;

                String FilePath = data.First().Value;
                string[] FileNames = FilePath.Split('\\');
                string FileName = FileNames.Last();

                // Write the metadata file to the server disk
                xd.Save(FileName + ".metadata");

                // Write the content file to disk
                File.WriteAllText(FileName, FileContent);
                return true;
            }
            catch
            {
                Console.Write("\n(!) Caught String Exception");
                return false;
            }
        }

        protected override void ProcessMessages()
        {
            while (true)
            {
                ServiceMessage msg = bq.deQ();

                if (msg.Contents == "quit")
                    break;

                Console.Write("\n  {0} Recieved Message:\n", msg.TargetCommunicator);
                //msg.ShowMessage();

                bool result = SubmitFileFromSubmissionMessage(msg);



                ServiceMessage reply = ServiceMessage.MakeMessage("submit-echo", "submit", result.ToString());
                reply.TargetUrl = msg.SourceUrl;
                reply.SourceUrl = msg.TargetUrl;
                AbstractMessageDispatcher dispatcher = AbstractMessageDispatcher.GetInstance();
                dispatcher.PostMessage(reply);
            }
        }
    }

    class Server
    {


        static void Main(string[] args)
        {
            Console.Write("\n  Starting CommService");
            Console.Write("\n ======================\n");

            string ServerUrl = "http://localhost:8000/CommService";
            Receiver receiver = new Receiver(ServerUrl);

            string ClientUrl = "http://localhost:8001/CommService";

            Sender sender = new Sender();
            sender.Name = "sender";
            sender.Connect(ClientUrl);
            receiver.Register(sender);
            sender.Start();

            // Test Component that simply echos message

            EchoCommunicator echo = new EchoCommunicator();
            echo.Name = "echo";
            receiver.Register(echo);
            echo.Start();

            // Placeholder for query processor

            QueryCommunicator query = new QueryCommunicator();
            query.Name = "query";
            receiver.Register(query);
            query.Start();

            // Placeholder for component that searches for and returns 
            // parent/child relationships

            NavigationCommunicator nav = new NavigationCommunicator();
            nav.Name = "nav";
            receiver.Register(nav);
            nav.Start();

            // Placeholder for component that handles submissions for and returns 
            // parent/child relationships

            SubmissionCommunicator submit = new SubmissionCommunicator();
            submit.Name = "submit";
            receiver.Register(submit);
            submit.Start();


            Console.Write("\n  Started CommService - Press key to exit:\n ");
            Console.ReadKey();


        }
    }
}
