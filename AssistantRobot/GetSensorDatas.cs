using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Reflection;

using LogPrinter;

namespace AssistantRobot
{
    /// <summary>
    /// 获得Senor数据
    /// </summary>
    public class GetSensorDatas
    {
        private Socket skt = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        private string serverIP = "127.0.0.1";
        private int serverPort = 40400;
        private int clientPort = 30400;

        private CancellationTokenSource recvCancelSource;
        private Task recvTask;

        public delegate void SendDoubleArray(double[] Datas); // double数组发送委托
        public delegate void SendBool(bool Flag); // bool发送委托

        public SendDoubleArray OnSendSensorDatas;
        public SendBool OnSendSensorConnectionBroken;

        private bool ifConnectionEstablished = false;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="ipAddress">IP地址</param>
        /// <param name="selfPort">客户端端口</param>
        /// <param name="oppositePort">服务端端口</param>
        public GetSensorDatas(string ipAddress = "127.0.0.1", int selfPort = 30400, int oppositePort = 40400)
        {
            serverIP = ipAddress;
            clientPort = selfPort;
            serverPort = oppositePort;
        }

        /// <summary>
        /// 连接到Sensor服务器
        /// </summary>
        /// <returns>返回连接结果</returns>
        public bool ConnectToSensorServer()
        {
            if (ifConnectionEstablished) return true;

            skt.Bind(new IPEndPoint(IPAddress.Parse(serverIP), clientPort));
            skt.ReceiveTimeout = 1000;

            try
            {
                skt.Connect(new IPEndPoint(IPAddress.Parse(serverIP), serverPort));
            }
            catch (Exception ex)
            {
                Logger.HistoryPrinting(Logger.Level.WARN, MethodBase.GetCurrentMethod().DeclaringType.FullName, "40400 port connection establishing failed.", ex);
                return false;
            }

            ifConnectionEstablished = true;

            recvCancelSource = new CancellationTokenSource();
            recvTask = new Task(() => RecvFunction(recvCancelSource.Token));
            recvTask.Start();

            return true;
        }

        /// <summary>
        /// 获得数据函数
        /// </summary>
        /// <param name="CancelFlag">停止标志</param>
        private void RecvFunction(CancellationToken CancelFlag)
        {
            Logger.HistoryPrinting(Logger.Level.INFO, MethodBase.GetCurrentMethod().DeclaringType.FullName, "Begin to recieve sensor datas.");

            while (true)
            {
                if (CancelFlag.IsCancellationRequested) break;

                byte[] buffer = new byte[100];
                try
                {
                    skt.Receive(buffer);
                }
                catch (Exception ex)
                {
                    ifConnectionEstablished = false;

                    Task.Run(new Action(() =>
                    {
                        OnSendSensorConnectionBroken(false);
                    }));

                    skt.Shutdown(SocketShutdown.Both);
                    skt.Close();
                    Logger.HistoryPrinting(Logger.Level.WARN, MethodBase.GetCurrentMethod().DeclaringType.FullName, "Recv datas failed.", ex);
                    break;
                }

                List<double> doubleBuffer = new List<double>(10);
                for (int i = 0; i < 10; i++)
                {
                    doubleBuffer.Add(
                        BitConverter.Int64BitsToDouble(
                        IPAddress.NetworkToHostOrder(
                        BitConverter.ToInt64(buffer, 0 + i * 8))));
                }

                OnSendSensorDatas(doubleBuffer.ToArray());
            }

            Task.Run(new Action(() =>
            {
                OnSendSensorConnectionBroken(true);
            })); 
            skt.Shutdown(SocketShutdown.Both);
            skt.Close();
            Logger.HistoryPrinting(Logger.Level.INFO, MethodBase.GetCurrentMethod().DeclaringType.FullName, "End to recieve sensor datas.");
        }

        /// <summary>
        /// 断开Sensor服务器连接
        /// </summary>
        /// <returns>返回断开结果</returns>
        public bool EndConnectionToSensorServer()
        {
            if (!ifConnectionEstablished) return true;

            recvCancelSource.Cancel();

            if (recvTask.Wait(2000))
            {
                Logger.HistoryPrinting(Logger.Level.INFO, MethodBase.GetCurrentMethod().DeclaringType.FullName, "Stop to recieve sensor datas.");
                return true;
            }
            else
            {
                Logger.HistoryPrinting(Logger.Level.WARN, MethodBase.GetCurrentMethod().DeclaringType.FullName, "Stop to recieve sensor datas time out.");
                return false; 
            }
        }
    }
}
