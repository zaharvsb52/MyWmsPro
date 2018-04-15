using System.Activities;
using System.ComponentModel;
using System.Windows;
using wmsMLC.General;
using wmsMLC.General.PL.WPF.Services;

namespace wmsMLC.Activities.RclViewInteraction
{
    public class RclShowMessageBoxActivity : NativeActivity
    {
        [DisplayName(@"Заголовок")]
        public InArgument<string> Title { get; set; }

        [DisplayName(@"Сообщение")]
        public InArgument<string> Message { get; set; }

        [DisplayName(@"Отображаемые кнопки")]
        public MessageBoxButton Buttons { get; set; }

        [DisplayName(@"Изображение")]
        public MessageBoxImage Image { get; set; }

        [DisplayName(@"Результат по умолчанию")]
        public MessageBoxResult DefaultResult { get; set; }

        [DisplayName(@"Размер шрифта")]
        [DefaultValue(14)]
        public InArgument<double?> FontSize { get; set; }

        [DisplayName(@"Результат диалога")]
        public OutArgument<bool> DialogResult { get; set; }

        public RclShowMessageBoxActivity()
        {
            DisplayName = "ТСД: Вывод сообщения";
        }

        protected override void Execute(NativeActivityContext context)
        {
            var viewService = IoC.Instance.Resolve<IViewService>();
            var fontSize = FontSize.Get(context);
            var result = viewService.ShowDialog(Title.Get(context), Message.Get(context), Buttons, Image, DefaultResult, fontSize);
            DialogResult.Set(context, result == MessageBoxResult.OK || result == MessageBoxResult.Yes);
        }
    }
}