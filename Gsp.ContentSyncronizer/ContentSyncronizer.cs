using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace Gsp.ContentSyncronizer
{
    
    public partial class ContentSyncronizer : ServiceBase
    {
        private bool KeepRunning = true;

        public ContentSyncronizer()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            StartService();
        }

        protected override void OnStop()
        {
            StopService();
        }


        public void StartService()
        {
            LoadSettings();

            var success = LoadPlugin();

            if (success) return;

            StartWatchingProcess();

            Console.CancelKeyPress += delegate (object sender, ConsoleCancelEventArgs e) {
                e.Cancel = true;
                KeepRunning = false;
            };

            while (KeepRunning) { }
            Console.WriteLine("exited gracefully");
        }
        public void StartWatchingProcess()
        {
            Tools.Watch("Local.txt", (oldContent,newContent) => {

                Tools.ShowMessageBox($"Old: {oldContent} \n\nNew: {newContent}");
            });
            Console.ReadLine();
        }
        public void StopService()
        {

        }
        public bool LoadPlugin()
        {
            return false;
        }
        public void LoadSettings()
        {
            if (!File.Exists("settings.xml")) return;

            var reader = new XmlTextReader("settings.xml");

            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element: // The node is an element.
                        Console.Write("<" + reader.Name);

                        while (reader.MoveToNextAttribute()) // Read the attributes.
                            Console.Write(" " + reader.Name + "='" + reader.Value + "'");
                        Console.WriteLine(">");
                        break;
                    case XmlNodeType.Text: //Display the text in each element.
                        Console.WriteLine(reader.Value);
                        break;
                    case XmlNodeType.EndElement: //Display the end of the element.
                        Console.Write("</" + reader.Name);
                        Console.WriteLine(">");
                        break;
                }
            }
        }



        private void button1_Click(object sender, EventArgs e)
        {
            Tools.ShowMessageBox("TEST");
        }
    }
}
