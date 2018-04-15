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
    [DisplayName(@"���������� ������")]
    public class ShowExceptionActivity : NativeActivity<MessageBoxResult>
    {
        public ShowExceptionActivity()
        {
            DisplayName = "����� ��������� �� ������";
        }

        #region .  Properties  .

        /// <summary>
        /// ���������.
        /// </summary>
        [DisplayName(@"���������")]
        public InArgument<string> Title { get; set; }

        /// <summary>
        /// ���������.
        /// </summary>
        [DisplayName(@"���������")]
        public InArgument<string> Message { get; set; }

        /// <summary>
        /// ������������ ������.
        /// </summary>
        [DisplayName(@"������ �������")]
        public MessageBoxButton Buttons { get; set; }

        /// <summary>
        /// �����������.
        /// </summary>
        [DisplayName(@"������ �������")]
        public MessageBoxImage Image { get; set; }

        /// <summary>
        /// ������ �� ���������.
        /// </summary>
        [DisplayName(@"������ �� ���������")]
        public MessageBoxResult DefaultResult { get; set; }

        [DisplayName(@"������ ������ (Exception)")]
        public InArgument<Collection<Exception>> ErrorList { get; set; }

        [DisplayName(@"������ (Exception)")]
        public InArgument<Exception> Error { get; set; }

        [DisplayName(@"�������� ��� ������")]
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

            // ����� �������� ���� ������
            var error = Error.Get(context);
            if (error != null)
                resultErrors.Add(error);

            // � ����� ������
            var errorList = ErrorList.Get(context);
            if (errorList != null && errorList.Count > 0)
                resultErrors.AddRange(errorList);

            var title = Title.Get(context);
            var message = Message.Get(context);

            // �������� ������
            var sbMessage = new StringBuilder(message);
            sbMessage.AppendLine();

            foreach (var e in resultErrors)
            {
                // ����� ������ ���������� ������
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
                // �������� ������
                var ex = error ?? (errorList == null ? null : errorList.FirstOrDefault());
                if (ex == null)
                    ex = new OperationException(totalErrorMessage);
                vs.ShowError(message, ex);
            }
        }

        #endregion .  Methods  .
    }
}