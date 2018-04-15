namespace wmsMLC.General.PL.WPF.Attributes
{
    public class EnableCreateAttribute : BaseEnableAttribute
    {
        public const bool DefaultEnableCreate = true;

        public EnableCreateAttribute(bool enable) : base(enable)
        {
        }
    }

    public class EnableEditAttribute : BaseEnableAttribute
    {
        public const bool DefaultEnableEdit = true;

        public EnableEditAttribute(bool enable) : base(enable)
        {
        }
    }
}