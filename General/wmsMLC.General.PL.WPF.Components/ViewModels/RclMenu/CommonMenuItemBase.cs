using System.Windows.Input;
using System.Windows.Media;

namespace wmsMLC.General.PL.WPF.Components.ViewModels.RclMenu
{
    public abstract class CommonMenuItemBase : MenuItemBase
    {
        #region Fields&Consts
        public const string GlyphSizePropertyName = "GlyphSize";
        public const string GlyphAlignmentPropertyName = "GlyphAlignment";
        public const string DisplayModePropertyName = "DisplayMode";

        private GlyphSizeType _glyphSize;
        private GlyphAlignmentType _glyphAlignment;
        private DisplayModeType _displayMode;
        #endregion

        #region Properties
        public string Name { get; set; }
        public KeyGesture HotKey { get; set; }

        /// <summary> 32x32 </summary>
        public ImageSource ImageLarge { get; set; }

        /// <summary> 16x16 </summary>
        public ImageSource ImageSmall { get; set; }


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