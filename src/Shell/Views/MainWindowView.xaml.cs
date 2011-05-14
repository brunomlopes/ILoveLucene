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
            if (_globalHotKeyHandler != null)
                _globalHotKeyHandler.Dispose();
        }

        public void Toggle()
        {
            if (!IsActive)
            {
                ClearInputBoxes();
                ShowThisWindow();
                return;
            }

            if (WindowState == WindowState.Minimized)
            {
                ClearInputBoxes();
                ShowThisWindow();
                return;
            }

            if (Visibility == Visibility.Hidden || Visibility == Visibility.Collapsed)
            {
                ShowThisWindow();
            }

            else
            {
                ClearInputBoxes();
                HideThisWindow();
            }
        }

        public void HideWindow()
        {
            ClearInputBoxes();
            HideThisWindow();
        }

        private void HideThisWindow()
        {
            Visibility = Visibility.Hidden;
        }

        private void ClearInputBoxes()
        {
            Input.Text = string.Empty;
            Description.Text = string.Empty;
            Arguments.Text = string.Empty;
        }

        private void ShowThisWindow()
        {
            Visibility = Visibility.Visible;
            Show();
            _focusHandler.SetForegroundWindow();
            Input.Focus();
        }
    }
}