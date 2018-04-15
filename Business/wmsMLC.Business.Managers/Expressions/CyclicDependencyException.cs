using wmsMLC.General;

namespace wmsMLC.Business.Managers.Expressions
{
    public class CyclicDependencyException : OperationException
    {
        public const string DefaultMessage = "Обнаружена циклическая связь при вычислении формул";

        public CyclicDependencyException() : base(DefaultMessage) { }
    }
}