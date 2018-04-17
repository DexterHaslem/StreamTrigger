﻿using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Input;
using System.Windows.Threading;
using System.Xml.Serialization;

namespace StreamTrigger
{
    public class ViewModel : NotifyPropertyChangedBase
    {
        private const string AppDataDir = "StreamTrigger";
        private const string SavedTriggersFile = "triggers.xml";
        private const int SerializationVersion = 1; 

        private const int MinPollRateSecs = 15;
        private const int UiUpdateIntervalSecs = 1;

        private readonly TwitchApi _api;
        private string _streamName;
        private string _statusText;
        private bool? _streamIsOnline;
        private DispatcherTimer _timer;
        private int _pollRateSeconds = 60;
        private int _uiUpdateCount;
        private int _updatePercent;
        private string _updateTooltip;
        private readonly MainWindow _view;
        private string _wentOnlineFileToExecute;
        private string _wentOfflineFileToExecute;

        public ObservableCollection<string> LogItems { get; } = new ObservableCollection<string>();

        public ObservableCollection<TriggerSource> TriggerSources { get; } = new ObservableCollection<TriggerSource>();

        public ICommand AddTriggerSourceCommand { get; private set; }

        public int UpdatePercent
        {
            get => _updatePercent;
            set
            {
                _updatePercent = value;
                OnPropertyChanged();
            }
        }

        public string UpdateTooltip
        {
            get => _updateTooltip;
            set
            {
                _updateTooltip = value;
                OnPropertyChanged();
            }
        }

        public string StreamName
        {
            get => _streamName;
            set
            {
                _streamName = value;
                OnPropertyChanged();
            }
        }

        public string WentOnlineFileToExecute
        {
            get => _wentOnlineFileToExecute;
            set
            {
                _wentOnlineFileToExecute = value;
                OnPropertyChanged();
            }
        }

        public string WentOfflineFileToExecute
        {
            get => _wentOfflineFileToExecute;
            set
            {
                _wentOfflineFileToExecute = value;
                OnPropertyChanged();
            }
        }

        public string StatusText
        {
            get => _statusText;
            private set
            {
                _statusText = value;
                OnPropertyChanged();
            }
        }

        public int PollRateSeconds
        {
            get => _pollRateSeconds;
            set
            {
                _pollRateSeconds = Math.Max(MinPollRateSecs, value);
                // dont mess with the timer here, it needs to be stuck at 1 for ui updates
                OnPropertyChanged();
            }
        }

        public ViewModel(MainWindow view)
        {
            _api = new TwitchApi(Properties.Settings.Default.ClientId);
            _view = view;

            WireCommands();

            LoadSettings();

            LoadTriggerSources();

            StartTimer();

            view.Loaded += (o, e) =>
            {
                // force an initial update so we know stream status right away
                _uiUpdateCount = PollRateSeconds;
                OnTimerTick(this, null);
            };
        }

        private void WireCommands()
        {
            AddTriggerSourceCommand = new RoutedUICommand("AddTriggerSource", "AddTriggerSource", typeof(MainWindow));
            _view.CommandBindings.Add(new CommandBinding(AddTriggerSourceCommand, OnAddNewTriggerSource));        
        }

        private void OnAddNewTriggerSource(object sender, ExecutedRoutedEventArgs e)
        {
            var newSource = new TriggerSource
            {
                Name = "Stream name..",
                Triggers = new ObservableCollection<Trigger>(),
                Type = TriggerSourceType.Channel
            };

            TriggerSources.Add(newSource);
        }

        private void LoadTriggerSources()
        {
            var fullSavedTriggersPath = GetSavedTriggersPath();
            if (!File.Exists(fullSavedTriggersPath))
                return;

            using (var reader = new StreamReader(fullSavedTriggersPath))
            {
                var xs = new XmlSerializer(typeof(SavedTriggerSources));
                var savedTriggerSourcesSerInfo = (SavedTriggerSources)xs.Deserialize(reader);

                TriggerSources.Clear();

                foreach (var ts in savedTriggerSourcesSerInfo.TriggerSources)
                    TriggerSources.Add(ts);
            }
        }

        private static string GetSavedTriggersPath()
        {
            var appDataDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var streamTriggerDir = Path.Combine(appDataDir, AppDataDir);
            if (!Directory.Exists(streamTriggerDir))
                Directory.CreateDirectory(streamTriggerDir);

            var fullSavedTriggersPath = Path.Combine(streamTriggerDir, SavedTriggersFile);
            return fullSavedTriggersPath;
        }

        private void LoadSettings()
        {
            PollRateSeconds = Properties.Settings.Default.PollRateSeconds;
            WentOnlineFileToExecute = Properties.Settings.Default.WentOnlineFile;
            WentOfflineFileToExecute = Properties.Settings.Default.WentOfflineFile;
            StreamName = Properties.Settings.Default.StreamName;
            _view.Width = Properties.Settings.Default.WindowWidth;
            _view.Height = Properties.Settings.Default.WindowHeight;

            Log("Settings loaded");
        }

        private void SaveSettings()
        {
            Properties.Settings.Default.PollRateSeconds = PollRateSeconds;
            Properties.Settings.Default.WentOnlineFile = WentOnlineFileToExecute;
            Properties.Settings.Default.WentOfflineFile = WentOfflineFileToExecute;
            Properties.Settings.Default.StreamName = StreamName;
            Properties.Settings.Default.WindowWidth = _view.Width;
            Properties.Settings.Default.WindowHeight = _view.Height;
            Properties.Settings.Default.Save();
        }

        internal void OnWindowClosing()
        {
            SaveSettings();
            SaveTriggerSources();

            _timer.Stop();
        }

        private void SaveTriggerSources()
        {
            var fullSavedTriggersPath = GetSavedTriggersPath();
            using (var writer = new StreamWriter(fullSavedTriggersPath))
            {
                var xs = new XmlSerializer(typeof(SavedTriggerSources));
                var saveSerialize = new SavedTriggerSources
                {
                    SaveVersion = SerializationVersion,
                    TriggerSources = TriggerSources.ToArray(),
                };

                xs.Serialize(writer, saveSerialize);
            }
        }

        private void StartTimer()
        {
            // run the timer on a UIUpdateIntervalSecs second interval for ui updates,
            // but only hit api based on poll rate seconds
            _timer = new DispatcherTimer(DispatcherPriority.Background, _view.Dispatcher)
            {
                Interval = TimeSpan.FromSeconds(UiUpdateIntervalSecs)
            };
            _timer.Tick += OnTimerTick;
            _timer.Start();
        }

        private void OnTimerTick(object sender, EventArgs e)
        {
            if (++_uiUpdateCount > PollRateSeconds)
            {
                try
                {
                    var streamData = _api.GetCurrentStreamInfo(StreamName);

                    // if this is our very first poll, we have to make an educated
                    // guess and assume this is a good 'starting' point. while 
                    // we could technically miss a transition in our first poll, 
                    // its unlikely, and this is the only way we can prevent 
                    // errornous flipping on - if we start the app when a stream is already live, etc
                    var newStreamOnlineValue = streamData != null && streamData.Type == "live";

                    if (_streamIsOnline == null)
                        _streamIsOnline = newStreamOnlineValue;
                    else if (newStreamOnlineValue != _streamIsOnline.Value)
                        TriggerTransistion(newStreamOnlineValue, streamData);

                    _uiUpdateCount = 0;
                }
                catch (Exception ex)
                {
                    Log("Exception polling twitch API: " + ex.Message);
                }
            }

            UpdateStatus();
        }

        private void TriggerTransistion(bool newStatus, StreamResponseData data)
        {
            Debug.Assert(_streamIsOnline != null);
            var oldStatus = _streamIsOnline.Value;
            Log("Stream went " + (newStatus ? "ONLINE" : "OFFLINE"));

            if (newStatus)
            {
                LogLiveStreamInfo(data);
            }

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
            _streamIsOnline = newStatus;
        }

        private void LogLiveStreamInfo(StreamResponseData data)
        {
            Log("Live stream info *** ");
            Log($"\tTitle: \"{data.Title}\"");
            Log($"\tUser: {StreamName}");
            Log($"\tViewer Count: {data.ViewerCount}");
            Log($"\tStarted on: {data.StartedAt.ToLocalTime()}");
        }

        private void Log(string item)
        {
            LogItems.Add($"{DateTime.Now}: {item}");
        }

        public void UpdateStatus()
        {
            if (_streamIsOnline != null)
                StatusText = "Stream is " + (_streamIsOnline.Value ? "ONLINE" : "OFFLINE");
            else
                StatusText = "Getting stream state";

            var floatPct = Math.Round((float) _uiUpdateCount / PollRateSeconds * 100);
            UpdateTooltip = $"{floatPct:##.00}%";
            UpdatePercent = (int) floatPct;
        }

        internal void OnFindExecutableFile(bool isWentOfflineFile)
        {
            var findFileDlg = new OpenFileDialog
            {
                DefaultExt = ".bat",
                Filter = "Batch (*.bat)|*.bat|Executable (*.exe)|*.exe|Powershell (*.ps1)|*.ps1",
                RestoreDirectory = true
            };

            // don't set find dialog's initial directory in code, this will nuke user's last used on a fresh start

            var usePath = findFileDlg.ShowDialog() == true;
            if (!usePath)
                return;

            if (isWentOfflineFile)
                WentOfflineFileToExecute = findFileDlg.FileName;
            else
                WentOnlineFileToExecute = findFileDlg.FileName;
        }
    }
}