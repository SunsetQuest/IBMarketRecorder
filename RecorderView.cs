// Market Recorder for Interactive Brokers
// This projected is licensed under the terms of the MIT license.
// This projected is licensed under the terms of the MIT license.
// NO WARRANTY. THE SOFTWARE IS PROVIDED TO YOU “AS IS” AND “WITH ALL FAULTS.”
// ANY USE OF THE SOFTWARE IS ENTIRELY AT YOUR OWN RISK.
// Copyright (c) 2011 - 2016 Ryan S. White

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices; // for WCF

namespace Capture
{
    public partial class RecorderView : Form
    {
        private static NLog.Logger logger;
        public Recorder recorder;
        public FixedStepDispatcherTimer timer;  // a reoccurring timer that does not lose time
        bool recorderExited = true;
        BackgroundWorker bw;
        private uint m_previousExecutionState; // this is to restore the sleep commands after exiting the program.


        public RecorderView()
        {
            InitializeComponent();
        }

        private void Capture_Load(object sender, EventArgs e)
        {
            /////////// Setup logger ///////////
            logger = NLog.LogManager.GetCurrentClassLogger();
            logger.Info("Starting application.");

            //target.Name = "display";
            //target.Layout = "${time} | ${Threadid} | ${level} | ${logger} | ${message}";
            //target.ControlName = "richTextBox1";
            //target.FormName = "Recorder";
            //target.UseDefaultRowColoringRules = true;
            //NLog.Config.SimpleConfigurator.ConfigureForTargetLogging(target, NLog.LogLevel.Trace);
            //target.MaxLines = 1024;
            //target.AutoScroll = true;


            ///////////// Set new state to prevent the system from entering sleep mode /////////////
            // Source: David Anson @ Microsoft (2009) http://dlaa.me/blog/post/9901642
            m_previousExecutionState = NativeMethods.SetThreadExecutionState(NativeMethods.ES_CONTINUOUS | NativeMethods.ES_SYSTEM_REQUIRED);
            if (0 == m_previousExecutionState)
            {
                MessageBox.Show("Failed to set system state for sleep mode.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                // No way to recover; fail gracefully
                Close();
            }


            /////////// Set timer to first snapshot /////////// 
            //(Note: this must by setup on the UI thread)
            try
            {
                logger.Info("Configuring Timer");
                timer = new FixedStepDispatcherTimer(new TimeSpan(0, 0, 1));
                DateTime now = DateTime.Now;
                DateTime firstTick = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second + 1);
                timer.Restart(firstTick);
            }
            catch (Exception ex)
            {
                logger.Error("Error creating DispatcherTimer: " + ex.Message);
            }

            logger.Info("Configuring Recorder BackgroundWorker");
            bw = new BackgroundWorker();
            bw.DoWork += new DoWorkEventHandler(backgroundWorker1_DoWork);
            bw.RunWorkerAsync();
        }

        private void btnDeleteAll_Click(object sender, EventArgs e)
        {
            DataClasses1DataContext dc = new DataClasses1DataContext();
            if (DialogResult.Yes == MessageBox.Show("Delete all StreamMoments?", "Warning", MessageBoxButtons.YesNo))
            {
                logger.Info("Starting User requested StreamMoment wipe.");
                dc.ExecuteCommand("Delete from StreamMoments");
                logger.Info("Completed User requested StreamMoment wipe.");
            }
        }

        private void Recorder_Resize(object sender, EventArgs e)
        {
            if (FormWindowState.Minimized == this.WindowState)
            {
                notifyIcon1.Visible = true;
                notifyIcon1.ShowBalloonTip(500, "Recorder", "The recorder has been minimized to the system tray", ToolTipIcon.Info);
                Hide();
            }
        }

        private void notifyIcon1_Click(object sender, EventArgs e)
        {
            Show();
            notifyIcon1.Visible = false;
            WindowState = FormWindowState.Normal;
        }

        bool exitingProgram = false;
        private void Recorder_FormClosed(object sender, FormClosedEventArgs e)
        {
            //Prevent from exiting multiple times
            if (exitingProgram)
                return;
            exitingProgram = true;

            notifyIcon1.Visible = false;

            //restore sleep system
            logger.Info("Restoring previous system sleep state.");
            if (0 == NativeMethods.SetThreadExecutionState(m_previousExecutionState))
                logger.Info("Returned 0?");// No way to recover; already exiting

            if (timer != null)
                if (timer.IsRunning)
                    timer.Stop();

            recorder.terminateRequested = true;

            int exitCounter = 0;
            while (!recorderExited)
            {
                System.Threading.Thread.Sleep(10);
                if (exitCounter++ > 200) // wait up to 2 seconds
                    break;
            }

            recorder.terminateRequested = true; 
            
            logger.Info("Exiting Recorder");
        }

        private void btnLaunchWebDemo_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://www.interactivebrokers.com/en/index.php?f=16052&ns=T&twsdemo=1");
            // alternative method thats more automated: http://www.interactivebrokers.com/java/classes/edemo.jnlp
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            logger.Info("Lunching Recorder Thread");
            recorderExited = false;
            Recorder rec = new Recorder(this);
            recorderExited = true;
            logger.Info("Recorder Thread Ended");
        }

        private void setLogLevel(NLog.LogLevel logLevel, string regex)
        {
            IList<NLog.Config.LoggingRule> rules = NLog.LogManager.Configuration.LoggingRules;
            //rules[0].Targets[0].
            //NLog.LogManager.ReconfigExistingLoggers();

            //LoggingConfiguration config = NLog.LogManager.Configuration;//new LoggingConfiguration(); 
            //NLog.LogManager.DisableLogging();
            //config.LoggingRules[0].DisableLoggingForLevel(NLog.LogLevel.Debug);
            //config.LoggingRules[1].DisableLoggingForLevel(NLog.LogLevel.Debug);

            //rules[0].EnableLoggingForLevel(NLog.LogLevel.Warn);
            ////rules[1].EnableLoggingForLevel(NLog.LogLevel.Warn);
            //NLog.LogManager.EnableLogging();
            //NLog.LogManager.ReconfigExistingLoggers();
            //NLog.LogManager.Configuration = config; 

            Regex validator = new Regex(regex);
            foreach (var rule in rules.Where(x => validator.IsMatch(x.Targets[0].Name)))
                if (!rule.IsLoggingEnabledForLevel(logLevel))
                    rule.EnableLoggingForLevel(logLevel);
        }

        private void clearLogLevel(NLog.LogLevel logLevel, string regex)
        {
            IList<NLog.Config.LoggingRule> rules = NLog.LogManager.Configuration.LoggingRules;
            Regex validator = new Regex(regex);
            foreach (var rule in rules.Where(x => validator.IsMatch(x.Targets[0].Name)))
                if (rule.IsLoggingEnabledForLevel(logLevel))
                    rule.DisableLoggingForLevel(logLevel);
        } 

        private void cboLogLevels_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;
            string applyTo = (string)comboBox.Tag;

            clearLogLevel(NLog.LogLevel.Fatal, applyTo);
            clearLogLevel(NLog.LogLevel.Error, applyTo);
            clearLogLevel(NLog.LogLevel.Warn, applyTo);
            clearLogLevel(NLog.LogLevel.Info, applyTo);
            clearLogLevel(NLog.LogLevel.Debug, applyTo);
            clearLogLevel(NLog.LogLevel.Trace, applyTo); 

            switch ((string)comboBox.SelectedItem)
	        {
                case "Trace": setLogLevel(NLog.LogLevel.Trace, applyTo);    goto case "Debug";
                case "Debug": setLogLevel(NLog.LogLevel.Debug, applyTo);    goto case "Info";
                case "Info": setLogLevel(NLog.LogLevel.Info, applyTo);      goto case "Warn";
                case "Warn": setLogLevel(NLog.LogLevel.Warn, applyTo);      goto case "Error";
                case "Error": setLogLevel(NLog.LogLevel.Error, applyTo);    goto case "Fatal";
                case "Fatal": setLogLevel(NLog.LogLevel.Fatal, applyTo);    break;
                case "Off":   break;
                default: break;
	        }
            NLog.LogManager.ReconfigExistingLoggers();
        }
    }

    // source: (2009) https://dlaa.me/blog/post/9901642
    internal static class NativeMethods  
    {
        // Import SetThreadExecutionState Win32 API and necessary flags
        [DllImport("kernel32.dll")]
        public static extern uint SetThreadExecutionState(uint esFlags);
        public const uint ES_CONTINUOUS = 0x80000000;
        public const uint ES_SYSTEM_REQUIRED = 0x00000001;
    }

}














