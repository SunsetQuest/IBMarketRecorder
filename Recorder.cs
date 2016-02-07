// Market Recorder for Interactive Brokers
// This projected is licensed under the terms of the MIT license.
// Copyright (c) 2013, 2014, 2015, 2016 Ryan S. White

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.IO;  // for BinaryWriter
using System.ServiceModel;
using Krs.Ats.IBNet;
using System.Text;
using System.Windows.Forms; // for WCF
using System.Diagnostics;
using System.Threading;

namespace Capture
{
    public partial class Recorder
    {
        Properties.Settings settings = Properties.Settings.Default;
        BinaryWriter capturingWriter, manufacturedWriterForFuture;
        object capturingWriterLock = new object();
        List<Symbol> symbols;
        TimeSpan START_TIME = new TimeSpan(5, 45, 0);
        TimeSpan STOP_TIME = new TimeSpan(14, 15, 0);
        bool isMarketOpen;
        BriefMakerServiceReference.BriefMakerClient briefMakerClient; // WCF notification to a BriefMaker
        const int MemoryStreamReserveSpace = 1024;
        DataClasses1DataContext dc = new DataClasses1DataContext();
        int totalCaptureEventsForDisplay = 0, lastTotalCaptureEventsForDisplay = 0;
        DateTime lastPushedTime;
        private static NLog.Logger logger, loggerWCF; // = NLog.LogManager.GetCurrentClassLogger();
        private static IBClient client;
        public volatile bool terminateRequested = false;
        RecorderView view;
        DateTime lastRecievedUpdate;// contains when the last marked data update was received
        DateTime lastUpdateTime; //contains the last time the 1secTickTimer ran
        //private System.Windows.Forms.Timer MonitorTimer;
        

        public Recorder(RecorderView view)
        {
            this.view = view;
            view.recorder = this;

            /////////// Setup logger ///////////
            logger = NLog.LogManager.GetCurrentClassLogger();
            loggerWCF = NLog.LogManager.GetLogger("Capture.Recorder.WCF");

            /////////// Setup WCF notification to a BriefMaker ///////////
            briefMakerClient = new BriefMakerServiceReference.BriefMakerClient();

            /////////// Download Symbols ///////////
            logger.Debug("Downloading symbols");
            //var symbols = (from symb in dc.Symbols select symb.Name.Trim()).ToList();
            symbols = (from s in dc.Symbols select s).ToList();

              // Setup BinaryWriters
            logger.Debug("Downloading symbols");
            capturingWriter = new BinaryWriter(new MemoryStream(MemoryStreamReserveSpace));
            capturingWriter.Write((long)0);  //leave some space at the beginning for time later
            manufacturedWriterForFuture = new BinaryWriter(new MemoryStream(MemoryStreamReserveSpace));
            manufacturedWriterForFuture.Write((long)0);  //leave some space at the beginning for time later


            Process.GetCurrentProcess().PriorityClass = System.Diagnostics.ProcessPriorityClass.High;
            //Thread.CurrentThread.Priority = ThreadPriority.AboveNormal;

            view.timer.Tick += new EventHandler(timer1Sec_Tick);


            while (true)
            {
                TimeSpan timespan = new TimeSpan(DateTime.Now.Ticks - lastRecievedUpdate.Ticks);
                if (timespan.Seconds > 30)
                {
                    logger.Info("No Data received in over 30 seconds. Requesting Reconnect...");
                    lastRecievedUpdate = DateTime.Now;
                    ConnectToIB();
                }

                if (terminateRequested || (DateTime.Now.TimeOfDay > settings.ShutDownTime))
                {
                    logger.Info("Close requested or automatic end of day shutdown.");
                    ShutdownRecorder();
                    break;
                }

                view.BeginInvoke((MethodInvoker)delegate
                {
                    view.toolStripStatusLabelEventCt.Text = totalCaptureEventsForDisplay.ToString() + " (" + (totalCaptureEventsForDisplay - lastTotalCaptureEventsForDisplay).ToString() + "/sec)";  // runs on UI thread 
                    lastTotalCaptureEventsForDisplay = totalCaptureEventsForDisplay;
                    view.toolStripStatusLabelLastBrfID.Text = lastUpdateTime.ToString();
                });  // runs on UI thread 

                Thread.Sleep(1000);
            }
        }


        private void ConnectToIB() 
        {
            DisposeIBClient();

            logger.Info("Rebuilding IBClient");

            if (client != null)
                client.Dispose();

            client = new IBClient();
            Thread.Sleep(1000);
            logger.Info("IBClient Rebuilt");


            //client.ThrowExceptions = true;
            int retryCt = 5;
            while (!client.Connected)
            {
                if ((retryCt--) <= 0)
                {
                    logger.Info("Tried to reconnect 5 times going to try re-creating the client...");
                    return;
                }
                
                if (terminateRequested) 
                    ShutdownRecorder();

                try
                {
                    //int clientID = (new Random()).Next(0, 2000);
                    logger.Info("Connecting to TWS Interactive brokers on port 7496..."+"(Try " + (4-retryCt) + " of 5)" );
                    client.Connect(settings.IBHostToConnectTo, settings.IBPortToConnectTo, 1);
                    logger.Info("Connection initiated, requesting data");
                    Thread.Sleep(2000);
                    client.RequestIds(1);
                    Thread.Sleep(2000);

                }
                catch (System.Exception ex)
                {
                    logger.Info("IB Connecting Exception: " + ex.Message);
                    if (terminateRequested) // changed: 2004-1-28
                        ShutdownRecorder();
                    Thread.Sleep(3000);
                }
            }
            logger.Info("TWS Client Connected is now true.");

            Thread.Sleep(2000);

            logger.Info("Connected to TWS server version {0} at {1}.", client.ServerVersion, client.TwsConnectionTime); 

            client.TickPrice += client_TickPrice;
            client.TickSize += client_TickSize;
            client.Error += client_Error;
            
            //client.UpdateMarketDepth += new EventHandler<UpdateMarketDepthEventArgs>(client_UpdateMarketDepth);
            //client.UpdateMarketDepthL2 += new EventHandler<UpdateMarketDepthL2EventArgs>(client_UpdateMarketDepthL2);
            //// client.TickString += new EventHandler<TickStringEventArgs>(client_TickString); // nothing of value that I see
            //client.TickGeneric += new EventHandler<TickGenericEventArgs>(client_TickGeneric);
            //client.TickEfp += new EventHandler<TickEfpEventArgs>(client_TickEfp);
            //client.FundamentalData += new EventHandler<FundamentalDetailsEventArgs>(client_FundamentalData);
            //client.ContractDetails += new EventHandler<ContractDetailsEventArgs>(client_ContractDetails);

            /////////// Register the list of symbols for TWS ///////////
            string listToEcho = "";
            for (int s = 0; s < symbols.Count(); s++)
            {
                //Warning: axTws1.reqMktData((symbolCt),... <--may need to be 0,1,2,3 and not symbolID
                //axTws1.reqMktData(curID++, symbols[s], "STK", "", 0, "", "", "SMART", "ISLAND", "USD", "", 0);
                string name = symbols[s].Name.Trim();
                SecurityType secType = symbols[s].Type.Trim() == "STK" ? SecurityType.Stock : SecurityType.Index;
                string market = symbols[s].Market.Trim(); //(secType==SecurityType.Stock) ? "smart" : symbols[s].Market.Trim();
                Contract item = new Contract(name, market, secType, "USD");
                client.RequestMarketData(s, item, null, false, false);

                //client.RequestMarketDepth(s, item, 10);
                //client.RequestContractDetails(s, item);
                //client.RequestFundamentalData(s, item, "Estimates");
                //client.RequestFundamentalData(s, item, "Financial Statements");
                //client.RequestFundamentalData(s, item, "Summary");
                //client.RequestNewsBulletins(false);

                listToEcho += ", " + name;
            }
            logger.Debug("Symbols: {0}", listToEcho); 
            //(NowString() + ": Symbols: " + listToEcho).Log();
        }



        //////////////////////////////////////////////////////////////////////
        ////////////////////  TWS message event handlers /////////////////////
        //////////////////////////////////////////////////////////////////////

        void client_TickPrice(object sender, TickPriceEventArgs e)
        {
            //Console.WriteLine("Price: " + e.Price + " Tick Type: " + EnumDescConverter.GetEnumDescription(e.TickType));
            DateTime now = System.DateTime.Now;
            int millisecond = (int)(now.Ticks % (TimeSpan.TicksPerSecond) / (TimeSpan.TicksPerSecond / 256));

            lock (capturingWriterLock)
            {
                capturingWriter.Write((byte)millisecond);
                capturingWriter.Write((byte)e.TickType);
                capturingWriter.Write((byte)e.TickerId);
                capturingWriter.Write((float)e.Price);
            }

            logger.Trace("{0} : {1} : {2} : {3}", e.TickType, symbols[e.TickerId].Name, e.TickerId, e.Price);
            totalCaptureEventsForDisplay++;
            lastRecievedUpdate = now;
        }

        void client_TickSize(object sender, TickSizeEventArgs e)
        {
            //Console.WriteLine("Tick Size: " + e.Size + " Tick Type: " + EnumDescConverter.GetEnumDescription(e.TickType));
            int millisecond = (int)(System.DateTime.Now.Ticks % (TimeSpan.TicksPerSecond) / (TimeSpan.TicksPerSecond / 256));

            lock (capturingWriterLock)
            {
                capturingWriter.Write((byte)millisecond);
                capturingWriter.Write((byte)e.TickType);
                capturingWriter.Write((byte)e.TickerId);
                capturingWriter.Write((float)e.Size);
            }
            if ((totalCaptureEventsForDisplay & 0x3C) == 0) //only shows the first 4 for every 64
                logger.Trace("{0} : {1} : {2} : {3}", e.TickType, symbols[e.TickerId].Name, e.TickerId, e.Size);
            totalCaptureEventsForDisplay++;
        }

        void client_Error(object sender, Krs.Ats.IBNet.ErrorEventArgs e)
        {
            logger.Error(" TWS Message TickerId={0} ErrorCode={1} MSG:{2}", e.TickerId, e.ErrorCode, e.ErrorMsg);
            //if (e.ErrorCode.ToString() == "1100" || e.ErrorCode.ToString() == "1300") // see http://www.interactivebrokers.com/php/apiguide/interoperability/socket_client_c++/errors.htm
            //{
            //    logger.Error(" Requesting reconnect...");
            //    reconnectRequested = true;
            //}
        }

        //void client_ContractDetails(object sender, ContractDetailsEventArgs e)
        //{
        //    logger.Debug(" TWS Message client_ContractDetails RequestId={0} MinTick={1} UnderConId={2} Ratings={3}", e.RequestId, e.ContractDetails.MinTick, e.ContractDetails.UnderConId, e.ContractDetails.Ratings);
        //    //throw new NotImplementedException();
        //}

        //void client_FundamentalData(object sender, FundamentalDetailsEventArgs e)
        //{
        //    logger.Debug(" TWS Message client_FundamentalData RequestId={0} Data={1}", e.RequestId, e.Data);
        //    //throw new NotImplementedException();
        //}

        //void client_TickEfp(object sender, TickEfpEventArgs e)
        //{
        //    logger.Debug(" TWS Message client_TickEfp TickerId={0} BasisPoints={1} DividendImpact={2} ImpliedFuture={3} DividendsToExpiry={4}", e.TickerId, e.BasisPoints, e.DividendImpact, e.ImpliedFuture, e.DividendsToExpiry);
        //    //throw new NotImplementedException();
        //}

        //void client_TickGeneric(object sender, TickGenericEventArgs e)
        //{
        //    logger.Debug(" TWS Message client_TickGeneric TickerId={0} TickType={1} Value={2}", e.TickerId, e.TickType, e.Value);
        //    //throw new NotImplementedException();
        //}

        ////void client_TickString(object sender, TickStringEventArgs e)
        ////{
        ////    logger.Debug(" TWS Message client_TickString TickerId={0} TickType={1} Value={2}", e.TickerId, e.TickType, e.Value);
        ////    //throw new NotImplementedException();
        ////}

        //void client_UpdateMarketDepthL2(object sender, UpdateMarketDepthL2EventArgs e)
        //{
        //    logger.Debug(" TWS Message client_UpdateMarketDepthL2 TickerId={0} Size={1} Price={2} Position={3}",
        //       e.TickerId, e.Size, e.Price, e.MarketMaker);
        //    //throw new NotImplementedException();
        //}

        //void client_UpdateMarketDepth(object sender, UpdateMarketDepthEventArgs e)
        //{
        //    //throw new NotImplementedException();
        //    logger.Debug(" TWS Message client_UpdateMarketDepth TickerId={0} Size={1} Price={2} Position={3}", 
        //       e.TickerId, e.Size, e.Price, e.Position);

        //}


        //////////////////////////////////////////////////////////////////////
        ////////////////////  One Second Snapshot stuff //////////////////////
        //////////////////////////////////////////////////////////////////////

        void timer1Sec_Tick(object sender, EventArgs e)
        {
            DateTime time = DateTime.Now; //issue here - thread does not start until last one finishes
            logger.Debug("Entering timer1Sec_Tick() time={0}", time.ToString("HH:mm:ss.fff"));
            Update_isMarketClosed(ref time);

            // Round down to nearest second
            DateTime roundedTime = new DateTime((time.Ticks / TimeSpan.TicksPerSecond) * TimeSpan.TicksPerSecond);  

            // Make sure we don't do the same upload twice
            if (roundedTime == lastPushedTime)
            {
                logger.Warn("timer1Sec_Tick() detected duplicate roundedTime for {0}, aborting save. "
                    + "This can happen if a previous time1Sec_Tick() took a long time and then several "
                    + "waiting time1Sec_Tick() execute at once.", roundedTime.ToString("HH:mm:ss.fff"));
                return;
            }
            lastPushedTime = roundedTime;

            // Set capturing stream to processing stream 
            BinaryWriter ProcWriter = capturingWriter;
            lock (capturingWriterLock) { capturingWriter = manufacturedWriterForFuture; }

            // Finish up processing stream, 
            ProcWriter.BaseStream.Position = 0;
            ProcWriter.Write(time.Ticks);
            ProcWriter.Flush();  //Flushes pending updates (maybe not needed?)

            // Notify Clients of new TstsRow 
            OpenAndSendWithWCF(ProcWriter);

            // Upload Brfs to the database
            if (isMarketOpen) 
            {
                 
                byte[] data = ((MemoryStream)ProcWriter.BaseStream).ToArray();

                // Round down to nearest second  
                roundedTime = new DateTime((time.Ticks / TimeSpan.TicksPerSecond) * TimeSpan.TicksPerSecond);

                // Save it to the database 
                submitBrfToDB(roundedTime, data);

                if (logger.IsTraceEnabled)
                    LogCapturedData(data);

                // Display pushed results
                logger.Debug("Snapshot complete with {0} bytes for time {1}", data.Length.ToString(), time.ToString("HH:mm:ss.fff"));

                // fixed memory leak issue I think
                ProcWriter.BaseStream.Dispose();
                ProcWriter.Dispose();
            }

            // Setup BinaryWriter for future use - we could of just created this above but it would have taken time before the update
            manufacturedWriterForFuture = new BinaryWriter(new MemoryStream(MemoryStreamReserveSpace));
            manufacturedWriterForFuture.Write((long)0);  //leave some space at the beginning for time later
            lastUpdateTime = time;
        }
        
        void LogCapturedData(byte[] buffer)
        {
            BinaryReader br = new BinaryReader(new MemoryStream(buffer));
            StringBuilder toShow = new StringBuilder();
            toShow.Append("Captured Data: [Time/Type/ID/Val] Time:" + br.ReadInt64() + "; ");
            int len = Math.Min(buffer.Length, 7 * 128);

            while (br.BaseStream.Position < len)
            {
                toShow.Append(br.ReadByte().ToString());                // time
                toShow.Append("/" + br.ReadByte().ToString());          // tick-type
                toShow.Append("/" + br.ReadByte().ToString());          // id
                toShow.Append("/" + br.ReadSingle().ToString() + "; "); // value
            }

            logger.Trace("Captured data: {0}", toShow.ToString());
        }

        void submitBrfToDB(DateTime roundedTime, byte[] data)
        {
            StreamMoment briefToSave = new StreamMoment() { SnapshotTime = roundedTime, Data = data };
            bool submitSuccess = false;
            try
            {
                dc.StreamMoments.InsertOnSubmit(briefToSave);
                dc.SubmitChanges();
                submitSuccess = true;
            }
            catch (System.Exception ex)
            {
                logger.Error("Exception with SubmitChanges() (will try again): {0}", ex.Message);
            }

            if (!submitSuccess)
            {
                logger.Error("Trying SubmitChanges() a second time.");

                try
                {
                    dc.Connection.Close();
                    dc = new DataClasses1DataContext();
                    dc.StreamMoments.InsertOnSubmit(briefToSave);
                    dc.SubmitChanges();
                    logger.Info("SubmitChanges() okay on second try.");
                }
                catch (System.Exception ex)
                {
                    logger.Error("Exception with SubmitChanges() on second try: {0}", ex.Message);
                }
            }
        }

        void OpenAndSendWithWCF(BinaryWriter ProcWriter)
        {
            if (briefMakerClient.State != CommunicationState.Opened)
            {
                loggerWCF.Debug("WCF Not opened. Trying to open a connection...");
                try
                {
                    briefMakerClient.ChannelFactory.BeginOpen(
                        (result) =>
                        {
                            loggerWCF.Debug("WCF received async result");
                            briefMakerClient.ChannelFactory.EndOpen(result);
                        }, null);

                    loggerWCF.Debug("BriefMaker Connection Established");
                }
                catch (System.Exception ex)
                {
                    loggerWCF.Debug("Exception when opening connection:(this can be normal)(Calling Abort,Close,recreate next) {0}", ex.Message);
                    //briefMakerClient.Abort();
                    //briefMakerClient.Close();
                    briefMakerClient = new BriefMakerServiceReference.BriefMakerClient();
                    loggerWCF.Debug("Abort,Close,recreate  completed");
                }
            }

            if (briefMakerClient.State == CommunicationState.Opened)
            {
                loggerWCF.Debug("WCF is open, calling AddDataStreamMomentUsingWCFAsync()");
                try
                {
                    briefMakerClient.AddDataStreamMomentUsingWCFAsync(((MemoryStream)ProcWriter.BaseStream).ToArray());
                    loggerWCF.Debug("AddDataStreamMomentUsingWCFAsync() completed");
                }
                catch (System.Exception ex)
                {
                    loggerWCF.Debug("Exception when calling AddDataStreamMomentUsingWCFAsync(): " + ex.Message);
                }
            }
        }

        void Update_isMarketClosed(ref DateTime time)
        {
            bool isClosedHours = (time.TimeOfDay < START_TIME) || (time.TimeOfDay >= STOP_TIME);
            bool isWeekend = (time.DayOfWeek == DayOfWeek.Saturday || time.DayOfWeek == DayOfWeek.Sunday);
            bool isMarketOpen_New = !(isClosedHours || isWeekend);
            if (!isMarketOpen_New && isMarketOpen )
            {
                view.BeginInvoke((MethodInvoker)delegate { view.toolStripStatusLabelMarketOpen.Text = "Closed"; });  // runs on UI thread 
                logger.Info("The market is now closed.  Hours: " + START_TIME.ToString() + "-" + STOP_TIME.ToString() + "  Time: " + time.ToShortTimeString());
            }
            if (isMarketOpen_New && !isMarketOpen)
            {
                view.BeginInvoke((MethodInvoker)delegate { view.toolStripStatusLabelMarketOpen.Text = "Open"; });  // runs on UI thread 
                logger.Info("The market is now open.  Hours: " + START_TIME.ToString() + "-" + STOP_TIME.ToString() + "  Time: " + time.ToShortTimeString());
            }
            isMarketOpen = isMarketOpen_New;
        }

        ///// <summary>
        ///// Can be used to make sure the range of values that are received are correct.
        ///// </summary>
        //string CheckRange(float value, float low, float high, int symbolID, string attribDesc)
        //{
        //    if ((value < low) || (value > high))
        //        return "Suspect - value range for Symbol/Attribute:" + symbols[symbolID] + "(" + symbolID + ")-" + attribDesc + " has value:" + value + " but expected:" + low + "-" + high + "\r\n";
        //    else
        //        return "";
        //}



        //////////////////////////////////////////////////////////////////////
        ///////////////////////  Close/Shutdown stuff ////////////////////////
        //////////////////////////////////////////////////////////////////////
        void ShutdownRecorder()
        {
            logger.Info("Shutdown initiated for IBClient...");
            DisposeIBClient();
            logger.Info("Shutdown initiated for BrfMakerClient...");
            if ((briefMakerClient.State == CommunicationState.Opened)|| (briefMakerClient.State == CommunicationState.Opening))
                briefMakerClient.Close();
            logger.Info("Shutdown Completed");
            Thread.Sleep(200);
            logger.Info("Exiting Application");
            Application.Exit(); 
        }

        void DisposeIBClient()
        {
            if (client != null)
            {
                logger.Debug("Disconnecting IBClient");
                client.Disconnect();
                Thread.Sleep(250);
                logger.Debug("Stopping IBClient Worker Thread");
                client.Stop();
                Thread.Sleep(250);
                logger.Debug("Disposing IB Client");
                client.Dispose();
                Thread.Sleep(250);
                client = null;
                Thread.Sleep(250);
            }
        }
    }
}






