///////////////////////////////////////////////////////////////////////////////
// Sender.cs - Document Vault and Client Message Sender                      //
//                                                                           //
// Jim Fawcett, CSE681 - Software Modeling and Analysis, Fall 2013           //
///////////////////////////////////////////////////////////////////////////////
/*
 * This package is slightly different from ServerSender.cs.
 *
 * Required Files:
 * - Sender.cs                                               Handles outgoing messages
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
using System.Threading;

namespace DocumentVault
{
  public class Sender : AbstractCommunicator
  {
    ICommService svc = null;

    public Sender()
    {
      this.Name = "sender";
    }
    public bool Connect(string url)
    {
      for (int i = 0; i < 100; ++i)
      {
        try
        {
          svc = CreateProxy(url);
          return true;
        }
        catch
        {
          Thread.Sleep(100);
        }
      }
      return false;
    }
    static ICommService CreateProxy(string url)
    {
      BasicHttpBinding binding = new BasicHttpBinding();
      EndpointAddress address = new EndpointAddress(url);
      ChannelFactory<ICommService> factory = new ChannelFactory<ICommService>(binding, address);
      return factory.CreateChannel();
    }
    public void Close()
    {
      ServiceMessage msg = ServiceMessage.MakeMessage(this.Name, this.Name, "quit");
      this.PostMessage(msg);
      t.Join();
      ((IClientChannel)svc).Close();
    }
    protected override void ProcessMessages()
    {
      while (true)
      {
        ServiceMessage msg = bq.deQ();
        if (msg.TargetUrl != currentUrl)
        {
          Console.Write("\n  connecting to {0}", msg.TargetUrl);
          this.Connect(msg.TargetUrl);
          currentUrl = msg.TargetUrl;
        }
        svc.PostMessage(msg);
        if (msg.TargetCommunicator == this.Name && msg.Contents == "quit")
          break;
      }
    }
  }
}
