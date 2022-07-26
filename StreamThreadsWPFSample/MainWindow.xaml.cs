using StreamThreads;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace StreamThreadsWPFSample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Example mycontrollerexample;
        private DispatcherTimer timer = new DispatcherTimer();

        public MainWindow()
        {
            InitializeComponent();

            mycontrollerexample = new Example(this);
            mycontrollerexample.State = mycontrollerexample.StartState().Await();

            timer.Interval = TimeSpan.FromMilliseconds(50);
            timer.Tick += timer_Tick;
            timer.Start();
        }

        private void timer_Tick(object? sender, EventArgs e)
        {
            if (mycontrollerexample?.State == null) return;

            if (mycontrollerexample!.State!.Loop())
                timer.Stop();
        }
    }
}
