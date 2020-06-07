using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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

using System.Reflection;

using LogPrinter;
using MahApps.Metro.Controls;

namespace AssistantRobot
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow
    {
        private readonly URViewModel urvm;
        private readonly URViewModelRemote_LocalPart urvmr_lp;

        private MainPage mainPage;
        private BaseControl baseContorlPage;
        private GalactophoreDetect galactophoreDetectPage;
        private ThyroidScan thyroidScanPage;

        private readonly ConverterThatTransformDoubleToString convertD2S = new ConverterThatTransformDoubleToString();
        private readonly ValueProcesser valuePdot25D2 = new ValueProcesser(0.25, "0.00");
        private readonly ValueProcesser valuePdot5D1A1dot5 = new ValueProcesser(0.5, "0.0", 1.5);
        private readonly ValueProcesser valuePdot1D1Adot2 = new ValueProcesser(0.1, "0.0", 0.2);
        private readonly ValueProcesser valuePdot1D1 = new ValueProcesser(0.1, "0.0");
        private readonly ValueProcesser valueP1D0A15 = new ValueProcesser(1.0, "0", 15);
        private readonly ValueProcesser valueP1D1A3 = new ValueProcesser(1.0, "0.0", 3);
        private readonly ValueProcesser valueP50D0A300 = new ValueProcesser(50.0, "0", 300);
        private readonly ValueProcesser valueP50D0A400 = new ValueProcesser(50.0, "0", 400);
        private readonly ValueProcesser valueP15D0A45 = new ValueProcesser(15.0, "0", 45);
        private readonly ValueProcesser valuePdot02D2Adot03 = new ValueProcesser(0.02, "0.00", 0.03);
        private readonly ConverterThatTransformDoubleToWord convertD2W = new ConverterThatTransformDoubleToWord();

        private byte modelInitialResult = 0;
        private bool appInitialResult = true;

        public MainWindow()
        {
            InitializeComponent();

            // 定义VM
            urvm = new URViewModel(out appInitialResult);

            // 定义RemoteVM_LocalPart
            urvmr_lp = new URViewModelRemote_LocalPart(urvm);
            urvm.DefineViewModel(urvmr_lp);

            // 初始化页
            mainPage = new MainPage(urvm);
            baseContorlPage = new BaseControl(urvm);
            galactophoreDetectPage = new GalactophoreDetect(urvm);
            thyroidScanPage = new ThyroidScan(urvm);
            urvm.DefineViews(
                this,
                mainPage,
                baseContorlPage,
                galactophoreDetectPage,
                thyroidScanPage);
            
            // 绑定必要元素
            urvm.BindingItems();

            // Model初始化
            modelInitialResult = urvm.ModelInitialization();

            // 建立部分绑定
            PartialBindingsEstablish();

            // 加载默认页
            urvm.NavigateToPage((URViewModel.ShowPage)(-1));
        }

        private void PartialBindingsEstablish()
        {
            #region GalactophoreDetect
            // 绑定：minForceSlider.Value {属性} ==> minForceText.Content {Flyout控件}
            Binding bindingFromMinForceSliderToMinForceText = new Binding();
            bindingFromMinForceSliderToMinForceText.ElementName = "minForceSlider";
            bindingFromMinForceSliderToMinForceText.Path = new PropertyPath("Value");
            bindingFromMinForceSliderToMinForceText.Mode = BindingMode.OneWay;
            bindingFromMinForceSliderToMinForceText.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            bindingFromMinForceSliderToMinForceText.Converter = convertD2S;
            bindingFromMinForceSliderToMinForceText.ConverterParameter = valuePdot25D2;
            BindingOperations.SetBinding(minForceText, Label.ContentProperty, bindingFromMinForceSliderToMinForceText);

            // 绑定：maxForceSlider.Value {属性} ==> maxForceText.Content {Flyout控件}
            Binding bindingFromMaxForceSliderToMaxForceText = new Binding();
            bindingFromMaxForceSliderToMaxForceText.ElementName = "maxForceSlider";
            bindingFromMaxForceSliderToMaxForceText.Path = new PropertyPath("Value");
            bindingFromMaxForceSliderToMaxForceText.Mode = BindingMode.OneWay;
            bindingFromMaxForceSliderToMaxForceText.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            bindingFromMaxForceSliderToMaxForceText.Converter = convertD2S;
            bindingFromMaxForceSliderToMaxForceText.ConverterParameter = valuePdot5D1A1dot5;
            BindingOperations.SetBinding(maxForceText, Label.ContentProperty, bindingFromMaxForceSliderToMaxForceText);

            // 绑定：minDetectSpeedSlider.Value {属性} ==> minDetectSpeedText.Content {Flyout控件}
            Binding bindingFromMinDetectSpeedSliderToMinDetectSpeedText = new Binding();
            bindingFromMinDetectSpeedSliderToMinDetectSpeedText.ElementName = "minDetectSpeedSlider";
            bindingFromMinDetectSpeedSliderToMinDetectSpeedText.Path = new PropertyPath("Value");
            bindingFromMinDetectSpeedSliderToMinDetectSpeedText.Mode = BindingMode.OneWay;
            bindingFromMinDetectSpeedSliderToMinDetectSpeedText.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            bindingFromMinDetectSpeedSliderToMinDetectSpeedText.Converter = convertD2S;
            bindingFromMinDetectSpeedSliderToMinDetectSpeedText.ConverterParameter = valuePdot1D1;
            BindingOperations.SetBinding(minDetectSpeedText, Label.ContentProperty, bindingFromMinDetectSpeedSliderToMinDetectSpeedText);

            // 绑定：vibrateDegreeSlider.Value {属性} ==> vibrateDegreeText.Content {Flyout控件}
            Binding bindingFromVibrateDegreeSliderToVibrateDegreeText = new Binding();
            bindingFromVibrateDegreeSliderToVibrateDegreeText.ElementName = "vibrateDegreeSlider";
            bindingFromVibrateDegreeSliderToVibrateDegreeText.Path = new PropertyPath("Value");
            bindingFromVibrateDegreeSliderToVibrateDegreeText.Mode = BindingMode.OneWay;
            bindingFromVibrateDegreeSliderToVibrateDegreeText.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            bindingFromVibrateDegreeSliderToVibrateDegreeText.Converter = convertD2W;
            bindingFromVibrateDegreeSliderToVibrateDegreeText.ConverterParameter = new string[] { "小", "中", "大" };
            BindingOperations.SetBinding(vibrateDegreeText, Label.ContentProperty, bindingFromVibrateDegreeSliderToVibrateDegreeText);

            // 绑定：speedDegreeSlider.Value {属性} ==> speedDegreeText.Content {Flyout控件}
            Binding bindingFromSpeedDegreeSliderToSpeedDegreeText = new Binding();
            bindingFromSpeedDegreeSliderToSpeedDegreeText.ElementName = "speedDegreeSlider";
            bindingFromSpeedDegreeSliderToSpeedDegreeText.Path = new PropertyPath("Value");
            bindingFromSpeedDegreeSliderToSpeedDegreeText.Mode = BindingMode.OneWay;
            bindingFromSpeedDegreeSliderToSpeedDegreeText.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            bindingFromSpeedDegreeSliderToSpeedDegreeText.Converter = convertD2W;
            bindingFromSpeedDegreeSliderToSpeedDegreeText.ConverterParameter = new string[] { "慢", "中", "快" };
            BindingOperations.SetBinding(speedDegreeText, Label.ContentProperty, bindingFromSpeedDegreeSliderToSpeedDegreeText);

            // 绑定：forceDegreeSlider.Value {属性} ==> forceDegreeText.Content {Flyout控件}
            Binding bindingFromForceDegreeSliderToForceDegreeText = new Binding();
            bindingFromForceDegreeSliderToForceDegreeText.ElementName = "forceDegreeSlider";
            bindingFromForceDegreeSliderToForceDegreeText.Path = new PropertyPath("Value");
            bindingFromForceDegreeSliderToForceDegreeText.Mode = BindingMode.OneWay;
            bindingFromForceDegreeSliderToForceDegreeText.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            bindingFromForceDegreeSliderToForceDegreeText.Converter = convertD2W;
            bindingFromForceDegreeSliderToForceDegreeText.ConverterParameter = new string[] { "轻", "较轻", "轻重", "重" };
            BindingOperations.SetBinding(forceDegreeText, Label.ContentProperty, bindingFromForceDegreeSliderToForceDegreeText);

            // 绑定：borderModeSlider.Value {属性} ==> borderModeText.Content {Flyout控件}
            Binding bindingFromborderModeSliderToborderModeText = new Binding();
            bindingFromborderModeSliderToborderModeText.ElementName = "borderModeSlider";
            bindingFromborderModeSliderToborderModeText.Path = new PropertyPath("Value");
            bindingFromborderModeSliderToborderModeText.Mode = BindingMode.OneWay;
            bindingFromborderModeSliderToborderModeText.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            bindingFromborderModeSliderToborderModeText.Converter = convertD2W;
            bindingFromborderModeSliderToborderModeText.ConverterParameter = new string[] { "单个", "上下", "四周" };
            BindingOperations.SetBinding(borderModeText, Label.ContentProperty, bindingFromborderModeSliderToborderModeText);

            // 绑定：rotateStepSlider.Value {属性} ==> rotateStepText.Content {Flyout控件}
            Binding bindingFromRotateStepSliderToRotateStepText = new Binding();
            bindingFromRotateStepSliderToRotateStepText.ElementName = "rotateStepSlider";
            bindingFromRotateStepSliderToRotateStepText.Path = new PropertyPath("Value");
            bindingFromRotateStepSliderToRotateStepText.Mode = BindingMode.OneWay;
            bindingFromRotateStepSliderToRotateStepText.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            bindingFromRotateStepSliderToRotateStepText.Converter = convertD2S;
            bindingFromRotateStepSliderToRotateStepText.ConverterParameter = valueP1D0A15;
            BindingOperations.SetBinding(rotateStepText, Label.ContentProperty, bindingFromRotateStepSliderToRotateStepText);
            #endregion

            #region Thyroid Scan
            // 绑定：minForceSliderThyroid.Value {属性} ==> minForceSliderThyroidText.Content {Flyout控件}
            Binding bindingFromMinForceSliderThyroidToMinForceSliderThyroidText = new Binding();
            bindingFromMinForceSliderThyroidToMinForceSliderThyroidText.ElementName = "minForceSliderThyroid";
            bindingFromMinForceSliderThyroidToMinForceSliderThyroidText.Path = new PropertyPath("Value");
            bindingFromMinForceSliderThyroidToMinForceSliderThyroidText.Mode = BindingMode.OneWay;
            bindingFromMinForceSliderThyroidToMinForceSliderThyroidText.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            bindingFromMinForceSliderThyroidToMinForceSliderThyroidText.Converter = convertD2S;
            bindingFromMinForceSliderThyroidToMinForceSliderThyroidText.ConverterParameter = valuePdot25D2;
            BindingOperations.SetBinding(minForceSliderThyroidText, Label.ContentProperty, bindingFromMinForceSliderThyroidToMinForceSliderThyroidText);

            // 绑定：maxForceSliderThyroid.Value {属性} ==> maxForceSliderThyroidText.Content {Flyout控件}
            Binding bindingFromMaxForceSliderThyroidToMaxForceSliderThyroidText = new Binding();
            bindingFromMaxForceSliderThyroidToMaxForceSliderThyroidText.ElementName = "maxForceSliderThyroid";
            bindingFromMaxForceSliderThyroidToMaxForceSliderThyroidText.Path = new PropertyPath("Value");
            bindingFromMaxForceSliderThyroidToMaxForceSliderThyroidText.Mode = BindingMode.OneWay;
            bindingFromMaxForceSliderThyroidToMaxForceSliderThyroidText.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            bindingFromMaxForceSliderThyroidToMaxForceSliderThyroidText.Converter = convertD2S;
            bindingFromMaxForceSliderThyroidToMaxForceSliderThyroidText.ConverterParameter = valuePdot5D1A1dot5;
            BindingOperations.SetBinding(maxForceSliderThyroidText, Label.ContentProperty, bindingFromMaxForceSliderThyroidToMaxForceSliderThyroidText);

            // 绑定：minDetectSpeedSliderThyroid.Value {属性} ==> minDetectSpeedSliderThyroidText.Content {Flyout控件}
            Binding bindingFromMinDetectSpeedSliderThyroidToMinDetectSpeedSliderThyroidText = new Binding();
            bindingFromMinDetectSpeedSliderThyroidToMinDetectSpeedSliderThyroidText.ElementName = "minDetectSpeedSliderThyroid";
            bindingFromMinDetectSpeedSliderThyroidToMinDetectSpeedSliderThyroidText.Path = new PropertyPath("Value");
            bindingFromMinDetectSpeedSliderThyroidToMinDetectSpeedSliderThyroidText.Mode = BindingMode.OneWay;
            bindingFromMinDetectSpeedSliderThyroidToMinDetectSpeedSliderThyroidText.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            bindingFromMinDetectSpeedSliderThyroidToMinDetectSpeedSliderThyroidText.Converter = convertD2S;
            bindingFromMinDetectSpeedSliderThyroidToMinDetectSpeedSliderThyroidText.ConverterParameter = valuePdot1D1;
            BindingOperations.SetBinding(minDetectSpeedSliderThyroidText, Label.ContentProperty, bindingFromMinDetectSpeedSliderThyroidToMinDetectSpeedSliderThyroidText);

            // 绑定：maxDetectSpeedSliderThyroid.Value {属性} ==> maxDetectSpeedSliderThyroidText.Content {Flyout控件}
            Binding bindingFromMaxDetectSpeedSliderThyroidToMaxDetectSpeedSliderThyroidText = new Binding();
            bindingFromMaxDetectSpeedSliderThyroidToMaxDetectSpeedSliderThyroidText.ElementName = "maxDetectSpeedSliderThyroid";
            bindingFromMaxDetectSpeedSliderThyroidToMaxDetectSpeedSliderThyroidText.Path = new PropertyPath("Value");
            bindingFromMaxDetectSpeedSliderThyroidToMaxDetectSpeedSliderThyroidText.Mode = BindingMode.OneWay;
            bindingFromMaxDetectSpeedSliderThyroidToMaxDetectSpeedSliderThyroidText.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            bindingFromMaxDetectSpeedSliderThyroidToMaxDetectSpeedSliderThyroidText.Converter = convertD2S;
            bindingFromMaxDetectSpeedSliderThyroidToMaxDetectSpeedSliderThyroidText.ConverterParameter = valuePdot1D1Adot2;
            BindingOperations.SetBinding(maxDetectSpeedSliderThyroidText, Label.ContentProperty, bindingFromMaxDetectSpeedSliderThyroidToMaxDetectSpeedSliderThyroidText);

            // 绑定：holdingPressureThyroid.Value {属性} ==> holdingPressureThyroidText.Content {Flyout控件}
            Binding bindingFromHoldingPressureThyroidToHoldingPressureThyroidText = new Binding();
            bindingFromHoldingPressureThyroidToHoldingPressureThyroidText.ElementName = "holdingPressureThyroid";
            bindingFromHoldingPressureThyroidToHoldingPressureThyroidText.Path = new PropertyPath("Value");
            bindingFromHoldingPressureThyroidToHoldingPressureThyroidText.Mode = BindingMode.OneWay;
            bindingFromHoldingPressureThyroidToHoldingPressureThyroidText.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            bindingFromHoldingPressureThyroidToHoldingPressureThyroidText.Converter = convertD2S;
            bindingFromHoldingPressureThyroidToHoldingPressureThyroidText.ConverterParameter = valueP1D1A3;
            BindingOperations.SetBinding(holdingPressureThyroidText, Label.ContentProperty, bindingFromHoldingPressureThyroidToHoldingPressureThyroidText);

            // 绑定：maxRadiusThyroid.Value {属性} ==> maxRadiusThyroidText.Content {Flyout控件}
            Binding bindingFromMaxRadiusThyroidToMaxRadiusThyroidText = new Binding();
            bindingFromMaxRadiusThyroidToMaxRadiusThyroidText.ElementName = "maxRadiusThyroid";
            bindingFromMaxRadiusThyroidToMaxRadiusThyroidText.Path = new PropertyPath("Value");
            bindingFromMaxRadiusThyroidToMaxRadiusThyroidText.Mode = BindingMode.OneWay;
            bindingFromMaxRadiusThyroidToMaxRadiusThyroidText.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            bindingFromMaxRadiusThyroidToMaxRadiusThyroidText.Converter = convertD2S;
            bindingFromMaxRadiusThyroidToMaxRadiusThyroidText.ConverterParameter = valueP50D0A300;
            BindingOperations.SetBinding(maxRadiusThyroidText, Label.ContentProperty, bindingFromMaxRadiusThyroidToMaxRadiusThyroidText);

            // 绑定：maxAngleThyroid.Value {属性} ==> maxAngleThyroidText.Text {Flyout控件}
            Binding bindingFromMaxAngleThyroidToMaxAngleThyroidText = new Binding();
            bindingFromMaxAngleThyroidToMaxAngleThyroidText.ElementName = "maxAngleThyroid";
            bindingFromMaxAngleThyroidToMaxAngleThyroidText.Path = new PropertyPath("Value");
            bindingFromMaxAngleThyroidToMaxAngleThyroidText.Mode = BindingMode.OneWay;
            bindingFromMaxAngleThyroidToMaxAngleThyroidText.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            bindingFromMaxAngleThyroidToMaxAngleThyroidText.Converter = convertD2S;
            bindingFromMaxAngleThyroidToMaxAngleThyroidText.ConverterParameter = valueP15D0A45;
            BindingOperations.SetBinding(maxAngleThyroidText, Label.ContentProperty, bindingFromMaxAngleThyroidToMaxAngleThyroidText);

            // 绑定：stopDistanceThyroid.Value {属性} ==> stopDistanceThyroidText.Content {Flyout控件}
            Binding bindingFromStopDistanceThyroidToStopDistanceThyroidText = new Binding();
            bindingFromStopDistanceThyroidToStopDistanceThyroidText.ElementName = "stopDistanceThyroid";
            bindingFromStopDistanceThyroidToStopDistanceThyroidText.Path = new PropertyPath("Value");
            bindingFromStopDistanceThyroidToStopDistanceThyroidText.Mode = BindingMode.OneWay;
            bindingFromStopDistanceThyroidToStopDistanceThyroidText.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            bindingFromStopDistanceThyroidToStopDistanceThyroidText.Converter = convertD2S;
            bindingFromStopDistanceThyroidToStopDistanceThyroidText.ConverterParameter = valueP50D0A400;
            BindingOperations.SetBinding(stopDistanceThyroidText, Label.ContentProperty, bindingFromStopDistanceThyroidToStopDistanceThyroidText);

            // change
            // 绑定：maxLoopDistThyroid.Value {属性} ==> maxLoopDistThyroidText.Content {Flyout控件}
            Binding bindingFromMaxLoopDistThyroidToMaxLoopDistThyroidText = new Binding();
            bindingFromMaxLoopDistThyroidToMaxLoopDistThyroidText.ElementName = "maxLoopDistThyroid";
            bindingFromMaxLoopDistThyroidToMaxLoopDistThyroidText.Path = new PropertyPath("Value");
            bindingFromMaxLoopDistThyroidToMaxLoopDistThyroidText.Mode = BindingMode.OneWay;
            bindingFromMaxLoopDistThyroidToMaxLoopDistThyroidText.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            bindingFromMaxLoopDistThyroidToMaxLoopDistThyroidText.Converter = convertD2S;
            bindingFromMaxLoopDistThyroidToMaxLoopDistThyroidText.ConverterParameter = valuePdot1D1Adot2;
            BindingOperations.SetBinding(maxLoopDistThyroidText, Label.ContentProperty, bindingFromMaxLoopDistThyroidToMaxLoopDistThyroidText);

            // 绑定：maxLoopAngleThyroid.Value {属性} ==> maxLoopAngleThyroidText.Text {Flyout控件}
            Binding bindingFromMaxLoopAngleThyroidToMaxLoopAngleThyroidText = new Binding();
            bindingFromMaxLoopAngleThyroidToMaxLoopAngleThyroidText.ElementName = "maxLoopAngleThyroid";
            bindingFromMaxLoopAngleThyroidToMaxLoopAngleThyroidText.Path = new PropertyPath("Value");
            bindingFromMaxLoopAngleThyroidToMaxLoopAngleThyroidText.Mode = BindingMode.OneWay;
            bindingFromMaxLoopAngleThyroidToMaxLoopAngleThyroidText.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            bindingFromMaxLoopAngleThyroidToMaxLoopAngleThyroidText.Converter = convertD2S;
            bindingFromMaxLoopAngleThyroidToMaxLoopAngleThyroidText.ConverterParameter = valuePdot02D2Adot03;
            BindingOperations.SetBinding(maxLoopAngleThyroidText, Label.ContentProperty, bindingFromMaxLoopAngleThyroidToMaxLoopAngleThyroidText);
            #endregion
        }

        // 加载界面
        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // app初始化失败
            if (!appInitialResult)
            {
                ReadyToClose();
                return;
            }

            // pipe连接
            bool pipeConnectResult = urvmr_lp.PipeBeginToConnect();
            if (!pipeConnectResult)
            {
                ReadyToCloseForPipe();
                return;
            }

            // model初始化失败
            if (modelInitialResult != 0)
            {
                ReadyToClose();
                return;
            }

            // UR连接
            urvm.StartConnection();
        }

        /// <summary>
        /// 初始化问题 准备关闭窗体
        /// </summary>
        private async void ReadyToClose()
        {
            if (!appInitialResult)
            {
                await urvm.ShowDialog("程序配置参数有误！", "错误", 13);

                urvm.ImmediateCloseWin();
                return;
            }

            if (modelInitialResult == 1)
            {
                await urvm.ShowDialog("资源配置检查过程出错！", "错误", 1);

                urvm.ImmediateCloseWin();
                return;
            }
            else if (modelInitialResult == 2)
            {
                await urvm.ShowDialog("数据库数据更新过程出错！", "错误", 12);

                urvm.ImmediateCloseWin();
                return;
            }

            urvm.ImmediateCloseWin();
        }

        /// <summary>
        /// Pipe连接问题 准备关闭窗体
        /// </summary>
        private async void ReadyToCloseForPipe()
        {
            await urvm.ShowDialog("Pipe连接失败！", "错误", 15);
            Logger.HistoryPrinting(Logger.Level.WARN, MethodBase.GetCurrentMethod().DeclaringType.FullName, "Pipe connection failed.");

            urvm.ImmediateCloseWin();
        }

        private void btnPowerOff_Click(object sender, RoutedEventArgs e)
        {
            urvm.CloseModelLogic();
        }

        private void btnElectricContorl_Click(object sender, RoutedEventArgs e)
        {
            urvm.SwitchElectricControlFly();

            e.Handled = true;
        }

        private void settingsFlyoutGalactophore_IsOpenChanged(object sender, RoutedEventArgs e)
        {
            bool nowState = (e.OriginalSource as Flyout).IsOpen;
            if (!nowState) urvm.SaveConfParameters(URViewModel.ConfPage.GalactophoreDetect);

            e.Handled = true;
        }

        private void settingsFlyoutPuncture_IsOpenChanged(object sender, RoutedEventArgs e)
        {
            bool nowState = (e.OriginalSource as Flyout).IsOpen;
            if (!nowState) urvm.SaveConfParameters(URViewModel.ConfPage.ThyroidScan);

            e.Handled = true;
        }

        private void powerOnBtn_Click(object sender, RoutedEventArgs e)
        {
            urvm.RobotPowerOn();

            e.Handled = true;
        }

        private void brakeLessBtn_Click(object sender, RoutedEventArgs e)
        {
            urvm.BrakeLess();

            e.Handled = true;
        }

        private void powerOffBtn_Click(object sender, RoutedEventArgs e)
        {
            urvm.RobotPowerOff();

            e.Handled = true;
        }

        private void powerDownBtn_Click(object sender, RoutedEventArgs e)
        {
            urvm.ControllerBoxPowerOff();

            e.Handled = true;
        }






    }
}
