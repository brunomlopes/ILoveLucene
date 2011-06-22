using System;
using System.Windows;
using System.Windows.Forms;
using Core;
using ILoveLucene.WindowsInterop;
using Application = System.Windows.Application;

namespace ILoveLucene.Views
{
    public partial class MainWindowView : Window
    {
        private readonly KeyboardHandler _globalHotKeyHandler;
        private readonly FocusHandler _focusHandler;
        private NotifyIcon _notifyIcon;

        public MainWindowView()
        {
            InitializeComponent();
            _globalHotKeyHandler = new KeyboardHandler(this, Toggle);
            _focusHandler = new FocusHandler(this);
            Input.Focus();

            SetupNotifyIcon();
        }

        private void SetupNotifyIcon()
        {
            _notifyIcon = new System.Windows.Forms.NotifyIcon();
            _notifyIcon.Text = "ILoveLucene v" + ProgramVersionInformation.Version;
            var imageUri = new Uri("/ILoveLucene;component/Images/1305540894_heart_magnifier.ico", UriKind.Relative);
            using (var stream = Application.GetResourceStream(imageUri).Stream)
            {
                _notifyIcon.Icon = new System.Drawing.Icon(stream);
                _notifyIcon.DoubleClick += (sender, e) => ShowThisWindow();
                _notifyIcon.Visible = true;
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            if (_globalHotKeyHandler != null)
                _globalHotKeyHandler.Dispose();
            if(_notifyIcon != null)
                _notifyIcon.Dispose();
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