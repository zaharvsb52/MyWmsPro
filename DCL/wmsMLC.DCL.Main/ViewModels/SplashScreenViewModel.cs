using wmsMLC.General;

namespace wmsMLC.DCL.Main.ViewModels
{
    public class SplashScreenViewModel
    {
        public AssemblyAttributeAccessors AttributeAccessors { get; private set; }

        public SplashScreenViewModel()
        {
            AttributeAccessors = new AssemblyAttributeAccessors();
        }
    }
}
