namespace wmsMLC.General.PL.WPF.Attributes
{
    public class DefaultDisplayFormatAttribute : BaseFormatAttribute
    {
        public DefaultDisplayFormatAttribute(string displayFormat) : base(displayFormat) { }
    }

    public class ListDisplayFormatAttribute : BaseFormatAttribute
    {
        public ListDisplayFormatAttribute(string displayFormat) : base(displayFormat) { }
    }

    public class DetailDisplayFormatAttribute : BaseFormatAttribute
    {
        public DetailDisplayFormatAttribute(string displayFormat) : base(displayFormat) { }
    }

    public class LookUpDisplayFormatAttribute : BaseFormatAttribute
    {
        public LookUpDisplayFormatAttribute(string displayFormat) : base(displayFormat) { }
    }

    public class SubListDisplayFormatAttribute : BaseFormatAttribute
    {
        public SubListDisplayFormatAttribute(string displayFormat) : base(displayFormat) { }
    }

    public class SubDetailDisplayFormatAttribute : BaseFormatAttribute
    {
        public SubDetailDisplayFormatAttribute(string displayFormat) : base(displayFormat) { }
    }
}