using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ILoveLucene.WindowsInterop;

namespace ILoveLucene
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly KeyboardHandler _globalHotKeyHandler;
        private readonly FocusHandler _focusHandler;

        public MainWindow()
        {
            InitializeComponent();
            _globalHotKeyHandler = new KeyboardHandler(this, Toggle);
            _focusHandler = new FocusHandler(this);
            Input.Focus();
            Input.Text = "Start typing...";
            Input.SelectAll();
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
