using System.Activities;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Mail;
using wmsMLC.Activities.General.Helpers;
using wmsMLC.General;

namespace wmsMLC.Activities.General
{
    public class SendMail : NativeActivity
    {
        [DisplayName(@"Почтовый сервер")]
        public InArgument<string> Host { get; set; }

        [DisplayName(@"Порт почтового сервера")]
        [DefaultValue(25)]
        public InArgument<int> Port { get; set; }

        [DisplayName(@"От кого")]
        public InArgument<string> From { get; set; }

        [DisplayName(@"Кому")]
        public InArgument<string> Recipients { get; set; }

        [DisplayName(@"Тема")]
        public InArgument<string> Subject { get; set; }

        [DisplayName(@"Сообщение")]
        public InArgument<string> Body { get; set; }

        [DisplayName(@"Отправлять сообщения в формате HTML")]
        public InArgument<bool> IsBodyHtml { get; set; }

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            var collection = new Collection<RuntimeArgument>();
            var type = GetType();

            ActivityHelpers.AddCacheMetadata(collection, metadata, Host, type.ExtractPropertyName(() => Host));
            ActivityHelpers.AddCacheMetadata(collection, metadata, Port, type.ExtractPropertyName(() => Port));
            ActivityHelpers.AddCacheMetadata(collection, metadata, From, type.ExtractPropertyName(() => From));
            ActivityHelpers.AddCacheMetadata(collection, metadata, Recipients, type.ExtractPropertyName(() => Recipients));
            ActivityHelpers.AddCacheMetadata(collection, metadata, Subject, type.ExtractPropertyName(() => Subject));
            ActivityHelpers.AddCacheMetadata(collection, metadata, Body, type.ExtractPropertyName(() => Body));
            ActivityHelpers.AddCacheMetadata(collection, metadata, IsBodyHtml, type.ExtractPropertyName(() => IsBodyHtml));

            metadata.SetArgumentsCollection(collection);
        }

        protected override void Execute(NativeActivityContext context)
        {
            var client = new SmtpClient(Host.Get(context), Port.Get(context));
            var message = new MailMessage(From.Get(context), Recipients.Get(context), Subject.Get(context), Body.Get(context))
                {
                    IsBodyHtml = IsBodyHtml.Get(context)
                };
            client.Send(message);
        }
    }
}
