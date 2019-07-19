using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Configuration;
using System.Reflection;
using System.IO;

using LogPrinter;

namespace AssistantRobot
{
    /// <summary>
    /// 通讯类
    /// </summary>
    public class CommunicationModel
    {
        private const byte header1 = 34; // 协议头1
        private const byte header2 = 84; // 协议头2

        #region 枚举 TCP协议
        /// <summary>
        /// TCP协议关键字
        /// </summary>
        public enum TCPProtocol : byte
        {
            Header1 = 0,
            Header2 = 1,
            DeviceID = 2,
            ProtocolKey = 3,
            DataLength = 4,
            DataContent = 8
        }

        /// <summary>
        /// TCP协议关键字 协议关键字
        /// </summary>
        public enum TCPProtocolKey : byte
        {
            RSAPublicKey = 1,
            AESCommonKeyAndIV = 2,
            NormalData = 12,
            EndRemoteControl = 13,
            PingSignal = 14,
            EndBothControl = 21
        }

        /// <summary>
        /// TCP协议关键字 协议关键字 AES公共密钥数据报格式
        /// </summary>
        public enum SecurityKeyLength : int
        {
            AESIVLength = 16,
            AESKeyLength = 32,
            RSAKeyLength = 1024
        }
        #endregion

        #region 枚举 Pipe通讯
        /// <summary>
        /// 应用协议总体格式
        /// </summary>
        public enum AppProtocol : byte
        {
            DataLength = 0,
            DataFlow = 4,
            DataKey = 5,
            DataContent = 6
        }

        /// <summary>
        /// 应用协议数据流向
        /// </summary>
        public enum AppProtocolDireciton : byte
        {
            Local2Remote = 9,
            Remote2Local = 219
        }

        /// <summary>
        /// 应用协议状态
        /// </summary>
        public enum AppProtocolStatus : byte
        {
            URRealTimeData = 1,
            URNetAbnormalAbort = 2,
            URWorkEmergencyState = 3,
            URNearSingularState = 4,
            URInitialPowerOnAsk = 5,
            URInitialPowerOnAskReply = 6,
            URAdditionalDeviceAbnormal = 7,

            BreastScanNipplePos = 100,
            BreastScanConfiguration = 101,
            BreastScanWorkStatus = 102,
            BreastScanConfigurationConfirmStatus = 103,
            BreastScanForceZerodStatus = 104,
            BreastScanConfigurationProcess = 151,
            BreastScanImmediateStop = 201,
            BreastScanImmediateStopRecovery = 202,

            ChangePage = 221,

            EndPipeConnection = 251
        }

        /// <summary>
        /// 应用协议状态 实时数据报格式
        /// </summary>
        public enum AppProtocolRTFeedBackDatagram : byte
        {
            TcpCoordinates = 0, // float
            JointCoordinates = 24, // float
            CurrentCoordinates = 48, // float
            TcpForce = 72, // float
            RobotState = 96, // byte
            RobotProgramState = 97 // byte
        }

        /// <summary>
        /// 应用协议状态 UR紧急状态数据报格式
        /// </summary>
        public enum AppProtocolEmergencyStateDatagram : byte
        {
            EmergencyState = 0 // byte: URDataProcessor.RobotEmergency
        }

        /// <summary>
        /// 应用协议状态 UR近奇异点状态数据报格式
        /// </summary>
        public enum AppProtocolNearSingularStateDatagram : byte
        {
            SingularState = 0
        }

        /// <summary>
        /// 应用协议状态 UR近奇异点状态数据报格式 奇异点种类
        /// </summary>
        public enum AppProtocolNearSingularStateDatagramClass : byte
        {
            ShoulderSingular = 0,
            ElbowSingular = 1,
            WristSingular = 2
        }

        /// <summary>
        /// 应用协议状态 UR外围设备异常状态数据报格式
        /// </summary>
        public enum AppProtocolAdditionalDeviceAbnormalDatagram : byte
        {
            AbnormalClass = 0
        }

        /// <summary>
        /// 应用协议状态 UR外围设备异常状态数据报格式 异常种类
        /// </summary>
        public enum AppProtocolAdditionalDeviceAbnormalDatagramClass : byte
        {
            DataBaseAttachFailed = 0,
            SerialPortAttachFailed = 1,
            URNetConnectionRecovery = 2
        }

        /// <summary>
        /// 应用协议状态 乳腺扫描配置数据报格式
        /// </summary>
        public enum AppProtocolBreastScanConfigurationDatagram : byte
        {
            DetectingErrorForceMinGDR = 0,
            DetectingErrorForceMaxGDR = 4,
            DetectingSpeedMinGDR = 8,
            IfEnableAngleCorrectedGDR = 12,
            NippleForbiddenRadiusGDR = 13,
            DetectingStopDistanceGDR = 17,
            DetectingSafetyLiftDistanceGDR = 21,
            IfEnableDetectingInitialForceGDR = 25,
            DetectingSinkDistanceGDR = 26,
            VibratingAngleDegreeGDR = 30,
            MovingSpeedDegreeGDR = 31,
            DetectingForceDegreeGDR = 32,
            DetectingAlignDegreeGDR = 33,
            MovingUpEdgeDistanceGDR = 34,
            MovingLeftEdgeDistanceGDR = 38,
            MovingDownEdgeDistanceGDR = 42,
            MovingRightEdgeDistanceGDR = 46,
            IfAutoReplaceConfigurationGDR = 50,
            IfCheckRightGalactophoreGDR = 51,
            IdentifyEdgeModeGDR = 52,
            CheckingStepGDR = 53
        }

        /// <summary>
        /// 应用协议状态 乳腺扫描工作状态数据报格式
        /// </summary>
        public enum AppProtocolBreastScanWorkStatusDatagram : byte
        {
            ModuleWorkingStatus = 0 // byte: OperateModuleBase.WorkStatus
        }

        /// <summary>
        /// 应用协议状态 乳腺扫描配置确认数据报格式
        /// </summary>
        public enum AppProtocolBreastScanConfigurationConfirmDatagram : byte
        {
            HasConfirmConfiguration = 0 // byte: 0--no 1--yes
        }

        /// <summary>
        /// 应用协议状态 乳腺扫描力清零数据报格式
        /// </summary>
        public enum AppProtocolBreastScanForceZerodDatagram : byte
        {
            HasForceZeroed = 0 // byte: 0--no 1--yes
        }

        /// <summary>
        /// 应用协议状态 乳腺扫描配置过程进度数据报格式
        /// </summary>
        public enum AppProtocolBreastScanConfigurationProcessDatagram : byte
        {
            ConfProcess = 0 // byte: 0--BeforeConfiguration
            // 1--NipplePos
            // 2--LiftDistance
            // 3--ForbiddenDistance
            // 4--ScanDepth
            // 5--UpEdge
            // 6--DownEdge
            // 7--LeftEdge
            // 8--RightEdge
            // 9--UpEdge
            // max--All
        }

        /// <summary>
        /// 应用协议指令
        /// </summary>
        public enum AppProtocolCommand : byte
        {
            MoveTcp = 1,
            MoveJoint = 2,
            MoveStop = 3,
            MoveReference = 4,
            MoveSpeed = 5,

            PowerOn = 51,
            BrakeRelease = 52,
            PowerOff = 53,
            AutoPowerOn = 61,

            ChangePage = 81,

            EnterBreastScanMode = 101,
            BreastScanModeBeginForceZeroed = 102,
            BreastScanModeBeginConfigurationSet = 103,
            BreastScanModeConfirmNipplePos = 104,
            BreastScanModeNextConfigurationItem = 105,
            BreastScanModeConfirmConfigurationSet = 106,
            BreastScanModeReadyAndStartBreastScan = 107,
            BreastScanModeSaveConfigurationSet = 108,

            StopBreastScanImmediately = 121,
            RecoveryFromStopBreastScanImmediately = 122,
            ExitBreastScanMode = 131
        }

        /// <summary>
        /// 应用协议指令 末端移动数据报格式
        /// </summary>
        public enum AppProtocolMoveTcpDatagram : byte
        {
            MoveDirection = 0, // byte: 0--negative 1--positive 
            MovePattern = 1, // byte: 0--translation 1--rotate
            MoveAxis = 2 // byte: 0--x 1--y 2--z
        }

        /// <summary>
        /// 应用协议指令 关节旋转数据报格式
        /// </summary>
        public enum AppProtocolMoveJointDatagram : byte
        {
            MoveDirection = 0, // byte: 0--negative 1--positive 
            MoveAxis = 1, // byte: 1~6--axis number
        }

        /// <summary>
        /// 应用协议指令 运动参考数据报格式
        /// </summary>
        public enum AppProtocolMoveReferenceDatagram : byte
        {
            ReferToBase = 0, // byte: 0--base 1--tool 
        }

        /// <summary>
        /// 应用协议指令 运动速度数据报格式
        /// </summary>
        public enum AppProtocolMoveSpeedDatagram : byte
        {
            SpeedRatio = 0, // float: 0.0~50.0
        }

        /// <summary>
        /// 应用协议指令 自动上电数据报格式
        /// </summary>
        public enum AppProtocolAutoPowerOnDatagram : byte
        {
            WhetherAutoPowerOn = 0, // byte: 0--No 1--Yes
        }

        /// <summary>
        /// 应用协议指令 换页数据报格式
        /// </summary>
        public enum AppProtocolChangePageDatagram : byte
        {
            AimPage = 0, // URViewModel.ShowPage
        }

        #endregion

        #region 字段 TCP
        private readonly bool ifAtSamePC = true;
        private readonly bool ifAtSameLAN = true;
        private readonly string netAdapterName = "unknown";

        private const string clientIPAtSamePC = "127.0.0.1";
        private const int clientPortTCPSendAtSamePC = 40003;
        private const int clientPortTCPRecieveAtSamePC = 40004;
        private const string serverIPAtSamePC = "127.0.0.1";

        private string clientIPAtDiffPC;
        private const int clientPortTCPSendAtDiffPC = 40001;
        private const int clientPortTCPRecieveAtDiffPC = 40002;
        private readonly string serverIPAtSameLAN = "192.168.1.13";
        private readonly string serverIPAtWAN = "202.120.48.24"; // 路由器的公网IP

        private const int serverPortTCPRecieveAny = 40001; // 端口转发应该设置同一端口
        private const int serverPortTCPSendAny = 40002; // 端口转发应该设置同一端口

        private readonly byte clientDeviceIndex = 1;

        private Socket tcpTransferSendSocket;
        private Socket tcpTransferRecieveSocket;
        private bool ifTCPTransferEstablished = false;

        private readonly int tcpSocketSendTimeOut = 500;
        private readonly int tcpSocketRecieveTimeOut = 1000;
        private readonly int tcpBeatInterval = 1000;
        private System.Timers.Timer tcpBeatClocker;

        private CancellationTokenSource tcpSendCancel;
        private Task tcpSendTask;
        private Queue<byte[]> tcpSendBuffer;
        private readonly int maxBufferSize = 10;
        private static readonly object tcpSendBufferLocker = new object();
        private readonly int waitTimerMsForBuffer = 10;

        private CancellationTokenSource tcpRecieveCancel;
        private Task tcpRecieveTask;

        private string publicKey = null;
        private string privateKey = null;
        const int remoteDevicePublicKeyLength = 1024;

        private byte[] commonKey = null;
        private byte[] commonIV = null;

        private bool existError = false;
        #endregion

        #region Construct
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="ifSuccessConstructed">是否成功构造</param>
        public CommunicationModel(out bool ifSuccessConstructed)
        {
            // 加载程序配置
            bool parseResult = true;

            bool ifAtSamePCTemp;
            parseResult = bool.TryParse(ConfigurationManager.AppSettings["ifAtSamePC"], out ifAtSamePCTemp);
            if (parseResult) ifAtSamePC = ifAtSamePCTemp;
            else
            {
                ifSuccessConstructed = false;
                Logger.HistoryPrinting(Logger.Level.WARN, MethodBase.GetCurrentMethod().DeclaringType.FullName, "App configuration parameter(" + "ifAtSamePC" + ") is wrong.");
                return;
            }

            bool ifAtSameLANTemp;
            parseResult = bool.TryParse(ConfigurationManager.AppSettings["ifAtSameLAN"], out ifAtSameLANTemp);
            if (parseResult) ifAtSameLAN = ifAtSameLANTemp;
            else
            {
                ifSuccessConstructed = false;
                Logger.HistoryPrinting(Logger.Level.WARN, MethodBase.GetCurrentMethod().DeclaringType.FullName, "App configuration parameter(" + "ifAtSameLAN" + ") is wrong.");
                return;
            }

            netAdapterName = ConfigurationManager.AppSettings["netAdapterName"];
            if (!ifAtSamePC)
            {
                parseResult = false;
                NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces();
                foreach (NetworkInterface adapter in adapters)
                {
                    if (adapter.Name == netAdapterName)
                    {
                        UnicastIPAddressInformationCollection unicastIPAddressInformation = adapter.GetIPProperties().UnicastAddresses;
                        foreach (var item in unicastIPAddressInformation)
                        {
                            if (item.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                            {
                                clientIPAtDiffPC = item.Address.ToString();
                                parseResult = true;
                                break;
                            }
                        }
                    }
                }
            }
            if (!parseResult)
            {
                ifSuccessConstructed = false;
                Logger.HistoryPrinting(Logger.Level.WARN, MethodBase.GetCurrentMethod().DeclaringType.FullName, "App configuration parameter(" + "netAdapterName" + ") is wrong.");
                return;
            }

            string serverIPAtSameLANTemp = ConfigurationManager.AppSettings["serverIPAtSameLAN"];
            if (new string(serverIPAtSameLANTemp.Take(10).ToArray()) == "192.168.1.") serverIPAtSameLAN = serverIPAtSameLANTemp;
            else
            {
                ifSuccessConstructed = false;
                Logger.HistoryPrinting(Logger.Level.WARN, MethodBase.GetCurrentMethod().DeclaringType.FullName, "App configuration parameter(" + "serverIPAtSameLAN" + ") is wrong.");
                return;
            }

            string serverIPAtWANTemp = ConfigurationManager.AppSettings["serverIPAtWAN"];
            if (serverIPAtWANTemp.Trim() == serverIPAtWANTemp) serverIPAtWAN = serverIPAtWANTemp;
            else
            {
                ifSuccessConstructed = false;
                Logger.HistoryPrinting(Logger.Level.WARN, MethodBase.GetCurrentMethod().DeclaringType.FullName, "App configuration parameter(" + "serverIPAtWAN" + ") is wrong.");
                return;
            }

            byte clientDeviceIndexTemp;
            parseResult = byte.TryParse(ConfigurationManager.AppSettings["clientDeviceIndex"], out clientDeviceIndexTemp);
            if (parseResult) clientDeviceIndex = clientDeviceIndexTemp;
            else
            {
                ifSuccessConstructed = false;
                Logger.HistoryPrinting(Logger.Level.WARN, MethodBase.GetCurrentMethod().DeclaringType.FullName, "App configuration parameter(" + "clientDeviceIndex" + ") is wrong.");
                return;
            }

            int tcpSocketSendTimeOutTemp;
            parseResult = int.TryParse(ConfigurationManager.AppSettings["tcpSocketSendTimeOut"], out tcpSocketSendTimeOutTemp);
            if (parseResult) tcpSocketSendTimeOut = tcpSocketSendTimeOutTemp;
            else
            {
                ifSuccessConstructed = false;
                Logger.HistoryPrinting(Logger.Level.WARN, MethodBase.GetCurrentMethod().DeclaringType.FullName, "App configuration parameter(" + "tcpSocketSendTimeOut" + ") is wrong.");
                return;
            }

            int tcpSocketRecieveTimeOutTemp;
            parseResult = int.TryParse(ConfigurationManager.AppSettings["tcpSocketRecieveTimeOut"], out tcpSocketRecieveTimeOutTemp);
            if (parseResult) tcpSocketRecieveTimeOut = tcpSocketRecieveTimeOutTemp;
            else
            {
                ifSuccessConstructed = false;
                Logger.HistoryPrinting(Logger.Level.WARN, MethodBase.GetCurrentMethod().DeclaringType.FullName, "App configuration parameter(" + "tcpSocketRecieveTimeOut" + ") is wrong.");
                return;
            }

            int tcpBeatIntervalTemp;
            parseResult = int.TryParse(ConfigurationManager.AppSettings["tcpBeatInterval"], out tcpBeatIntervalTemp);
            if (parseResult) tcpBeatInterval = tcpBeatIntervalTemp;
            else
            {
                ifSuccessConstructed = false;
                Logger.HistoryPrinting(Logger.Level.WARN, MethodBase.GetCurrentMethod().DeclaringType.FullName, "App configuration parameter(" + "tcpBeatInterval" + ") is wrong.");
                return;
            }

            int maxBufferSizeTemp;
            parseResult = int.TryParse(ConfigurationManager.AppSettings["maxBufferSize"], out maxBufferSizeTemp);
            if (parseResult) maxBufferSize = maxBufferSizeTemp;
            else
            {
                ifSuccessConstructed = false;
                Logger.HistoryPrinting(Logger.Level.WARN, MethodBase.GetCurrentMethod().DeclaringType.FullName, "App configuration parameter(" + "maxBufferSize" + ") is wrong.");
                return;
            }

            int waitTimerMsForBufferTemp;
            parseResult = int.TryParse(ConfigurationManager.AppSettings["waitTimerMsForBuffer"], out waitTimerMsForBufferTemp);
            if (parseResult) waitTimerMsForBuffer = waitTimerMsForBufferTemp;
            else
            {
                ifSuccessConstructed = false;
                Logger.HistoryPrinting(Logger.Level.WARN, MethodBase.GetCurrentMethod().DeclaringType.FullName, "App configuration parameter(" + "waitTimerMsForBuffer" + ") is wrong.");
                return;
            }

            // 装上TCP心跳定时器
            tcpBeatClocker = new System.Timers.Timer(tcpBeatInterval);
            tcpBeatClocker.AutoReset = true;
            tcpBeatClocker.Elapsed += tcpBeatClocker_Elapsed;

            // 缓冲区初始化
            tcpSendBuffer = new Queue<byte[]>(maxBufferSize);

            ifSuccessConstructed = true;
        }
        #endregion

        #region Interface
        #region Interface Functions
        /// <summary>
        /// 连接到服务器
        /// </summary>
        /// <returns>返回连接的结果：0（成功）| -1（已连接）| -2（连接出错）| -3（未知错误）</returns>
        public int ConnectToServer()
        {
            // TCP连接已经建立就退出
            if (ifTCPTransferEstablished) return -1;

            // 更新RSA公钥和密钥
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(1024);
            publicKey = rsa.ToXmlString(false);
            privateKey = rsa.ToXmlString(true);

            // 建立新的TCP连接
            // Client --> Server
            tcpTransferSendSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            tcpTransferSendSocket.Bind(new IPEndPoint(IPAddress.Parse(ifAtSamePC ? clientIPAtSamePC : clientIPAtDiffPC), ifAtSamePC ? clientPortTCPSendAtSamePC : clientPortTCPSendAtDiffPC));
            tcpTransferSendSocket.SendTimeout = tcpSocketSendTimeOut;
            tcpTransferSendSocket.ReceiveTimeout = -1;
            try
            {
                tcpTransferSendSocket.Connect(new IPEndPoint(IPAddress.Parse(ifAtSamePC ? serverIPAtSamePC : (ifAtSameLAN ? serverIPAtSameLAN : serverIPAtWAN)), serverPortTCPRecieveAny));
            }
            catch (SocketException ex)
            {
                tcpTransferSendSocket.Close();
                if (ex.SocketErrorCode == SocketError.ConnectionRefused || ex.SocketErrorCode == SocketError.TimedOut)
                {
                    Logger.HistoryPrinting(Logger.Level.INFO, MethodBase.GetCurrentMethod().DeclaringType.FullName, "AssistantRobot remote contoller tcp connection can not established.", ex);
                    return -2;
                }
                else
                {
                    Logger.HistoryPrinting(Logger.Level.WARN, MethodBase.GetCurrentMethod().DeclaringType.FullName, "Not deal exception during tcp connection.", ex);
                    return -3;
                }
            }

            // Client <-- Server
            tcpTransferRecieveSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            tcpTransferRecieveSocket.Bind(new IPEndPoint(IPAddress.Parse(ifAtSamePC ? clientIPAtSamePC : clientIPAtDiffPC), ifAtSamePC ? clientPortTCPRecieveAtSamePC : clientPortTCPRecieveAtDiffPC));
            tcpTransferRecieveSocket.SendTimeout = -1;
            tcpTransferRecieveSocket.ReceiveTimeout = -1;
            try
            {
                tcpTransferRecieveSocket.Connect(new IPEndPoint(IPAddress.Parse(ifAtSamePC ? serverIPAtSamePC : (ifAtSameLAN ? serverIPAtSameLAN : serverIPAtWAN)), serverPortTCPSendAny));
            }
            catch (SocketException ex)
            {
                try
                {
                    tcpTransferSendSocket.Shutdown(SocketShutdown.Both);
                    tcpTransferSendSocket.Close();
                    Logger.HistoryPrinting(Logger.Level.INFO, MethodBase.GetCurrentMethod().DeclaringType.FullName, "Half-connection is established, close the established direction.");
                }
                catch (Exception a_ex)
                {
                    Logger.HistoryPrinting(Logger.Level.WARN, MethodBase.GetCurrentMethod().DeclaringType.FullName, "Half-connection is established, close the established direction but failed.");
                }

                tcpTransferRecieveSocket.Close();
                if (ex.SocketErrorCode == SocketError.ConnectionRefused || ex.SocketErrorCode == SocketError.TimedOut)
                {
                    Logger.HistoryPrinting(Logger.Level.INFO, MethodBase.GetCurrentMethod().DeclaringType.FullName, "AssistantRobot remote contoller tcp connection can not established.", ex);
                    return -2;
                }
                else
                {
                    Logger.HistoryPrinting(Logger.Level.WARN, MethodBase.GetCurrentMethod().DeclaringType.FullName, "Not deal exception during tcp connection.", ex);
                    return -3;
                }
            }

            ifTCPTransferEstablished = true;
            Logger.HistoryPrinting(Logger.Level.INFO, MethodBase.GetCurrentMethod().DeclaringType.FullName, "AssistantRobot remote contoller tcp connection has been established.");

            // 开始允许TCP发送队列内的数据
            tcpSendCancel = new CancellationTokenSource();
            tcpSendTask = new Task(() => TcpSendTaskWork(tcpSendCancel.Token));
            tcpSendTask.Start();
            Logger.HistoryPrinting(Logger.Level.INFO, MethodBase.GetCurrentMethod().DeclaringType.FullName, "AssistantRobot remote contoller tcp tranfer can send datas.");

            // 心跳发送定时器打开
            tcpBeatClocker.Start();
            Logger.HistoryPrinting(Logger.Level.INFO, MethodBase.GetCurrentMethod().DeclaringType.FullName, "AssistantRobot remote contoller tcp begin to beat.");

            // 发送RSA公钥
            SendCmd(TCPProtocolKey.RSAPublicKey, Encoding.UTF8.GetBytes(publicKey));

            // 开始允许TCP接收数据
            tcpRecieveCancel = new CancellationTokenSource();
            tcpRecieveTask = new Task(() => TcpRecieveTaskWork(tcpSendCancel.Token));
            tcpRecieveTask.Start();
            Logger.HistoryPrinting(Logger.Level.INFO, MethodBase.GetCurrentMethod().DeclaringType.FullName, "AssistantRobot remote contoller tcp tranfer can recieve datas.");

            return 0;
        }

        /// <summary>
        /// 结束到服务器的连接
        /// </summary>
        /// <param name="ifCloseLocalControl">是否关闭本地控制</param>
        /// <returns>返回中止连接的结果</returns>
        public bool EndConnectionToServer(bool ifCloseLocalControl = false)
        {
            // TCP连接未建立就退出
            if (!ifTCPTransferEstablished) return false;

            // 发送停止接收
            if (ifCloseLocalControl) SendCmd(TCPProtocolKey.EndBothControl);
            else SendCmd(TCPProtocolKey.EndRemoteControl);

            return true;
        }

        /// <summary>
        /// 发送指令
        /// </summary>
        /// <param name="tcpKey">TCP关键字</param>
        /// <param name="content">发送内容</param>
        /// <param name="appCmd">APP命令字</param>
        public void SendCmd(TCPProtocolKey tcpKey, byte[] content = null, AppProtocolCommand appCmd = AppProtocolCommand.PowerOff)
        {
            if (!ifTCPTransferEstablished) return;

            switch (tcpKey)
            {
                case TCPProtocolKey.EndBothControl:
                case TCPProtocolKey.EndRemoteControl:
                case TCPProtocolKey.PingSignal:
                    // TCP打包 无内容
                    lock (tcpSendBufferLocker)
                    { //  入队
                        if (tcpSendBuffer.Count < maxBufferSize)
                            tcpSendBuffer.Enqueue(PackageTCP(tcpKey));
                        else
                            Logger.HistoryPrinting(Logger.Level.WARN, MethodBase.GetCurrentMethod().DeclaringType.FullName, "Send queue is full.");
                    }
                    break;
                case TCPProtocolKey.RSAPublicKey:
                    // TCP打包 有内容
                    if (Object.Equals(content, null)) return;
                    lock (tcpSendBufferLocker)
                    { //  入队
                        if (tcpSendBuffer.Count < maxBufferSize)
                            tcpSendBuffer.Enqueue(PackageTCP(tcpKey, content));
                        else
                            Logger.HistoryPrinting(Logger.Level.WARN, MethodBase.GetCurrentMethod().DeclaringType.FullName, "Send queue is full.");
                    }
                    break;
                case TCPProtocolKey.NormalData:
                    byte[] encryptedBytes = EncryptByAES( // AES加密
                                                                        PackagePipe( // Pipe打包
                                                                            appCmd, content
                                                                        )
                                                                    );
                    if (Object.Equals(encryptedBytes, null)) return; // 加密出错

                    lock (tcpSendBufferLocker)
                    {
                        if (tcpSendBuffer.Count < maxBufferSize)
                        {
                            tcpSendBuffer.Enqueue( //  入队
                                                            PackageTCP( // TCP打包
                                                                tcpKey, encryptedBytes
                                                            )
                                                        );
                        }
                        else
                            Logger.HistoryPrinting(Logger.Level.WARN, MethodBase.GetCurrentMethod().DeclaringType.FullName, "Send queue is full.");
                    }
                    break;
                case TCPProtocolKey.AESCommonKeyAndIV:
                default:
                    break;
            }

            if (tcpKey != TCPProtocolKey.PingSignal)
            {
                Logger.HistoryPrinting(Logger.Level.INFO, MethodBase.GetCurrentMethod().DeclaringType.FullName, "Ready to send msg \"" +
                    Enum.GetName(tcpKey.GetType(), tcpKey) +
                    (tcpKey == TCPProtocolKey.NormalData ? " - " + Enum.GetName(appCmd.GetType(), appCmd) + "\"." : "\"."));
            }
        }
        #endregion

        #region Interface Event
        public delegate void SendDoubleArray(double[] sendArray);
        public delegate void SendVoid();
        public delegate void SendIndex(int sendIndex);
        public delegate void SendStringArrayList(List<string[]> sendList);
        public delegate void SendBool(bool sendBool);

        public event SendDoubleArray OnSendURRealTimeData;
        public event SendVoid OnSendURNetAbnormalAbort;
        public event SendIndex OnSendURWorkEmergencyState;
        public event SendIndex OnSendURNearSingularState;
        public event SendVoid OnSendURInitialPowerOnAsk;
        public event SendVoid OnSendURInitialPowerOnAskReply;
        public event SendIndex OnSendURAdditionalDeviceAbnormal;

        public event SendVoid OnSendBreastScanNipplePos;
        public event SendStringArrayList OnSendBreastScanConfiguration;
        public event SendIndex OnSendBreastScanWorkStatus;
        public event SendBool OnSendBreastScanConfigurationConfirmStatus;
        public event SendBool OnSendBreastScanForceZerodStatus;
        public event SendIndex OnSendBreastScanConfigurationProcess;
        public event SendVoid OnSendBreastScanImmediateStop;
        public event SendVoid OnSendBreastScanImmediateStopRecovery;

        public event SendBool OnSendTcpDisconnected;

        public event SendIndex OnSendChangePage;
        #endregion
        #endregion

        #region Transfer
        /// <summary>
        /// TCP传输心跳定时器
        /// </summary>
        private void tcpBeatClocker_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            SendCmd(TCPProtocolKey.PingSignal);
        }

        /// <summary>
        /// TCP发送数据任务
        /// </summary>
        /// <param name="cancelFlag">停止标志</param>
        private void TcpSendTaskWork(CancellationToken cancelFlag)
        {
            Logger.HistoryPrinting(Logger.Level.INFO, MethodBase.GetCurrentMethod().DeclaringType.FullName, "AssistantRobot remote controller tcp transfer begins to send datas.");

            while (true)
            {
                if (cancelFlag.IsCancellationRequested) break;

                Thread.Sleep(waitTimerMsForBuffer);

                byte[] sendBytes = null;
                lock (tcpSendBufferLocker)
                {
                    if (tcpSendBuffer.Count > 0) // 出队
                    {
                        sendBytes = tcpSendBuffer.Dequeue();
                    }
                }

                if (Object.Equals(sendBytes, null)) continue; // 空队直接继续

                try
                {
                    tcpTransferSendSocket.Send(sendBytes.ToArray());
                }
                catch (SocketException ex)
                {
                    if (ex.SocketErrorCode == SocketError.ConnectionReset || ex.SocketErrorCode == SocketError.ConnectionAborted || ex.SocketErrorCode == SocketError.TimedOut)
                    {
                        EndAllLoop();
                        Logger.HistoryPrinting(Logger.Level.INFO, MethodBase.GetCurrentMethod().DeclaringType.FullName, "AssistantRobot remote controller tcp transfer send datas failed.", ex);
                    }
                    else
                    {
                        Logger.HistoryPrinting(Logger.Level.WARN, MethodBase.GetCurrentMethod().DeclaringType.FullName, "Not deal exception.", ex);
                        EndAllLoop(false);
                    }
                }
            }

            Logger.HistoryPrinting(Logger.Level.INFO, MethodBase.GetCurrentMethod().DeclaringType.FullName, "AssistantRobot remote controller tcp transfer stops to send datas.");

            tcpRecieveTask.Wait();
            Logger.HistoryPrinting(Logger.Level.INFO, MethodBase.GetCurrentMethod().DeclaringType.FullName, "AssistantRobot remote controller tcp transfer stops to send datas, and recieve datas too.");

            bool ifError = FinishAllConnection();
            OnSendTcpDisconnected(ifError);

            ifTCPTransferEstablished = false;
        }

        /// <summary>
        /// TCP接收数据任务
        /// </summary>
        /// <param name="cancelFlag">停止标志</param>
        private void TcpRecieveTaskWork(CancellationToken cancelFlag)
        {
            Logger.HistoryPrinting(Logger.Level.INFO, MethodBase.GetCurrentMethod().DeclaringType.FullName, "AssistantRobot remote controller tcp transfer begins to recieve datas.");

            while (true)
            {
                if (cancelFlag.IsCancellationRequested) break;

                try
                {
                    byte[] reciveDatas = new byte[1024 + 8];
                    int actualLength = tcpTransferRecieveSocket.Receive(reciveDatas);
                    DealWithTcpRecieveDatas(reciveDatas.Take(actualLength).ToArray());
                }
                catch (SocketException ex)
                {
                    if (ex.SocketErrorCode == SocketError.ConnectionReset || ex.SocketErrorCode == SocketError.ConnectionAborted || ex.SocketErrorCode == SocketError.TimedOut)
                    {
                        EndAllLoop();
                        Logger.HistoryPrinting(Logger.Level.INFO, MethodBase.GetCurrentMethod().DeclaringType.FullName, "AssistantRobot remote controller tcp transfer recieve datas failed.", ex);
                    }
                    else
                    {
                        Logger.HistoryPrinting(Logger.Level.WARN, MethodBase.GetCurrentMethod().DeclaringType.FullName, "Not deal exception.", ex);
                        EndAllLoop(false);
                    }
                }
            }

            Logger.HistoryPrinting(Logger.Level.INFO, MethodBase.GetCurrentMethod().DeclaringType.FullName, "AssistantRobot remote controller tcp transfer stops to recieve datas.");
        }

        /// <summary>
        /// 处理TCP接收的数据
        /// </summary>
        /// <param name="datas">所收数据</param>
        private void DealWithTcpRecieveDatas(byte[] datas)
        {
            // TCP解包
            bool dealResult;
            byte[] contentTcp = UnpackageTCP(datas, out dealResult);
            if (!dealResult) 
                return; // 解包失败
            if (Object.Equals(contentTcp, null)) 
                return; // 无需后续操作

            // AES解密
            byte[] bytesDecryptedByAES = DecryptByAES(contentTcp);
            if (Object.Equals(bytesDecryptedByAES, null)) 
                return; // AES解密失败

            // Pipe解包
            byte key;
            byte[] contentPipe = UnpackagePipe(bytesDecryptedByAES, out key, out dealResult);
            if (!dealResult) 
                return; // 解包失败

            // 数据处理
            if (tcpTransferRecieveSocket.ReceiveTimeout < 0) tcpTransferRecieveSocket.ReceiveTimeout = tcpSocketRecieveTimeOut;
            DealRecievedPipeData((AppProtocolStatus)key, contentPipe);
        }

        /// <summary>
        /// 处理收到的Pipe数据
        /// </summary>
        /// <param name="keyStatus">状态</param>
        /// <param name="content">内容</param>
        private void DealRecievedPipeData(AppProtocolStatus keyStatus, byte[] content)
        {
            switch (keyStatus)
            {
                case AppProtocolStatus.URRealTimeData:
                    OnSendURRealTimeData(GetURRealTimeDatasFromBytes(content));
                    break;
                case AppProtocolStatus.URNetAbnormalAbort:
                    OnSendURNetAbnormalAbort();
                    break;
                case AppProtocolStatus.URWorkEmergencyState:
                    OnSendURWorkEmergencyState(content[(byte)AppProtocolEmergencyStateDatagram.EmergencyState]);
                    break;
                case AppProtocolStatus.URNearSingularState:
                    OnSendURNearSingularState(content[(byte)AppProtocolNearSingularStateDatagram.SingularState]);
                    break;
                case AppProtocolStatus.URInitialPowerOnAsk:
                    OnSendURInitialPowerOnAsk();
                    break;
                case AppProtocolStatus.URInitialPowerOnAskReply:
                    OnSendURInitialPowerOnAskReply();
                    break;
                case AppProtocolStatus.URAdditionalDeviceAbnormal:
                    OnSendURAdditionalDeviceAbnormal(content[(byte)AppProtocolAdditionalDeviceAbnormalDatagram.AbnormalClass]);
                    break;

                case AppProtocolStatus.BreastScanNipplePos:
                    OnSendBreastScanNipplePos();
                    break;
                case AppProtocolStatus.BreastScanConfiguration:
                    OnSendBreastScanConfiguration(GetBreastScanConfFromBytes(content));
                    break;
                case AppProtocolStatus.BreastScanWorkStatus:
                    OnSendBreastScanWorkStatus(Math.Min(Math.Max(content[(byte)AppProtocolBreastScanWorkStatusDatagram.ModuleWorkingStatus] - 10, -1), 100));
                    break;
                case AppProtocolStatus.BreastScanConfigurationConfirmStatus:
                    OnSendBreastScanConfigurationConfirmStatus(content[(byte)AppProtocolBreastScanConfigurationConfirmDatagram.HasConfirmConfiguration] == 1);
                    break;
                case AppProtocolStatus.BreastScanForceZerodStatus:
                    OnSendBreastScanForceZerodStatus(content[(byte)AppProtocolBreastScanForceZerodDatagram.HasForceZeroed] == 1);
                    break;
                case AppProtocolStatus.BreastScanConfigurationProcess:
                    OnSendBreastScanConfigurationProcess(content[(byte)AppProtocolBreastScanConfigurationProcessDatagram.ConfProcess]);
                    break;
                case AppProtocolStatus.BreastScanImmediateStop:
                    OnSendBreastScanImmediateStop();
                    break;
                case AppProtocolStatus.BreastScanImmediateStopRecovery:
                    OnSendBreastScanImmediateStopRecovery();
                    break;

                case AppProtocolStatus.ChangePage:
                    OnSendChangePage(content[(byte)AppProtocolChangePageDatagram.AimPage]);
                    break;

                case AppProtocolStatus.EndPipeConnection:
                default:
                    break;
            }

            if (keyStatus != AppProtocolStatus.URRealTimeData)
                Logger.HistoryPrinting(Logger.Level.INFO, MethodBase.GetCurrentMethod().DeclaringType.FullName, "Recieve msg \"" + Enum.GetName(keyStatus.GetType(), keyStatus) + "\".");
        }

        /// <summary>
        /// 获得实时UR数据
        /// </summary>
        /// <param name="inputBytes">字节流</param>
        /// <returns>返回数据数组</returns>
        double[] GetURRealTimeDatasFromBytes(byte[] inputBytes)
        {
            List<double> returnDoubleArray = new List<double>(26);

            for (int i = 0; i < 6; ++i)
            {
                returnDoubleArray.Add(
                    Convert.ToDouble(
                    BitConverter.ToSingle(
                    BitConverter.GetBytes(
                    IPAddress.NetworkToHostOrder(
                    BitConverter.ToInt32(inputBytes, (byte)AppProtocolRTFeedBackDatagram.TcpCoordinates + i * 4))), 0)));
            }
            for (int i = 0; i < 6; ++i)
            {
                returnDoubleArray.Add(
                    Convert.ToDouble(
                    BitConverter.ToSingle(
                    BitConverter.GetBytes(
                    IPAddress.NetworkToHostOrder(
                    BitConverter.ToInt32(inputBytes, (byte)AppProtocolRTFeedBackDatagram.JointCoordinates + i * 4))), 0)));
            }
            for (int i = 0; i < 6; ++i)
            {
                returnDoubleArray.Add(
                    Convert.ToDouble(
                    BitConverter.ToSingle(
                    BitConverter.GetBytes(
                    IPAddress.NetworkToHostOrder(
                    BitConverter.ToInt32(inputBytes, (byte)AppProtocolRTFeedBackDatagram.CurrentCoordinates + i * 4))), 0)));
            }
            for (int i = 0; i < 6; ++i)
            {
                returnDoubleArray.Add(
                    Convert.ToDouble(
                    BitConverter.ToSingle(
                    BitConverter.GetBytes(
                    IPAddress.NetworkToHostOrder(
                    BitConverter.ToInt32(inputBytes, (byte)AppProtocolRTFeedBackDatagram.TcpForce + i * 4))), 0)));
            }

            returnDoubleArray.Add(
                Convert.ToDouble(inputBytes[(byte)AppProtocolRTFeedBackDatagram.RobotState]));
            returnDoubleArray.Add(
                Convert.ToDouble(inputBytes[(byte)AppProtocolRTFeedBackDatagram.RobotProgramState]));

            return returnDoubleArray.ToArray();
        }

        /// <summary>
        /// 获得乳腺扫描配置
        /// </summary>
        /// <param name="inputBytes">字节流</param>
        /// <returns>返回数据列表</returns>
        List<string[]> GetBreastScanConfFromBytes(byte[] inputBytes)
        {
            List<string[]> returnArray = new List<string[]>(24);

            returnArray.Add(new string[] {
                Convert.ToDouble(
                BitConverter.ToSingle(
                BitConverter.GetBytes(
                IPAddress.NetworkToHostOrder(
                BitConverter.ToInt32(inputBytes, (byte)AppProtocolBreastScanConfigurationDatagram.DetectingErrorForceMinGDR))), 0)).ToString()
            });
            returnArray.Add(new string[] {
                Convert.ToDouble(
                BitConverter.ToSingle(
                BitConverter.GetBytes(
                IPAddress.NetworkToHostOrder(
                BitConverter.ToInt32(inputBytes, (byte)AppProtocolBreastScanConfigurationDatagram.DetectingErrorForceMaxGDR))), 0)).ToString()
            });
            returnArray.Add(new string[] {
                Convert.ToDouble(
                BitConverter.ToSingle(
                BitConverter.GetBytes(
                IPAddress.NetworkToHostOrder(
                BitConverter.ToInt32(inputBytes, (byte)AppProtocolBreastScanConfigurationDatagram.DetectingSpeedMinGDR))), 0)).ToString()
            });

            returnArray.Add(new string[] {
                inputBytes[(byte)AppProtocolBreastScanConfigurationDatagram.IfEnableAngleCorrectedGDR] == 1 ? "True" : "False"
            });

            returnArray.Add(new string[] { " " }); // {vibratingAttitudeMaxAtSmoothPart}
            returnArray.Add(new string[] { " " }); // {vibratingAttitudeMinAtSteepPart}
            returnArray.Add(new string[] { " " }); // {vibratingAttitudeMaxAtSteepPart}

            returnArray.Add(new string[] {
                Convert.ToDouble(
                BitConverter.ToSingle(
                BitConverter.GetBytes(
                IPAddress.NetworkToHostOrder(
                BitConverter.ToInt32(inputBytes, (byte)AppProtocolBreastScanConfigurationDatagram.NippleForbiddenRadiusGDR))), 0)).ToString()
            });

            returnArray.Add(new string[] { " " }); // {movingStopDistance}

            returnArray.Add(new string[] {
                Convert.ToDouble(
                BitConverter.ToSingle(
                BitConverter.GetBytes(
                IPAddress.NetworkToHostOrder(
                BitConverter.ToInt32(inputBytes, (byte)AppProtocolBreastScanConfigurationDatagram.DetectingStopDistanceGDR))), 0)).ToString()
            });
            returnArray.Add(new string[] {
                Convert.ToDouble(
                BitConverter.ToSingle(
                BitConverter.GetBytes(
                IPAddress.NetworkToHostOrder(
                BitConverter.ToInt32(inputBytes, (byte)AppProtocolBreastScanConfigurationDatagram.DetectingSafetyLiftDistanceGDR))), 0)).ToString()
            });

            returnArray.Add(new string[] {
                inputBytes[(byte)AppProtocolBreastScanConfigurationDatagram.IfEnableDetectingInitialForceGDR] == 1 ? "True" : "False"
            });

            returnArray.Add(new string[] {
                Convert.ToDouble(
                BitConverter.ToSingle(
                BitConverter.GetBytes(
                IPAddress.NetworkToHostOrder(
                BitConverter.ToInt32(inputBytes, (byte)AppProtocolBreastScanConfigurationDatagram.DetectingSinkDistanceGDR))), 0)).ToString()
            });

            returnArray.Add(new string[] {
                inputBytes[(byte)AppProtocolBreastScanConfigurationDatagram.VibratingAngleDegreeGDR].ToString()
            });
            returnArray.Add(new string[] {
                inputBytes[(byte)AppProtocolBreastScanConfigurationDatagram.MovingSpeedDegreeGDR].ToString()
            });
            returnArray.Add(new string[] {
                inputBytes[(byte)AppProtocolBreastScanConfigurationDatagram.DetectingForceDegreeGDR].ToString()
            });
            returnArray.Add(new string[] {
                inputBytes[(byte)AppProtocolBreastScanConfigurationDatagram.DetectingAlignDegreeGDR].ToString()
            });

            returnArray.Add(new string[] {
                Convert.ToDouble(
                BitConverter.ToSingle(
                BitConverter.GetBytes(
                IPAddress.NetworkToHostOrder(
                BitConverter.ToInt32(inputBytes, (byte)AppProtocolBreastScanConfigurationDatagram.MovingUpEdgeDistanceGDR))), 0)).ToString()
            });
            returnArray.Add(new string[] {
                Convert.ToDouble(
                BitConverter.ToSingle(
                BitConverter.GetBytes(
                IPAddress.NetworkToHostOrder(
                BitConverter.ToInt32(inputBytes, (byte)AppProtocolBreastScanConfigurationDatagram.MovingLeftEdgeDistanceGDR))), 0)).ToString()
            });
            returnArray.Add(new string[] {
                Convert.ToDouble(
                BitConverter.ToSingle(
                BitConverter.GetBytes(
                IPAddress.NetworkToHostOrder(
                BitConverter.ToInt32(inputBytes, (byte)AppProtocolBreastScanConfigurationDatagram.MovingDownEdgeDistanceGDR))), 0)).ToString()
            });
            returnArray.Add(new string[] {
                Convert.ToDouble(
                BitConverter.ToSingle(
                BitConverter.GetBytes(
                IPAddress.NetworkToHostOrder(
                BitConverter.ToInt32(inputBytes, (byte)AppProtocolBreastScanConfigurationDatagram.MovingRightEdgeDistanceGDR))), 0)).ToString()
            });

            returnArray.Add(new string[] {
                inputBytes[(byte)AppProtocolBreastScanConfigurationDatagram.IfAutoReplaceConfigurationGDR] == 1 ? "True" : "False"
            });

            returnArray.Add(new string[] {
                inputBytes[(byte)AppProtocolBreastScanConfigurationDatagram.IfCheckRightGalactophoreGDR].ToString()
            });
            returnArray.Add(new string[] {
                inputBytes[(byte)AppProtocolBreastScanConfigurationDatagram.IdentifyEdgeModeGDR].ToString()
            });

            returnArray.Add(new string[] {
                Convert.ToDouble(
                BitConverter.ToSingle(
                BitConverter.GetBytes(
                IPAddress.NetworkToHostOrder(
                BitConverter.ToInt32(inputBytes, (byte)AppProtocolBreastScanConfigurationDatagram.CheckingStepGDR))), 0)).ToString()
            });

            return returnArray;
        }
        #endregion

        #region (Unp|P)ack & (De|En)crypt
        /// <summary>
        /// TCP层解包
        /// </summary>
        /// <param name="packagedData">待解包数据</param>
        /// <param name="unpackageSuccess">解包是否成功</param>
        /// <returns>返回解包内容</returns>
        private byte[] UnpackageTCP(byte[] packagedData, out bool unpackageSuccess)
        {
            int dataLength = 0;
            if (packagedData.Length > 8)
            {
                dataLength = Convert.ToInt32(IPAddress.NetworkToHostOrder(
                             BitConverter.ToInt32(packagedData, (byte)TCPProtocol.DataLength)));
                if (dataLength != packagedData.Length - 8)
                {
                    unpackageSuccess = false;
                    return null; // 数据段长度不匹配
                }
            }
            else
            {
                if (packagedData.Length != 4)
                {
                    unpackageSuccess = false;
                    return null;  // 总长度不匹配
                }
            }

            if (packagedData[(byte)TCPProtocol.Header1] != header1 ||
                packagedData[(byte)TCPProtocol.Header2] != header2)
            {
                unpackageSuccess = false;
                return null; // 协议头不匹配
            }

            byte deviceIndex = packagedData[(byte)TCPProtocol.DeviceID]; // 设备号
            if (clientDeviceIndex != deviceIndex)
            {
                unpackageSuccess = false;
                return null; // 设备号不匹配
            }

            TCPProtocolKey protocolKey = (TCPProtocolKey)packagedData[(byte)TCPProtocol.ProtocolKey]; // 标志位

            if (Object.Equals(commonKey, null) && !protocolKey.Equals(TCPProtocolKey.AESCommonKeyAndIV))
            {
                unpackageSuccess = false;
                return null; // 尚未获得AES密钥 不允许接收AES密钥以外的消息
            }

            switch (protocolKey)
            {
                case TCPProtocolKey.AESCommonKeyAndIV:
                    byte[] aesKeys = DecryptByRSA(packagedData.Skip((byte)TCPProtocol.DataContent).ToArray());
                    if (Object.Equals(aesKeys, null))
                    {
                        unpackageSuccess = false;
                        return null; // RSA解密失败
                    }

                    if (aesKeys.Length != (int)SecurityKeyLength.AESIVLength + (int)SecurityKeyLength.AESKeyLength)
                    {
                        unpackageSuccess = false;
                        return null; // AES密钥长度不匹配
                    }

                    commonIV = aesKeys.Take((byte)SecurityKeyLength.AESIVLength).ToArray();
                    commonKey = aesKeys.Skip((byte)SecurityKeyLength.AESIVLength).ToArray();

                    Logger.HistoryPrinting(Logger.Level.INFO, MethodBase.GetCurrentMethod().DeclaringType.FullName, "AESKey saved.");
                    break;
                case TCPProtocolKey.NormalData:
                    // 返回数据部分
                    //Logger.HistoryPrinting(Logger.Level.INFO, MethodBase.GetCurrentMethod().DeclaringType.FullName, "Get data part for TCP.");
                    unpackageSuccess = true;
                    return packagedData.Skip((byte)TCPProtocol.DataContent).ToArray();
                case TCPProtocolKey.PingSignal:
                case TCPProtocolKey.EndRemoteControl:
                case TCPProtocolKey.EndBothControl:
                case TCPProtocolKey.RSAPublicKey:
                    Logger.HistoryPrinting(Logger.Level.INFO, MethodBase.GetCurrentMethod().DeclaringType.FullName, "Should not appear this command.");
                    break;
                default:
                    Logger.HistoryPrinting(Logger.Level.WARN, MethodBase.GetCurrentMethod().DeclaringType.FullName, "No such control command.");
                    break;
            }

            unpackageSuccess = true;
            return null;
        }

        /// <summary>
        /// TCP层打包
        /// </summary>
        /// <param name="functionalNum">功能码</param>
        /// <param name="unPackagedData">待打包数据</param>
        /// <returns>返回打包内容</returns>
        private byte[] PackageTCP(TCPProtocolKey functionalNum, byte[] unPackagedData = null)
        {
            List<byte> packagedBytes = new List<byte>(8 +
                (Object.Equals(unPackagedData, null) ? 0 : unPackagedData.Length));
            packagedBytes.Add(header1);
            packagedBytes.Add(header2);
            packagedBytes.Add(clientDeviceIndex);
            packagedBytes.Add((byte)functionalNum);

            if (!Object.Equals(unPackagedData, null))
            {
                packagedBytes.AddRange(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(unPackagedData.Length)));
                packagedBytes.AddRange(unPackagedData);
            }

            return packagedBytes.ToArray();
        }

        /// <summary>
        /// Pipe层解包
        /// </summary>
        /// <param name="packagedData">待解包数据</param>
        /// <param name="key">数据关键字</param>
        /// <param name="unpackageSuccess">解包是否成功</param>
        /// <returns>返回解包内容</returns>
        private byte[] UnpackagePipe(byte[] packagedData, out byte key, out bool unpackageSuccess)
        {
            int dataLength = 0;
            if (packagedData.Length > 6)
            {
                dataLength = Convert.ToInt32(IPAddress.NetworkToHostOrder(
                             BitConverter.ToInt32(packagedData, (byte)AppProtocol.DataLength)));
                if (dataLength != packagedData.Length - 4)
                {
                    unpackageSuccess = false;
                    key = 0;
                    return null; // 数据段长度不匹配
                }
            }
            else
            {
                if (packagedData.Length != 6)
                {
                    unpackageSuccess = false;
                    key = 0;
                    return null;  // 总长度不匹配
                }
            }

            if ((AppProtocolDireciton)packagedData[(byte)AppProtocol.DataFlow] != AppProtocolDireciton.Local2Remote)
            {
                unpackageSuccess = false;
                key = 0;
                return null;  // 数据流向不匹配
            }

            unpackageSuccess = true;
            key = packagedData[(byte)AppProtocol.DataKey];
            return packagedData.Skip((byte)AppProtocol.DataContent).ToArray();
        }

        /// <summary>
        /// Pipe层打包
        /// </summary>
        /// <param name="functionalNum">功能码</param>
        /// <param name="unPackagedData">待打包数据</param>
        /// <returns>返回打包内容</returns>
        private byte[] PackagePipe(AppProtocolCommand functionalNum, byte[] unPackagedData = null)
        {
            int len = 6;
            if (!Object.Equals(unPackagedData, null)) len += unPackagedData.Length;
            List<byte> packagedBytes = new List<byte>(len);
            if (!Object.Equals(unPackagedData, null)) packagedBytes.AddRange(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(unPackagedData.Length + 2)));
            else packagedBytes.AddRange(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(2)));

            packagedBytes.Add((byte)AppProtocolDireciton.Remote2Local);
            packagedBytes.Add((byte)functionalNum);

            if (!Object.Equals(unPackagedData, null)) packagedBytes.AddRange(unPackagedData);

            return packagedBytes.ToArray();
        }

        /// <summary>
        /// RSA密钥解密数据
        /// </summary>
        /// <param name="nonDecryptedBytes">待解密字节流</param>
        /// <returns>解密后的字节流</returns>
        private byte[] DecryptByRSA(byte[] nonDecryptedBytes)
        {
            if (Object.Equals(nonDecryptedBytes, null) || nonDecryptedBytes.Length < 1)
            {
                Logger.HistoryPrinting(Logger.Level.WARN, MethodBase.GetCurrentMethod().DeclaringType.FullName, "Datas for decrypting by RSA is abnormal.");
                return null; // 待解密数据异常
            }
            if (Object.Equals(publicKey, null) || Object.Equals(privateKey, null))
            {
                Logger.HistoryPrinting(Logger.Level.WARN, MethodBase.GetCurrentMethod().DeclaringType.FullName, "RSA keys have not been known yet.");
                return null; // RSA公密钥未知
            }

            byte[] decryptedBytes = null;
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
            {
                rsa.FromXmlString(privateKey);
                if (nonDecryptedBytes.Length > ((int)SecurityKeyLength.RSAKeyLength) / 8) return null; // 待解密数据过长

                decryptedBytes = rsa.Decrypt(nonDecryptedBytes, false);
            }
            return decryptedBytes;
        }

        /// <summary>
        /// AES加密数据
        /// </summary>
        /// <param name="nonEncryptedBytes">待加密字节流</param>
        /// <returns>加密后的字节流</returns>
        private byte[] EncryptByAES(byte[] nonEncryptedBytes)
        {
            if (Object.Equals(nonEncryptedBytes, null) || nonEncryptedBytes.Length < 1)
            {
                Logger.HistoryPrinting(Logger.Level.WARN, MethodBase.GetCurrentMethod().DeclaringType.FullName, "Datas for encrypting by AES is abnormal.");
                return null; // 待加密数据异常
            }
            if (Object.Equals(commonIV, null) ||
                Object.Equals(commonKey, null))
            {
                Logger.HistoryPrinting(Logger.Level.WARN, MethodBase.GetCurrentMethod().DeclaringType.FullName, "AES key has not been known yet.");
                return null; // AES密钥和初始向量未知
            }

            string nonEncryptedString = Convert.ToBase64String(nonEncryptedBytes);

            byte[] encryptedBytes = null;
            using (AesCryptoServiceProvider aes = new AesCryptoServiceProvider())
            {
                aes.Key = commonKey; aes.IV = commonIV;
                ICryptoTransform encryptorByAES = aes.CreateEncryptor();

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptorByAES, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(nonEncryptedString);
                        }
                        encryptedBytes = msEncrypt.ToArray();
                    }
                }
            }
            return encryptedBytes;
        }

        /// <summary>
        /// AES解密数据
        /// </summary>
        /// <param name="encryptedBytes">待解密字节流</param>
        /// <returns>解密后的字节流</returns>
        private byte[] DecryptByAES(byte[] encryptedBytes)
        {
            if (Object.Equals(encryptedBytes, null) || encryptedBytes.Length < 1)
            {
                Logger.HistoryPrinting(Logger.Level.WARN, MethodBase.GetCurrentMethod().DeclaringType.FullName, "Datas for decrypting by AES is abnormal.");
                return null; // 待解密数据异常
            }
            if (Object.Equals(commonIV, null) ||
                Object.Equals(commonKey, null))
            {
                Logger.HistoryPrinting(Logger.Level.WARN, MethodBase.GetCurrentMethod().DeclaringType.FullName, "AES key has not been known yet.");
                return null; // AES密钥和初始向量未知
            }

            byte[] decryptedBytes = null;
            using (AesCryptoServiceProvider aes = new AesCryptoServiceProvider())
            {
                aes.Key = commonKey; aes.IV = commonIV;
                ICryptoTransform decryptorByAES = aes.CreateDecryptor();

                using (MemoryStream msDecrypt = new MemoryStream(encryptedBytes))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptorByAES, CryptoStreamMode.Read))
                    {
                        using (StreamReader swDecrypt = new StreamReader(csDecrypt))
                        {
                            string decryptedString = swDecrypt.ReadToEnd();
                            decryptedBytes = Convert.FromBase64String(decryptedString);
                        }
                    }
                }
            }
            return decryptedBytes;
        }
        #endregion

        #region Abort
        /// <summary>
        /// 结束所有循环等待
        /// </summary>
        /// <param name="noError">没有错误</param>
        private void EndAllLoop(bool noError = true)
        {
            tcpSendCancel.Cancel();
            tcpRecieveCancel.Cancel();
            tcpBeatClocker.Stop();

            if (!noError) existError = true;
        }

        /// <summary>
        /// 结束所有连接
        /// </summary>
        /// <returns>返回是否有未知错误</returns>
        private bool FinishAllConnection()
        {
            tcpTransferSendSocket.Shutdown(SocketShutdown.Both);
            tcpTransferSendSocket.Close();

            tcpTransferRecieveSocket.Shutdown(SocketShutdown.Both);
            tcpTransferRecieveSocket.Close();

            // 未知网络传输错误
            return existError;
        }
        #endregion
    }
}
