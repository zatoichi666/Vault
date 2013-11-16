///////////////////////////////////////////////////////////////////////////////
// Client.cs - Document Vault client prototype                               //
//                                                                           //
// Jim Fawcett, CSE681 - Software Modeling and Analysis, Fall 2013           //
///////////////////////////////////////////////////////////////////////////////
/*
 *  Package Contents:
 *  -----------------
 *  This package defines two classes:
 *  Client
 *    Defines the behavior of a DocumentVault prototype client.
 *  EchoCommunicator
 *    Defines prototype behavior for processing client received messages.
 *    In this demo it simply displays the message contents on the Console.
 *  
 * Required Files:
 * - Client:      Client.cs, Sender.cs, Receiver.cs
 * - Components:  ICommLib, AbstractCommunicator, BlockingQueue
 * - CommService: ICommService, CommService
 *
 *  Required References:
 *  - System.ServiceModel
 *  - System.RuntimeSerialization
 *
 *  Build Command:  devenv Project4HelpF13.sln /rebuild debug
 *
 *  Maintenace History:
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

namespace DocumentVault
{
  // Echo Communicator displays reply message

  class EchoCommunicator : AbstractCommunicator
  {
    protected override void ProcessMessages()
    {
      while (true)
      {
        ServiceMessage msg = bq.deQ();
        Console.Write("\n  {0} Recieved Message:\n", msg.TargetCommunicator);
        msg.ShowMessage();
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
  class Client
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

      ServiceMessage msg4 =
        ServiceMessage.MakeMessage("submit", "ServiceClient", "<root>some nav stuff</root>", "no name");
      msg4.SourceUrl = ClientUrl;
      msg4.TargetUrl = ServerUrl;
      Console.Write("\n  Posting message to \"{0}\" component", msg3.TargetCommunicator);
      sender.PostMessage(msg4);



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
