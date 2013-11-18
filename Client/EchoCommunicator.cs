///////////////////////////////////////////////////////////////////////////////
// EchoCommunicator.cs - Document Vault client communicator                  //
//                                                                           //
// Modified by Matthew Synborski                                             //
// Jim Fawcett, CSE681 - Software Modeling and Analysis, Fall 2013           //
///////////////////////////////////////////////////////////////////////////////
/*
 *  Package Contents:
 *  -----------------
 *  This package defines two classes:
 *  someEventArgs:
 *    Container class for event handling with EchoCommunicator.
 * 
 *  EchoCommunicator:
 *    Serves as communicator for response processing at the Vault Client 
 *    (console or WPF GUI).  Handles responses from the NavigationCommunicator, 
 *    QueryCommunicator or SubmissionCommunicator.
 *  
 * Required Files:
 * - Client:      EchoCommunicator.cs, Sender.cs, Receiver.cs
 * - Components:  ICommLib, AbstractCommunicator, BlockingQueue
 * - CommService: ICommService, CommService
 *
 *  Required References:
 *  - System.ServiceModel
 *  - System.RuntimeSerialization
 *
 *  Build Command:  devenv Vault.sln /rebuild debug
 *
 *  Maintenance History:
 *  ver 3.0 : Nov 17, 2013
 *  ver 2.1 : Nov 7, 2013
 *  - replaced ClientSender with a merged Sender class
 *  ver 2.0 : Nov 5, 2013
 *  - fixed bugs in the client shutdown process
 *  ver 1.0 : Oct 29, 2013
 *  - first release
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using System.Runtime.Serialization;
using System.Threading;
using System.Xml.Linq;

namespace DocumentVault
{

    public class someEventArgs : EventArgs
    {
        private string _msg;
        public someEventArgs(string msg)
        {
            _msg = msg;
        }
        public string msg
        {
            get
            { return _msg; }
            set
            { _msg = value; }
        }
    }
    // Echo Communicator displays reply message

    public class EchoCommunicator : AbstractCommunicator
    {
        public delegate void incomingMsgEventHandler(object sender, EventArgs seva);
        public event incomingMsgEventHandler gotMessage;

        private void ExtractFileList(ref VaultModel vm, XDocument xd)
        {
            var q1 = from x in
                         xd.Elements("Navigation")
                             //.Elements("FileList")
                         .Descendants("FileList")
                     select x;
            foreach (var elem in q1) { vm.SetFileList(elem.Value.Split(';').ToList<string>()); }
        }

        private void ExtractCategoryList(ref VaultModel vm, XDocument xd)
        {
            var q2 = from x in
                         xd.Elements("Navigation").Descendants("CategoryList")
                     select x;
            foreach (var elem in q2) { vm.SetCategories(elem.Value.Split(';').ToList<string>()); }
        }

        private void ExtractFileContent(ref VaultModel vm, XDocument xd)
        {
            var q3 = from x in
                         xd.Elements("Navigation").Descendants("FileContent")
                     select x;
            foreach (var elem in q3) { vm._ContentFile = Encoding.UTF8.GetString(Convert.FromBase64String(elem.Value)); }
        }

        private void ExtractMetadataContent(ref VaultModel vm, XDocument xd)
        {
            var q4 = from x in
                         xd.Elements("Navigation").Descendants("MetadataContent")
                     select x;
            foreach (var elem in q4) { vm._MetadataFile = Encoding.UTF8.GetString(Convert.FromBase64String(elem.Value)); }
        }

        private void ExtractParentList(ref VaultModel vm, XDocument xd)
        {
            var q5 = from x in
                         xd.Elements("Navigation").Descendants("ParentList")
                     select x;
            foreach (var elem in q5)
            {
                List<String> parentList = elem.Value.Split(';').ToList<string>().Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToList();
                if (elem.Value.Length > 0)
                { vm.SetParents(parentList); }
                else
                { vm.SetParents(new List<string>()); }
            }
        }

        private void ExtractChildList(ref VaultModel vm, XDocument xd)
        {
            var q6 = from x in
                         xd.Elements("Navigation").Descendants("ChildList")
                     select x;
            foreach (var elem in q6)
            {
                List<String> childList = elem.Value.Split(';').ToList<string>().Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToList();
                if (elem.Value.Length > 0)
                { vm.SetChildren(childList); }
                else
                { vm.SetChildren(new List<string>()); }
            }
        }

        public void HandleMessage(String message, ref VaultModel vm)
        {
            XDocument xd = XDocument.Parse(message);
            List<String> FileList = new List<String>();

            List<String> CategoryList = new List<String>();
            this.ExtractFileList(ref vm, xd);
            this.ExtractCategoryList(ref vm, xd);
            this.ExtractFileContent(ref vm, xd);
            this.ExtractMetadataContent(ref vm, xd);
            this.ExtractParentList(ref vm, xd);
            this.ExtractChildList(ref vm, xd);

        }

        protected override void ProcessMessages()
        {
            while (true)
            {
                ServiceMessage msg = bq.deQ();
                Console.Write("\n  {0} Recieved Message:\n", msg.TargetCommunicator);
                msg.ShowMessage();

                // fire the event gotMessage
                someEventArgs seva = new someEventArgs(msg.Contents);
                if (gotMessage != null)
                    gotMessage(this, seva);
                                
                if (msg.Contents == "quit")
                {
                    //if (Verbose)
                        Console.Write("\n  Echo shutting down");

                    // shut down dispatcher
                    msg.TargetCommunicator = "dispatcher";
                    AbstractMessageDispatcher.GetInstance().PostMessage(msg);
                    break;
                }

                

            }
        }
    }

}
