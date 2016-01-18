namespace Gsp.ContentSyncronizer
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public interface Plugin
    {
        void Init();//if the user want to run code on init
        void ChangeRemote();//tell me how to change the remote file
        void ChangeLocal(); //tell  me how to change the local file
        Change_From Change { get; set; }

        Action<string> FileInSystemChanged { get; set; }//if the user want to fire his own filechange event
        Action<string> Merge { get; set; } // if the user want to fire his own merge implementation
    }
}
