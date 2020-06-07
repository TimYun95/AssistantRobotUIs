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

            BreastScanNipplePos = 100,
            BreastScanConfiguration = 101,
            BreastScanWorkStatus = 102,
            BreastScanConfigurationConfirmStatus = 103,
            BreastScanForceZerodStatus = 104,
            BreastScanConfigurationProcess = 111,
            BreastScanImmediateStop = 121,
            BreastScanImmediateStopRecovery = 122,

            RemoteScanStartPos = 150,
            RemoteScanConfiguration = 151,
            RemoteScanWorkStatus = 152,
            RemoteScanConfigurationConfirmStatus = 153,
            RemoteScanForceZerodStatus = 154,
            RemoteScanConfigurationProcess = 161,
            RemoteScanImmediateStop = 171,
            RemoteScanImmediateStopRecovery = 172,

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
        /// 应用协议状态 远程扫描配置数据报格式
        /// </summary>
        public enum AppProtocolRemoteScanConfigurationDatagram : byte
        {
            DetectingErrorForceMinTSR = 0,
            DetectingErrorForceMaxTSR = 4,
            DetectingSpeedMinTSR = 8,
            DetectingSpeedMaxTSR = 12,
            IfEnableForceKeepingTSR = 16,
            IfEnableForceTrackingTSR = 17,
            DetectingBasicPreservedForceTSR = 18,
            MaxAvailableRadiusTSR = 22,
            MaxAvailableAngleTSR = 26,
            StopRadiusTSR = 30,
            MaxDistPeriodTSR = 34,
            MaxAnglePeriodTSR = 38,
            PositionOverrideTSR = 42,
            AngleOverrideTSR = 46,
            ForceOverrideTSR = 50,
            IfEnableAttitudeTrackingTSR = 54,
            IfEnableTranslationTrackingTSR = 55
        }

        /// <summary>
        /// 应用协议状态 远程扫描工作状态数据报格式
        /// </summary>
        public enum AppProtocolRemoteScanWorkStatusDatagram : byte
        {
            ModuleWorkingStatus = 0 // byte: OperateModuleBase.WorkStatus
        }

        /// <summary>
        /// 应用协议状态 远程扫描配置确认数据报格式
        /// </summary>
        public enum AppProtocolRemoteScanConfigurationConfirmDatagram : byte
        {
            HasConfirmConfiguration = 0 // byte: 0--no 1--yes
        }

        /// <summary>
        /// 应用协议状态 远程扫描力清零数据报格式
        /// </summary>
        public enum AppProtocolRemoteScanForceZerodDatagram : byte
        {
            HasForceZeroed = 0 // byte: 0--no 1--yes
        }

        /// <summary>
        /// 应用协议状态 远程扫描配置过程进度数据报格式
        /// </summary>
        public enum AppProtocolRemoteScanConfigurationProcessDatagram : byte
        {
            ConfProcess = 0 // byte: max-1--BeforeConfiguration
            // 0--Initial
            // 1--StartPos
            // 2--TranslateRatio
            // 3--PostureRatio
            // 4--PressureRatio
            // 5--PositionTrack
            // 6--PostureTrack
            // 7--PressureKeep
            // 8--PressureTrack
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
            ExitBreastScanMode = 131,

            EnterRemoteScanMode = 141,
            RemoteScanModeBeginForceZeroed = 142,
            RemoteScanModeBeginConfigurationSet = 143,
            RemoteScanModeConfirmStartPos = 144,
            RemoteScanModeNextConfigurationItem = 145,
            RemoteScanModeConfirmConfigurationSet = 146,
            RemoteScanModeReadyAndStartBreastScan = 147,
            RemoteScanModeSaveConfigurationSet = 148,

            StopRemoteScanImmediately = 151,
            RecoveryFromStopRemoteScanImmediately = 152,
            ExitRemoteScanMode = 161,

            AdjustPartRemoteScanConfigurationSet = 171,
            RefreshRemoteScanAimPos = 172,

            NotifyRemoteConnected = 251
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
        /// 应用协议指令 远程扫描部分配置参数数据报格式
        /// </summary>
        public enum AppProtocolRemoteScanAimPosDatagram : byte
        {
            SignalNum = 0,
            AimPosX = 4,
            AimPosY = 12,
            AimPosZ = 20,
            AimAttX = 28,
            AimAttY = 36,
            AimAttZ = 44
        }

        /// <summary>
        /// 应用协议指令 远程扫描目标位置数据报格式
        /// </summary>
        public enum AppProtocolAdjustPartRemoteScanConfigurationSetDatagram : byte
        {
            PosRatio = 0,
            AttRatio = 4,
            FosRatio = 8,
            PosSwitch = 12,
            AttSwitch = 13,
            FosKeepSwitch = 14,
            FosTrackSwitch = 15
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
                    urvm.BaseMoveSpeedRatio = Convert.ToDouble(
                                                                        BitConverter.ToSingle(
                                                                        BitConverter.GetBytes(
                                                                        IPAddress.NetworkToHostOrder(
                                                                        BitConverter.ToInt32(getBytes, (byte)AppProtocol.DataContent + (byte)AppProtocolMoveSpeedDatagram.SpeedRatio))), 0));
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
                    urvm.NavigateToPageRemote((URViewModel.ShowPage)getBytes[(byte)AppProtocol.DataContent + (byte)AppProtocolChangePageDatagram.AimPage]);
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
                    urvm.NippleFoundGalactophoreDetectModule(true);
                    break;
                case AppProtocolCommand.BreastScanModeNextConfigurationItem:
                    urvm.ConfParamsNextParamsGalactophoreDetectModule(true);
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
                    urvm.StopMotionNowGalactophoreDetectModule(true);
                    break;
                case AppProtocolCommand.RecoveryFromStopBreastScanImmediately:
                    urvm.RemoteRecoveryFromStopImmediatelyGalactophoreDetectModule();
                    break;
                case AppProtocolCommand.ExitBreastScanMode:
                    urvm.ExitGalactophoreDetectModule();
                    break;

                case AppProtocolCommand.EnterRemoteScanMode:
                    urvm.EnterThyroidScanningModule();
                    break;
                case AppProtocolCommand.RemoteScanModeBeginForceZeroed:
                    urvm.ForceClearThyroidScanningModule();
                    break;
                case AppProtocolCommand.RemoteScanModeBeginConfigurationSet:
                    urvm.ConfParamsThyroidScanningModule();
                    break;
                case AppProtocolCommand.RemoteScanModeConfirmStartPos:
                    urvm.StartPositionFoundThyroidScanningModule(true);
                    break;
                case AppProtocolCommand.RemoteScanModeNextConfigurationItem:
                    urvm.ConfParamsNextParamsThyroidScannerModule(true);
                    break;
                case AppProtocolCommand.RemoteScanModeConfirmConfigurationSet:
                    urvm.ConfirmConfParamsThyroidScanModule(UnpackConfigurationParameters(getBytes.Skip((byte)AppProtocol.DataContent).ToArray(), URViewModel.ConfPage.ThyroidScan));
                    break;
                case AppProtocolCommand.RemoteScanModeReadyAndStartBreastScan:
                    urvm.ReadyAndStartThyroidScanningModule();
                    break;
                case AppProtocolCommand.RemoteScanModeSaveConfigurationSet:
                    urvm.SaveConfParameters(URViewModel.ConfPage.ThyroidScan, UnpackConfigurationParameters(getBytes.Skip((byte)AppProtocol.DataContent).ToArray(), URViewModel.ConfPage.ThyroidScan));
                    break;
                case AppProtocolCommand.StopRemoteScanImmediately:
                    urvm.StopMotionNowThyroidScanModule(true);
                    break;
                case AppProtocolCommand.RecoveryFromStopRemoteScanImmediately:
                    urvm.RemoteRecoveryFromStopImmediatelyThyroidScanModule();
                    break;
                case AppProtocolCommand.ExitRemoteScanMode:
                    urvm.ExitThyroidScanningModule();
                    break;
                case AppProtocolCommand.AdjustPartRemoteScanConfigurationSet:
                    urvm.TransferPartConfiguration(UnpackPartRemoteScanConfigurationParameters(getBytes.Skip((byte)AppProtocol.DataContent).ToArray()));
                    break;
                case AppProtocolCommand.RefreshRemoteScanAimPos:
                    urvm.RefreshAimPosFromRemote(UnpackPartRemoteScanAimPosition(getBytes.Skip((byte)AppProtocol.DataContent).ToArray()));
                    break;

                case AppProtocolCommand.NotifyRemoteConnected:
                    urvm.NotifyRemoteConnectedNow();
                    break;
                default:
                    Logger.HistoryPrinting(Logger.Level.WARN, MethodBase.GetCurrentMethod().DeclaringType.FullName, "No such command, command number: +" + ((byte)getKey).ToString() + ".");
                    break;
            }

            Logger.HistoryPrinting(Logger.Level.INFO, MethodBase.GetCurrentMethod().DeclaringType.FullName, "Recieve cmd \"" + Enum.GetName(getKey.GetType(), getKey) + "\".");
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

            if (statusFlag != AppProtocolStatus.URRealTimeData)
                Logger.HistoryPrinting(Logger.Level.INFO, MethodBase.GetCurrentMethod().DeclaringType.FullName, "Data enqueue for pipe to msgserver.");
        }

        /// <summary>
        /// 解包配置参数
        /// </summary>
        /// <param name="bufferBytes">待解包配置参数</param>
        /// <returns>返回解包后的结果</returns>
        protected List<string> UnpackConfigurationParameters(byte[] bufferBytes, URViewModel.ConfPage confSource = URViewModel.ConfPage.GalactophoreDetect)
        {
            List<string> returnedString = new List<string>(25);

            switch (confSource)
            {
                case URViewModel.ConfPage.GalactophoreDetect:
                    {
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
                    }
                    break;
                case URViewModel.ConfPage.ThyroidScan:
                    {
                        returnedString.Add(BitConverter.ToSingle(
                            BitConverter.GetBytes(
                            IPAddress.NetworkToHostOrder(
                            BitConverter.ToInt32(bufferBytes,
                                                                (byte)AppProtocolRemoteScanConfigurationDatagram.DetectingErrorForceMinTSR))), 0).ToString());
                        returnedString.Add(BitConverter.ToSingle(
                            BitConverter.GetBytes(
                            IPAddress.NetworkToHostOrder(
                            BitConverter.ToInt32(bufferBytes,
                                                                (byte)AppProtocolRemoteScanConfigurationDatagram.DetectingErrorForceMaxTSR))), 0).ToString());
                        
                        returnedString.Add(BitConverter.ToSingle(
                                        BitConverter.GetBytes(
                                        IPAddress.NetworkToHostOrder(
                                        BitConverter.ToInt32(bufferBytes,
                                                                            (byte)AppProtocolRemoteScanConfigurationDatagram.DetectingSpeedMinTSR))), 0).ToString());
                        returnedString.Add(BitConverter.ToSingle(
                                        BitConverter.GetBytes(
                                        IPAddress.NetworkToHostOrder(
                                        BitConverter.ToInt32(bufferBytes,
                                                                            (byte)AppProtocolRemoteScanConfigurationDatagram.DetectingSpeedMaxTSR))), 0).ToString());

                        returnedString.Add((bufferBytes[(byte)AppProtocolRemoteScanConfigurationDatagram.IfEnableForceKeepingTSR] == 1).ToString());
                        returnedString.Add((bufferBytes[(byte)AppProtocolRemoteScanConfigurationDatagram.IfEnableForceTrackingTSR] == 1).ToString());

                        returnedString.Add(BitConverter.ToSingle(
                                        BitConverter.GetBytes(
                                        IPAddress.NetworkToHostOrder(
                                        BitConverter.ToInt32(bufferBytes,
                                                                            (byte)AppProtocolRemoteScanConfigurationDatagram.DetectingBasicPreservedForceTSR))), 0).ToString());
                       
                        returnedString.Add(BitConverter.ToSingle(
                                        BitConverter.GetBytes(
                                        IPAddress.NetworkToHostOrder(
                                        BitConverter.ToInt32(bufferBytes,
                                                                            (byte)AppProtocolRemoteScanConfigurationDatagram.MaxAvailableRadiusTSR))), 0).ToString());
                        returnedString.Add(BitConverter.ToSingle(
                                        BitConverter.GetBytes(
                                        IPAddress.NetworkToHostOrder(
                                        BitConverter.ToInt32(bufferBytes,
                                                                            (byte)AppProtocolRemoteScanConfigurationDatagram.MaxAvailableAngleTSR))), 0).ToString());

                        returnedString.Add(BitConverter.ToSingle(
                                        BitConverter.GetBytes(
                                        IPAddress.NetworkToHostOrder(
                                        BitConverter.ToInt32(bufferBytes,
                                                                            (byte)AppProtocolRemoteScanConfigurationDatagram.StopRadiusTSR))), 0).ToString());
                        returnedString.Add(BitConverter.ToSingle(
                                        BitConverter.GetBytes(
                                        IPAddress.NetworkToHostOrder(
                                        BitConverter.ToInt32(bufferBytes,
                                                                            (byte)AppProtocolRemoteScanConfigurationDatagram.MaxDistPeriodTSR))), 0).ToString());
                        returnedString.Add(BitConverter.ToSingle(
                                         BitConverter.GetBytes(
                                         IPAddress.NetworkToHostOrder(
                                         BitConverter.ToInt32(bufferBytes,
                                                                            (byte)AppProtocolRemoteScanConfigurationDatagram.MaxAnglePeriodTSR))), 0).ToString());

                        returnedString.Add(BitConverter.ToSingle(
                                       BitConverter.GetBytes(
                                       IPAddress.NetworkToHostOrder(
                                       BitConverter.ToInt32(bufferBytes,
                                                                           (byte)AppProtocolRemoteScanConfigurationDatagram.PositionOverrideTSR))), 0).ToString());
                        returnedString.Add(BitConverter.ToSingle(
                                        BitConverter.GetBytes(
                                        IPAddress.NetworkToHostOrder(
                                        BitConverter.ToInt32(bufferBytes,
                                                                            (byte)AppProtocolRemoteScanConfigurationDatagram.AngleOverrideTSR))), 0).ToString());
                        returnedString.Add(BitConverter.ToSingle(
                                         BitConverter.GetBytes(
                                         IPAddress.NetworkToHostOrder(
                                         BitConverter.ToInt32(bufferBytes,
                                                                            (byte)AppProtocolRemoteScanConfigurationDatagram.ForceOverrideTSR))), 0).ToString());

                        returnedString.Add((bufferBytes[(byte)AppProtocolRemoteScanConfigurationDatagram.IfEnableAttitudeTrackingTSR] == 1).ToString());
                        returnedString.Add((bufferBytes[(byte)AppProtocolRemoteScanConfigurationDatagram.IfEnableTranslationTrackingTSR] == 1).ToString());
                    }
                    break;
                default:
                    break;
            }


            return returnedString;
        }

        /// <summary>
        /// 解包远程扫描部分配置参数
        /// </summary>
        /// <param name="bufferBytes">待解包配置参数</param>
        /// <returns>返回解包后的结果</returns>
        protected List<string> UnpackPartRemoteScanConfigurationParameters(byte[] bufferBytes)
        {
            List<string> returnedString = new List<string>(7);

            returnedString.Add(BitConverter.ToSingle(
                                    BitConverter.GetBytes(
                                    IPAddress.NetworkToHostOrder(
                                    BitConverter.ToInt32(bufferBytes,
                                                                        (byte)AppProtocolAdjustPartRemoteScanConfigurationSetDatagram.PosRatio))), 0).ToString());
            returnedString.Add(BitConverter.ToSingle(
                                    BitConverter.GetBytes(
                                    IPAddress.NetworkToHostOrder(
                                    BitConverter.ToInt32(bufferBytes,
                                                                        (byte)AppProtocolAdjustPartRemoteScanConfigurationSetDatagram.AttRatio))), 0).ToString());
            returnedString.Add(BitConverter.ToSingle(
                                    BitConverter.GetBytes(
                                    IPAddress.NetworkToHostOrder(
                                    BitConverter.ToInt32(bufferBytes,
                                                                        (byte)AppProtocolAdjustPartRemoteScanConfigurationSetDatagram.FosRatio))), 0).ToString());

            returnedString.Add((bufferBytes[(byte)AppProtocolAdjustPartRemoteScanConfigurationSetDatagram.PosSwitch] == 1).ToString());
            returnedString.Add((bufferBytes[(byte)AppProtocolAdjustPartRemoteScanConfigurationSetDatagram.AttSwitch] == 1).ToString());
            returnedString.Add((bufferBytes[(byte)AppProtocolAdjustPartRemoteScanConfigurationSetDatagram.FosKeepSwitch] == 1).ToString());
            returnedString.Add((bufferBytes[(byte)AppProtocolAdjustPartRemoteScanConfigurationSetDatagram.FosTrackSwitch] == 1).ToString());

            return returnedString;
        }

        /// <summary>
        /// 解包远程扫描目标位置
        /// </summary>
        /// <param name="bufferBytes">待解包目标位置参数</param>
        /// <returns>返回解包后的结果</returns>
        protected List<string> UnpackPartRemoteScanAimPosition(byte[] bufferBytes)
        {
            List<string> returnedString = new List<string>(7);

            returnedString.Add(IPAddress.NetworkToHostOrder(
                                    BitConverter.ToInt32(bufferBytes,
                                                                        (byte)AppProtocolRemoteScanAimPosDatagram.SignalNum)).ToString());

            returnedString.Add(BitConverter.Int64BitsToDouble(
                                    IPAddress.NetworkToHostOrder(
                                    BitConverter.ToInt64(bufferBytes,
                                                             (byte)AppProtocolRemoteScanAimPosDatagram.AimPosX))).ToString());
            returnedString.Add(BitConverter.Int64BitsToDouble(
                         IPAddress.NetworkToHostOrder(
                         BitConverter.ToInt64(bufferBytes,
                                                             (byte)AppProtocolRemoteScanAimPosDatagram.AimPosY))).ToString());
            returnedString.Add(BitConverter.Int64BitsToDouble(
                         IPAddress.NetworkToHostOrder(
                         BitConverter.ToInt64(bufferBytes,
                                                             (byte)AppProtocolRemoteScanAimPosDatagram.AimPosZ))).ToString());
            returnedString.Add(BitConverter.Int64BitsToDouble(
                         IPAddress.NetworkToHostOrder(
                         BitConverter.ToInt64(bufferBytes,
                                                             (byte)AppProtocolRemoteScanAimPosDatagram.AimAttX))).ToString());
            returnedString.Add(BitConverter.Int64BitsToDouble(
                         IPAddress.NetworkToHostOrder(
                         BitConverter.ToInt64(bufferBytes,
                                                             (byte)AppProtocolRemoteScanAimPosDatagram.AimAttY))).ToString());
            returnedString.Add(BitConverter.Int64BitsToDouble(
                         IPAddress.NetworkToHostOrder(
                         BitConverter.ToInt64(bufferBytes,
                                                             (byte)AppProtocolRemoteScanAimPosDatagram.AimAttZ))).ToString());
           
            return returnedString;
        }

        #endregion

    }
}
