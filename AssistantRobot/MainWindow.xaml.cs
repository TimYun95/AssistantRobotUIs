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

        private readonly ConverterThatTransformDoubleToString convertD2S = new ConverterThatTransformDoubleToString();
        private readonly ValueProcesser valuePdot25D2 = new ValueProcesser(0.25, "0.00");
        private readonly ValueProcesser valuePdot5D1A1dot5 = new ValueProcesser(0.5, "0.0", 1.5);
        private readonly ValueProcesser valuePdot1D1 = new ValueProcesser(0.1, "0.0");
        private readonly ValueProcesser valueP1D0A15 = new ValueProcesser(1.0, "0", 15);
        private readonly ConverterThatTransformDoubleToWord convertD2W = new ConverterThatTransformDoubleToWord();

        private byte modelInitialResult = 0;
        private bool appInitialResult = true;

        public MainWindow()
        {
            InitializeComponent();

            // 定义VM
            urvm = new URVIewModel(out appInitialResult);

            // 定义RemoteVM_LocalPart
            urvmr_lp = new URViewModelRemote_LocalPart(urvm);

            // 初始化页
            mainPage = new MainPage(urvm);
            baseContorlPage = new BaseControl(urvm);
            galactophoreDetectPage = new GalactophoreDetect(urvm);
            urvm.DefineViews(this, mainPage, baseContorlPage, galactophoreDetectPage);

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

			// model初始化失败
            if (modelInitialResult != 0)
            {
                ReadyToClose();
				return;
            }
			
            // pipe连接
            bool pipeConnectResult = urvmr_lp.PipeBeginToConnect();
            if (!pipeConnectResult)
            {
                urvm.DirectCloseModelLogic();
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
            }
            else if (modelInitialResult == 2)
            {
                await urvm.ShowDialog("数据库数据更新过程出错！", "错误", 12);
            }
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
