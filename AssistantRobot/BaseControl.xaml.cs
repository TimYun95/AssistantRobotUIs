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

namespace AssistantRobot
{
    /// <summary>
    /// BaseControl.xaml 的交互逻辑
    /// </summary>
	public partial class BaseControl
	{
        private readonly URVIewModel urvm;
        private readonly ConverterThatTransformDoubleToString convertD2S = new ConverterThatTransformDoubleToString();
        private readonly ValueProcesser valueP1D2 = new ValueProcesser(1.0, "0.00");

        public BaseControl(URVIewModel URVM)
		{
			this.InitializeComponent();

            urvm = URVM;
            PartialBindingsEstablish();

            teachMotionBtn.AddHandler(Button.MouseDownEvent, new MouseButtonEventHandler(teachMotionBtn_MouseDown), true);
            teachMotionBtn.AddHandler(Button.MouseUpEvent, new MouseButtonEventHandler(teachMotionBtn_MouseUp), true);
		}

        private void PartialBindingsEstablish()
        {
            // 绑定：j1Silder.Value {属性} ==> j1Motion.Text {BaseControl控件}
            Binding bindingFromJ1SilderValueToJ1MotionText = new Binding();
            bindingFromJ1SilderValueToJ1MotionText.ElementName = "j1Silder";
            bindingFromJ1SilderValueToJ1MotionText.Path = new PropertyPath("Value");
            bindingFromJ1SilderValueToJ1MotionText.Mode = BindingMode.OneWay;
            bindingFromJ1SilderValueToJ1MotionText.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            bindingFromJ1SilderValueToJ1MotionText.Converter = convertD2S;
            bindingFromJ1SilderValueToJ1MotionText.ConverterParameter = valueP1D2;
            BindingOperations.SetBinding(j1Motion, TextBox.TextProperty, bindingFromJ1SilderValueToJ1MotionText);

            // 绑定：j2Silder.Value {属性} ==> j2Motion.Text {BaseControl控件}
            Binding bindingFromJ2SilderValueToJ2MotionText = new Binding();
            bindingFromJ2SilderValueToJ2MotionText.ElementName = "j2Silder";
            bindingFromJ2SilderValueToJ2MotionText.Path = new PropertyPath("Value");
            bindingFromJ2SilderValueToJ2MotionText.Mode = BindingMode.OneWay;
            bindingFromJ2SilderValueToJ2MotionText.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            bindingFromJ2SilderValueToJ2MotionText.Converter = convertD2S;
            bindingFromJ2SilderValueToJ2MotionText.ConverterParameter = valueP1D2;
            BindingOperations.SetBinding(j2Motion, TextBox.TextProperty, bindingFromJ2SilderValueToJ2MotionText);

            // 绑定：j3Silder.Value {属性} ==> j3Motion.Text {BaseControl控件}
            Binding bindingFromJ3SilderValueToJ3MotionText = new Binding();
            bindingFromJ3SilderValueToJ3MotionText.ElementName = "j3Silder";
            bindingFromJ3SilderValueToJ3MotionText.Path = new PropertyPath("Value");
            bindingFromJ3SilderValueToJ3MotionText.Mode = BindingMode.OneWay;
            bindingFromJ3SilderValueToJ3MotionText.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            bindingFromJ3SilderValueToJ3MotionText.Converter = convertD2S;
            bindingFromJ3SilderValueToJ3MotionText.ConverterParameter = valueP1D2;
            BindingOperations.SetBinding(j3Motion, TextBox.TextProperty, bindingFromJ3SilderValueToJ3MotionText);

            // 绑定：j4Silder.Value {属性} ==> j4Motion.Text {BaseControl控件}
            Binding bindingFromJ4SilderValueToJ4MotionText = new Binding();
            bindingFromJ4SilderValueToJ4MotionText.ElementName = "j4Silder";
            bindingFromJ4SilderValueToJ4MotionText.Path = new PropertyPath("Value");
            bindingFromJ4SilderValueToJ4MotionText.Mode = BindingMode.OneWay;
            bindingFromJ4SilderValueToJ4MotionText.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            bindingFromJ4SilderValueToJ4MotionText.Converter = convertD2S;
            bindingFromJ4SilderValueToJ4MotionText.ConverterParameter = valueP1D2;
            BindingOperations.SetBinding(j4Motion, TextBox.TextProperty, bindingFromJ4SilderValueToJ4MotionText);

            // 绑定：j5Silder.Value {属性} ==> j5Motion.Text {BaseControl控件}
            Binding bindingFromJ5SilderValueToJ5MotionText = new Binding();
            bindingFromJ5SilderValueToJ5MotionText.ElementName = "j5Silder";
            bindingFromJ5SilderValueToJ5MotionText.Path = new PropertyPath("Value");
            bindingFromJ5SilderValueToJ5MotionText.Mode = BindingMode.OneWay;
            bindingFromJ5SilderValueToJ5MotionText.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            bindingFromJ5SilderValueToJ5MotionText.Converter = convertD2S;
            bindingFromJ5SilderValueToJ5MotionText.ConverterParameter = valueP1D2;
            BindingOperations.SetBinding(j5Motion, TextBox.TextProperty, bindingFromJ5SilderValueToJ5MotionText);

            // 绑定：j6Silder.Value {属性} ==> j6Motion.Text {BaseControl控件}
            Binding bindingFromJ6SilderValueToJ6MotionText = new Binding();
            bindingFromJ6SilderValueToJ6MotionText.ElementName = "j6Silder";
            bindingFromJ6SilderValueToJ6MotionText.Path = new PropertyPath("Value");
            bindingFromJ6SilderValueToJ6MotionText.Mode = BindingMode.OneWay;
            bindingFromJ6SilderValueToJ6MotionText.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            bindingFromJ6SilderValueToJ6MotionText.Converter = convertD2S;
            bindingFromJ6SilderValueToJ6MotionText.ConverterParameter = valueP1D2;
            BindingOperations.SetBinding(j6Motion, TextBox.TextProperty, bindingFromJ6SilderValueToJ6MotionText);
        }

        private void iconBackMotion_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack) NavigationService.GoBack();

            e.Handled = true;
        }

        private void gridMotionT_MouseOnlyDown(object sender, RoutedEventArgs e)
        {
            string buttonName = (e.OriginalSource as IconButton).Name;
            char axis = 'x'; bool ifPos = true;
            switch (buttonName)
            {
                case "iconMotionXP":
                    axis = 'x'; ifPos = true;
                    break;
                case "iconMotionXN":
                    axis = 'x'; ifPos = false;
                    break;
                case "iconMotionYP":
                    axis = 'y'; ifPos = true;
                    break;
                case "iconMotionYN":
                    axis = 'y'; ifPos = false;
                    break;
                case "iconMotionZP":
                    axis = 'z'; ifPos = true;
                    break;
                case "iconMotionZN":
                    axis = 'z'; ifPos = false;
                    break;
                default:
                    break;
            }
            urvm.BaseMovingTranslationBegin(axis, ifPos);

            e.Handled = true;
        }

        private void gridMotionA_MouseOnlyDown(object sender, RoutedEventArgs e)
        {
            string buttonName = (e.OriginalSource as IconButton).Name;
            char axis = 'x'; bool ifPos = true;
            switch (buttonName)
            {
                case "iconAttitudeXP":
                    axis = 'x'; ifPos = true;
                    break;
                case "iconAttitudeXN":
                    axis = 'x'; ifPos = false;
                    break;
                case "iconAttitudeYP":
                    axis = 'y'; ifPos = true;
                    break;
                case "iconAttitudeYN":
                    axis = 'y'; ifPos = false;
                    break;
                case "iconAttitudeZP":
                    axis = 'z'; ifPos = true;
                    break;
                case "iconAttitudeZN":
                    axis = 'z'; ifPos = false;
                    break;
                default:
                    break;
            }
            urvm.BaseMovingSpinBegin(axis, ifPos);

            e.Handled = true;
        }

        private void gridMotionJ_MouseOnlyDown(object sender, RoutedEventArgs e)
        {
            string buttonName = (e.OriginalSource as IconButton).Name;
            char axis = '1'; bool ifPos = true;
            switch (buttonName)
            {
                case "j1AMotion":
                    axis = '1'; ifPos = true;
                    break;
                case "j1SMotion":
                    axis = '1'; ifPos = false;
                    break;
                case "j2AMotion":
                    axis = '2'; ifPos = true;
                    break;
                case "j2SMotion":
                    axis = '2'; ifPos = false;
                    break;
                case "j3AMotion":
                    axis = '3'; ifPos = true;
                    break;
                case "j3SMotion":
                    axis = '3'; ifPos = false;
                    break;
                case "j4AMotion":
                    axis = '4'; ifPos = true;
                    break;
                case "j4SMotion":
                    axis = '4'; ifPos = false;
                    break;
                case "j5AMotion":
                    axis = '5'; ifPos = true;
                    break;
                case "j5SMotion":
                    axis = '5'; ifPos = false;
                    break;
                case "j6AMotion":
                    axis = '6'; ifPos = true;
                    break;
                case "j6SMotion":
                    axis = '6'; ifPos = false;
                    break;
                default:
                    break;
            }
            urvm.BaseMovingSingleSpinBegin(axis, ifPos);

            e.Handled = true;
        }

        private void gridMotion_MouseOnlyUp(object sender, RoutedEventArgs e)
        {
            urvm.BaseMovingEnd();

            e.Handled = true;
        }

        private void teachMotionBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            urvm.TeachModeTurn();

            e.Handled = true;
        }

        private void teachMotionBtn_MouseUp(object sender, MouseButtonEventArgs e)
        {
            urvm.TeachModeTurn(false);

            e.Handled = true;
        }


	}
}