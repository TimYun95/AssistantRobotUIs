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

namespace AssistantRobot
{
    /// <summary>
    /// ThyroidScan.xaml 的交互逻辑
    /// </summary>
    public partial class ThyroidScan
    {
        private readonly URViewModel urvm;
        private readonly ConverterThatTransformDoubleToString convertD2S = new ConverterThatTransformDoubleToString();
        private readonly ValueProcesser valueP25D0A25 = new ValueProcesser(25.0, "0", 25.0);
        private readonly ValueProcesser valueP50D0A50 = new ValueProcesser(50.0, "0", 50.0);
        
        public ThyroidScan(URViewModel URVM)
        {
            this.InitializeComponent();

            urvm = URVM;
            PartialBindingsEstablish();
        }

        private void PartialBindingsEstablish()
        {
            // 绑定：factorPosSlider.Value {属性} ==> factorPos.Text {ThyroidScan控件}
            Binding bindingFromFactorPosSliderToFactorPos = new Binding();
            bindingFromFactorPosSliderToFactorPos.ElementName = "factorPosSlider";
            bindingFromFactorPosSliderToFactorPos.Path = new PropertyPath("Value");
            bindingFromFactorPosSliderToFactorPos.Mode = BindingMode.OneWay;
            bindingFromFactorPosSliderToFactorPos.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            bindingFromFactorPosSliderToFactorPos.Converter = convertD2S;
            bindingFromFactorPosSliderToFactorPos.ConverterParameter = valueP25D0A25;
            BindingOperations.SetBinding(factorPos, TextBox.TextProperty, bindingFromFactorPosSliderToFactorPos);

            // 绑定：factorAttSlider.Value {属性} ==> factorAtt.Text {ThyroidScan控件}
            Binding bindingFromFactorAttSliderToFactorAtt = new Binding();
            bindingFromFactorAttSliderToFactorAtt.ElementName = "factorAttSlider";
            bindingFromFactorAttSliderToFactorAtt.Path = new PropertyPath("Value");
            bindingFromFactorAttSliderToFactorAtt.Mode = BindingMode.OneWay;
            bindingFromFactorAttSliderToFactorAtt.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            bindingFromFactorAttSliderToFactorAtt.Converter = convertD2S;
            bindingFromFactorAttSliderToFactorAtt.ConverterParameter = valueP25D0A25;
            BindingOperations.SetBinding(factorAtt, TextBox.TextProperty, bindingFromFactorAttSliderToFactorAtt);

            // 绑定：factorFosSlider.Value {属性} ==> factorFos.Text {ThyroidScan控件}
            Binding bindingFromFactorFosSliderToFactorFos = new Binding();
            bindingFromFactorFosSliderToFactorFos.ElementName = "factorFosSlider";
            bindingFromFactorFosSliderToFactorFos.Path = new PropertyPath("Value");
            bindingFromFactorFosSliderToFactorFos.Mode = BindingMode.OneWay;
            bindingFromFactorFosSliderToFactorFos.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            bindingFromFactorFosSliderToFactorFos.Converter = convertD2S;
            bindingFromFactorFosSliderToFactorFos.ConverterParameter = valueP50D0A50;
            BindingOperations.SetBinding(factorFos, TextBox.TextProperty, bindingFromFactorFosSliderToFactorFos);
        }

        private void iconBackThyroid_Click(object sender, RoutedEventArgs e)
        {
            urvm.NavigateToPage(URViewModel.ShowPage.MainNav);

            // 退出甲状腺扫描模块
            urvm.ExitThyroidScanningModule();

            e.Handled = true;
        }

        private void iconSetThyroid_Click(object sender, RoutedEventArgs e)
        {
            urvm.SwitchThyroidOwnConf();

            e.Handled = true;
        }

        private void iconForceToZeroThyroid_Click(object sender, RoutedEventArgs e)
        {
            // 力清零
            urvm.ForceClearThyroidScanningModule();

            e.Handled = true;
        }

        private void iconConfThyroid_Click(object sender, RoutedEventArgs e)
        {
            // 配置参数
            urvm.ConfParamsThyroidScanningModule();

            e.Handled = true;
        }

        private void startThyroidBtn_Click(object sender, RoutedEventArgs e)
        {
            if ((string)startThyroidBtn.Content == "开始寻找")
            {
                urvm.StartPositionFindThyroidScanningModule();
                startThyroidBtn.Content = "确认找到";
            }
            else
            {
                urvm.StartPositionFoundThyroidScanningModule();
                startThyroidBtn.Content = "开始寻找";
            }

            e.Handled = true;
        }

        private void nextBtn_Click(object sender, RoutedEventArgs e)
        {
            urvm.ConfParamsNextParamsThyroidScannerModule();

            e.Handled = true;
        }

        private void iconConfConfirmThyroid_Click(object sender, RoutedEventArgs e)
        {
            ConfirmParams();
            e.Handled = true;
        }

        private async void ConfirmParams()
        {
            bool result = await urvm.ShowBranchDialog("是否确认所配置的参数？", "提问");

            // 确认参数配置
            if (result) urvm.ConfirmConfParamsThyroidScanningModule();
        }

        private void iconBeginThyroid_Click(object sender, RoutedEventArgs e)
        {
            BeginScan();
            e.Handled = true;
        }

        private async void BeginScan()
        {
            bool result = await urvm.ShowBranchDialog("是否开始扫查？", "提问");

            // 确认参数配置
            if (result) urvm.ReadyAndStartGalactophoreDetectModule();
        }

        private void iconStopThyroid_Click(object sender, RoutedEventArgs e)
        {
            // 急停
            urvm.StopMotionNowThyroidScanningModule();

            e.Handled = true;
        }

        private void parametersChangeWhenRunning(object sender, RoutedEventArgs e)
        {
            urvm.ModifyControlParametersInRunWorkThyroidScannerModule();

            e.Handled = true;
        }

    }
}
