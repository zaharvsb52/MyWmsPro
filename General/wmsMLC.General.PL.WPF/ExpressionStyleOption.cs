using System;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using System.Xml;
using System.Xml.Serialization;
using wmsMLC.General.PL.WPF.Attributes;

namespace wmsMLC.General.PL.WPF
{
    public class ExpressionStyleOption : INotifyPropertyChanged, IXmlSerializable
    {
        public const string ApplyToRowPropertyName = "ApplyToRow";
        public const string FieldNamePropertyName = "FieldName";
        public const string ExpressionStringPropertyName = "ExpressionString";
        public const string NamePropertyName = "Name";
        public const string ThresholdPropertyName = "Threshold";
        public const string FormatConditionTypePropertyName = "FormatConditionType";
        public const string PredefinedFormatNamePropertyName = "PredefinedFormatName";
        
        private readonly CultureInfo _culture;

        public ExpressionStyleOption()
        {
            _culture = CultureInfo.InvariantCulture;
            FormatConditionType = FormatConditionType.Default;
        }

        private FormatConditionType _formatConditionType;

        [XtraSerializableProperty]
        public FormatConditionType FormatConditionType
        {
            get { return _formatConditionType; }
            set
            {
                if (_formatConditionType == value)
                    return;
                _formatConditionType = value;
                OnPropertyChanged("FormatConditionTypePropertyName");
            }
        }

        private Brush _background;
        [XtraSerializableProperty]
        public Brush Background
        {
            get { return _background; }
            set
            {
                if (Equals(_background, value)) 
                    return;
                _background = value;
                OnPropertyChanged("Background");
            }
        }

        private Brush _foreground;
        [XtraSerializableProperty]
        public Brush Foreground
        {
            get { return _foreground; }
            set
            {
                if (Equals(_foreground, value)) 
                    return;
                _foreground = value;
                OnPropertyChanged("Foreground");
            }
        }

        private FontStyle? _fontStyle;
        [XtraSerializableProperty]
        public FontStyle? FontStyle
        {
            get { return _fontStyle; }
            set
            {
                if (_fontStyle == value) 
                    return;
                _fontStyle = value;
                OnPropertyChanged("FontStyle");
            }
        }

        private FontFamily _fontFamily;
        [XtraSerializableProperty]
        public FontFamily FontFamily
        {
            get { return _fontFamily; }
            set
            {
                if (Equals(_fontFamily, value)) 
                    return;
                _fontFamily = value;
                OnPropertyChanged("FontFamily");
            }
        }

        private int _fontSize;
        [XtraSerializableProperty]
        public int FontSize
        {
            get { return _fontSize; }
            set
            {
                if (_fontSize == value) 
                    return;
                _fontSize = value;
                OnPropertyChanged("FontSize");
            }
        }

        private FontWeight? _fontWeight;
        [XtraSerializableProperty]
        public FontWeight? FontWeight
        {
            get { return _fontWeight; }
            set
            {
                if (_fontWeight == value)
                    return;
                _fontWeight = value;
                OnPropertyChanged("FontWeight");
            }
        }

        private string _fieldName;
        [XtraSerializableProperty]
        public string FieldName
        {
            get { return _fieldName; }
            set
            {
                if (_fieldName != value)
                {
                    _fieldName = value;
                    OnPropertyChanged(FieldNamePropertyName);
                }
            }
        }

        private string _expressionString;
        [XtraSerializableProperty]
        public string ExpressionString
        {
            get { return _expressionString; }
            set
            {
                if (_expressionString != value)
                {
                    _expressionString = value;
                    OnPropertyChanged(ExpressionStringPropertyName);
                }
            }
        }

        private bool _applyToRow;
        [XtraSerializableProperty]
        public bool ApplyToRow
        {
            get { return _applyToRow; }
            set
            {
                if (_applyToRow != value)
                {
                    _applyToRow = value;
                    OnPropertyChanged(ApplyToRowPropertyName);
                }
            }
        }

        private string _name;
        [XtraSerializableProperty]
        public string Name
        {
            get { return _name; }
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged(NamePropertyName);
                }
            }
        }

        private string _predefinedFormatName;
        [XtraSerializableProperty]
        public string PredefinedFormatName
        {
            get { return _predefinedFormatName; }
            set
            {
                if (_predefinedFormatName == value)
                    return;
                _predefinedFormatName = value;
                OnPropertyChanged(PredefinedFormatName);
            }
        }

        private double _threshold;
        /// <summary>
        /// Используется в типе TopBottomRuleFormatCondition.
        /// </summary>
        [XtraSerializableProperty]
        public double Threshold
        {
            get { return _threshold; }
            set
            {
                if (_threshold == value)
                    return;
                _threshold = value;
                OnPropertyChanged(ThresholdPropertyName);
            }
        }

        private string _comment;
        [XtraSerializableProperty]
        public string Comment
        {
            get { return _comment; }
            set
            {
                if (_comment != value)
                {
                    _comment = value;
                    OnPropertyChanged("Comment");
                }
            }
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion INotifyPropertyChanged

        #region IXmlSerializable
        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            if (reader.MoveToContent() == XmlNodeType.Element && reader.LocalName == GetType().Name)
            {
                foreach (PropertyDescriptor p in PropertyDescriptorCollection)
                {
                    var text = reader[p.Name];
                    if (string.IsNullOrEmpty(text))
                        continue;
                    if (p.PropertyType == typeof(Brush))
                    {
                        var convertFromString = ColorConverter.ConvertFromString(text);
                        if (convertFromString == null)
                            continue;
                        var value = new SolidColorBrush((Color)convertFromString);
                        p.SetValue(this, value);
                    }
                    else if (p.PropertyType == typeof(FontStyle?))
                    {
                        var value = FindFontStyle(text);
                        if (value == null)
                            continue;
                        p.SetValue(this, value);
                    }
                    else if (p.PropertyType == typeof(FontFamily))
                    {
                        try
                        {
                            var value = new FontFamily(text);
                            p.SetValue(this, value);
                        }
                        catch
                        {
                            continue;
                        }
                    }
                    else if (p.PropertyType == typeof(int))
                    {
                        int value;
                        if (int.TryParse(text, out value))
                        {
                            p.SetValue(this, value);
                        }
                    }
                    else if (p.PropertyType == typeof(bool))
                    {
                        bool value;
                        if (bool.TryParse(text, out value))
                        {
                            p.SetValue(this, value);
                        }
                    }
                    else if (p.PropertyType == typeof(double))
                    {
                        double value;
                        if (double.TryParse(text, NumberStyles.None, _culture,  out value))
                        {
                            p.SetValue(this, value);
                        }
                    }
                    else if (p.PropertyType == typeof(float))
                    {
                        float value;
                        if (float.TryParse(text, NumberStyles.None, _culture, out value))
                        {
                            p.SetValue(this, value);
                        }
                    }
                    else if (p.PropertyType == typeof(decimal))
                    {
                        decimal value;
                        if (decimal.TryParse(text, NumberStyles.None, _culture, out value))
                        {
                            p.SetValue(this, value);
                        }
                    }
                    else if (p.PropertyType == typeof(FontWeight?))
                    {
                        var value = FindFontWeight(text);
                        if (value == null)
                            continue;
                        p.SetValue(this, value);
                    }
                    else if (p.PropertyType == typeof (FormatConditionType))
                    {
                        p.SetValue(this, text.To(FormatConditionType.Default));
                    }
                    else
                    {
                        try
                        {
                            p.SetValue(this, text);
                        }
                        catch(Exception ex)
                        {
                            throw new DeveloperException(string.Format("Type '{0}' is not defined for deserialize ExpressionStyleOption.", p.PropertyType), ex);
                        }
                    }
                }

                reader.Read();
            }
        }

        public void WriteXml(XmlWriter writer)
        {
            foreach (PropertyDescriptor p in PropertyDescriptorCollection)
            {
                if (p.Attributes[typeof(XtraSerializablePropertyAttribute)] == null)
                    continue;
                var value = p.GetValue(this);

                var text = string.Empty;
                if (value != null)
                {
                    if (value is double)
                        text = ((double) value).ToString(_culture);
                    else if (value is float)
                        text = ((float) value).ToString(_culture);
                    else if (value is decimal)
                        text = ((decimal) value).ToString(_culture);
                    else
                        text = value.ToString();
                }

                writer.WriteAttributeString(p.Name, text);
            }
        }

        private PropertyDescriptorCollection _propertyDescriptorCollection;
        private PropertyDescriptorCollection PropertyDescriptorCollection
        {
            get
            {
                return _propertyDescriptorCollection ?? (_propertyDescriptorCollection = TypeDescriptor.GetProperties(GetType()));
            }
        }

        private FontStyle? FindFontStyle(string name)
        {
            if (string.IsNullOrEmpty(name))
                return null;
            var propertyInfo = typeof(FontStyles).GetProperty(name, BindingFlags.Static | BindingFlags.Public | BindingFlags.IgnoreCase);
            if (propertyInfo == null)
                return null;
            return (FontStyle)propertyInfo.GetValue(null, null);
        }

        private FontWeight? FindFontWeight(string name)
        {
            if (string.IsNullOrEmpty(name))
                return null;
            var propertyInfo = typeof(FontWeights).GetProperty(name, BindingFlags.Static | BindingFlags.Public | BindingFlags.IgnoreCase);
            if (propertyInfo == null)
                return null;
            return (FontWeight)propertyInfo.GetValue(null, null);
        }
        #endregion IXmlSerializable
    }

    public enum FormatConditionType
    {
        Default,
        TopItemsRule,
        TopPersentRule,
        BottomItemsRule,
        BottomPercentRule,
        AboveAverageRule,
        BelowAverageRule,
        DataBar,
        ColorScale,
        IconSet
    }
}
