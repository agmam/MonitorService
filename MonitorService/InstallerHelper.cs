using MonitorService.Properties;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MonitorService
{
    [RunInstaller(true)]
    public partial class InstallerHelper : System.Configuration.Install.Installer
    {
        //public InstallerHelper()
        //{
        //    InitializeComponent();
        //}

        public override void Install(IDictionary stateSaver)
        {

            base.Install(stateSaver);



            string strKey = Context.Parameters["KeyValue"];

            Settings.Default.ServerName = strKey;
            Settings.Default.Save();
            string sPath = Context.Parameters["assemblypath"];
            string fName = "txt";
            string eName = "exe";
          sPath = sPath.Replace(eName, fName);
            
            //string sPath = @"c:\Test.txt";

            if (File.Exists(sPath))

                File.Delete(sPath);



            File.WriteAllText(sPath, strKey);
        }
    }
}
