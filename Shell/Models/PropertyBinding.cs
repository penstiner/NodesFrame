using Nodify;

namespace Shell.Models
{
    public class PropertyBinding : ObservableObject
    {
        private bool _isBound;
        private string _boundVariableName;

        public bool IsBound { get => _isBound; set => SetProperty(ref _isBound, value); }
        public string BoundVariableName { get => _boundVariableName; set => SetProperty(ref _boundVariableName, value); }
    }
}
