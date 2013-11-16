///////////////////////////////////////////////////////////////////////
// ICommLib.cs - interface for internal Document Vault communication //
//                                                                   //
// Jim Fawcett, CSE681 - Software Modeling and Analysis, Fall 2013   //
///////////////////////////////////////////////////////////////////////
/*
 *  Package Contents:
 *  -----------------
 *  This package defines the IComm interface, providing a contract
 *  for sending messages and naming.
 *
 *  Required Files:
 *  - ICommLib:           defines Communicator interface
 *  - ICommServiceModel:  defines ServiceMessage
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

namespace DocumentVault
{
  public interface IComm
  {
    void PostMessage(ServiceMessage msg);
    string Name { get; set; } 
  }
}
