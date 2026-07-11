namespace Philadelphus.Presentation.ViewModels.EntitiesVMs.InfrastructureVMs
{
    /// <summary>
    /// Модель представления дополнительного параметра строки подключения.
    /// </summary>
    public class ConnectionStringParameterVM : ViewModelBase
    {
        private string _key;
        private string _value;

        public string Key
        {
            get
            {
                return _key;
            }
            set
            {
                if (_key == value)
                    return;

                _key = value;
                OnPropertyChanged(nameof(Key));
            }
        }

        public string Value
        {
            get
            {
                return _value;
            }
            set
            {
                if (_value == value)
                    return;

                _value = value;
                OnPropertyChanged(nameof(Value));
            }
        }

        public ConnectionStringParameterVM(string key, string value)
        {
            _key = key;
            _value = value;
        }
    }
}
