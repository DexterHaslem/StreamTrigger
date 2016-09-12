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
        //private     DateTime?       triggeredTime;
        private     MainWindow      view;
        private     string          streamName;
        private     string          wentOnlineFileToExecute;
        private     string          wentOfflineFileToExecute;
        private     string          statusText;
        //private     bool            hasTriggered;
        private     bool            streamIsOnline;
        private     int             updatePercent; // 0 to 100 for progress bar

        private     DispatcherTimer timer;

        public int UpdatePercent
        {
            get { return updatePercent; }
            set
            {
                updatePercent = value;
                OnPropertyChanged();
            }
        }

        public string StreamName
        {
            get { return streamName; }
            set
            {
                streamName = value;
                OnPropertyChanged();
            }
        }

        public string WentOnlineFileToExecute
        {
            get { return wentOnlineFileToExecute; }
            set
            {
                wentOnlineFileToExecute = value;
                OnPropertyChanged();
            }
        }

        public string WentOfflineFileToExecute
        {
            get { return wentOfflineFileToExecute; }
            set
            {
                wentOfflineFileToExecute = value;
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

        public ViewModel(MainWindow view)
        {
            this.view = view;

            LoadSettings();

            StartTimer();

            UpdateStatus();
        }

        private void LoadSettings()
        {
            PollRateSeconds = Properties.Settings.Default.PollRateSeconds;
            WentOnlineFileToExecute = Properties.Settings.Default.ScriptFilePath;
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
            if (++uiUpdateCount > PollRateSeconds)
            {
                lastApiCheckTime = DateTime.Now;
                bool newStatus = TwitchApi.CheckStreamIsOnline(StreamName);
                if (newStatus != streamIsOnline)
                    TriggerTransistion(newStatus);
                uiUpdateCount = 0;
            }
            UpdateStatus();
        }

        private void TriggerTransistion(bool newStatus)
        {
            bool oldStatus = streamIsOnline;

            // this method should only be called when things effectively change
            if (!oldStatus)
            {
                // stream just went ONLINE
            }
            else
            {
                // stream just went OFFLINE
            }

            // we're called before UI update, so set this and UI will be right
            streamIsOnline = newStatus;
        }

        private void SaveSettings()
        {
            Properties.Settings.Default.PollRateSeconds = PollRateSeconds;
            Properties.Settings.Default.ScriptFilePath = WentOnlineFileToExecute;
            Properties.Settings.Default.StreamName = StreamName;
            Properties.Settings.Default.Save();
        }

        public void UpdateStatus()
        {
            StatusText = "Stream is " + (streamIsOnline ? "ONLINE" : "OFFLINE");
            UpdatePercent = (int)(((float)uiUpdateCount / PollRateSeconds) * 100);
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

            WentOnlineFileToExecute = findFileDlg.FileName;
        }
    }
}
