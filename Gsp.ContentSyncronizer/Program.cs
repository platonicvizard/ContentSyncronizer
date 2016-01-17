using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace Gsp.ContentSyncronizer
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {

#if (!DEBUG)
           ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new ContentSyncronizer()
            };
            ServiceBase.Run(ServicesToRun);
#else
            ContentSyncronizer myServ = new ContentSyncronizer();
            myServ.ShowMessageBox("Testing 123");
            // here Process is my Service function
            // that will run when my service onstart is call
            // you need to call your own method or function name here instead of Process();
#endif
        }
    }
}
