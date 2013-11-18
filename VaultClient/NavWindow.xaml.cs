///////////////////////////////////////////////////////////////////////////////
// NavWindow.xaml.cs - Document Vault client codebehind                      //
//  Provides minor window paint logic for Main Navigation View.              //
//                                                                           //
// Matthew Synborski - Software Modeling and Analysis, Fall 2013             //
///////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;
using System.Windows.Threading;
using System.Threading.Tasks;
using System.IO;
using ConsoleManager;
using DocumentVault;
using TextAnalyzer;
using FileTransferService;

namespace DocumentVault
{
    public partial class NavWindow : Window
    {
        QueryVaultServer qvs = null;
        List<string> foundfiles = new List<string>();
        EchoCommunicator echo = new EchoCommunicator();
        VaultModel vm = new VaultModel();
        Sender sender = new Sender();
        Receiver receiver = null;
        

        public NavWindow()
        {
            //ConsoleManager.ConsoleManager.Show();
            InitializeComponent();
            this.Unloaded += new RoutedEventHandler(NavWindow_Unloaded);
  
            
            RequestNav();

        }
        /*-- invoke on UI thread --------------------------------*/

        public void RequestNav()
        {

            string ServerUrl = "http://localhost:8000/CommService";
            sender.Connect(ServerUrl);
            sender.Start();

            string ClientUrl = "http://localhost:8001/CommService";
            receiver = new Receiver(ClientUrl);
            echo.Name = "nav-echo";
            receiver.Register(echo);
            echo.Start();
            echo.gotMessage +=
                new EchoCommunicator.incomingMsgEventHandler(instanceHandler_OnIncomingNavEchoEvent);
            ServiceMessage msg3 =
              ServiceMessage.MakeMessage("nav", "ServiceClient", "~" + vm.ToXmlMessage().ToString(), "no name");
            msg3.SourceUrl = ClientUrl;
            msg3.TargetUrl = ServerUrl;
            sender.PostMessage(msg3);
        }

        public void setCurrentFileName(String filename)
        {
            vm.UpdateFileIndex(filename);
            RequestNav();
        }

 

        void instanceHandler_OnIncomingNavEchoEvent(object obj, EventArgs seva)
        {
            // Handle the incoming message
            if (!((someEventArgs)seva).msg.Equals("quit"))
            {
                echo.HandleMessage(((someEventArgs)seva).msg, ref vm);


                paintCategories();
                paintFileList();
                paintContent();
            }
        }

        public void paintCategories()
        {

            Dispatcher.Invoke(DispatcherPriority.Normal, (Action)(() =>
            {

                listCategory1.Items.Clear();
                foreach (String cat in vm.GetCategories())
                {
                    listCategory1.Items.Add(cat);
                }
            }));

        }

        public void paintFileList()
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, (Action)(() =>
{

    listVaultFiles1.Items.Clear();
    foreach (String vfe in vm.GetVaultFileList())
    {
        listVaultFiles1.Items.Add(vfe);
    }
}));

        }

        public void paintContent()
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, (Action)(() =>
{

    textBox2.Text = vm._ContentFile;
    textBox3.Text = vm._MetadataFile;
    ParentList.Items.Clear();
    try
    {
        Parallel.ForEach(vm.GetParents(), r => ParentList.Items.Add(r));
    }
    catch { }
    ChildList.Items.Clear();
    try
    {
        Parallel.ForEach(vm.GetChildren(), r => ChildList.Items.Add(r));
    }
    catch { }
}));

        }

        /*-- Start search on asynchronous delegate's thread -----*/

        private void QueryButton_Click(object sender, RoutedEventArgs e)
        {

            if (qvs == null)
            {
                qvs = new QueryVaultServer(this);
                this.Unloaded += new RoutedEventHandler(qvs.QueryVaultServer_Unloaded);
                qvs.Show();
            }
            else
            {
                qvs.Focus();
            }
            
        }

        private void InsertButton_Click(object sender, RoutedEventArgs e)
        {
            // Show the file dialog
            listVaultFiles1.Items.Clear();
            OpenFileDialog dlg = new OpenFileDialog();
            string path = AppDomain.CurrentDomain.BaseDirectory;
            dlg.FileName = path;
            DialogResult result = dlg.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                path = dlg.FileName;
                // Spawn the window for the InsertLocalFile portion of Vault Client
                InsertLocalFile ilf = new InsertLocalFile(path, this);
                this.Unloaded += new RoutedEventHandler(ilf.InsertLocalFile_Unloaded);
                ilf.Show();
            }



        }

        private void listVaultFiles1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {


            try
            {
                if (listVaultFiles1.SelectedIndex > -1)
                {
                    vm.UpdateFileIndex(listVaultFiles1.SelectedItem.ToString());
                    e.Handled = true;
                    RequestNav();
                }
            }
            catch { }



        }

        private void listCategory1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {


            // Update the vaultmodel
            vm._indexSelectedCategory = listCategory1.SelectedIndex;
            e.Handled = true;



        }

        private void kill()
        {
            string ServerUrl = "http://localhost:8000/CommService";
            sender.Connect(ServerUrl);
            sender.Start();
            string ClientUrl = "http://localhost:8001/CommService";
            receiver = new Receiver(ClientUrl);
            EchoCommunicator echo = new EchoCommunicator();
            echo.Name = "nav-echo";
            receiver.Register(echo);
            echo.Start();
            ServiceMessage msg4 =
  ServiceMessage.MakeMessage("echo", "ServiceClient", "exit", "no name");
            msg4.SourceUrl = ClientUrl;
            msg4.TargetUrl = ServerUrl;
            sender.PostMessage(msg4);
        }

        public void QuitButton_Click(object sender, RoutedEventArgs e)
        {
            echo.Stop();
            
            // kill(); // Don't use, assumes just one client, ever.
            this.sender.Stop();
            this.sender.Wait();
            this.sender.Close();
            this.receiver.Close();
            System.Windows.Application.Current.Shutdown();

            this.Close();
        }

        private void ConsoleShow_Unchecked(object sender, RoutedEventArgs e)
        {
            ConsoleManager.ConsoleManager.Toggle();
        }

        private void ConsoleShow_Checked(object sender, RoutedEventArgs e)
        {
            ConsoleManager.ConsoleManager.Toggle();
        }

        private void NavButton_Click(object sender, RoutedEventArgs e)
        {
            RequestNav();
        }

        private void ChildList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (ChildList.SelectedIndex > -1)
            {
                vm.UpdateFileIndex(ChildList.SelectedItem.ToString());
                RequestNav();
            }



        }

        private void ParentList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {


            if (ParentList.SelectedIndex > -1)
            {
                vm.UpdateFileIndex(ParentList.SelectedItem.ToString());
                RequestNav();
            }

        }

        private void InsertNewMetadataFile(String ContentFilename)
        {
            String submitMsg = "~" + textBox3.Text + "[";
            try
            {
                String fileContent = File.ReadAllText(ContentFilename);
                submitMsg += fileContent ;
            }
            catch { }
            submitMsg += "]";
            Sender msgsender = new Sender();
            //Receiver receiver = null;
            string ServerUrl = "http://localhost:8000/CommService";
            msgsender.Connect(ServerUrl);
            msgsender.Start();
            string ClientUrl = "http://localhost:8001/CommService";
            receiver = new Receiver(ClientUrl);
            EchoCommunicator echo = new EchoCommunicator();
            echo.Name = "submit-echo";
            receiver.Register(echo);
            echo.Start();
            
            ServiceMessage msg4 =
            ServiceMessage.MakeMessage("submit", "ServiceClient", submitMsg, "no name");
            msg4.SourceUrl = "http://localhost:8001/CommService";
            msg4.TargetUrl = "http://localhost:8000/CommService";
            msgsender.PostMessage(msg4);
            echo.Stop();

            RequestNav();
        }



        private void InsertTag_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                XDocument xd = XDocument.Parse(textBox3.Text);
                var elements = xd.Descendants(TagCombo.Text)
                          .ToList();
                elements.Where(x => x.Name == TagCombo.Text).Remove();
                xd.Element("metadata").Add(new XElement(TagCombo.Text, TagContent.Text));

                textBox3.Text = xd.ToString();

                String filename = "";
                var q4 = from x in
                             xd.Elements("metadata")
                             .Descendants("filename")
                         select x;
                foreach (var elem in q4)
                {
                    filename = System.IO.Path.GetFileName(elem.Value);
                }
                InsertNewMetadataFile(filename);
                //RequestNav();
            }
            catch { }
        }

        void NavWindow_Unloaded(object sender, RoutedEventArgs e)
        {
            echo.Stop();

            // kill(); // Don't use, assumes just one client, ever.
            this.sender.Stop();
            this.sender.Wait();
            this.sender.Close();
            this.receiver.Close();
            System.Windows.Application.Current.Shutdown();

            this.Close();
        }

    }
}
