using System;
using System.Windows;
using ILoveLucene.WindowsInterop;

namespace ILoveLucene.Views
{
    public partial class MainWindowView : Window
    {
        private readonly KeyboardHandler _globalHotKeyHandler;
        private readonly FocusHandler _focusHandler;

        public MainWindowView()
        {
            InitializeComponent();
            _globalHotKeyHandler = new KeyboardHandler(this, Toggle);
            _focusHandler = new FocusHandler(this);
        }

        protected override void OnClosed(EventArgs e)
        {
            if(_globalHotKeyHandler != null)
                _globalHotKeyHandler.Dispose(); 
        }

        public void Toggle()
        {
            if (this.Visibility == Visibility.Hidden)
            {
                this.Visibility = Visibility.Visible;
                this.Show();
                _focusHandler.SetForegroundWindow();
                SetTextTo(string.Empty);
                Input.Focus();
                
            }
            else
            {
                this.Visibility = Visibility.Hidden;
            }
        }

        private void SetTextTo(string text)
        {
            Input.Text = text;
        }
    }
}
