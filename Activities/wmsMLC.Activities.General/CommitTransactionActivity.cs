using System.Activities;

namespace wmsMLC.Activities.General
{
    public class CommitTransactionActivity : NativeActivity
    {
        protected override void Execute(NativeActivityContext context)
        {
            BeginTransactionActivity.CommitChanges(context);
        }
    }
}
