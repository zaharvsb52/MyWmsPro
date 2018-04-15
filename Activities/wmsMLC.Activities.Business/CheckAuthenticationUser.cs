using System;
using System.Activities;
using System.ComponentModel;
using wmsMLC.Business.Managers;
using wmsMLC.General.BL.Validation.Attributes;
using wmsMLC.General;

namespace wmsMLC.Activities.Business
{
    public class CheckAuthenticationUser : NativeActivity
    {
        #region . Arguments .

        /// <summary>
        /// Логин пользователя
        /// </summary>
        [Required]
        [DisplayName(@"Логин пользователя")]
        public InArgument<string> Login { get; set; }

        /// <summary>
        /// Пароль
        /// </summary>
        [Required]
        [DisplayName(@"Пароль")]
        public InArgument<string> Password { get; set; }

        [DisplayName(@"Ошибка")]
        public OutArgument<Exception> Exception { get; set; }

        /// <summary>
        /// Пользователь
        /// </summary>
        [DisplayName(@"Пользователь")]
        public OutArgument<string> User { get; set; }

        #endregion

        public CheckAuthenticationUser()
        {
            DisplayName = "Проверка аутентификации пользователя";
        }

        protected override void Execute(NativeActivityContext context)
        {
            var result = Check(context);
            User.Set(context, result);
        }

        private string Check(NativeActivityContext context)
        {
            try
            {
                var login = Login.Get(context);
                var password = Password.Get(context);

                var mgr = IoC.Instance.Resolve<IUserManager>();
                return mgr.Authenticate(login, password);
            }
            catch (Exception ex)
            {
                Exception.Set(context, ex);
                return string.Empty;
            }
        }
    }
}