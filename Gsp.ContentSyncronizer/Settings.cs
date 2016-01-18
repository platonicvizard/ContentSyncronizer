using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gsp.ContentSyncronizer
{
    public class Settings
    {
        public bool CheckRemoteFileChange { get; set; }
        public int CheckRemoteFileInterval { get; set; }
        public string RemotePath { get; set; }
        public string LocalPath { get; set; }
    }
}
