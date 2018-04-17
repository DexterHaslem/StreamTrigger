namespace StreamTrigger
{
    public enum TriggerCondition
    {
        Unknown,
        StreamWentOnline,
        StreamWentOffline,
    }

    public enum TriggerActionType
    {
        StartExternalProgram,
    }

    public class Trigger : NotifyPropertyChangedBase
    {
        private TriggerActionType _actionType;
        private string _actionParameter;
        private TriggerCondition _condition;

        public TriggerActionType ActionType
        {
            get => _actionType;
            set
            {
                _actionType = value;
                OnPropertyChanged();
            }
        }

        public string ActionParameter
        {
            get => _actionParameter;
            set
            {
                _actionParameter = value;
                OnPropertyChanged();
            }
        }

        public TriggerCondition Condition
        {
            get => _condition;
            set
            {
                _condition = value;
                OnPropertyChanged();
            }
        }
    }
}