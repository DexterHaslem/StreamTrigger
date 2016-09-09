using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Threading;

namespace StreamTrigger
{
    public abstract class NotifyPropertyChangedBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName]string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class ViewModel : NotifyPropertyChangedBase
    {
        private     int             pollRateSeconds = 60;
        private     int             uiUpdateCount;
        private     DateTime?       lastApiCheckTime;
        private     DateTime?       triggeredTime;
        private     MainWindow      view;
        private     string          streamName;
        private     string          fileToExecute;
        private     string          statusText;
        private     bool            hasTriggered;

        private     DispatcherTimer timer;

        public bool HasTriggered
        {
            get { return hasTriggered; }
            set
            {
                hasTriggered = value;
                OnPropertyChanged();
            }
        }

        public ViewModel(MainWindow view)
        {
            this.view = view;

            LoadSettings();

            StartTimer();

            UpdateStatusText();
        }

        private void LoadSettings()
        {
            PollRateSeconds = Properties.Settings.Default.PollRateSeconds;
            FileToExecute = Properties.Settings.Default.ScriptFilePath;
            StreamName = Properties.Settings.Default.StreamName;
            //if (string.IsNullOrWhiteSpace(FileToExecute) || !File.Exists(FileToExecute))
            //    FileToExecute = "";
        }

        internal void OnWindowClosing()
        {
            SaveSettings();

            timer.Stop();
        }

        private void StartTimer()
        {
            timer = new DispatcherTimer(DispatcherPriority.Background, view.Dispatcher);
            // run the timer on a 1 second interval for ui updates, but only hit api based on poll rate seconds
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += OnTimerTick;
            timer.Start();
        }

        private void OnTimerTick(object sender, EventArgs e)
        {
            Debug.WriteLine("OnTimerTick");
            if (!HasTriggered && ++uiUpdateCount >= PollRateSeconds)
            {
                Debug.WriteLine("OnTimerTick: check api");
                lastApiCheckTime = DateTime.Now;
                if (TwitchApi.CheckStreamIsOnline(StreamName))
                {
                    Debug.WriteLine("OnTimerTick: api triggered");
                    Trigger();
                }
                uiUpdateCount = 0;
            }
            UpdateStatusText();
        }

        private void Trigger()
        {
            HasTriggered = true;
            triggeredTime = DateTime.Now;
            if (File.Exists(FileToExecute))
                Process.Start(FileToExecute);
        }

        private void SaveSettings()
        {
            Properties.Settings.Default.PollRateSeconds = PollRateSeconds;
            Properties.Settings.Default.ScriptFilePath = FileToExecute;
            Properties.Settings.Default.StreamName = StreamName;
            Properties.Settings.Default.Save();
        }

        public void Reset()
        {
            HasTriggered = false;
            triggeredTime = null;
            lastApiCheckTime = null;
        }

        public string StreamName
        {
            get { return streamName; }
            set
            {
                streamName = value;
                Reset();
                OnPropertyChanged();
            }
        }

        public string FileToExecute
        {
            get { return fileToExecute; }
            set
            {
                fileToExecute = value;
                OnPropertyChanged();
            }
        }

        public string StatusText
        {
            get { return statusText; }
            private set
            {
                statusText = value;
                OnPropertyChanged();
            }
        }

        public int PollRateSeconds
        {
            get { return pollRateSeconds; }
            set
            {
                pollRateSeconds = Math.Max(1, value);
                // dont mess with the timer here, it needs to be stuck at 1 for ui updates
                OnPropertyChanged();
            }
        }

        public void UpdateStatusText()
        {
            if (HasTriggered)
            {
                StatusText = $"Not running, stream went online & triggered at {triggeredTime}";
            }
            else
            {
                StatusText = $"Checking stream in {PollRateSeconds - uiUpdateCount} seconds...";
            }
        }

        internal void OnFindScriptFile()
        {
            OpenFileDialog findFileDlg = new OpenFileDialog();
            findFileDlg.DefaultExt = ".bat";
            findFileDlg.Filter = "Batch (*.bat)|*.bat|Executable (*.exe)|*.exe|Powershell (*.ps1)|*.ps1";

            findFileDlg.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            findFileDlg.RestoreDirectory = true;

            var usePath = findFileDlg.ShowDialog() == true;
            if (!usePath)
                return;

            FileToExecute = findFileDlg.FileName;
        }

        internal void OnReset()
        {
            Reset();
        }
    }
}
