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
  