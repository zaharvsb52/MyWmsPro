using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace wmsMLC.General.PL.WPF
{
    public static class BitMapEx
    {
        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        public static BitmapImage GetBitmapImage_old(this Bitmap bmp)
        {
            //return null;
            // если уже преобразовывали - достаем из "кэша"
            var tag = bmp.Tag as BitmapImage;
            if (tag != null)
                return tag;

            using (var memory = new MemoryStream())
            {
                bmp.Save(memory, ImageFormat.Png);
                memory.Position = 0;
                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                //RenderOptions.SetBitmapScalingMode(bitmapImage, BitmapScalingMode.NearestNeighbor);
                //RenderOptions.SetEdgeMode(bitmapImage, EdgeMode.Aliased);
                bitmapImage.EndInit();
                bitmapImage.Freeze();
                bmp.Tag = bitmapImage;
                return bitmapImage;
            }
        }

        public static ImageSource GetBitmapImage(this Bitmap bitmap)
        {
            // если уже преобразовывали - достаем из "кэша"
            var tag = bitmap.Tag as ImageSource;
            if (tag != null)
                return tag;

            IntPtr hBitmap = bitmap.GetHbitmap();
            try
            {
                var img = Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                img.Freeze();
                bitmap.Tag = img;
                return img;
            }
            catch
            {
                return null;
            }
            finally
            {
                DeleteObject(hBitmap);
            }
        }
    }
}