using System;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Xml;
using wmsMLC.APS.wmsWebAPI.Attributes;
using wmsMLC.Business.Objects;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.DAL;
using System.Collections.Generic;

namespace wmsMLC.APS.wmsWebAPI.Controllers.V1
{
    [EntityController]
    public class EntityController<T> : BaseController
        where T : WMSBusinessObject, new()
    {
        public HttpResponseMessage Get(string id)
        {
            var uowFactory = IoC.Instance.Resolve<IUnitOfWorkFactory>();
            using (var uow = uowFactory.Create(GetUnitOfWorkContext()))
            {
                try
                {
                    using (var mgr = IoC.Instance.Resolve<IBaseManager<T>>())
                    {
                        var objId = GetTrueKeyValue(id, mgr);
                        if (objId == null)
                            return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, string.Format("Key not found for {0}", typeof(T).Name));

                        mgr.SetUnitOfWork(uow);
                        T obj = mgr.Get(objId);

                        if (obj == null)
                            return Request.CreateResponse(HttpStatusCode.NotFound);

                        return Request.CreateResponse(HttpStatusCode.OK, SingleEntityResult(obj));
                    }
                }
                catch (Exception ex)
                {
                    Log.Debug(ex);
                    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
                }
            }
        }

        public HttpResponseMessage GetAll(string q)
        {
            var uowFactory = IoC.Instance.Resolve<IUnitOfWorkFactory>();
            using (var uow = uowFactory.Create(GetUnitOfWorkContext()))
            {
                try
                {
                    using (var mgr = IoC.Instance.Resolve<IBaseManager<T>>())
                    {
                        mgr.SetUnitOfWork(uow);
                        var objList = mgr.GetFiltered(q).ToList();
                        return Request.CreateResponse(HttpStatusCode.OK, EntityListResult(objList));
                    }
                }
                catch (Exception ex)
                {
                    Log.Debug(ex);
                    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
                }
            }
        }

        public HttpResponseMessage Post()
        {
            var uowFactory = IoC.Instance.Resolve<IUnitOfWorkFactory>();
            using (var uow = uowFactory.Create(GetUnitOfWorkContext()))
            {
                try
                {
                    var xmlDoc = GetRequestDocument();

                    using (var mgr = IoC.Instance.Resolve<IBaseManager<T>>())
                    {
                        uow.BeginChanges();
                        mgr.SetUnitOfWork(uow, false);

                        // определяем как работать с данными
                        if (!xmlDoc.DocumentElement.Name.EqIgnoreCase("ITEMS"))
                        {
                            var obj = (T)XmlDocumentConverter.ConvertTo(typeof(T), xmlDoc);
                            mgr.Insert(ref obj);
                            uow.CommitChanges();
                            return Request.CreateResponse(HttpStatusCode.Created, obj);
                        }
                        else
                        {
                            var objList = new List<T>();
                            foreach (XmlNode node in xmlDoc.DocumentElement.ChildNodes)
                            {
                                var doc = new XmlDocument();
                                doc.LoadXml(node.OuterXml);
                                var obj = (T)XmlDocumentConverter.ConvertTo(typeof(T), doc);
                                objList.Add(obj);
                            }
                            var list = (IEnumerable<T>) objList;
                            mgr.Insert(ref list);
                            uow.CommitChanges();
                            return Request.CreateResponse(HttpStatusCode.Created, EntityListResult(list));
                        }
                    }
                }
                catch (Exception ex)
                {
                    uow.RollbackChanges();
                    Log.Debug(ex);
                    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
                }
            }
        }

        public HttpResponseMessage Put()
        {
            var uowFactory = IoC.Instance.Resolve<IUnitOfWorkFactory>();
            using (var uow = uowFactory.Create(GetUnitOfWorkContext()))
            {
                try
                {
                    var xmlDoc = GetRequestDocument();

                    using (var mgr = IoC.Instance.Resolve<IBaseManager<T>>())
                    {
                        uow.BeginChanges();
                        mgr.SetUnitOfWork(uow, false);

                        // определяем как работать с данными
                        if (!xmlDoc.DocumentElement.Name.EqIgnoreCase("ITEMS"))
                        {
                            var obj = (T)XmlDocumentConverter.ConvertTo(typeof (T), xmlDoc);
                            mgr.Update(obj);
                            uow.CommitChanges();
                            return Request.CreateResponse(HttpStatusCode.OK, obj);
                        }
                        else
                        {
                            var objList = new List<T>();
                            foreach (XmlNode node in xmlDoc.DocumentElement.ChildNodes)
                            {
                                var doc = new XmlDocument();
                                doc.LoadXml(node.OuterXml);
                                var obj = (T) XmlDocumentConverter.ConvertTo(typeof (T), doc);
                                objList.Add(obj);
                            }
                            mgr.Update(objList);
                            uow.CommitChanges();
                            return Request.CreateResponse(HttpStatusCode.OK, objList);
                        }
                    }
                }
                catch (Exception ex)
                {
                    uow.RollbackChanges();
                    Log.Debug(ex);
                    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
                }
            }
        }

        public HttpResponseMessage Delete(string id)
        {
            var uowFactory = IoC.Instance.Resolve<IUnitOfWorkFactory>();
            using (var uow = uowFactory.Create(GetUnitOfWorkContext()))
            {
                try
                {
                    using (var mgr = IoC.Instance.Resolve<IBaseManager<T>>())
                    {
                        var objId = GetTrueKeyValue(id, mgr);
                        if (objId == null)
                            return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, string.Format("Key not found for {0}", typeof(T).Name));

                        uow.BeginChanges();
                        mgr.SetUnitOfWork(uow, false);
                        T obj = mgr.Get(objId);
                        mgr.Delete(obj);
                        uow.CommitChanges();
                        return Request.CreateResponse(HttpStatusCode.OK);
                    }
                }
                catch (Exception ex)
                {
                    uow.RollbackChanges();
                    Log.Debug(ex);
                    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
                }
            }
        }
    }
}
