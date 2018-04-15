namespace wmsMLC.General
{
    /// <summary>
    /// Информация о БД.
    /// </summary>
    public sealed class DbSysInfo
    {
        public DbSysInfo(string version, string environment, string site)
        {
            Version = version;
            Environment = environment;
            Site = site;
        }

        public string Version { get; private set; }
        public string Environment { get; private set; }
        public string Site { get; private set; }
    }
}
