namespace wmsMLC.APS.wmsSI.Wrappers
{
    public abstract class BaseWrapper : IBaseWrapper
    {
        private bool? _updateNullValues = false;

        public bool? UpdateNullValues
        {
            get
            {
                return _updateNullValues;
            }
            set
            {
                _updateNullValues = value;
            }
        }
    }
}
