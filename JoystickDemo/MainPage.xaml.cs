using JoystickControl;
using System.ComponentModel;

namespace JoystickDemo
{
    public partial class MainPage : ContentPage, INotifyPropertyChanged
    {
        private float _x;
        private float _y;

        public float X
        {
            get => _x;
            set
            {
                if (_x != value)
                {
                    _x = value;
                    OnPropertyChanged(nameof(X));
                }
            }
        }

        public float Y
        {
            get => _y;
            set
            {
                if (_y != value)
                {
                    _y = value;
                    OnPropertyChanged(nameof(Y));
                }
            }
        }

        public MainPage()
        {
            InitializeComponent();
            BindingContext = this;
        }

        private void joystick_JoystickMoved(object sender, JoystickEventArgs e)
        {
            X = e.X;
            Y = e.Y;
        }

        public new event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

}
