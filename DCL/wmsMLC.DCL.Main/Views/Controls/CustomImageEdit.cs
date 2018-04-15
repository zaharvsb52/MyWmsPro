using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using DevExpress.Xpf.Core.Native;
using DevExpress.Xpf.Editors;
using Microsoft.Win32;

namespace wmsMLC.DCL.Main.Views.Controls
{
    public class CustomImageEdit : ImageEdit
    {
        #region URL
        //WPF UI zoom feature https://www.devexpress.com/Support/Center/Question/Details/Q268770
        //DXTabControl Class https://documentation.devexpress.com/#WPF/clsDevExpressXpfCoreDXTabControltopic
        //LayoutGroup tab change event https://www.devexpress.com/Support/Center/Question/Details/Q302307

        //http://stackoverflow.com/questions/25330802/black-border-around-circle-after-copying-bitmap-to-another-bitmap

        //ImageEdit-MenuCustomization https://www.devexpress.com/Support/Center/Example/Details/E2563
        #endregion URL

        private const double ScaleTransformValue = 1.2;

        public CustomImageEdit()
        {
            CanCutExecute =
                CanCopyExecute =
                    CanPasteExecute =
                        CanLoadExecute =
                            CanDeleteExecute =
                                CanSaveExecute = true;
        }

        public event EventHandler<ImageLoadingEventArgs> ImageLoading;
        public event EventHandler<EventArgs> MenuPopupOpened;

        public bool CanCutExecute { get; set; }
        public bool CanCopyExecute { get; set; }
        public bool CanPasteExecute { get; set; }
        public bool CanLoadExecute { get; set; }
        public bool CanDeleteExecute { get; set; }
        public bool CanSaveExecute { get; set; }

        protected override void LoadCore()
        {
            if (Image != null)
            {
                var imageSource = LoadImage();
                if (imageSource != null)
                    EditStrategy.SetImage(imageSource);
            }
        }

        private ImageSource LoadImage()
        {
            var dialog = new OpenFileDialog
            {
                Filter = EditorLocalizer.GetString(EditorStringId.ImageEdit_OpenFileFilter)
            };

            if (dialog.ShowDialog() == true)
            {
                var e = new ImageLoadingEventArgs(dialog.FileName);
                OnImageLoading(e);
                if (e.IsCanceled)
                    return null;

                using (var stream = dialog.OpenFile())
                {
                    var data = new MemoryStream(stream.GetDataFromStream());
                    return ImageHelper.CreateImageFromStream(data);
                }
            }
            return null;
        }

        protected override void InitMenuPopup()
        {
            base.InitMenuPopup();
            if (MenuPopup != null)
            {
                MenuPopup.LayoutTransform = new ScaleTransform(ScaleTransformValue, ScaleTransformValue);
                MenuPopup.UpdateLayout();
            }
            OnMenuPopupOpened();
        }

        protected override void UpdateMenuPosition()
        {
            if ((MenuPopup != null) && (MenuPopupContentControl != null))
            {
                var actualSize = GetActualSize();
                MenuPopup.HorizontalOffset = (actualSize.Width - MenuPopupContentControl.ActualWidth * ScaleTransformValue) / 2.0;
                MenuPopup.VerticalOffset = (actualSize.Height - MenuPopupContentControl.ActualHeight * ScaleTransformValue) - 12.0;
            }
        }

        //protected override void CopyCore()
        //{
        //    try
        //    {
        //        //var width = Source.Width;
        //        //var height = Source.Height;
        //        //var bmpCopied = new RenderTargetBitmap((int)Math.Round(width), (int)Math.Round(height), 96, 96,
        //        //    PixelFormats.Default);
        //        //var dv = new DrawingVisual();
        //        //using (var dc = dv.RenderOpen())
        //        //{
        //        //    var vb = new VisualBrush(dv);
        //        //    dc.DrawRectangle(vb, null, new Rect(new Point(), new Size(width, height)));
        //        //}
        //        //bmpCopied.Render(dv);
        //        //Clipboard.SetImage(bmpCopied);

        //        //var source = (BitmapSource)Source;
        //        //var visual = new DrawingVisual();
        //        //var context = visual.RenderOpen();
        //        //context.DrawImage(Source, new Rect(0.0, 0.0, source.Width, source.Height));
        //        //context.Close();
        //        ////var bitmap = new RenderTargetBitmap(source.PixelWidth, (int)source.PixelHeight, 96.0, 96.0, PixelFormats.Pbgra32);
        //        //var bitmap = new RenderTargetBitmap(source.PixelWidth, (int)source.PixelHeight, 96.0, 96.0, PixelFormats.Default);
        //        //bitmap.Render(visual);
        //        //Clipboard.SetImage(bitmap);

        //        //var img = new BitmapImage();
        //        //var stream = new MemoryStream((byte[]) EditValue);
        //        //{
        //        //    img.BeginInit();
        //        //    img.StreamSource = stream;
        //        //    img.EndInit();
        //        //}
        //        //Clipboard.SetImage(img);

        //        //Clipboard.SetImage(ImageLoader.GetSafeBitmapSource((BitmapSource) Source, ImageEffect));
        //    }
        //        // ReSharper disable once EmptyGeneralCatchClause
        //    catch { }
        //}

        private void OnImageLoading(ImageLoadingEventArgs e)
        {
            var handler = ImageLoading;
            if (handler != null)
                handler(this, e);
        }

        private void OnMenuPopupOpened()
        {
            var handler = MenuPopupOpened;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        protected override void CanCut(object sender, CanExecuteRoutedEventArgs e)
        {
            base.CanCut(sender, e);
            e.CanExecute = e.CanExecute && CanCutExecute;
        }

        protected override void CanCopy(object sender, CanExecuteRoutedEventArgs e)
        {
            base.CanCopy(sender, e);
            e.CanExecute = e.CanExecute && CanCopyExecute;
        }

        protected override void CanPaste(object sender, CanExecuteRoutedEventArgs e)
        {
            base.CanPaste(sender, e);
            e.CanExecute = e.CanExecute && CanCopyExecute;
        }

        protected override void CanLoad(object sender, CanExecuteRoutedEventArgs e)
        {
            base.CanLoad(sender, e);
            e.CanExecute = e.CanExecute && CanLoadExecute;
        }

        protected override void CanDelete(object sender, CanExecuteRoutedEventArgs e)
        {
            base.CanDelete(sender, e);
            e.CanExecute = e.CanExecute && CanDeleteExecute;
        }

        protected override void CanSave(object sender, CanExecuteRoutedEventArgs e)
        {
            base.CanSave(sender, e);
            e.CanExecute = e.CanExecute && CanSaveExecute;
        }
    }

    public sealed class ImageLoadingEventArgs : EventArgs
    {
        public ImageLoadingEventArgs(string fileName)
        {
            FileName = fileName;
        }

        public string FileName { get; private set; }
        public bool IsCanceled { get; set; }
    }
}
