using System;
using System.Activities;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using wmsMLC.Activities.General.Helpers;
using wmsMLC.General;

namespace wmsMLC.Activities.Business
{
    public class FormatExceptionMessage : NativeActivity
    {
        /// <summary>
        /// Сообщение
        /// </summary>
        [DisplayName(@"Сообщение")]
        public InArgument<string> Message { get; set; }

        [DisplayName(@"Список ошибок (Exception)")]
        [DefaultValue(null)]
        public InArgument<Collection<Exception>> ErrorList { get; set; }

        [DisplayName(@"Ошибка (Exception)")]
        [DefaultValue(null)]
        public InArgument<Exception> Error { get; set; }

        [DisplayName(@"Форматированное сообщение")]
        public OutArgument<string> FormattedMessage { get; set; }

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            var collection = new Collection<RuntimeArgument>();
            var type = GetType();

            ActivityHelpers.AddCacheMetadata(collection, metadata, FormattedMessage, type.ExtractPropertyName(() => FormattedMessage));
            ActivityHelpers.AddCacheMetadata(collection, metadata, Message, type.ExtractPropertyName(() => Message));
            ActivityHelpers.AddCacheMetadata(collection, metadata, ErrorList, type.ExtractPropertyName(() => ErrorList));
            ActivityHelpers.AddCacheMetadata(collection, metadata, Error, type.ExtractPropertyName(() => Error));

            metadata.SetArgumentsCollection(collection);
        }

        protected override void Execute(NativeActivityContext context)
        {
            var resultErrors = new List<Exception>();
            var error = Error.Get(context);
            var errorList = ErrorList.Get(context);
            if (errorList == null)
                errorList = new Collection<Exception>();
            if (error != null)
                resultErrors.Add(error);
            if (errorList.Count > 0)
                resultErrors.AddRange(errorList);

            var message = Message.Get(context);
            var innerExceptions = GetInnerExceptions(resultErrors).ToArray();


            var errorMessage = string.IsNullOrEmpty(message) ? string.Empty : string.Format("{0}{1}", message, "\r\n");
            foreach (var e in innerExceptions)
            {
                errorMessage += string.Format("{0}{1}", e.Message, "\r\n\r\n");
            }
            FormattedMessage.Set(context, errorMessage);
        }

        private IEnumerable<Exception> GetInnerExceptions(IEnumerable<Exception> ex)
        {
            var innerList = new List<Exception>();
            foreach (var e in ex)
            {
                var inner = GetInnerException(e);
                innerList.Add(inner);
            }
            return innerList;
        }

        private Exception GetInnerException(Exception e)
        {
            if (e.InnerException != null)
                return GetInnerException(e.InnerException);
            return e;
        }
    }
}
