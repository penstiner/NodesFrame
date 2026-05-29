using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Core.UI.Converter
{
    /// <summary>
    /// 把图像路径转换为位图,给到Image控件使用
    /// </summary>
    public class ImageConvert : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            BitmapImage bitmapImage = null;
            //路径为空直接返回null
            if(value == null) { return null; }
            //文件不存在直接返回null
            if (!File.Exists(value.ToString())) return null;
            using (BinaryReader reader = new BinaryReader(File.Open(value.ToString(), FileMode.Open)))
            {
                try
                {
                    FileInfo fi = new FileInfo(value.ToString());
                    byte[] bytes = reader.ReadBytes((int)fi.Length);
                    reader.Close();

                    bitmapImage = new BitmapImage();
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;

                    bitmapImage.BeginInit();
                    bitmapImage.StreamSource = new MemoryStream(bytes);
                    bitmapImage.EndInit();
                }
                catch (Exception) 
                {
                    return null;
                }
            }
            return bitmapImage;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
