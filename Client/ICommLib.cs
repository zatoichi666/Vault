///////////////////////////////////////////////////////////////////////
// ICommLib.cs - interface for internal Document Vault communication //
//                                                                   //
// Jim Fawcett, CSE681 - Software Modeling and Analysis, Fall 2013   //
///////////////////////////////////////////////////////////////////////
/*
 *  Required Files:
 *  - ICommLib:           defines Communicator interface
 *  - ICommServiceModel:  defines ServiceMessage
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

namespace DocumentVault
{
  public interface IComm
  {
    void PostMessage(ServiceMessage msg);
    string Name { get; set; } 
  }
}
