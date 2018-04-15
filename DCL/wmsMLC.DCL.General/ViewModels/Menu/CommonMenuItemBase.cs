using System.Windows.Input;
using System.Windows.Media;

namespace wmsMLC.DCL.General.ViewModels.Menu
{
    public abstract class CommonMenuItemBase : MenuItemBase
    {
        #region Fields&Consts
        public const string GlyphSizePropertyName = "GlyphSize";
        public const string GlyphAlignmentPropertyName = "GlyphAlignment";
        public const string DisplayModePropertyName = "DisplayMode";
        public const string KeywordPropertyName = "Keyword";
        public const string ImageSmallPropertyName = "ImageSmall";
        public const string ImageLargePropertyName = "ImageLarge";

        private GlyphSizeType _glyphSize;
        private GlyphAlignmentType _glyphAlignment;
        private DisplayModeType _displayMode;
        private string _keyword;
        #endregion

        protected CommonMenuItemBase()
        {
            GlyphSize = GlyphSizeType.Default;
        }

        #region Properties
        //public string Name { get; set; }
        public KeyGesture HotKey { get; set; }

        private ImageSource _imageLarge;

        /// <summary> 32x32 </summary>
        public ImageSource ImageLarge
        {
            get { return _imageLarge; }
            set
            {
                _imageLarge = value;
                OnPropertyChanged(ImageLargePropertyName);
            }
        }

        private ImageSource _imageSmall;
        /// <summary> 16x16 </summary>
        public ImageSource ImageSmall {
            get { return _imageSmall; }
            set
            {
                _imageSmall = value;
                OnPropertyChanged(ImageSmallPropertyName);
            }
        }


        public GlyphSizeType GlyphSize
        {
            get { return _glyphSize; }
            set
            {
                if (_glyphSize == value)
                    return;

                _glyphSize = value;
                OnPropertyChanged(GlyphSizePropertyName);

            }
        }
        public GlyphAlignmentType GlyphAlignment
        {
            get { return _glyphAlignment; }
            set
            {
                if (_glyphAlignment == value)
                    return;

                _glyphAlignment = value;
                OnPropertyChanged(GlyphAlignmentPropertyName);
            }
        }
        public DisplayModeType DisplayMode
        {
            get { return _displayMode; }
            set
            {
                if (_displayMode == value)
                    return;

                _displayMode = value;
                OnPropertyChanged(DisplayModePropertyName);

            }
        }
        public string Keyword
        {
            get { return _keyword; }
            set
            {
                if (_keyword == value)
                    return;

                _keyword = value;
                OnPropertyChanged(KeywordPropertyName);

            }
        }
        #endregion
    }

    public enum GlyphAlignmentType
    {
        Bottom = 0,
        Left = 1,
        Right = 2,
        Top = 3,
    }

    public enum DisplayModeType
    {
        Default = 0,
        Content = 1,
        ContentAndGlyph = 2,
    }
}