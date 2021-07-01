using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;


namespace ProcessNote
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Process[] _processes;
        private HashSet<ProcessThread> _processThreads = new HashSet<ProcessThread>();
        private Process _currentProcess;
        private readonly Dictionary<int, List<string>> _processComments = new Dictionary<int, List<string>>();
        private bool _isCommentBoxOpen = false;

        private readonly PerformanceCounter _theCpuCounter =
            new PerformanceCounter("Processor", "% Processor Time", "_Total");

        public MainWindow()
        {
            InitializeComponent();
            _processes = Process.GetProcesses();
            var processList = _processes.Select(process => new ListedProcess()
                {Id = process.Id, Name = process.ProcessName});

            ProcessInfo.ItemsSource = processList;
        }

        private void ShowThreads_Click(object sender, RoutedEventArgs e)
        {
            string dialogContent;
            if (_currentProcess == null)
            {
                dialogContent = "Please select a process first.";
            }
            else
            {
                try
                {
                    var threadList = _processThreads
                        .Select(processThread =>
                            $"    Thread ID: {processThread.Id}   Priority: {processThread.BasePriority}   State: {processThread.ThreadState}");
                    dialogContent = string.Join(Environment.NewLine, threadList);
                }
                catch (Exception)
                {
                    dialogContent = "No threads linked to this process.";
                }
            }

            MessageBox.Show(dialogContent);
        }


        private void Select_Row(object sender, SelectionChangedEventArgs e)
        {
            CheckIfCommentBoxIsOpen();
            ShowAttributes();
            DisplayComments();
        }

        private void CloseCommentBox()
        {
            CommentBox.Visibility = Visibility.Collapsed;
            InputTextBox.Text = string.Empty;
            _isCommentBoxOpen = false;
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
            if (_processComments.ContainsKey(_currentProcess.Id))
            {
                var commentList = _processComments[_currentProcess.Id]
                    .Select(comment => new ProcessComment() {Content = comment});
                Comments.ItemsSource = commentList;
            }
            else
            {
                Comments.ItemsSource = null;
            }
        }

        private void SaveComment(string comment)
        {
            if (_processComments.ContainsKey(_currentProcess.Id))
            {
                _processComments[_currentProcess.Id].Add(comment);
            }
            else
            {
                _processComments[_currentProcess.Id] = new List<string>() {comment};
            }
        }

        private void CollectThreads()
        {
            foreach (ProcessThread processThread in _currentProcess.Threads)
            {
                _processThreads.Add(processThread);
            }
        }

        private void AddComment_Click(object sender, RoutedEventArgs e)
        {
            if (_currentProcess == null)
            {
                MessageBox.Show("Please select a process");
            }
            else
            {
                CommentBox.Visibility = Visibility.Visible;
                _isCommentBoxOpen = true;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            string comment = InputTextBox.Text;
            SaveComment(comment);
            CloseCommentBox();
            DisplayComments();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            CloseCommentBox();
        }


        private void ProcessInfo_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            CheckIfCommentBoxIsOpen();
            ShowAttributes();
            DisplayComments();
        }

        private void ShowAttributes()
        {
            SetCurrentProcess();

            List<ProcessAttributes> processAttributesList = new List<ProcessAttributes>();

            try
            {
                var processAttribute = GetProcessAttributes();

                processAttributesList.Add(processAttribute);

                Attributes.ItemsSource = processAttributesList;

                _processThreads = new HashSet<ProcessThread>();
                CollectThreads();
            }
            catch (Exception)
            {
                MessageBox.Show("This process isn't running at this time.");
                Attributes.ItemsSource = null;
            }
        }

        private ProcessAttributes GetProcessAttributes()
        {
            var processAttribute = new ProcessAttributes()
            {
                Cpu = (this._theCpuCounter.NextValue() / 100).ToString("0.00") + "%",
                Memory = (_currentProcess.PeakWorkingSet64 / (1024 * 1024)).ToString("0.0") + " MB",
                StartTime = _currentProcess.StartTime,
                Runtime = _currentProcess.TotalProcessorTime
            };
            return processAttribute;
        }

        private void SetCurrentProcess()
        {
            ListedProcess selectedProcess = (ListedProcess) ProcessInfo.SelectedItem;
            _currentProcess = _processes.First(process => process.Id.Equals(selectedProcess.Id));
        }

        private void CheckIfCommentBoxIsOpen()
        {
            if (_isCommentBoxOpen)
            {
                string comment = InputTextBox.Text;
                if (comment != "")
                {
                    AskUserToSavePreviousComment(comment);
                }

                CloseCommentBox();
            }
        }

        private void AlwaysOnTop_CheckedChanged(object sender, RoutedEventArgs e)
        {
            Topmost = AlwaysOnTop.IsChecked == true;
        }

        private void GoogleSearch_OnClick(object sender, RoutedEventArgs e)
        {
            if (_currentProcess != null)
            {
                var googleSearch = "https://www.google.com/search?q=" + _currentProcess.ProcessName;
                Process.Start("chrome.exe", googleSearch);
            }
            else
            {
                MessageBox.Show("There is no process selected, there is nothing to search for.");
            }
        }

        internal class ProcessComment
        {
            public string Content { get; set; }
        }

        internal class ListedProcess
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        internal class ProcessAttributes
        {
            public string Cpu { get; set; }
            public string Memory { get; set; }
            public DateTime StartTime { get; set; }
            public TimeSpan Runtime { get; set; }
        }
    }
}