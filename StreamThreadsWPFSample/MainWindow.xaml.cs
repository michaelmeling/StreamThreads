﻿using StreamThreads;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;
using static StreamThreads.StreamExtensions;

namespace StreamThreadsWPFSample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        internal int threads;
        private Example mycontrollerexample;
        private DispatcherTimer timer = new DispatcherTimer(DispatcherPriority.Render);

        public MainWindow()
        {
            InitializeComponent();

            mycontrollerexample = new Example(this);
            mycontrollerexample.State = mycontrollerexample.StartState().Await();

            timer.Interval = TimeSpan.FromMilliseconds(30);
            timer.Tick += timer_Tick;
            timer.Start();
        }

        private void timer_Tick(object? sender, EventArgs e)
        {
            if (mycontrollerexample?.State == null) return;

            using (var d = Dispatcher.DisableProcessing())
            {
                /* your work... Use dispacher.begininvoke... */

                Stopwatch sw = Stopwatch.StartNew();
                if (mycontrollerexample!.State!.Loop())
                    timer.Stop();

                sw.Stop();
                txtfps.Text = (10000000.0 / sw.ElapsedTicks).ToString("N2");
                txtthreads.Text = threads.ToString("N0");
            }

            SecondsSinceLast = 0;
        }
    }
}
