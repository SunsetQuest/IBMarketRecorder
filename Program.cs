// Market Recorder for Interactive Brokers
// This projected is licensed under the terms of the MIT license.
// NO WARRANTY. THE SOFTWARE IS PROVIDED TO YOU “AS IS” AND “WITH ALL FAULTS.”
// ANY USE OF THE SOFTWARE IS ENTIRELY AT YOUR OWN RISK.
// Copyright (c) 2011 - 2016 Ryan S. White

using System;
using System.Linq;
using System.Windows.Forms;
using System.Diagnostics;

namespace Capture
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                string item = args[i].Trim(new char[] { '/', '-', ' ' }).ToLower();
                if (item == "q" || item == "quit")
                {
                    var currentProcess = Process.GetCurrentProcess();
                    var matchingProcesses = Process.GetProcesses().Where(x => x.Id != currentProcess.Id && x.ProcessName == currentProcess.ProcessName);
                    foreach (var process in matchingProcesses)
                        process.CloseMainWindow();
                    return; // exit this application
                }
                if (item == "h" || item == "help")
                {
                    MessageBox.Show("/q    closes any running instances\n/h    shows this window", "Command Line Parameters");
                    return; // exit this application
                }
            }
            
            if (PriorProcess() != null)
            {
                MessageBox.Show("Another instance of the recorder is already running.");
                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new RecorderView());
        }

        /// <summary>
        /// Returns a System.Diagnostics.Process pointing to a pre-existing process with the same name as the current one, if any; or null if the current process is unique.
        /// Source: http://www.covingtoninnovations.com/mc/SingleInstance.html (2004)
        /// </summary>
        public static Process PriorProcess()
        {
            Process curr = Process.GetCurrentProcess();
            Process[] procs = Process.GetProcessesByName(curr.ProcessName);
            foreach (Process p in procs)
            {
                if ((p.Id != curr.Id) &&
                    (p.MainModule.FileName == curr.MainModule.FileName))
                    return p;
            }
            return null;
        }
    }
}
