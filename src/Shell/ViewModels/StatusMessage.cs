using System.ComponentModel;
using System.ComponentModel.Composition;
using Core;

namespace ILoveLucene.ViewModels
{
    [Export(typeof(StatusMessage))]
    public class StatusMessage : INotifyPropertyChanged
    {
        private string _message;
        public string Message
        {
            get { return _message; }
            private set
            {
                _message = value;
                PropertyChanged(this, new PropertyChangedEventArgs("Message"));
            }
        }
        public void SetMessage(object sender, string message)
        {
            this.Message = message;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public StatusMessage()
        {
            PropertyChanged += (sender, e) => { };
            Message = string.Format("ILoveLucene version {0} built {1}", ProgramVersionInformation.Version, ProgramVersionInformation.PackageDate.ToLongDateString());
        }
    }
}