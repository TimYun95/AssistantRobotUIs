using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Data;
using System.Globalization;
using URModule;

namespace AssistantRobot
{
    /// <summary>
    /// 数值处理类
    /// </summary>
    public class ValueProcesser
    {
        /// <summary>
        /// 数值倍数
        /// </summary>
        public double ValueRatio { get; set; }

        /// <summary>
        /// 数值加数
        /// </summary>
        public double ValueAdd { get; set; }

        /// <summary>
        /// 小数舍入精度
        /// </summary>
        public string DecimalPrecision { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="Prop">数值倍数</param>
        /// <param name="BitCapacity">小数舍入精度</param>
        /// <param name="Add">数值加数</param>
        public ValueProcesser(double Prop, string BitCapacity, double Add = 0.0)
        {
            ValueRatio = Prop;
            DecimalPrecision = BitCapacity;
            ValueAdd = Add;
        }
    }

    /// <summary>
    /// double数据转化为string，有舍入精度
    /// </summary>
    public class ConverterThatTransformDoubleToString : IValueConverter
    {
        // source --> target
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double dataFromSource = (double)value;
            ValueProcesser transformedDecimal = (ValueProcesser)parameter;
            string dataToTarget = (dataFromSource * transformedDecimal.ValueRatio + transformedDecimal.ValueAdd).ToString(transformedDecimal.DecimalPrecision);
            return dataToTarget;
        }

        // source <-- target
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// double数据转化为double，乘以倍数且限幅
    /// </summary>
    public class ConverterThatTransformDoubleToDoubleSlider : IValueConverter
    {
        // source --> target
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double dataFromSource = (double)value;
            double dataRatio = (double)parameter;
            double dataToTarget = (double)Math.Sign(dataFromSource * dataRatio) * ((Math.Abs(dataFromSource * dataRatio) + 360.0) % 720.0 - 360.0); // 限幅于-360~+360
            return dataToTarget;
        }

        // source <-- target
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// double数据转化为string（有语义）
    /// </summary>
    public class ConverterThatTransformDoubleToWord : IValueConverter
    {
        // source --> target
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double dataFromSource = (double)value;
            int wordFlag = (int)Math.Round(dataFromSource);
            string[] showingWords = (string[])parameter;
            string dataToTarget = showingWords[wordFlag];
            return dataToTarget;
        }

        // source <-- target
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// double[]数据转化为string，按照index选择并放大和取相应位数
    /// </summary>
    public class ConverterThatTransformDoubleArrayToString : IValueConverter
    {
        // source --> target
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double[] dataFromSource = (double[])value;
            int index = ((int[])parameter)[0];
            int prop = ((int[])parameter)[1];
            int deci = ((int[])parameter)[2];
            string formatDecimal = "0";
            if (deci >= 1)
            {
                formatDecimal += ".";
                for (int i = 0; i < deci; ++i)
                {
                    formatDecimal += "0"; 
                }
            }
            string dataToTarget = (dataFromSource[index] * (double)prop).ToString(formatDecimal);
            return dataToTarget;
        }

        // source <-- target
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// double数据转化为double，平移放大并取整
    /// </summary>
    public class ConverterThatTransformDoubleToDoubleInteger : IValueConverter
    {
        // source --> target
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double dataFromSource = (double)value;
            double[] dataTransform = (double[])parameter;
            double dataToTarget = Math.Round((dataFromSource + dataTransform[0]) * dataTransform[1]);
            return dataToTarget;
        }

        // source <-- target
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// enum数据转化为double
    /// </summary>
    public class ConverterThatTransformEnumToDouble : IValueConverter
    {
        // source --> target
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Type type = (Type)parameter;
            return (byte)Enum.Parse(type, value.ToString());
        }

        // source <-- target
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// enum数据转化为bool
    /// </summary>
    public class ConverterThatTransformEnumToBool : IValueConverter
    {
        // source --> target
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Type type = (Type)parameter;
            var nameArray = Enum.GetValues(type);
            for (int i = 0; i < nameArray.Length; ++i)
            {
                if (nameArray.GetValue(i).Equals(value))
                {
                    return i <= 0 ? false : true;
                }
            }
            return false;
        }

        // source <-- target
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// 乳腺扫查中相关状态转化为Enable属性bool类型
    /// </summary>
    public class ConverterMultiStatusToEnableBool : IMultiValueConverter
    {
        // source --> target
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            short nowWorkStatus = (short)values[0];
            bool ifFZ = false;
            bool ifCP = false;
            object[] paramsGet = (object[])parameter;
            byte[] ifSubDivision = (byte[])paramsGet[0];
            byte subDivisionFlag = 0;
            OperateModuleBase.WorkStatus[] aimWorkStatus = (OperateModuleBase.WorkStatus[])(paramsGet[1]);

            int lengthV = values.Length;
            switch (lengthV)
            {
                case 1:
                    foreach (OperateModuleBase.WorkStatus item in aimWorkStatus)
                    {
                        if ((short)item == nowWorkStatus) return true;
                    }
                    return false;
                case 2:
                    if (ifSubDivision[0] >= 1)
                    {
                        subDivisionFlag = (byte)values[1];
                        foreach (byte flag in ifSubDivision)
                        {
                            foreach (OperateModuleBase.WorkStatus item in aimWorkStatus)
                            {
                                if (((short)item == nowWorkStatus) && (flag == subDivisionFlag)) return true;
                            }
                        }
                    }
                    else
                    {
                        ifFZ = (bool)values[1]; // 也可ifCP
                        foreach (OperateModuleBase.WorkStatus item in aimWorkStatus)
                        {
                            if (((short)item == nowWorkStatus) && ifFZ) return true;
                        }
                    }
                    return false;
                case 3:
                    ifFZ = (bool)values[1];
                    ifCP = (bool)values[2];
                    foreach (OperateModuleBase.WorkStatus item in aimWorkStatus)
                    {
                        if (((short)item == nowWorkStatus) && ifFZ && ifCP) return true;
                    }
                    return false;
                default:
                    return false;
            }
        }

        // source <-- target
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    
    /// <summary>
    /// 多源绑定 与逻辑 Bool输出
    /// </summary>
    public class ConverterMultiEnableToEnableAndLogicBool : IMultiValueConverter
    {
        // source --> target
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            foreach (bool item in values)
            {
                if (!item) return false;
            }
            return true;
        }

        // source <-- target
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// 多源绑定 与逻辑 画板色输出
    /// </summary>
    public class ConverterMultiEnableToBackgroundAndLogicColor : IMultiValueConverter
    {
        // source --> target
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            object[] paramsGet = (object[])parameter;
            foreach (bool item in values)
            {
                if (!item) return paramsGet[0];
            }
            return paramsGet[1];
        }

        // source <-- target
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }



































}
