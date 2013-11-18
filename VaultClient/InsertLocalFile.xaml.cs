///////////////////////////////////////////////////////////////////////////////
// InsertLocalFile.xaml.cs - Document Vault client codebehind                //
//  Provides minor window paint logic for Insert View.                       //
//                                                                           //
// Matthew Synborski - Software Modeling and Analysis, Fall 2013             //
///////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using TextAnalyzer;
using DocumentVault;

namespace DocumentVault
{
    /// <summary>
    /// Interaction logic for InsertLocalFile.xaml
    /// </summary>
    public partial class InsertLocalFile : Window
    {
        private String _Content;
        private NavWindow _mw;
        EchoCommunicator echo = new EchoCommunicator();
        
        public String pContentText
        {
            get { return _Content; }
            set
            {
                if (value != _Content)
                {
                    _Content = value;

                }
            }
        }

        public InsertLocalFile(String fullFileName, NavWindow mw)
        {            
            InitializeComponent();
            _mw = mw;
            DateTime dt = File.GetLastWriteTime(fullFileName);
            String fileContent = File.ReadAllText(fullFileName);
            ContentText1.Text = fileContent;             
            MetadataGenJob mj = new MetadataGenJob();
            // Make the Metadata            
            List<Switch_c> switches = new List<Switch_c>();
            //Get the filename of the selected file
            Switch_c sw1 = new Switch_c(); sw1.payload = fullFileName; sw1.switchChar = "F"; switches.Add(sw1);
            //Get the file size in bytes of the selected file
            Switch_c sw2 = new Switch_c(); sw2.payload = fileContent.Length.ToString(); sw2.switchChar = "S"; switches.Add(sw2);
            //Get the date/time string of the selected file last access time
            Switch_c sw3 = new Switch_c(); sw3.payload = dt.ToString("yyyyMMddhhmmss"); sw3.switchChar = "T"; switches.Add(sw3);
            Switch_c sw4 = new Switch_c(); sw4.payload = ""; sw4.switchChar = "D"; switches.Add(sw4);
            mj.setJobFromSwitches(switches);
            String metadataUnformatted = mj.GenerateMetadata().OuterXml;
            MemoryStream mStream = new MemoryStream();
            XmlTextWriter writer = new XmlTextWriter(mStream, Encoding.Unicode);
            XmlDocument document = new XmlDocument();
            try
            {
                // Load the XmlDocument with the XML.
                document.LoadXml(metadataUnformatted);
                writer.Formatting = Formatting.Indented;
                // Write the XML into a formatting XmlTextWriter
                document.WriteContentTo(writer);
                writer.Flush();
                mStream.Flush();
                // Have to rewind the MemoryStream in order to read
                // its contents.
                mStream.Position = 0;
                // Read MemoryStream contents into a StreamReader.
                StreamReader sReader = new StreamReader(mStream);
                // Extract the text from the StreamReader.
                String FormattedXML = sReader.ReadToEnd();
                MetadataText1.Text = FormattedXML;
            }
            catch (XmlException) { }            
        }

        private void CancelAddMetadata1_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void AddMetadata1_Click(object sender, RoutedEventArgs e)
        {
            // Evaluate the .xml content of the metadata textbox. 
            // If violates a schema for the metadata format, show an error dialog
            // Otherwise put the xml file path into the main window listbox
            try
            {
                XDocument xd = XDocument.Parse(MetadataText1.Text);

                String submitMsg = "~" + xd.FirstNode.ToString() + "[" + ContentText1.Text + "]";


                Sender msgsender = new Sender();
                Receiver receiver = null;

                string ServerUrl = "http://localhost:8000/CommService";
                msgsender.Connect(ServerUrl);
                msgsender.Start();

                string ClientUrl = "http://localhost:8001/CommService";
                receiver = new Receiver(ClientUrl);

                
                echo.Name = "submit-echo";
                receiver.Register(echo);
                echo.Start();
                echo.gotMessage +=
                    new EchoCommunicator.incomingMsgEventHandler(instanceHandler_OnIncomingEchoEvent);
                
                ServiceMessage msg4 =
                ServiceMessage.MakeMessage("submit", "ServiceClient", submitMsg, "no name");
                msg4.SourceUrl = "http://localhost:8001/CommService";
                msg4.TargetUrl = "http://localhost:8000/CommService";
                msgsender.PostMessage(msg4);
                
                this.Close();
            }
            catch (XmlException)
            {
                
            }            
                      

        }

        void instanceHandler_OnIncomingEchoEvent(object obj, EventArgs seva)
        {

                try
                {
                    
                    String FileNameSelected = "";
                    XDocument xd = XDocument.Parse(MetadataText1.Text);
                                    var q2 = from x in
                                                 xd.Elements("metadata")
                    .Descendants("filename")
                    
                select x;
                foreach (var elem in q2)
                {
                    FileNameSelected = System.IO.Path.GetFileName(elem.Value);                    
                }

                _mw.setCurrentFileName(FileNameSelected);     
                    //QueryResults.Items.Clear();
                }
                catch { }


        }


        public void InsertLocalFile_Unloaded(object sender, RoutedEventArgs e)
        {
            echo.Stop();

            this.Close();
        }
                
    }
}







