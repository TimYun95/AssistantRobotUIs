using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Threading.Tasks;
using System.Threading;

namespace AssistantRobot
{
    /// <summary>
    /// GalactophoreDetect.xaml 的交互逻辑
    /// </summary>
    public partial class GalactophoreDetect
    {
        private readonly URViewModel urvm;

        public GalactophoreDetect(URViewModel URVM)
        {
            this.InitializeComponent();

            urvm = URVM;
        }

        private void iconBackGalactophore_Click(object sender, RoutedEventArgs e)
        {
            urvm.NavigateToPage(URViewModel.ShowPage.MainNav);

            // 退出乳腺扫描模块
            urvm.ExitGalactophoreDetectModule();

            e.Handled = true;
        }

        private void iconSetGalactophore_Click(object sender, RoutedEventArgs e)
        {
            urvm.SwitchGalactophoreOwnConf();

            e.Handled = true;
        }

        private void iconForceToZeroGalactophore_Click(object sender, RoutedEventArgs e)
        {
            // 力清零
            urvm.ForceClearGalactophoreDetectModule();

            e.Handled = true;
        }

        private void iconConfGalactophore_Click(object sender, RoutedEventArgs e)
        {
            // 配置参数
            urvm.ConfParamsGalactophoreDetectModule();

            e.Handled = true;
        }

        private void nippleBtn_Click(object sender, RoutedEventArgs e)
        {
            if ((string)nippleBtn.Content == "开始寻找")
            {
                urvm.NippleFindGalactophoreDetectModule();
                nippleBtn.Content = "确认找到";
            }
            else
            {
                urvm.NippleFoundGalactophoreDetectModule();
                nippleBtn.Content = "开始寻找";
            }

            e.Handled = true;
        }

        private void liftDistanceBtn_Click(object sender, RoutedEventArgs e)
        {
            if ((string)liftDistanceBtn.Content == "开始寻找")
            {
                urvm.LiftDistanceFindGalactophoreDetectModule();
                liftDistanceBtn.Content = "确认找到";
            }
            else
            {
                urvm.LiftDistanceFoundGalactophoreDetectModule();
                liftDistanceBtn.Content = "开始寻找";
            }

            e.Handled = true;
        }

        private void minRadiusBtn_Click(object sender, RoutedEventArgs e)
        {
            if ((string)minRadiusBtn.Content == "开始寻找")
            {
                urvm.MinRadiusFindGalactophoreDetectModule();
                minRadiusBtn.Content = "确认找到";
            }
            else
            {
                urvm.MinRadiusFoundGalactophoreDetectModule();
                minRadiusBtn.Content = "开始寻找";
            }

            e.Handled = true;
        }

        private void scanDistanceBtn_Click(object sender, RoutedEventArgs e)
        {
            if ((string)scanDistanceBtn.Content == "开始寻找")
            {
                urvm.ScanDeepFindGalactophoreDetectModule();
                scanDistanceBtn.Content = "确认找到";
            }
            else
            {
                urvm.ScanDeepFoundGalactophoreDetectModule();
                scanDistanceBtn.Content = "开始寻找";
            }

            e.Handled = true;
        }

        private void headBoundBtn_Click(object sender, RoutedEventArgs e)
        {
            if ((string)headBoundBtn.Content == "开始寻找")
            {
                if (urvm.BoundFindGalactophoreDetectModule("head")) headBoundBtn.Content = "确认找到";
            }
            else
            {
                urvm.BoundFoundGalactophoreDetectModule("head");
                headBoundBtn.Content = "开始寻找";
            }

            e.Handled = true;
        }

        private void tailBoundBtn_Click(object sender, RoutedEventArgs e)
        {
            if ((string)tailBoundBtn.Content == "开始寻找")
            {
                if (urvm.BoundFindGalactophoreDetectModule("tail")) tailBoundBtn.Content = "确认找到";
            }
            else
            {
                urvm.BoundFoundGalactophoreDetectModule("tail");
                tailBoundBtn.Content = "开始寻找";
            }

            e.Handled = true;
        }

        private void outBoundBtn_Click(object sender, RoutedEventArgs e)
        {
            if ((string)outBoundBtn.Content == "开始寻找")
            {
                if (urvm.BoundFindGalactophoreDetectModule("out")) outBoundBtn.Content = "确认找到";
            }
            else
            {
                urvm.BoundFoundGalactophoreDetectModule("out");
                outBoundBtn.Content = "开始寻找";
            }

            e.Handled = true;
        }

        private void inBoundBtn_Click(object sender, RoutedEventArgs e)
        {
            if ((string)inBoundBtn.Content == "开始寻找")
            {
                if (urvm.BoundFindGalactophoreDetectModule("in")) inBoundBtn.Content = "确认找到";
            }
            else
            {
                urvm.BoundFoundGalactophoreDetectModule("in");
                inBoundBtn.Content = "开始寻找";
            }

            e.Handled = true;
        }

        //private void sinkDistanceBtn_Click(object sender, RoutedEventArgs e)
        //{
        //    urvm.SinkDeepFindGalactophoreDetectModule();

        //    e.Handled = true;
        //}

        private void nextBtn_Click(object sender, RoutedEventArgs e)
        {
            urvm.ConfParamsNextParamsGalactophoreDetectModule();

            e.Handled = true;
        }

        private void iconConfConfirmGalactophore_Click(object sender, RoutedEventArgs e)
        {
            ConfirmParams();
            e.Handled = true;
        }

        private async void ConfirmParams()
        {
            bool result = await urvm.ShowBranchDialog("是否确认所配置的参数？", "提问");

            // 确认参数配置
            if (result) urvm.ConfirmConfParamsGalactophoreDetectModule();
        }

        private void iconBeginGalactophore_Click(object sender, RoutedEventArgs e)
        {
            BeginScan();
            e.Handled = true;
        }

        private async void BeginScan()
        {
            bool result = await urvm.ShowBranchDialog("是否开始扫查？", "提问");
            if (result) result = await urvm.ShowBranchDialog("是否进行完整扫查？", "提问");
            if (result)
            {
                result = await urvm.ShowBranchDialog("是否改变姿态角？", "提问");
                if (result) urvm.ReadyAndStartGalactophoreDetectModule();
                else urvm.ReadyAndStartGalactophoreDetectModule(false);
            }
            else
            {
                string reply = await urvm.ShowInputDialog("请输入要扫描的角度(Deg)：", "输入");
                if (!string.IsNullOrEmpty(reply))
                {
                    result = await urvm.ShowBranchDialog("是否改变姿态角？", "提问");
                    if (result) urvm.ReadyAndStartGalactophoreDetectModule(true, false, double.Parse(reply) / 180.0 * Math.PI);
                    else urvm.ReadyAndStartGalactophoreDetectModule(false, false, double.Parse(reply) / 180.0 * Math.PI);
                }
            }
        }

        private void iconStopGalactophore_Click(object sender, RoutedEventArgs e)
        {
            // 急停
            urvm.StopMotionNowGalactophoreDetectModule();

            e.Handled = true;
        }
    }
}