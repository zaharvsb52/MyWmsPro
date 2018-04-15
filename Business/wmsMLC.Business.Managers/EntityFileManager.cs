using System;
using System.Collections.Generic;
using System.Linq;
using wmsMLC.Business.DAL;
using wmsMLC.Business.Objects;
using wmsMLC.General;
using wmsMLC.General.BL;

namespace wmsMLC.Business.Managers
{
    public class EntityFileManager : WMSBusinessObjectManager<EntityFile, decimal>, IEntityFileManager
    {
        public string GetFileBodyByEntity(string pEntity, string pKey)
        {
            using (var repo = GetRepository<IEntityFileRepository>())
                return repo.GetFileBodyByEntity(pEntity, pKey);
        }

        public void SetFileBody(string pEntity, string pKey, string pBody)
        {
            using (var repo = GetRepository<IEntityFileRepository>())
                repo.SetFileBody(pEntity, pKey, pBody);
        }

        public IEnumerable<EntityFile> GetFileHeaders(string entity)
        {
            var filter = string.Format("({0} = '{1}')", EntityFile.File2EntityPropertyName.ToUpper(), entity);
            return GetFiltered(filter);
        }

        public string GetFileData(decimal pKey)
        {
            using (var repo = GetRepository<IEntityFileRepository>())
                return repo.GetFileData(pKey);
        }

        public void SetFileData(decimal pKey, string data)
        {
            using (var repo = GetRepository<IEntityFileRepository>())
                repo.SetFileData(pKey, data);
        }
    }

    public class EntityLinkManager : WMSBusinessObjectManager<EntityLink, string>, IEntityLinkManager
    {
        public IEnumerable<EntityLink> GetByEntityType(Type type)
        {
            //TODO: имя класса может отличаться от имени объекта в БД
            return GetAll().Where(i => type.Name.EqIgnoreCase(i.EntityLinkFrom));
        }
    }

    public interface IEntityLinkManager : IBaseManager<EntityLink, string>
    {
        IEnumerable<EntityLink> GetByEntityType(Type type);
    }
}