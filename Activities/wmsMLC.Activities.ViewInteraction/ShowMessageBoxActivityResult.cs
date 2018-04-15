using System.Activities;
using System.ComponentModel;
using System.Windows;
using wmsMLC.General;
using wmsMLC.General.PL.WPF.Services;

namespace wmsMLC.Activities.ViewInteraction
{
    [DisplayNameAttribute(@"Сообщение")]
    public class ShowMessageBoxActivityResult : NativeActivity
    {
        #region . Properties .

        [DisplayName(@"Результат диалога")]
        public OutArgument<bool?> DialogResult { get; set; }

        [DisplayName(@"Заголовок")]
        public InArgument<string> Title { get; set; }

        [DisplayName(@"Сообщение")]
        public InArgument<string> Message { get; set; }

        [DisplayName(@"Кнопки")]
        public MessageBoxButton Buttons { get; set; }

        [DisplayName(@"Картинка")]
        public MessageBoxImage Image { get; set; }

        [DisplayName(@"Значение по умолчанию")]
        public MessageBoxResult DefaultResult { get; set; }

        #endregion

        #region . Methods .

        public ShowMessageBoxActivityResult()
        {
            DisplayName = @"Сообщение";
        }

        protected override void Execute(NativeActivityContext context)
        {
            var viewService = IoC.Instance.Resolve<IViewService>();
            var result = viewService.ShowDialog(Title.Get(context), Message.Get(context), Buttons, Image, DefaultResult);
            bool? mesBoxResult = null;
            if (result != MessageBoxResult.Cancel && result != MessageBoxResult.None)
                mesBoxResult = result == MessageBoxResult.OK || result == MessageBoxResult.Yes;
            DialogResult.Set(context, mesBoxResult);
        }


        #endregion
    }
}