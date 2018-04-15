using wmsMLC.General;

namespace wmsMLC.RCL.Main.ViewModels
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
