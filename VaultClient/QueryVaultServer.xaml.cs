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
using System.Xml.Linq;
using DocumentVault;
using TextAnalyzer;



namespace WPF_DispatcherDemo
{
    /// <summary>
    /// Interaction logic for QueryVaultServer.xaml
    /// </summary>
    public partial class QueryVaultServer : Window
    {

        private NavWindow _mw;

        public QueryVaultServer(NavWindow mw)
        {
            InitializeComponent();
            _mw = mw;

        }

        private void QueryButton_Click(object sender, RoutedEventArgs e)
        {
            Sender msgsender = new Sender();
            Receiver receiver = null;

            string ServerUrl = "http://localhost:8000/CommService";
            msgsender.Connect(ServerUrl);
            msgsender.Start();

            string ClientUrl = "http://localhost:8001/CommService";
            receiver = new Receiver(ClientUrl);


            bool findAllOption = (bool)All.IsChecked;
            bool recursiveOption = (bool)RecursiveCheckbox.IsChecked;

            XDocument xd = new XDocument(
                new XElement("Query",
                    new XElement("TextQuery", TextQueryTextBox.Text),
                    new XElement("MetadataQuery", MetadataQueryTextBox.Text),
                    new XElement("FindAll", findAllOption),
                    new XElement("Recursive", recursiveOption)));

            EchoCommunicator echo = new EchoCommunicator();
            echo.Name = "query-echo";
            receiver.Register(echo);
            echo.Start();
            echo.gotMessage +=
                new EchoCommunicator.incomingMsgEventHandler(instanceHandler_OnIncomingEchoEvent);
            ServiceMessage msg4 =
            ServiceMessage.MakeMessage("query", "ServiceClient", xd.ToString(), "no name");
            msg4.SourceUrl = "http://localhost:8001/CommService";
            msg4.TargetUrl = "http://localhost:8000/CommService";
            msgsender.PostMessage(msg4);
        }

        void instanceHandler_OnIncomingEchoEvent(object obj, EventArgs seva)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, (Action)(() =>
            {
                try
                {
                    QueryResults.Items.Clear();
                    XDocument xd = XDocument.Parse(((someEventArgs)seva).msg);
                    var q = from x in
                                xd.Elements("Query")
                                .Descendants("File")
                            select x;
                    foreach (var elem in q)
                    {
                        QueryResults.Items.Add(elem.ToString());
                    }
                }
                catch
                {
                    // XDocument Exception
                }
            }));


        }


        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void QueryResults_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, (Action)(() =>
            {
                try
                {
                    XDocument xd = XDocument.Parse(QueryResults.SelectedItem.ToString());
                    String FileNameSelected = xd.Element("File").FirstNode.ToString();

                    _mw.setCurrentFileName(FileNameSelected);

                }
                catch
                {
                    // XDocument Exception
                }

            }));
        }
    }
}
