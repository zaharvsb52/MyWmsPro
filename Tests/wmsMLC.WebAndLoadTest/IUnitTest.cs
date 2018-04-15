using System.Collections;

namespace wmsMLC.WebAndLoadTest
{
    public interface IUnitTest
    {
        void Initialize(IDictionary parameters);
        void Terminate();
        void Run();
    }
}
