namespace wmsMLC.General.PL.WPF.Attributes
{
    public class ListVisibleAttribute : BaseVisibleAttribute
    {
        public ListVisibleAttribute(bool allowView) : base(allowView) { }
    }

    public class DetailVisibleAttribute : BaseVisibleAttribute
    {
        public DetailVisibleAttribute(bool allowView) : base(allowView) { }
    }
    public class FilterVisibleAttribute : BaseVisibleAttribute
    {
        public FilterVisibleAttribute(bool allowView) : base(allowView) { }
    }

    public class LookUpVisibleAttribute : BaseVisibleAttribute
    {
        public LookUpVisibleAttribute(bool allowView) : base(allowView) { }
    }

    public class SubListVisibleAttribute : BaseVisibleAttribute
    {
        public SubListVisibleAttribute(bool allowView) : base(allowView) { }
    }

    public class SubDetailVisibleAttribute : BaseVisibleAttribute
    {
        public SubDetailVisibleAttribute(bool allowView) : base(allowView) { }
    }

    public class ListMemoVisibleAttribute : BaseVisibleAttribute
    {
        public ListMemoVisibleAttribute(bool allowView) : base(allowView) { }
    }
    public class DetailMemoVisibleAttribute : BaseVisibleAttribute
    {
        public DetailMemoVisibleAttribute(bool allowView) : base(allowView) { }
    }
}