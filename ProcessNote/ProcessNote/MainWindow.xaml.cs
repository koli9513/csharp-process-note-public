using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace ProcessNote
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Process[] processes;
        public MainWindow()
        {
            InitializeComponent();
            processes = Process.GetProcesses();
            List<ProcessList> processlist = new List<ProcessList>();
            foreach (Process item in processes)
            {
                processlist.Add(new ProcessList() { id = item.Id, name = item.ProcessName });
            }
            ProcessInfo.ItemsSource = processlist;
        }

        private void ShowThreads_Click(object sender, RoutedEventArgs e)
        {
            ProcessThreadCollection currentThreads = Process.GetCurrentProcess().Threads;

            List<ThreadList> threadList = new List<ThreadList>();
            foreach (ProcessThread thread in currentThreads)
            {
                threadList.Add(new ThreadList() { id = thread.Id, startTime = thread.StartTime });
            }
            var message = string.Join(Environment.NewLine, threadList);
            MessageBox.Show(message);
        }

        private void ProcessInfo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ProcessList SelectedItem = (ProcessList)ProcessInfo.SelectedItem;

            if (SelectedItem != null)
            {

            }
        }
    }

    internal class ThreadList
    {
        public int id { get; set; }
        public DateTime startTime { get; set; }

        public override string ToString()
        {
            return "ID: " + id + " Start time: " + startTime;
        }
    }

    internal class ProcessList
    {
        public int id { get; set; }
        public string name { get; set; }

    }
   
}
