namespace wmsMLC.General.DAL
{
    public static class ParameterHelper
    {
        public static string Escape(string val)
        {
            return string.IsNullOrEmpty(val) ? val : "'" + val.Replace("'", "''") + "'";
        }
    }
}