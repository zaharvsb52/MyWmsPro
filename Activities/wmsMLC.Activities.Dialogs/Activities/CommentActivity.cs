using System.Activities;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace wmsMLC.Activities.Dialogs.Activities
{
    public class CommentActivity : NativeActivity
    {
        [Required]
        [DisplayName(@"Текст комментария")]
        public string Comment { get; set; }

        public CommentActivity()
        {
            DisplayName = "Комментарий";
        }

        protected override void Execute(NativeActivityContext context)
        {
        }
    }
}