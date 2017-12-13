using Microsoft.Win32;
using MonitorService.Properties;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Diagnostics;
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
        public override void Commit(IDictionary savedState)
        {
            base.Commit(savedState);
            string sPath = Context.Parameters["assemblypath"];
            Process.Start(sPath);
            
        }
        public override void Install(IDictionary stateSaver)
        {

            base.Install(stateSaver);
            RegistryKey rk = Registry.CurrentUser.OpenSubKey
           ("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            string sPath = Context.Parameters["assemblypath"];
            rk.SetValue("Monistor Service", sPath);


            string strKey = Context.Parameters["KeyValue"];
        
          
            
            string fName = "txt";
            string eName = "exe";
          sPath = sPath.Replace(eName, fName);
            

            if (File.Exists(sPath))
            { File.Delete(sPath);
            }
               



            File.WriteAllText(sPath, strKey);
        }
    }
}
