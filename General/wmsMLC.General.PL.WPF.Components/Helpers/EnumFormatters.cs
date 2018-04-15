using System.Windows.Input;

namespace wmsMLC.General.PL.WPF.Components.Helpers
{
    public static class EnumFormatters
    {
        public static string Format(Key key)
        {
            switch (key)
            {
                case Key.Escape:
                    return "Esc:";
                case Key.Enter:
                    return "Ent:";

                default:
                    var numkey = KeyHelper.GetNumKey(key);
                    return numkey.HasValue ? string.Format("{0}:", numkey) : string.Format("{0}:", key);
            }
        }
    }
}
