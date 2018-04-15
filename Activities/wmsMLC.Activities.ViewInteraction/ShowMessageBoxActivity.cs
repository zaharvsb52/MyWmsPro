using System.Activities;
using System.Windows;
using wmsMLC.General;
using wmsMLC.General.PL.WPF.Services;

namespace wmsMLC.Activities.ViewInteraction
{
    public class ShowMessageBoxActivity : NativeActivity
    {
        /// <summary>
        /// Заголовок
        /// </summary>
        public InArgument<string> Title { get; set; }

        /// <summary>
        /// Сообщение
        /// </summary>
        public InArgument<string> Message { get; set; }

        /// <summary>
        /// Отображаемые кнопки
        /// </summary>
        public MessageBoxButton Buttons { get; set; }

        /// <summary>
        /// Изображение
        /// </summary>
        public MessageBoxImage Image { get; set; }

        /// <summary>
        /// Результат по умолчанию
        /// </summary>
        public MessageBoxResult DefaultResult { get; set; }
        
        /// <summary>
        /// Выходной результат
        /// </summary>
        public MessageBoxResult Result { get; set; }

        protected override void Execute(NativeActivityContext context)
        {
            var viewService = IoC.Instance.Resolve<IViewService>();
            Result = viewService.ShowDialog(Title.Get(context), Message.Get(context), Buttons, Image, DefaultResult);
        }
    }
}