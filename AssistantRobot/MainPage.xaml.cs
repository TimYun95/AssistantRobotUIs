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
        private readonly URVIewModel urvm;

        public MainPage(URVIewModel URVM)
        {
            InitializeComponent();

            urvm = URVM;
        }

        private void baseControl_Click(object sender, RoutedEventArgs e)
        {
            urvm.NavigateToPage(URVIewModel.ShowPage.BaseControl);

            e.Handled = true;
        }

        private void galactophoreCheck_Click(object sender, RoutedEventArgs e)
        {
            urvm.NavigateToPage(URVIewModel.ShowPage.GalactophoreDetect);

            // test
            urvm.GalactophoreDetectorWorkStatus = 0;

            e.Handled = true;
        }





    }
}
