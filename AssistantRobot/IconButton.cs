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
    public class IconButton : Button
    {
        /// <summary>
        /// 静态构造，替换默认值，仅执行一次
        /// </summary>
        static IconButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(IconButton),
                                                                                 new FrameworkPropertyMetadata(typeof(IconButton)));
        }

        #region Property Wrappers
        public Brush MouseOverBackground
        {
            get
            {
                return (Brush)GetValue(MouseOverBackgroundProperty);
            }
            set { SetValue(MouseOverBackgroundProperty, value); }
        }

        public Brush MouseDownBackground
        {
            get
            {
                return (Brush)GetValue(MouseDownBackgroundProperty);
            }
            set { SetValue(MouseDownBackgroundProperty, value); }
        }

        public Brush MouseOverForeground
        {
            get
            {
                return (Brush)GetValue(MouseOverForegroundProperty);
            }
            set { SetValue(MouseOverForegroundProperty, value); }
        }

        public Brush MouseDownForeground
        {
            get
            {
                return (Brush)GetValue(MouseDownForegroundProperty);
            }
            set { SetValue(MouseDownForegroundProperty, value); }
        }

        public Brush MouseOverBorderBrush
        {
            get { return (Brush)GetValue(MouseOverBorderBrushProperty); }
            set { SetValue(MouseOverBorderBrushProperty, value); }
        }

        public Brush MouseDownBorderBrush
        {
            get { return (Brush)GetValue(MouseDownBorderBrushProperty); }
            set { SetValue(MouseDownBorderBrushProperty, value); }
        }

        public CornerRadius CornerRadius
        {
            get { return (CornerRadius)GetValue(CornerRadiusProperty); }
            set { SetValue(CornerRadiusProperty, value); }
        }

        public Brush TextColor
        {
            get { return (Brush)GetValue(TextColorProperty); }
            set { SetValue(TextColorProperty, value); }
        }

        public string TextContent
        {
            get { return (string)GetValue(TextContentProperty); }
            set { SetValue(TextContentProperty, value); }
        }

        public double TextSize
        {
            get { return (double)GetValue(TextSizeProperty); }
            set { SetValue(TextSizeProperty, value); }
        }

        public HorizontalAlignment TextHorizontalAlignment
        {
            get { return (HorizontalAlignment)GetValue(TextHorizontalAlignmentProperty); }
            set { SetValue(TextHorizontalAlignmentProperty, value); }
        }

        public VerticalAlignment TextVerticalAlignment
        {
            get { return (VerticalAlignment)GetValue(TextVerticalAlignmentProperty); }
            set { SetValue(TextVerticalAlignmentProperty, value); }
        }
        #endregion

        #region Dependency Properties
        /// <summary>  
        /// 鼠标移上去的背景颜色  
        /// </summary>  
        public static readonly DependencyProperty MouseOverBackgroundProperty
            = DependencyProperty.Register("MouseOverBackground", typeof(Brush), typeof(IconButton), new PropertyMetadata());

        /// <summary>  
        /// 鼠标按下去的背景颜色  
        /// </summary>  
        public static readonly DependencyProperty MouseDownBackgroundProperty
            = DependencyProperty.Register("MouseDownBackground", typeof(Brush), typeof(IconButton), new PropertyMetadata());

        /// <summary>  
        /// 鼠标移上去的字体颜色  
        /// </summary>  
        public static readonly DependencyProperty MouseOverForegroundProperty
            = DependencyProperty.Register("MouseOverForeground", typeof(Brush), typeof(IconButton), new PropertyMetadata());

        /// <summary>  
        /// 鼠标按下去的字体颜色  
        /// </summary>  
        public static readonly DependencyProperty MouseDownForegroundProperty
            = DependencyProperty.Register("MouseDownForeground", typeof(Brush), typeof(IconButton), new PropertyMetadata());

        /// <summary>  
        /// 鼠标移上去的边框颜色  
        /// </summary>  
        public static readonly DependencyProperty MouseOverBorderBrushProperty
            = DependencyProperty.Register("MouseOverBorderBrush", typeof(Brush), typeof(IconButton), new PropertyMetadata());

        /// <summary>  
        /// 鼠标按下去的边框颜色  
        /// </summary>  
        public static readonly DependencyProperty MouseDownBorderBrushProperty
            = DependencyProperty.Register("MouseDownBorderBrush", typeof(Brush), typeof(IconButton), new PropertyMetadata());

        /// <summary>  
        /// 圆角  
        /// </summary>  
        public static readonly DependencyProperty CornerRadiusProperty
            = DependencyProperty.Register("CornerRadius", typeof(CornerRadius), typeof(IconButton), new PropertyMetadata());

        /// <summary>  
        /// 字体颜色
        /// </summary>  
        public static readonly DependencyProperty TextColorProperty
            = DependencyProperty.Register("TextColor", typeof(Brush), typeof(IconButton), new PropertyMetadata());

        /// <summary>  
        /// 字体内容 
        /// </summary>  
        public static readonly DependencyProperty TextContentProperty
            = DependencyProperty.Register("TextContent", typeof(string), typeof(IconButton), new PropertyMetadata());

        /// <summary>  
        /// 字体大小 
        /// </summary>  
        public static readonly DependencyProperty TextSizeProperty
            = DependencyProperty.Register("TextSize", typeof(double), typeof(IconButton), new PropertyMetadata());

        /// <summary>  
        /// 字体水平位置 
        /// </summary>  
        public static readonly DependencyProperty TextHorizontalAlignmentProperty
            = DependencyProperty.Register("TextHorizontalAlignment", typeof(HorizontalAlignment), typeof(IconButton), new PropertyMetadata());

        /// <summary>  
        /// 字体垂直位置 
        /// </summary>  
        public static readonly DependencyProperty TextVerticalAlignmentProperty
            = DependencyProperty.Register("TextVerticalAlignment", typeof(VerticalAlignment), typeof(IconButton), new PropertyMetadata());
        #endregion

        #region Event Wrappers
        public event EventHandler<RoutedEventArgs> MouseOnlyDown
        {
            add { this.AddHandler(MouseOnlyDownEvent, value); }
            remove { this.RemoveHandler(MouseOnlyDownEvent, value); }
        }

        public event EventHandler<RoutedEventArgs> MouseOnlyUp
        {
            add { this.AddHandler(MouseOnlyUpEvent, value); }
            remove { this.RemoveHandler(MouseOnlyUpEvent, value); }
        }
        #endregion

        #region Dependency Events
        /// <summary>  
        /// 鼠标按下 
        /// </summary>  
        public static readonly RoutedEvent MouseOnlyDownEvent
            = EventManager.RegisterRoutedEvent("MouseOnlyDown", RoutingStrategy.Bubble, typeof(EventHandler<RoutedEventArgs>), typeof(IconButton));

        /// <summary>  
        /// 鼠标抬起 
        /// </summary>  
        public static readonly RoutedEvent MouseOnlyUpEvent
            = EventManager.RegisterRoutedEvent("MouseOnlyUp", RoutingStrategy.Bubble, typeof(EventHandler<RoutedEventArgs>), typeof(IconButton));
        #endregion

        #region Active Functions
        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);

            RoutedEventArgs args = new RoutedEventArgs(MouseOnlyDownEvent, this);
            this.RaiseEvent(args);
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);

            RoutedEventArgs args = new RoutedEventArgs(MouseOnlyUpEvent, this);
            this.RaiseEvent(args);
        }
        #endregion
    }
}

