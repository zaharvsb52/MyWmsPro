using System.Activities;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace wmsMLC.Activities.Dialogs.Activities
{
    public class CommentActivity : NativeActivity
    {
        [Required]
        [DisplayName(@"����� �����������")]
        public string Comment { get; set; }

        public CommentActivity()
        {
            DisplayName = "�����������";
        }

        protected override void Execute(NativeActivityContext context)
        {
        }
    }
}