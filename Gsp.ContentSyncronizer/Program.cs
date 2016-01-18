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
        [STAThread]
        static void Main(string [] args)
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
            myServ.StartService();
#endif
        }
    }
}
