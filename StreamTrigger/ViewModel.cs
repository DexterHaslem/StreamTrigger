using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
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
        private     DateTime?                       lastApiCheckTime;
        //private     DateTime?                     triggeredTime;
        private     MainWindow                      view;
        private     string                          streamName;
        private     string                          wentOnlineFileToExecute;
        private     string                          wentOfflineFileToExecute;
        private     string                          statusText;
        //private     bool                          hasTriggered;
        private     bool?                           streamIsOnline;
        private     int                             pollRateSeconds = 60;
        private     int                             uiUpdateCount;
        private     int                             updatePercent; // 0 to 100 for progress bar
        private     ObservableCollection<string>    logItems = new ObservableCollection<string>();
        private     DispatcherTimer timer;


        public ObservableCollection<string> LogItems => logItems;

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

            view.Loaded += (o,e) =>
            {
                // force an initial update so we know stream status right away
                uiUpdateCount = PollRateSeconds;
                OnTimerTick(this, null);
            };
            
        }

        private void LoadSettings()
        {
            PollRateSeconds = Properties.Settings.Default.PollRateSeconds;
            WentOnlineFileToExecute = Properties.Settings.Default.WentOnlineFile;
            WentOfflineFileToExecute = Properties.Settings.Default.WentOfflineFile;
            StreamName = Properties.Settings.Default.StreamName;
            view.Width = Properties.Settings.Default.WindowWidth;
            view.Height = Properties.Settings.Default.WindowHeight;
            Log("Settings loaded");
        }

        private void SaveSettings()
        {
            Properties.Settings.Default.PollRateSeconds = PollRateSeconds;
            Properties.Settings.Default.WentOnlineFile = WentOnlineFileToExecute;
            Properties.Settings.Default.WentOfflineFile = WentOfflineFileToExecute;
            Properties.Settings.Default.StreamName = StreamName;
            Properties.Settings.Default.WindowWidth = view.Width;
            Properties.Settings.Default.WindowHeight = view.Height;
            Properties.Settings.Default.Save();
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

                // if this is our very first poll, we have to make an educated
                // guess and assume this is a good 'starting' point. while 
                // we could technically miss a transition in our first poll, 
                // its unlikely, and this is the only way we can prevent 
                // errornous flipping on - if we start the app when a stream is already live, etc
                if (streamIsOnline == null)
                    streamIsOnline = newStatus;
                else if (newStatus != streamIsOnline.Value)
                    TriggerTransistion(newStatus);
                uiUpdateCount = 0;
            }
            UpdateStatus();
        }

        private void TriggerTransistion(bool newStatus)
        {
            Debug.Assert(streamIsOnline != null);
            bool oldStatus = streamIsOnline.Value;
            Log("Stream went " + (newStatus ? "ONLINE" : "OFFLINE"));
            // this method should only be called when things effectively change
            if (!oldStatus)
            {
                // stream just went ONLINE
                if (File.Exists(WentOnlineFileToExecute))
                {
                    Process.Start(WentOnlineFileToExecute);
                    Log("Started " + WentOnlineFileToExecute);
                }
            }
            else
            {
                // stream just went OFFLINE
                if (File.Exists(WentOfflineFileToExecute))
                {
                    Process.Start(WentOfflineFileToExecute);
                    Log("Started " + WentOfflineFileToExecute);
                }
            }

            // we're called before UI update, so set this and UI will be right
            streamIsOnline = newStatus;
        }

        private void Log(string item)
        {
            LogItems.Add($"{DateTime.Now}: {item}");
        }

        public void UpdateStatus()
        {
            if (streamIsOnline != null)
                StatusText = "Stream is " + (streamIsOnline.Value ? "ONLINE" : "OFFLINE");
            else
                StatusText = "Getting stream state";
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
