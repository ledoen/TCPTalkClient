using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace TCPTalkClient
{
    /// <summary>
    /// 负责服务器与客户端的连接、接收、发送、关闭
    /// </summary>
    public class TcpMessageService
    {
        Socket localSocket = null;
        public TcpMessageService()
        {
            //新建socket并绑定IP和端口
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse("192.168.50.167"), 12000);
            localSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            localSocket.Bind(endPoint);
            localSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        }

        #region 连接状态变化事件
        public delegate void ClientStateChangedHandler(object sender, ClientStateChangedEventArgs e);
        public event ClientStateChangedHandler ClientStateChanged;
        public class ClientStateChangedEventArgs : EventArgs
        {
            public string ClientAddress { get; set; }
            public string ClientState { get; set; }
        }
        private void OnClientStateChanged(string address, string state)
        {
            ClientStateChanged?.Invoke(this,
                new ClientStateChangedEventArgs() { ClientAddress = address, ClientState = state });
        }
        #endregion

        #region 接收信息事件
        public delegate void HistoryMessageChangedHandler(object sender, HistoryMessageChangedEventArgs e);
        public event HistoryMessageChangedHandler HistoryMessageChanged;
        private void OnHistoryMessageChanged(string message)
        {
            HistoryMessageChanged?.Invoke(this, new HistoryMessageChangedEventArgs(message));
        }
        public class HistoryMessageChangedEventArgs : EventArgs
        {
            public string content { get; set; }
            public HistoryMessageChangedEventArgs(string message)
            {
                content = message;
            }
        }
        #endregion
        /// <summary>
        /// 连接到服务器
        /// </summary>
        public void ConnectToServer(string address, string port)
        {
            try
            {
                //请求连接
                localSocket.BeginConnect(IPAddress.Parse(address), Convert.ToInt32(port), AcceptCallback, localSocket);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        /// <summary>
        /// 连接成功的回调函数
        /// </summary>
        /// <param name="ar"></param>
        private void AcceptCallback(IAsyncResult ar)
        {
            //成功连接
            if (localSocket.Connected == false)
            {
                //return;
            }
            try
            {
                IPEndPoint clientEndPoint = (IPEndPoint)localSocket.RemoteEndPoint;
                //调用连接状态变化事件
                OnClientStateChanged(clientEndPoint.Address.ToString(), "已连接");
                ObjectState objectState = new ObjectState();
                objectState.socket = localSocket;

                //开始接收数据
                localSocket.BeginReceive(objectState.buffer, 0, objectState.buffer.Length, 0, ReadCallback, objectState);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 接收到数据时的回调函数
        /// </summary>
        /// <param name="ar"></param>
        private void ReadCallback(IAsyncResult ar)
        {
            byte[] buffer = ((ObjectState)ar.AsyncState).buffer;
            Socket handsocket = ((ObjectState)ar.AsyncState).socket;
            string str = Encoding.ASCII.GetString(buffer).Trim('\0');
            //判断是否为断开连接信号
            if (str.Length == 0)
            {
                HandleClientDisconnet();
                return;
            }
            Console.WriteLine(str.Length);
            OnHistoryMessageChanged(str);
            ObjectState objectState = new ObjectState();
            objectState.socket = handsocket;
            //继续下一次读取
            handsocket.BeginReceive(objectState.buffer, 0, objectState.buffer.Length, 0, ReadCallback, objectState);
        }

        /// <summary>
        /// 用于断开连接时的处理
        /// </summary>
        private void HandleClientDisconnet()
        {
            //调用连接状态变化事件
            OnClientStateChanged("", "未连接");
        }

        public void SendMessage(MessageModel message)
        {
            if (localSocket == null)
            {
                return;
            }
            if (localSocket.Connected == false)
            {
                return;
            }

            byte[] buffer = Encoding.ASCII.GetBytes(message.SendTime + "," + message.MessageContent);
            try
            {
                localSocket.Send(buffer, buffer.Length, 0);
            }
            catch (SocketException e)
            {
                if (e.ErrorCode == 10054)
                {
                    throw new Exception("服务器已断开连接");
                }
                else throw e;
            }
        }
    }

    /// <summary>
    /// 用于接收数据回归函数的传递对象，传递buffer和handlesocket
    /// </summary>
    internal class ObjectState
    {
        public byte[] buffer { get; set; }
        public Socket socket { get; set; }
        public ObjectState()
        {
            buffer = new byte[1024];
        }
    }
}
