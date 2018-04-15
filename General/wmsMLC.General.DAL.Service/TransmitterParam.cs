using System;

namespace wmsMLC.General.DAL.Service
{
    public class TransmitterParam
    {
        public string Name { get; set; }
        public Type Type { get; set; }
        public Object Value { get; set; }
        public bool IsOut { get; set; }
    }
}