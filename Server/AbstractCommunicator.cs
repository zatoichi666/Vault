///////////////////////////////////////////////////////////////////////////////
// AbstractCommunicator.cs - Abstract base for message-passing communicator  //
//                                                                           //
// Jim Fawcett, CSE681 - Software Modeling and Analysis, Fall 2013           //
///////////////////////////////////////////////////////////////////////////////
/*
 *  Package Operations:
 *  -------------------
 *  This package contains three classes:
 *  AbstractCommunicator
 *    Serves as a base class for named objects that process messages on child threads.
 *  AbstractMessageDispatcher
 *    Is a base class for mediators that handle message dispatching.  The dispatcher
 *    is the first place a message goes when delivered by a communication channel.  
 *    Its job is to route messages based on the names of the intended targets.
 *  Utilities
 *    Provides an extendion method, Title(), for strings, that format titles for
 *    display.
 *  
 *  Required Files:
 *  - AbstractCommunicator.cs  Defines Communicator behavior
 *  - ICommLib.cs              Defines communication interface
 *  - ICommServiceLib.cs       Defines ServiceMessage class
 *
 *  Required References:
 *  - System.ServiceModel
 *  - System.RuntimeSerialization
 *
 *  Build Command:  devenv Project4HelpF13.sln /rebuild debug
 *
 *  Maintenance History:
 *  --------------------
 *  ver 1.0 : Oct 29, 2013 
 *  - first release
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using SWTools;

namespace DocumentVault
{
  ///////////////////////////////////////////////////////////////////////
  // AbstractCommunicator 
  // - defines parts that accept messages
  // - each Communicator has a thread for processing messages
  // - behavior is defined by application override of ProcessServiceMessages()

  public abstract class AbstractCommunicator : IComm
  {
    protected BlockingQueue<ServiceMessage> bq = new BlockingQueue<ServiceMessage>();
    protected Thread t = null;
    private string name_ = "";
    protected string currentUrl = "";

    public string Name    // property with default value ""
    {
      get
      {
        return name_;
      }
      set
      {
        name_ = value;
      }
    }

    public bool Verbose { get; set; }

    public void PostMessage(ServiceMessage msg)
    {
      bq.enQ(msg);
    }
    protected ServiceMessage GetMessage()
    {
      return bq.deQ();
    }
    public void Start()
    {
      t = new Thread(ProcessMessages);
      t.Start();
    }
    public void Stop()
    {
      ServiceMessage msg = ServiceMessage.MakeMessage(this.Name, this.Name, "quit");
      msg.TargetUrl = "http://localhost:8001/CommService";
      msg.SourceUrl = msg.TargetUrl;
      this.PostMessage(msg);
    }
    public void Wait()
    {
      t.Join();
    }

    abstract protected void ProcessMessages();
  }

  ///////////////////////////////////////////////////////////////////////
  // AbstractMessageDispatcher 
  // - serves as message routher between message-passing parts

  public abstract class AbstractMessageDispatcher : AbstractCommunicator
  {
    Dictionary<string, IComm> CommunicatorLookup = new Dictionary<string, IComm>();

    static AbstractMessageDispatcher reference_ = null;    // will hold reference to this

    protected AbstractMessageDispatcher()
    {
      reference_ = this;
    }
    //----< make MessageDispatcher available in any scope >----

    public static AbstractMessageDispatcher GetInstance()
    {
      return reference_; 
    }
    //----< register communicator for routing messages >-------

    public void Register(IComm communicator)
    {
      if (communicator.Name == this.Name)  // don't allow MessageDispatcher to register itself
        return;
      CommunicatorLookup