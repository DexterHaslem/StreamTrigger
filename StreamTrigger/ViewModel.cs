using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        private const   int         pollRateSeconds = 60;
        private         DateTime    lastCheckTime;
        private         MainWindow  view;
        private         string      streamName;

        
        private         string      fileToExecute;
        private         string      statusText;

        public bool HasTriggered { get; private set; }

        public ViewModel(MainWindow view)
        {
            this.view = view;

            LoadSettings();
        }

        private void LoadSettings()
        {
            
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
