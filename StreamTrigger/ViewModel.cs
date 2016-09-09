using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

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
        private     int         pollRateSeconds = 60;
        private     DateTime    lastCheckTime;
        private     MainWindow  view;
        private     string      streamName;
        private     string      fileToExecute;
        private     string      statusText;
        private     bool        hasTriggered;

       

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
                pollRateSeconds = value;
                OnPropertyChanged();
            }
        }

        public void UpdateStatusText()
        {
            // TODO:
        }

        internal void OnFindScriptFile()
        {
            OpenFileDialog findFileDlg = new OpenFileDialog();
            findFileDlg.DefaultExt = ".bat";
            findFileDlg.Filter = "Batch (*.bat)|*.bat|Executables (*.exe)|*.exe|Powershell (*.ps1)|*.ps1";

            findFileDlg.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            findFileDlg.RestoreDirectory = true;
        }

    }
}
