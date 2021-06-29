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
            MessageBox.Show("Threads");
        }
    }

    internal class ProcessList
    {
        public int id { get; set; }
        public string name { get; set; }

    }
   
}
