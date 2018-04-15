using System;

namespace wmsMLC.General.DAL
{
    public sealed class UnitOfWorkContext
    {
        public int? SessionId { get; set; }

        public string UserSignature { get; set; }
        public string ConfigurationString { get; set; }
        public Guid Id { get; set; }
        public int? TimeOut { get; set; }        
    }
}