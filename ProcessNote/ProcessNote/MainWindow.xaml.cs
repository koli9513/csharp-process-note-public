using System;
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
        private Process[] processes;
        private HashSet<ProcessThread> processThreads = new HashSet<ProcessThread>();
        private Process currentProcess;
        private Dictionary<int, List<string>> processComments = new Dictionary<int, List<string>>();
        private bool isCommentBoxOpen = false;
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
            if (isCommentBoxOpen)
            {
                string comment = InputTextBox.Text;
                if (comment != "")
                {
                    string messageBoxText = "Would you like to save your comment first?";
                    MessageBoxResult result = MessageBox.Show(messageBoxText, "", MessageBoxButton.OKCancel);
                    if (result == MessageBoxResult.OK)
                    {
                        SaveComment(comment);
                    }
                }

                CommentBox.Visibility = Visibility.Collapsed;
                InputTextBox.Text = string.Empty;

                isCommentBoxOpen = false;
            }
            ProcessList selectedProcess = (ProcessList)ProcessInfo.SelectedItem;

            currentProcess = processes.Where(process => process.Id.Equals(selectedProcess.id)).First();

            processThreads = new HashSet<ProcessThread>();
            CollectThreads();
        }

        private void SaveComment(string comment)
        {
            if (processComments.ContainsKey(currentProcess.Id))
            {
                processComments[currentProcess.Id].Add(comment);
            }
            else
            {
                processComments[currentProcess.Id] = new List<string>() { comment };
            }
        }

        private void CollectThreads()
        {
            foreach(ProcessThread processThread in currentProcess.Threads)
            {
                processThreads.Add(processThread);
            }
        }

        private void AddComment_Click(object sender, RoutedEventArgs e)
        {
            if (currentProcess == null)
            {
                MessageBox.Show("Please select a process");
            }
            else
            {
                CommentBox.Visibility = Visibility.Visible;
                isCommentBoxOpen = true;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            CommentBox.Visibility = Visibility.Collapsed;
            string comment = InputTextBox.Text;
            InputTextBox.Text = string.Empty;

            SaveComment(comment);

            isCommentBoxOpen = false;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            CommentBox.Visibility = Visibility.Collapsed;
            InputTextBox.Text = string.Empty;
            isCommentBoxOpen = false;
        }
    }

    internal class ProcessList
    {
        public int id { get; set; }
        public string name { get; set; }

    }
   
}
