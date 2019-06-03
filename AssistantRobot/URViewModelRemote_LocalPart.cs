using System;
//using System.Collections.Generic;
//using System.Linq;
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
            URStatus = 1,
            BreastScanStatus = 2,

            AdditionalMsg = 101
        }

        /// <summary>
        /// 应用协议状态 UR状态
        /// </summary>
        public enum AppProtocolURStatus : byte
        {
            URRealTimeData = 1,
            URNetAbnormalAbort = 2,
            URWorkEmergencyState = 3,
            URNearSingularState = 4,
            URPowerOnAsk = 5,
            URPowerOnAskReply = 6
        }

        /// <summary>
        /// 应用协议状态 乳腺扫描状态
        /// </summary>
        public enum AppProtocolBreastScanStatus : byte
        {
            BreastScanConfiguration = 1,
            BreastScanWorkStatus = 2,
            BreastScanConfirmStatus = 3,
            BreastScanForceZerodStatus = 4,

            BreastScanImmediateStop = 101,
            BreastScanImmediateStopRecovery = 102
        }

        /// <summary>
        /// 应用协议状态 额外消息状态
        /// </summary>
        public enum AppProtocolAdditionalMsgStatus : byte
        {

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
        /// 应用协议指令
        /// </summary>
        public enum AppProtocolCommand : byte
        {
            MoveTcp = 1,
            MoveJoint = 2,
            MoveStop = 3,
            MoveReference = 4,
            MoveSpeed = 5,
            BreastScan = 6
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
        /// 应用协议指令 乳腺扫描数据报格式
        /// </summary>
        public enum AppProtocolBreastScanDatagram : byte 
        {
            EnterBreastScanMode = 1,
            BeginForceZeroed = 2,
            BeginConfigurationSet = 3,
            NextConfigurationItem = 4,
            ConfirmConfigurationSet = 5,
            ReadyAndStartBreastScan = 6,
            SaveConfigurationSet = 7,

            StopBreastScanImmediately = 101,

            ExitBreastScanMode = 201
        }





        #endregion


        #region 字段
        private readonly URViewModel urvm;
        private PipeConnector ppc = new PipeConnector("innerCommunication");

        private const double maxSpeedRatio = 50.0; // 最大速度比例

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
        /// 接收字节流处理
        /// </summary>
        /// <param name="getBytes">收到的字节流</param>
        void DealWithRecievedBytes(byte[] getBytes)
        {
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
                case AppProtocolCommand.BreastScan:
                    {








                    }
                    break;
                default:
                    Logger.HistoryPrinting(Logger.Level.WARN, MethodBase.GetCurrentMethod().DeclaringType.FullName, "No such command, command number: +" + ((byte)getKey).ToString() + ".");
                    break;
            }
        }

        /// <summary>
        /// 管道已经断开
        /// </summary>
        void GetPipeCrashed()
        {
            urvm.IfRecievedPipeCrashed = true;
            urvm.DirectCloseModelLogic();
        }







        #endregion





    }
}
