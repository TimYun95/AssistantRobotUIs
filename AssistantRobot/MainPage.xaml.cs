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
    /// MainPage.xaml 的交互逻辑
    /// </summary>
    public partial class MainPage : Page
    {
        private readonly URViewModel urvm;

        public MainPage(URViewModel URVM)
        {
            InitializeComponent();

            urvm = URVM;
        }

        private void baseControl_Click(object sender, RoutedEventArgs e)
        {
            urvm.NavigateToPage(URViewModel.ShowPage.BaseControl);

            e.Handled = true;
        }

        private void galactophoreCheck_Click(object sender, RoutedEventArgs e)
        {
            urvm.NavigateToPage(URViewModel.ShowPage.GalactophoreDetect);

            // 进入乳腺扫查模块
            urvm.EnterGalactophoreDetectModule();

            e.Handled = true;
        }

        private void thyroidScanning_Click(object sender, RoutedEventArgs e)
        {
            urvm.NavigateToPage(URViewModel.ShowPage.ThyroidScanning);

            // 进入甲状腺扫查模块
            urvm.EnterThyroidScanningModule();

            e.Handled = true;
        }





    }
}
