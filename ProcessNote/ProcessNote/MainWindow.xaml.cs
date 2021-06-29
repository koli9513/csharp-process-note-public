using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
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
        HashSet<ProcessThread> processThreads = new HashSet<ProcessThread>();
        Process currentProcess;
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
            string dialogContent;
            try
            {
                var threadList = processThreads.Select(processThread => processThread.Id + " " + processThread.StartTime).ToArray();
                dialogContent = string.Join(Environment.NewLine, threadList);
            }
            catch (Exception)
            {
                dialogContent = "No threads linked to this process";
            }
            MessageBox.Show(dialogContent);
        }

        private void Select_Row(object sender, SelectionChangedEventArgs e)
        {
            ProcessList selectedProcess = (ProcessList)ProcessInfo.SelectedItem;
            List<ProcessAttributes> processAttributesList = new List<ProcessAttributes>();

            currentProcess = processes.Where(process => process.Id.Equals(selectedProcess.id)).First();

            try
            {
                var processAttribute = new ProcessAttributes()
                {
                    memory = currentProcess.PeakWorkingSet64, starttime = currentProcess.StartTime,
                    runtime = currentProcess.TotalProcessorTime
                };

                processAttributesList.Add(processAttribute);

                Attributes.ItemsSource = processAttributesList;
            }
            catch(Exception)
            {
                MessageBox.Show("This process isn't running at this time.");
            }

            processThreads = new HashSet<ProcessThread>();
            collectThreads();
        }

        private void collectThreads()
        {
            foreach(ProcessThread processThread in currentProcess.Threads)
            {
                processThreads.Add(processThread);
            }
        }
    }

    internal class ProcessList
    {
        public int id { get; set; }
        public string name { get; set; }
        

    }

    internal class ProcessAttributes
    {
        
        public long memory { get; set; }
        public DateTime starttime { get; set; }
        public TimeSpan runtime { get; set; }
        
    }
   
}
