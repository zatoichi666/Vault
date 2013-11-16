/////////////////////////////////////////////////////////////
// MainWindow.xaml.cs - WPF Dispatcher Demo                //
//                                                         //
// Jim Fawcett, CSE775 - Distributed Objects, Spring 2011  //
/////////////////////////////////////////////////////////////

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


namespace WPF_DispatcherDemo
{
    public partial class NavWindow : Window
    {
        List<string> foundfiles = new List<string>();

        VaultModel vm = new VaultModel();
        Sender sender = new Sender();
        Receiver receiver = null;

        public NavWindow()
        {
            //ConsoleManager.ConsoleManager.Show();
            InitializeComponent();
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

            EchoCommunicator echo = new EchoCommunicator();
            echo.Name = "nav-echo";
            receiver.Register(echo);
            echo.Start();

            echo.gotMessage +=
                new EchoCommunicator.incomingMsgEventHandler(instanceHandler_OnIncomingNavEchoEvent);

            ServiceMessage msg3 =
              ServiceMessage.MakeMessage("nav", "ServiceClient", vm.ToXmlMessage().ToString(), "no name");
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
            XDocument xd = XDocument.Parse(((someEventArgs)seva).msg);

            List<String> FileList = new List<String>();
            var q1 = from x in
                         xd.Elements("Navigation")
                             //.Elements("FileList")
                         .Descendants("FileList")
                     select x;
            foreach (var elem in q1)
            {
                vm.SetFileList(elem.Value.Split(';').ToList<string>());
            }

            List<String> CategoryList = new List<String>();
            var q2 = from x in
                         xd.Elements("Navigation")
                         .Descendants("CategoryList")
                     select x;
            foreach (var elem in q2)
            {
                vm.SetCategories(elem.Value.Split(';').ToList<string>());
            }


            var q3 = from x in
                         xd.Elements("Navigation")
                         .Descendants("FileContent")
                     select x;
            foreach (var elem in q3)
            {

                vm._ContentFile = Encoding.UTF8.GetString(Convert.FromBase64String(elem.Value));

            }

            var q4 = from x in
                         xd.Elements("Navigation")
                         .Descendants("MetadataContent")
                     select x;
            foreach (var elem in q4)
            {

                vm._MetadataFile = Encoding.UTF8.GetString(Convert.FromBase64String(elem.Value));

            }

            var q5 = from x in
                         xd.Elements("Navigation")
                         .Descendants("ParentList")
                     select x;
            foreach (var elem in q5)
            {
                List<String> parentList = elem.Value.Split(';').ToList<string>().Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToList();
                if (elem.Value.Length > 0)
                    vm.SetParents(parentList);
                else
                    vm.SetParents(new List<string>());

            }

            var q6 = from x in
                         xd.Elements("Navigation")
                         .Descendants("ChildList")
                     select x;
            foreach (var elem in q6)
            {
                List<String> childList = elem.Value.Split(';').ToList<string>().Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToList();
                if (elem.Value.Length > 0)
                    vm.SetChildren(childList);
                else
                    vm.SetChildren(new List<string>());
            }

            Dispatcher.Invoke(DispatcherPriority.Normal, (Action)(() =>
        {
            paintCategories();
            paintFileList();
            paintContent();
        }));
        }

        void showPath(string path)
        {
            textBlock1.Text = path;
        }
        /*-- invoke on UI thread --------------------------------*/

        void addFile(string file)
        {
            listVaultFiles1.Items.Add(file);
        }

        public void paintCategories()
        {
            listCategory1.Items.Clear();
            foreach (String cat in vm.GetCategories())
            {
                listCategory1.Items.Add(cat);
            }
        }

        public void paintFileList()
        {
            listVaultFiles1.Items.Clear();
            foreach (String vfe in vm.GetVaultFileList())
            {
                listVaultFiles1.Items.Add(vfe);
            }
        }

        public void paintContent()
        {
            textBox2.Text = vm._ContentFile;
            textBox3.Text = vm._MetadataFile;

            ParentList.Items.Clear();

            try
            {

                Parallel.ForEach(vm.GetParents(), r => ParentList.Items.Add(r));
            }
            catch
            {

            }

            ChildList.Items.Clear();
            try
            {
                Parallel.ForEach(vm.GetChildren(), r => ChildList.Items.Add(r));
            }
            catch
            {

            }
        }

        /*-- Start search on asynchronous delegate's thread -----*/

        private void QueryButton_Click(object sender, RoutedEventArgs e)
        {
            QueryVaultServer qvs = new QueryVaultServer( this );
            qvs.Show();
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
                    RequestNav();
                }

            }
            catch (InvalidOperationException ex)
            {
                // index exception

            }
        }

        private void listCategory1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Update the vaultmodel
            vm._indexSelectedCategory = listCategory1.SelectedIndex;
            vm._indexSelectedContentFile = -1;

            listVaultFiles1.SelectedIndex = vm._indexSelectedContentFile;
            Console.Write(vm.ToXmlMessage());
        }

        private void QuitButton_Click(object sender, RoutedEventArgs e)
        {
            this.sender.Stop();
            this.sender.Wait();
            this.sender.Close();
            this.receiver.Close();
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

    }
}
