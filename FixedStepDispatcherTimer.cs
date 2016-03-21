// Market Recorder for Interactive Brokers
// This projected is licensed under the terms of the MIT license.
// Copyright (c) 2013, 2014, 2015, 2016 Ryan S. White

using System;
using System.Windows.Threading;

namespace Capture
{
    /// <summary>
    /// Similar to the timer class except for this timer does not loose time over time.
    /// This timer has almost no drift.
    /// Source: John Leidegren (2010, http://stackoverflow.com/a/3250747/2352507)
    /// </summary>
    public class FixedStepDispatcherTimer  
    {
        /// <summary> 
        /// Occurs when the timer interval has elapsed. 
        /// </summary> 
        public event EventHandler Tick;

        DispatcherTimer timer;

        public bool IsRunning { get { return timer.IsEnabled; } }

        long step; //interval length
        long nextTick;
        long n;

        public TimeSpan Elapsed { get { return new TimeSpan(n * step); } }

        public FixedStepDispatcherTimer(TimeSpan interval)
        {
            if (interval < TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException("interval");
            }
            this.timer = new DispatcherTimer();
            //timer.Dispatcher.Thread.Priority = System.ThreadPriority.;
            this.timer.Tick += new EventHandler(OnTimerTick);
            this.step = interval.Ticks;

        }

        TimeSpan GetTimerInterval()
        {
            var interval = nextTick - DateTime.Now.Ticks;
            if (interval > 0)
            {
                return new TimeSpan(interval);
            }
            return TimeSpan.Zero; // yield 
        }

        void OnTimerTick(object sender, EventArgs e)
        {
            if (DateTime.Now.Ticks >= nextTick)
            {
                n++;
                if (Tick != null)
                {
                    Tick(this, EventArgs.Empty);
                }
                nextTick += step;
            }
            var interval = GetTimerInterval();
            System.Diagnostics.Trace.WriteLine(interval);
            timer.Interval = interval;
        }

        public void Reset()
        {
            n = 0;
            nextTick = 0;
        }

        public void Start()
        {
            var now = DateTime.Now.Ticks;
            nextTick = now + (step - (nextTick % step));
            timer.Interval = GetTimerInterval();
            timer.Start();
        }

        public void Restart(DateTime startAt)
        {
            n = 0;
            nextTick = startAt.Ticks;
            timer.Interval = GetTimerInterval();
            timer.Start();
        }

        public void Stop()
        {
            timer.Stop();
            nextTick = DateTime.Now.Ticks % step;
        }
    }
}
