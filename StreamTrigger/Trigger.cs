namespace StreamTrigger
{
    internal enum TriggerCondition
    {
        Unknown,
        StreamWentOnline,
        StreamWentOffline,
    }

    internal enum TriggerActionType
    {
        StartExternalProgram,
    }

    internal class Trigger : NotifyPropertyChangedBase
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