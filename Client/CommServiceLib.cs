///////////////////////////////////////////////////////////////////////////////
// CommServiceLib.cs - Document Vault communication service prototype        //
//                                                                           //
// Jim Fawcett, CSE681 - Software Modeling and Analysis, Fall 2013           //
///////////////////////////////////////////////////////////////////////////////
/*
 * Required Files:
 * - ICommLib.cs, AbstractCommunicator.cs, BlockingQueue   Defines AbstractMessageDispatcher
 * - ICommServiceLib.cs, CommServiceLib.cs                 Defines message-passing service
 *
 * Required References:
 * - System.ServiceModel
 * - System.Runtime.Serialization
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;

namespace DocumentVault
{
  [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession)]
  public class CommService : ICommService  //: IComm
  {
    AbstractMessageDispatcher dispatcher = null;

    public string Name { get; set; }

    //----< Create service and get reference to dispatcher >---------

    public CommService()
    {
      Name = "CommService";
      dispatcher = AbstractMessageDispatcher.GetInstance();
      /*
       *  A class that derives from AbstractMessageDispatcher must
       *  be created before calling GetInstance().  The service
       *  Host must do that.
       */
    }
    //----< Post a message to the MessageDispatcher >----------------

    public void PostMessage(ServiceMessage servMsg)
    {
      //Console.Write("\n  CommService.PostMessage called with Message:");
      //msg.ShowMessage();
      dispatcher.PostMessage(servMsg);
    }
  }
  //----< create ServiceHost and return to caller >------------------

  public class Host
  {
    public static ServiceHost CreateChannel(string url)
    {
      BasicHttpBinding binding = new BasicHttpBinding();
      Uri address = new Uri(url);
      Type service = typeof(CommService);
      ServiceHost host = new ServiceHost(service, address);
      host.AddServiceEndpoint(typeof(ICommService), binding, address);
      return host;
    }
  }
}
