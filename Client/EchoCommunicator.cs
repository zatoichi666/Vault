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
                    if (Verbose)
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
