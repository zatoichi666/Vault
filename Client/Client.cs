///////////////////////////////////////////////////////////////////////////////
// Client.cs - Document Vault client prototype                               //
//                                                                           //
// Modified by Matthew Synborski                                             //
// Jim Fawcett, CSE681 - Software Modeling and Analysis, Fall 2013           //
///////////////////////////////////////////////////////////////////////////////
/*
 *  Package Contents:
 *  -----------------
 *
 *  Client:
 *    Test client (console app) that exercises the EchoCommunicator class, and 
 *    interacts with the Vault Server.
 *  
 * Required Files:
 * - Client:      Client.cs, Sender.cs, Receiver.cs
 * - Components:  EchoCommunicator.cs, ICommLib, AbstractCommunicator, BlockingQueue
 * - CommService: ICommService, CommService
 *
 *  Required References:
 *  - System.ServiceModel
 *  - System.RuntimeSerialization
 *
 *  Build Command:  devenv Vault.sln /rebuild debug
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocumentVault
{
    public class Client
    {
        static void Main(string[] args)
        {
            Console.Write("\n  Starting CommService Client");
            Console.Write("\n =============================\n");

            string ServerUrl = "http://localhost:8000/CommService";
            Sender sender = null;

            Console.Write("\n  Press key to start client: ");
            Console.ReadKey();

            sender = new Sender();
            sender.Connect(ServerUrl);
            sender.Start();

            string ClientUrl = "http://localhost:8001/CommService";
            Receiver receiver = new Receiver(ClientUrl);

            // Don't need to start receiver unless you want
            // to send it messages, which we won't as all
            // our messages go to the server
            //receiver.Start();

            EchoCommunicator echo = new EchoCommunicator();
            echo.Name = "client-echo";
            receiver.Register(echo);
            echo.Start();

            EchoCommunicator echo1 = new EchoCommunicator();
            echo1.Name = "submit-echo";
            receiver.Register(echo1);
            echo1.Start();

            EchoCommunicator echo2 = new EchoCommunicator();
            echo2.Name = "nav-echo";
            receiver.Register(echo2);
            echo2.Start();

            EchoCommunicator echo3 = new EchoCommunicator();
            echo3.Name = "query-echo";
            receiver.Register(echo3);
            echo3.Start();


            ServiceMessage msg1 =
              ServiceMessage.MakeMessage("echo", "ServiceClient", "<root>some echo stuff</root>", "no name");
            msg1.SourceUrl = ClientUrl;
            msg1.TargetUrl = ServerUrl;
            Console.Write("\n  Posting message to \"{0}\" component", msg1.TargetCommunicator);
            sender.PostMessage(msg1);

            ServiceMessage msg2 =
              ServiceMessage.MakeMessage("query", "ServiceClient", "<root>some query stuff</root>", "no name");
            msg2.SourceUrl = ClientUrl;
            msg2.TargetUrl = ServerUrl;
            Console.Write("\n  Posting message to \"{0}\" component", msg2.TargetCommunicator);
            sender.PostMessage(msg2);

            ServiceMessage msg3 =
              ServiceMessage.MakeMessage("nav", "ServiceClient", "<root>some nav stuff</root>", "no name");
            msg3.SourceUrl = ClientUrl;
            msg3.TargetUrl = ServerUrl;
            Console.Write("\n  Posting message to \"{0}\" component", msg3.TargetCommunicator);
            sender.PostMessage(msg3);

            // wait for all server replies to be sent back

            Console.Write("\n  Wait for Server replies, then press a key to exit: ");
            Console.ReadKey();
            sender.Stop();  // this function sends a quit message to client-echo
            sender.Wait();
            echo.Stop();
            echo.Wait();
            receiver.Close();
            Console.Write("\n\n");
        }
    }
}
