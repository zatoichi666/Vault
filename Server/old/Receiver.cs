///////////////////////////////////////////////////////////////////////////////
// Receiver.cs - Document Vault Message Receiver                             //
//                                                                           //
// Jim Fawcett, CSE681 - Software Modeling and Analysis, Fall 2013           //
///////////////////////////////////////////////////////////////////////////////
/*
 * Required Files:
 * - Receiver.cs                                             Handles incoming messages
 * - ICommLib.cs, AbstractCommunicator.cs, BlockingQueue.cs  Defines behavior of Communicators 
 * - ICommServiceLib.cs, CommServiceLibe.cs                  Defines message-passing service
 *
 *  Required References:
 *  - System.ServiceModel
 *  - System.Runtime.Serialization
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using System.Runtime.Serialization;

namespace DocumentVault
{
  public class VaultDispatcher : AbstractMessageDispatcher { }

  public class Receiver
  {
    public VaultDispatcher dispatcher { get; set; }
    ServiceHost host = null;

    public Receiver(string url)
    {
      dispatcher = new VaultDispatcher();
      dispatcher.Verbose = true;
      dispatcher.Name = "dispatcher";
      dispatcher.Start();

      ServiceHost host = null;
      try
      {
        host = Host.CreateChannel(url);
        host.Open();
      }
      catch (Exception ex)
      {
        Console.Write("\n\n  {0}", ex.Message);
      }
      finally
      {
        //host.Close();
      }
    }
    public void Stop()
    {
      dispatcher.Stop();
    }
    public void Close()
    {
      host.Close();
    }
    public void Register(IComm communicator)
    {
      dispatcher.Register(communicator);
    }
  }
}
