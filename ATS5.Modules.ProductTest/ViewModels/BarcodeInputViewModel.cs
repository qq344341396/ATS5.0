using Prism.Mvvm;

namespace ATS5.Modules.ProductTest.ViewModels
{
    public sealed class BarcodeInputViewModel : BindableBase
    {
        private string _value = string.Empty;

        public BarcodeInputViewModel(int index, string label)
        {
            Index = index;
            Label = label;
        }

        public int Index { get; }

        public string Label { get; }

        public string Value
        {
            get => _value;
            set => SetProperty(ref _value, value ?? string.Empty);
        }
    }
}
