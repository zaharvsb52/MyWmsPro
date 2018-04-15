using System;
using System.IdentityModel.Selectors;
using System.Net;
using System.ServiceModel.Web;

namespace wmsMLC.General
{
    public class CustomValidator : UserNamePasswordValidator
    {
        public override void Validate(string userName, string password)
        {
            try
            {
                if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
                    throw new ArgumentNullException();

                var auth = IoC.Instance.Resolve<IAuthenticationProvider>();
                auth.Authenticate(userName, password);
            }
            catch (Exception ex)
            {
                //TODO: разобраться как передать сообщение ошибки
                var fault = new WebFaultException<Exception>(ex, HttpStatusCode.Unauthorized);
                // HACK: недокументированный прием передать код статуса
                fault.Data["HttpStatusCode"] = HttpStatusCode.Unauthorized;
                throw fault;
            }
        }
    }
}
