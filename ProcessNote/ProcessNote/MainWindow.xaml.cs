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
                processlist.Add(new ProcessList() {id = item.Id, name = item.ProcessName});

            }

            ProcessInfo.ItemsSource = processlist;
        }

        private void ShowThreads_Click(object sender, RoutedEventArgs e)
        {
            string dialogContent;
            try
            {
                var threadList = processThreads
                    .Select(processThread => processThread.Id + " " + processThread.StartTime).ToArray();
                dialogContent = string.Join(Environment.NewLine, threadList);
            }
            catch (Exception)
            {
                dialogContent = "No threads linked to this process";
            }

            MessageBox.Show(dialogContent);
        }

        private readonly PerformanceCounter _theCpuCounter =
            new PerformanceCounter("Processor", "% Processor Time", "_Total");


        private void Select_Row(object sender, SelectionChangedEventArgs e)
        {
            RemindUserToSaveComment();
            ShowAttributes();
            DisplayComments();
        }

        private void CloseCommentMessageBox()
        {
            CommentBox.Visibility = Visibility.Collapsed;
            InputTextBox.Text = string.Empty;
            isCommentBoxOpen = false;
        }

        private void AskUserToSavePreviousComment(string comment)
        {
            string messageBoxText = "Would you like to save your comment first?";
            MessageBoxResult result = MessageBox.Show(messageBoxText, "", MessageBoxButton.OKCancel);
            if (result == MessageBoxResult.OK)
            {
                SaveComment(comment);
            }
        }

        private void DisplayComments()
        {
            if (processComments.ContainsKey(currentProcess.Id))
            {
                List<ProcessComment> commentList = new List<ProcessComment>();
                foreach (string comment in processComments[currentProcess.Id])
                {
                    commentList.Add(new ProcessComment() {Content = comment});
                }

                Comments.ItemsSource = commentList;
            }
            else
            {
                Comments.ItemsSource = null;
            }
        }

        private void SaveComment(string comment)
        {
            if (processComments.ContainsKey(currentProcess.Id))
            {
                processComments[currentProcess.Id].Add(comment);
            }
            else
            {
                processComments[currentProcess.Id] = new List<string>() {comment};
            }
        }

        private void CollectThreads()
        {
            foreach (ProcessThread processThread in currentProcess.Threads)
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
            string comment = InputTextBox.Text;
            SaveComment(comment);
            CloseCommentMessageBox();
            DisplayComments();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            CloseCommentMessageBox();
        }


        private void ProcessInfo_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            RemindUserToSaveComment();
            ShowAttributes();
            DisplayComments();
        }

        private void ShowAttributes()
        {
            ProcessList selectedProcess = (ProcessList) ProcessInfo.SelectedItem;
            List<ProcessAttributes> processAttributesList = new List<ProcessAttributes>();

            currentProcess = processes.Where(process => process.Id.Equals(selectedProcess.id)).First();

            try
            {
                var processAttribute = new ProcessAttributes()
                {
                    cpu = (this._theCpuCounter.NextValue() / 100).ToString("0.00") + "%",
                    memory = (currentProcess.PeakWorkingSet64 / (1024 * 1024)).ToString("0.0") + " MB",
                    starttime = currentProcess.StartTime,
                    runtime = currentProcess.TotalProcessorTime
                };

                processAttributesList.Add(processAttribute);

                Attributes.ItemsSource = processAttributesList;
            }
            catch (Exception)
            {
                MessageBox.Show("This process isn't running at this time.");
                Attributes.ItemsSource = null;
            }

            processThreads = new HashSet<ProcessThread>();
            CollectThreads();


        }

        private void RemindUserToSaveComment()
        {
            if (isCommentBoxOpen)
            {
                string comment = InputTextBox.Text;
                if (comment != "")
                {
                    AskUserToSavePreviousComment(comment);
                }

                CloseCommentMessageBox();
            }

        }

        private void AlwaysOnTop_CheckedChanged(object sender, RoutedEventArgs e)
        {
            Topmost = AlwaysOnTop.IsChecked == true;

        }
        internal class ProcessComment
        {
            public string Content { get; set; }
        }

        internal class ProcessList
        {
            public int id { get; set; }
            public string name { get; set; }


        }

        internal class ProcessAttributes
        {

            public string cpu { get; set; }
            public string memory { get; set; }
            public DateTime starttime { get; set; }
            public TimeSpan runtime { get; set; }

        }

    }
}
