using System;
using wmsMLC.Business.Managers;
using wmsMLC.Business.Objects;
using wmsMLC.General;

namespace wmsMLC.Business
{
    public class RegisterClass
    {
        public RegisterClass()
        {
            ManagerLifeTime = LifeTime.None;
            OracleRepositoryLifeTime = LifeTime.None;
            ServiceRepositoryLifeTime = LifeTime.None;
        }

        public Type ObjectType;
        public Type KeyType;

        public Type ManagerType;
        public Type OracleManagerType;

        public Type OracleRepositoryType;
        public Type ServiceRepositoryType;

        public LifeTime ManagerLifeTime;
        public LifeTime OracleRepositoryLifeTime;
        public LifeTime ServiceRepositoryLifeTime;
    }

    /// <summary>
    /// Класс для регистрации объектов в системе. По-умолчанию для объекта создается кэшируемые репозитории и включается функционал получения истории
    /// </summary>
    /// <typeparam name="TObj">Тип объекта</typeparam>
    /// <typeparam name="TKey">Тип первичного ключа объекта</typeparam>
    public class StandardRegisterClass<TObj, TKey> : RegisterClass
        where TObj : WMSBusinessObject, new()
    {
        public StandardRegisterClass(bool haveDatabaseCache = true,bool haveClientCache = true, bool haveHistory = true)
        {
            ObjectType = typeof(TObj);
            KeyType = typeof(TKey);
            ManagerType = typeof(WMSBusinessObjectManager<TObj, TKey>);

            var msk = (haveDatabaseCache ? "1" : "0") + (haveClientCache ? "1" : "0") + (haveHistory ? "1" : "0");
            switch (msk)
            {
                case "000":
                    {
                        OracleRepositoryType = typeof(wmsMLC.General.DAL.Oracle.Repository<TObj, TKey>);
                        ServiceRepositoryType = typeof(wmsMLC.General.DAL.Service.BaseRepository<TObj, TKey>);
                        break;
                    }
                case "001":
                    {
                        OracleRepositoryType = typeof(wmsMLC.Business.DAL.Oracle.BaseHistoryRepository<TObj, TKey>);
                        ServiceRepositoryType = typeof(wmsMLC.Business.DAL.Service.BaseHistoryRepository<TObj, TKey>);
                        break;
                    }
                case "010":
                    {
                        OracleRepositoryType = typeof(wmsMLC.General.DAL.Oracle.Repository<TObj, TKey>);
                        ServiceRepositoryType = typeof(wmsMLC.General.DAL.Service.CacheableRepository<TObj, TKey>);
                        break;
                    }
                case "011":
                    {
                        OracleRepositoryType = typeof(wmsMLC.Business.DAL.Oracle.BaseHistoryRepository<TObj, TKey>);
                        ServiceRepositoryType = typeof(wmsMLC.Business.DAL.Service.BaseHistoryCacheableRepository<TObj, TKey>);
                        break;
                    }
                case "100":
                    {
                        OracleRepositoryType = typeof(wmsMLC.General.DAL.Oracle.CacheableRepository<TObj, TKey>);
                        ServiceRepositoryType = typeof(wmsMLC.General.DAL.Service.BaseRepository<TObj, TKey>);
                        break;
                    }
                case "101":
                    {
                        OracleRepositoryType = typeof(wmsMLC.Business.DAL.Oracle.BaseHistoryCacheableRepository<TObj, TKey>);
                        ServiceRepositoryType = typeof(wmsMLC.Business.DAL.Service.BaseHistoryRepository<TObj, TKey>);
                        break;
                    }
                case "110":
                    {
                        OracleRepositoryType = typeof(wmsMLC.General.DAL.Oracle.CacheableRepository<TObj, TKey>);
                        ServiceRepositoryType = typeof(wmsMLC.General.DAL.Service.CacheableRepository<TObj, TKey>);
                        break;
                    }
                case "111":
                    {
                        OracleRepositoryType = typeof (wmsMLC.Business.DAL.Oracle.BaseHistoryCacheableRepository<TObj, TKey>);
                        ServiceRepositoryType = typeof (wmsMLC.Business.DAL.Service.BaseHistoryCacheableRepository<TObj, TKey>);
                        break;
                    }
            }
        }
    }
}