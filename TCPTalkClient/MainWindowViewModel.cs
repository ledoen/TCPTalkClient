using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace TCPTalkClient
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private string clientAddress;
        private string clientState;
        private string serverPort;

        public List<MessageModel> HistoryMessages { get; set; } = new List<MessageModel>();
        public string ClientAddress
        {
            get => clientAddress;
            set
            {
                clientAddress = value;
                OnPropertyChanged(nameof(ClientAddress));
            }
        }

        public string ServerPort
        {
            get => serverPort;
            set
            {
                serverPort = value;
                OnPropertyChanged(nameof(ServerPort));
            }
        }

        public string ClientState
        {
            get => clientState;
            set
            {
                clientState = value;
                OnPropertyChanged(nameof(clientState));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
