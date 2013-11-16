using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace WPF_DispatcherDemo
{

    class VaultModel
    {
        // Properties
        public int _indexSelectedCategory { get; set; }
        public int _indexSelectedContentFile { get; set; }
        public String _ContentFile { get; set; }
        public String _MetadataFile { get; set; }

        // Private Member Data
        private List<String> _vaultFiles = new List<String>();
        private List<String> _vaultCategories = new List<String>();
        private List<String> _FileParents = new List<String>();
        private List<String> _FileChildren = new List<String>();

        public void UpdateFileIndex(String selectedFile)
        {
            _indexSelectedContentFile = _vaultFiles.IndexOf(selectedFile);
        }

        public void SetCategories(List<String> Category_s)
        {            
            _vaultCategories = Category_s;
        }

        public List<String> GetCategories()
        {
            return _vaultCategories;
        }

        public void SetChildren(List<String> Children_s)
        {
            _FileChildren = Children_s;
        }

        public List<String> GetChildren()
        {
            return _FileChildren;
        }

        public void SetParents(List<String> Parents_s)
        {
            _FileParents = Parents_s;
        }

        public List<String> GetParents()
        {
            return _FileParents;
        }

        public void SetFileList(List<String> FileList)
        {
            _vaultFiles = FileList;
        }

        public List<String> GetVaultFileList()
        {
            return _vaultFiles;
        }
        
        /*
        public void InsertVaultFile(VaultFileEntry vfe)
        {
            _vaultFiles.Add(vfe);
        }
        */

        public XDocument ToXmlMessage()
        {
            String Category = "";
            if (_indexSelectedCategory > -1)
                Category = _vaultCategories[_indexSelectedCategory];

            String File = "";
            if (_indexSelectedContentFile > -1)
                File = _vaultFiles[_indexSelectedContentFile];

            XDocument xd = new XDocument(
                new XDeclaration("1.0", "utf-8", "yes"),
                new XElement("ContentRequest",
                    new XElement("Category", Category),
                    new XElement("File", File)));

            return xd;
        }

        public VaultModel()
        {
            _indexSelectedCategory = -1;
            _indexSelectedContentFile = -1;
            
        }

        static void Main(string[] args)
        {
            VaultModel vm = new VaultModel();

            Console.WriteLine("Hello, made new vaultmodel\n");
        }
    }
}
