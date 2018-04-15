using System;
using System.Activities;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using wmsMLC.Activities.General.Helpers;
using wmsMLC.General;
using wmsMLC.General.PL.WPF.Services;

namespace wmsMLC.Activities.Dialogs.Activities
{
    [DisplayName(@"Отобразить ошибки")]
    public class ShowExceptionActivity : NativeActivity<MessageBoxResult>
    {
        public ShowExceptionActivity()
        {
            DisplayName = "Вывод сообщения об ошибке";
        }

        #region .  Properties  .

        /// <summary>
        /// Заголовок.
        /// </summary>
        [DisplayName(@"Заголовок")]
        public InArgument<string> Title { get; set; }

        /// <summary>
        /// Сообщение.
        /// </summary>
        [DisplayName(@"Сообщение")]
        public InArgument<string> Message { get; set; }

        /// <summary>
        /// Отображаемые кнопки.
        /// </summary>
        [DisplayName(@"Кнопки диалога")]
        public MessageBoxButton Buttons { get; set; }

        /// <summary>
        /// Изображение.
        /// </summary>
        [DisplayName(@"Значок диалога")]
        public MessageBoxImage Image { get; set; }

        /// <summary>
        /// Кнопка по умолчанию.
        /// </summary>
        [DisplayName(@"Кнопка по умолчанию")]
        public MessageBoxResult DefaultResult { get; set; }

        [DisplayName(@"Список ошибок (Exception)")]
        public InArgument<Collection<Exception>> ErrorList { get; set; }

        [DisplayName(@"Ошибка (Exception)")]
        public InArgument<Exception> Error { get; set; }

        [DisplayName(@"Показать как ошибку")]
        public bool ForceError { get; set; }
        #endregion .  Properties  .

        #region .  Methods  .
        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            var collection = new Collection<RuntimeArgument>();
            var type = GetType();

            ActivityHelpers.AddCacheMetadata(collection, metadata, Title, type.ExtractPropertyName(() => Title));
            ActivityHelpers.AddCacheMetadata(collection, metadata, Message, type.ExtractPropertyName(() => Message));
            ActivityHelpers.AddCacheMetadata(collection, metadata, ErrorList, type.ExtractPropertyName(() => ErrorList));
            ActivityHelpers.AddCacheMetadata(collection, metadata, Error, type.ExtractPropertyName(() => Error));

            metadata.SetArgumentsCollection(collection);
        }

        protected override void Execute(NativeActivityContext context)
        {
            var resultErrors = new List<Exception>();

            // могли передать одну ошибку
            var error = Error.Get(context);
            if (error != null)
                resultErrors.Add(error);

            // а могли список
            var errorList = ErrorList.Get(context);
            if (errorList != null && errorList.Count > 0)
                resultErrors.AddRange(errorList);

            var title = Title.Get(context);
            var message = Message.Get(context);

            // собираем ошибку
            var sbMessage = new StringBuilder(message);
            sbMessage.AppendLine();

            foreach (var e in resultErrors)
            {
                // пишем только уникальные ошибки
                //var errMessage = ExceptionHelper.GetErrorMessage(e);
                var trueEx = ExceptionHelper.GetFirstMeaningException(e);
                var errMessage = ExceptionHelper.GetErrorMessage(trueEx);
                if (sbMessage.Length == 0 || !sbMessage.ToString().Contains(errMessage))
                    sbMessage.AppendLine(errMessage);
            }

            var totalErrorMessage = sbMessage.ToString();

            var vs = IoC.Instance.Resolve<IViewService>();
            if (resultErrors.Any(i => i is ICustomException || i.InnerException is ICustomException) && !ForceError)
            {
                //var dialogMessage = string.Format("{0}{1}{1}{2}", totalErrorMessage, Environment.NewLine, sbText);
                var result = vs.ShowDialog(title, totalErrorMessage, Buttons, Image, DefaultResult);
                Result.Set(context, result);
            }
            else
            {
                // выбираем ошибку
                var ex = error ?? (errorList == null ? null : errorList.FirstOrDefault());
                if (ex == null)
                    ex = new OperationException(totalErrorMessage);
                vs.ShowError(message, ex);
            }
        }

        #endregion .  Methods  .
    }
}