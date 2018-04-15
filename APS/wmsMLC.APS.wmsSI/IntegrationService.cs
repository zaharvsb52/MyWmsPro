using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Activation;
using log4net;
using wmsMLC.APS.wmsSI.Helpers;
using wmsMLC.APS.wmsSI.Wrappers;
using wmsMLC.Business.Managers;
using wmsMLC.Business.Objects;
using wmsMLC.Business.Objects.Processes;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.DAL;

namespace wmsMLC.APS.wmsSI
{
    public class WorkActCommit
    {
        public decimal ActId;
        public string ActIdIn1C;
        public DateTime FixDate1C;
        public string ActNumber1C;
    }

    [ServiceBehavior(
        InstanceContextMode = InstanceContextMode.PerSession,
        ConcurrencyMode = ConcurrencyMode.Multiple,
        IncludeExceptionDetailInFaults = true,
        AddressFilterMode = AddressFilterMode.Any,
        AutomaticSessionShutdown = true)]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public partial class IntegrationService : IIntegrationService
    {
        private const int PartnerNameFieldMaxLength = 128;

        private readonly ILog Log = LogManager.GetLogger(typeof (IntegrationService));
        private readonly Dictionary<string, string> _wfCache = new Dictionary<string, string>();

        #region .  SOAP  .

        public class IntegrationLogicalException : BaseException
        {
            public IntegrationLogicalException() { }
            public IntegrationLogicalException(string message) : base(message) { }
            public IntegrationLogicalException(string message, Exception innerException) : base(message, innerException) { }
            public IntegrationLogicalException(string messageFormat, params object[] args) : base(string.Format(messageFormat, args)) { }
        }

        #region . Private methods .
        private decimal? CheckMandant(decimal? mandantId, string mandantCode, IUnitOfWork uow = null, bool critical = true)
        {
            // вычисляем Mandant-а
            if (mandantId == null)
            {
                var mgr = IoC.Instance.Resolve<IBaseManager<Mandant>>();
                if (uow != null)
                    mgr.SetUnitOfWork(uow);

                var key = SourceNameHelper.Instance.GetPropertySourceName(typeof (Mandant), Mandant.MANDANTCODEPropertyName);
                var filter = string.Format("{0} = '{1}'", key, mandantCode);
                var mandant = mgr.GetFiltered(filter, GetModeEnum.Partial).FirstOrDefault();
                if (mandant == null && !critical)
                    return null;

                if (mandant == null)
                    throw new DeveloperException("Мандант с кодом {0} не найден", mandantCode);

                mandantId = mandant.MandantId;
                Log.DebugFormat("ID манданта = {0}", mandantId);
            }
            else
                Log.DebugFormat("Установлен ID манданта = {0}", mandantId);

            return mandantId;
        }

        private static decimal? CheckPartnerHostRefOrName(decimal mandantId, string partnerHostRef, string partnerName, IUnitOfWork uow = null, bool hostrefAndName = false)
        {
            if (string.IsNullOrEmpty(partnerName) && string.IsNullOrEmpty(partnerHostRef))
                return null;

            var partnertype = typeof (Partner);
            var partnerFilter = string.Format("{0} = {1} AND (Upper({2}) = '{3}' or Upper({4}) = '{5}')",
                SourceNameHelper.Instance.GetPropertySourceName(partnertype, Partner.MANDANTIDPropertyName),
                mandantId,
                SourceNameHelper.Instance.GetPropertySourceName(partnertype, Partner.PARTNERNAMEPropertyName),
                string.IsNullOrEmpty(partnerName) ? null : partnerName.ToUpper(),
                SourceNameHelper.Instance.GetPropertySourceName(partnertype, Partner.PARTNERHOSTREFPropertyName),
                string.IsNullOrEmpty(partnerHostRef) ? null : partnerHostRef.ToUpper());

            Partner[] partners;
            using (var mgr = IoC.Instance.Resolve<IBaseManager<Partner>>())
            {
                if (uow != null)
                    mgr.SetUnitOfWork(uow);

                partners = mgr.GetFiltered(partnerFilter).ToArray();
            }

            // если передали код и наименование, то ищем по двум полям
            if ((!string.IsNullOrEmpty(partnerHostRef)) && (!string.IsNullOrEmpty(partnerName)))
            {
                var pHostRefsNames = partners.Where(i => i.PartnerHostRef.EqIgnoreCase(partnerHostRef) && i.PartnerName.EqIgnoreCase(partnerName)).ToArray();
                if (pHostRefsNames.Length > 0)
                    return pHostRefsNames[0].PartnerId;
                if (hostrefAndName)
                    return null;
            }

            // если передали код, то ищем только по нему
            if (!string.IsNullOrEmpty(partnerHostRef))
            {
                var pHostRefs = partners.Where(i => i.PartnerHostRef.EqIgnoreCase(partnerHostRef)).ToArray();
                if (pHostRefs.Length > 0)
                    return pHostRefs[0].PartnerId;
            }

            // если кода не передавали - ищем по имени
            var pNames = partners.Where(i => i.PartnerName.EqIgnoreCase(partnerName)).ToArray();
            return pNames.Length > 0 ? pNames[0].PartnerId : null;
        }

        private static decimal? CheckPartnerHostRef(decimal mandantId, string partnerHostRef, string partnerName,
            out bool updateSenderName, IUnitOfWork uow = null)
        {
            updateSenderName = false;
            if (string.IsNullOrEmpty(partnerHostRef))
                return null;


            var partnertype = typeof(Partner);

            var partnerFilter = string.Format("{0} = {1} AND Upper({2}) = '{3}'",
                SourceNameHelper.Instance.GetPropertySourceName(partnertype, Partner.MANDANTIDPropertyName),
                mandantId,
                SourceNameHelper.Instance.GetPropertySourceName(partnertype, Partner.PARTNERHOSTREFPropertyName),
                string.IsNullOrEmpty(partnerHostRef) ? null : partnerHostRef.ToUpper());

            Partner[] partners;
            using (var mgr = IoC.Instance.Resolve<IBaseManager<Partner>>())
            {
                if (uow != null)
                    mgr.SetUnitOfWork(uow);

                partners = mgr.GetFiltered(partnerFilter).ToArray();
            }

            var pHostRefs = partners.Where(i => i.PartnerHostRef.EqIgnoreCase(partnerHostRef)).ToArray();
            if (pHostRefs.Length == 0)
                return null;

            if (!string.IsNullOrEmpty(partnerName) && !string.Equals(pHostRefs[0].PartnerName, partnerName))
                updateSenderName = true;

            return pHostRefs[0].PartnerId;
        }

        private static bool CheckCpv(decimal? mandantId, IUnitOfWork uow = null)
        {
            var mgr = IoC.Instance.Resolve<IBaseManager<CustomParamValue>>();

            if (uow != null)
                mgr.SetUnitOfWork(uow);

            var mndFilter = string.Format("{0} = '{1}' and {2} = '1' and {3} = 'MandantUseRouteL2'",
                SourceNameHelper.Instance.GetPropertySourceName(typeof(CustomParamValue), CustomParamValue.CPVKeyPropertyName), mandantId,
                SourceNameHelper.Instance.GetPropertySourceName(typeof(CustomParamValue), CustomParamValue.CPVValuePropertyName), 
                SourceNameHelper.Instance.GetPropertySourceName(typeof(CustomParamValue), CustomParamValue.CustomParamCodePropertyName));
            var existCpv = mgr.GetFiltered(mndFilter).ToArray();

            if (existCpv.Length > 0)
                return true;

            return false;
        }

        private void AddOrUpdatePartnerAddress(Partner partner, ICollection<AddressBook> addCollection, bool donotupdatelegaladdress)
        {
            if (partner == null || addCollection == null || addCollection.Count == 0)
                return;

            if (partner.Address == null || partner.Address.Count == 0)
            {
                //У партнера нет адреса

                var result = new List<AddressBook>();
                //Выбираем все не юридические адреса
                var adrs = addCollection.Where(p => p.ADDRESSBOOKTYPECODE != AddressBookType.ADR_LEGAL.ToString()).ToArray();
                if (adrs.Length > 0)
                    result.AddRange(adrs);

                //Добавляем только один юр. адрес
                var adrlegal = addCollection.FirstOrDefault(p => p.ADDRESSBOOKTYPECODE == AddressBookType.ADR_LEGAL.ToString());
                if (adrlegal != null)
                    result.Add(adrlegal);

                if (result.Count > 0)
                    partner.Address = new WMSBusinessCollection<AddressBook>(result);
            }
            else
            {
                //У партнера есть адрес

                //Получим список новых адресов
                var newaddreses = addCollection.Where(p => AddressHelper.FindAddressInCollection(partner.Address, p) == null).ToArray();

                //Добавляем все новые не юридические адреса
                var adrs = newaddreses.Where(p => p.ADDRESSBOOKTYPECODE != AddressBookType.ADR_LEGAL.ToString()).ToArray();
                if (adrs.Length > 0)
                    partner.Address.AddRange(adrs);

                //Ищем новый юридический адрес
                var adrlegal = newaddreses.FirstOrDefault(p => p.ADDRESSBOOKTYPECODE == AddressBookType.ADR_LEGAL.ToString());
                if (adrlegal != null)
                {
                    var existsadrlegal = AddressHelper.GetAddressWithMaxIdByType(partner.Address, AddressBookType.ADR_LEGAL.ToString());
                    if (existsadrlegal != null)
                    {
                        if (!donotupdatelegaladdress)
                        {
                            //Будем обновлять юр. адрес по макс. id
                            var id = existsadrlegal.GetKey<decimal>();
                            MapTo(adrlegal, existsadrlegal);
                            existsadrlegal.SetProperty(existsadrlegal.GetPrimaryKeyPropertyName(), id);
                        }
                    }
                    else
                    {
                        //Добавляем
                        partner.Address.Add(adrlegal);
                    }
                }
            }
        }
        #endregion . Private methods .

        public MandantWrapper[] MandantGet()
        {
            try
            {
                Log.InfoFormat("Start of MandantGet");
                var result = new List<MandantWrapper>();
                var mgr = IoC.Instance.Resolve<IBaseManager<Mandant>>();
                var mandantList = mgr.GetAll().ToArray();
                foreach (var m in mandantList)
                {
                    var mw = new MandantWrapper();
                    mw = MapTo(m, mw);
                    if (m.Address != null)
                    {
                        mw.ADDRESS = new List<AddressBookWrapper>();
                        foreach (var aw in from a in m.Address let aw = new AddressBookWrapper() select MapTo(a, aw))
                        {
                            mw.ADDRESS.Add(aw);
                        }
                    }
                    if (m.GlobalParamVal != null)
                    {
                        mw.GLOBALPARAMVAL = new List<MandantGpvWrapper>();
                        foreach (var g in m.GlobalParamVal)
                        {
                            var gw = new MandantGpvWrapper();
                            gw = MapTo(g, gw);
                            mw.GLOBALPARAMVAL.Add(gw);
                        }
                    }
                    result.Add(mw);
                }
                return result.ToArray();
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
                Log.Debug(ex);
                throw new FaultException<string>(ex.Message, new FaultReason(ex.Message));
            }
            finally
            {
                Log.InfoFormat("End of MandantGet");
            }
        }

        #endregion .  SOAP  .

        #region . ExtMethods .
        private void SetXmlIgnore(object o, bool ignore)
        {
            var cs = o as ICustomXmlSerializable;
            if (cs != null)
            {
                //cs.OverrideIgnore = ignore;
                cs.IgnoreInnerEntity = ignore;
            }
        }

        /// <summary>
        /// По параметру source попытка заполнить параметр dest. По равенству имен свойств.
        /// </summary>
        private T MapTo<T>(object source, T dest)
        {
            var sourceProperties = TypeDescriptor.GetProperties(source);
            var destProperties = TypeDescriptor.GetProperties(dest);
            foreach (PropertyDescriptor property in sourceProperties)
            {
                var prop = destProperties.Cast<PropertyDescriptor>().FirstOrDefault(i => i.Name.EqIgnoreCase(property.Name));
                if (prop == null)
                    continue;
                var value = property.GetValue(source);
                var updateNullValues = false;
                //Если нужно записать в поле значение null
                var nullValue = source as IBaseWrapper;
                if (nullValue != null)
                {
                    if (nullValue.UpdateNullValues != null)
                        updateNullValues = nullValue.UpdateNullValues.Value;
                }
                if (value == null && !updateNullValues)
                    continue;

                try
                {
                    prop.SetValue(dest, value);
                }
                catch (Exception ex)
                {
                    Log.DebugFormat("Can't cast : {0}", ex.Message);
                }
            }

            //HACK: Проблемы со свойством ADDRESSBOOKINDEXSTR
            if (source is AddressBookWrapper)
            {
                var addressBookDest = dest as AddressBook;
                if (addressBookDest != null)
                    addressBookDest.ADDRESSBOOKINDEXSTR = ((AddressBookWrapper)source).ADDRESSBOOKINDEXSTR;
            }
            else if (source is AddressBook)
            {
                var addressBookWrapperDest = dest as AddressBookWrapper;
                if (addressBookWrapperDest != null)
                    addressBookWrapperDest.ADDRESSBOOKINDEXSTR = ((AddressBook)source).ADDRESSBOOKINDEXSTR;
            }

            return dest;
        }
        #endregion . ExtMethods .

        #region . Workflow .
        private string GetWorkflowXaml(string workflowCode)
        {
            if (string.IsNullOrEmpty(workflowCode))
                throw new OperationException("WorkflowCode is null.");

            if (_wfCache.ContainsKey(workflowCode))
                return _wfCache[workflowCode];

            string xaml;
            using (var mgr = (IXamlManager<BPWorkflow>)IoC.Instance.Resolve<IBaseManager<BPWorkflow>>())
            {
                var wf = mgr.Get(workflowCode);
                if (wf == null)
                    throw new OperationException("Workflow с кодом '{0}' не существует.", workflowCode);
                xaml = mgr.GetXaml(workflowCode);
            }

            if (string.IsNullOrEmpty(xaml))
                throw new DeveloperException("Workflow с кодом '{0}' не определен,", workflowCode);

            _wfCache[workflowCode] = xaml;
            return _wfCache[workflowCode];
        }
        #endregion . Workflow .
    }
}