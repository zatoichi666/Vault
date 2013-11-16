/////////////////////////////////////////////////////////////////////////////
// ICommServiceLib.cs - interface for Document Vault communication service //
//                                                                         //
// Jim Fawcett, CSE681 - Software Modeling and Analysis, Fall 2013         //
/////////////////////////////////////////////////////////////////////////////
/*
 *  Package Contents:
 *  -----------------
 *  This package defines the service contract, ICommService, for messaging and 
 *  defines the class ServiceMessage as a data contract.
 *
 *  Required Files:
 *  - None
 *
 *  Required References:
 *  - System.ServiceModel
 *  - System.Runtime.Serialization
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
using System.ServiceModel;
using System.Runtime.Serialization;

namespace DocumentVault
{
  [ServiceContract]
  public interface ICommService
  {
    [OperationContract(IsOneWay = true)]
    void PostMessage(ServiceMessage msg);
  }

  [DataContract]
  public class ServiceMessage
  {
    [DataMember]
    public string TargetUrl { get; set; }
    [DataMember]
    public string SourceUrl { get; set; }
    [DataMember]
    public string TargetCommunicator { get; set; }
    [DataMember]
    public string SourceCommunicator { get; set; }
    [DataMember]
    public string Contents { get; set; }
    [DataMember]
    public string ResourceName { get; set; }

    public void ShowMessage()
    {
      Console.Write(
        "\n  Message Contents:\n  TargetUrl:\t{0}\n  SourceUrl:\t{1}\n  TargetCommunicator:\t{2}\n  SourceCommunicator:\t{3}\n  contents:\t{4}\n  name:\t{5}\n",
        TargetUrl, SourceUrl, TargetCommunicator, SourceCommunicator, Contents, ResourceName
      );
    }
    public static void ShowText(string text)
    {
      Console.Write("\n  {0}", text);
    }

    public static ServiceMessage MakeMessage(string TargetCommunicator, string SourceCommunicator, string contents, string resName = "")
    {
      ServiceMessage msg = new ServiceMessage();
      msg.TargetCommunicator = TargetCommunicator;
      msg.SourceCommunicator = SourceCommunicator;
      msg.ResourceName = resName;
      msg.Contents = contents;
      return msg;
    }
  }
}