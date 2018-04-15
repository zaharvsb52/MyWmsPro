using wmsMLC.General.BL;

namespace wmsMLC.Business.Objects
{
    public class OutputBatch : WMSBusinessObject
    {
        public const string BatchPropertyName = "Batch";

        [HardCodedProperty]
        public WMSBusinessCollection<Output> Batch
        {
            get { return GetProperty<WMSBusinessCollection<Output>>(BatchPropertyName); }
            set { SetProperty(BatchPropertyName, value); }
        }
    }
}