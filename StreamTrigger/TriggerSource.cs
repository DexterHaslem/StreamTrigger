using System.Collections.ObjectModel;

namespace StreamTrigger
{
    public enum TriggerSourceType
    {
        Unknown,
        Channel,
    }

    public class TriggerSource : NotifyPropertyChangedBase
    {
        private string _name;
        private TriggerSourceType _type;

        public ObservableCollection<Trigger> Triggers { get; set;  } 

        public TriggerSourceType Type
        {
            get => _type;
            set
            {
                _type = value;
                OnPropertyChanged();
            }

        }

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }
    }

    public class SavedTriggerSources
    {
        // for save version conversion adapters to make backwards compatible 
        public int SaveVersion { get; set; }
        public TriggerSource[] TriggerSources { get; set; }
    }
}