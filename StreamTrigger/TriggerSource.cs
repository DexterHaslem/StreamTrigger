using System.Collections.ObjectModel;

namespace StreamTrigger
{
    internal enum TriggerSourceType
    {
        Unknown,
        Channel,
    }

    internal class TriggerSource : NotifyPropertyChangedBase
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
}