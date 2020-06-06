﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Threading;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using MahApps.Metro.IconPacks;

using System.Windows;
using System.Windows.Data;
using System.ComponentModel;
using System.Reflection;
using System.Configuration;
using System.Net;
using System.Diagnostics;
using System.IO;

using LogPrinter;
using URCommunication;
using URModule;


namespace AssistantRobot
{
    public class URViewModel : INotifyPropertyChanged
    {
        #region 枚举
        /// <summary>
        /// 末端工具类型
        /// </summary>
        public enum ToolType : short
        {
            Probe_LA523 = 1,
            Needle = 2
        }

        /// <summary>
        /// 显示页的标号
        /// </summary>
        public enum ShowPage : short
        {
            MainNav = 0,
            BaseControl = 1,

            GalactophoreDetect = 2,

            ThyroidScanning = 3
        }

        /// <summary>
        /// 浮动窗体索引
        /// </summary>
        public enum ConfPage : short
        {
            ElecCtrl = 0,
            ProbeCatch = 1,
            GalactophoreDetect = 2,
            ThyroidPuncture = 3,
            ThyroidScan = 4
        }
        #endregion

        #region Model
        private CommunicationModel cm;
        //private GetSensorDatas gsd;

        // 移动最高最低速度和加速度
        private readonly double fastSpeedL = 0.2;
        private readonly double slowSpeedL = 0.1;
        private readonly double minSpeedL = 0.00002;
        private readonly double fastAccelerationL = 0.2;
        private readonly double slowAccelerationL = 0.1;
        private readonly double minAccelerationL = 0.001;
        private readonly double fastSpeedj = 0.4;
        private readonly double slowSpeedj = 0.2;
        private readonly double minSpeedj = 0.0004;
        private readonly double fastAccelerationj = 0.4;
        private readonly double slowAccelerationj = 0.2;
        private readonly double minAccelerationj = 0.002;

        // 当前位置缓存
        private double[] posCacheNow = new double[6];

        // 是否正在初始化机械臂
        private bool ifInitialArm = false;

        // 是否正在等待恢复GDR控制权
        private bool ifWaitForGDR = false;

                // 是否正在等待恢复TSR控制权
        private bool ifWaitForTSR = false;

        // 动作捕捉数据获取是否成功
        private bool ifActionSensorDataCatched = false;

        // 动作捕捉是否开始
        private bool ifActionCatchStart = false;

        // 首个动作是否记录
        private int actionCatchedNum = 0;

        // 首个动作的记录
        private double[] firstActionCatched = new double[10];

        // 监控程序
        private readonly string superviseProgramName = "AssistantRobotRemoteSupervisor.exe";
        private readonly string superviseProgramPath = "D:\\";
        Process supervisingProgram = new Process();
        #endregion

        #region View
        private MainWindow mw;
        private MainPage mp;
        private BaseControl bc;
        private GalactophoreDetect gd;
        private ThyroidScan ts;

        private bool[] occupyArray = new bool[100];
        public readonly double titleSize = 18;
        public readonly double messageSize = 22;

        #endregion

        #region ViewModel
        public event PropertyChangedEventHandler PropertyChanged;

        #region Close Window Enable
        private bool closeWinEnable = false;
        /// <summary>
        /// 关闭程序使能
        /// </summary>
        public bool CloseWinEnable
        {
            get { return closeWinEnable; }
            set
            {
                closeWinEnable = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("CloseWinEnable"));
                }
            }
        }
        #endregion

        #region Global Controls Enable
        private bool localEnable = false;
        /// <summary>
        /// 当地允许所有动作
        /// </summary>
        public bool LocalEnable
        {
            get { return localEnable; }
            set
            {
                localEnable = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("LocalEnable"));
                }
            }
        }

        private bool remoteEnable = false;
        /// <summary>
        /// 远程允许所有动作
        /// </summary>
        public bool RemoteEnable
        {
            get { return remoteEnable; }
            set
            {
                remoteEnable = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("RemoteEnable"));
                }
            }
        }
        #endregion

        #region WindowCommand Kind And Text
        private string superviseBtnText = "打开监控";
        /// <summary>
        /// 监控按钮文字
        /// </summary>
        public string SuperviseBtnText
        {
            get { return superviseBtnText; }
            set
            {
                superviseBtnText = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("SuperviseBtnText"));
                }
            }
        }

        private string connectBtnText = "建立连接";
        /// <summary>
        /// 连接按钮文字
        /// </summary>
        public string ConnectBtnText
        {
            get { return connectBtnText; }
            set
            {
                connectBtnText = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("ConnectBtnText"));
                }
            }
        }

        private PackIconFontAwesomeKind superviseBtnIcon = PackIconFontAwesomeKind.EyeSolid;
        /// <summary>
        /// 监控按钮图标
        /// </summary>
        public PackIconFontAwesomeKind SuperviseBtnIcon
        {
            get { return superviseBtnIcon; }
            set
            {
                superviseBtnIcon = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("SuperviseBtnIcon"));
                }
            }
        }

        private PackIconMaterialKind connectBtnIcon = PackIconMaterialKind.LanConnect;
        /// <summary>
        ///  连接按钮图标
        /// </summary>
        public PackIconMaterialKind ConnectBtnIcon
        {
            get { return connectBtnIcon; }
            set
            {
                connectBtnIcon = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("ConnectBtnIcon"));
                }
            }
        }

        private bool superviseBtnEnable = true;
        /// <summary>
        /// 监控按钮使能
        /// </summary>
        public bool SuperviseBtnEnable
        {
            get { return superviseBtnEnable; }
            set
            {
                superviseBtnEnable = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("SuperviseBtnEnable"));
                }
            }
        }

        private bool connectBtnEnable = true;
        /// <summary>
        /// 连接按钮使能
        /// </summary>
        public bool ConnectBtnEnable
        {
            get { return connectBtnEnable; }
            set
            {
                connectBtnEnable = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("ConnectBtnEnable"));
                }
            }
        }
        #endregion

        #region StatusBar Content And Color
        private string statusBarContent = "当地网络连接未建立";
        /// <summary>
        /// 状态栏内容
        /// </summary>
        public string StatusBarContent
        {
            get { return statusBarContent; }
            set
            {
                statusBarContent = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("StatusBarContent"));
                }
            }
        }

        private static readonly SolidColorBrush defaultBlueColor = new SolidColorBrush(Color.FromRgb(0x41, 0xB1, 0xE1));
        private static readonly SolidColorBrush defaultRedColor = new SolidColorBrush(Color.FromRgb(0xFA, 0x4C, 0x8F));
        private static readonly SolidColorBrush defaultGreenColor = new SolidColorBrush(Color.FromRgb(0x4B, 0xF8, 0xCB));
        private SolidColorBrush statusBarBackgroundColor = defaultBlueColor;
        /// <summary>
        /// 状态栏背景颜色
        /// </summary>
        public SolidColorBrush StatusBarBackgroundColor
        {
            get { return statusBarBackgroundColor; }
            set
            {
                statusBarBackgroundColor = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("StatusBarBackgroundColor"));
                }
            }
        }
        #endregion

        #region StatusBarRemote Content And Color
        private string statusBarRemoteContent = "远程网络连接未建立";
        /// <summary>
        /// 远程状态栏内容
        /// </summary>
        public string StatusBarRemoteContent
        {
            get { return statusBarRemoteContent; }
            set
            {
                statusBarRemoteContent = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("StatusBarRemoteContent"));
                }
            }
        }

        private SolidColorBrush statusBarRemoteBackgroundColor = defaultBlueColor;
        /// <summary>
        /// 远程状态栏背景颜色
        /// </summary>
        public SolidColorBrush StatusBarRemoteBackgroundColor
        {
            get { return statusBarRemoteBackgroundColor; }
            set
            {
                statusBarRemoteBackgroundColor = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("StatusBarRemoteBackgroundColor"));
                }
            }
        }
        #endregion

        #region Base Moving Refrence Cordinate And Moving Speed Ratio
        private bool baseMoveCordinate = false;
        /// <summary>
        /// 基本移动相对的坐标系
        /// </summary>
        public bool BaseMoveCordinate
        {
            get { return baseMoveCordinate; }
            set
            {
                baseMoveCordinate = value;
                cm.SendCmd(CommunicationModel.TCPProtocolKey.NormalData,
                    new byte[] { Convert.ToByte(baseMoveCordinate ? 1 : 0) },
                    CommunicationModel.AppProtocolCommand.MoveReference);
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("BaseMoveCordinate"));
                }
            }
        }

        private double baseMoveSpeedRatio = 50.0;
        /// <summary>
        /// 基本移动速度系数
        /// </summary>
        public double BaseMoveSpeedRatio
        {
            get { return baseMoveSpeedRatio; }
            set
            {
                baseMoveSpeedRatio = value;
                cm.SendCmd(CommunicationModel.TCPProtocolKey.NormalData,
                    BitConverter.GetBytes(
                    IPAddress.HostToNetworkOrder(
                    BitConverter.ToInt32(
                    BitConverter.GetBytes(
                    Convert.ToSingle(baseMoveSpeedRatio)), 0))),
                    CommunicationModel.AppProtocolCommand.MoveSpeed);
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("BaseMoveSpeedRatio"));
                }
            }
        }
        #endregion

        #region Parameters Needed To Show On Window

        #region Tool TCP Cordinates
        private double toolTCPCordinateX = 0.0;
        /// <summary>
        /// 工具TCP坐标X分量
        /// </summary>
        public double ToolTCPCordinateX
        {
            get { return toolTCPCordinateX; }
            set
            {
                toolTCPCordinateX = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("ToolTCPCordinateX"));
                }
            }
        }

        private double toolTCPCordinateY = 0.0;
        /// <summary>
        /// 工具TCP坐标Y分量
        /// </summary>
        public double ToolTCPCordinateY
        {
            get { return toolTCPCordinateY; }
            set
            {
                toolTCPCordinateY = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("ToolTCPCordinateY"));
                }
            }
        }

        private double toolTCPCordinateZ = 0.0;
        /// <summary>
        /// 工具TCP坐标Z分量
        /// </summary>
        public double ToolTCPCordinateZ
        {
            get { return toolTCPCordinateZ; }
            set
            {
                toolTCPCordinateZ = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("ToolTCPCordinateZ"));
                }
            }
        }

        private double toolTCPCordinateRX = 0.0;
        /// <summary>
        /// 工具TCP坐标RX分量
        /// </summary>
        public double ToolTCPCordinateRX
        {
            get { return toolTCPCordinateRX; }
            set
            {
                toolTCPCordinateRX = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("ToolTCPCordinateRX"));
                }
            }
        }

        private double toolTCPCordinateRY = 0.0;
        /// <summary>
        /// 工具TCP坐标RY分量
        /// </summary>
        public double ToolTCPCordinateRY
        {
            get { return toolTCPCordinateRY; }
            set
            {
                toolTCPCordinateRY = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("ToolTCPCordinateRY"));
                }
            }
        }

        private double toolTCPCordinateRZ = 0.0;
        /// <summary>
        /// 工具TCP坐标RZ分量
        /// </summary>
        public double ToolTCPCordinateRZ
        {
            get { return toolTCPCordinateRZ; }
            set
            {
                toolTCPCordinateRZ = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("ToolTCPCordinateRZ"));
                }
            }
        }
        #endregion

        #region Robot Joints Angles
        private double robotJointBaseAngle = 0.0;
        /// <summary>
        /// 机械臂基轴角度
        /// </summary>
        public double RobotJointBaseAngle
        {
            get { return robotJointBaseAngle; }
            set
            {
                robotJointBaseAngle = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("RobotJointBaseAngle"));
                }
            }
        }

        private double robotJointShoulderAngle = 0.0;
        /// <summary>
        /// 机械臂肩轴角度
        /// </summary>
        public double RobotJointShoulderAngle
        {
            get { return robotJointShoulderAngle; }
            set
            {
                robotJointShoulderAngle = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("RobotJointShoulderAngle"));
                }
            }
        }

        private double robotJointElbowAngle = 0.0;
        /// <summary>
        /// 机械臂肘轴角度
        /// </summary>
        public double RobotJointElbowAngle
        {
            get { return robotJointElbowAngle; }
            set
            {
                robotJointElbowAngle = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("RobotJointElbowAngle"));
                }
            }
        }

        private double robotJointWrist1Angle = 0.0;
        /// <summary>
        /// 机械臂腕轴1角度
        /// </summary>
        public double RobotJointWrist1Angle
        {
            get { return robotJointWrist1Angle; }
            set
            {
                robotJointWrist1Angle = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("RobotJointWrist1Angle"));
                }
            }
        }

        private double robotJointWrist2Angle = 0.0;
        /// <summary>
        /// 机械臂腕轴2角度
        /// </summary>
        public double RobotJointWrist2Angle
        {
            get { return robotJointWrist2Angle; }
            set
            {
                robotJointWrist2Angle = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("RobotJointWrist2Angle"));
                }
            }
        }

        private double robotJointWrist3Angle = 0.0;
        /// <summary>
        /// 机械臂腕轴3角度
        /// </summary>
        public double RobotJointWrist3Angle
        {
            get { return robotJointWrist3Angle; }
            set
            {
                robotJointWrist3Angle = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("RobotJointWrist3Angle"));
                }
            }
        }
        #endregion

        #region Robot Joints Temperatures
        private double robotJointBaseTemperature = 0.0;
        /// <summary>
        /// 机械臂基轴温度
        /// </summary>
        public double RobotJointBaseTemperature
        {
            get { return robotJointBaseTemperature; }
            set
            {
                robotJointBaseTemperature = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("RobotJointBaseTemperature"));
                }
            }
        }

        private double robotJointShoulderTemperature = 0.0;
        /// <summary>
        /// 机械臂肩轴温度
        /// </summary>
        public double RobotJointShoulderTemperature
        {
            get { return robotJointShoulderTemperature; }
            set
            {
                robotJointShoulderTemperature = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("RobotJointShoulderTemperature"));
                }
            }
        }

        private double robotJointElbowTemperature = 0.0;
        /// <summary>
        /// 机械臂肘轴温度
        /// </summary>
        public double RobotJointElbowTemperature
        {
            get { return robotJointElbowTemperature; }
            set
            {
                robotJointElbowTemperature = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("RobotJointElbowTemperature"));
                }
            }
        }

        private double robotJointWrist1Temperature = 0.0;
        /// <summary>
        /// 机械臂腕轴1温度
        /// </summary>
        public double RobotJointWrist1Temperature
        {
            get { return robotJointWrist1Temperature; }
            set
            {
                robotJointWrist1Temperature = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("RobotJointWrist1Temperature"));
                }
            }
        }

        private double robotJointWrist2Temperature = 0.0;
        /// <summary>
        /// 机械臂腕轴2温度
        /// </summary>
        public double RobotJointWrist2Temperature
        {
            get { return robotJointWrist2Temperature; }
            set
            {
                robotJointWrist2Temperature = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("RobotJointWrist2Temperature"));
                }
            }
        }

        private double robotJointWrist3Temperature = 0.0;
        /// <summary>
        /// 机械臂腕轴3温度
        /// </summary>
        public double RobotJointWrist3Temperature
        {
            get { return robotJointWrist3Temperature; }
            set
            {
                robotJointWrist3Temperature = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("RobotJointWrist3Temperature"));
                }
            }
        }
        #endregion

        #region Robot Joints Currents
        private double robotJointBaseCurrent = 0.0;
        /// <summary>
        /// 机械臂基轴电流
        /// </summary>
        public double RobotJointBaseCurrent
        {
            get { return robotJointBaseCurrent; }
            set
            {
                robotJointBaseCurrent = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("RobotJointBaseCurrent"));
                }
            }
        }

        private double robotJointShoulderCurrent = 0.0;
        /// <summary>
        /// 机械臂肩轴电流
        /// </summary>
        public double RobotJointShoulderCurrent
        {
            get { return robotJointShoulderCurrent; }
            set
            {
                robotJointShoulderCurrent = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("RobotJointShoulderCurrent"));
                }
            }
        }

        private double robotJointElbowCurrent = 0.0;
        /// <summary>
        /// 机械臂肘轴电流
        /// </summary>
        public double RobotJointElbowCurrent
        {
            get { return robotJointElbowCurrent; }
            set
            {
                robotJointElbowCurrent = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("RobotJointElbowCurrent"));
                }
            }
        }

        private double robotJointWrist1Current = 0.0;
        /// <summary>
        /// 机械臂腕轴1电流
        /// </summary>
        public double RobotJointWrist1Current
        {
            get { return robotJointWrist1Current; }
            set
            {
                robotJointWrist1Current = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("RobotJointWrist1Current"));
                }
            }
        }

        private double robotJointWrist2Current = 0.0;
        /// <summary>
        /// 机械臂腕轴2电流
        /// </summary>
        public double RobotJointWrist2Current
        {
            get { return robotJointWrist2Current; }
            set
            {
                robotJointWrist2Current = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("RobotJointWrist2Current"));
                }
            }
        }

        private double robotJointWrist3Current = 0.0;
        /// <summary>
        /// 机械臂腕轴3电流
        /// </summary>
        public double RobotJointWrist3Current
        {
            get { return robotJointWrist3Current; }
            set
            {
                robotJointWrist3Current = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("RobotJointWrist3Current"));
                }
            }
        }
        #endregion

        #region Tool Input Digital IO
        private bool toolInputDigitialIO1 = false;
        /// <summary>
        /// 工具数字IO输入1
        /// </summary>
        public bool ToolInputDigitialIO1
        {
            get { return toolInputDigitialIO1; }
            set
            {
                toolInputDigitialIO1 = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("ToolInputDigitialIO1"));
                }
            }
        }

        private bool toolInputDigitialIO2 = false;
        /// <summary>
        /// 工具数字IO输入2
        /// </summary>
        public bool ToolInputDigitialIO2
        {
            get { return toolInputDigitialIO2; }
            set
            {
                toolInputDigitialIO2 = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("ToolInputDigitialIO2"));
                }
            }
        }
        #endregion

        #region Robot Status And Program Status
        private URDataProcessor.RobotStatus robotCurrentStatus = UR30003Connector.RobotStatus.PowerOff;
        /// <summary>
        /// 机械臂当前状态
        /// </summary>
        public URDataProcessor.RobotStatus RobotCurrentStatus
        {
            get { return robotCurrentStatus; }
            set
            {
                robotCurrentStatus = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("RobotCurrentStatus"));
                }
            }
        }

        private URDataProcessor.RobotProgramStatus robotProgramCurrentStatus = UR30003Connector.RobotProgramStatus.Begin;
        /// <summary>
        /// 机械臂程序当前状态
        /// </summary>
        public URDataProcessor.RobotProgramStatus RobotProgramCurrentStatus
        {
            get { return robotProgramCurrentStatus; }
            set
            {
                robotProgramCurrentStatus = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("RobotProgramCurrentStatus"));
                }
            }
        }
        #endregion

        #region Tool Force And Torque
        private double toolForceX = 0.0;
        /// <summary>
        /// 工具末端X方向力
        /// </summary>
        public double ToolForceX
        {
            get { return toolForceX; }
            set
            {
                toolForceX = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("ToolForceX"));
                }
            }
        }

        private double toolForceY = 0.0;
        /// <summary>
        /// 工具末端Y方向力
        /// </summary>
        public double ToolForceY
        {
            get { return toolForceY; }
            set
            {
                toolForceY = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("ToolForceY"));
                }
            }
        }

        private double toolForceZ = 0.0;
        /// <summary>
        /// 工具末端Z方向力
        /// </summary>
        public double ToolForceZ
        {
            get { return toolForceZ; }
            set
            {
                toolForceZ = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("ToolForceZ"));
                }
            }
        }

        private double toolTorqueX = 0.0;
        /// <summary>
        /// 工具末端X方向力矩
        /// </summary>
        public double ToolTorqueX
        {
            get { return toolTorqueX; }
            set
            {
                toolTorqueX = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("ToolTorqueX"));
                }
            }
        }

        private double toolTorqueY = 0.0;
        /// <summary>
        /// 工具末端Y方向力矩
        /// </summary>
        public double ToolTorqueY
        {
            get { return toolTorqueY; }
            set
            {
                toolTorqueY = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("ToolTorqueY"));
                }
            }
        }

        private double toolTorqueZ = 0.0;
        /// <summary>
        /// 工具末端Z方向力矩
        /// </summary>
        public double ToolTorqueZ
        {
            get { return toolTorqueZ; }
            set
            {
                toolTorqueZ = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("ToolTorqueZ"));
                }
            }
        }
        #endregion

        #endregion

        #region Nipple Position At Galactophore Detecting
        private double[] nipplePositionGDR = new double[3];
        /// <summary>
        /// 乳腺扫查中的乳头位置
        /// </summary>
        public double[] NipplePositionGDR
        {
            get { return (double[])nipplePositionGDR.Clone(); }
            set
            {
                nipplePositionGDR = (double[])value.Clone();
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("NipplePositionGDR"));
                }
            }
        }
        #endregion

        #region Configuration Parameters Of GalactophoreDetector

        #region Detecting Direction Force Limits And Speed Limits
        private double detectingErrorForceMinGDR = 0.0;
        /// <summary>
        /// 乳腺扫查中探测方向误差力最小值
        /// </summary>
        public double DetectingErrorForceMinGDR
        {
            get { return detectingErrorForceMinGDR; }
            set
            {
                detectingErrorForceMinGDR = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("DetectingErrorForceMinGDR"));
                }
            }
        }

        private double detectingErrorForceMaxGDR = 0.0;
        /// <summary>
        /// 乳腺扫查中探测方向误差力最大值
        /// </summary>
        public double DetectingErrorForceMaxGDR
        {
            get { return detectingErrorForceMaxGDR; }
            set
            {
                detectingErrorForceMaxGDR = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("DetectingErrorForceMaxGDR"));
                }
            }
        }

        private double detectingSpeedMinGDR = 0.0;
        /// <summary>
        /// 乳腺扫查中探测方向运动速度最小值
        /// </summary>
        public double DetectingSpeedMinGDR
        {
            get { return detectingSpeedMinGDR; }
            set
            {
                detectingSpeedMinGDR = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("DetectingSpeedMinGDR"));
                }
            }
        }
        #endregion

        #region Detecting Motion Limits
        private double nippleForbiddenRadiusGDR = 0.0;
        /// <summary>
        /// 乳腺扫查中乳头防撞禁止半径
        /// </summary>
        public double NippleForbiddenRadiusGDR
        {
            get { return nippleForbiddenRadiusGDR; }
            set
            {
                nippleForbiddenRadiusGDR = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("NippleForbiddenRadiusGDR"));
                }
            }
        }

        private double detectingStopDistanceGDR = 0.0;
        /// <summary>
        /// 乳腺扫查中探测方向停止距离
        /// </summary>
        public double DetectingStopDistanceGDR
        {
            get { return detectingStopDistanceGDR; }
            set
            {
                detectingStopDistanceGDR = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("DetectingStopDistanceGDR"));
                }
            }
        }

        private double detectingSafetyLiftDistanceGDR = 0.0;
        /// <summary>
        /// 乳腺扫查中探测方向安全上升距离
        /// </summary>
        public double DetectingSafetyLiftDistanceGDR
        {
            get { return detectingSafetyLiftDistanceGDR; }
            set
            {
                detectingSafetyLiftDistanceGDR = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("DetectingSafetyLiftDistanceGDR"));
                }
            }
        }

        private double detectingSinkDistanceGDR = 0.0;
        /// <summary>
        /// 乳腺扫查中探测方向下沉距离
        /// </summary>
        public double DetectingSinkDistanceGDR
        {
            get { return detectingSinkDistanceGDR; }
            set
            {
                detectingSinkDistanceGDR = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("DetectingSinkDistanceGDR"));
                }
            }
        }

        private bool ifEnableDetectingInitialForceGDR = false;
        /// <summary>
        /// 乳腺扫查中初始力检查以确定初始姿态角开关
        /// </summary>
        public bool IfEnableDetectingInitialForceGDR
        {
            get { return ifEnableDetectingInitialForceGDR; }
            set
            {
                ifEnableDetectingInitialForceGDR = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("IfEnableDetectingInitialForceGDR"));
                }
            }
        }

        private bool ifEnableAngleCorrectedGDR = false;
        /// <summary>
        /// 乳腺扫查中姿态角矫正开关
        /// </summary>
        public bool IfEnableAngleCorrectedGDR
        {
            get { return ifEnableAngleCorrectedGDR; }
            set
            {
                ifEnableAngleCorrectedGDR = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("IfEnableAngleCorrectedGDR"));
                }
            }
        }
        #endregion

        #region Degree Control Parameters
        private GalactophoreDetector.VibratingMagnitude vibratingAngleDegreeGDR = GalactophoreDetector.VibratingMagnitude.Medium;
        /// <summary>
        /// 乳腺扫查中摆动方向摆动幅度
        /// </summary>
        public GalactophoreDetector.VibratingMagnitude VibratingAngleDegreeGDR
        {
            get { return vibratingAngleDegreeGDR; }
            set
            {
                vibratingAngleDegreeGDR = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("VibratingAngleDegreeGDR"));
                }
            }
        }

        private GalactophoreDetector.MovingLevel movingSpeedDegreeGDR = GalactophoreDetector.MovingLevel.Medium;
        /// <summary>
        /// 乳腺扫查中移动方向移动快慢
        /// </summary>
        public GalactophoreDetector.MovingLevel MovingSpeedDegreeGDR
        {
            get { return movingSpeedDegreeGDR; }
            set
            {
                movingSpeedDegreeGDR = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("MovingSpeedDegreeGDR"));
                }
            }
        }

        private GalactophoreDetector.DetectingIntensity detectingForceDegreeGDR = GalactophoreDetector.DetectingIntensity.SlightlyLight;
        /// <summary>
        /// 乳腺扫查中探测方向力度大小
        /// </summary>
        public GalactophoreDetector.DetectingIntensity DetectingForceDegreeGDR
        {
            get { return detectingForceDegreeGDR; }
            set
            {
                detectingForceDegreeGDR = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("DetectingForceDegreeGDR"));
                }
            }
        }

        private GalactophoreDetector.AligningDegree detectingAlignDegreeGDR = GalactophoreDetector.AligningDegree.Loose;
        /// <summary>
        /// 乳腺扫查中探测整体贴合程度
        /// </summary>
        public GalactophoreDetector.AligningDegree DetectingAlignDegreeGDR
        {
            get { return detectingAlignDegreeGDR; }
            set
            {
                detectingAlignDegreeGDR = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("DetectingAlignDegreeGDR"));
                }
            }
        }
        #endregion

        #region Detecting Edge
        private GalactophoreDetector.IdentifyBoundary identifyEdgeModeGDR = GalactophoreDetector.IdentifyBoundary.OnlyUpBoundary;
        /// <summary>
        /// 乳腺扫查中获得边界的方法
        /// </summary>
        public GalactophoreDetector.IdentifyBoundary IdentifyEdgeModeGDR
        {
            get { return identifyEdgeModeGDR; }
            set
            {
                identifyEdgeModeGDR = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("IdentifyEdgeModeGDR"));
                }
            }
        }

        private double movingUpEdgeDistanceGDR = 0.0;
        /// <summary>
        /// 乳腺扫查中移动方向上边界距离
        /// </summary>
        public double MovingUpEdgeDistanceGDR
        {
            get { return movingUpEdgeDistanceGDR; }
            set
            {
                movingUpEdgeDistanceGDR = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("MovingUpEdgeDistanceGDR"));
                }
            }
        }

        private double movingLeftEdgeDistanceGDR = 0.0;
        /// <summary>
        /// 乳腺扫查中移动方向左边界距离
        /// </summary>
        public double MovingLeftEdgeDistanceGDR
        {
            get { return movingLeftEdgeDistanceGDR; }
            set
            {
                movingLeftEdgeDistanceGDR = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("MovingLeftEdgeDistanceGDR"));
                }
            }
        }

        private double movingDownEdgeDistanceGDR = 0.0;
        /// <summary>
        /// 乳腺扫查中移动方向下边界距离
        /// </summary>
        public double MovingDownEdgeDistanceGDR
        {
            get { return movingDownEdgeDistanceGDR; }
            set
            {
                movingDownEdgeDistanceGDR = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("MovingDownEdgeDistanceGDR"));
                }
            }
        }

        private double movingRightEdgeDistanceGDR = 0.0;
        /// <summary>
        /// 乳腺扫查中移动方向右边界距离
        /// </summary>
        public double MovingRightEdgeDistanceGDR
        {
            get { return movingRightEdgeDistanceGDR; }
            set
            {
                movingRightEdgeDistanceGDR = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("MovingRightEdgeDistanceGDR"));
                }
            }
        }
        #endregion

        #region Other
        private bool ifAutoReplaceConfigurationGDR = true;
        /// <summary>
        /// 乳腺扫查中文件自动转存开关
        /// </summary>
        public bool IfAutoReplaceConfigurationGDR
        {
            get { return ifAutoReplaceConfigurationGDR; }
            set
            {
                ifAutoReplaceConfigurationGDR = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("IfAutoReplaceConfigurationGDR"));
                }
            }
        }

        private GalactophoreDetector.ScanningRegion ifCheckRightGalactophoreGDR = GalactophoreDetector.ScanningRegion.RightGalactophore;
        /// <summary>
        /// 乳腺扫查中是否检测右侧乳房
        /// </summary>
        public GalactophoreDetector.ScanningRegion IfCheckRightGalactophoreGDR
        {
            get { return ifCheckRightGalactophoreGDR; }
            set
            {
                ifCheckRightGalactophoreGDR = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("IfCheckRightGalactophoreGDR"));
                }
            }
        }

        private double checkingStepGDR = 0.3;
        /// <summary>
        /// 乳腺扫查中扫查过程的步长
        /// </summary>
        public double CheckingStepGDR
        {
            get { return checkingStepGDR; }
            set
            {
                checkingStepGDR = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("CheckingStepGDR"));
                }
            }
        }
        #endregion

        #endregion

        #region GalactophoreDetector Working Status
        private short galactophoreDetectorWorkStatus = -1;
        /// <summary>
        /// 乳腺扫查者工作状态
        /// </summary>
        public short GalactophoreDetectorWorkStatus
        {
            get { return galactophoreDetectorWorkStatus; }
            set
            {
                galactophoreDetectorWorkStatus = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("GalactophoreDetectorWorkStatus"));
                }
            }
        }
        #endregion

        #region GalactophoreDetector Parameter Confirm
        private bool galactophoreDetectorParameterConfirm = false;
        /// <summary>
        /// 乳腺扫查者配置参数确认情况
        /// </summary>
        public bool GalactophoreDetectorParameterConfirm
        {
            get { return galactophoreDetectorParameterConfirm; }
            set
            {
                galactophoreDetectorParameterConfirm = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("GalactophoreDetectorParameterConfirm"));
                }
            }
        }
        #endregion

        #region GalactophoreDetector ForceSenor Cleared
        private bool galactophoreDetectorForceSensorCleared = false;
        /// <summary>
        ///  乳腺扫查者力传感器清零状况
        /// </summary>
        public bool GalactophoreDetectorForceSensorCleared
        {
            get { return galactophoreDetectorForceSensorCleared; }
            set
            {
                galactophoreDetectorForceSensorCleared = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("GalactophoreDetectorForceSensorCleared"));
                }
            }
        }
        #endregion

        #region GalactophoreDetector Parameter Confirm State
        private byte galactophoreDetectorParameterConfirmState = 0;
        /// <summary>
        ///  乳腺扫查者配置参数更新状态
        /// </summary>
        public byte GalactophoreDetectorParameterConfirmState
        {
            get { return galactophoreDetectorParameterConfirmState; }
            set
            {
                galactophoreDetectorParameterConfirmState = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("GalactophoreDetectorParameterConfirmState"));
                }
            }
        }
        #endregion

        #region GalactophoreDetector Parameter Change Move
        private bool breastScanConfMovingEnable = false;
        /// <summary>
        /// 远程允许所有动作
        /// </summary>
        public bool BreastScanConfMovingEnable
        {
            get { return breastScanConfMovingEnable; }
            set
            {
                breastScanConfMovingEnable = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("BreastScanConfMovingEnable"));
                }
            }
        }
        #endregion

        #region Start Position At Thyroid Scanning
        private double[] startPositionTSR = new double[3];
        /// <summary>
        /// 甲状腺扫查中的起始位置
        /// </summary>
        public double[] StartPositionTSR
        {
            get { return (double[])startPositionTSR.Clone(); }
            set
            {
                startPositionTSR = (double[])value.Clone();
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("StartPositionTSR"));
                }
            }
        }
        #endregion

        #region Configuration Parameters Of ThyroidScanner

        #region Press Direction Parameters
        private double detectingErrorForceMinTSR = 0.0;
        /// <summary>
        /// 甲状腺扫查中探测方向误差力最小值
        /// </summary>
        public double DetectingErrorForceMinTSR
        {
            get { return detectingErrorForceMinTSR; }
            set
            {
                detectingErrorForceMinTSR = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("DetectingErrorForceMinTSR"));
                }
            }
        }

        private double detectingErrorForceMaxTSR = 0.0;
        /// <summary>
        /// 甲状腺扫查中探测方向误差力最大值
        /// </summary>
        public double DetectingErrorForceMaxTSR
        {
            get { return detectingErrorForceMaxTSR; }
            set
            {
                detectingErrorForceMaxTSR = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("DetectingErrorForceMaxTSR"));
                }
            }
        }

        private double detectingSpeedMinTSR = 0.0;
        /// <summary>
        /// 甲状腺扫查中探测方向速度最小值
        /// </summary>
        public double DetectingSpeedMinTSR
        {
            get { return detectingSpeedMinTSR; }
            set
            {
                detectingSpeedMinTSR = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("DetectingSpeedMinTSR"));
                }
            }
        }

        private double detectingSpeedMaxTSR = 0.0;
        /// <summary>
        /// 甲状腺扫查中探测方向速度最大值
        /// </summary>
        public double DetectingSpeedMaxTSR
        {
            get { return detectingSpeedMaxTSR; }
            set
            {
                detectingSpeedMaxTSR = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("DetectingSpeedMaxTSR"));
                }
            }
        }

        private bool ifEnableForceKeepingTSR = false;
        /// <summary>
        /// 甲状腺扫查中力保持开关
        /// </summary>
        public bool IfEnableForceKeepingTSR
        {
            get { return ifEnableForceKeepingTSR; }
            set
            {
                ifEnableForceKeepingTSR = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("IfEnableForceKeepingTSR"));
                }
            }
        }

        private bool ifEnableForceTrackingTSR = false;
        /// <summary>
        /// 甲状腺扫查中力跟踪开关
        /// </summary>
        public bool IfEnableForceTrackingTSR
        {
            get { return ifEnableForceTrackingTSR; }
            set
            {
                ifEnableForceTrackingTSR = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("IfEnableForceTrackingTSR"));
                }
            }
        }

        private double detectingBasicPreservedForceTSR = 0.0;
        /// <summary>
        /// 甲状腺扫查中基本保持力大小
        /// </summary>
        public double DetectingBasicPreservedForceTSR
        {
            get { return detectingBasicPreservedForceTSR; }
            set
            {
                detectingBasicPreservedForceTSR = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("DetectingBasicPreservedForceTSR"));
                }
            }
        }
        #endregion

        #region Limit Parameters
        private double maxAvailableRadiusTSR = 0.0;
        /// <summary>
        /// 甲状腺扫查中最大可达半径
        /// </summary>
        public double MaxAvailableRadiusTSR
        {
            get { return maxAvailableRadiusTSR; }
            set
            {
                maxAvailableRadiusTSR = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("MaxAvailableRadiusTSR"));
                }
            }
        }

        private double maxAvailableAngleTSR = 0.0;
        /// <summary>
        /// 甲状腺扫查中最大可达角度
        /// </summary>
        public double MaxAvailableAngleTSR
        {
            get { return maxAvailableAngleTSR; }
            set
            {
                maxAvailableAngleTSR = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("MaxAvailableAngleTSR"));
                }
            }
        }

        private double stopRadiusTSR = 0.0;
        /// <summary>
        /// 甲状腺扫查中终止距离
        /// </summary>
        public double StopRadiusTSR
        {
            get { return stopRadiusTSR; }
            set
            {
                stopRadiusTSR = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("StopRadiusTSR"));
                }
            }
        }

        private double maxDistPeriodTSR = 0.0;
        /// <summary>
        /// 甲状腺扫查中单周期最大距离
        /// </summary>
        public double MaxDistPeriodTSR
        {
            get { return maxDistPeriodTSR; }
            set
            {
                maxDistPeriodTSR = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("MaxDistPeriodTSR"));
                }
            }
        }

        private double maxAnglePeriodTSR = 0.0;
        /// <summary>
        /// 甲状腺扫查中单周期最大角度
        /// </summary>
        public double MaxAnglePeriodTSR
        {
            get { return maxAnglePeriodTSR; }
            set
            {
                maxAnglePeriodTSR = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("MaxAnglePeriodTSR"));
                }
            }
        }
        #endregion

        #region Factor
        private double positionOverrideTSR = 0.0;
        /// <summary>
        /// 甲状腺扫查中传输位置倍率
        /// </summary>
        public double PositionOverrideTSR
        {
            get { return positionOverrideTSR; }
            set
            {
                positionOverrideTSR = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("PositionOverrideTSR"));
                }
            }
        }

        private double angleOverrideTSR = 0.0;
        /// <summary>
        /// 甲状腺扫查中传输角度倍率
        /// </summary>
        public double AngleOverrideTSR
        {
            get { return angleOverrideTSR; }
            set
            {
                angleOverrideTSR = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("AngleOverrideTSR"));
                }
            }
        }

        private double forceOverrideTSR = 0.0;
        /// <summary>
        /// 甲状腺扫查中传输力倍率
        /// </summary>
        public double ForceOverrideTSR
        {
            get { return forceOverrideTSR; }
            set
            {
                forceOverrideTSR = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("ForceOverrideTSR"));
                }
            }
        }
        #endregion

        #region Other Switch
        private bool ifEnableAttitudeTrackingTSR = false;
        /// <summary>
        /// 甲状腺扫查中姿态跟踪开关
        /// </summary>
        public bool IfEnableAttitudeTrackingTSR
        {
            get { return ifEnableAttitudeTrackingTSR; }
            set
            {
                ifEnableAttitudeTrackingTSR = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("IfEnableAttitudeTrackingTSR"));
                }
            }
        }

        private bool ifEnableTranslationTrackingTSR = false;
        /// <summary>
        /// 甲状腺扫查中位置跟踪开关
        /// </summary>
        public bool IfEnableTranslationTrackingTSR
        {
            get { return ifEnableTranslationTrackingTSR; }
            set
            {
                ifEnableTranslationTrackingTSR = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("IfEnableTranslationTrackingTSR"));
                }
            }
        }
        #endregion

        #endregion

        #region ThyroidScanner Working Status
        private short thyroidScannerWorkStatus = -1;
        /// <summary>
        /// 甲状腺扫查者工作状态
        /// </summary>
        public short ThyroidScannerWorkStatus
        {
            get { return thyroidScannerWorkStatus; }
            set
            {
                thyroidScannerWorkStatus = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("ThyroidScannerWorkStatus"));
                }
            }
        }
        #endregion

        #region ThyroidScanner Parameter Confirm
        private bool thyroidScannerParameterConfirm = false;
        /// <summary>
        /// 甲状腺扫查者配置参数确认情况
        /// </summary>
        public bool ThyroidScannerParameterConfirm
        {
            get { return thyroidScannerParameterConfirm; }
            set
            {
                thyroidScannerParameterConfirm = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("ThyroidScannerParameterConfirm"));
                }
            }
        }
        #endregion

        #region ThyroidScanner ForceSenor Cleared
        private bool thyroidScannerForceSensorCleared = false;
        /// <summary>
        /// 甲状腺扫查者力传感器清零状况
        /// </summary>
        public bool ThyroidScannerForceSensorCleared
        {
            get { return thyroidScannerForceSensorCleared; }
            set
            {
                thyroidScannerForceSensorCleared = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("ThyroidScannerForceSensorCleared"));
                }
            }
        }
        #endregion

        #region ThyroidScanner Parameter Confirm State
        private byte thyroidScannerParameterConfirmState = 0;
        /// <summary>
        ///  甲状腺扫查者配置参数更新状态
        /// </summary>
        public byte ThyroidScannerParameterConfirmState
        {
            get { return thyroidScannerParameterConfirmState; }
            set
            {
                thyroidScannerParameterConfirmState = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("ThyroidScannerParameterConfirmState"));
                }
            }
        }
        #endregion

        #region ThyroidScanner Parameter Change Move
        private bool thyroidScanConfMovingEnable = false;
        /// <summary>
        /// 远程允许所有动作
        /// </summary>
        public bool ThyroidScanConfMovingEnable
        {
            get { return thyroidScanConfMovingEnable; }
            set
            {
                thyroidScanConfMovingEnable = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("ThyroidScanConfMovingEnable"));
                }
            }
        }
        #endregion
        #endregion

        #region Construct Function
        /// <summary>
        /// 构造函数，载入配置
        /// </summary>
        /// <param name="ifSuccess">是否配置成功</param>
        public URViewModel(out bool ifSuccess)
        {
            for (int pnum = 0; pnum < occupyArray.Length; pnum++) occupyArray[pnum] = false;

            ifSuccess = true;
            bool parseResult = true;

            double fastSpeedLTemp;
            parseResult = double.TryParse(ConfigurationManager.AppSettings["fastSpeedL"], out fastSpeedLTemp);
            if (parseResult) fastSpeedL = fastSpeedLTemp;
            else
            {
                ifSuccess = false;
                Logger.HistoryPrinting(Logger.Level.WARN, MethodBase.GetCurrentMethod().DeclaringType.FullName, "App configuration parameter(" + "fastSpeedL" + ") is wrong.");
                return;
            }

            double slowSpeedLTemp;
            parseResult = double.TryParse(ConfigurationManager.AppSettings["slowSpeedL"], out slowSpeedLTemp);
            if (parseResult) slowSpeedL = slowSpeedLTemp;
            else
            {
                ifSuccess = false;
                Logger.HistoryPrinting(Logger.Level.WARN, MethodBase.GetCurrentMethod().DeclaringType.FullName, "App configuration parameter(" + "slowSpeedL" + ") is wrong.");
                return;
            }

            double minSpeedLTemp;
            parseResult = double.TryParse(ConfigurationManager.AppSettings["minSpeedL"], out minSpeedLTemp);
            if (parseResult) minSpeedL = minSpeedLTemp;
            else
            {
                ifSuccess = false;
                Logger.HistoryPrinting(Logger.Level.WARN, MethodBase.GetCurrentMethod().DeclaringType.FullName, "App configuration parameter(" + "minSpeedL" + ") is wrong.");
                return;
            }

            double fastAccelerationLTemp;
            parseResult = double.TryParse(ConfigurationManager.AppSettings["fastAccelerationL"], out fastAccelerationLTemp);
            if (parseResult) fastAccelerationL = fastAccelerationLTemp;
            else
            {
                ifSuccess = false;
                Logger.HistoryPrinting(Logger.Level.WARN, MethodBase.GetCurrentMethod().DeclaringType.FullName, "App configuration parameter(" + "fastAccelerationL" + ") is wrong.");
                return;
            }

            double slowAccelerationLTemp;
            parseResult = double.TryParse(ConfigurationManager.AppSettings["slowAccelerationL"], out slowAccelerationLTemp);
            if (parseResult) slowAccelerationL = slowAccelerationLTemp;
            else
            {
                ifSuccess = false;
                Logger.HistoryPrinting(Logger.Level.WARN, MethodBase.GetCurrentMethod().DeclaringType.FullName, "App configuration parameter(" + "slowAccelerationL" + ") is wrong.");
                return;
            }

            double minAccelerationLTemp;
            parseResult = double.TryParse(ConfigurationManager.AppSettings["minAccelerationL"], out minAccelerationLTemp);
            if (parseResult) minAccelerationL = minAccelerationLTemp;
            else
            {
                ifSuccess = false;
                Logger.HistoryPrinting(Logger.Level.WARN, MethodBase.GetCurrentMethod().DeclaringType.FullName, "App configuration parameter(" + "minAccelerationL" + ") is wrong.");
                return;
            }

            double fastSpeedjTemp;
            parseResult = double.TryParse(ConfigurationManager.AppSettings["fastSpeedj"], out fastSpeedjTemp);
            if (parseResult) fastSpeedj = fastSpeedjTemp;
            else
            {
                ifSuccess = false;
                Logger.HistoryPrinting(Logger.Level.WARN, MethodBase.GetCurrentMethod().DeclaringType.FullName, "App configuration parameter(" + "fastSpeedj" + ") is wrong.");
                return;
            }

            double slowSpeedjTemp;
            parseResult = double.TryParse(ConfigurationManager.AppSettings["slowSpeedj"], out slowSpeedjTemp);
            if (parseResult) slowSpeedj = slowSpeedjTemp;
            else
            {
                ifSuccess = false;
                Logger.HistoryPrinting(Logger.Level.WARN, MethodBase.GetCurrentMethod().DeclaringType.FullName, "App configuration parameter(" + "slowSpeedj" + ") is wrong.");
                return;
            }

            double minSpeedjTemp;
            parseResult = double.TryParse(ConfigurationManager.AppSettings["minSpeedj"], out minSpeedjTemp);
            if (parseResult) minSpeedj = minSpeedjTemp;
            else
            {
                ifSuccess = false;
                Logger.HistoryPrinting(Logger.Level.WARN, MethodBase.GetCurrentMethod().DeclaringType.FullName, "App configuration parameter(" + "minSpeedj" + ") is wrong.");
                return;
            }

            double fastAccelerationjTemp;
            parseResult = double.TryParse(ConfigurationManager.AppSettings["fastAccelerationj"], out fastAccelerationjTemp);
            if (parseResult) fastAccelerationj = fastAccelerationjTemp;
            else
            {
                ifSuccess = false;
                Logger.HistoryPrinting(Logger.Level.WARN, MethodBase.GetCurrentMethod().DeclaringType.FullName, "App configuration parameter(" + "fastAccelerationj" + ") is wrong.");
                return;
            }

            double slowAccelerationjTemp;
            parseResult = double.TryParse(ConfigurationManager.AppSettings["slowAccelerationj"], out slowAccelerationjTemp);
            if (parseResult) slowAccelerationj = slowAccelerationjTemp;
            else
            {
                ifSuccess = false;
                Logger.HistoryPrinting(Logger.Level.WARN, MethodBase.GetCurrentMethod().DeclaringType.FullName, "App configuration parameter(" + "slowAccelerationj" + ") is wrong.");
                return;
            }

            double minAccelerationjTemp;
            parseResult = double.TryParse(ConfigurationManager.AppSettings["minAccelerationj"], out minAccelerationjTemp);
            if (parseResult) minAccelerationj = minAccelerationjTemp;
            else
            {
                ifSuccess = false;
                Logger.HistoryPrinting(Logger.Level.WARN, MethodBase.GetCurrentMethod().DeclaringType.FullName, "App configuration parameter(" + "minAccelerationj" + ") is wrong.");
                return;
            }

            double titleSizeTemp;
            parseResult = double.TryParse(ConfigurationManager.AppSettings["titleSize"], out titleSizeTemp);
            if (parseResult) titleSize = titleSizeTemp;
            else
            {
                ifSuccess = false;
                Logger.HistoryPrinting(Logger.Level.WARN, MethodBase.GetCurrentMethod().DeclaringType.FullName, "App configuration parameter(" + "titleSize" + ") is wrong.");
                return;
            }

            double messageSizeTemp;
            parseResult = double.TryParse(ConfigurationManager.AppSettings["messageSize"], out messageSizeTemp);
            if (parseResult) messageSize = messageSizeTemp;
            else
            {
                ifSuccess = false;
                Logger.HistoryPrinting(Logger.Level.WARN, MethodBase.GetCurrentMethod().DeclaringType.FullName, "App configuration parameter(" + "messageSize" + ") is wrong.");
                return;
            }

            string superviseProgramNameTemp = ConfigurationManager.AppSettings["superviseProgramName"];
            if (superviseProgramNameTemp == "AssistantRobotRemoteSupervisor.exe") superviseProgramName = superviseProgramNameTemp;
            else
            {
                ifSuccess = false;
                Logger.HistoryPrinting(Logger.Level.WARN, MethodBase.GetCurrentMethod().DeclaringType.FullName, "App configuration parameter(" + "superviseProgramName" + ") is wrong.");
                return;
            }

            string superviseProgramPathTemp = ConfigurationManager.AppSettings["superviseProgramPath"];
            if (Directory.Exists(superviseProgramPathTemp) && File.Exists(superviseProgramPathTemp + superviseProgramName))
            {
                superviseProgramPath = superviseProgramPathTemp;
            }
            else
            {
                ifSuccess = false;
                Logger.HistoryPrinting(Logger.Level.WARN, MethodBase.GetCurrentMethod().DeclaringType.FullName, "App configuration parameter(" + "superviseProgramPath" + ") is wrong.");
                return;
            }
            supervisingProgram.StartInfo.FileName = superviseProgramPath + superviseProgramName;
            supervisingProgram.EnableRaisingEvents = true;
            supervisingProgram.Exited += supervisingProgram_Exited;
        }

        private void supervisingProgram_Exited(object sender, EventArgs e)
        {
            SuperviseBtnIcon = PackIconFontAwesomeKind.EyeSolid;
            SuperviseBtnText = "打开监控";
            SuperviseBtnEnable = false;
            Task.Run(new Action(() =>
            {
                Thread.Sleep(500);
                SuperviseBtnEnable = true;
            }));
        }
        #endregion

        #region Method
        /// <summary>
        /// 打开监控程序
        /// </summary>
        /// <returns>返回监控器打开结果</returns>
        public bool OpenSuperViseProgram()
        {
            try
            {
                supervisingProgram.Start();
            }
            catch (Exception ex)
            {
                Logger.HistoryPrinting(Logger.Level.WARN, MethodBase.GetCurrentMethod().DeclaringType.FullName, "Open supervising program error.", ex);
                return false;
            }

            return true;
        }

        /// <summary>
        /// 关闭监控程序
        /// </summary>
        /// <returns>返回关闭结果</returns>
        public bool CloseSuperViseProgram()
        {
            return supervisingProgram.CloseMainWindow();
        }

        /// <summary>
        /// 机械臂上电
        /// </summary>
        public void RobotPowerOn()
        {
            if (!(LocalEnable && RemoteEnable))
            {
                ShowDialog("连接有问题，无法下达指令！", "问题", 12);
                return;
            }
            if (robotCurrentStatus == UR30003Connector.RobotStatus.PowerOff)
            {
                Task.Run(new Action(() =>
                {
                    cm.SendCmd(CommunicationModel.TCPProtocolKey.NormalData, null, CommunicationModel.AppProtocolCommand.PowerOn);
                }));
            }
        }

        /// <summary>
        /// 制动器松开
        /// </summary>
        public void BrakeLess()
        {
            if (!(LocalEnable && RemoteEnable))
            {
                ShowDialog("连接有问题，无法下达指令！", "问题", 12);
                return;
            }
            if (robotCurrentStatus == UR30003Connector.RobotStatus.Idle)
            {
                Task.Run(new Action(() =>
                {
                    cm.SendCmd(CommunicationModel.TCPProtocolKey.NormalData, null, CommunicationModel.AppProtocolCommand.BrakeRelease);
                }));
            }
        }

        /// <summary>
        /// 机械臂断电
        /// </summary>
        public void RobotPowerOff()
        {
            if (!(LocalEnable && RemoteEnable))
            {
                ShowDialog("连接有问题，无法下达指令！", "问题", 12);
                return;
            }
            if (robotCurrentStatus != UR30003Connector.RobotStatus.PowerOff)
            {
                Task.Run(new Action(() =>
                {
                    cm.SendCmd(CommunicationModel.TCPProtocolKey.NormalData, null, CommunicationModel.AppProtocolCommand.PowerOff);
                }));
            }
        }

        /// <summary>
        /// 控制箱关闭
        /// </summary>
        public async void ControllerBoxPowerOff()
        { // Remote禁止
        }

        /// <summary>
        /// 电气控制打开
        /// </summary>
        public void SwitchElectricControlFly()
        {
            bool nowState = (mw.Flyouts.Items[(int)ConfPage.ElecCtrl] as Flyout).IsOpen;
            (mw.Flyouts.Items[(int)ConfPage.ElecCtrl] as Flyout).IsOpen = !nowState;
        }

        /// <summary>
        /// 页导航
        /// </summary>
        /// <param name="ShowPageNum">要显示的页</param>
        public void NavigateToPage(ShowPage ShowPageNum)
        {
            switch (ShowPageNum)
            {
                case ShowPage.MainNav:
                    if (mw.frameNav.NavigationService.CanGoBack)
                    {
                        CloseWinEnable = true;
                        mw.frameNav.NavigationService.GoBack();
                        cm.SendCmd(CommunicationModel.TCPProtocolKey.NormalData, new byte[] { (byte)ShowPageNum }, CommunicationModel.AppProtocolCommand.ChangePage);
                    }
                    break;
                case ShowPage.BaseControl:
                    CloseWinEnable = false;
                    mw.frameNav.NavigationService.Navigate(bc);
                    cm.SendCmd(CommunicationModel.TCPProtocolKey.NormalData, new byte[] { (byte)ShowPageNum }, CommunicationModel.AppProtocolCommand.ChangePage);
                    break;

                case ShowPage.GalactophoreDetect:
                    CloseWinEnable = false;
                    mw.frameNav.NavigationService.Navigate(gd);
                    cm.SendCmd(CommunicationModel.TCPProtocolKey.NormalData, new byte[] { (byte)ShowPageNum }, CommunicationModel.AppProtocolCommand.ChangePage);
                    break;
                case ShowPage.ThyroidScanning:
                    CloseWinEnable = false;
                    mw.frameNav.NavigationService.Navigate(ts);
                    cm.SendCmd(CommunicationModel.TCPProtocolKey.NormalData, new byte[] { (byte)ShowPageNum }, CommunicationModel.AppProtocolCommand.ChangePage);
                    break;

                default:
                    CloseWinEnable = true;
                    mw.frameNav.NavigationService.Navigate(mp);
                    cm.SendCmd(CommunicationModel.TCPProtocolKey.NormalData, new byte[] { (byte)ShowPageNum }, CommunicationModel.AppProtocolCommand.ChangePage);
                    break;
            }
        }

        /// <summary>
        /// 页导航
        /// </summary>
        /// <param name="showPage">要显示的页</param>
        public void NavigateToPageLocal(int showPage)
        {
            ShowPage ShowPageNum = (ShowPage)showPage;

            mw.Dispatcher.BeginInvoke(new Action(() =>
            {
                switch (ShowPageNum)
                {
                    case ShowPage.MainNav:
                        CloseWinEnable = true;
                        if (mw.frameNav.NavigationService.CanGoBack) mw.frameNav.NavigationService.GoBack();
                        break;
                    case ShowPage.BaseControl:
                        CloseWinEnable = false;
                        mw.frameNav.NavigationService.Navigate(bc);
                        break;

                    case ShowPage.GalactophoreDetect:
                        CloseWinEnable = false;
                        mw.frameNav.NavigationService.Navigate(gd);
                         break;
                     case ShowPage.ThyroidScanning:
                        CloseWinEnable = false;
                        mw.frameNav.NavigationService.Navigate(ts);
                        break;
                    default:
                        CloseWinEnable = true;
                        mw.frameNav.NavigationService.Navigate(mp);
                        break;
                }
            }));
        }

        /// <summary>
        /// 主窗口分支弹窗
        /// </summary>
        /// <param name="message">消息</param>
        /// <param name="title">抬头</param>
        /// <param name="showStartAnimation">显示入场动画</param>
        /// <param name="showEndAnimation">显示退场动画</param>
        /// <returns>返回bool值，指示是否点击确定或者可以弹窗</returns>
        public async Task<bool> ShowBranchDialog(string message, string title, bool showStartAnimation = true, bool showEndAnimation = true)
        {
            var mySettings = new MetroDialogSettings()
            {
                AnimateShow = showStartAnimation,
                AnimateHide = showEndAnimation,
                AffirmativeButtonText = "是",
                NegativeButtonText = "否",
                DialogTitleFontSize = titleSize,
                DialogMessageFontSize = messageSize,
                ColorScheme = MetroDialogColorScheme.Theme
            };

            if (mw.CheckAccess())
            {
                MessageDialogResult result = await mw.ShowMessageAsync(title, message, MessageDialogStyle.AffirmativeAndNegative, mySettings);
                return result == MessageDialogResult.Affirmative;
            }

            return false;
        }

        public delegate void ShowBranchDialogDelegate(string message, string title, DealBranchDialogDelegate dealFunction, bool showStartAnimation, bool showEndAnimation);
        public delegate void DealBranchDialogDelegate(bool messageResult);
        private async void ShowBranchDialogTask(string message, string title, DealBranchDialogDelegate dealFunction, bool showStartAnimation = true, bool showEndAnimation = true)
        {
            bool result = await ShowBranchDialog(message, title, showStartAnimation, showEndAnimation);
            dealFunction(result);
        }

        /// <summary>
        /// 主窗口分支弹窗，切换到UI线程运行
        /// </summary>
        /// <param name="message">消息</param>
        /// <param name="title">抬头</param>
        /// <param name="dealFunction">信息获取后的回调函数</param>
        /// <param name="showStartAnimation">显示入场动画</param>
        /// <param name="showEndAnimation">显示退场动画</param>
        private void ShowBranchDialogAtUIThread(string message, string title, DealBranchDialogDelegate dealFunction, bool showStartAnimation = true, bool showEndAnimation = true)
        {
            mw.Dispatcher.BeginInvoke(
                new ShowBranchDialogDelegate(ShowBranchDialogTask),
                DispatcherPriority.Normal,
                new object[] { message, title, dealFunction, showStartAnimation, showEndAnimation });
        }

        /// <summary>
        /// 主窗口弹窗
        /// </summary>
        /// <param name="message">消息</param>
        /// <param name="title">抬头</param>
        /// <param name="occupyNum">控制位</param>
        /// <param name="showStartAnimation">显示入场动画</param>
        /// <param name="showEndAnimation">显示退场动画</param>
        /// <returns>返回bool值，指示是否已经点击确定</returns>
        public async Task<bool> ShowDialog(string message, string title, int occupyNum, bool showStartAnimation = true, bool showEndAnimation = true)
        {
            if (occupyArray[occupyNum]) return false;
            occupyArray[occupyNum] = true;

            var mySettings = new MetroDialogSettings()
            {
                AnimateShow = showStartAnimation,
                AnimateHide = showEndAnimation,
                AffirmativeButtonText = "确认",
                DialogTitleFontSize = titleSize,
                DialogMessageFontSize = messageSize,
                ColorScheme = MetroDialogColorScheme.Theme
            };

            if (mw.CheckAccess())
            {
                await mw.ShowMessageAsync(title, message, MessageDialogStyle.Affirmative, mySettings);
            }

            occupyArray[occupyNum] = false;
            return true;
        }

        public delegate void ShowDialogDelegate(string message, string title, int occupyNum, bool showStartAnimation = true, bool showEndAnimation = true);
        private void ShowDialogTask(string message, string title, int occupyNum, bool showStartAnimation = true, bool showEndAnimation = true)
        {
            ShowDialog(message, title, occupyNum, showStartAnimation, showEndAnimation);
        }

        /// <summary>
        /// 主窗口弹窗，切换到UI线程运行
        /// </summary>
        /// <param name="message">消息</param>
        /// <param name="title">抬头</param>
        /// <param name="occupyNum">控制位</param>
        /// <param name="showStartAnimation">显示入场动画</param>
        /// <param name="showEndAnimation">显示退场动画</param>
        private void ShowDialogAtUIThread(string message, string title, int occupyNum, bool showStartAnimation = true, bool showEndAnimation = true)
        {
            mw.Dispatcher.BeginInvoke(
                new ShowDialogDelegate(ShowDialogTask),
                DispatcherPriority.Normal,
                new object[] { message, title, occupyNum, showStartAnimation, showEndAnimation });
        }

        /// <summary>
        /// 基本运动参数设置
        /// </summary>
        /// <param name="cordinateTool">是否以工具为参考</param>
        /// <param name="moveSpeed">移动速度</param>
        public void BaseMovingParameterSet(bool cordinateTool, double moveSpeed)
        {
            BaseMoveCordinate = cordinateTool;
            BaseMoveSpeedRatio = moveSpeed;
        }

        /// <summary>
        /// 基本运动参数发送
        /// </summary>
        private void BaseMovingParameterSend()
        {
            cm.SendCmd(CommunicationModel.TCPProtocolKey.NormalData,
               new byte[] { Convert.ToByte(baseMoveCordinate ? 1 : 0) },
               CommunicationModel.AppProtocolCommand.MoveReference);

            cm.SendCmd(CommunicationModel.TCPProtocolKey.NormalData,
                    BitConverter.GetBytes(
                    IPAddress.HostToNetworkOrder(
                    BitConverter.ToInt32(
                    BitConverter.GetBytes(
                    Convert.ToSingle(baseMoveSpeedRatio)), 0))),
                    CommunicationModel.AppProtocolCommand.MoveSpeed);
        }

        /// <summary>
        /// 基本平移开始
        /// </summary>
        /// <param name="Axis">移动轴</param>
        /// <param name="IfPositive">移动方向</param>
        public void BaseMovingTranslationBegin(char Axis, bool IfPositive)
        {
            if (baseMoveSpeedRatio < Double.Epsilon * 10.0) return;

            BaseMovingParameterSend();

            cm.SendCmd(CommunicationModel.TCPProtocolKey.NormalData,
                                    new byte[] {Convert.ToByte(IfPositive? 1:0),
                                                        Convert.ToByte(0),
                                                        Convert.ToByte(Axis=='z'? 2:Axis=='y'?1:0)},
                                    CommunicationModel.AppProtocolCommand.MoveTcp);
        }

        /// <summary>
        /// 基本旋转开始
        /// </summary>
        /// <param name="Axis">旋转轴</param>
        /// <param name="IfPositive">旋转方向</param>
        public void BaseMovingSpinBegin(char Axis, bool IfPositive)
        {
            if (baseMoveSpeedRatio < Double.Epsilon * 10.0) return;

            BaseMovingParameterSend();

            cm.SendCmd(CommunicationModel.TCPProtocolKey.NormalData,
                                     new byte[] {Convert.ToByte(IfPositive? 1:0),
                                                        Convert.ToByte(1),
                                                        Convert.ToByte(Axis=='z'? 2:Axis=='y'?1:0)},
                                     CommunicationModel.AppProtocolCommand.MoveTcp);
        }

        /// <summary>
        /// 基本单轴移动开始
        /// </summary>
        /// <param name="Axis">旋转轴</param>
        /// <param name="IfPositive">旋转方向</param>
        public void BaseMovingSingleSpinBegin(char Axis, bool IfPositive)
        {
            if (baseMoveSpeedRatio < Double.Epsilon * 10.0) return;

            BaseMovingParameterSend();

            cm.SendCmd(CommunicationModel.TCPProtocolKey.NormalData,
                                    new byte[] {Convert.ToByte(IfPositive? 1:0),
                                                        byte.Parse(Convert.ToString(Axis))},
                                    CommunicationModel.AppProtocolCommand.MoveJoint);
        }

        /// <summary>
        /// 基本运动停止
        /// </summary>
        public void BaseMovingEnd()
        {
            cm.SendCmd(CommunicationModel.TCPProtocolKey.NormalData, null, CommunicationModel.AppProtocolCommand.MoveStop);
        }

        /// <summary>
        /// 反驱示教模式启停
        /// </summary>
        /// <param name="SwitchMode">模式开关</param>
        public void TeachModeTurn(bool SwitchMode = true)
        {// Remote禁止
        }

        #region GalactophoreDetect

        /// <summary>
        /// 切换乳腺扫查配置窗口
        /// </summary>
        public void SwitchGalactophoreOwnConf()
        {
            bool nowState = (mw.Flyouts.Items[(int)ConfPage.GalactophoreDetect] as Flyout).IsOpen;
            (mw.Flyouts.Items[(int)ConfPage.GalactophoreDetect] as Flyout).IsOpen = !nowState;
        }

        /// <summary>
        /// 进入乳腺扫查模块
        /// </summary>
        public void EnterGalactophoreDetectModule()
        {
            Task.Run(new Action(() =>
            {
                cm.SendCmd(CommunicationModel.TCPProtocolKey.NormalData, null,
                    CommunicationModel.AppProtocolCommand.EnterBreastScanMode);
            }));
        }

        /// <summary>
        /// 退出乳腺扫查模块
        /// </summary>
        public void ExitGalactophoreDetectModule()
        {
            Task.Run(new Action(() =>
            {
                cm.SendCmd(CommunicationModel.TCPProtocolKey.NormalData, null,
                    CommunicationModel.AppProtocolCommand.ExitBreastScanMode);
            }));
        }

        /// <summary>
        /// 乳腺扫查模块清零力传感器
        /// </summary>
        public void ForceClearGalactophoreDetectModule()
        {
            Task.Run(new Action(() =>
            {
                cm.SendCmd(CommunicationModel.TCPProtocolKey.NormalData, null,
                                    CommunicationModel.AppProtocolCommand.BreastScanModeBeginForceZeroed);
            }));
        }

        /// <summary>
        /// 乳腺扫查模块配置参数
        /// </summary>
        public void ConfParamsGalactophoreDetectModule()
        {
            Task.Run(new Action(() =>
            {
                cm.SendCmd(CommunicationModel.TCPProtocolKey.NormalData, null,
                                    CommunicationModel.AppProtocolCommand.BreastScanModeBeginConfigurationSet);
            }));
        }

        /// <summary>
        /// 乳腺扫查模块寻找乳头
        /// </summary>
        public void NippleFindGalactophoreDetectModule()
        {
            Task.Run(new Action(() =>
            {
                SaveCachePos();
                BreastScanConfMovingEnable = true;
            }));
        }

        /// <summary>
        /// 乳腺扫查模块找到乳头
        /// </summary>
        public void NippleFoundGalactophoreDetectModule()
        {
            Task.Run(new Action(() =>
            {
                BreastScanConfMovingEnable = false;
                cm.SendCmd(CommunicationModel.TCPProtocolKey.NormalData, null,
                    CommunicationModel.AppProtocolCommand.BreastScanModeConfirmNipplePos);

                Thread.Sleep(100);
                NipplePositionGDR = new double[] { ToolTCPCordinateX, ToolTCPCordinateY, ToolTCPCordinateZ };
                //SaveCachePos(nippleNow);
            }));
        }

        /// <summary>
        /// 乳腺扫查模块寻找抬升距离
        /// </summary>
        public void LiftDistanceFindGalactophoreDetectModule()
        {
            Task.Run(new Action(() =>
            {
                SaveCachePos();
                BreastScanConfMovingEnable = true;
            }));
        }

        /// <summary>
        /// 乳腺扫查模块找到抬升距离
        /// </summary>
        public void LiftDistanceFoundGalactophoreDetectModule()
        {
            Task.Run(new Action(() =>
            {
                BreastScanConfMovingEnable = false;
                Thread.Sleep(100);
                double distanceBias = Math.Abs(ToolTCPCordinateZ - posCacheNow[2]);
                DetectingSafetyLiftDistanceGDR = distanceBias;

                Thread.Sleep(100);
                SaveConfParameters(ConfPage.GalactophoreDetect);
                //SaveCachePos(posNow);
            }));
        }

        /// <summary>
        /// 乳腺扫查模块寻找最小半径
        /// </summary>
        public void MinRadiusFindGalactophoreDetectModule()
        {
            Task.Run(new Action(() =>
            {
                SaveCachePos();
                BreastScanConfMovingEnable = true;
            }));
        }

        /// <summary>
        /// 乳腺扫查模块找到最小半径
        /// </summary>
        public void MinRadiusFoundGalactophoreDetectModule()
        {
            Task.Run(new Action(() =>
            {
                BreastScanConfMovingEnable = false;
                Thread.Sleep(100);
                double distanceBias = Math.Sqrt(Math.Pow(ToolTCPCordinateX - posCacheNow[0], 2) + Math.Pow(ToolTCPCordinateY - posCacheNow[1], 2));
                NippleForbiddenRadiusGDR = distanceBias;

                Thread.Sleep(100);
                SaveConfParameters(ConfPage.GalactophoreDetect);
                //SaveCachePos(posNow);
            }));
        }

        /// <summary>
        /// 乳腺扫查模块寻找探测深度
        /// </summary>
        public void ScanDeepFindGalactophoreDetectModule()
        {
            Task.Run(new Action(() =>
            {
                SaveCachePos();
                BreastScanConfMovingEnable = true;
            }));
        }

        /// <summary>
        /// 乳腺扫查模块找到探测深度
        /// </summary>
        public void ScanDeepFoundGalactophoreDetectModule()
        {
            Task.Run(new Action(() =>
            {
                BreastScanConfMovingEnable = false;
                Thread.Sleep(100);
                double distanceBias = Math.Abs(ToolTCPCordinateZ - posCacheNow[2]);
                DetectingStopDistanceGDR = distanceBias;

                Thread.Sleep(100);
                SaveConfParameters(ConfPage.GalactophoreDetect);
                //SaveCachePos(posNow);
            }));
        }

        /// <summary>
        /// 乳腺扫查模块寻找四侧距离
        /// </summary>
        /// <param name="Side">四侧距离指示，"head"=头侧，"tail"=尾侧，"out"=外侧，"in"=内侧</param>
        /// <returns>返回是否执行操作</returns>
        public bool BoundFindGalactophoreDetectModule(string Side)
        {
            bool givenBackResult = false;
            if (Side == "head") givenBackResult = true;
            if (Side == "tail" && identifyEdgeModeGDR != GalactophoreDetector.IdentifyBoundary.OnlyUpBoundary) givenBackResult = true;
            if (Side == "out" && identifyEdgeModeGDR == GalactophoreDetector.IdentifyBoundary.AllBoundary) givenBackResult = true;
            if (Side == "in" && identifyEdgeModeGDR == GalactophoreDetector.IdentifyBoundary.AllBoundary) givenBackResult = true;

            if (givenBackResult)
            {
                Task.Run(new Action(() =>
                {
                    SaveCachePos();
                    BreastScanConfMovingEnable = true;
                }));
            }

            return givenBackResult;
        }

        /// <summary>
        /// 乳腺扫查模块找到四侧距离
        /// </summary>
        /// <param name="Side">四侧距离指示，"head"=头侧，"tail"=尾侧，"out"=外侧，"in"=内侧</param>
        public void BoundFoundGalactophoreDetectModule(string Side)
        {
            Task.Run(new Action(() =>
            {
                BreastScanConfMovingEnable = false;
                Thread.Sleep(100);
                double distanceBias = Math.Sqrt(Math.Pow(ToolTCPCordinateX - posCacheNow[0], 2) + Math.Pow(ToolTCPCordinateY - posCacheNow[1], 2));
                if (Side == "tail") MovingDownEdgeDistanceGDR = distanceBias;
                else if (Side == "out") MovingLeftEdgeDistanceGDR = distanceBias;
                else if (Side == "in") MovingRightEdgeDistanceGDR = distanceBias;
                else MovingUpEdgeDistanceGDR = distanceBias;

                Thread.Sleep(100);
                SaveConfParameters(ConfPage.GalactophoreDetect);
                //SaveCachePos(posNow);
            }));
        }

        /// <summary>
        /// 乳腺扫查模块寻找下沉距离
        /// </summary>
        /// <returns>返回执行Task</returns>
        //public async void SinkDeepFindGalactophoreDetectModule()
        //{
        //    if (!ifEnableDetectingForceCheckGDR) return;

        //    if (ifCheckingSinkDistance) return;
        //    ifCheckingSinkDistance = true;

        //    SaveCachePos();
        //    double radius = Double.Parse(gd.minRadius.Text.Trim()) / 1000.0;

        //    //await gdr.ScanForceCheck(radius, DetectingForceDegreeGDR);

        //    double[] posNow = urdp.PositionsTcpActual;
        //    double distanceBias = Math.Abs(posNow[2] - nipplePositionGDR[2]);
        //    DetectingSinkDistanceGDR = distanceBias;
        //    //SaveCachePos(posNow);

        //    ifCheckingSinkDistance = false;
        //}

        /// <summary>
        /// 乳腺扫查模块确认配置参数
        /// </summary>
        public void ConfirmConfParamsGalactophoreDetectModule()
        {
            Task.Run(new Action(() =>
            {
                cm.SendCmd(CommunicationModel.TCPProtocolKey.NormalData, PickParametersFormView(ConfPage.GalactophoreDetect).ToArray(), CommunicationModel.AppProtocolCommand.BreastScanModeConfirmConfigurationSet);
            }));
        }

        /// <summary>
        /// 乳腺扫查模块准备并开始
        /// </summary>
        public void ReadyAndStartGalactophoreDetectModule()
        {
            Task.Run(new Action(() =>
            {
                cm.SendCmd(CommunicationModel.TCPProtocolKey.NormalData, null,
                    CommunicationModel.AppProtocolCommand.BreastScanModeReadyAndStartBreastScan);
            }));
        }

        /// <summary>
        /// 立即停止所有乳腺扫查模块中的活动
        /// </summary>
        public async void StopMotionNowGalactophoreDetectModule()
        {
            Task.Run(new Action(() =>
            {
                cm.SendCmd(CommunicationModel.TCPProtocolKey.NormalData, null,
                    CommunicationModel.AppProtocolCommand.StopBreastScanImmediately);
                Logger.HistoryPrinting(Logger.Level.WARN, MethodBase.GetCurrentMethod().DeclaringType.FullName, "Galactophore scanning module is stopped immediately.");
            }));

            await ShowDialog("乳腺扫查模块被紧急停止，请按下确定恢复控制权！", "紧急状态", 7);

            Task.Run(new Action(() =>
            {
                cm.SendCmd(CommunicationModel.TCPProtocolKey.NormalData, null,
                    CommunicationModel.AppProtocolCommand.RecoveryFromStopBreastScanImmediately);
                Logger.HistoryPrinting(Logger.Level.INFO, MethodBase.GetCurrentMethod().DeclaringType.FullName, "Galactophore scanning module is recoveried from stopping immediately.");
            }));
        }

        /// <summary>
        /// 乳腺扫查模块转到下一个配置参数
        /// </summary>
        public void ConfParamsNextParamsGalactophoreDetectModule()
        {
            Task.Run(new Action(() =>
            {
                SaveConfParameters(ConfPage.GalactophoreDetect);

                cm.SendCmd(CommunicationModel.TCPProtocolKey.NormalData, null,
                    CommunicationModel.AppProtocolCommand.BreastScanModeNextConfigurationItem);
            }));
        }

        #endregion

        #region ThyroidScan
        /// <summary>
        /// 切换甲状腺扫查配置窗口
        /// </summary>
        public void SwitchThyroidOwnConf()
        {
            bool nowState = (mw.Flyouts.Items[(int)ConfPage.ThyroidScan] as Flyout).IsOpen;
            (mw.Flyouts.Items[(int)ConfPage.ThyroidScan] as Flyout).IsOpen = !nowState;
        }

        /// <summary>
        /// 进入甲状腺扫查模块
        /// </summary>
        public void EnterThyroidScanningModule()
        {
            Task.Run(new Action(() =>
            {
                cm.SendCmd(CommunicationModel.TCPProtocolKey.NormalData, null,
                    CommunicationModel.AppProtocolCommand.EnterBreastScanMode);
            }));
        }

        /// <summary>
        /// 退出甲状腺扫查模块
        /// </summary>
        public void ExitThyroidScanningModule()
        {
            Task.Run(new Action(() =>
            {
                cm.SendCmd(CommunicationModel.TCPProtocolKey.NormalData, null,
                    CommunicationModel.AppProtocolCommand.ExitBreastScanMode);
            }));
        }

        /// <summary>
        /// 甲状腺扫查模块清零力传感器
        /// </summary>
        public void ForceClearThyroidScanningModule()
        {
            Task.Run(new Action(() =>
            {
               cm.SendCmd(CommunicationModel.TCPProtocolKey.NormalData, null,
                                    CommunicationModel.AppProtocolCommand.BreastScanModeBeginForceZeroed);
            }));
        }

        /// <summary>
        /// 甲状腺扫查模块配置参数
        /// </summary>
        public void ConfParamsThyroidScanningModule()
        {
            Task.Run(new Action(() =>
            {
                cm.SendCmd(CommunicationModel.TCPProtocolKey.NormalData, null,
                                    CommunicationModel.AppProtocolCommand.BreastScanModeBeginConfigurationSet);
            }));
        }

        /// <summary>
        /// 甲状腺扫查模块寻找起始位置
        /// </summary>
        public void StartPositionFindThyroidScanningModule()
        {
            Task.Run(new Action(() =>
            {
                SaveCachePos();
                ThyroidScanConfMovingEnable = true;
            }));
        }

        /// <summary>
        /// 甲状腺扫查模块找到起始位置
        /// </summary>
        public void StartPositionFoundThyroidScanningModule()
        {
            Task.Run(new Action(() =>
            {
                 ThyroidScanConfMovingEnable = false;
                cm.SendCmd(CommunicationModel.TCPProtocolKey.NormalData, null,
                    CommunicationModel.AppProtocolCommand.BreastScanModeConfirmNipplePos);

                Thread.Sleep(100);
                StartPositionTSR = new double[] { ToolTCPCordinateX, ToolTCPCordinateY, ToolTCPCordinateZ };

            }));
        }

        /// <summary>
        /// 甲状腺扫查模块确认配置参数
        /// </summary>
        public void ConfirmConfParamsThyroidScanningModule()
        {
            Task.Run(new Action(() =>
            {
                cm.SendCmd(CommunicationModel.TCPProtocolKey.NormalData, PickParametersFormView(ConfPage.ThyroidScan).ToArray(), CommunicationModel.AppProtocolCommand.BreastScanModeConfirmConfigurationSet);
            }));
        }

        /// <summary>
        /// 甲状腺扫查模块准备并开始
        /// </summary>
        public void ReadyAndStartThyroidScanningModule()
        {
            Task.Run(new Action(() =>
            {
                cm.SendCmd(CommunicationModel.TCPProtocolKey.NormalData, null,
                    CommunicationModel.AppProtocolCommand.BreastScanModeReadyAndStartBreastScan);
            }));
        }

        /// <summary>
        /// 立即停止所有甲状腺扫查模块中的活动
        /// </summary>
        public async void StopMotionNowThyroidScanningModule()
        {
            Task.Run(new Action(() =>
            {
                cm.SendCmd(CommunicationModel.TCPProtocolKey.NormalData, null,
                    CommunicationModel.AppProtocolCommand.StopBreastScanImmediately);
                Logger.HistoryPrinting(Logger.Level.WARN, MethodBase.GetCurrentMethod().DeclaringType.FullName, "Thyroid scanning module is stopped immediately.");
            }));

            await ShowDialog("甲状腺扫查模块被紧急停止，请按下确定恢复控制权！", "紧急状态", 7);

            Task.Run(new Action(() =>
            {
                cm.SendCmd(CommunicationModel.TCPProtocolKey.NormalData, null,
                    CommunicationModel.AppProtocolCommand.RecoveryFromStopBreastScanImmediately);
                Logger.HistoryPrinting(Logger.Level.INFO, MethodBase.GetCurrentMethod().DeclaringType.FullName, "Thyroid scanning module is recoveried from stopping immediately.");
            }));
        }

        /// <summary>
        /// 甲状腺扫查模块转到下一个配置参数
        /// </summary>
        public void ConfParamsNextParamsThyroidScannerModule()
        {
            Task.Run(new Action(() =>
            {
                SaveConfParameters(ConfPage.ThyroidScan);

                cm.SendCmd(CommunicationModel.TCPProtocolKey.NormalData, null,
                    CommunicationModel.AppProtocolCommand.BreastScanModeNextConfigurationItem);
            }));
        }

        /// <summary>
        /// 在模块工作中更改部分控制变量
        /// </summary>
        public void ModifyControlParametersInRunWorkThyroidScannerModule()
        {
            Task.Run(new Action(() =>
            {

            
            
            
            }));
        }
        #endregion

        #region SaveModuleConfParams

        /// <summary>
        /// 保存该页配置
        /// </summary>
        /// <param name="modifyPage">修改的页</param>
        public void SaveConfParameters(ConfPage modifyPage)
        {
            switch (modifyPage)
            {
                case ConfPage.GalactophoreDetect:
                    cm.SendCmd(CommunicationModel.TCPProtocolKey.NormalData, PickParametersFormView(modifyPage).ToArray(), CommunicationModel.AppProtocolCommand.BreastScanModeSaveConfigurationSet);
                    break;

                case ConfPage.ThyroidScan:
                    cm.SendCmd(CommunicationModel.TCPProtocolKey.NormalData, PickParametersFormView(modifyPage).ToArray(), CommunicationModel.AppProtocolCommand.BreastScanModeSaveConfigurationSet);
                    break;

                default:
                    break;
            }
        }

        /// <summary>
        /// 从相应页获取配置参数
        /// </summary>
        /// <param name="modifyPage">修改的页</param>
        /// <returns>返回配置参数</returns>
        public List<byte> PickParametersFormView(ConfPage modifyPage)
        {
            switch (modifyPage)
            {
                case ConfPage.GalactophoreDetect:
                    if (mw.CheckAccess())
                        return PickParameterFromBreastScan();
                    else
                    {
                        DispatcherOperation pickOperation = mw.Dispatcher.BeginInvoke(new Func<List<byte>>(PickParameterFromBreastScan));
                        pickOperation.Wait();
                        return (List<byte>)pickOperation.Result;
                    }
                case ConfPage.ThyroidScan:
                    if (mw.CheckAccess())
                        return PickParameterFromThyroidScan();
                    else
                    {
                        DispatcherOperation pickOperation = mw.Dispatcher.BeginInvoke(new Func<List<byte>>(PickParameterFromThyroidScan));
                        pickOperation.Wait();
                        return (List<byte>)pickOperation.Result;
                    }
                default:
                    return null;
            }
        }

        /// <summary>
        /// 从乳腺扫描界面获取数据
        /// </summary>
        /// <returns>返回配置参数</returns>
        private List<byte> PickParameterFromBreastScan()
        {
            List<byte> returnConf = new List<byte>(100);

            returnConf.AddRange(
                BitConverter.GetBytes(
                IPAddress.HostToNetworkOrder(
                BitConverter.ToInt32(
                BitConverter.GetBytes(
                float.Parse(
                (string)mw.minForceText.Content)), 0))));
            returnConf.AddRange(
                BitConverter.GetBytes(
                IPAddress.HostToNetworkOrder(
                BitConverter.ToInt32(
                BitConverter.GetBytes(
                float.Parse(
                (string)mw.maxForceText.Content)), 0))));
            returnConf.AddRange(
                BitConverter.GetBytes(
                IPAddress.HostToNetworkOrder(
                BitConverter.ToInt32(
                BitConverter.GetBytes(
                Convert.ToSingle(
                double.Parse((string)mw.minDetectSpeedText.Content) / 1000.0)), 0))));

            returnConf.Add(Convert.ToByte(mw.ARectifySwitch.IsChecked.Value ? 1 : 0));

            returnConf.AddRange(
                BitConverter.GetBytes(
                IPAddress.HostToNetworkOrder(
                BitConverter.ToInt32(
                BitConverter.GetBytes(
                Convert.ToSingle(
                double.Parse(gd.minRadius.Text.Trim()) / 1000.0)), 0))));

            returnConf.AddRange(
                BitConverter.GetBytes(
                IPAddress.HostToNetworkOrder(
                BitConverter.ToInt32(
                BitConverter.GetBytes(
                Convert.ToSingle(
                double.Parse(gd.scanDistance.Text.Trim()) / 1000.0)), 0))));
            returnConf.AddRange(
                BitConverter.GetBytes(
                IPAddress.HostToNetworkOrder(
                BitConverter.ToInt32(
                BitConverter.GetBytes(
                Convert.ToSingle(
                double.Parse(gd.liftDistance.Text.Trim()) / 1000.0)), 0))));

            returnConf.Add(Convert.ToByte(mw.IACheckSwitch.IsChecked.Value ? 1 : 0));

            returnConf.AddRange( // 占位
                BitConverter.GetBytes(
                IPAddress.HostToNetworkOrder(
                BitConverter.ToInt32(
                BitConverter.GetBytes(
                Convert.ToSingle(-2.0)), 0))));

            returnConf.Add(Convert.ToByte(Math.Round(mw.vibrateDegreeSlider.Value)));
            returnConf.Add(Convert.ToByte(Math.Round(mw.speedDegreeSlider.Value)));
            returnConf.Add(Convert.ToByte(Math.Round(mw.forceDegreeSlider.Value)));
            returnConf.Add(Convert.ToByte(mw.attachSwitch.IsChecked.Value ? 1 : 0));

            returnConf.AddRange(
                BitConverter.GetBytes(
                IPAddress.HostToNetworkOrder(
                BitConverter.ToInt32(
                BitConverter.GetBytes(
                Convert.ToSingle(
                double.Parse(gd.headBound.Text.Trim()) / 1000.0)), 0))));
            returnConf.AddRange(
               BitConverter.GetBytes(
               IPAddress.HostToNetworkOrder(
               BitConverter.ToInt32(
               BitConverter.GetBytes(
               Convert.ToSingle(
               double.Parse(gd.outBound.Text.Trim()) / 1000.0)), 0))));
            returnConf.AddRange(
               BitConverter.GetBytes(
               IPAddress.HostToNetworkOrder(
               BitConverter.ToInt32(
               BitConverter.GetBytes(
               Convert.ToSingle(
               double.Parse(gd.tailBound.Text.Trim()) / 1000.0)), 0))));
            returnConf.AddRange(
               BitConverter.GetBytes(
               IPAddress.HostToNetworkOrder(
               BitConverter.ToInt32(
               BitConverter.GetBytes(
               Convert.ToSingle(
               double.Parse(gd.inBound.Text.Trim()) / 1000.0)), 0))));

            returnConf.Add(Convert.ToByte(mw.autoSaveSwitch.IsChecked.Value ? 1 : 0));
            returnConf.Add(Convert.ToByte(mw.galactophoreDirectionSwitch.IsChecked.Value ? 1 : 0));
            returnConf.Add(Convert.ToByte(Math.Round(mw.borderModeSlider.Value)));

            returnConf.AddRange(
               BitConverter.GetBytes(
               IPAddress.HostToNetworkOrder(
               BitConverter.ToInt32(
               BitConverter.GetBytes(
               Convert.ToSingle(
               Math.PI / 180.0 * double.Parse((string)mw.rotateStepText.Content))), 0))));

            return returnConf;
        }
        
        /// <summary>
        /// 从甲状腺扫描界面获取数据
        /// </summary>
        /// <returns>返回配置参数</returns>
        private List<byte> PickParameterFromThyroidScan()
        {
            List<byte> returnConf = new List<byte>(100);

            returnConf.AddRange(
                BitConverter.GetBytes(
                IPAddress.HostToNetworkOrder(
                BitConverter.ToInt32(
                BitConverter.GetBytes(
                float.Parse(
                (string)mw.minForceSliderThyroidText.Content)), 0))));
            returnConf.AddRange(
                BitConverter.GetBytes(
                IPAddress.HostToNetworkOrder(
                BitConverter.ToInt32(
                BitConverter.GetBytes(
                float.Parse(
                (string)mw.maxForceSliderThyroidText.Content)), 0))));

            returnConf.AddRange(
                BitConverter.GetBytes(
                IPAddress.HostToNetworkOrder(
                BitConverter.ToInt32(
                BitConverter.GetBytes(
                Convert.ToSingle(
                double.Parse((string)mw.minDetectSpeedSliderThyroidText.Content) / 1000.0)), 0))));
            returnConf.AddRange(
                BitConverter.GetBytes(
                IPAddress.HostToNetworkOrder(
                BitConverter.ToInt32(
                BitConverter.GetBytes(
                Convert.ToSingle(
                double.Parse((string)mw.maxDetectSpeedSliderThyroidText.Content) / 1000.0)), 0))));

            returnConf.Add(Convert.ToByte(ts.enableFosKeepSwitch.IsChecked.Value ? 1 : 0));
            returnConf.Add(Convert.ToByte(ts.enableFosTrackSwitch.IsChecked.Value ? 1 : 0));

            returnConf.AddRange(
                BitConverter.GetBytes(
                IPAddress.HostToNetworkOrder(
                BitConverter.ToInt32(
                BitConverter.GetBytes(
                float.Parse(
                (string)mw.holdingPressureThyroidText.Content)), 0))));

            returnConf.AddRange(
                BitConverter.GetBytes(
                IPAddress.HostToNetworkOrder(
                BitConverter.ToInt32(
                BitConverter.GetBytes(
                Convert.ToSingle(
                double.Parse((string)mw.maxRadiusThyroidText.Content) / 1000.0)), 0))));
            returnConf.AddRange(
                BitConverter.GetBytes(
                IPAddress.HostToNetworkOrder(
                BitConverter.ToInt32(
                BitConverter.GetBytes(
                Convert.ToSingle(
                double.Parse((string)mw.maxAngleThyroidText.Content) / 180.0 * Math.PI)), 0))));

            returnConf.AddRange(
                BitConverter.GetBytes(
                IPAddress.HostToNetworkOrder(
                BitConverter.ToInt32(
                BitConverter.GetBytes(
                Convert.ToSingle(
                double.Parse((string)mw.stopDistanceThyroidText.Content) / 1000.0)), 0))));
            returnConf.AddRange(
                BitConverter.GetBytes(
                IPAddress.HostToNetworkOrder(
                BitConverter.ToInt32(
                BitConverter.GetBytes(
                Convert.ToSingle(
                double.Parse((string)mw.maxLoopDistThyroidText.Content) / 1000.0)), 0))));
            returnConf.AddRange(
                BitConverter.GetBytes(
                IPAddress.HostToNetworkOrder(
                BitConverter.ToInt32(
                BitConverter.GetBytes(
                Convert.ToSingle(
                double.Parse((string)mw.maxLoopAngleThyroidText.Content) / 180.0 * Math.PI)), 0))));

            returnConf.AddRange(
                BitConverter.GetBytes(
                IPAddress.HostToNetworkOrder(
                BitConverter.ToInt32(
                BitConverter.GetBytes(
                Convert.ToSingle(
                double.Parse(ts.factorPos.Text) / 100.0)), 0))));
            returnConf.AddRange(
                BitConverter.GetBytes(
                IPAddress.HostToNetworkOrder(
                BitConverter.ToInt32(
                BitConverter.GetBytes(
                Convert.ToSingle(
                double.Parse(ts.factorAtt.Text) / 100.0)), 0))));
            returnConf.AddRange(
                BitConverter.GetBytes(
                IPAddress.HostToNetworkOrder(
                BitConverter.ToInt32(
                BitConverter.GetBytes(
                Convert.ToSingle(
                double.Parse(ts.factorFos.Text) / 100.0)), 0))));

            returnConf.Add(Convert.ToByte(ts.enableAttSwitch.IsChecked.Value ? 1 : 0));
            returnConf.Add(Convert.ToByte(ts.enablePosSwitch.IsChecked.Value ? 1 : 0));

            return returnConf;
        }
        #endregion



        /// <summary>
        /// 缓存当前位置
        /// </summary>
        private void SaveCachePos(double[] InputPos = null)
        {
            if (Object.Equals(InputPos, null))
            {
                posCacheNow[0] = ToolTCPCordinateX;
                posCacheNow[1] = ToolTCPCordinateY;
                posCacheNow[2] = ToolTCPCordinateZ;
                posCacheNow[3] = ToolTCPCordinateRX;
                posCacheNow[4] = ToolTCPCordinateRY;
                posCacheNow[5] = ToolTCPCordinateRZ;
            }
            else
            {
                for (int i = 0; i < 6; ++i)
                {
                    posCacheNow[i] = InputPos[i];
                }
            }
        }

        /// <summary>
        /// 直接关闭窗体
        /// </summary>
        public async void ImmediateCloseWin()
        {
            await Task.Delay(100);
            mw.Close();
        }

        /// <summary>
        /// 关闭Model逻辑
        /// </summary>
        public async void CloseModelLogic()
        {
            var mySettings = new MetroDialogSettings()
            {
                AffirmativeButtonText = "关闭程序",
                NegativeButtonText = " + 当地程序",
                DialogTitleFontSize = titleSize,
                DialogMessageFontSize = messageSize,
                CustomResourceDictionary = new ResourceDictionary(),
                ColorScheme = MetroDialogColorScheme.Theme
            };

            MessageDialogResult result = await mw.ShowMessageAsync("选择", "请选择想要的关闭方式！",
                MessageDialogStyle.AffirmativeAndNegative, mySettings);

            var controller = await mw.ShowProgressAsync("请稍后", "正在关闭程序。。。", settings: new MetroDialogSettings()
            {
                AnimateShow = false,
                AnimateHide = false,
                DialogTitleFontSize = titleSize,
                DialogMessageFontSize = messageSize,
                ColorScheme = MetroDialogColorScheme.Theme
            });

            controller.SetIndeterminate();

            bool closeResult = false;
            if (result == MessageDialogResult.Affirmative)
            {
                closeResult = cm.EndConnectionToServer();
                if (!closeResult)
                {
                    Logger.HistoryPrinting(Logger.Level.INFO, MethodBase.GetCurrentMethod().DeclaringType.FullName, "Remote connection has not been established when closing remote ui.");
                }
            }
            else if (result == MessageDialogResult.Negative)
            {
                closeResult = cm.EndConnectionToServer(true);
                if (!closeResult)
                {
                    Logger.HistoryPrinting(Logger.Level.WARN, MethodBase.GetCurrentMethod().DeclaringType.FullName, "Remote connection has not been established when closing both ui.");
                }
            }

            while (RemoteEnable)
            {
                await Task.Delay(500);
            }
            ClearAllEvents(cm);

            await controller.CloseAsync();
            await Task.Delay(200);
            mw.Close();
        }

        /// <summary>
        /// 清空事件绑定
        /// </summary>
        /// <param name="objectHasEvents">待清空的对象</param>
        private void ClearAllEvents(object objectHasEvents)
        {
            if (objectHasEvents == null)
            {
                return;
            }
            try
            {
                EventInfo[] events = objectHasEvents.GetType().GetEvents(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (events == null || events.Length < 1)
                {
                    return;
                }
                for (int i = 0; i < events.Length; i++)
                {
                    EventInfo ei = events[i];
                    FieldInfo fi = ei.DeclaringType.GetField(ei.Name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    if (fi != null)
                    {
                        fi.SetValue(objectHasEvents, null);
                    }
                }
            }
            catch
            {
                Logger.HistoryPrinting(Logger.Level.WARN, MethodBase.GetCurrentMethod().DeclaringType.FullName, "Error on disconnect events of object with type \'" + objectHasEvents.GetType().ToString() + "\'.");
            }
        }

        /// <summary>
        /// 连接Model服务器
        /// </summary>
        /// <returns>连接结果</returns>
        public int ConnectToModelServer()
        {
            return cm.ConnectToServer();
        }

        /// <summary>
        /// 断开Model服务器连接
        /// </summary>
        /// <param name="ifCloseLocalCtrl">是否一并关闭当地控制</param>
        /// <returns>断开结果</returns>
        public bool BreakFromModelServer(bool ifCloseLocalCtrl = false)
        {
            return cm.EndConnectionToServer(ifCloseLocalCtrl);
        }

        /// <summary>
        /// 打开动作传感器连接
        /// </summary>
        /// <returns>返回连接结果</returns>
        public bool StartActionSensorConnection()
        {
            if (gsd.ConnectToSensorServer())
            {
                ifActionSensorDataCatched = true;
                return true;
            }
            else
            {
                ifActionSensorDataCatched = false;
                return false;
            }
        }

        #endregion

        #region Preparation
        private readonly ConverterThatTransformDoubleToString convertD2S = new ConverterThatTransformDoubleToString();
        private readonly ValueProcesser valueP1000D0 = new ValueProcesser(1000.0, "0");
        private readonly ValueProcesser valueP1000D1 = new ValueProcesser(1000.0, "0.0");
        private readonly ValueProcesser valueP1000D2 = new ValueProcesser(1000.0, "0.00");
        private readonly ValueProcesser valueP1D2 = new ValueProcesser(1.0, "0.00");
        private readonly ValueProcesser valueP1D3 = new ValueProcesser(1.0, "0.000");
        private readonly ValueProcesser valueP1D4 = new ValueProcesser(1.0, "0.0000");
        private readonly ConverterThatTransformDoubleToDoubleSlider convertD2DSlider = new ConverterThatTransformDoubleToDoubleSlider();
        private const double radToDegRatio = 180.0 / Math.PI;
        private const double degToRadRatio = Math.PI / 180.0;
        private readonly ConverterThatTransformDoubleArrayToString convertDA2S = new ConverterThatTransformDoubleArrayToString();
        private readonly ConverterThatTransformDoubleToDoubleInteger convertD2DI = new ConverterThatTransformDoubleToDoubleInteger();
        private readonly ConverterThatTransformEnumToDouble convertE2D = new ConverterThatTransformEnumToDouble();
        private readonly ConverterThatTransformEnumToBool convertE2B = new ConverterThatTransformEnumToBool();
        private readonly ConverterMultiStatusToEnableBool convertMS2EB = new ConverterMultiStatusToEnableBool();
        private readonly ConverterMultiEnableToEnableAndLogicBool convertME2EALB = new ConverterMultiEnableToEnableAndLogicBool();
        private readonly ConverterMultiEnableToBackgroundAndLogicColor convertME2BKALC = new ConverterMultiEnableToBackgroundAndLogicColor();

        /// <summary>
        /// 绑定元素
        /// </summary>
        public void BindingItems()
        {
            BindingItemsCloseWindowEnable();
            BindingItemsGlobalControlsEnable();
            BindingItemsWindowCommandKindAndText();
            BindingItemsStatusBarContentAndColor();
            BindingItemsStatusBarRemoteContentAndColor();
            BindingItemsBaseMovingRefrenceCordinateAndMovingSpeedRatio();
            BindingItemsParametersNeededToShowOnWindowToolTCPCordinates();
            BindingItemsParametersNeededToShowOnWindowToolForceAndTorque();
            BindingItemsParametersNeededToShowOnWindowRobotJointsCurrents();
            BindingItemsParametersNeededToShowOnWindowRobotJointsAngles();

            BindingItemsNipplePositionAtGalactophoreDetecting();
            BindingItemsConfigurationParametersOfGalactophoreDetectorDetectingDirectionForceLimitsAndSpeedLimits();
            BindingItemsConfigurationParametersOfGalactophoreDetectorDetectingMotionLimits();
            BindingItemsConfigurationParametersOfGalactophoreDetectorDegreeControlParameters();
            BindingItemsConfigurationParametersOfGalactophoreDetectorDetectingEdge();
            BindingItemsConfigurationParametersOfGalactophoreDetectorOther();

            BindingItemsGalactophoreDetectorWorkingStatus();
            BindingItemsGalactophoreDetectorMoveEnable();

            BindingItemsStartPositionAtThyroidScanning();
            BindingItemsConfigurationParametersOfThyroidScannerPressDirectionParameters();
            BindingItemsConfigurationParametersOfThyroidScannerLimitParameters();
            BindingItemsConfigurationParametersOfThyroidScannerFactor();
            BindingItemsConfigurationParametersOfThyroidScannerOtherSwitch();

            BindingItemsThyroidScannerWorkingStatus();
            BindingItemsThyroidScannerMoveEnable();
        }

        #region SubBindingItems
        /// <summary>
        /// 绑定域 --| Close Window Enable |-- 内元素
        /// </summary>
        private void BindingItemsCloseWindowEnable()
        {
            // 绑定：CloseWinEnable {属性} ==> btnPowerOff {MainWindow控件}
            Binding bindingFromCloseWinEnableToBtnPowerOff = new Binding();
            bindingFromCloseWinEnableToBtnPowerOff.Source = this;
            bindingFromCloseWinEnableToBtnPowerOff.Path = new PropertyPath("CloseWinEnable");
            bindingFromCloseWinEnableToBtnPowerOff.Mode = BindingMode.OneWay;
            bindingFromCloseWinEnableToBtnPowerOff.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            BindingOperations.SetBinding(mw.btnPowerOff, Button.IsEnabledProperty, bindingFromCloseWinEnableToBtnPowerOff);
        }

        /// <summary>
        /// 绑定域 --| Global Controls Enable |-- 内元素
        /// </summary>
        private void BindingItemsGlobalControlsEnable()
        {
            Binding bindingFromLocalEnable = new Binding();
            bindingFromLocalEnable.Source = this;
            bindingFromLocalEnable.Path = new PropertyPath("LocalEnable");
            bindingFromLocalEnable.Mode = BindingMode.OneWay;
            bindingFromLocalEnable.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            Binding bindingFromRemoteEnable = new Binding();
            bindingFromRemoteEnable.Source = this;
            bindingFromRemoteEnable.Path = new PropertyPath("RemoteEnable");
            bindingFromRemoteEnable.Mode = BindingMode.OneWay;
            bindingFromRemoteEnable.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;

            // 绑定：LocalEnable | RemoteEnable {属性} ==> frameNav {MainWindow控件}
            MultiBinding mbindingToFrameNavOfMW = new MultiBinding();
            mbindingToFrameNavOfMW.Mode = BindingMode.OneWay;
            mbindingToFrameNavOfMW.Bindings.Add(bindingFromLocalEnable);
            mbindingToFrameNavOfMW.Bindings.Add(bindingFromRemoteEnable);
            mbindingToFrameNavOfMW.Converter = convertME2EALB;
            BindingOperations.SetBinding(mw.frameNav, Frame.IsEnabledProperty, mbindingToFrameNavOfMW);

            // 绑定：LocalEnable | RemoteEnable {属性} ==> statusBarLast {MainWindow控件}
            MultiBinding mbindingToStatusBarLastOfMW = new MultiBinding();
            mbindingToStatusBarLastOfMW.Mode = BindingMode.OneWay;
            mbindingToStatusBarLastOfMW.Bindings.Add(bindingFromLocalEnable);
            mbindingToStatusBarLastOfMW.Bindings.Add(bindingFromRemoteEnable);
            mbindingToStatusBarLastOfMW.Converter = convertME2BKALC;
            mbindingToStatusBarLastOfMW.ConverterParameter = new object[] { defaultBlueColor, defaultGreenColor };
            BindingOperations.SetBinding(mw.statusBarLast, StatusBarItem.BackgroundProperty, mbindingToStatusBarLastOfMW);
        }

        /// <summary>
        /// 绑定域 --| WindowCommand Kind And Text |-- 内元素
        /// </summary>
        private void BindingItemsWindowCommandKindAndText()
        {
            // 绑定：ConnectBtnText {属性} ==> btnRemoteConnectionText {MainWindow控件}
            Binding bindingFromConnectBtnTextToBtnRemoteConnectionTextOfMW = new Binding();
            bindingFromConnectBtnTextToBtnRemoteConnectionTextOfMW.Source = this;
            bindingFromConnectBtnTextToBtnRemoteConnectionTextOfMW.Path = new PropertyPath("ConnectBtnText");
            bindingFromConnectBtnTextToBtnRemoteConnectionTextOfMW.Mode = BindingMode.OneWay;
            bindingFromConnectBtnTextToBtnRemoteConnectionTextOfMW.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            BindingOperations.SetBinding(mw.btnRemoteConnectionText, TextBlock.TextProperty, bindingFromConnectBtnTextToBtnRemoteConnectionTextOfMW);

            // 绑定：SuperviseBtnText {属性} ==> btnRemoteSuperviseText {MainWindow控件}
            Binding bindingFromSuperviseBtnTextToBtnRemoteSuperviseTextOfMW = new Binding();
            bindingFromSuperviseBtnTextToBtnRemoteSuperviseTextOfMW.Source = this;
            bindingFromSuperviseBtnTextToBtnRemoteSuperviseTextOfMW.Path = new PropertyPath("SuperviseBtnText");
            bindingFromSuperviseBtnTextToBtnRemoteSuperviseTextOfMW.Mode = BindingMode.OneWay;
            bindingFromSuperviseBtnTextToBtnRemoteSuperviseTextOfMW.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            BindingOperations.SetBinding(mw.btnRemoteSuperviseText, TextBlock.TextProperty, bindingFromSuperviseBtnTextToBtnRemoteSuperviseTextOfMW);

            // 绑定：ConnectBtnIcon {属性} ==> btnRemoteConnectionIcon {MainWindow控件}
            Binding bindingFromConnectBtnIconToBtnRemoteConnectionIconOfMW = new Binding();
            bindingFromConnectBtnIconToBtnRemoteConnectionIconOfMW.Source = this;
            bindingFromConnectBtnIconToBtnRemoteConnectionIconOfMW.Path = new PropertyPath("ConnectBtnIcon");
            bindingFromConnectBtnIconToBtnRemoteConnectionIconOfMW.Mode = BindingMode.OneWay;
            bindingFromConnectBtnIconToBtnRemoteConnectionIconOfMW.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            BindingOperations.SetBinding(mw.btnRemoteConnectionIcon, PackIconMaterial.KindProperty, bindingFromConnectBtnIconToBtnRemoteConnectionIconOfMW);

            // 绑定：SuperviseBtnIcon {属性} ==> btnRemoteSuperviseIcon {MainWindow控件}
            Binding bindingFromSuperviseBtnIconToBtnRemoteSuperviseIconOfMW = new Binding();
            bindingFromSuperviseBtnIconToBtnRemoteSuperviseIconOfMW.Source = this;
            bindingFromSuperviseBtnIconToBtnRemoteSuperviseIconOfMW.Path = new PropertyPath("SuperviseBtnIcon");
            bindingFromSuperviseBtnIconToBtnRemoteSuperviseIconOfMW.Mode = BindingMode.OneWay;
            bindingFromSuperviseBtnIconToBtnRemoteSuperviseIconOfMW.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            BindingOperations.SetBinding(mw.btnRemoteSuperviseIcon, PackIconFontAwesome.KindProperty, bindingFromSuperviseBtnIconToBtnRemoteSuperviseIconOfMW);

            // 绑定：ConnectBtnEnable {属性} ==> btnRemoteConnection {MainWindow控件}
            Binding bindingFromConnectBtnEnableToBtnRemoteConnectionOfMW = new Binding();
            bindingFromConnectBtnEnableToBtnRemoteConnectionOfMW.Source = this;
            bindingFromConnectBtnEnableToBtnRemoteConnectionOfMW.Path = new PropertyPath("ConnectBtnEnable");
            bindingFromConnectBtnEnableToBtnRemoteConnectionOfMW.Mode = BindingMode.OneWay;
            bindingFromConnectBtnEnableToBtnRemoteConnectionOfMW.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            BindingOperations.SetBinding(mw.btnRemoteConnection, Button.IsEnabledProperty, bindingFromConnectBtnEnableToBtnRemoteConnectionOfMW);

            // 绑定：SuperviseBtnEnable {属性} ==> btnRemoteSupervise {MainWindow控件}
            Binding bindingFromSuperviseBtnEnableToBtnRemoteSuperviseOfMW = new Binding();
            bindingFromSuperviseBtnEnableToBtnRemoteSuperviseOfMW.Source = this;
            bindingFromSuperviseBtnEnableToBtnRemoteSuperviseOfMW.Path = new PropertyPath("SuperviseBtnEnable");
            bindingFromSuperviseBtnEnableToBtnRemoteSuperviseOfMW.Mode = BindingMode.OneWay;
            bindingFromSuperviseBtnEnableToBtnRemoteSuperviseOfMW.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            BindingOperations.SetBinding(mw.btnRemoteSupervise, Button.IsEnabledProperty, bindingFromSuperviseBtnEnableToBtnRemoteSuperviseOfMW);
        }

        /// <summary>
        /// 绑定域 --| StatusBar Content And Color |-- 内元素
        /// </summary>
        private void BindingItemsStatusBarContentAndColor()
        {
            // 绑定：StatusBarContent {属性} ==> statusBar {MainWindow控件}
            Binding bindingFromStatusBarContentToStatusBarOfMW = new Binding();
            bindingFromStatusBarContentToStatusBarOfMW.Source = this;
            bindingFromStatusBarContentToStatusBarOfMW.Path = new PropertyPath("StatusBarContent");
            bindingFromStatusBarContentToStatusBarOfMW.Mode = BindingMode.OneWay;
            bindingFromStatusBarContentToStatusBarOfMW.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            BindingOperations.SetBinding(mw.statusBar, StatusBarItem.ContentProperty, bindingFromStatusBarContentToStatusBarOfMW);

            // 绑定：StatusBarBackgroundColor {属性} ==> statusBar {MainWindow控件}
            Binding bindingFromStatusBarBackgroundColorToStatusBarOfMW = new Binding();
            bindingFromStatusBarBackgroundColorToStatusBarOfMW.Source = this;
            bindingFromStatusBarBackgroundColorToStatusBarOfMW.Path = new PropertyPath("StatusBarBackgroundColor");
            bindingFromStatusBarBackgroundColorToStatusBarOfMW.Mode = BindingMode.OneWay;
            bindingFromStatusBarBackgroundColorToStatusBarOfMW.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            BindingOperations.SetBinding(mw.statusBar, StatusBarItem.BackgroundProperty, bindingFromStatusBarBackgroundColorToStatusBarOfMW);
        }

        /// <summary>
        /// 绑定域 --| StatusBarRemote Content And Color |-- 内元素
        /// </summary>
        private void BindingItemsStatusBarRemoteContentAndColor()
        {
            // 绑定：StatusBarRemoteContent {属性} ==> statusBarRemote {MainWindow控件}
            Binding bindingFromStatusBarRemoteContentToStatusBarRemoteOfMW = new Binding();
            bindingFromStatusBarRemoteContentToStatusBarRemoteOfMW.Source = this;
            bindingFromStatusBarRemoteContentToStatusBarRemoteOfMW.Path = new PropertyPath("StatusBarRemoteContent");
            bindingFromStatusBarRemoteContentToStatusBarRemoteOfMW.Mode = BindingMode.OneWay;
            bindingFromStatusBarRemoteContentToStatusBarRemoteOfMW.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            BindingOperations.SetBinding(mw.statusBarRemote, StatusBarItem.ContentProperty, bindingFromStatusBarRemoteContentToStatusBarRemoteOfMW);

            // 绑定：StatusBarRemoteBackgroundColor {属性} ==> statusBarRemote {MainWindow控件}
            Binding bindingFromStatusBarRemoteBackgroundColorToStatusBarOfMW = new Binding();
            bindingFromStatusBarRemoteBackgroundColorToStatusBarOfMW.Source = this;
            bindingFromStatusBarRemoteBackgroundColorToStatusBarOfMW.Path = new PropertyPath("StatusBarRemoteBackgroundColor");
            bindingFromStatusBarRemoteBackgroundColorToStatusBarOfMW.Mode = BindingMode.OneWay;
            bindingFromStatusBarRemoteBackgroundColorToStatusBarOfMW.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            BindingOperations.SetBinding(mw.statusBarRemote, StatusBarItem.BackgroundProperty, bindingFromStatusBarRemoteBackgroundColorToStatusBarOfMW);
        }

        /// <summary>
        /// 绑定域 --| Base Moving Refrence Cordinate And Moving Speed Ratio |-- 内元素
        /// </summary>
        private void BindingItemsBaseMovingRefrenceCordinateAndMovingSpeedRatio()
        {
            // 绑定：BaseMoveCordinate {属性} ==> chooseMotionWay {BaseControl控件}
            Binding bindingFromBaseMoveCordinateToChooseMotionWayOfBC = new Binding();
            bindingFromBaseMoveCordinateToChooseMotionWayOfBC.Source = this;
            bindingFromBaseMoveCordinateToChooseMotionWayOfBC.Path = new PropertyPath("BaseMoveCordinate");
            bindingFromBaseMoveCordinateToChooseMotionWayOfBC.Mode = BindingMode.TwoWay;
            bindingFromBaseMoveCordinateToChooseMotionWayOfBC.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            BindingOperations.SetBinding(bc.chooseMotionWay, ToggleSwitch.IsCheckedProperty, bindingFromBaseMoveCordinateToChooseMotionWayOfBC);

            // 绑定：BaseMoveSpeedRatio {属性} ==> speedSlider {BaseControl控件}
            Binding bindingFromBaseMoveSpeedRatioToSpeedSliderOfBC = new Binding();
            bindingFromBaseMoveSpeedRatioToSpeedSliderOfBC.Source = this;
            bindingFromBaseMoveSpeedRatioToSpeedSliderOfBC.Path = new PropertyPath("BaseMoveSpeedRatio");
            bindingFromBaseMoveSpeedRatioToSpeedSliderOfBC.Mode = BindingMode.TwoWay;
            bindingFromBaseMoveSpeedRatioToSpeedSliderOfBC.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            BindingOperations.SetBinding(bc.speedSlider, Slider.ValueProperty, bindingFromBaseMoveSpeedRatioToSpeedSliderOfBC);
        }

        /// <summary>
        /// 绑定域 --| Parameters Needed To Show On Window --> Tool TCP Cordinates |-- 内元素
        /// </summary>
        private void BindingItemsParametersNeededToShowOnWindowToolTCPCordinates()
        {
            #region BaseControl
            // 绑定：ToolTCPCordinateX {属性} ==> tcpXMotion {BaseControl控件}
            Binding bindingFromToolTCPCordinateXToTcpXMotion = new Binding();
            bindingFromToolTCPCordinateXToTcpXMotion.Source = this;
            bindingFromToolTCPCordinateXToTcpXMotion.Path = new PropertyPath("ToolTCPCordinateX");
            bindingFromToolTCPCordinateXToTcpXMotion.Mode = BindingMode.OneWay;
            bindingFromToolTCPCordinateXToTcpXMotion.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            bindingFromToolTCPCordinateXToTcpXMotion.Converter = convertD2S;
            bindingFromToolTCPCordinateXToTcpXMotion.ConverterParameter = valueP1000D2;
            BindingOperations.SetBinding(bc.tcpXMotion, TextBox.TextProperty, bindingFromToolTCPCordinateXToTcpXMotion);

            // 绑定：ToolTCPCordinateY {属性} ==> tcpYMotion {BaseControl控件}
            Binding bindingFromToolTCPCordinateYToTcpYMotion = new Binding();
            bindingFromToolTCPCordinateYToTcpYMotion.Source = this;
            bindingFromToolTCPCordinateYToTcpYMotion.Path = new PropertyPath("ToolTCPCordinateY");
            bindingFromToolTCPCordinateYToTcpYMotion.Mode = BindingMode.OneWay;
            bindingFromToolTCPCordinateYToTcpYMotion.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            bindingFromToolTCPCordinateYToTcpYMotion.Converter = convertD2S;
            bindingFromToolTCPCordinateYToTcpYMotion.ConverterParameter = valueP1000D2;
            BindingOperations.SetBinding(bc.tcpYMotion, TextBox.TextProperty, bindingFromToolTCPCordinateYToTcpYMotion);

            // 绑定：ToolTCPCordinateZ {属性} ==> tcpZMotion {BaseControl控件}
            Binding bindingFromToolTCPCordinateZToTcpZMotion = new Binding();
            bindingFromToolTCPCordinateZToTcpZMotion.Source = this;
            bindingFromToolTCPCordinateZToTcpZMotion.Path = new PropertyPath("ToolTCPCordinateZ");
            bindingFromToolTCPCordinateZToTcpZMotion.Mode = BindingMode.OneWay;
            bindingFromToolTCPCordinateZToTcpZMotion.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            bindingFromToolTCPCordinateZToTcpZMotion.Converter = convertD2S;
            bindingFromToolTCPCordinateZToTcpZMotion.ConverterParameter = valueP1000D2;
            BindingOperations.SetBinding(bc.tcpZMotion, TextBox.TextProperty, bindingFromToolTCPCordinateZToTcpZMotion);

            // 绑定：ToolTCPCordinateRX {属性} ==> tcpRXMotion {BaseControl控件}
            Binding bindingFromToolTCPCordinateRXToTcpRXMotion = new Binding();
            bindingFromToolTCPCordinateRXToTcpRXMotion.Source = this;
            bindingFromToolTCPCordinateRXToTcpRXMotion.Path = new PropertyPath("ToolTCPCordinateRX");
            bindingFromToolTCPCordinateRXToTcpRXMotion.Mode = BindingMode.OneWay;
            bindingFromToolTCPCordinateRXToTcpRXMotion.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            bindingFromToolTCPCordinateRXToTcpRXMotion.Converter = convertD2S;
            bindingFromToolTCPCordinateRXToTcpRXMotion.ConverterParameter = valueP1D4;
            BindingOperations.SetBinding(bc.tcpRXMotion, TextBox.TextProperty, bindingFromToolTCPCordinateRXToTcpRXMotion);

            // 绑定：ToolTCPCordinateRY {属性} ==> tcpRYMotion {BaseControl控件}
            Binding bindingFromToolTCPCordinateRYToTcpRYMotion = new Binding();
            bindingFromToolTCPCordinateRYToTcpRYMotion.Source = this;
            bindingFromToolTCPCordinateRYToTcpRYMotion.Path = new PropertyPath("ToolTCPCordinateRY");
            bindingFromToolTCPCordinateRYToTcpRYMotion.Mode = BindingMode.OneWay;
            bindingFromToolTCPCordinateRYToTcpRYMotion.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            bindingFromToolTCPCordinateRYToTcpRYMotion.Converter = convertD2S;
            bindingFromToolTCPCordinateRYToTcpRYMotion.ConverterParameter = valueP1D4;
            BindingOperations.SetBinding(bc.tcpRYMotion, TextBox.TextProperty, bindingFromToolTCPCordinateRYToTcpRYMotion);

            // 绑定：ToolTCPCordinateRZ {属性} ==> tcpRZMotion {BaseControl控件}
            Binding bindingFromToolTCPCordinateRZToTcpRZMotion = new Binding();
            bindingFromToolTCPCordinateRZToTcpRZMotion.Source = this;
            bindingFromToolTCPCordinateRZToTcpRZMotion.Path = new PropertyPath("ToolTCPCordinateRZ");
            bindingFromToolTCPCordinateRZToTcpRZMotion.Mode = BindingMode.OneWay;
            bindingFromToolTCPCordinateRZToTcpRZMotion.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            bindingFromToolTCPCordinateRZToTcpRZMotion.Converter = convertD2S;
            bindingFromToolTCPCordinateRZToTcpRZMotion.ConverterParameter = valueP1D4;
            BindingOperations.SetBinding(bc.tcpRZMotion, TextBox.TextProperty, bindingFromToolTCPCordinateRZToTcpRZMotion);
            #endregion

            #region GalactophoreDetect
            // 绑定：ToolTCPCordinateX {属性} ==> tcpXGalactophore {GalactophoreDetect控件}
            Binding bindingFromToolTCPCordinateXToTcpXGalactophore = new Binding();
            bindingFromToolTCPCordinateXToTcpXGalactophore.Source = this;
            bindingFromToolTCPCordinateXToTcpXGalactophore.Path = new PropertyPath("ToolTCPCordinateX");
            bindingFromToolTCPCordinateXToTcpXGalactophore.Mode = BindingMode.OneWay;
            bindingFromToolTCPCordinateXToTcpXGalactophore.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            bindingFromToolTCPCordinateXToTcpXGalactophore.Converter = convertD2S;
            bindingFromToolTCPCordinateXToTcpXGalactophore.ConverterParameter = valueP1000D2;
            BindingOperations.SetBinding(gd.tcpXGalactophore, TextBox.TextProperty, bindingFromToolTCPCordinateXToTcpXGalactophore);

            // 绑定：ToolTCPCordinateY {属性} ==> tcpYGalactophore {GalactophoreDetect控件}
            Binding bindingFromToolTCPCordinateYToTcpYGalactophore = new Binding();
            bindingFromToolTCPCordinateYToTcpYGalactophore.Source = this;
            bindingFromToolTCPCordinateYToTcpYGalactophore.Path = new PropertyPath("ToolTCPCordinateY");
            bindingFromToolTCPCordinateYToTcpYGalactophore.Mode = BindingMode.OneWay;
            bindingFromToolTCPCordinateYToTcpYGalactophore.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            bindingFromToolTCPCordinateYToTcpYGalactophore.Converter = convertD2S;
            bindingFromToolTCPCordinateYToTcpYGalactophore.ConverterParameter = valueP1000D2;
            BindingOperations.SetBinding(gd.tcpYGalactophore, TextBox.TextProperty, bindingFromToolTCPCordinateYToTcpYGalactophore);

            // 绑定：ToolTCPCordinateZ {属性} ==> tcpZGalactophore {GalactophoreDetect控件}
            Binding bindingFromToolTCPCordinateZToTcpZGalactophore = new Binding();
            bindingFromToolTCPCordinateZToTcpZGalactophore.Source = this;
            bindingFromToolTCPCordinateZToTcpZGalactophore.Path = new PropertyPath("ToolTCPCordinateZ");
            bindingFromToolTCPCordinateZToTcpZGalactophore.Mode = BindingMode.OneWay;
            bindingFromToolTCPCordinateZToTcpZGalactophore.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            bindingFromToolTCPCordinateZToTcpZGalactophore.Converter = convertD2S;
            bindingFromToolTCPCordinateZToTcpZGalactophore.ConverterParameter = valueP1000D2;
            BindingOperations.SetBinding(gd.tcpZGalactophore, TextBox.TextProperty, bindingFromToolTCPCordinateZToTcpZGalactophore);

            // 绑定：ToolTCPCordinateRX {属性} ==> tcpRXGalactophore {GalactophoreDetect控件}
            Binding bindingFromToolTCPCordinateRXToTcpRXGalactophore = new Binding();
            bindingFromToolTCPCordinateRXToTcpRXGalactophore.Source = this;
            bindingFromToolTCPCordinateRXToTcpRXGalactophore.Path = new PropertyPath("ToolTCPCordinateRX");
            bindingFromToolTCPCordinateRXToTcpRXGalactophore.Mode = BindingMode.OneWay;
            bindingFromToolTCPCordinateRXToTcpRXGalactophore.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            bindingFromToolTCPCordinateRXToTcpRXGalactophore.Converter = convertD2S;
            bindingFromToolTCPCordinateRXToTcpRXGalactophore.ConverterParameter = valueP1D4;
            BindingOperations.SetBinding(gd.tcpRXGalactophore, TextBox.TextProperty, bindingFromToolTCPCordinateRXToTcpRXGalactophore);

            // 绑定：ToolTCPCordinateRY {属性} ==> tcpRYGalactophore {GalactophoreDetect控件}
            Binding bindingFromToolTCPCordinateRYToTcpRYGalactophore = new Binding();
            bindingFromToolTCPCordinateRYToTcpRYGalactophore.Source = this;
            bindingFromToolTCPCordinateRYToTcpRYGalactophore.Path = new PropertyPath("ToolTCPCordinateRY");
            bindingFromToolTCPCordinateRYToTcpRYGalactophore.Mode = BindingMode.OneWay;
            bindingFromToolTCPCordinateRYToTcpRYGalactophore.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            bindingFromToolTCPCordinateRYToTcpRYGalactophore.Converter = convertD2S;
            bindingFromToolTCPCordinateRYToTcpRYGalactophore.ConverterParameter = valueP1D4;
            BindingOperations.SetBinding(gd.tcpRYGalactophore, TextBox.TextProperty, bindingFromToolTCPCordinateRYToTcpRYGalactophore);

            // 绑定：ToolTCPCordinateRZ {属性} ==> tcpRZGalactophore {GalactophoreDetect控件}
            Binding bindingFromToolTCPCordinateRZToTcpRZGalactophore = new Binding();
            bindingFromToolTCPCordinateRZToTcpRZGalactophore.Source = this;
            bindingFromToolTCPCordinateRZToTcpRZGalactophore.Path = new PropertyPath("ToolTCPCordinateRZ");
            bindingFromToolTCPCordinateRZToTcpRZGalactophore.Mode = BindingMode.OneWay;
            bindingFromToolTCPCordinateRZToTcpRZGalactophore.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            bindingFromToolTCPCordinateRZToTcpRZGalactophore.Converter = convertD2S;
            bindingFromToolTCPCordinateRZToTcpRZGalactophore.ConverterParameter = valueP1D4;
            BindingOperations.SetBinding(gd.tcpRZGalactophore, TextBox.TextProperty, bindingFromToolTCPCordinateRZToTcpRZGalactophore);
            #endregion

            #region ThyroidScan
            // 绑定：ToolTCPCordinateX {属性} ==> tcpXThyroid {ThyroidScan控件}
            Binding bindingFromToolTCPCordinateXToTcpXThyroid = new Binding();
            bindingFromToolTCPCordinateXToTcpXThyroid.Source = this;
            bindingFromToolTCPCordinateXToTcpXThyroid.Path = new PropertyPath("ToolTCPCordinateX");
            bindingFromToolTCPCordinateXToTcpXThyroid.Mode = BindingMode.OneWay;
            bindingFromToolTCPCordinateXToTcpXThyroid.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            bindingFromToolTCPCordinateXToTcpXThyroid.Converter = convertD2S;
            bindingFromToolTCPCordinateXToTcpXThyroid.ConverterParameter = valueP1000D2;
            BindingOperations.SetBinding(ts.tcpXThyroid, TextBox.TextProperty, bindingFromToolTCPCordinateXToTcpXThyroid);

            // 绑定：ToolTCPCordinateY {属性} ==> tcpYThyroid {ThyroidScan控件}
            Binding bindingFromToolTCPCordinateYToTcpYThyroid = new Binding();
            bindingFromToolTCPCordinateYToTcpYThyroid.Source = this;
            bindingFromToolTCPCordinateYToTcpYThyroid.Path = new PropertyPath("ToolTCPCordinateY");
            bindingFromToolTCPCordinateYToTcpYThyroid.Mode = BindingMode.OneWay;
            bindingFromToolTCPCordinateYToTcpYThyroid.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            bindingFromToolTCPCordinateYToTcpYThyroid.Converter = convertD2S;
            bindingFromToolTCPCordinateYToTcpYThyroid.ConverterParameter = valueP1000D2;
            BindingOperations.SetBinding(ts.tcpYThyroid, TextBox.TextProperty, bindingFromToolTCPCordinateYToTcpYThyroid);

            // 绑定：ToolTCPCordinateZ {属性} ==> tcpZThyroid {ThyroidScan控件}
            Binding bindingFromToolTCPCordinateZToTcpZThyroid = new Binding();
            bindingFromToolTCPCordinateZToTcpZThyroid.Source = this;
            bindingFromToolTCPCordinateZToTcpZThyroid.Path = new PropertyPath("ToolTCPCordinateZ");
            bindingFromToolTCPCordinateZToTcpZThyroid.Mode = BindingMode.OneWay;
            bindingFromToolTCPCordinateZToTcpZThyroid.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            bindingFromToolTCPCordinateZToTcpZThyroid.Converter = convertD2S;
            bindingFromToolTCPCordinateZToTcpZThyroid.ConverterParameter = valueP1000D2;
            BindingOperations.SetBinding(ts.tcpZThyroid, TextBox.TextProperty, bindingFromToolTCPCordinateZToTcpZThyroid);

            // 绑定：ToolTCPCordinateRX {属性} ==> tcpRXThyroid {ThyroidScan控件}
            Binding bindingFromToolTCPCordinateRXToTcpRXThyroid = new Binding();
            bindingFromToolTCPCordinateRXToTcpRXThyroid.Source = this;
            bindingFromToolTCPCordinateRXToTcpRXThyroid.Path = new PropertyPath("ToolTCPCordinateRX");
            bindingFromToolTCPCordinateRXToTcpRXThyroid.Mode = BindingMode.OneWay;
            bindingFromToolTCPCordinateRXToTcpRXThyroid.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            bindingFromToolTCPCordinateRXToTcpRXThyroid.Converter = convertD2S;
            bindingFromToolTCPCordinateRXToTcpRXThyroid.ConverterParameter = valueP1D4;
            BindingOperations.SetBinding(ts.tcpRXThyroid, TextBox.TextProperty, bindingFromToolTCPCordinateRXToTcpRXThyroid);

            // 绑定：ToolTCPCordinateRY {属性} ==> tcpRYThyroid {ThyroidScan控件}
            Binding bindingFromToolTCPCordinateRYToTcpRYThyroid = new Binding();
            bindingFromToolTCPCordinateRYToTcpRYThyroid.Source = this;
            bindingFromToolTCPCordinateRYToTcpRYThyroid.Path = new PropertyPath("ToolTCPCordinateRY");
            bindingFromToolTCPCordinateRYToTcpRYThyroid.Mode = BindingMode.OneWay;
            bindingFromToolTCPCordinateRYToTcpRYThyroid.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            bindingFromToolTCPCordinateRYToTcpRYThyroid.Converter = convertD2S;
            bindingFromToolTCPCordinateRYToTcpRYThyroid.ConverterParameter = valueP1D4;
            BindingOperations.SetBinding(ts.tcpRYThyroid, TextBox.TextProperty, bindingFromToolTCPCordinateRYToTcpRYThyroid);

            // 绑定：ToolTCPCordinateRZ {属性} ==> tcpRZThyroid {ThyroidScan控件}
            Binding bindingFromToolTCPCordinateRZToTcpRZThyroid = new Binding();
            bindingFromToolTCPCordinateRZToTcpRZThyroid.Source = this;
            bindingFromToolTCPCordinateRZToTcpRZThyroid.Path = new PropertyPath("ToolTCPCordinateRZ");
            bindingFromToolTCPCordinateRZToTcpRZThyroid.Mode = BindingMode.OneWay;
            bindingFromToolTCPCordinateRZToTcpRZThyroid.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            bindingFromToolTCPCordinateRZToTcpRZThyroid.Converter = convertD2S;
            bindingFromToolTCPCordinateRZToTcpRZThyroid.ConverterParameter = valueP1D4;
            BindingOperations.SetBinding(ts.tcpRZThyroid, TextBox.TextProperty, bindingFromToolTCPCordinateRZToTcpRZThyroid);
            #endregion
        }

        /// <summary>
        /// 绑定域 --| Parameters Needed To Show On Window --> Tool Force And Torque |-- 内元素
        /// </summary>
        private void BindingItemsParametersNeededToShowOnWindowToolForceAndTorque()
        {
            #region BaseControl
            // 绑定：ToolForceX {属性} ==> tcpFXMotion {BaseControl控件}
            Binding bindingFromToolForceXToTcpFXMotion = new Binding();
            bindingFromToolForceXToTcpFXMotion.Source = this;
            bindingFromToolForceXToTcpFXMotion.Path = new PropertyPath("ToolForceX");
            bindingFromToolForceXToTcpFXMotion.Mode = BindingMode.OneWay;
            bindingFromToolForceXToTcpFXMotion.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            bindingFromToolForceXToTcpFXMotion.Converter = convertD2S;
            bindingFromToolForceXToTcpFXMotion.ConverterParameter = valueP1D2;
            BindingOperations.SetBinding(bc.tcpFXMotion, TextBox.TextProperty, bindingFromToolForceXToTcpFXMotion);

            // 绑定：ToolForceY {属性} ==> tcpFYMotion {BaseControl控件}
            Binding bindingFromToolForceYToTcpFYMotion = new Binding();
            bindingFromToolForceYToTcpFYMotion.Source = this;
            bindingFromToolForceYToTcpFYMotion.Path = new PropertyPath("ToolForceY");
            bindingFromToolForceYToTcpFYMotion.Mode = BindingMode.OneWay;
            bindingFromToolForceYToTcpFYMotion.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            bindingFromToolForceYToTcpFYMotion.Converter = convertD2S;
            bindingFromToolForceYToTcpFYMotion.ConverterParameter = valueP1D2;
            BindingOperations.SetBinding(bc.tcpFYMotion, TextBox.TextProperty, bindingFromToolForceYToTcpFYMotion);

            // 绑定：ToolForceZ {属性} ==> tcpFZMotion {BaseControl控件}
            Binding bindingFromToolForceZToTcpFZMotion = new Binding();
            bindingFromToolForceZToTcpFZMotion.Source = this;
            bindingFromToolForceZToTcpFZMotion.Path = new PropertyPath("ToolForceZ");
            bindingFromToolForceZToTcpFZMotion.Mode = BindingMode.OneWay;
            bindingFromToolForceZToTcpFZMotion.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            bindingFromToolForceZToTcpFZMotion.Converter = convertD2S;
            bindingFromToolForceZToTcpFZMotion.ConverterParameter = valueP1D2;
            BindingOperations.SetBinding(bc.tcpFZMotion, TextBox.TextProperty, bindingFromToolForceZToTcpFZMotion);

            // 绑定：ToolTorqueX {属性} ==> tcpTXMotion {BaseControl控件}
            Binding bindingFromToolTorqueXToTcpTXMotion = new Binding();
            bindingFromToolTorqueXToTcpTXMotion.Source = this;
            bindingFromToolTorqueXToTcpTXMotion.Path = new PropertyPath("ToolTorqueX");
            bindingFromToolTorqueXToTcpTXMotion.Mode = BindingMode.OneWay;
            bindingFromToolTorqueXToTcpTXMotion.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            bindingFromToolTorqueXToTcpTXMotion.Converter = convertD2S;
            bindingFromToolTorqueXToTcpTXMotion.ConverterParameter = valueP1D3;
            BindingOperations.SetBinding(bc.tcpTXMotion, TextBox.TextProperty, bindingFromToolTorqueXToTcpTXMotion);

            // 绑定：ToolTorqueY {属性} ==> tcpTYMotion {BaseControl控件}
            Binding bindingFromToolTorqueYToTcpTYMotion = new Binding();
            bindingFromToolTorqueYToTcpTYMotion.Source = this;
            bindingFromToolTorqueYToTcpTYMotion.Path = new PropertyPath("ToolTorqueY");
            bindingFromToolTorqueYToTcpTYMotion.Mode = BindingMode.OneWay;
            bindingFromToolTorqueYToTcpTYMotion.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            bindingFromToolTorqueYToTcpTYMotion.Converter = convertD2S;
            bindingFromToolTorqueYToTcpTYMotion.ConverterParameter = valueP1D3;
            BindingOperations.SetBinding(bc.tcpTYMotion, TextBox.TextProperty, bindingFromToolTorqueYToTcpTYMotion);

            // 绑定：ToolTorqueZ {属性} ==> tcpTZMotion {BaseControl控件}
            Binding bindingFromToolTorqueZToTcpTZMotion = new Binding();
            bindingFromToolTorqueZToTcpTZMotion.Source = this;
            bindingFromToolTorqueZToTcpTZMotion.Path = new PropertyPath("ToolTorqueZ");
            bindingFromToolTorqueZToTcpTZMotion.Mode = BindingMode.OneWay;
            bindingFromToolTorqueZToTcpTZMotion.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            bindingFromToolTorqueZToTcpTZMotion.Converter = convertD2S;
            bindingFromToolTorqueZToTcpTZMotion.ConverterParameter = valueP1D3;
            BindingOperations.SetBinding(bc.tcpTZMotion, TextBox.TextProperty, bindingFromToolTorqueZToTcpTZMotion);
            #endregion

            #region GalactophoreDetect
            // 绑定：ToolForceX {属性} ==> tcpFXGalactophore {GalactophoreDetect控件}
            Binding bindingFromToolForceXToTcpFXGalactophore = new Binding();
            bindingFromToolForceXToTcpFXGalactophore.Source = this;
            bindingFromToolForceXToTcpFXGalactophore.Path = new PropertyPath("ToolForceX");
            bindingFromToolForceXToTcpFXGalactophore.Mode = BindingMode.OneWay;
            bindingFromToolForceXToTcpFXGalactophore.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            bindingFromToolForceXToTcpFXGalactophore.Converter = convertD2S;
            bindingFromToolForceXToTcpFXGalactophore.ConverterParameter = valueP1D2;
            BindingOperations.SetBinding(gd.tcpFXGalactophore, TextBox.TextProperty, bindingFromToolForceXToTcpFXGalactophore);

            // 绑定：ToolForceY {属性} ==> tcpFYGalactophore {GalactophoreDetect控件}
            Binding bindingFromToolForceYToTcpFYGalactophore = new Binding();
            bindingFromToolForceYToTcpFYGalactophore.Source = this;
            bindingFromToolForceYToTcpFYGalactophore.Path = new PropertyPath("ToolForceY");
            bindingFromToolForceYToTcpFYGalactophore.Mode = BindingMode.OneWay;
            bindingFromToolForceYToTcpFYGalactophore.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            bindingFromToolForceYToTcpFYGalactophore.Converter = convertD2S;
            bindingFromToolForceYToTcpFYGalactophore.ConverterParameter = valueP1D2;
            BindingOperations.SetBinding(gd.tcpFYGalactophore, TextBox.TextProperty, bindingFromToolForceYToTcpFYGalactophore);

            // 绑定：ToolForceZ {属性} ==> tcpFZGalactophore {GalactophoreDetect控件}
            Binding bindingFromToolForceZToTcpFZGalactophore = new Binding();
            bindingFromToolForceZToTcpFZGalactophore.Source = this;
            bindingFromToolForceZToTcpFZGalactophore.Path = new PropertyPath("ToolForceZ");
            bindingFromToolForceZToTcpFZGalactophore.Mode = BindingMode.OneWay;
            bindingFromToolForceZToTcpFZGalactophore.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            bindingFromToolForceZToTcpFZGalactophore.Converter = convertD2S;
            bindingFromToolForceZToTcpFZGalactophore.ConverterParameter = valueP1D2;
            BindingOperations.SetBinding(gd.tcpFZGalactophore, TextBox.TextProperty, bindingFromToolForceZToTcpFZGalactophore);
            #endregion

            #region ThyroidScan
            // 绑定：ToolForceX {属性} ==> tcpFXThyroid {ThyroidScan控件}
            Binding bindingFromToolForceXToTcpFXThyroid = new Binding();
            bindingFromToolForceXToTcpFXThyroid.Source = this;
            bindingFromToolForceXToTcpFXThyroid.Path = new PropertyPath("ToolForceX");
            bindingFromToolForceXToTcpFXThyroid.Mode = BindingMode.OneWay;
            bindingFromToolForceXToTcpFXThyroid.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            bindingFromToolForceXToTcpFXThyroid.Converter = convertD2S;
            bindingFromToolForceXToTcpFXThyroid.ConverterParameter = valueP1D2;
            BindingOperations.SetBinding(ts.tcpFXThyroid, TextBox.TextProperty, bindingFromToolForceXToTcpFXThyroid);

            // 绑定：ToolForceY {属性} ==> tcpFYThyroid {ThyroidScan控件}
            Binding bindingFromToolForceYToTcpFYThyroid = new Binding();
            bindingFromToolForceYToTcpFYThyroid.Source = this;
            bindingFromToolForceYToTcpFYThyroid.Path = new PropertyPath("ToolForceY");
            bindingFromToolForceYToTcpFYThyroid.Mode = BindingMode.OneWay;
            bindingFromToolForceYToTcpFYThyroid.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            bindingFromToolForceYToTcpFYThyroid.Converter = convertD2S;
            bindingFromToolForceYToTcpFYThyroid.ConverterParameter = valueP1D2;
            BindingOperations.SetBinding(ts.tcpFYThyroid, TextBox.TextProperty, bindingFromToolForceYToTcpFYThyroid);

            // 绑定：ToolForceZ {属性} ==> tcpFZThyroid {ThyroidScan控件}
            Binding bindingFromToolForceZToTcpFZThyroid = new Binding();
            bindingFromToolForceZToTcpFZThyroid.Source = this;
            bindingFromToolForceZToTcpFZThyroid.Path = new PropertyPath("ToolForceZ");
            bindingFromToolForceZToTcpFZThyroid.Mode = BindingMode.OneWay;
            bindingFromToolForceZToTcpFZThyroid.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            bindingFromToolForceZToTcpFZThyroid.Converter = convertD2S;
            bindingFromToolForceZToTcpFZThyroid.ConverterParameter = valueP1D2;
            BindingOperations.SetBinding(ts.tcpFZThyroid, TextBox.TextProperty, bindingFromToolForceZToTcpFZThyroid);
            #endregion
        }

        /// <summary>
        /// 绑定域 --| Parameters Needed To Show On Window --> Robot Joints Currents |-- 内元素
        /// </summary>
        private void BindingItemsParametersNeededToShowOnWindowRobotJointsCurrents()
        {
            // 绑定：RobotJointBaseCurrent {属性} ==> baseCurrentMotion {BaseControl控件}
            Binding bindingFromRobotJointBaseCurrentToBaseCurrentMotion = new Binding();
            bindingFromRobotJointBaseCurrentToBaseCurrentMotion.Source = this;
            bindingFromRobotJointBaseCurrentToBaseCurrentMotion.Path = new PropertyPath("RobotJointBaseCurrent");
            bindingFromRobotJointBaseCurrentToBaseCurrentMotion.Mode = BindingMode.OneWay;
            bindingFromRobotJointBaseCurrentToBaseCurrentMotion.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            bindingFromRobotJointBaseCurrentToBaseCurrentMotion.Converter = convertD2S;
            bindingFromRobotJointBaseCurrentToBaseCurrentMotion.ConverterParameter = valueP1000D1;
            BindingOperations.SetBinding(bc.baseCurrentMotion, TextBox.TextProperty, bindingFromRobotJointBaseCurrentToBaseCurrentMotion);

            // 绑定：RobotJointShoulderCurrent {属性} ==> shoulderCurrentMotion {BaseControl控件}
            Binding bindingFromRobotJointShoulderCurrentToShoulderCurrentMotion = new Binding();
            bindingFromRobotJointShoulderCurrentToShoulderCurrentMotion.Source = this;
            bindingFromRobotJointShoulderCurrentToShoulderCurrentMotion.Path = new PropertyPath("RobotJointShoulderCurrent");
            bindingFromRobotJointShoulderCurrentToShoulderCurrentMotion.Mode = BindingMode.OneWay;
            bindingFromRobotJointShoulderCurrentToShoulderCurrentMotion.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            bindingFromRobotJointShoulderCurrentToShoulderCurrentMotion.Converter = convertD2S;
            bindingFromRobotJointShoulderCurrentToShoulderCurrentMotion.ConverterParameter = valueP1000D1;
            BindingOperations.SetBinding(bc.shoulderCurrentMotion, TextBox.TextProperty, bindingFromRobotJointShoulderCurrentToShoulderCurrentMotion);

            // 绑定：RobotJointElbowCurrent {属性} ==> elbowCurrentMotion {BaseControl控件}
            Binding bindingFromRobotJointElbowCurrentToElbowCurrentMotion = new Binding();
            bindingFromRobotJointElbowCurrentToElbowCurrentMotion.Source = this;
            bindingFromRobotJointElbowCurrentToElbowCurrentMotion.Path = new PropertyPath("RobotJointElbowCurrent");
            bindingFromRobotJointElbowCurrentToElbowCurrentMotion.Mode = BindingMode.OneWay;
            bindingFromRobotJointElbowCurrentToElbowCurrentMotion.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            bindingFromRobotJointElbowCurrentToElbowCurrentMotion.Converter = convertD2S;
            bindingFromRobotJointElbowCurrentToElbowCurrentMotion.ConverterParameter = valueP1000D1;
            BindingOperations.SetBinding(bc.elbowCurrentMotion, TextBox.TextProperty, bindingFromRobotJointElbowCurrentToElbowCurrentMotion);

            // 绑定：RobotJointWrist1Current {属性} ==> wrist1CurrentMotion {BaseControl控件}
            Binding bindingFromRobotJointWrist1CurrentToWrist1CurrentMotion = new Binding();
            bindingFromRobotJointWrist1CurrentToWrist1CurrentMotion.Source = this;
            bindingFromRobotJointWrist1CurrentToWrist1CurrentMotion.Path = new PropertyPath("RobotJointWrist1Current");
            bindingFromRobotJointWrist1CurrentToWrist1CurrentMotion.Mode = BindingMode.OneWay;
            bindingFromRobotJointWrist1CurrentToWrist1CurrentMotion.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            bindingFromRobotJointWrist1CurrentToWrist1CurrentMotion.Converter = convertD2S;
            bindingFromRobotJointWrist1CurrentToWrist1CurrentMotion.ConverterParameter = valueP1000D1;
            BindingOperations.SetBinding(bc.wrist1CurrentMotion, TextBox.TextProperty, bindingFromRobotJointWrist1CurrentToWrist1CurrentMotion);

            // 绑定：RobotJointWrist2Current {属性} ==> wrist2CurrentMotion {BaseControl控件}
            Binding bindingFromRobotJointWrist2CurrentToWrist2CurrentMotion = new Binding();
            bindingFromRobotJointWrist2CurrentToWrist2CurrentMotion.Source = this;
            bindingFromRobotJointWrist2CurrentToWrist2CurrentMotion.Path = new PropertyPath("RobotJointWrist2Current");
            bindingFromRobotJointWrist2CurrentToWrist2CurrentMotion.Mode = BindingMode.OneWay;
            bindingFromRobotJointWrist2CurrentToWrist2CurrentMotion.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            bindingFromRobotJointWrist2CurrentToWrist2CurrentMotion.Converter = convertD2S;
            bindingFromRobotJointWrist2CurrentToWrist2CurrentMotion.ConverterParameter = valueP1000D1;
            BindingOperations.SetBinding(bc.wrist2CurrentMotion, TextBox.TextProperty, bindingFromRobotJointWrist2CurrentToWrist2CurrentMotion);

            // 绑定：RobotJointWrist3Current {属性} ==> wrist3CurrentMotion {BaseControl控件}
            Binding bindingFromRobotJointWrist3CurrentToWrist3CurrentMotion = new Binding();
            bindingFromRobotJointWrist3CurrentToWrist3CurrentMotion.Source = this;
            bindingFromRobotJointWrist3CurrentToWrist3CurrentMotion.Path = new PropertyPath("RobotJointWrist3Current");
            bindingFromRobotJointWrist3CurrentToWrist3CurrentMotion.Mode = BindingMode.OneWay;
            bindingFromRobotJointWrist3CurrentToWrist3CurrentMotion.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            bindingFromRobotJointWrist3CurrentToWrist3CurrentMotion.Converter = convertD2S;
            bindingFromRobotJointWrist3CurrentToWrist3CurrentMotion.ConverterParameter = valueP1000D1;
            BindingOperations.SetBinding(bc.wrist3CurrentMotion, TextBox.TextProperty, bindingFromRobotJointWrist3CurrentToWrist3CurrentMotion);
        }

        /// <summary>
        /// 绑定域 --| Parameters Needed To Show On Window --> Robot Joints Angles |-- 内元素
        /// </summary>
        private void BindingItemsParametersNeededToShowOnWindowRobotJointsAngles()
        {
            // 绑定：RobotJointBaseAngle {属性} ==> j1Silder {BaseControl控件}
            Binding bindingFromRobotJointBaseAngleToJ1Silder = new Binding();
            bindingFromRobotJointBaseAngleToJ1Silder.Source = this;
            bindingFromRobotJointBaseAngleToJ1Silder.Path = new PropertyPath("RobotJointBaseAngle");
            bindingFromRobotJointBaseAngleToJ1Silder.Mode = BindingMode.OneWay;
            bindingFromRobotJointBaseAngleToJ1Silder.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            bindingFromRobotJointBaseAngleToJ1Silder.Converter = convertD2DSlider;
            bindingFromRobotJointBaseAngleToJ1Silder.ConverterParameter = radToDegRatio;
            BindingOperations.SetBinding(bc.j1Silder, Slider.ValueProperty, bindingFromRobotJointBaseAngleToJ1Silder);

            // 绑定：RobotJointShoulderAngle {属性} ==> j2Silder {BaseControl控件}
            Binding bindingFromRobotJointShoulderAngleToJ2Silder = new Binding();
            bindingFromRobotJointShoulderAngleToJ2Silder.Source = this;
            bindingFromRobotJointShoulderAngleToJ2Silder.Path = new PropertyPath("RobotJointShoulderAngle");
            bindingFromRobotJointShoulderAngleToJ2Silder.Mode = BindingMode.OneWay;
            bindingFromRobotJointShoulderAngleToJ2Silder.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            bindingFromRobotJointShoulderAngleToJ2Silder.Converter = convertD2DSlider;
            bindingFromRobotJointShoulderAngleToJ2Silder.ConverterParameter = radToDegRatio;
            BindingOperations.SetBinding(bc.j2Silder, Slider.ValueProperty, bindingFromRobotJointShoulderAngleToJ2Silder);

            // 绑定：RobotJointElbowAngle {属性} ==> j3Silder {BaseControl控件}
            Binding bindingFromRobotJointElbowAngleToJ3Silder = new Binding();
            bindingFromRobotJointElbowAngleToJ3Silder.Source = this;
            bindingFromRobotJointElbowAngleToJ3Silder.Path = new PropertyPath("RobotJointElbowAngle");
            bindingFromRobotJointElbowAngleToJ3Silder.Mode = BindingMode.OneWay;
            bindingFromRobotJointElbowAngleToJ3Silder.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            bindingFromRobotJointElbowAngleToJ3Silder.Converter = convertD2DSlider;
            bindingFromRobotJointElbowAngleToJ3Silder.ConverterParameter = radToDegRatio;
            BindingOperations.SetBinding(bc.j3Silder, Slider.ValueProperty, bindingFromRobotJointElbowAngleToJ3Silder);

            // 绑定：RobotJointWrist1Angle {属性} ==> j4Silder {BaseControl控件}
            Binding bindingFromRobotJointWrist1AngleToJ4Silder = new Binding();
            bindingFromRobotJointWrist1AngleToJ4Silder.Source = this;
            bindingFromRobotJointWrist1AngleToJ4Silder.Path = new PropertyPath("RobotJointWrist1Angle");
            bindingFromRobotJointWrist1AngleToJ4Silder.Mode = BindingMode.OneWay;
            bindingFromRobotJointWrist1AngleToJ4Silder.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            bindingFromRobotJointWrist1AngleToJ4Silder.Converter = convertD2DSlider;
            bindingFromRobotJointWrist1AngleToJ4Silder.ConverterParameter = radToDegRatio;
            BindingOperations.SetBinding(bc.j4Silder, Slider.ValueProperty, bindingFromRobotJointWrist1AngleToJ4Silder);

            // 绑定：RobotJointWrist2Angle {属性} ==> j5Silder {BaseControl控件}
            Binding bindingFromRobotJointWrist2AngleToJ5Silder = new Binding();
            bindingFromRobotJointWrist2AngleToJ5Silder.Source = this;
            bindingFromRobotJointWrist2AngleToJ5Silder.Path = new PropertyPath("RobotJointWrist2Angle");
            bindingFromRobotJointWrist2AngleToJ5Silder.Mode = BindingMode.OneWay;
            bindingFromRobotJointWrist2AngleToJ5Silder.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            bindingFromRobotJointWrist2AngleToJ5Silder.Converter = convertD2DSlider;
            bindingFromRobotJointWrist2AngleToJ5Silder.ConverterParameter = radToDegRatio;
            BindingOperations.SetBinding(bc.j5Silder, Slider.ValueProperty, bindingFromRobotJointWrist2AngleToJ5Silder);

            // 绑定：RobotJointWrist3Angle {属性} ==> j6Silder {BaseControl控件}
            Binding bindingFromRobotJointWrist3AngleToJ6Silder = new Binding();
            bindingFromRobotJointWrist3AngleToJ6Silder.Source = this;
            bindingFromRobotJointWrist3AngleToJ6Silder.Path = new PropertyPath("RobotJointWrist3Angle");
            bindingFromRobotJointWrist3AngleToJ6Silder.Mode = BindingMode.OneWay;
            bindingFromRobotJointWrist3AngleToJ6Silder.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            bindingFromRobotJointWrist3AngleToJ6Silder.Converter = convertD2DSlider;
            bindingFromRobotJointWrist3AngleToJ6Silder.ConverterParameter = radToDegRatio;
            BindingOperations.SetBinding(bc.j6Silder, Slider.ValueProperty, bindingFromRobotJointWrist3AngleToJ6Silder);
        }

        /// <summary>
        /// 绑定域 --| Nipple Position At Galactophore Detecting |-- 内元素
        /// </summary>
        private void BindingItemsNipplePositionAtGalactophoreDetecting()
        {
            // 绑定：NipplePositionGDR {属性} ==> nippleX {GalactophoreDetect控件}
            Binding bindingFromNipplePositionGDRToNippleX = new Binding();
            bindingFromNipplePositionGDRToNippleX.Source = this;
            bindingFromNipplePositionGDRToNippleX.Path = new PropertyPath("NipplePositionGDR");
            bindingFromNipplePositionGDRToNippleX.Mode = BindingMode.OneWay;
            bindingFromNipplePositionGDRToNippleX.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            bindingFromNipplePositionGDRToNippleX.Converter = convertDA2S;
            bindingFromNipplePositionGDRToNippleX.ConverterParameter = new int[] { 0, 1000, 2 };
            BindingOperations.SetBinding(gd.nippleX, TextBox.TextProperty, bindingFromNipplePositionGDRToNippleX);

            // 绑定：NipplePositionGDR {属性} ==> nippleY {GalactophoreDetect控件}
            Binding bindingFromNipplePositionGDRToNippleY = new Binding();
            bindingFromNipplePositionGDRToNippleY.Source = this;
            bindingFromNipplePositionGDRToNippleY.Path = new PropertyPath("NipplePositionGDR");
            bindingFromNipplePositionGDRToNippleY.Mode = BindingMode.OneWay;
            bindingFromNipplePositionGDRToNippleY.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            bindingFromNipplePositionGDRToNippleY.Converter = convertDA2S;
            bindingFromNipplePositionGDRToNippleY.ConverterParameter = new int[] { 1, 1000, 2 };
            BindingOperations.SetBinding(gd.nippleY, TextBox.TextProperty, bindingFromNipplePositionGDRToNippleY);

            // 绑定：NipplePositionGDR {属性} ==> nippleZ {GalactophoreDetect控件}
            Binding bindingFromNipplePositionGDRToNippleZ = new Binding();
            bindingFromNipplePositionGDRToNippleZ.Source = this;
            bindingFromNipplePositionGDRToNippleZ.Path = new PropertyPath("NipplePositionGDR");
            bindingFromNipplePositionGDRToNippleZ.Mode = BindingMode.OneWay;
            bindingFromNipplePositionGDRToNippleZ.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            bindingFromNipplePositionGDRToNippleZ.Converter = convertDA2S;
            bindingFromNipplePositionGDRToNippleZ.ConverterParameter = new int[] { 2, 1000, 2 };
            BindingOperations.SetBinding(gd.nippleZ, TextBox.TextProperty, bindingFromNipplePositionGDRToNippleZ);
        }

        /// <summary>
        /// 绑定域 --| Configuration Parameters Of GalactophoreDetector --> Detecting Direction Force Limits And Speed Limits |-- 内元素
        /// </summary>
        private void BindingItemsConfigurationParametersOfGalactophoreDetectorDetectingDirectionForceLimitsAndSpeedLimits()
        {
            // 绑定：DetectingErrorForceMinGDR {属性} ==> minForceSlider {Flyout控件}
            Binding bindingFromDetectingErrorForceMinGDRToMinForceSlider = new Binding();
            bindingFromDetectingErrorForceMinGDRToMinForceSlider.Source = this;
            bindingFromDetectingErrorForceMinGDRToMinForceSlider.Path = new PropertyPath("DetectingErrorForceMinGDR");
            bindingFromDetectingErrorForceMinGDRToMinForceSlider.Mode = BindingMode.OneWay;
            bindingFromDetectingErrorForceMinGDRToMinForceSlider.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            bindingFromDetectingErrorForceMinGDRToMinForceSlider.Converter = convertD2DI;
            bindingFromDetectingErrorForceMinGDRToMinForceSlider.ConverterParameter = new double[] { 0.0, 4.0 };
            BindingOperations.SetBinding(mw.minForceSlider, Slider.ValueProperty, bindingFromDetectingErrorForceMinGDRToMinForceSlider);

            // 绑定：DetectingErrorForceMaxGDR {属性} ==> maxForceSlider {Flyout控件}
            Binding bindingFromDetectingErrorForceMaxGDRToMaxForceSlider = new Binding();
            bindingFromDetectingErrorForceMaxGDRToMaxForceSlider.Source = this;
            bindingFromDetectingErrorForceMaxGDRToMaxForceSlider.Path = new PropertyPath("DetectingErrorForceMaxGDR");
            bindingFromDetectingErrorForceMaxGDRToMaxForceSlider.Mode = BindingMode.OneWay;
            bindingFromDetectingErrorForceMaxGDRToMaxForceSlider.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            bindingFromDetectingErrorForceMaxGDRToMaxForceSlider.Converter = convertD2DI;
            bindingFromDetectingErrorForceMaxGDRToMaxForceSlider.ConverterParameter = new double[] { -1.5, 2.0 };
            BindingOperations.SetBinding(mw.maxForceSlider, Slider.ValueProperty, bindingFromDetectingErrorForceMaxGDRToMaxForceSlider);

            // 绑定：DetectingSpeedMinGDR {属性} ==> minDetectSpeedSlider {Flyout控件}
            Binding bindingFromDetectingSpeedMinGDRToMinDetectSpeedSlider = new Binding();
            bindingFromDetectingSpeedMinGDRToMinDetectSpeedSlider.Source = this;
            bindingFromDetectingSpeedMinGDRToMinDetectSpeedSlider.Path = new PropertyPath("DetectingSpeedMinGDR");
            bindingFromDetectingSpeedMinGDRToMinDetectSpeedSlider.Mode = BindingMode.OneWay;
            bindingFromDetectingSpeedMinGDRToMinDetectSpeedSlider.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            bindingFromDetectingSpeedMinGDRToMinDetectSpeedSlider.Converter = convertD2DI;
            bindingFromDetectingSpeedMinGDRToMinDetectSpeedSlider.ConverterParameter = new double[] { 0.0, 10000.0 };
            BindingOperations.SetBinding(mw.minDetectSpeedSlider, Slider.ValueProperty, bindingFromDetectingSpeedMinGDRToMinDetectSpeedSlider);
        }

        /// <summary>
        /// 绑定域 --| Configuration Parameters Of GalactophoreDetector --> Detecting Motion Limits |-- 内元素
        /// </summary>
        private void BindingItemsConfigurationParametersOfGalactophoreDetectorDetectingMotionLimits()
        {
            // 绑定：NippleForbiddenRadiusGDR {属性} ==> minRadius {GalactophoreDetect控件}
            Binding bindingFromNippleForbiddenRadiusGDRToMinRadius = new Binding();
            bindingFromNippleForbiddenRadiusGDRToMinRadius.Source = this;
            bindingFromNippleForbiddenRadiusGDRToMinRadius.Path = new PropertyPath("NippleForbiddenRadiusGDR");
            bindingFromNippleForbiddenRadiusGDRToMinRadius.Mode = BindingMode.OneWay;
            bindingFromNippleForbiddenRadiusGDRToMinRadius.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            bindingFromNippleForbiddenRadiusGDRToMinRadius.Converter = convertD2S;
            bindingFromNippleForbiddenRadiusGDRToMinRadius.ConverterParameter = valueP1000D0;
            BindingOperations.SetBinding(gd.minRadius, TextBox.TextProperty, bindingFromNippleForbiddenRadiusGDRToMinRadius);

            // 绑定：DetectingStopDistanceGDR {属性} ==> scanDistance {GalactophoreDetect控件}
            Binding bindingFromDetectingStopDistanceGDRToScanDistance = new Binding();
            bindingFromDetectingStopDistanceGDRToScanDistance.Source = this;
            bindingFromDetectingStopDistanceGDRToScanDistance.Path = new PropertyPath("DetectingStopDistanceGDR");
            bindingFromDetectingStopDistanceGDRToScanDistance.Mode = BindingMode.OneWay;
            bindingFromDetectingStopDistanceGDRToScanDistance.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            bindingFromDetectingStopDistanceGDRToScanDistance.Converter = convertD2S;
            bindingFromDetectingStopDistanceGDRToScanDistance.ConverterParameter = valueP1000D0;
            BindingOperations.SetBinding(gd.scanDistance, TextBox.TextProperty, bindingFromDetectingStopDistanceGDRToScanDistance);

            // 绑定：DetectingSafetyLiftDistanceGDR {属性} ==> liftDistance {GalactophoreDetect控件}
            Binding bindingFromDetectingSafetyLiftDistanceGDRToLiftDistance = new Binding();
            bindingFromDetectingSafetyLiftDistanceGDRToLiftDistance.Source = this;
            bindingFromDetectingSafetyLiftDistanceGDRToLiftDistance.Path = new PropertyPath("DetectingSafetyLiftDistanceGDR");
            bindingFromDetectingSafetyLiftDistanceGDRToLiftDistance.Mode = BindingMode.OneWay;
            bindingFromDetectingSafetyLiftDistanceGDRToLiftDistance.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            bindingFromDetectingSafetyLiftDistanceGDRToLiftDistance.Converter = convertD2S;
            bindingFromDetectingSafetyLiftDistanceGDRToLiftDistance.ConverterParameter = valueP1000D0;
            BindingOperations.SetBinding(gd.liftDistance, TextBox.TextProperty, bindingFromDetectingSafetyLiftDistanceGDRToLiftDistance);

            // 绑定：DetectingSinkDistanceGDR {属性} ==> sinkDistance {GalactophoreDetect控件}
            //Binding bindingFromDetectingSinkDistanceGDRToSinkDistance = new Binding();
            //bindingFromDetectingSinkDistanceGDRToSinkDistance.Source = this;
            //bindingFromDetectingSinkDistanceGDRToSinkDistance.Path = new PropertyPath("DetectingSinkDistanceGDR");
            //bindingFromDetectingSinkDistanceGDRToSinkDistance.Mode = BindingMode.OneWay;
            //bindingFromDetectingSinkDistanceGDRToSinkDistance.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            //bindingFromDetectingSinkDistanceGDRToSinkDistance.Converter = convertD2S;
            //bindingFromDetectingSinkDistanceGDRToSinkDistance.ConverterParameter = valueP1000D0;
            //BindingOperations.SetBinding(gd.sinkDistance, TextBox.TextProperty, bindingFromDetectingSinkDistanceGDRToSinkDistance);

            // 绑定：IfEnableDetectingInitialForceGDR {属性} ==> IACheckSwitch {Flyout控件}
            Binding bindingFromIfEnableDetectingInitialForceGDRToIACheckSwitch = new Binding();
            bindingFromIfEnableDetectingInitialForceGDRToIACheckSwitch.Source = this;
            bindingFromIfEnableDetectingInitialForceGDRToIACheckSwitch.Path = new PropertyPath("IfEnableDetectingInitialForceGDR");
            bindingFromIfEnableDetectingInitialForceGDRToIACheckSwitch.Mode = BindingMode.OneWay;
            bindingFromIfEnableDetectingInitialForceGDRToIACheckSwitch.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            BindingOperations.SetBinding(mw.IACheckSwitch, ToggleSwitch.IsCheckedProperty, bindingFromIfEnableDetectingInitialForceGDRToIACheckSwitch);

            // 绑定：IfEnableAngleCorrectedGDR {属性} ==> ARectifySwitch {Flyout控件}
            Binding bindingFromIfEnableAngleCorrectedGDRToARectifySwitch = new Binding();
            bindingFromIfEnableAngleCorrectedGDRToARectifySwitch.Source = this;
            bindingFromIfEnableAngleCorrectedGDRToARectifySwitch.Path = new PropertyPath("IfEnableAngleCorrectedGDR");
            bindingFromIfEnableAngleCorrectedGDRToARectifySwitch.Mode = BindingMode.OneWay;
            bindingFromIfEnableAngleCorrectedGDRToARectifySwitch.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            BindingOperations.SetBinding(mw.ARectifySwitch, ToggleSwitch.IsCheckedProperty, bindingFromIfEnableAngleCorrectedGDRToARectifySwitch);
        }

        /// <summary>
        /// 绑定域 --| Configuration Parameters Of GalactophoreDetector --> Degree Control Parameters |-- 内元素
        /// </summary>
        private void BindingItemsConfigurationParametersOfGalactophoreDetectorDegreeControlParameters()
        {
            // 绑定：VibratingAngleDegreeGDR {属性} ==> vibrateDegreeSlider {Flyout控件}
            Binding bindingFromVibratingAngleDegreeGDRToVibrateDegreeSlider = new Binding();
            bindingFromVibratingAngleDegreeGDRToVibrateDegreeSlider.Source = this;
            bindingFromVibratingAngleDegreeGDRToVibrateDegreeSlider.Path = new PropertyPath("VibratingAngleDegreeGDR");
            bindingFromVibratingAngleDegreeGDRToVibrateDegreeSlider.Mode = BindingMode.OneWay;
            bindingFromVibratingAngleDegreeGDRToVibrateDegreeSlider.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            bindingFromVibratingAngleDegreeGDRToVibrateDegreeSlider.Converter = convertE2D;
            bindingFromVibratingAngleDegreeGDRToVibrateDegreeSlider.ConverterParameter = typeof(GalactophoreDetector.VibratingMagnitude);
            BindingOperations.SetBinding(mw.vibrateDegreeSlider, Slider.ValueProperty, bindingFromVibratingAngleDegreeGDRToVibrateDegreeSlider);

            // 绑定：MovingSpeedDegreeGDR {属性} ==> speedDegreeSlider {Flyout控件}
            Binding bindingFromMovingSpeedDegreeGDRToSpeedDegreeSlider = new Binding();
            bindingFromMovingSpeedDegreeGDRToSpeedDegreeSlider.Source = this;
            bindingFromMovingSpeedDegreeGDRToSpeedDegreeSlider.Path = new PropertyPath("MovingSpeedDegreeGDR");
            bindingFromMovingSpeedDegreeGDRToSpeedDegreeSlider.Mode = BindingMode.OneWay;
            bindingFromMovingSpeedDegreeGDRToSpeedDegreeSlider.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            bindingFromMovingSpeedDegreeGDRToSpeedDegreeSlider.Converter = convertE2D;
            bindingFromMovingSpeedDegreeGDRToSpeedDegreeSlider.ConverterParameter = typeof(GalactophoreDetector.MovingLevel);
            BindingOperations.SetBinding(mw.speedDegreeSlider, Slider.ValueProperty, bindingFromMovingSpeedDegreeGDRToSpeedDegreeSlider);

            // 绑定：DetectingForceDegreeGDR {属性} ==> forceDegreeSlider {Flyout控件}
            Binding bindingFromDetectingForceDegreeGDRToForceDegreeSlider = new Binding();
            bindingFromDetectingForceDegreeGDRToForceDegreeSlider.Source = this;
            bindingFromDetectingForceDegreeGDRToForceDegreeSlider.Path = new PropertyPath("DetectingForceDegreeGDR");
            bindingFromDetectingForceDegreeGDRToForceDegreeSlider.Mode = BindingMode.OneWay;
            bindingFromDetectingForceDegreeGDRToForceDegreeSlider.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            bindingFromDetectingForceDegreeGDRToForceDegreeSlider.Converter = convertE2D;
            bindingFromDetectingForceDegreeGDRToForceDegreeSlider.ConverterParameter = typeof(GalactophoreDetector.DetectingIntensity);
            BindingOperations.SetBinding(mw.forceDegreeSlider, Slider.ValueProperty, bindingFromDetectingForceDegreeGDRToForceDegreeSlider);

            // 绑定：DetectingAlignDegreeGDR {属性} ==> attachSwitch {Flyout控件}
            Binding bindingFromDetectingAlignDegreeGDRToAttachSwitch = new Binding();
            bindingFromDetectingAlignDegreeGDRToAttachSwitch.Source = this;
            bindingFromDetectingAlignDegreeGDRToAttachSwitch.Path = new PropertyPath("DetectingAlignDegreeGDR");
            bindingFromDetectingAlignDegreeGDRToAttachSwitch.Mode = BindingMode.OneWay;
            bindingFromDetectingAlignDegreeGDRToAttachSwitch.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            bindingFromDetectingAlignDegreeGDRToAttachSwitch.Converter = convertE2B;
            bindingFromDetectingAlignDegreeGDRToAttachSwitch.ConverterParameter = typeof(GalactophoreDetector.AligningDegree);
            BindingOperations.SetBinding(mw.attachSwitch, ToggleSwitch.IsCheckedProperty, bindingFromDetectingAlignDegreeGDRToAttachSwitch);
        }

        /// <summary>
        /// 绑定域 --| Configuration Parameters Of GalactophoreDetector --> Detecting Edge |-- 内元素
        /// </summary>
        private void BindingItemsConfigurationParametersOfGalactophoreDetectorDetectingEdge()
        {
            // 绑定：IdentifyEdgeModeGDR {属性} ==> borderModeSlider {Flyout控件}
            Binding bindingFromIdentifyEdgeModeGDRToBorderModeSlider = new Binding();
            bindingFromIdentifyEdgeModeGDRToBorderModeSlider.Source = this;
            bindingFromIdentifyEdgeModeGDRToBorderModeSlider.Path = new PropertyPath("IdentifyEdgeModeGDR");
            bindingFromIdentifyEdgeModeGDRToBorderModeSlider.Mode = BindingMode.OneWay;
            bindingFromIdentifyEdgeModeGDRToBorderModeSlider.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            bindingFromIdentifyEdgeModeGDRToBorderModeSlider.Converter = convertE2D;
            bindingFromIdentifyEdgeModeGDRToBorderModeSlider.ConverterParameter = typeof(GalactophoreDetector.IdentifyBoundary);
            BindingOperations.SetBinding(mw.borderModeSlider, Slider.ValueProperty, bindingFromIdentifyEdgeModeGDRToBorderModeSlider);

            // 绑定：MovingUpEdgeDistanceGDR {属性} ==> headBound {GalactophoreDetect控件}
            Binding bindingFromMovingUpEdgeDistanceGDRToHeadBound = new Binding();
            bindingFromMovingUpEdgeDistanceGDRToHeadBound.Source = this;
            bindingFromMovingUpEdgeDistanceGDRToHeadBound.Path = new PropertyPath("MovingUpEdgeDistanceGDR");
            bindingFromMovingUpEdgeDistanceGDRToHeadBound.Mode = BindingMode.OneWay;
            bindingFromMovingUpEdgeDistanceGDRToHeadBound.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            bindingFromMovingUpEdgeDistanceGDRToHeadBound.Converter = convertD2S;
            bindingFromMovingUpEdgeDistanceGDRToHeadBound.ConverterParameter = valueP1000D0;
            BindingOperations.SetBinding(gd.headBound, TextBox.TextProperty, bindingFromMovingUpEdgeDistanceGDRToHeadBound);

            // 绑定：MovingDownEdgeDistanceGDR {属性} ==> tailBound {GalactophoreDetect控件}
            Binding bindingFromMovingDownEdgeDistanceGDRToTailBound = new Binding();
            bindingFromMovingDownEdgeDistanceGDRToTailBound.Source = this;
            bindingFromMovingDownEdgeDistanceGDRToTailBound.Path = new PropertyPath("MovingDownEdgeDistanceGDR");
            bindingFromMovingDownEdgeDistanceGDRToTailBound.Mode = BindingMode.OneWay;
            bindingFromMovingDownEdgeDistanceGDRToTailBound.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            bindingFromMovingDownEdgeDistanceGDRToTailBound.Converter = convertD2S;
            bindingFromMovingDownEdgeDistanceGDRToTailBound.ConverterParameter = valueP1000D0;
            BindingOperations.SetBinding(gd.tailBound, TextBox.TextProperty, bindingFromMovingDownEdgeDistanceGDRToTailBound);

            // 绑定：MovingLeftEdgeDistanceGDR {属性} ==> outBound {GalactophoreDetect控件}
            Binding bindingFromMovingLeftEdgeDistanceGDRToOutBound = new Binding();
            bindingFromMovingLeftEdgeDistanceGDRToOutBound.Source = this;
            bindingFromMovingLeftEdgeDistanceGDRToOutBound.Path = new PropertyPath("MovingLeftEdgeDistanceGDR");
            bindingFromMovingLeftEdgeDistanceGDRToOutBound.Mode = BindingMode.OneWay;
            bindingFromMovingLeftEdgeDistanceGDRToOutBound.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            bindingFromMovingLeftEdgeDistanceGDRToOutBound.Converter = convertD2S;
            bindingFromMovingLeftEdgeDistanceGDRToOutBound.ConverterParameter = valueP1000D0;
            BindingOperations.SetBinding(gd.outBound, TextBox.TextProperty, bindingFromMovingLeftEdgeDistanceGDRToOutBound);

            // 绑定：MovingRightEdgeDistanceGDR {属性} ==> inBound {GalactophoreDetect控件}
            Binding bindingFromMovingRightEdgeDistanceGDRToInBound = new Binding();
            bindingFromMovingRightEdgeDistanceGDRToInBound.Source = this;
            bindingFromMovingRightEdgeDistanceGDRToInBound.Path = new PropertyPath("MovingRightEdgeDistanceGDR");
            bindingFromMovingRightEdgeDistanceGDRToInBound.Mode = BindingMode.OneWay;
            bindingFromMovingRightEdgeDistanceGDRToInBound.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            bindingFromMovingRightEdgeDistanceGDRToInBound.Converter = convertD2S;
            bindingFromMovingRightEdgeDistanceGDRToInBound.ConverterParameter = valueP1000D0;
            BindingOperations.SetBinding(gd.inBound, TextBox.TextProperty, bindingFromMovingRightEdgeDistanceGDRToInBound);
        }

        /// <summary>
        /// 绑定域 --| Configuration Parameters Of GalactophoreDetector --> Other |-- 内元素
        /// </summary>
        private void BindingItemsConfigurationParametersOfGalactophoreDetectorOther()
        {
            // 绑定：IfAutoReplaceConfigurationGDR {属性} ==> autoSaveSwitch {Flyout控件}
            Binding bindingFromIfAutoReplaceConfigurationGDRToAutoSaveSwitch = new Binding();
            bindingFromIfAutoReplaceConfigurationGDRToAutoSaveSwitch.Source = this;
            bindingFromIfAutoReplaceConfigurationGDRToAutoSaveSwitch.Path = new PropertyPath("IfAutoReplaceConfigurationGDR");
            bindingFromIfAutoReplaceConfigurationGDRToAutoSaveSwitch.Mode = BindingMode.OneWay;
            bindingFromIfAutoReplaceConfigurationGDRToAutoSaveSwitch.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            BindingOperations.SetBinding(mw.autoSaveSwitch, ToggleSwitch.IsCheckedProperty, bindingFromIfAutoReplaceConfigurationGDRToAutoSaveSwitch);

            // 绑定：IfCheckRightGalactophoreGDR {属性} ==> galactophoreDirectionSwitch {Flyout控件}
            Binding bindingFromIfCheckRightGalactophoreGDRToGalactophoreDirectionSwitch = new Binding();
            bindingFromIfCheckRightGalactophoreGDRToGalactophoreDirectionSwitch.Source = this;
            bindingFromIfCheckRightGalactophoreGDRToGalactophoreDirectionSwitch.Path = new PropertyPath("IfCheckRightGalactophoreGDR");
            bindingFromIfCheckRightGalactophoreGDRToGalactophoreDirectionSwitch.Mode = BindingMode.OneWay;
            bindingFromIfCheckRightGalactophoreGDRToGalactophoreDirectionSwitch.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            bindingFromIfCheckRightGalactophoreGDRToGalactophoreDirectionSwitch.Converter = convertE2B;
            bindingFromIfCheckRightGalactophoreGDRToGalactophoreDirectionSwitch.ConverterParameter = typeof(GalactophoreDetector.ScanningRegion);
            BindingOperations.SetBinding(mw.galactophoreDirectionSwitch, ToggleSwitch.IsCheckedProperty, bindingFromIfCheckRightGalactophoreGDRToGalactophoreDirectionSwitch);

            // 绑定：CheckingStepGDR {属性} ==> rotateStepSlider {Flyout控件}
            Binding bindingFromCheckingStepGDRToRotateStepSlider = new Binding();
            bindingFromCheckingStepGDRToRotateStepSlider.Source = this;
            bindingFromCheckingStepGDRToRotateStepSlider.Path = new PropertyPath("CheckingStepGDR");
            bindingFromCheckingStepGDRToRotateStepSlider.Mode = BindingMode.OneWay;
            bindingFromCheckingStepGDRToRotateStepSlider.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            bindingFromCheckingStepGDRToRotateStepSlider.Converter = convertD2DI;
            bindingFromCheckingStepGDRToRotateStepSlider.ConverterParameter = new double[] { -Math.PI / 12.0, radToDegRatio };
            BindingOperations.SetBinding(mw.rotateStepSlider, Slider.ValueProperty, bindingFromCheckingStepGDRToRotateStepSlider);
        }

        /// <summary>
        /// 联合绑定
        /// 绑定域 --| GalactophoreDetector Working Status |-- 内元素
        /// 绑定域 --| GalactophoreDetector Paramete Confirm |-- 内元素
        /// 绑定域 --| GalactophoreDetector ForceSenor Cleared |-- 内元素
        /// 绑定域 --| GalactophoreDetector Parameter Confirm State |-- 内元素
        /// </summary>
        private void BindingItemsGalactophoreDetectorWorkingStatus()
        {
            // 绑定：GalactophoreDetectorWorkStatus {属性} ==> {i} {GalactophorDetect控件}
            Binding bindingFromGalactophoreDetectorWorkStatus = new Binding();
            bindingFromGalactophoreDetectorWorkStatus.Source = this;
            bindingFromGalactophoreDetectorWorkStatus.Path = new PropertyPath("GalactophoreDetectorWorkStatus");
            bindingFromGalactophoreDetectorWorkStatus.Mode = BindingMode.OneWay;
            bindingFromGalactophoreDetectorWorkStatus.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;

            // 绑定：GalactophoreDetectorParameterConfirm {属性} ==> {i} {GalactophorDetect控件}
            Binding bindingFromGalactophoreDetectorParameterConfirm = new Binding();
            bindingFromGalactophoreDetectorParameterConfirm.Source = this;
            bindingFromGalactophoreDetectorParameterConfirm.Path = new PropertyPath("GalactophoreDetectorParameterConfirm");
            bindingFromGalactophoreDetectorParameterConfirm.Mode = BindingMode.OneWay;
            bindingFromGalactophoreDetectorParameterConfirm.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;

            // 绑定：GalactophoreDetectorForceSensorCleared {属性} ==> {i} {GalactophorDetect控件}
            Binding bindingFromGalactophoreDetectorForceSensorCleared = new Binding();
            bindingFromGalactophoreDetectorForceSensorCleared.Source = this;
            bindingFromGalactophoreDetectorForceSensorCleared.Path = new PropertyPath("GalactophoreDetectorForceSensorCleared");
            bindingFromGalactophoreDetectorForceSensorCleared.Mode = BindingMode.OneWay;
            bindingFromGalactophoreDetectorForceSensorCleared.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;

            // 绑定：GalactophoreDetectorParameterConfirmState {属性} ==> {i} {GalactophorDetect控件}
            Binding bindingFromGalactophoreDetectorParameterConfirmState = new Binding();
            bindingFromGalactophoreDetectorParameterConfirmState.Source = this;
            bindingFromGalactophoreDetectorParameterConfirmState.Path = new PropertyPath("GalactophoreDetectorParameterConfirmState");
            bindingFromGalactophoreDetectorParameterConfirmState.Mode = BindingMode.OneWay;
            bindingFromGalactophoreDetectorParameterConfirmState.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;

            // 1. iconBackGalactophore
            MultiBinding mbindingToIconBackGalactophore = new MultiBinding();
            mbindingToIconBackGalactophore.Mode = BindingMode.OneWay;
            mbindingToIconBackGalactophore.Bindings.Add(bindingFromGalactophoreDetectorWorkStatus);
            mbindingToIconBackGalactophore.Converter = convertMS2EB;
            mbindingToIconBackGalactophore.ConverterParameter = new object[] { new byte[] { 0 }, new OperateModuleBase.WorkStatus[] { OperateModuleBase.WorkStatus.CanDoWork } };
            BindingOperations.SetBinding(gd.iconBackGalactophore, IconButton.IsEnabledProperty, mbindingToIconBackGalactophore);

            // 2. iconForceToZeroGalactophore
            MultiBinding mbindingToIconForceToZeroGalactophore = new MultiBinding();
            mbindingToIconForceToZeroGalactophore.Mode = BindingMode.OneWay;
            mbindingToIconForceToZeroGalactophore.Bindings.Add(bindingFromGalactophoreDetectorWorkStatus);
            mbindingToIconForceToZeroGalactophore.Converter = convertMS2EB;
            mbindingToIconForceToZeroGalactophore.ConverterParameter = new object[] { new byte[] { 0 }, new OperateModuleBase.WorkStatus[] { OperateModuleBase.WorkStatus.CanDoWork } };
            BindingOperations.SetBinding(gd.iconForceToZeroGalactophore, IconButton.IsEnabledProperty, mbindingToIconForceToZeroGalactophore);

            // 3. iconFromZeroToConfGalactophore
            MultiBinding mbindingToIconFromZeroToConfGalactophore = new MultiBinding();
            mbindingToIconFromZeroToConfGalactophore.Mode = BindingMode.OneWay;
            mbindingToIconFromZeroToConfGalactophore.Bindings.Add(bindingFromGalactophoreDetectorWorkStatus);
            mbindingToIconFromZeroToConfGalactophore.Bindings.Add(bindingFromGalactophoreDetectorForceSensorCleared);
            mbindingToIconFromZeroToConfGalactophore.Converter = convertMS2EB;
            mbindingToIconFromZeroToConfGalactophore.ConverterParameter = new object[] { new byte[] { 0 }, new OperateModuleBase.WorkStatus[] { OperateModuleBase.WorkStatus.InitialForceDevice } };
            BindingOperations.SetBinding(gd.iconFromZeroToConfGalactophore, IconButton.IsEnabledProperty, mbindingToIconFromZeroToConfGalactophore);

            // 4. iconConfGalactophore
            MultiBinding mbindingToIconConfGalactophore = new MultiBinding();
            mbindingToIconConfGalactophore.Mode = BindingMode.OneWay;
            mbindingToIconConfGalactophore.Bindings.Add(bindingFromGalactophoreDetectorWorkStatus);
            mbindingToIconConfGalactophore.Bindings.Add(bindingFromGalactophoreDetectorForceSensorCleared);
            mbindingToIconConfGalactophore.Converter = convertMS2EB;
            mbindingToIconConfGalactophore.ConverterParameter = new object[] { new byte[] { 0 }, new OperateModuleBase.WorkStatus[] { OperateModuleBase.WorkStatus.InitialForceDevice } };
            BindingOperations.SetBinding(gd.iconConfGalactophore, IconButton.IsEnabledProperty, mbindingToIconConfGalactophore);

            // 5. iconFromConfToParaGalactophore
            MultiBinding mbindingToIconFromConfToParaGalactophore = new MultiBinding();
            mbindingToIconFromConfToParaGalactophore.Mode = BindingMode.OneWay;
            mbindingToIconFromConfToParaGalactophore.Bindings.Add(bindingFromGalactophoreDetectorWorkStatus);
            mbindingToIconFromConfToParaGalactophore.Bindings.Add(bindingFromGalactophoreDetectorParameterConfirmState);
            mbindingToIconFromConfToParaGalactophore.Converter = convertMS2EB;
            mbindingToIconFromConfToParaGalactophore.ConverterParameter = new object[] { new byte[] { 1 }, new OperateModuleBase.WorkStatus[] { OperateModuleBase.WorkStatus.ParametersConfiguration } };
            BindingOperations.SetBinding(gd.iconFromConfToParaGalactophore, IconButton.IsEnabledProperty, mbindingToIconFromConfToParaGalactophore);

            // 6. iconConfNipple
            MultiBinding mbindingToIconConfNipple = new MultiBinding();
            mbindingToIconConfNipple.Mode = BindingMode.OneWay;
            mbindingToIconConfNipple.Bindings.Add(bindingFromGalactophoreDetectorWorkStatus);
            mbindingToIconConfNipple.Bindings.Add(bindingFromGalactophoreDetectorParameterConfirmState);
            mbindingToIconConfNipple.Converter = convertMS2EB;
            mbindingToIconConfNipple.ConverterParameter = new object[] { new byte[] { 1, Byte.MaxValue }, new OperateModuleBase.WorkStatus[] { OperateModuleBase.WorkStatus.ParametersConfiguration } };
            BindingOperations.SetBinding(gd.iconConfNipple, IconButton.IsEnabledProperty, mbindingToIconConfNipple);

            // 7. iconConfLift
            MultiBinding mbindingToIconConfLift = new MultiBinding();
            mbindingToIconConfLift.Mode = BindingMode.OneWay;
            mbindingToIconConfLift.Bindings.Add(bindingFromGalactophoreDetectorWorkStatus);
            mbindingToIconConfLift.Bindings.Add(bindingFromGalactophoreDetectorParameterConfirmState);
            mbindingToIconConfLift.Converter = convertMS2EB;
            mbindingToIconConfLift.ConverterParameter = new object[] { new byte[] { 2, Byte.MaxValue }, new OperateModuleBase.WorkStatus[] { OperateModuleBase.WorkStatus.ParametersConfiguration } };
            BindingOperations.SetBinding(gd.iconConfLift, IconButton.IsEnabledProperty, mbindingToIconConfLift);

            // 8. iconConfForbidden
            MultiBinding mbindingToIconConfForbidden = new MultiBinding();
            mbindingToIconConfForbidden.Mode = BindingMode.OneWay;
            mbindingToIconConfForbidden.Bindings.Add(bindingFromGalactophoreDetectorWorkStatus);
            mbindingToIconConfForbidden.Bindings.Add(bindingFromGalactophoreDetectorParameterConfirmState);
            mbindingToIconConfForbidden.Converter = convertMS2EB;
            mbindingToIconConfForbidden.ConverterParameter = new object[] { new byte[] { 3, Byte.MaxValue }, new OperateModuleBase.WorkStatus[] { OperateModuleBase.WorkStatus.ParametersConfiguration } };
            BindingOperations.SetBinding(gd.iconConfForbidden, IconButton.IsEnabledProperty, mbindingToIconConfForbidden);

            // 9. iconConfScan
            MultiBinding mbindingToIconConfScan = new MultiBinding();
            mbindingToIconConfScan.Mode = BindingMode.OneWay;
            mbindingToIconConfScan.Bindings.Add(bindingFromGalactophoreDetectorWorkStatus);
            mbindingToIconConfScan.Bindings.Add(bindingFromGalactophoreDetectorParameterConfirmState);
            mbindingToIconConfScan.Converter = convertMS2EB;
            mbindingToIconConfScan.ConverterParameter = new object[] { new byte[] { 4, Byte.MaxValue }, new OperateModuleBase.WorkStatus[] { OperateModuleBase.WorkStatus.ParametersConfiguration } };
            BindingOperations.SetBinding(gd.iconConfScan, IconButton.IsEnabledProperty, mbindingToIconConfScan);

            // 10. iconConfUp
            MultiBinding mbindingToIconConfUp = new MultiBinding();
            mbindingToIconConfUp.Mode = BindingMode.OneWay;
            mbindingToIconConfUp.Bindings.Add(bindingFromGalactophoreDetectorWorkStatus);
            mbindingToIconConfUp.Bindings.Add(bindingFromGalactophoreDetectorParameterConfirmState);
            mbindingToIconConfUp.Converter = convertMS2EB;
            mbindingToIconConfUp.ConverterParameter = new object[] { new byte[] { 5, Byte.MaxValue }, new OperateModuleBase.WorkStatus[] { OperateModuleBase.WorkStatus.ParametersConfiguration } };
            BindingOperations.SetBinding(gd.iconConfUp, IconButton.IsEnabledProperty, mbindingToIconConfUp);

            // 11. iconConfDown
            MultiBinding mbindingToIconConfDown = new MultiBinding();
            mbindingToIconConfDown.Mode = BindingMode.OneWay;
            mbindingToIconConfDown.Bindings.Add(bindingFromGalactophoreDetectorWorkStatus);
            mbindingToIconConfDown.Bindings.Add(bindingFromGalactophoreDetectorParameterConfirmState);
            mbindingToIconConfDown.Converter = convertMS2EB;
            mbindingToIconConfDown.ConverterParameter = new object[] { new byte[] { 6, Byte.MaxValue }, new OperateModuleBase.WorkStatus[] { OperateModuleBase.WorkStatus.ParametersConfiguration } };
            BindingOperations.SetBinding(gd.iconConfDown, IconButton.IsEnabledProperty, mbindingToIconConfDown);

            // 12. iconConfLeft
            MultiBinding mbindingToIconConfLeft = new MultiBinding();
            mbindingToIconConfLeft.Mode = BindingMode.OneWay;
            mbindingToIconConfLeft.Bindings.Add(bindingFromGalactophoreDetectorWorkStatus);
            mbindingToIconConfLeft.Bindings.Add(bindingFromGalactophoreDetectorParameterConfirmState);
            mbindingToIconConfLeft.Converter = convertMS2EB;
            mbindingToIconConfLeft.ConverterParameter = new object[] { new byte[] { 7, Byte.MaxValue }, new OperateModuleBase.WorkStatus[] { OperateModuleBase.WorkStatus.ParametersConfiguration } };
            BindingOperations.SetBinding(gd.iconConfLeft, IconButton.IsEnabledProperty, mbindingToIconConfLeft);

            // 13. iconConfRight
            MultiBinding mbindingToIconConfRight = new MultiBinding();
            mbindingToIconConfRight.Mode = BindingMode.OneWay;
            mbindingToIconConfRight.Bindings.Add(bindingFromGalactophoreDetectorWorkStatus);
            mbindingToIconConfRight.Bindings.Add(bindingFromGalactophoreDetectorParameterConfirmState);
            mbindingToIconConfRight.Converter = convertMS2EB;
            mbindingToIconConfRight.ConverterParameter = new object[] { new byte[] { 8, Byte.MaxValue }, new OperateModuleBase.WorkStatus[] { OperateModuleBase.WorkStatus.ParametersConfiguration } };
            BindingOperations.SetBinding(gd.iconConfRight, IconButton.IsEnabledProperty, mbindingToIconConfRight);

            // 14. iconConfSink
            //MultiBinding mbindingToIconConfSink = new MultiBinding();
            //mbindingToIconConfSink.Mode = BindingMode.OneWay;
            //mbindingToIconConfSink.Bindings.Add(bindingFromGalactophoreDetectorWorkStatus);
            //mbindingToIconConfSink.Bindings.Add(bindingFromGalactophoreDetectorParameterConfirmState);
            //mbindingToIconConfSink.Converter = convertMS2EB;
            //mbindingToIconConfSink.ConverterParameter = new object[] { new byte[] { 9, Byte.MaxValue }, new OperateModuleBase.WorkStatus[] { OperateModuleBase.WorkStatus.ParametersConfiguration } };
            //BindingOperations.SetBinding(gd.iconConfSink, IconButton.IsEnabledProperty, mbindingToIconConfSink);

            // 15. iconFromParaToConfirmGalactophore
            MultiBinding mbindingToIconFromParaToConfirmGalactophore = new MultiBinding();
            mbindingToIconFromParaToConfirmGalactophore.Mode = BindingMode.OneWay;
            mbindingToIconFromParaToConfirmGalactophore.Bindings.Add(bindingFromGalactophoreDetectorWorkStatus);
            mbindingToIconFromParaToConfirmGalactophore.Bindings.Add(bindingFromGalactophoreDetectorParameterConfirmState);
            mbindingToIconFromParaToConfirmGalactophore.Converter = convertMS2EB;
            mbindingToIconFromParaToConfirmGalactophore.ConverterParameter = new object[] { new byte[] { Byte.MaxValue }, new OperateModuleBase.WorkStatus[] { OperateModuleBase.WorkStatus.ParametersConfiguration } };
            BindingOperations.SetBinding(gd.iconFromParaToConfirmGalactophore, IconButton.IsEnabledProperty, mbindingToIconFromParaToConfirmGalactophore);

            // 16. iconConfConfirmGalactophore
            MultiBinding mbindingToIconConfConfirmGalactophore = new MultiBinding();
            mbindingToIconConfConfirmGalactophore.Mode = BindingMode.OneWay;
            mbindingToIconConfConfirmGalactophore.Bindings.Add(bindingFromGalactophoreDetectorWorkStatus);
            mbindingToIconConfConfirmGalactophore.Bindings.Add(bindingFromGalactophoreDetectorParameterConfirmState);
            mbindingToIconConfConfirmGalactophore.Converter = convertMS2EB;
            mbindingToIconConfConfirmGalactophore.ConverterParameter = new object[] { new byte[] { Byte.MaxValue }, new OperateModuleBase.WorkStatus[] { OperateModuleBase.WorkStatus.ParametersConfiguration } };
            BindingOperations.SetBinding(gd.iconConfConfirmGalactophore, IconButton.IsEnabledProperty, mbindingToIconConfConfirmGalactophore);

            // 17. iconFromConfirmToRunGalactophore
            MultiBinding mbindingToIconFromConfirmToRunGalactophore = new MultiBinding();
            mbindingToIconFromConfirmToRunGalactophore.Mode = BindingMode.OneWay;
            mbindingToIconFromConfirmToRunGalactophore.Bindings.Add(bindingFromGalactophoreDetectorWorkStatus);
            mbindingToIconFromConfirmToRunGalactophore.Bindings.Add(bindingFromGalactophoreDetectorForceSensorCleared);
            mbindingToIconFromConfirmToRunGalactophore.Bindings.Add(bindingFromGalactophoreDetectorParameterConfirm);
            mbindingToIconFromConfirmToRunGalactophore.Converter = convertMS2EB;
            mbindingToIconFromConfirmToRunGalactophore.ConverterParameter = new object[] { new byte[] { 0 }, new OperateModuleBase.WorkStatus[] { OperateModuleBase.WorkStatus.ParametersConfiguration } };
            BindingOperations.SetBinding(gd.iconFromConfirmToRunGalactophore, IconButton.IsEnabledProperty, mbindingToIconFromConfirmToRunGalactophore);

            // 18. iconFromZeroToRunGalactophore
            MultiBinding mbindingToIconFromZeroToRunGalactophore = new MultiBinding();
            mbindingToIconFromZeroToRunGalactophore.Mode = BindingMode.OneWay;
            mbindingToIconFromZeroToRunGalactophore.Bindings.Add(bindingFromGalactophoreDetectorWorkStatus);
            mbindingToIconFromZeroToRunGalactophore.Bindings.Add(bindingFromGalactophoreDetectorForceSensorCleared);
            mbindingToIconFromZeroToRunGalactophore.Bindings.Add(bindingFromGalactophoreDetectorParameterConfirm);
            mbindingToIconFromZeroToRunGalactophore.Converter = convertMS2EB;
            mbindingToIconFromZeroToRunGalactophore.ConverterParameter = new object[] { new byte[] { 0 }, new OperateModuleBase.WorkStatus[] { OperateModuleBase.WorkStatus.InitialForceDevice } };
            BindingOperations.SetBinding(gd.iconFromZeroToRunGalactophore, IconButton.IsEnabledProperty, mbindingToIconFromZeroToRunGalactophore);

            // 19. iconBeginGalactophore
            MultiBinding mbindingToIconBeginGalactophore = new MultiBinding();
            mbindingToIconBeginGalactophore.Mode = BindingMode.OneWay;
            mbindingToIconBeginGalactophore.Bindings.Add(bindingFromGalactophoreDetectorWorkStatus);
            mbindingToIconBeginGalactophore.Bindings.Add(bindingFromGalactophoreDetectorForceSensorCleared);
            mbindingToIconBeginGalactophore.Bindings.Add(bindingFromGalactophoreDetectorParameterConfirm);
            mbindingToIconBeginGalactophore.Converter = convertMS2EB;
            mbindingToIconBeginGalactophore.ConverterParameter = new object[] { new byte[] { 0 }, new OperateModuleBase.WorkStatus[] { OperateModuleBase.WorkStatus.InitialForceDevice, OperateModuleBase.WorkStatus.ParametersConfiguration } };
            BindingOperations.SetBinding(gd.iconBeginGalactophore, IconButton.IsEnabledProperty, mbindingToIconBeginGalactophore);

            // 20. nippleNextBtn
            MultiBinding mbindingToNippleNextBtn = new MultiBinding();
            mbindingToNippleNextBtn.Mode = BindingMode.OneWay;
            mbindingToNippleNextBtn.Bindings.Add(bindingFromGalactophoreDetectorWorkStatus);
            mbindingToNippleNextBtn.Bindings.Add(bindingFromGalactophoreDetectorParameterConfirmState);
            mbindingToNippleNextBtn.Converter = convertMS2EB;
            mbindingToNippleNextBtn.ConverterParameter = new object[] { new byte[] { 1 }, new OperateModuleBase.WorkStatus[] { OperateModuleBase.WorkStatus.ParametersConfiguration } };
            BindingOperations.SetBinding(gd.nippleNextBtn, IconButton.IsEnabledProperty, mbindingToNippleNextBtn);

            // 21. liftDistanceNextBtn
            MultiBinding mbindingToLiftDistanceNextBtn = new MultiBinding();
            mbindingToLiftDistanceNextBtn.Mode = BindingMode.OneWay;
            mbindingToLiftDistanceNextBtn.Bindings.Add(bindingFromGalactophoreDetectorWorkStatus);
            mbindingToLiftDistanceNextBtn.Bindings.Add(bindingFromGalactophoreDetectorParameterConfirmState);
            mbindingToLiftDistanceNextBtn.Converter = convertMS2EB;
            mbindingToLiftDistanceNextBtn.ConverterParameter = new object[] { new byte[] { 2 }, new OperateModuleBase.WorkStatus[] { OperateModuleBase.WorkStatus.ParametersConfiguration } };
            BindingOperations.SetBinding(gd.liftDistanceNextBtn, IconButton.IsEnabledProperty, mbindingToLiftDistanceNextBtn);

            // 22. minRadiusNextBtn
            MultiBinding mbindingToMinRadiusNextBtn = new MultiBinding();
            mbindingToMinRadiusNextBtn.Mode = BindingMode.OneWay;
            mbindingToMinRadiusNextBtn.Bindings.Add(bindingFromGalactophoreDetectorWorkStatus);
            mbindingToMinRadiusNextBtn.Bindings.Add(bindingFromGalactophoreDetectorParameterConfirmState);
            mbindingToMinRadiusNextBtn.Converter = convertMS2EB;
            mbindingToMinRadiusNextBtn.ConverterParameter = new object[] { new byte[] { 3 }, new OperateModuleBase.WorkStatus[] { OperateModuleBase.WorkStatus.ParametersConfiguration } };
            BindingOperations.SetBinding(gd.minRadiusNextBtn, IconButton.IsEnabledProperty, mbindingToMinRadiusNextBtn);

            // 23. scanDistanceNextBtn
            MultiBinding mbindingToScanDistanceNextBtn = new MultiBinding();
            mbindingToScanDistanceNextBtn.Mode = BindingMode.OneWay;
            mbindingToScanDistanceNextBtn.Bindings.Add(bindingFromGalactophoreDetectorWorkStatus);
            mbindingToScanDistanceNextBtn.Bindings.Add(bindingFromGalactophoreDetectorParameterConfirmState);
            mbindingToScanDistanceNextBtn.Converter = convertMS2EB;
            mbindingToScanDistanceNextBtn.ConverterParameter = new object[] { new byte[] { 4 }, new OperateModuleBase.WorkStatus[] { OperateModuleBase.WorkStatus.ParametersConfiguration } };
            BindingOperations.SetBinding(gd.scanDistanceNextBtn, IconButton.IsEnabledProperty, mbindingToScanDistanceNextBtn);

            // 24. headBoundNextBtn
            MultiBinding mbindingToHeadBoundNextBtn = new MultiBinding();
            mbindingToHeadBoundNextBtn.Mode = BindingMode.OneWay;
            mbindingToHeadBoundNextBtn.Bindings.Add(bindingFromGalactophoreDetectorWorkStatus);
            mbindingToHeadBoundNextBtn.Bindings.Add(bindingFromGalactophoreDetectorParameterConfirmState);
            mbindingToHeadBoundNextBtn.Converter = convertMS2EB;
            mbindingToHeadBoundNextBtn.ConverterParameter = new object[] { new byte[] { 5 }, new OperateModuleBase.WorkStatus[] { OperateModuleBase.WorkStatus.ParametersConfiguration } };
            BindingOperations.SetBinding(gd.headBoundNextBtn, IconButton.IsEnabledProperty, mbindingToHeadBoundNextBtn);

            // 25. tailBoundNextBtn
            MultiBinding mbindingToTailBoundNextBtn = new MultiBinding();
            mbindingToTailBoundNextBtn.Mode = BindingMode.OneWay;
            mbindingToTailBoundNextBtn.Bindings.Add(bindingFromGalactophoreDetectorWorkStatus);
            mbindingToTailBoundNextBtn.Bindings.Add(bindingFromGalactophoreDetectorParameterConfirmState);
            mbindingToTailBoundNextBtn.Converter = convertMS2EB;
            mbindingToTailBoundNextBtn.ConverterParameter = new object[] { new byte[] { 6 }, new OperateModuleBase.WorkStatus[] { OperateModuleBase.WorkStatus.ParametersConfiguration } };
            BindingOperations.SetBinding(gd.tailBoundNextBtn, IconButton.IsEnabledProperty, mbindingToTailBoundNextBtn);

            // 26. outBoundNextBtn
            MultiBinding mbindingToOutBoundNextBtn = new MultiBinding();
            mbindingToOutBoundNextBtn.Mode = BindingMode.OneWay;
            mbindingToOutBoundNextBtn.Bindings.Add(bindingFromGalactophoreDetectorWorkStatus);
            mbindingToOutBoundNextBtn.Bindings.Add(bindingFromGalactophoreDetectorParameterConfirmState);
            mbindingToOutBoundNextBtn.Converter = convertMS2EB;
            mbindingToOutBoundNextBtn.ConverterParameter = new object[] { new byte[] { 7 }, new OperateModuleBase.WorkStatus[] { OperateModuleBase.WorkStatus.ParametersConfiguration } };
            BindingOperations.SetBinding(gd.outBoundNextBtn, IconButton.IsEnabledProperty, mbindingToOutBoundNextBtn);

            // 27. inBoundNextBtn
            MultiBinding mbindingToInBoundNextBtn = new MultiBinding();
            mbindingToInBoundNextBtn.Mode = BindingMode.OneWay;
            mbindingToInBoundNextBtn.Bindings.Add(bindingFromGalactophoreDetectorWorkStatus);
            mbindingToInBoundNextBtn.Bindings.Add(bindingFromGalactophoreDetectorParameterConfirmState);
            mbindingToInBoundNextBtn.Converter = convertMS2EB;
            mbindingToInBoundNextBtn.ConverterParameter = new object[] { new byte[] { 8 }, new OperateModuleBase.WorkStatus[] { OperateModuleBase.WorkStatus.ParametersConfiguration } };
            BindingOperations.SetBinding(gd.inBoundNextBtn, IconButton.IsEnabledProperty, mbindingToInBoundNextBtn);

            // 28. sinkDistanceNextBtn
            //MultiBinding mbindingToSinkDistanceNextBtn = new MultiBinding();
            //mbindingToSinkDistanceNextBtn.Mode = BindingMode.OneWay;
            //mbindingToSinkDistanceNextBtn.Bindings.Add(bindingFromGalactophoreDetectorWorkStatus);
            //mbindingToSinkDistanceNextBtn.Bindings.Add(bindingFromGalactophoreDetectorParameterConfirmState);
            //mbindingToSinkDistanceNextBtn.Converter = convertMS2EB;
            //mbindingToSinkDistanceNextBtn.ConverterParameter = new object[] { new byte[] { 9 }, new OperateModuleBase.WorkStatus[] { OperateModuleBase.WorkStatus.ParametersConfiguration } };
            //BindingOperations.SetBinding(gd.sinkDistanceNextBtn, IconButton.IsEnabledProperty, mbindingToSinkDistanceNextBtn);
        }

        /// <summary>
        /// 绑定域 --| GalactophoreDetector Parameter Change Move |-- 内元素
        /// </summary>
        private void BindingItemsGalactophoreDetectorMoveEnable()
        {
            // 绑定：BreastScanConfMovingEnable {属性} ==> iconMoveSettingGalactophore {GalactophorDetect控件}
            Binding bindingFromBreastScanConfMovingEnableToIconMoveSettingGalactophore = new Binding();
            bindingFromBreastScanConfMovingEnableToIconMoveSettingGalactophore.Source = this;
            bindingFromBreastScanConfMovingEnableToIconMoveSettingGalactophore.Path = new PropertyPath("BreastScanConfMovingEnable");
            bindingFromBreastScanConfMovingEnableToIconMoveSettingGalactophore.Mode = BindingMode.OneWay;
            bindingFromBreastScanConfMovingEnableToIconMoveSettingGalactophore.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            BindingOperations.SetBinding(gd.iconMoveSettingGalactophore, Grid.IsEnabledProperty, bindingFromBreastScanConfMovingEnableToIconMoveSettingGalactophore);
        }

        /// <summary>
        /// 绑定域 --| Start Position At Thyroid Scanning |-- 内元素
        /// </summary>
        private void BindingItemsStartPositionAtThyroidScanning()
        {
            // 绑定：StartPositionTSR {属性} ==> startThyroidX {ThyroidScan控件}
            Binding bindingFromStartPositionTSRToStartThyroidX = new Binding();
            bindingFromStartPositionTSRToStartThyroidX.Source = this;
            bindingFromStartPositionTSRToStartThyroidX.Path = new PropertyPath("StartPositionTSR");
            bindingFromStartPositionTSRToStartThyroidX.Mode = BindingMode.OneWay;
            bindingFromStartPositionTSRToStartThyroidX.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            bindingFromStartPositionTSRToStartThyroidX.Converter = convertDA2S;
            bindingFromStartPositionTSRToStartThyroidX.ConverterParameter = new int[] { 0, 1000, 2 };
            BindingOperations.SetBinding(ts.startThyroidX, TextBox.TextProperty, bindingFromStartPositionTSRToStartThyroidX);

            // 绑定：StartPositionTSR {属性} ==> startThyroidY {ThyroidScan控件}
            Binding bindingFromStartPositionTSRToStartThyroidY = new Binding();
            bindingFromStartPositionTSRToStartThyroidY.Source = this;
            bindingFromStartPositionTSRToStartThyroidY.Path = new PropertyPath("StartPositionTSR");
            bindingFromStartPositionTSRToStartThyroidY.Mode = BindingMode.OneWay;
            bindingFromStartPositionTSRToStartThyroidY.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            bindingFromStartPositionTSRToStartThyroidY.Converter = convertDA2S;
            bindingFromStartPositionTSRToStartThyroidY.ConverterParameter = new int[] { 1, 1000, 2 };
            BindingOperations.SetBinding(ts.startThyroidY, TextBox.TextProperty, bindingFromStartPositionTSRToStartThyroidY);

            // 绑定：StartPositionTSR {属性} ==> startThyroidZ {ThyroidScan控件}
            Binding bindingFromStartPositionTSRToStartThyroidZ = new Binding();
            bindingFromStartPositionTSRToStartThyroidZ.Source = this;
            bindingFromStartPositionTSRToStartThyroidZ.Path = new PropertyPath("StartPositionTSR");
            bindingFromStartPositionTSRToStartThyroidZ.Mode = BindingMode.OneWay;
            bindingFromStartPositionTSRToStartThyroidZ.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            bindingFromStartPositionTSRToStartThyroidZ.Converter = convertDA2S;
            bindingFromStartPositionTSRToStartThyroidZ.ConverterParameter = new int[] { 2, 1000, 2 };
            BindingOperations.SetBinding(ts.startThyroidZ, TextBox.TextProperty, bindingFromStartPositionTSRToStartThyroidZ);
        }

        /// <summary>
        /// 绑定域 --| Configuration Parameters Of ThyroidScanner --> Press Direction Parameters |-- 内元素
        /// </summary>
        private void BindingItemsConfigurationParametersOfThyroidScannerPressDirectionParameters()
        {
            // 绑定：DetectingErrorForceMinTSR {属性} ==> minForceSliderThyroid {Flyout控件}
            Binding bindingFromDetectingErrorForceMinTSRToMinForceSliderThyroid = new Binding();
            bindingFromDetectingErrorForceMinTSRToMinForceSliderThyroid.Source = this;
            bindingFromDetectingErrorForceMinTSRToMinForceSliderThyroid.Path = new PropertyPath("DetectingErrorForceMinTSR");
            bindingFromDetectingErrorForceMinTSRToMinForceSliderThyroid.Mode = BindingMode.OneWay;
            bindingFromDetectingErrorForceMinTSRToMinForceSliderThyroid.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            bindingFromDetectingErrorForceMinTSRToMinForceSliderThyroid.Converter = convertD2DI;
            bindingFromDetectingErrorForceMinTSRToMinForceSliderThyroid.ConverterParameter = new double[] { 0.0, 4.0 };
            BindingOperations.SetBinding(mw.minForceSliderThyroid, Slider.ValueProperty, bindingFromDetectingErrorForceMinTSRToMinForceSliderThyroid);

            // 绑定：DetectingErrorForceMaxTSR {属性} ==> maxForceSliderThyroid {Flyout控件}
            Binding bindingFromDetectingErrorForceMaxTSRToMaxForceSliderThyroid = new Binding();
            bindingFromDetectingErrorForceMaxTSRToMaxForceSliderThyroid.Source = this;
            bindingFromDetectingErrorForceMaxTSRToMaxForceSliderThyroid.Path = new PropertyPath("DetectingErrorForceMaxTSR");
            bindingFromDetectingErrorForceMaxTSRToMaxForceSliderThyroid.Mode = BindingMode.OneWay;
            bindingFromDetectingErrorForceMaxTSRToMaxForceSliderThyroid.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            bindingFromDetectingErrorForceMaxTSRToMaxForceSliderThyroid.Converter = convertD2DI;
            bindingFromDetectingErrorForceMaxTSRToMaxForceSliderThyroid.ConverterParameter = new double[] { -1.5, 2.0 };
            BindingOperations.SetBinding(mw.maxForceSliderThyroid, Slider.ValueProperty, bindingFromDetectingErrorForceMaxTSRToMaxForceSliderThyroid);

            // 绑定：DetectingSpeedMinTSR {属性} ==> minDetectSpeedSliderThyroid {Flyout控件}
            Binding bindingFromDetectingSpeedMinTSRToMinDetectSpeedSliderThyroid = new Binding();
            bindingFromDetectingSpeedMinTSRToMinDetectSpeedSliderThyroid.Source = this;
            bindingFromDetectingSpeedMinTSRToMinDetectSpeedSliderThyroid.Path = new PropertyPath("DetectingSpeedMinTSR");
            bindingFromDetectingSpeedMinTSRToMinDetectSpeedSliderThyroid.Mode = BindingMode.OneWay;
            bindingFromDetectingSpeedMinTSRToMinDetectSpeedSliderThyroid.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            bindingFromDetectingSpeedMinTSRToMinDetectSpeedSliderThyroid.Converter = convertD2DI;
            bindingFromDetectingSpeedMinTSRToMinDetectSpeedSliderThyroid.ConverterParameter = new double[] { 0.0, 10000.0 };
            BindingOperations.SetBinding(mw.minDetectSpeedSliderThyroid, Slider.ValueProperty, bindingFromDetectingSpeedMinTSRToMinDetectSpeedSliderThyroid);

            // 绑定：DetectingSpeedMaxTSR {属性} ==> maxDetectSpeedSliderThyroid {Flyout控件}
            Binding bindingFromDetectingSpeedMaxTSRToMaxDetectSpeedSliderThyroid = new Binding();
            bindingFromDetectingSpeedMaxTSRToMaxDetectSpeedSliderThyroid.Source = this;
            bindingFromDetectingSpeedMaxTSRToMaxDetectSpeedSliderThyroid.Path = new PropertyPath("DetectingSpeedMaxTSR");
            bindingFromDetectingSpeedMaxTSRToMaxDetectSpeedSliderThyroid.Mode = BindingMode.OneWay;
            bindingFromDetectingSpeedMaxTSRToMaxDetectSpeedSliderThyroid.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            bindingFromDetectingSpeedMaxTSRToMaxDetectSpeedSliderThyroid.Converter = convertD2DI;
            bindingFromDetectingSpeedMaxTSRToMaxDetectSpeedSliderThyroid.ConverterParameter = new double[] { -0.0002, 10000.0 };
            BindingOperations.SetBinding(mw.maxDetectSpeedSliderThyroid, Slider.ValueProperty, bindingFromDetectingSpeedMaxTSRToMaxDetectSpeedSliderThyroid);

            // 绑定：IfEnableForceKeepingTSR {属性} ==> enableFosKeepSwitch {ThyroidScan控件}
            Binding bindingFromIfEnableForceKeepingTSRToEnableFosKeepSwitch = new Binding();
            bindingFromIfEnableForceKeepingTSRToEnableFosKeepSwitch.Source = this;
            bindingFromIfEnableForceKeepingTSRToEnableFosKeepSwitch.Path = new PropertyPath("IfEnableForceKeepingTSR");
            bindingFromIfEnableForceKeepingTSRToEnableFosKeepSwitch.Mode = BindingMode.OneWay;
            bindingFromIfEnableForceKeepingTSRToEnableFosKeepSwitch.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            BindingOperations.SetBinding(ts.enableFosKeepSwitch, ToggleSwitch.IsCheckedProperty, bindingFromIfEnableForceKeepingTSRToEnableFosKeepSwitch);

            // 绑定：IfEnableForceTrackingTSR {属性} ==> enableFosTrackSwitch {ThyroidScan控件}
            Binding bindingFromIfEnableForceTrackingTSRToEnableFosTrackSwitch = new Binding();
            bindingFromIfEnableForceTrackingTSRToEnableFosTrackSwitch.Source = this;
            bindingFromIfEnableForceTrackingTSRToEnableFosTrackSwitch.Path = new PropertyPath("IfEnableForceTrackingTSR");
            bindingFromIfEnableForceTrackingTSRToEnableFosTrackSwitch.Mode = BindingMode.OneWay;
            bindingFromIfEnableForceTrackingTSRToEnableFosTrackSwitch.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            BindingOperations.SetBinding(ts.enableFosTrackSwitch, ToggleSwitch.IsCheckedProperty, bindingFromIfEnableForceTrackingTSRToEnableFosTrackSwitch);

            // 绑定：DetectingBasicPreservedForceTSR {属性} ==> holdingPressureThyroid {Flyout控件}
            Binding bindingFromDetectingBasicPreservedForceTSRToHoldingPressureThyroid = new Binding();
            bindingFromDetectingBasicPreservedForceTSRToHoldingPressureThyroid.Source = this;
            bindingFromDetectingBasicPreservedForceTSRToHoldingPressureThyroid.Path = new PropertyPath("DetectingBasicPreservedForceTSR");
            bindingFromDetectingBasicPreservedForceTSRToHoldingPressureThyroid.Mode = BindingMode.OneWay;
            bindingFromDetectingBasicPreservedForceTSRToHoldingPressureThyroid.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            bindingFromDetectingBasicPreservedForceTSRToHoldingPressureThyroid.Converter = convertD2DI;
            bindingFromDetectingBasicPreservedForceTSRToHoldingPressureThyroid.ConverterParameter = new double[] { -3.0, 1.0 };
            BindingOperations.SetBinding(mw.holdingPressureThyroid, Slider.ValueProperty, bindingFromDetectingBasicPreservedForceTSRToHoldingPressureThyroid);
        }

        /// <summary>
        /// 绑定域 --| Configuration Parameters Of ThyroidScanner --> Limit Parameters |-- 内元素
        /// </summary>
        private void BindingItemsConfigurationParametersOfThyroidScannerLimitParameters()
        {
            // 绑定：MaxAvailableRadiusTSR {属性} ==> maxRadiusThyroid {Flyout控件}
            Binding bindingFromMaxAvailableRadiusTSRToMaxRadiusThyroid = new Binding();
            bindingFromMaxAvailableRadiusTSRToMaxRadiusThyroid.Source = this;
            bindingFromMaxAvailableRadiusTSRToMaxRadiusThyroid.Path = new PropertyPath("MaxAvailableRadiusTSR");
            bindingFromMaxAvailableRadiusTSRToMaxRadiusThyroid.Mode = BindingMode.OneWay;
            bindingFromMaxAvailableRadiusTSRToMaxRadiusThyroid.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            bindingFromMaxAvailableRadiusTSRToMaxRadiusThyroid.Converter = convertD2DI;
            bindingFromMaxAvailableRadiusTSRToMaxRadiusThyroid.ConverterParameter = new double[] { -0.3, 20.0 };
            BindingOperations.SetBinding(mw.maxRadiusThyroid, Slider.ValueProperty, bindingFromMaxAvailableRadiusTSRToMaxRadiusThyroid);

            // 绑定：MaxAvailableAngleTSR {属性} ==> maxAngleThyroid {Flyout控件}
            Binding bindingFromMaxAvailableAngleTSRToMaxAngleThyroid = new Binding();
            bindingFromMaxAvailableAngleTSRToMaxAngleThyroid.Source = this;
            bindingFromMaxAvailableAngleTSRToMaxAngleThyroid.Path = new PropertyPath("MaxAvailableAngleTSR");
            bindingFromMaxAvailableAngleTSRToMaxAngleThyroid.Mode = BindingMode.OneWay;
            bindingFromMaxAvailableAngleTSRToMaxAngleThyroid.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            bindingFromMaxAvailableAngleTSRToMaxAngleThyroid.Converter = convertD2DI;
            bindingFromMaxAvailableAngleTSRToMaxAngleThyroid.ConverterParameter = new double[] { -45 * degToRadRatio, 1 / (15 * degToRadRatio) };
            BindingOperations.SetBinding(mw.maxAngleThyroid, Slider.ValueProperty, bindingFromMaxAvailableAngleTSRToMaxAngleThyroid);

            // 绑定：StopRadiusTSR {属性} ==> stopDistanceThyroid {Flyout控件}
            Binding bindingFromStopRadiusTSRToStopDistanceThyroid = new Binding();
            bindingFromStopRadiusTSRToStopDistanceThyroid.Source = this;
            bindingFromStopRadiusTSRToStopDistanceThyroid.Path = new PropertyPath("StopRadiusTSR");
            bindingFromStopRadiusTSRToStopDistanceThyroid.Mode = BindingMode.OneWay;
            bindingFromStopRadiusTSRToStopDistanceThyroid.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            bindingFromStopRadiusTSRToStopDistanceThyroid.Converter = convertD2DI;
            bindingFromStopRadiusTSRToStopDistanceThyroid.ConverterParameter = new double[] { -0.4, 20.0 };
            BindingOperations.SetBinding(mw.stopDistanceThyroid, Slider.ValueProperty, bindingFromStopRadiusTSRToStopDistanceThyroid);

            // 绑定：MaxDistPeriodTSR {属性} ==> maxLoopDistThyroid {Flyout控件}
            Binding bindingFromMaxDistPeriodTSRToMaxLoopDistThyroid = new Binding();
            bindingFromMaxDistPeriodTSRToMaxLoopDistThyroid.Source = this;
            bindingFromMaxDistPeriodTSRToMaxLoopDistThyroid.Path = new PropertyPath("MaxDistPeriodTSR");
            bindingFromMaxDistPeriodTSRToMaxLoopDistThyroid.Mode = BindingMode.OneWay;
            bindingFromMaxDistPeriodTSRToMaxLoopDistThyroid.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            bindingFromMaxDistPeriodTSRToMaxLoopDistThyroid.Converter = convertD2DI;
            bindingFromMaxDistPeriodTSRToMaxLoopDistThyroid.ConverterParameter = new double[] { -0.0002, 10000.0 };
            BindingOperations.SetBinding(mw.maxLoopDistThyroid, Slider.ValueProperty, bindingFromMaxDistPeriodTSRToMaxLoopDistThyroid);

            // 绑定：MaxAnglePeriodTSR {属性} ==> maxLoopAngleThyroid {Flyout控件}
            Binding bindingFromMaxAnglePeriodTSRToMaxLoopAngleThyroid = new Binding();
            bindingFromMaxAnglePeriodTSRToMaxLoopAngleThyroid.Source = this;
            bindingFromMaxAnglePeriodTSRToMaxLoopAngleThyroid.Path = new PropertyPath("MaxAnglePeriodTSR");
            bindingFromMaxAnglePeriodTSRToMaxLoopAngleThyroid.Mode = BindingMode.OneWay;
            bindingFromMaxAnglePeriodTSRToMaxLoopAngleThyroid.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            bindingFromMaxAnglePeriodTSRToMaxLoopAngleThyroid.Converter = convertD2DI;
            bindingFromMaxAnglePeriodTSRToMaxLoopAngleThyroid.ConverterParameter = new double[] { -0.03 * degToRadRatio, 1 / (0.02 * degToRadRatio) };
            BindingOperations.SetBinding(mw.maxLoopAngleThyroid, Slider.ValueProperty, bindingFromMaxAnglePeriodTSRToMaxLoopAngleThyroid);
        }

        /// <summary>
        /// 绑定域 --| Configuration Parameters Of ThyroidScanner --> Factor |-- 内元素
        /// </summary>
        private void BindingItemsConfigurationParametersOfThyroidScannerFactor()
        {
            // 绑定：PositionOverrideTSR {属性} ==> factorPosSlider {ThyroidScan控件}
            Binding bindingFromPositionOverrideTSRToFactorPosSlider = new Binding();
            bindingFromPositionOverrideTSRToFactorPosSlider.Source = this;
            bindingFromPositionOverrideTSRToFactorPosSlider.Path = new PropertyPath("PositionOverrideTSR");
            bindingFromPositionOverrideTSRToFactorPosSlider.Mode = BindingMode.OneWay;
            bindingFromPositionOverrideTSRToFactorPosSlider.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            bindingFromPositionOverrideTSRToFactorPosSlider.Converter = convertD2DI;
            bindingFromPositionOverrideTSRToFactorPosSlider.ConverterParameter = new double[] { -0.25, 4.0 };
            BindingOperations.SetBinding(ts.factorPosSlider, Slider.ValueProperty, bindingFromPositionOverrideTSRToFactorPosSlider);

            // 绑定：AngleOverrideTSR {属性} ==> factorAttSlider {ThyroidScan控件}
            Binding bindingFromAngleOverrideTSRToFactorAttSliderd = new Binding();
            bindingFromAngleOverrideTSRToFactorAttSliderd.Source = this;
            bindingFromAngleOverrideTSRToFactorAttSliderd.Path = new PropertyPath("AngleOverrideTSR");
            bindingFromAngleOverrideTSRToFactorAttSliderd.Mode = BindingMode.OneWay;
            bindingFromAngleOverrideTSRToFactorAttSliderd.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            bindingFromAngleOverrideTSRToFactorAttSliderd.Converter = convertD2DI;
            bindingFromAngleOverrideTSRToFactorAttSliderd.ConverterParameter = new double[] { -0.25, 4.0 };
            BindingOperations.SetBinding(ts.factorAttSlider, Slider.ValueProperty, bindingFromAngleOverrideTSRToFactorAttSliderd);

            // 绑定：ForceOverrideTSR {属性} ==> factorFosSlider {ThyroidScan控件}
            Binding bindingFromForceOverrideTSRToFactorFosSlider = new Binding();
            bindingFromForceOverrideTSRToFactorFosSlider.Source = this;
            bindingFromForceOverrideTSRToFactorFosSlider.Path = new PropertyPath("ForceOverrideTSR");
            bindingFromForceOverrideTSRToFactorFosSlider.Mode = BindingMode.OneWay;
            bindingFromForceOverrideTSRToFactorFosSlider.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            bindingFromForceOverrideTSRToFactorFosSlider.Converter = convertD2DI;
            bindingFromForceOverrideTSRToFactorFosSlider.ConverterParameter = new double[] { -0.5, 2.0 };
            BindingOperations.SetBinding(ts.factorFosSlider, Slider.ValueProperty, bindingFromForceOverrideTSRToFactorFosSlider);
        }

        /// <summary>
        /// 绑定域 --| Configuration Parameters Of ThyroidScanner --> Other Switch |-- 内元素
        /// </summary>
        private void BindingItemsConfigurationParametersOfThyroidScannerOtherSwitch()
        {
            // 绑定：IfEnableAttitudeTrackingTSR {属性} ==> enableAttSwitch {ThyroidScan控件}
            Binding bindingFromIfEnableAttitudeTrackingTSRToEnableAttSwitch = new Binding();
            bindingFromIfEnableAttitudeTrackingTSRToEnableAttSwitch.Source = this;
            bindingFromIfEnableAttitudeTrackingTSRToEnableAttSwitch.Path = new PropertyPath("IfEnableAttitudeTrackingTSR");
            bindingFromIfEnableAttitudeTrackingTSRToEnableAttSwitch.Mode = BindingMode.OneWay;
            bindingFromIfEnableAttitudeTrackingTSRToEnableAttSwitch.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            BindingOperations.SetBinding(ts.enableAttSwitch, ToggleSwitch.IsCheckedProperty, bindingFromIfEnableAttitudeTrackingTSRToEnableAttSwitch);

            // 绑定：IfEnableTranslationTrackingTSR {属性} ==> enablePosSwitch {ThyroidScan控件}
            Binding bindingFromIfEnableTranslationTrackingTSRToEnablePosSwitch = new Binding();
            bindingFromIfEnableTranslationTrackingTSRToEnablePosSwitch.Source = this;
            bindingFromIfEnableTranslationTrackingTSRToEnablePosSwitch.Path = new PropertyPath("IfEnableTranslationTrackingTSR");
            bindingFromIfEnableTranslationTrackingTSRToEnablePosSwitch.Mode = BindingMode.OneWay;
            bindingFromIfEnableTranslationTrackingTSRToEnablePosSwitch.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            BindingOperations.SetBinding(ts.enablePosSwitch, ToggleSwitch.IsCheckedProperty, bindingFromIfEnableTranslationTrackingTSRToEnablePosSwitch);
        }

        /// <summary>
        /// 联合绑定
        /// 绑定域 --| ThyroidScanner Working Status |-- 内元素
        /// 绑定域 --| ThyroidScanner Paramete Confirm |-- 内元素
        /// 绑定域 --| ThyroidScanner ForceSenor Cleared |-- 内元素
        /// 绑定域 --| ThyroidScanner Parameter Confirm State |-- 内元素
        /// </summary>
        private void BindingItemsThyroidScannerWorkingStatus()
        {
            // 绑定：ThyroidScannerWorkStatus {属性} ==> {i} {ThyroidScan控件}
            Binding bindingFromThyroidScannerWorkStatus = new Binding();
            bindingFromThyroidScannerWorkStatus.Source = this;
            bindingFromThyroidScannerWorkStatus.Path = new PropertyPath("ThyroidScannerWorkStatus");
            bindingFromThyroidScannerWorkStatus.Mode = BindingMode.OneWay;
            bindingFromThyroidScannerWorkStatus.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;

            // 绑定：ThyroidScannerParameterConfirm {属性} ==> {i} {ThyroidScan控件}
            Binding bindingFromThyroidScannerParameterConfirm = new Binding();
            bindingFromThyroidScannerParameterConfirm.Source = this;
            bindingFromThyroidScannerParameterConfirm.Path = new PropertyPath("ThyroidScannerParameterConfirm");
            bindingFromThyroidScannerParameterConfirm.Mode = BindingMode.OneWay;
            bindingFromThyroidScannerParameterConfirm.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;

            // 绑定：ThyroidScannerForceSensorCleared {属性} ==> {i} {ThyroidScan控件}
            Binding bindingFromThyroidScannerForceSensorCleared = new Binding();
            bindingFromThyroidScannerForceSensorCleared.Source = this;
            bindingFromThyroidScannerForceSensorCleared.Path = new PropertyPath("ThyroidScannerForceSensorCleared");
            bindingFromThyroidScannerForceSensorCleared.Mode = BindingMode.OneWay;
            bindingFromThyroidScannerForceSensorCleared.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;

            // 绑定：ThyroidScannerParameterConfirmState {属性} ==> {i} {ThyroidScan控件}
            Binding bindingFromThyroidScannerParameterConfirmState = new Binding();
            bindingFromThyroidScannerParameterConfirmState.Source = this;
            bindingFromThyroidScannerParameterConfirmState.Path = new PropertyPath("ThyroidScannerParameterConfirmState");
            bindingFromThyroidScannerParameterConfirmState.Mode = BindingMode.OneWay;
            bindingFromThyroidScannerParameterConfirmState.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;

            // 1. iconBackThyroid
            MultiBinding mbindingToIconBackThyroid = new MultiBinding();
            mbindingToIconBackThyroid.Mode = BindingMode.OneWay;
            mbindingToIconBackThyroid.Bindings.Add(bindingFromThyroidScannerWorkStatus);
            mbindingToIconBackThyroid.Converter = convertMS2EB;
            mbindingToIconBackThyroid.ConverterParameter = new object[] { new byte[] { 0 }, new OperateModuleBase.WorkStatus[] { OperateModuleBase.WorkStatus.CanDoWork } };
            BindingOperations.SetBinding(ts.iconBackThyroid, IconButton.IsEnabledProperty, mbindingToIconBackThyroid);

            // 2. iconForceToZeroThyroid
            MultiBinding mbindingToIconForceToZeroThyroid = new MultiBinding();
            mbindingToIconForceToZeroThyroid.Mode = BindingMode.OneWay;
            mbindingToIconForceToZeroThyroid.Bindings.Add(bindingFromThyroidScannerWorkStatus);
            mbindingToIconForceToZeroThyroid.Converter = convertMS2EB;
            mbindingToIconForceToZeroThyroid.ConverterParameter = new object[] { new byte[] { 0 }, new OperateModuleBase.WorkStatus[] { OperateModuleBase.WorkStatus.CanDoWork } };
            BindingOperations.SetBinding(ts.iconForceToZeroThyroid, IconButton.IsEnabledProperty, mbindingToIconForceToZeroThyroid);

            // 3. iconFromZeroToConfThyroid
            MultiBinding mbindingToIconFromZeroToConfThyroid = new MultiBinding();
            mbindingToIconFromZeroToConfThyroid.Mode = BindingMode.OneWay;
            mbindingToIconFromZeroToConfThyroid.Bindings.Add(bindingFromThyroidScannerWorkStatus);
            mbindingToIconFromZeroToConfThyroid.Bindings.Add(bindingFromThyroidScannerForceSensorCleared);
            mbindingToIconFromZeroToConfThyroid.Converter = convertMS2EB;
            mbindingToIconFromZeroToConfThyroid.ConverterParameter = new object[] { new byte[] { 0 }, new OperateModuleBase.WorkStatus[] { OperateModuleBase.WorkStatus.InitialForceDevice } };
            BindingOperations.SetBinding(ts.iconFromZeroToConfThyroid, IconButton.IsEnabledProperty, mbindingToIconFromZeroToConfThyroid);

            // 4. iconConfThyroid
            MultiBinding mbindingToIconConfThyroid = new MultiBinding();
            mbindingToIconConfThyroid.Mode = BindingMode.OneWay;
            mbindingToIconConfThyroid.Bindings.Add(bindingFromThyroidScannerWorkStatus);
            mbindingToIconConfThyroid.Bindings.Add(bindingFromThyroidScannerForceSensorCleared);
            mbindingToIconConfThyroid.Converter = convertMS2EB;
            mbindingToIconConfThyroid.ConverterParameter = new object[] { new byte[] { 0 }, new OperateModuleBase.WorkStatus[] { OperateModuleBase.WorkStatus.InitialForceDevice } };
            BindingOperations.SetBinding(ts.iconConfThyroid, IconButton.IsEnabledProperty, mbindingToIconConfThyroid);

            // 5. iconFromConfToParaThyroid
            MultiBinding mbindingToIconFromConfToParaThyroid = new MultiBinding();
            mbindingToIconFromConfToParaThyroid.Mode = BindingMode.OneWay;
            mbindingToIconFromConfToParaThyroid.Bindings.Add(bindingFromThyroidScannerWorkStatus);
            mbindingToIconFromConfToParaThyroid.Bindings.Add(bindingFromThyroidScannerParameterConfirmState);
            mbindingToIconFromConfToParaThyroid.Converter = convertMS2EB;
            mbindingToIconFromConfToParaThyroid.ConverterParameter = new object[] { new byte[] { 1 }, new OperateModuleBase.WorkStatus[] { OperateModuleBase.WorkStatus.ParametersConfiguration } };
            BindingOperations.SetBinding(ts.iconFromConfToParaThyroid, IconButton.IsEnabledProperty, mbindingToIconFromConfToParaThyroid);

            // 6. iconConfStartThyroid
            MultiBinding mbindingToIconConfStartThyroid = new MultiBinding();
            mbindingToIconConfStartThyroid.Mode = BindingMode.OneWay;
            mbindingToIconConfStartThyroid.Bindings.Add(bindingFromThyroidScannerWorkStatus);
            mbindingToIconConfStartThyroid.Bindings.Add(bindingFromThyroidScannerParameterConfirmState);
            mbindingToIconConfStartThyroid.Converter = convertMS2EB;
            mbindingToIconConfStartThyroid.ConverterParameter = new object[] { new byte[] { 1, Byte.MaxValue }, new OperateModuleBase.WorkStatus[] { OperateModuleBase.WorkStatus.ParametersConfiguration } };
            BindingOperations.SetBinding(ts.iconConfStartThyroid, IconButton.IsEnabledProperty, mbindingToIconConfStartThyroid);

            // 7. iconConfFactorPos
            MultiBinding mbindingToIconConfFactorPos = new MultiBinding();
            mbindingToIconConfFactorPos.Mode = BindingMode.OneWay;
            mbindingToIconConfFactorPos.Bindings.Add(bindingFromThyroidScannerWorkStatus);
            mbindingToIconConfFactorPos.Bindings.Add(bindingFromThyroidScannerParameterConfirmState);
            mbindingToIconConfFactorPos.Converter = convertMS2EB;
            mbindingToIconConfFactorPos.ConverterParameter = new object[] { new byte[] { 2, Byte.MaxValue, Byte.MaxValue - 1 }, new OperateModuleBase.WorkStatus[] { OperateModuleBase.WorkStatus.ParametersConfiguration, OperateModuleBase.WorkStatus.WorkRunning } };
            BindingOperations.SetBinding(ts.iconConfFactorPos, IconButton.IsEnabledProperty, mbindingToIconConfFactorPos);

            // 8. iconConfFactorAtt
            MultiBinding mbindingToIconConfFactorAtt = new MultiBinding();
            mbindingToIconConfFactorAtt.Mode = BindingMode.OneWay;
            mbindingToIconConfFactorAtt.Bindings.Add(bindingFromThyroidScannerWorkStatus);
            mbindingToIconConfFactorAtt.Bindings.Add(bindingFromThyroidScannerParameterConfirmState);
            mbindingToIconConfFactorAtt.Converter = convertMS2EB;
            mbindingToIconConfFactorAtt.ConverterParameter = new object[] { new byte[] { 3, Byte.MaxValue, Byte.MaxValue - 1 }, new OperateModuleBase.WorkStatus[] { OperateModuleBase.WorkStatus.ParametersConfiguration, OperateModuleBase.WorkStatus.WorkRunning } };
            BindingOperations.SetBinding(ts.iconConfFactorAtt, IconButton.IsEnabledProperty, mbindingToIconConfFactorAtt);

            // 9. iconConfFactorFos
            MultiBinding mbindingToIconConfFactorFos = new MultiBinding();
            mbindingToIconConfFactorFos.Mode = BindingMode.OneWay;
            mbindingToIconConfFactorFos.Bindings.Add(bindingFromThyroidScannerWorkStatus);
            mbindingToIconConfFactorFos.Bindings.Add(bindingFromThyroidScannerParameterConfirmState);
            mbindingToIconConfFactorFos.Converter = convertMS2EB;
            mbindingToIconConfFactorFos.ConverterParameter = new object[] { new byte[] { 4, Byte.MaxValue, Byte.MaxValue - 1 }, new OperateModuleBase.WorkStatus[] { OperateModuleBase.WorkStatus.ParametersConfiguration, OperateModuleBase.WorkStatus.WorkRunning } };
            BindingOperations.SetBinding(ts.iconConfFactorFos, IconButton.IsEnabledProperty, mbindingToIconConfFactorFos);

            // 10. iconConfEnablePos
            MultiBinding mbindingToIconConfEnablePos = new MultiBinding();
            mbindingToIconConfEnablePos.Mode = BindingMode.OneWay;
            mbindingToIconConfEnablePos.Bindings.Add(bindingFromThyroidScannerWorkStatus);
            mbindingToIconConfEnablePos.Bindings.Add(bindingFromThyroidScannerParameterConfirmState);
            mbindingToIconConfEnablePos.Converter = convertMS2EB;
            mbindingToIconConfEnablePos.ConverterParameter = new object[] { new byte[] { 5, Byte.MaxValue, Byte.MaxValue - 1 }, new OperateModuleBase.WorkStatus[] { OperateModuleBase.WorkStatus.ParametersConfiguration, OperateModuleBase.WorkStatus.WorkRunning } };
            BindingOperations.SetBinding(ts.iconConfEnablePos, IconButton.IsEnabledProperty, mbindingToIconConfEnablePos);

            // 11. iconConfEnableAtt
            MultiBinding mbindingToIconConfEnableAtt = new MultiBinding();
            mbindingToIconConfEnableAtt.Mode = BindingMode.OneWay;
            mbindingToIconConfEnableAtt.Bindings.Add(bindingFromThyroidScannerWorkStatus);
            mbindingToIconConfEnableAtt.Bindings.Add(bindingFromThyroidScannerParameterConfirmState);
            mbindingToIconConfEnableAtt.Converter = convertMS2EB;
            mbindingToIconConfEnableAtt.ConverterParameter = new object[] { new byte[] { 6, Byte.MaxValue, Byte.MaxValue - 1 }, new OperateModuleBase.WorkStatus[] { OperateModuleBase.WorkStatus.ParametersConfiguration, OperateModuleBase.WorkStatus.WorkRunning } };
            BindingOperations.SetBinding(ts.iconConfEnableAtt, IconButton.IsEnabledProperty, mbindingToIconConfEnableAtt);

            // 12. iconConfEnableFKeep
            MultiBinding mbindingToIconConfEnableFKeep = new MultiBinding();
            mbindingToIconConfEnableFKeep.Mode = BindingMode.OneWay;
            mbindingToIconConfEnableFKeep.Bindings.Add(bindingFromThyroidScannerWorkStatus);
            mbindingToIconConfEnableFKeep.Bindings.Add(bindingFromThyroidScannerParameterConfirmState);
            mbindingToIconConfEnableFKeep.Converter = convertMS2EB;
            mbindingToIconConfEnableFKeep.ConverterParameter = new object[] { new byte[] { 7, Byte.MaxValue, Byte.MaxValue - 1 }, new OperateModuleBase.WorkStatus[] { OperateModuleBase.WorkStatus.ParametersConfiguration, OperateModuleBase.WorkStatus.WorkRunning } };
            BindingOperations.SetBinding(ts.iconConfEnableFKeep, IconButton.IsEnabledProperty, mbindingToIconConfEnableFKeep);

            // 13. iconConfEnableFTrack
            MultiBinding mbindingToIconConfEnableFTrack = new MultiBinding();
            mbindingToIconConfEnableFTrack.Mode = BindingMode.OneWay;
            mbindingToIconConfEnableFTrack.Bindings.Add(bindingFromThyroidScannerWorkStatus);
            mbindingToIconConfEnableFTrack.Bindings.Add(bindingFromThyroidScannerParameterConfirmState);
            mbindingToIconConfEnableFTrack.Converter = convertMS2EB;
            mbindingToIconConfEnableFTrack.ConverterParameter = new object[] { new byte[] { 8, Byte.MaxValue, Byte.MaxValue - 1 }, new OperateModuleBase.WorkStatus[] { OperateModuleBase.WorkStatus.ParametersConfiguration, OperateModuleBase.WorkStatus.WorkRunning } };
            BindingOperations.SetBinding(ts.iconConfEnableFTrack, IconButton.IsEnabledProperty, mbindingToIconConfEnableFTrack);

            // 14. iconFromParaToConfirmThyroid
            MultiBinding mbindingToIconFromParaToConfirmThyroid = new MultiBinding();
            mbindingToIconFromParaToConfirmThyroid.Mode = BindingMode.OneWay;
            mbindingToIconFromParaToConfirmThyroid.Bindings.Add(bindingFromThyroidScannerWorkStatus);
            mbindingToIconFromParaToConfirmThyroid.Bindings.Add(bindingFromThyroidScannerParameterConfirmState);
            mbindingToIconFromParaToConfirmThyroid.Converter = convertMS2EB;
            mbindingToIconFromParaToConfirmThyroid.ConverterParameter = new object[] { new byte[] { Byte.MaxValue }, new OperateModuleBase.WorkStatus[] { OperateModuleBase.WorkStatus.ParametersConfiguration } };
            BindingOperations.SetBinding(ts.iconFromParaToConfirmThyroid, IconButton.IsEnabledProperty, mbindingToIconFromParaToConfirmThyroid);

            // 15. iconConfConfirmThyroid
            MultiBinding mbindingToIconConfConfirmThyroid = new MultiBinding();
            mbindingToIconConfConfirmThyroid.Mode = BindingMode.OneWay;
            mbindingToIconConfConfirmThyroid.Bindings.Add(bindingFromThyroidScannerWorkStatus);
            mbindingToIconConfConfirmThyroid.Bindings.Add(bindingFromThyroidScannerParameterConfirmState);
            mbindingToIconConfConfirmThyroid.Converter = convertMS2EB;
            mbindingToIconConfConfirmThyroid.ConverterParameter = new object[] { new byte[] { Byte.MaxValue }, new OperateModuleBase.WorkStatus[] { OperateModuleBase.WorkStatus.ParametersConfiguration } };
            BindingOperations.SetBinding(ts.iconConfConfirmThyroid, IconButton.IsEnabledProperty, mbindingToIconConfConfirmThyroid);

            // 16. iconFromConfirmToRunThyroid
            MultiBinding mbindingToIconFromConfirmToRunThyroid = new MultiBinding();
            mbindingToIconFromConfirmToRunThyroid.Mode = BindingMode.OneWay;
            mbindingToIconFromConfirmToRunThyroid.Bindings.Add(bindingFromThyroidScannerWorkStatus);
            mbindingToIconFromConfirmToRunThyroid.Bindings.Add(bindingFromThyroidScannerForceSensorCleared);
            mbindingToIconFromConfirmToRunThyroid.Bindings.Add(bindingFromThyroidScannerParameterConfirm);
            mbindingToIconFromConfirmToRunThyroid.Converter = convertMS2EB;
            mbindingToIconFromConfirmToRunThyroid.ConverterParameter = new object[] { new byte[] { 0 }, new OperateModuleBase.WorkStatus[] { OperateModuleBase.WorkStatus.ParametersConfiguration } };
            BindingOperations.SetBinding(ts.iconFromConfirmToRunThyroid, IconButton.IsEnabledProperty, mbindingToIconFromConfirmToRunThyroid);

            // 17.iconFromZeroToRunThyroid
            MultiBinding mbindingToIconFromZeroToRunThyroid = new MultiBinding();
            mbindingToIconFromZeroToRunThyroid.Mode = BindingMode.OneWay;
            mbindingToIconFromZeroToRunThyroid.Bindings.Add(bindingFromThyroidScannerWorkStatus);
            mbindingToIconFromZeroToRunThyroid.Bindings.Add(bindingFromThyroidScannerForceSensorCleared);
            mbindingToIconFromZeroToRunThyroid.Bindings.Add(bindingFromThyroidScannerParameterConfirm);
            mbindingToIconFromZeroToRunThyroid.Converter = convertMS2EB;
            mbindingToIconFromZeroToRunThyroid.ConverterParameter = new object[] { new byte[] { 0 }, new OperateModuleBase.WorkStatus[] { OperateModuleBase.WorkStatus.InitialForceDevice } };
            BindingOperations.SetBinding(ts.iconFromZeroToRunThyroid, IconButton.IsEnabledProperty, mbindingToIconFromZeroToRunThyroid);

            // 18. iconBeginThyroid
            MultiBinding mbindingToIconBeginThyroid = new MultiBinding();
            mbindingToIconBeginThyroid.Mode = BindingMode.OneWay;
            mbindingToIconBeginThyroid.Bindings.Add(bindingFromThyroidScannerWorkStatus);
            mbindingToIconBeginThyroid.Bindings.Add(bindingFromThyroidScannerForceSensorCleared);
            mbindingToIconBeginThyroid.Bindings.Add(bindingFromThyroidScannerParameterConfirm);
            mbindingToIconBeginThyroid.Converter = convertMS2EB;
            mbindingToIconBeginThyroid.ConverterParameter = new object[] { new byte[] { 0 }, new OperateModuleBase.WorkStatus[] { OperateModuleBase.WorkStatus.InitialForceDevice, OperateModuleBase.WorkStatus.ParametersConfiguration } };
            BindingOperations.SetBinding(ts.iconBeginThyroid, IconButton.IsEnabledProperty, mbindingToIconBeginThyroid);

            // 19. startThyroidNextBtn
            MultiBinding mbindingToStartThyroidNextBtn = new MultiBinding();
            mbindingToStartThyroidNextBtn.Mode = BindingMode.OneWay;
            mbindingToStartThyroidNextBtn.Bindings.Add(bindingFromThyroidScannerWorkStatus);
            mbindingToStartThyroidNextBtn.Bindings.Add(bindingFromThyroidScannerParameterConfirmState);
            mbindingToStartThyroidNextBtn.Converter = convertMS2EB;
            mbindingToStartThyroidNextBtn.ConverterParameter = new object[] { new byte[] { 1 }, new OperateModuleBase.WorkStatus[] { OperateModuleBase.WorkStatus.ParametersConfiguration } };
            BindingOperations.SetBinding(ts.startThyroidNextBtn, IconButton.IsEnabledProperty, mbindingToStartThyroidNextBtn);

            // 20. factorPosNextBtn
            MultiBinding mbindingToFactorPosNextBtn = new MultiBinding();
            mbindingToFactorPosNextBtn.Mode = BindingMode.OneWay;
            mbindingToFactorPosNextBtn.Bindings.Add(bindingFromThyroidScannerWorkStatus);
            mbindingToFactorPosNextBtn.Bindings.Add(bindingFromThyroidScannerParameterConfirmState);
            mbindingToFactorPosNextBtn.Converter = convertMS2EB;
            mbindingToFactorPosNextBtn.ConverterParameter = new object[] { new byte[] { 2 }, new OperateModuleBase.WorkStatus[] { OperateModuleBase.WorkStatus.ParametersConfiguration } };
            BindingOperations.SetBinding(ts.factorPosNextBtn, IconButton.IsEnabledProperty, mbindingToFactorPosNextBtn);

            // 21. factorAttNextBtn
            MultiBinding mbindingToFactorAttNextBtn = new MultiBinding();
            mbindingToFactorAttNextBtn.Mode = BindingMode.OneWay;
            mbindingToFactorAttNextBtn.Bindings.Add(bindingFromThyroidScannerWorkStatus);
            mbindingToFactorAttNextBtn.Bindings.Add(bindingFromThyroidScannerParameterConfirmState);
            mbindingToFactorAttNextBtn.Converter = convertMS2EB;
            mbindingToFactorAttNextBtn.ConverterParameter = new object[] { new byte[] { 3 }, new OperateModuleBase.WorkStatus[] { OperateModuleBase.WorkStatus.ParametersConfiguration } };
            BindingOperations.SetBinding(ts.factorAttNextBtn, IconButton.IsEnabledProperty, mbindingToFactorAttNextBtn);

            // 22. factorFosNextBtn
            MultiBinding mbindingToFactorFosNextBtn = new MultiBinding();
            mbindingToFactorFosNextBtn.Mode = BindingMode.OneWay;
            mbindingToFactorFosNextBtn.Bindings.Add(bindingFromThyroidScannerWorkStatus);
            mbindingToFactorFosNextBtn.Bindings.Add(bindingFromThyroidScannerParameterConfirmState);
            mbindingToFactorFosNextBtn.Converter = convertMS2EB;
            mbindingToFactorFosNextBtn.ConverterParameter = new object[] { new byte[] { 4 }, new OperateModuleBase.WorkStatus[] { OperateModuleBase.WorkStatus.ParametersConfiguration } };
            BindingOperations.SetBinding(ts.factorFosNextBtn, IconButton.IsEnabledProperty, mbindingToFactorFosNextBtn);

            // 23. enablePosNextBtn
            MultiBinding mbindingToEnablePosNextBtn = new MultiBinding();
            mbindingToEnablePosNextBtn.Mode = BindingMode.OneWay;
            mbindingToEnablePosNextBtn.Bindings.Add(bindingFromThyroidScannerWorkStatus);
            mbindingToEnablePosNextBtn.Bindings.Add(bindingFromThyroidScannerParameterConfirmState);
            mbindingToEnablePosNextBtn.Converter = convertMS2EB;
            mbindingToEnablePosNextBtn.ConverterParameter = new object[] { new byte[] { 5 }, new OperateModuleBase.WorkStatus[] { OperateModuleBase.WorkStatus.ParametersConfiguration } };
            BindingOperations.SetBinding(ts.enablePosNextBtn, IconButton.IsEnabledProperty, mbindingToEnablePosNextBtn);

            // 24. enableAttNextBtn
            MultiBinding mbindingToEnableAttNextBtn = new MultiBinding();
            mbindingToEnableAttNextBtn.Mode = BindingMode.OneWay;
            mbindingToEnableAttNextBtn.Bindings.Add(bindingFromThyroidScannerWorkStatus);
            mbindingToEnableAttNextBtn.Bindings.Add(bindingFromThyroidScannerParameterConfirmState);
            mbindingToEnableAttNextBtn.Converter = convertMS2EB;
            mbindingToEnableAttNextBtn.ConverterParameter = new object[] { new byte[] { 6 }, new OperateModuleBase.WorkStatus[] { OperateModuleBase.WorkStatus.ParametersConfiguration } };
            BindingOperations.SetBinding(ts.enableAttNextBtn, IconButton.IsEnabledProperty, mbindingToEnableAttNextBtn);

            // 25. enableFosKeepNextBtn
            MultiBinding mbindingToEnableFosKeepNextBtnn = new MultiBinding();
            mbindingToEnableFosKeepNextBtnn.Mode = BindingMode.OneWay;
            mbindingToEnableFosKeepNextBtnn.Bindings.Add(bindingFromThyroidScannerWorkStatus);
            mbindingToEnableFosKeepNextBtnn.Bindings.Add(bindingFromThyroidScannerParameterConfirmState);
            mbindingToEnableFosKeepNextBtnn.Converter = convertMS2EB;
            mbindingToEnableFosKeepNextBtnn.ConverterParameter = new object[] { new byte[] { 7 }, new OperateModuleBase.WorkStatus[] { OperateModuleBase.WorkStatus.ParametersConfiguration } };
            BindingOperations.SetBinding(ts.enableFosKeepNextBtn, IconButton.IsEnabledProperty, mbindingToEnableFosKeepNextBtnn);

            // 26. enableFosTrackNextBtn
            MultiBinding mbindingToEnableFosTrackNextBtn = new MultiBinding();
            mbindingToEnableFosTrackNextBtn.Mode = BindingMode.OneWay;
            mbindingToEnableFosTrackNextBtn.Bindings.Add(bindingFromThyroidScannerWorkStatus);
            mbindingToEnableFosTrackNextBtn.Bindings.Add(bindingFromThyroidScannerParameterConfirmState);
            mbindingToEnableFosTrackNextBtn.Converter = convertMS2EB;
            mbindingToEnableFosTrackNextBtn.ConverterParameter = new object[] { new byte[] { 8 }, new OperateModuleBase.WorkStatus[] { OperateModuleBase.WorkStatus.ParametersConfiguration } };
            BindingOperations.SetBinding(ts.enableFosTrackNextBtn, IconButton.IsEnabledProperty, mbindingToEnableFosTrackNextBtn);
        }

        /// <summary>
        /// 绑定域 --| ThyroidScanner Parameter Change Move |-- 内元素
        /// </summary>
        private void BindingItemsThyroidScannerMoveEnable()
        {
            // 绑定：BreastScanConfMovingEnable {属性} ==> iconMoveSettingGalactophore {GalactophorDetect控件}
            Binding bindingFromBreastScanConfMovingEnableToIconMoveSettingGalactophore = new Binding();
            bindingFromBreastScanConfMovingEnableToIconMoveSettingGalactophore.Source = this;
            bindingFromBreastScanConfMovingEnableToIconMoveSettingGalactophore.Path = new PropertyPath("BreastScanConfMovingEnable");
            bindingFromBreastScanConfMovingEnableToIconMoveSettingGalactophore.Mode = BindingMode.OneWay;
            bindingFromBreastScanConfMovingEnableToIconMoveSettingGalactophore.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            BindingOperations.SetBinding(gd.iconMoveSettingGalactophore, Grid.IsEnabledProperty, bindingFromBreastScanConfMovingEnableToIconMoveSettingGalactophore);
        }
        #endregion

        /// <summary>
        /// View赋值
        /// </summary>
        /// <param name="MW">主窗口</param>
        /// <param name="MP">主导航页</param>
        /// <param name="BC">基本控制页</param>
        /// <param name="GD">乳腺扫描页</param>
        /// <param name="TS">甲状腺扫描页</param>
        public void DefineViews(
            MainWindow MW,
            MainPage MP,
            BaseControl BC,
            GalactophoreDetect GD,
            ThyroidScan TS)
        {
            mw = MW;
            mp = MP;
            bc = BC;
            gd = GD;
            ts = TS;
        }

        /// <summary>
        /// Model初始化
        /// </summary>
        /// <returns>返回初始化结果</returns>
        public byte ModelInitialization()
        {
            // 检查环境
            if (!Functions.CheckEnvironment())
            {
                RemoteEnable = false;
                return 1;
            }
            Logger.HistoryPrinting(Logger.Level.INFO, MethodBase.GetCurrentMethod().DeclaringType.FullName, "Starts with successful checked.");

            if (!CommunicationModelInitialization()) return 9;

            GetSensorDatasInitialization();
            return 0;
        }

        protected bool dataBaseInitialDone = false;
        /// <summary>
        /// 数据传输模型初始化
        /// </summary>
        /// <returns>返回初始化结果</returns>
        private bool CommunicationModelInitialization()
        {
            bool result;
            cm = new CommunicationModel(out result);
            if (!result) return false;

            cm.OnSendURRealTimeData += URRefreshParams;
            cm.OnSendURNetAbnormalAbort += URNetAbort;
            cm.OnSendURWorkEmergencyState += UREmergencyStatus;
            cm.OnSendURNearSingularState += URSingularState;
            cm.OnSendURInitialPowerOnAsk += URInitialPowerOnQuestion;
            cm.OnSendURInitialPowerOnAskReply += URInitialPowerOnQuestionReply;
            cm.OnSendURAdditionalDeviceAbnormal += URAdditionalDeviceAbnormalStatus;

            cm.OnSendBreastScanNipplePos += new CommunicationModel.SendVoid(new Action(() => { NipplePositionGDR = new double[] { ToolTCPCordinateX, ToolTCPCordinateY, ToolTCPCordinateZ }; }));
            cm.OnSendBreastScanConfiguration += GDRConfParams;
            cm.OnSendBreastScanWorkStatus += GDRWorkStatus;
            cm.OnSendBreastScanConfigurationConfirmStatus += GDRParameterConfirmStatus;
            cm.OnSendBreastScanForceZerodStatus += GDRForceClearedStatus;
            cm.OnSendBreastScanConfigurationProcess += GDRConfigurationProcess;
            cm.OnSendBreastScanImmediateStop += GDRStopNow;
            cm.OnSendBreastScanImmediateStopRecovery += GDRStopRecovery;

            cm.OnSendTcpDisconnected += RemoteConnectionBroken;

            cm.OnSendChangePage += NavigateToPageLocal;
            return true;
        }

        /// <summary>
        /// 初始化信号采集模块
        /// </summary>
        private void GetSensorDatasInitialization()
        {
            gsd = new GetSensorDatas();

            gsd.OnSendSensorConnectionBroken += new GetSensorDatas.SendBool(GSDBrokenState);
            gsd.OnSendSensorDatas += new GetSensorDatas.SendDoubleArray(GSDSensorDatas);
        }
        #endregion

        #region BoundEvent
        /// <summary>
        /// UR相关数据反馈
        /// </summary>
        /// <param name="Parameters">反馈数据</param>
        private void URRefreshParams(double[] Parameters)
        {
            ToolTCPCordinateX = Parameters[0];
            ToolTCPCordinateY = Parameters[1];
            ToolTCPCordinateZ = Parameters[2];
            ToolTCPCordinateRX = Parameters[3];
            ToolTCPCordinateRY = Parameters[4];
            ToolTCPCordinateRZ = Parameters[5];

            RobotJointBaseAngle = Parameters[6];
            RobotJointShoulderAngle = Parameters[7];
            RobotJointElbowAngle = Parameters[8];
            RobotJointWrist1Angle = Parameters[9];
            RobotJointWrist2Angle = Parameters[10];
            RobotJointWrist3Angle = Parameters[11];

            RobotJointBaseCurrent = Parameters[12];
            RobotJointShoulderCurrent = Parameters[13];
            RobotJointElbowCurrent = Parameters[14];
            RobotJointWrist1Current = Parameters[15];
            RobotJointWrist2Current = Parameters[16];
            RobotJointWrist3Current = Parameters[17];

            ToolForceX = Parameters[18];
            ToolForceY = Parameters[19];
            ToolForceZ = Parameters[20];
            ToolTorqueX = Parameters[21];
            ToolTorqueY = Parameters[22];
            ToolTorqueZ = Parameters[23];

            RobotCurrentStatus = (URDataProcessor.RobotStatus)Parameters[24];
            RobotProgramCurrentStatus = (URDataProcessor.RobotProgramStatus)Parameters[25];

            if (!RemoteEnable)
            {
                StatusBarRemoteContent = "远程网络连接正常";
                StatusBarRemoteBackgroundColor = defaultGreenColor;
                RemoteEnable = true;
                ConnectBtnEnable = true;
            }

            if (!LocalEnable)
            {
                StatusBarContent = "当地网络连接正常";
                StatusBarBackgroundColor = defaultGreenColor;
                LocalEnable = true;
            }
        }

        /// <summary>
        /// UR网络连接断开
        /// </summary>
        private void URNetAbort()
        {
            StatusBarContent = "当地网络连接异常中断";
            StatusBarBackgroundColor = defaultRedColor;
            LocalEnable = false;
            ShowDialogAtUIThread("当地网络连接由于未知原因发生中断，机械臂急停！", "错误", 5);
            Logger.HistoryPrinting(Logger.Level.ERROR, MethodBase.GetCurrentMethod().DeclaringType.FullName, "Local net connection is unexpectly broken.");
        }

        /// <summary>
        /// UR反馈状态异常
        /// </summary>
        /// <param name="AbnormalStatus">异常状态</param>
        private void UREmergencyStatus(int AbnormalStatus)
        {
            string showStr;
            if (AbnormalStatus == (int)URDataProcessor.RobotEmergency.ProtectiveStop)
            {
                showStr = "机械臂触发保护停止！";
            }
            else if (AbnormalStatus == (int)URDataProcessor.RobotEmergency.EmergencyStop)
            {
                showStr = "机械臂触发紧急停止！";
            }
            else if (AbnormalStatus == (int)URDataProcessor.RobotEmergency.CurrentOverflow)
            {
                showStr = "机械臂关节电流偏离超限，已停止断电！";
            }
            else if (AbnormalStatus == (int)URDataProcessor.RobotEmergency.ForceOverflow)
            {
                showStr = "机械臂末端力和力矩过大，已停止断电！";
            }
            else showStr = "机械臂发生未知的紧急状况！";

            ShowDialogAtUIThread(showStr, "紧急状态", 4);

            string outputStatus = ((URDataProcessor.RobotEmergency)AbnormalStatus).ToString();
            Logger.HistoryPrinting(Logger.Level.WARN, MethodBase.GetCurrentMethod().DeclaringType.FullName, "Robot Emergency status: " + outputStatus + ".");
            return;
        }

        /// <summary>
        /// UR奇异点临近状态反馈
        /// </summary>
        /// <param name="SingularState">奇异点临近状态</param>
        private void URSingularState(int SingularState)
        {
            string showStr;
            switch (SingularState)
            {
                case 1:
                    showStr = "临近肩部奇异位置，已停止相关运动，请当地手动控制机械臂远离奇异点！";
                    Logger.HistoryPrinting(Logger.Level.WARN, MethodBase.GetCurrentMethod().DeclaringType.FullName, "Near singular point at shoulder.");
                    break;
                case 2:
                    showStr = "临近肘部奇异位置，已停止相关运动，请当地手动控制机械臂远离奇异点！";
                    Logger.HistoryPrinting(Logger.Level.WARN, MethodBase.GetCurrentMethod().DeclaringType.FullName, "Near singular point at elbow.");
                    break;
                case 3:
                    showStr = "临近腕部奇异位置，已停止相关运动，请当地手动控制机械臂远离奇异点！";
                    Logger.HistoryPrinting(Logger.Level.WARN, MethodBase.GetCurrentMethod().DeclaringType.FullName, "Near singular point at wrist.");
                    break;
                default:
                    showStr = "临近未知奇异位置，已停止相关运动，请断电检查程序奇异点定义！";
                    Logger.HistoryPrinting(Logger.Level.WARN, MethodBase.GetCurrentMethod().DeclaringType.FullName, "Near unknown singular point.");
                    break;
            }
            ShowDialogAtUIThread(showStr, "紧急状态", 6);
            return;
        }

        /// <summary>
        /// UR初始化询问
        /// </summary>
        private void URInitialPowerOnQuestion()
        {
            ShowBranchDialogAtUIThread("检测到机械臂未处于运行状态，是否为机械臂上电并松开制动器？", "提问", new DealBranchDialogDelegate(DealWithFirstNetConnection));
        }

        /// <summary>
        /// UR初始化结束
        /// </summary>
        private void URInitialPowerOnQuestionReply()
        {
            ifInitialArm = false;
        }

        /// <summary>
        /// 处理首次连接网络的问题
        /// </summary>
        /// <param name="result">操作指示标志</param>
        private async void DealWithFirstNetConnection(bool result)
        {
            if (result)
            {
                cm.SendCmd(CommunicationModel.TCPProtocolKey.NormalData, new byte[] { 1 }, CommunicationModel.AppProtocolCommand.AutoPowerOn);
                ifInitialArm = true;

                var controller = await mw.ShowProgressAsync("请稍后", "正在为机械臂初始化。。。", settings: new MetroDialogSettings()
                {
                    AnimateShow = false,
                    AnimateHide = false,
                    DialogTitleFontSize = titleSize,
                    DialogMessageFontSize = messageSize,
                    ColorScheme = MetroDialogColorScheme.Theme
                });

                controller.SetIndeterminate();
                while (ifInitialArm) await Task.Delay(500);

                await controller.CloseAsync();

                await ShowDialog("机械臂已处于运行状态！", "完成", 8);
            }
            else
            {
                cm.SendCmd(CommunicationModel.TCPProtocolKey.NormalData, new byte[] { 0 }, CommunicationModel.AppProtocolCommand.AutoPowerOn);
            }
        }

        /// <summary>
        /// UR额外设备问题
        /// </summary>
        /// <param name="status">状态字</param>
        private void URAdditionalDeviceAbnormalStatus(int status)
        {
            CommunicationModel.AppProtocolAdditionalDeviceAbnormalDatagramClass key = (CommunicationModel.AppProtocolAdditionalDeviceAbnormalDatagramClass)status;

            switch (key)
            {
                case CommunicationModel.AppProtocolAdditionalDeviceAbnormalDatagramClass.DataBaseAttachFailed:
                    DataBaseCanNotBeAttached();
                    break;
                case CommunicationModel.AppProtocolAdditionalDeviceAbnormalDatagramClass.SerialPortAttachFailed:
                    SerialPortCanNotBeAttached();
                    break;
                case CommunicationModel.AppProtocolAdditionalDeviceAbnormalDatagramClass.URNetConnectionRecovery:
                    StatusBarContent = "当地网络连接正常";
                    StatusBarBackgroundColor = defaultGreenColor;
                    if (!LocalEnable) LocalEnable = true;
                    Logger.HistoryPrinting(Logger.Level.INFO, MethodBase.GetCurrentMethod().DeclaringType.FullName, "Local net connection is established automatically.");
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 数据库连接出错
        /// </summary>
        private void DataBaseCanNotBeAttached()
        {
            LocalEnable = false;
            ShowDialogAtUIThread("无法连接到数据库！", "错误", 2);
            Logger.HistoryPrinting(Logger.Level.ERROR, MethodBase.GetCurrentMethod().DeclaringType.FullName, "DataBase can not be attached.");
            return;
        }

        /// <summary>
        /// COM口连接出错
        /// </summary>
        private void SerialPortCanNotBeAttached()
        {
            LocalEnable = false;
            ShowDialogAtUIThread("无法连接到串口！", "错误", 3);
            Logger.HistoryPrinting(Logger.Level.ERROR, MethodBase.GetCurrentMethod().DeclaringType.FullName, "Serial Port can not be attached.");
            return;
        }

        /// <summary>
        /// GDR相关配置参数反馈
        /// </summary>
        /// <param name="Parameters">反馈的配置参数</param>
        private void GDRConfParams(List<string[]> Parameters)
        {
            DetectingErrorForceMinGDR = double.Parse(Parameters[0][0]);
            DetectingErrorForceMaxGDR = double.Parse(Parameters[1][0]);
            DetectingSpeedMinGDR = double.Parse(Parameters[2][0]);
            IfEnableAngleCorrectedGDR = bool.Parse(Parameters[3][0]);
            // {vibratingAttitudeMaxAtSmoothPart}
            // {vibratingAttitudeMinAtSteepPart}
            // {vibratingAttitudeMaxAtSteepPart}
            NippleForbiddenRadiusGDR = double.Parse(Parameters[7][0]);
            // {movingStopDistance}
            DetectingStopDistanceGDR = double.Parse(Parameters[9][0]);
            DetectingSafetyLiftDistanceGDR = double.Parse(Parameters[10][0]);
            IfEnableDetectingInitialForceGDR = bool.Parse(Parameters[11][0]);
            DetectingSinkDistanceGDR = double.Parse(Parameters[12][0]);
            VibratingAngleDegreeGDR = (GalactophoreDetector.VibratingMagnitude)Enum.Parse(typeof(GalactophoreDetector.VibratingMagnitude), Parameters[13][0]);
            MovingSpeedDegreeGDR = (GalactophoreDetector.MovingLevel)Enum.Parse(typeof(GalactophoreDetector.MovingLevel), Parameters[14][0]);
            DetectingForceDegreeGDR = (GalactophoreDetector.DetectingIntensity)Enum.Parse(typeof(GalactophoreDetector.DetectingIntensity), Parameters[15][0]);
            DetectingAlignDegreeGDR = (GalactophoreDetector.AligningDegree)Enum.Parse(typeof(GalactophoreDetector.AligningDegree), Parameters[16][0]);
            MovingUpEdgeDistanceGDR = double.Parse(Parameters[17][0]);
            MovingLeftEdgeDistanceGDR = double.Parse(Parameters[18][0]);
            MovingDownEdgeDistanceGDR = double.Parse(Parameters[19][0]);
            MovingRightEdgeDistanceGDR = double.Parse(Parameters[20][0]);
            IfAutoReplaceConfigurationGDR = bool.Parse(Parameters[21][0]);
            IfCheckRightGalactophoreGDR = (GalactophoreDetector.ScanningRegion)Enum.Parse(typeof(GalactophoreDetector.ScanningRegion), Parameters[22][0]);
            IdentifyEdgeModeGDR = (GalactophoreDetector.IdentifyBoundary)Enum.Parse(typeof(GalactophoreDetector.IdentifyBoundary), Parameters[23][0]);
            CheckingStepGDR = double.Parse(Parameters[24][0]);
        }

        /// <summary>
        /// GDR工作状态反馈
        /// </summary>
        /// <param name="Status">反馈的工作状态</param>
        private void GDRWorkStatus(int Status)
        {
            GalactophoreDetectorWorkStatus = Convert.ToInt16(Status);
        }

        /// <summary>
        /// GDR参数确认状态反馈
        /// </summary>
        /// <param name="Status">反馈的参数确认状态</param>
        private void GDRParameterConfirmStatus(bool Status)
        {
            GalactophoreDetectorParameterConfirm = Status;
        }

        /// <summary>
        /// GDR力清零状态反馈
        /// </summary>
        /// <param name="Status">反馈的力清零状态</param>
        private void GDRForceClearedStatus(bool Status)
        {
            GalactophoreDetectorForceSensorCleared = Status;
        }

        /// <summary>
        /// GDR配置过程状态
        /// </summary>
        /// <param name="process">过程状态</param>
        private void GDRConfigurationProcess(int process)
        {
            GalactophoreDetectorParameterConfirmState = Convert.ToByte(process);
        }

        /// <summary>
        /// GDR立即停止
        /// </summary>
        private void GDRStopNow()
        {
            ifWaitForGDR = true;
            mw.Dispatcher.BeginInvoke(new Action(GDRStopLogic));
        }

        /// <summary>
        /// GDR急停逻辑
        /// </summary>
        private async void GDRStopLogic()
        {
            var controller = await mw.ShowProgressAsync("请稍后", "当地急停乳腺扫查，请等待当地恢复控制权。。。", settings: new MetroDialogSettings()
            {
                AnimateShow = false,
                AnimateHide = false,
                DialogTitleFontSize = titleSize,
                DialogMessageFontSize = messageSize,
                ColorScheme = MetroDialogColorScheme.Theme
            });

            controller.SetIndeterminate();

            while (ifWaitForGDR) await Task.Delay(500);

            await controller.CloseAsync();

            await ShowDialog("当地已经恢复乳腺扫查控制权！", "完成", 18);
        }

        /// <summary>
        /// GDR立即停止恢复
        /// </summary>
        private void GDRStopRecovery()
        {
            ifWaitForGDR = false;
        }

        /// <summary>
        /// 远程连接断开
        /// </summary>
        private void RemoteConnectionBroken(bool error)
        {
            StatusBarRemoteContent = "远程网络连接异常中断";
            StatusBarRemoteBackgroundColor = defaultRedColor;
            RemoteEnable = false;
            ConnectBtnEnable = true;
            ConnectBtnText = "建立连接";

            if (error) mw.Dispatcher.BeginInvoke(new Action(ErrorAtRemoteConnection));
        }

        /// <summary>
        /// 出现未知网络错误
        /// </summary>
        private async void ErrorAtRemoteConnection()
        {
            var mySettings = new MetroDialogSettings()
            {
                AffirmativeButtonText = "确认",
                DialogTitleFontSize = titleSize,
                DialogMessageFontSize = messageSize,
                ColorScheme = MetroDialogColorScheme.Theme
            };

            await mw.ShowMessageAsync("问题", "出现未知远程网络连接错误！", MessageDialogStyle.Affirmative, mySettings);

            ClearAllEvents(cm);
            await Task.Delay(200);
            mw.Close();
        }

        /// TSR相关配置参数反馈
        /// </summary>
        /// <param name="Parameters">反馈的配置参数</param>
        private void TSRConfParams(List<string[]> Parameters)
        {
            DetectingErrorForceMinTSR = double.Parse(Parameters[0][0]);
            DetectingErrorForceMaxTSR = double.Parse(Parameters[1][0]);
            DetectingSpeedMinTSR = double.Parse(Parameters[2][0]);
            DetectingSpeedMaxTSR = double.Parse(Parameters[3][0]);
            IfEnableForceKeepingTSR = bool.Parse(Parameters[4][0]);
            IfEnableForceTrackingTSR = bool.Parse(Parameters[5][0]);
            DetectingBasicPreservedForceTSR = double.Parse(Parameters[6][0]);

            MaxAvailableRadiusTSR = double.Parse(Parameters[7][0]);
            MaxAvailableAngleTSR = double.Parse(Parameters[8][0]);
            StopRadiusTSR = double.Parse(Parameters[9][0]);
            MaxDistPeriodTSR = double.Parse(Parameters[10][0]);
            MaxAnglePeriodTSR = double.Parse(Parameters[11][0]);

            PositionOverrideTSR = double.Parse(Parameters[12][0]);
            AngleOverrideTSR = double.Parse(Parameters[13][0]);
            ForceOverrideTSR = double.Parse(Parameters[14][0]);

            IfEnableAttitudeTrackingTSR = bool.Parse(Parameters[15][0]);
            IfEnableTranslationTrackingTSR = bool.Parse(Parameters[16][0]);
        }

        /// <summary>
        /// TSR工作状态反馈
        /// </summary>
        /// <param name="Status">反馈的工作状态</param>
        private void TSRWorkStatus(short Status)
        {
            ThyroidScannerWorkStatus = Status;
        }

        /// <summary>
        /// TSR参数确认状态反馈
        /// </summary>
        /// <param name="Status">反馈的参数确认状态</param>
        private void TSRParameterConfirmStatus(bool Status)
        {
            ThyroidScannerParameterConfirm = Status;
        }

        /// <summary>
        /// TSR力清零状态反馈
        /// </summary>
        /// <param name="Status">反馈的力清零状态</param>
        private void TSRForceClearedStatus(bool Status)
        {
            ThyroidScannerForceSensorCleared = Status;
        }

        /// <summary>
        /// GSD连接断开反馈
        /// </summary>
        /// <param name="BrokenState">断开状态</param>
        private void GSDBrokenState(bool BrokenState)
        {
            ifActionSensorDataCatched = false;
            if (!BrokenState)
            {
                if (ifUsingSerialPort) sc.SendOpenRelay(); // 急停按钮按下
                EnableAll = false;
                if (currentPage == ShowPage.ThyroidScanning)
                    mw.Dispatcher.BeginInvoke(new Action(StopMotionNowThyroidScanningModule));
                else
                    ShowDialogAtUIThread("动作捕捉传感器连接由于未知原因发生中断，机械臂急停！", "错误", 35);

                Logger.HistoryPrinting(Logger.Level.ERROR, MethodBase.GetCurrentMethod().DeclaringType.FullName, "Action sensor connection is unexpectly broken.");
            }
            else
                Logger.HistoryPrinting(Logger.Level.INFO, MethodBase.GetCurrentMethod().DeclaringType.FullName, "Action sensor connection is actively broken.");
        }

        /// <summary>
        /// 获得动作传感器数据
        /// </summary>
        /// <param name="datas">传感器数据</param>
        private void GSDSensorDatas(double[] datas)
        {
            if (!ifActionCatchStart) return;

            double[] recvPos = new double[] { datas[0], datas[1] };
            double[] recvAtt = new double[] { datas[2], datas[3], datas[4], datas[5] };
            double[] recvFos = new double[] { 
                3992.0 * Math.Exp(-0.5633 * datas[6]), 
                3992.0 * Math.Exp(-0.5633 * datas[7]),
                3992.0 * Math.Exp(-0.5633 * datas[8]),
                3992.0 * Math.Exp(-0.5633 * datas[9]) };

            if (actionCatchedNum == 0)
            {
                firstActionCatched[0] = recvPos[0];
                firstActionCatched[1] = recvPos[1];
                firstActionCatched[2] = recvAtt[0];
                firstActionCatched[3] = recvAtt[1];
                firstActionCatched[4] = recvAtt[2];
                firstActionCatched[5] = recvAtt[3];
                firstActionCatched[6] = recvFos[0];
                firstActionCatched[7] = recvFos[1];
                firstActionCatched[8] = recvFos[2];
                firstActionCatched[9] = recvFos[3];
                actionCatchedNum = 1;
                return;
            }

            double[] sendCommand = new double[6];

            // 位置指令
            sendCommand[0] = recvPos[0] - firstActionCatched[0];
            sendCommand[1] = recvPos[1] - firstActionCatched[1];

            // 姿态指令
            Quatnum q0 = new Quatnum(new double[] { firstActionCatched[2], firstActionCatched[3], firstActionCatched[4], firstActionCatched[5] });
            Quatnum qrt = new Quatnum(recvAtt);
            double[] q0ToQrt = URMath.Quatnum2AxisAngle(URMath.FindTransitQuatnum(q0, qrt));
            double q0ToQrtAngle = URMath.LengthOfArray(q0ToQrt);
            double[] q0ToQrtAxis = new double[] { q0ToQrt[0] / q0ToQrtAngle, q0ToQrt[1] / q0ToQrtAngle, q0ToQrt[2] / q0ToQrtAngle };
            double[] q0ToQrtAxisAtQ0 = URMath.FindDirectionToSecondReferenceFromFirstReference(q0ToQrtAxis, q0);
            double[] q0ToQrtAtQ0 = new double[] { q0ToQrtAngle * q0ToQrtAxisAtQ0[0], q0ToQrtAngle * q0ToQrtAxisAtQ0[1], q0ToQrtAngle * q0ToQrtAxisAtQ0[2] };
            sendCommand[2] = q0ToQrtAtQ0[0];
            sendCommand[3] = q0ToQrtAtQ0[1];
            sendCommand[4] = q0ToQrtAtQ0[2];

            // 压力指令
            double pressure = Math.Max(recvFos[3],
                                                Math.Max(recvFos[2],
                                                    Math.Max(recvFos[1], recvFos[0])));
            if (pressure < 3.0) pressure = 3.0;
            if (pressure > 6.0) pressure = 6.0;
            sendCommand[5] = -pressure;

            tsr.RefreshAimPostion(actionCatchedNum, sendCommand);
            actionCatchedNum++;
            if (actionCatchedNum > int.MaxValue - 100) actionCatchedNum = 1;
        }
        #endregion
         
    }
}
