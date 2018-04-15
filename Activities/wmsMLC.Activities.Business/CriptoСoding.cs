using System.Activities;
using System.ComponentModel;
using wmsMLC.General;

namespace wmsMLC.Activities.Business
{
    public class CriptoСoding : NativeActivity
    {
        /// <summary>
        /// Текстовая строка, которую необходимо закодировать.
        /// </summary>
        [DisplayName(@"Исходный текст")]
        public InArgument<string> InStr { get; set; }

        [DisplayName(@"Индекс ключа")]
        public InArgument<int> KeyIndex { get; set; }

        /// <summary>
        /// Возвращаемая текстовая строка.
        /// </summary>
        [DisplayName(@"Возвращаемый текст")]
        public OutArgument<string> OutStr { get; set; }

        /// <summary>
        /// Вариант работы.
        /// </summary>
        [DisplayName(@"Вариант работы")]
        public EnumCoding RunArgument { get; set; }

        public CriptoСoding()
        {
            DisplayName = "Кодирование/Декодирование строки";
        }

        protected override void Execute(NativeActivityContext context)
        {
            var cryptoKeyProvider = IoC.Instance.Resolve<ICryptoKeyProvider>();
            var keyindex = KeyIndex.Get(context);
            var key = cryptoKeyProvider.GetKey(keyindex);
            var str = InStr.Get(context);
            var result = RunArgument == EnumCoding.Encoding ? CryptoHelper.Encrypt(str, key) : CryptoHelper.Decrypt(str, key);
            OutStr.Set(context, result);
        }
    }

    public enum EnumCoding
    {
        Encoding,
        Decoding
    }
}