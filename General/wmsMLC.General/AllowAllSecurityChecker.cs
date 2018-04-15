namespace wmsMLC.General
{
    public class AllowAllSecurityChecker : ISecurityChecker
    {
        public bool Check(string actionName)
        {
            return true;
        }

        public bool Check(string actionName, string userName)
        {
            return true;
        }
    }
}