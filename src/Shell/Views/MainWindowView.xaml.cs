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
            Input.Focus();
        }

        protected override void OnClosed(EventArgs e)
        {
            if(_globalHotKeyHandler != null)
                _globalHotKeyHandler.Dispose(); 
        }

        public void Toggle()
        {
            if (WindowState == WindowState.Minimized)
            {
                ClearInputBoxes();
                ShowThisWindow();
                return;
            }

            if (this.Visibility == Visibility.Hidden || this.Visibility == System.Windows.Visibility.Collapsed)
            {
                ShowThisWindow();
            }

            else
            {
                ClearInputBoxes();
                HideThisWindow();
            }
        }

        private void HideThisWindow()
        {
            this.Visibility = Visibility.Hidden;
        }

        private void ClearInputBoxes()
        {
            Input.Text = string.Empty;
            Arguments.Text = string.Empty;
        }

        private void ShowThisWindow()
        {
            this.Visibility = Visibility.Visible;
            this.Show();
            _focusHandler.SetForegroundWindow();
            Input.Focus();
        }
    }
}
