using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TCPTalkClient
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private TcpMessageService tcpMessageService = new TcpMessageService();
        private MainWindowViewModel viewModel = new MainWindowViewModel();

        public MainWindow()
        {
            DataContext = viewModel;
            InitializeComponent();
        }

        //连接状态发生变化事件处理函数，改变相关控件状态
        private void SetClientInfo(object sender, TcpMessageService.ClientStateChangedEventArgs e)
        {
            //this.textBoxForClientAdress.Text = e.ClientAddress;
            //this.textBoxForClientState.Text = e.ClientState;
            //this.textBoxForClientAdress.Dispatcher.BeginInvoke(new Action(()=> {
            //    this.textBoxForClientAdress.Text = e.ClientAddress;
            //}));
            //this.textBoxForClientState.Dispatcher.BeginInvoke(new Action(() => {
            //    this.textBoxForClientState.Text = e.ClientState;
            //}));
            viewModel.ClientAddress = e.ClientAddress;
            viewModel.ClientState = e.ClientState;
        }
        /// <summary>
        /// 接收到数据处理函数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UpdateHistoryMessage(object sender, TcpMessageService.HistoryMessageChangedEventArgs e)
        {
            string totalMessage = e.content;
            MessageModel newMessage = new MessageModel
            {
                SendTime = totalMessage.Split(',')[0],
                MessageContent = totalMessage.Split(',')[1]
            };
            viewModel.HistoryMessages.Add(newMessage);
            this.itemControlForHistoryMessages.Dispatcher.BeginInvoke(new Action(() =>
            {
                this.itemControlForHistoryMessages.Items.Add(newMessage);
            }));
        }
        private void buttonForSend_Click(object sender, RoutedEventArgs e)
        {
            if (this.textBoxForSendMessage.Text.Trim().Length == 0)
            {
                return;
            }
            if (viewModel.ClientState == "未连接")
            {
                return;
            }
            MessageModel message = new MessageModel { MessageContent = this.textBoxForSendMessage.Text.Trim() };
            try
            {
                tcpMessageService.SendMessage(message);

            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
                return;
            }

            //更新历史消息列表
            viewModel.HistoryMessages.Add(message);
            this.itemControlForHistoryMessages.Dispatcher.BeginInvoke(new Action(() =>
            {
                this.itemControlForHistoryMessages.Items.Add(message);
            }));
        }
        /// <summary>
        /// 连接到服务器
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonForConnect_Click(object sender, RoutedEventArgs e)
        {
            if (this.textBoxForServerAdress.Text.Trim().Length == 0)
            {
                return;
            }
            if (this.textBoxForServerPort.Text.Trim().Length == 0)
            {
                return;
            }
            if (viewModel.ClientState == "已连接")
            {
                return;
            }
            try
            {
                tcpMessageService.ConnectToServer(this.textBoxForServerAdress.Text.Trim(), this.textBoxForServerPort.Text.Trim());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //开始监听并建立连接
            //放在窗体加载之后，而不是构造函数里，不卡界面
            viewModel.ClientAddress = "192.168.50.167";
            viewModel.ServerPort = "11000";
            tcpMessageService.ClientStateChanged += SetClientInfo;
            tcpMessageService.HistoryMessageChanged += UpdateHistoryMessage;
            viewModel.ClientState = "未连接";
        }
    }
}
