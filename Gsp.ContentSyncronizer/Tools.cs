namespace Gsp.ContentSyncronizer
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;
    using System.Windows.Forms;
    using menelabs.core;

    public class Tools
    {

        private static FileSystemSafeWatcher Watcher;

        private static Action<string, string> onChanged;
        private static DateTime LastRead = DateTime.MinValue;
        private static List<string> _changedFiles = new List<string>();

        private static string OldContent { get; set; }
        private static string NewContent { get; set; }

        public static bool Watch(string file, Action<string, string> newChanges)
        {
            onChanged = newChanges;

            ReadText(file, (txt) => {
                OldContent = txt;
            });

            if (Watcher == null) Watcher = new FileSystemSafeWatcher();

            var path = Path.GetFullPath(file);
            var fileName = Path.GetFileName(file);
            var dir = Path.GetDirectoryName(path);
            Watcher.Path = dir + "\\";
            Watcher.Filter = fileName;

            Watcher.NotifyFilter = NotifyFilters.LastAccess |
                             NotifyFilters.LastWrite |
                             NotifyFilters.FileName |
                             NotifyFilters.DirectoryName;
            Watcher.IncludeSubdirectories = false;

            Watcher.Changed += new FileSystemEventHandler(OnChanged);
            Watcher.Created += new FileSystemEventHandler(OnCreated);
            Watcher.Deleted += new FileSystemEventHandler(OnDeleted);
            Watcher.Renamed += new RenamedEventHandler(OnRenamed);
            Watcher.EnableRaisingEvents = true;
            return true;
        }

        private static void OnDeleted(object sender, FileSystemEventArgs e)
        {
            throw new NotImplementedException();
        }

        private static void OnCreated(object sender, FileSystemEventArgs e)
        {
            throw new NotImplementedException();
        }

        private static void OnRenamed(object sender, RenamedEventArgs e)
        {
            ShowMessageBox($"Renamed: from {e.OldName} to {e.Name}");
        }

        private static void OnChanged(object sender, FileSystemEventArgs e)
        {
            ReadText(e.FullPath, (str) =>
            {
                NewContent = str;

                try
                {
                  
                    if (onChanged != null)
                    {
                        onChanged(OldContent, NewContent);

                        OldContent = NewContent;
                    }

                }
                catch (Exception ex)
                {
                    ShowMessageBox("Error: " + ex.Message);
                }

            });
            //var lastWriteTime = File.GetLastWriteTime(e.FullPath);

            //if (lastWriteTime == LastRead)
            //{
            //    LastRead = DateTime.MinValue;
            //    return;
            //}

            //LastRead = lastWriteTime;


            //lock (_changedFiles)
            //{
            //    if (_changedFiles.Contains(e.FullPath))
            //    {
            //        return;
            //    }
            //    _changedFiles.Add(e.FullPath);
            //}


            //try
            //{
            //    Watcher.EnableRaisingEvents = false;
            //    ReadText(e.FullPath, (str) =>
            //    {
            //        NewContent = str;

            //        try
            //        {
            //            lock (_changedFiles)
            //            {
            //                _changedFiles.Remove(e.FullPath);
            //            }

            //            if (onChanged != null)
            //            {
            //                onChanged(OldContent, NewContent);

            //                OldContent = NewContent;
            //            }

            //        }
            //        catch (Exception ex)
            //        {
            //            ShowMessageBox("Error: " + ex.Message);
            //        }

            //    });
            //}
            //finally
            //{
            //    Watcher.EnableRaisingEvents = true;
            //}


        }


        public static void ShowMessageBox(string message)
        {
            MessageBox.Show(message);
        }

        public static string Stream2String(Stream stream)
        {
            // convert stream to string
            StreamReader reader = new StreamReader(stream);
            string str = reader.ReadToEnd();

            return str;
        }

        public static Stream String2Stream(string str)
        {
            // convert string to stream
            byte[] byteArray = System.Text.Encoding.ASCII.GetBytes(str);
            MemoryStream stream = new MemoryStream(byteArray);

            return stream;
        }

        /// <summary>
        /// Try to do an action on a file until a specific amount of time
        /// </summary>
        /// <param name="path">Path of the file</param>
        /// <param name="action">Action to execute on file</param>
        /// <param name="milliSecondMax">Maimum amount of time to try to do the action</param>
        /// <returns>true if action occur and false otherwise</returns>
        public static bool TryTo(string path, Action<FileStream> action, int milliSecondMax = Timeout.Infinite)
        {
            bool result = false;
            DateTime dateTimestart = DateTime.Now;
            Tuple<AutoResetEvent, FileSystemWatcher> tuple = null;

            while (true)
            {
                try
                {
                    using (var file = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        action(file);
                        result = true;
                        break;
                    }
                }
                catch (IOException)
                {
                    // Init only once and only if needed. Prevent against many instantiation in case of multhreaded 
                    // file access concurrency (if file is frequently accessed by someone else). Better memory usage.
                    if (tuple == null)
                    {
                        var autoResetEvent = new AutoResetEvent(true);
                        var fileSystemWatcher = new FileSystemWatcher(Path.GetDirectoryName(path))
                        {
                            EnableRaisingEvents = true
                        };

                        fileSystemWatcher.Changed +=
                            (o, e) =>
                            {
                                if (Path.GetFullPath(e.FullPath) == Path.GetFullPath(path))
                                {
                                    autoResetEvent.Set();
                                }
                            };

                        tuple = new Tuple<AutoResetEvent, FileSystemWatcher>(autoResetEvent, fileSystemWatcher);
                    }

                    int milliSecond = Timeout.Infinite;
                    if (milliSecondMax != Timeout.Infinite)
                    {
                        milliSecond = (int)(DateTime.Now - dateTimestart).TotalMilliseconds;
                        if (milliSecond >= milliSecondMax)
                        {
                            result = false;
                            break;
                        }
                    }

                    tuple.Item1.WaitOne(milliSecond);
                }
            }

            if (tuple != null && tuple.Item1 != null) // Dispose of resources now (don't wait the GC).
            {
                tuple.Item1.Dispose();
                tuple.Item2.Dispose();
            }

            return result;
        }
        public static bool ReadText(string path, Action<string> action, int milliSecondMax = Timeout.Infinite)
        {
            bool result = false;
            DateTime dateTimestart = DateTime.Now;
            Tuple<AutoResetEvent, FileSystemWatcher> tuple = null;

            while (true)
            {
                try
                {
                    action(File.ReadAllText(path));
                    result = true;
                    break;
                }
                catch (IOException)
                {
                    // Init only once and only if needed. Prevent against many instantiation in case of multhreaded 
                    // file access concurrency (if file is frequently accessed by someone else). Better memory usage.
                    if (tuple == null)
                    {
                        var autoResetEvent = new AutoResetEvent(true);
                        var fileSystemWatcher = new FileSystemWatcher(Path.GetDirectoryName(path))
                        {
                            EnableRaisingEvents = true
                        };

                        fileSystemWatcher.Changed +=
                            (o, e) =>
                            {
                                if (Path.GetFullPath(e.FullPath) == Path.GetFullPath(path))
                                {
                                    autoResetEvent.Set();
                                }
                            };

                        tuple = new Tuple<AutoResetEvent, FileSystemWatcher>(autoResetEvent, fileSystemWatcher);
                    }

                    int milliSecond = Timeout.Infinite;
                    if (milliSecondMax != Timeout.Infinite)
                    {
                        milliSecond = (int)(DateTime.Now - dateTimestart).TotalMilliseconds;
                        if (milliSecond >= milliSecondMax)
                        {
                            result = false;
                            break;
                        }
                    }

                    tuple.Item1.WaitOne(milliSecond);
                }
            }

            if (tuple != null && tuple.Item1 != null) // Dispose of resources now (don't wait the GC).
            {
                tuple.Item1.Dispose();
                tuple.Item2.Dispose();
            }

            return result;
        }

    }
}
