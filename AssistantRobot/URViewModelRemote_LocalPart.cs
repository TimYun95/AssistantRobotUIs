using System;
using System.Collections.Generic;
using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
using System.Net;
using System.Reflection;

using LogPrinter;
using PipeCommunication;


namespace AssistantRobot
{
    public class URViewModelRemote_LocalPart
    {
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

            BreastScanConfiguration = 101,
            BreastScanWorkStatus = 102,
            BreastScanConfigurationConfirmStatus = 103,
            BreastScanForceZerodStatus = 104,
            BreastScanConfigurationProcess = 151,
            BreastScanImmediateStop = 201,
            BreastScanImmediateStopRecovery = 202,

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

        #region 字段
        private readonly URViewModel urvm;
        private PipeConnector ppc = new PipeConnector();

        private const double maxSpeedRatio = 50.0; // 最大速度比例
        private bool ifPipeConnected = false;
        #endregion

        #region 方法
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="URVM">URViewModel</param>
        public URViewModelRemote_LocalPart(URViewModel URVM)
        {
            urvm = URVM;

            ppc.OnSendByteRecieved += new PipeConnector.SendByteArray(DealWithRecievedBytes);
            ppc.OnSendPipeCrashed += new PipeConnector.SendVoid(GetPipeCrashed);
        }

        /// <summary>
        /// Pipe连接
        /// </summary>
        /// <returns>返回连接结果</returns>
        public bool PipeBeginToConnect()
        {
            return ifPipeConnected = ppc.PipeConnect();
        }

        /// <summary>
        /// 接收字节流处理
        /// </summary>
        /// <param name="getBytes">收到的字节流</param>
        protected void DealWithRecievedBytes(byte[] getBytes)
        {
            if (getBytes.Length < 6) return; // 字节流长度太短

            int theoryDataLength = Convert.ToInt32(
                IPAddress.NetworkToHostOrder(
                BitConverter.ToInt32(getBytes, (int)AppProtocol.DataLength)));

            if (theoryDataLength != getBytes.Length - 4) return; // 长度不匹配 直接返回

            if ((AppProtocolDireciton)getBytes[(byte)AppProtocol.DataFlow] != AppProtocolDireciton.Remote2Local) return; // 非远端传输 直接返回

            AppProtocolCommand getKey = (AppProtocolCommand)getBytes[(byte)AppProtocol.DataKey];
            switch (getKey)
            {
                case AppProtocolCommand.MoveTcp:
                    {
                        bool ifPos = getBytes[(byte)AppProtocol.DataContent + (byte)AppProtocolMoveTcpDatagram.MoveDirection] == 1;
                        bool ifSpin = getBytes[(byte)AppProtocol.DataContent + (byte)AppProtocolMoveTcpDatagram.MovePattern] == 1;
                        char axisMotion = 'x';
                        switch (getBytes[(byte)AppProtocol.DataContent + (byte)AppProtocolMoveTcpDatagram.MoveAxis])
                        {
                            case 2: axisMotion = 'z'; break;
                            case 1: axisMotion = 'y'; break;
                            case 0:
                            default: axisMotion = 'x'; break;
                        }

                        // 远端速度降低为50%
                        if (urvm.BaseMoveSpeedRatio > maxSpeedRatio) urvm.BaseMoveSpeedRatio = maxSpeedRatio;

                        if (ifSpin) urvm.BaseMovingSpinBegin(axisMotion, ifPos);
                        else urvm.BaseMovingTranslationBegin(axisMotion, ifPos);
                    }
                    break;
                case AppProtocolCommand.MoveJoint:
                    {
                        bool ifPos = getBytes[(byte)AppProtocol.DataContent + (byte)AppProtocolMoveJointDatagram.MoveDirection] == 1;
                        char axisSpin = '1';
                        switch (getBytes[(byte)AppProtocol.DataContent + (byte)AppProtocolMoveJointDatagram.MoveAxis])
                        {
                            case 6: axisSpin = '6'; break;
                            case 5: axisSpin = '5'; break;
                            case 4: axisSpin = '4'; break;
                            case 3: axisSpin = '3'; break;
                            case 2: axisSpin = '2'; break;
                            case 1:
                            default: axisSpin = '1'; break;
                        }

                        // 远端速度降低为50%
                        if (urvm.BaseMoveSpeedRatio > maxSpeedRatio) urvm.BaseMoveSpeedRatio = maxSpeedRatio;

                        urvm.BaseMovingSingleSpinBegin(axisSpin, ifPos);
                    }
                    break;
                case AppProtocolCommand.MoveStop:
                    urvm.BaseMovingEnd();
                    break;
                case AppProtocolCommand.MoveReference:
                    urvm.BaseMoveCordinate = getBytes[(byte)AppProtocol.DataContent + (byte)AppProtocolMoveReferenceDatagram.ReferToBase] == 1;
                    break;
                case AppProtocolCommand.MoveSpeed:
                    urvm.BaseMoveSpeedRatio = (double)getBytes[(byte)AppProtocol.DataContent + (byte)AppProtocolMoveSpeedDatagram.SpeedRatio];
                    break;

                case AppProtocolCommand.PowerOn:
                    urvm.RobotPowerOn();
                    break;
                case AppProtocolCommand.BrakeRelease:
                    urvm.BrakeLess();
                    break;
                case AppProtocolCommand.PowerOff:
                    urvm.RobotPowerOff();
                    break;
                case AppProtocolCommand.AutoPowerOn:
                    urvm.ChooseFirstNetConnection(getBytes[(byte)AppProtocol.DataContent + (byte)AppProtocolAutoPowerOnDatagram.WhetherAutoPowerOn] == 1);
                    break;

                case AppProtocolCommand.ChangePage:
                    urvm.NavigateToPage((URViewModel.ShowPage)getBytes[(byte)AppProtocol.DataContent + (byte)AppProtocolChangePageDatagram.AimPage]);
                    break;

                case AppProtocolCommand.EnterBreastScanMode:
                    urvm.EnterGalactophoreDetectModule();
                    break;
                case AppProtocolCommand.BreastScanModeBeginForceZeroed:
                    urvm.ForceClearGalactophoreDetectModule();
                    break;
                case AppProtocolCommand.BreastScanModeBeginConfigurationSet:
                    urvm.ConfParamsGalactophoreDetectModule();
                    break;
                case AppProtocolCommand.BreastScanModeConfirmNipplePos:
                    urvm.NippleFoundGalactophoreDetectModule();
                    break;
                case AppProtocolCommand.BreastScanModeNextConfigurationItem:
                    urvm.ConfParamsNextParamsGalactophoreDetectModule();
                    break;
                case AppProtocolCommand.BreastScanModeConfirmConfigurationSet:
                    urvm.ConfirmConfParamsGalactophoreDetectModule(UnpackConfigurationParameters(getBytes.Skip((byte)AppProtocol.DataContent).ToArray()));
                    break;
                case AppProtocolCommand.BreastScanModeReadyAndStartBreastScan:
                    urvm.ReadyAndStartGalactophoreDetectModule();
                    break;
                case AppProtocolCommand.BreastScanModeSaveConfigurationSet:
                    urvm.SaveConfParameters(URViewModel.ConfPage.GalactophoreDetect, UnpackConfigurationParameters(getBytes.Skip((byte)AppProtocol.DataContent).ToArray()));
                    break;
                case AppProtocolCommand.StopBreastScanImmediately:
                    urvm.StopMotionNowGalactophoreDetectModule(500);
                    break;
                case AppProtocolCommand.ExitBreastScanMode:
                    urvm.ExitGalactophoreDetectModule();
                    break;
                default:
                    Logger.HistoryPrinting(Logger.Level.WARN, MethodBase.GetCurrentMethod().DeclaringType.FullName, "No such command, command number: +" + ((byte)getKey).ToString() + ".");
                    break;
            }

            //Logger.HistoryPrinting(Logger.Level.INFO, MethodBase.GetCurrentMethod().DeclaringType.FullName, "Recieve cmd \"" + Enum.GetName(getKey.GetType(), getKey) + "\".");
        }

        /// <summary>
        /// 管道已经断开
        /// </summary>
        protected void GetPipeCrashed()
        {
            urvm.IfRecievedPipeCrashed = true;
            urvm.DirectCloseModelLogic();
        }

        /// <summary>
        /// 管道发送数据流
        /// </summary>
        /// <param name="statusFlag">状态标志位</param>
        /// <param name="sendBytes">发送数据</param>
        public void SendPipeDataStream(AppProtocolStatus statusFlag, List<byte> sendBytes = null)
        {
            if (!ifPipeConnected) return;

            if (Object.Equals(sendBytes, null)) sendBytes = new List<byte>();

            int sendLength = sendBytes.Count + 2;
            sendBytes.Insert(0, (byte)statusFlag);
            sendBytes.Insert(0, (byte)AppProtocolDireciton.Local2Remote);
            sendBytes.InsertRange(0, BitConverter.GetBytes(IPAddress.HostToNetworkOrder(sendLength)));

            ppc.SendBytes(sendBytes.ToArray());

            if (statusFlag == AppProtocolStatus.URRealTimeData)
                Logger.HistoryPrinting(Logger.Level.INFO, MethodBase.GetCurrentMethod().DeclaringType.FullName, "Send status \"" + Enum.GetName(statusFlag.GetType(), statusFlag) + "\".");
        }

        /// <summary>
        /// 解包配置参数
        /// </summary>
        /// <param name="bufferBytes">待解包配置参数</param>
        /// <returns>返回解包后的结果</returns>
        protected List<string> UnpackConfigurationParameters(byte[] bufferBytes)
        {
            List<string> returnedString = new List<string>(25);

            returnedString.Add(BitConverter.ToSingle(
                BitConverter.GetBytes(
                IPAddress.NetworkToHostOrder(
                BitConverter.ToInt32(bufferBytes,
                                                    (byte)AppProtocolBreastScanConfigurationDatagram.DetectingErrorForceMinGDR))), 0).ToString());
            returnedString.Add(BitConverter.ToSingle(
                BitConverter.GetBytes(
                IPAddress.NetworkToHostOrder(
                BitConverter.ToInt32(bufferBytes,
                                                    (byte)AppProtocolBreastScanConfigurationDatagram.DetectingErrorForceMaxGDR))), 0).ToString());
            returnedString.Add(BitConverter.ToSingle(
                            BitConverter.GetBytes(
                            IPAddress.NetworkToHostOrder(
                            BitConverter.ToInt32(bufferBytes,
                                                                (byte)AppProtocolBreastScanConfigurationDatagram.DetectingSpeedMinGDR))), 0).ToString());

            returnedString.Add((bufferBytes[(byte)AppProtocolBreastScanConfigurationDatagram.IfEnableAngleCorrectedGDR] == 1).ToString());

            returnedString.Add("-2.0");
            returnedString.Add("-2.0");
            returnedString.Add("-2.0");

            returnedString.Add(BitConverter.ToSingle(
                            BitConverter.GetBytes(
                            IPAddress.NetworkToHostOrder(
                            BitConverter.ToInt32(bufferBytes,
                                                                (byte)AppProtocolBreastScanConfigurationDatagram.NippleForbiddenRadiusGDR))), 0).ToString());

            returnedString.Add("-2.0");

            returnedString.Add(BitConverter.ToSingle(
                           BitConverter.GetBytes(
                           IPAddress.NetworkToHostOrder(
                           BitConverter.ToInt32(bufferBytes,
                                                               (byte)AppProtocolBreastScanConfigurationDatagram.DetectingStopDistanceGDR))), 0).ToString());
            returnedString.Add(BitConverter.ToSingle(
                           BitConverter.GetBytes(
                           IPAddress.NetworkToHostOrder(
                           BitConverter.ToInt32(bufferBytes,
                                                               (byte)AppProtocolBreastScanConfigurationDatagram.DetectingSafetyLiftDistanceGDR))), 0).ToString());

            returnedString.Add((bufferBytes[(byte)AppProtocolBreastScanConfigurationDatagram.IfEnableDetectingInitialForceGDR] == 1).ToString());

            returnedString.Add("-2.0"); // 实际占位的

            returnedString.Add(bufferBytes[(byte)AppProtocolBreastScanConfigurationDatagram.VibratingAngleDegreeGDR].ToString());
            returnedString.Add(bufferBytes[(byte)AppProtocolBreastScanConfigurationDatagram.MovingSpeedDegreeGDR].ToString());
            returnedString.Add(bufferBytes[(byte)AppProtocolBreastScanConfigurationDatagram.DetectingForceDegreeGDR].ToString());
            returnedString.Add(bufferBytes[(byte)AppProtocolBreastScanConfigurationDatagram.DetectingAlignDegreeGDR].ToString());

            returnedString.Add(BitConverter.ToSingle(
                           BitConverter.GetBytes(
                           IPAddress.NetworkToHostOrder(
                           BitConverter.ToInt32(bufferBytes,
                                                               (byte)AppProtocolBreastScanConfigurationDatagram.MovingUpEdgeDistanceGDR))), 0).ToString());
            returnedString.Add(BitConverter.ToSingle(
                                       BitConverter.GetBytes(
                                       IPAddress.NetworkToHostOrder(
                                       BitConverter.ToInt32(bufferBytes,
                                                                           (byte)AppProtocolBreastScanConfigurationDatagram.MovingLeftEdgeDistanceGDR))), 0).ToString());
            returnedString.Add(BitConverter.ToSingle(
                                       BitConverter.GetBytes(
                                       IPAddress.NetworkToHostOrder(
                                       BitConverter.ToInt32(bufferBytes,
                                                                           (byte)AppProtocolBreastScanConfigurationDatagram.MovingDownEdgeDistanceGDR))), 0).ToString());
            returnedString.Add(BitConverter.ToSingle(
                                       BitConverter.GetBytes(
                                       IPAddress.NetworkToHostOrder(
                                       BitConverter.ToInt32(bufferBytes,
                                                                           (byte)AppProtocolBreastScanConfigurationDatagram.MovingRightEdgeDistanceGDR))), 0).ToString());

            returnedString.Add((bufferBytes[(byte)AppProtocolBreastScanConfigurationDatagram.IfAutoReplaceConfigurationGDR] == 1).ToString());

            returnedString.Add(bufferBytes[(byte)AppProtocolBreastScanConfigurationDatagram.IfCheckRightGalactophoreGDR].ToString());
            returnedString.Add(bufferBytes[(byte)AppProtocolBreastScanConfigurationDatagram.IdentifyEdgeModeGDR].ToString());

            returnedString.Add(BitConverter.ToSingle(
                                       BitConverter.GetBytes(
                                       IPAddress.NetworkToHostOrder(
                                       BitConverter.ToInt32(bufferBytes,
                                                                           (byte)AppProtocolBreastScanConfigurationDatagram.CheckingStepGDR))), 0).ToString());

            return returnedString;
        }

        #endregion

    }
}
