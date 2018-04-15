using System.Diagnostics.Contracts;

namespace wmsMLC.General
{
    [ContractClass(typeof(IAuthServiceContract))]
    public interface IAuthService
    {
        bool Authenticate(string login, string password, out string userCode);
        void LogOff();
    }

    [ContractClassFor(typeof(IAuthService))]
    abstract class IAuthServiceContract : IAuthService
    {
        bool IAuthService.Authenticate(string login, string password, out string userCode)
        {
            Contract.Requires(login != null);

            throw new System.NotImplementedException();
        }

        public void LogOff()
        {
            throw new System.NotImplementedException();
        }
    }
}