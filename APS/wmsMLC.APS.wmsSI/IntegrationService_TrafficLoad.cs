using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using wmsMLC.APS.wmsSI.Wrappers;
using wmsMLC.Business.Objects;
using wmsMLC.General;
using wmsMLC.General.BL;

namespace wmsMLC.APS.wmsSI
{
    public partial class IntegrationService
    {
        public void YExternalTrafficLoad(YExternalTrafficWrapper item)
        {
            Log.InfoFormat("Start of YExternalTrafficLoad");
            Log.Debug("Получен объект");
            Log.Debug(item.DumpToXML());
            var startAllTime = DateTime.Now;
            try
            {
                using (var uow = UnitOfWorkHelper.GetUnit())
                {
                    try
                    {
                        uow.BeginChanges();
                        var isUpdate = false;

                        if (string.IsNullOrEmpty(item.EXTERNALTRAFFICPASSNUMBER)) 
                            throw new NullReferenceException("Не указан 'Номер пропуска'!");

                        var existFilter = string.Format("{0} = '{1}'",
                            SourceNameHelper.Instance.GetPropertySourceName(typeof(ExternalTraffic),
                            ExternalTraffic.EXTERNALTRAFFICPASSNUMBERPropertyName), item.EXTERNALTRAFFICPASSNUMBER);
                        var extTrafficMgr = IoC.Instance.Resolve<IBaseManager<ExternalTraffic>>();
                        extTrafficMgr.SetUnitOfWork(uow);
                        var extTraffic = extTrafficMgr.GetFiltered(existFilter).FirstOrDefault();

                        // если рейс уже есть в системе, то просто обновим и выйдем
                        if (extTraffic != null)
                        {
                            if (item.STATUSCODE_R == "CAR_DEPARTED")
                            {
                                extTraffic = MapTo(item, extTraffic);
                                SetXmlIgnore(extTraffic, false);
                                extTrafficMgr.Update(extTraffic);
                                uow.CommitChanges();
                                Log.DebugFormat("ExternalTraffic '{0}' updated", extTraffic.GetKey());
                                return;
                            }
                            else
                            {
                                isUpdate = true;
                            }
                        }
                        else
                        {
                            extTraffic = extTrafficMgr.New();
                        }

                        item.MANDANTID = CheckMandant(item.MANDANTID, item.MandantCode, uow, false);
                        if (item.MANDANTID == null)
                            throw new NullReferenceException("Не указан MandantCode");

                        Log.DebugFormat("Мандант = {0}", item.MandantCode);

                        if (item.Vehicle == null) 
                            throw new Exception("VEHICLE not set");

                        if (item.Vehicle.CARTYPE == null) 
                            throw new Exception("CARTYPE not set");

                        if (item.EXTERNALTRAFFICCARRIER == null)
                        {
                            using (var mgr = IoC.Instance.Resolve<IBaseManager<Partner>>())
                            {
                                var filter = string.Format("{0} = '{1}'",
                                    SourceNameHelper.Instance.GetPropertySourceName(typeof (Partner),
                                        Partner.PARTNERNAMEPropertyName),
                                    item.CarrierName);
                                var carrier = mgr.GetFiltered(filter).FirstOrDefault();
                                
                                if (carrier != null)
                                    item.EXTERNALTRAFFICCARRIER = carrier.PartnerId;
                                else
                                    Log.DebugFormat("Set EXTERNALTRAFFICCARRIER by code");
                            }
                        }
                        else
                        {
                            Log.DebugFormat("EXTERNALTRAFFICCARRIER already set");
                        }

                        var carTypeMgr = IoC.Instance.Resolve<IBaseManager<CarType>>();
                        carTypeMgr.SetUnitOfWork(uow);
                        var carTypefilter = string.Format("({0}='{1}' and nvl({2},0)=nvl('{3}',0))",
                            SourceNameHelper.Instance.GetPropertySourceName(typeof(CarType), CarType.CarTypeMarkPropertyName),
                            item.Vehicle.CARTYPE.CARTYPEMARK,
                            SourceNameHelper.Instance.GetPropertySourceName(typeof(CarType), CarType.CarTypeModelPropertyName),
                            item.Vehicle.CARTYPE.CARTYPEMODEL);

                        var carTypeObj = carTypeMgr.GetFiltered(carTypefilter).FirstOrDefault();
                        if (carTypeObj == null)
                        {
                            carTypeObj = new CarType();
                            carTypeObj = MapTo(item.Vehicle.CARTYPE, carTypeObj);
                            carTypeMgr.Insert(ref carTypeObj);
                            Log.DebugFormat("CarType added with code '{0}'", carTypeObj.GetKey());
                        }
                        else
                        {
                            Log.DebugFormat("CarType exists");
                        }

                        item.Vehicle.CARTYPEID_R = carTypeObj.GetKey().To<decimal>();

                        if (item.Vehicle.OWNERLEGAL != null)
                        {
                            var partnerMgr = IoC.Instance.Resolve<IBaseManager<Partner>>();
                            partnerMgr.SetUnitOfWork(uow);
                            var partnerFilter = string.Format("{0}='{1}'",
                                SourceNameHelper.Instance.GetPropertySourceName(typeof(Partner), Partner.PARTNERNAMEPropertyName),
                                item.Vehicle.OWNERLEGAL.PARTNERNAME);

                            var partnerObj = partnerMgr.GetFiltered(partnerFilter).FirstOrDefault();
                            if (partnerObj == null)
                            {
                                partnerObj = new Partner();
                                partnerObj = MapTo(item.Vehicle.OWNERLEGAL, partnerObj);
                                if (!partnerObj.MandantId.HasValue)
                                    partnerObj.MandantId = item.MANDANTID;

                                if (item.Vehicle.OWNERLEGAL.ADDRESS != null)
                                {
                                    partnerObj.Address = new WMSBusinessCollection<AddressBook>();
                                    foreach (var a in from addr in item.Vehicle.OWNERLEGAL.ADDRESS let a = new AddressBook() select MapTo(addr, a))
                                    {
                                        partnerObj.Address.Add(a);
                                    }
                                }
                                SetXmlIgnore(partnerObj, false);
                                partnerMgr.Insert(ref partnerObj);
                                Log.DebugFormat("Partner added with code '{0}'", partnerObj.GetKey());
                            }
                            item.Vehicle.VEHICLEOWNERLEGAL = partnerObj.GetKey().To<decimal>();
                        }
                        else // иначе - физическое
                        {
                            if (item.Vehicle.OWNERPERSON != null)
                            {
                                var workerMgr = IoC.Instance.Resolve<IBaseManager<Worker>>();
                                workerMgr.SetUnitOfWork(uow);
                                var workerFilter =
                                    string.Format("{0}='{1}' and {2}='{3}' and nvl({4},'*')=nvl('{5}','*')",
                                        SourceNameHelper.Instance.GetPropertySourceName(typeof (Worker),
                                            Worker.WorkerLastNamePropertyName),
                                        item.Vehicle.OWNERPERSON.WORKERLASTNAME,
                                        SourceNameHelper.Instance.GetPropertySourceName(typeof (Worker),
                                            Worker.WorkerNamePropertyName),
                                        item.Vehicle.OWNERPERSON.WORKERNAME,
                                        SourceNameHelper.Instance.GetPropertySourceName(typeof (Worker),
                                            Worker.WorkerMiddleNamePropertyName),
                                        item.Vehicle.OWNERPERSON.WORKERMIDDLENAME);

                                var workerObj = workerMgr.GetFiltered(workerFilter).FirstOrDefault();
                                if (workerObj == null)
                                {
                                    workerObj = new Worker();
                                    workerObj = MapTo(item.Vehicle.OWNERPERSON, workerObj);
                                    if (item.Vehicle.OWNERPERSON.WORKERADDRESS != null)
                                    {
                                        workerObj.WorkerAddress = new WMSBusinessCollection<AddressBook>();
                                        foreach (
                                            var a in
                                                from addr in item.Vehicle.OWNERPERSON.WORKERADDRESS
                                                let a = new AddressBook()
                                                select MapTo(addr, a))
                                        {
                                            workerObj.WorkerAddress.Add(a);
                                        }
                                    }
                                    SetXmlIgnore(workerObj, false);
                                    workerMgr.Insert(ref workerObj);
                                    Log.DebugFormat("Worker added with code '{0}'", workerObj.GetKey());
                                }
                                item.Vehicle.VEHICLEPERSON = workerObj.GetKey().To<decimal>();
                            }
                            else
                            {
                                Log.DebugFormat("There is no owner or person for vehicle");
                            }
                        }

                        // ищем или создаем машину
                        var vehicleMgr = IoC.Instance.Resolve<IBaseManager<Vehicle>>();
                        vehicleMgr.SetUnitOfWork(uow);
                        var vehicleFilter =
                            string.Format(
                                "{0}='{1}' and nvl({2},0)=nvl('{3}',0) and nvl({4},0)=nvl('{5}',0) and {6}={7} and (nvl({8},0)=nvl({9},0) or nvl({10},0)=nvl({11},0))",
                                SourceNameHelper.Instance.GetPropertySourceName(typeof(Vehicle), Vehicle.VehicleRNPropertyName), item.Vehicle.VEHICLERN,
                                SourceNameHelper.Instance.GetPropertySourceName(typeof(Vehicle), Vehicle.VehicleVINPropertyName), item.Vehicle.VEHICLEVIN,
                                SourceNameHelper.Instance.GetPropertySourceName(typeof(Vehicle), Vehicle.VehicleTrailerRNPropertyName), item.Vehicle.VEHICLETRAILERRN,
                                SourceNameHelper.Instance.GetPropertySourceName(typeof(Vehicle), Vehicle.CarTypeIDPropertyName), item.Vehicle.CARTYPEID_R,
                                SourceNameHelper.Instance.GetPropertySourceName(typeof(Vehicle), Vehicle.VehicleOwnerLegalPropertyName), item.Vehicle.VEHICLEOWNERLEGAL ?? 0,
                                SourceNameHelper.Instance.GetPropertySourceName(typeof(Vehicle), Vehicle.VehiclePersonPropertyName), item.Vehicle.VEHICLEPERSON ?? 0);
                        var vehicleObj = vehicleMgr.GetFiltered(vehicleFilter).FirstOrDefault();
                        if (vehicleObj == null)
                        {
                            vehicleObj = new Vehicle();
                            vehicleObj = MapTo(item.Vehicle, vehicleObj);
                            SetXmlIgnore(vehicleObj, false);
                            vehicleMgr.Insert(ref vehicleObj);
                            Log.DebugFormat("Vehicle added with code '{0}'", vehicleObj.GetKey());
                        }
                        else
                        {
                            Log.DebugFormat("Vehicle exists");
                        }

                        item.Vehicle.VEHICLEID = vehicleObj.GetKey().To<decimal>();
                        item.VEHICLEID_R = item.Vehicle.VEHICLEID;

                        // ищем или создаем водителя
                        if (item.Driver != null)
                        {
                            var driverMgr = IoC.Instance.Resolve<IBaseManager<Worker>>();
                            Worker driverObj = null;
                            driverMgr.SetUnitOfWork(uow);
                            var isDocsExists = item.Driver.WORKERPASSL != null && item.Driver.WORKERPASSL.Any();
                            if (isDocsExists)
                            {
                                var workerPass = item.Driver.WORKERPASSL.First();
                                var driverFilter =
                                    string.Format(
                                        "{1} = '{2}' and {3} = '{4}' and nvl({5},'*') = nvl('{6}','*') and {0}.{7} = '{8}' and {0}.{9} = '{10}' and {0}.{11} = '{12}'",
                                        Worker.WorkerPassLPropertyName,
                                        Worker.WorkerLastNamePropertyName, item.Driver.WORKERLASTNAME,
                                        Worker.WorkerNamePropertyName, item.Driver.WORKERNAME,
                                        Worker.WorkerMiddleNamePropertyName, item.Driver.WORKERMIDDLENAME,
                                        WorkerPass.WORKERPASSTYPEPropertyName, workerPass.WORKERPASSTYPE,
                                        WorkerPass.WORKERPASSSERIESPropertyName, workerPass.WORKERPASSSERIES,
                                        WorkerPass.WORKERPASSNUMBERPropertyName, workerPass.WORKERPASSNUMBER);

                                driverObj = driverMgr.GetFiltered(driverFilter).FirstOrDefault();
                            }

                            if (driverObj == null)
                            {
                                driverObj = new Worker();
                                driverObj = MapTo(item.Driver, driverObj);

                                if (isDocsExists)
                                {
                                    driverObj.WorkerPassL = new WMSBusinessCollection<WorkerPass>();
                                    foreach (
                                        var newPas in
                                            from pas in item.Driver.WORKERPASSL
                                            let newPas = new WorkerPass()
                                            select MapTo(pas, newPas))
                                    {
                                        driverObj.WorkerPassL.Add(newPas);
                                    }
                                }
                                if (item.Driver.WORKERADDRESS != null && item.Driver.WORKERADDRESS.Count > 0)
                                {
                                    driverObj.WorkerAddress = new WMSBusinessCollection<AddressBook>();
                                    foreach (
                                        var newAddr in
                                            from addr in item.Driver.WORKERADDRESS
                                            let newAddr = new AddressBook()
                                            select MapTo(addr, newAddr))
                                    {
                                        driverObj.WorkerAddress.Add(newAddr);
                                    }
                                }

                                driverObj.CUSTOMPARAMVAL = new WMSBusinessCollection<WorkerCpv>();
                                // скажем что это водитель
                                //var isCardriver = new WorkerGpv {GlobalParamCode_R = "IsCarDriver"};

                                var workersIsDiriver = new List<WorkerCpv>();
                                var workerIsDiriver = new WorkerCpv
                                {
                                    CPV2Entity = "WORKER",
                                    CPVID = -1,
                                    CustomParamCode = "WorkerType"
                                };
                                workersIsDiriver.Add(workerIsDiriver);

                                workerIsDiriver = new WorkerCpv
                                {
                                    CPV2Entity = "WORKER",
                                    CPVParent = -1,
                                    CPVValue = "1",
                                    CustomParamCode = "IsCarDriver"
                                };
                                workersIsDiriver.Add(workerIsDiriver);

                                driverObj.CUSTOMPARAMVAL.AddRange(workersIsDiriver);

                                //if (item.Driver.GLOBALPARAMVAL != null && item.Driver.GLOBALPARAMVAL.Count > 0)
                                //{
                                //    foreach (
                                //        var newGpv in
                                //            from gpv in item.Driver.GLOBALPARAMVAL
                                //            let newGpv = new WorkerGpv()
                                //            select MapTo(gpv, newGpv))
                                //    {
                                //        driverObj.GlobalParamVal.Add(newGpv);
                                //    }
                                //}

                                SetXmlIgnore(driverObj, false);
                                driverMgr.Insert(ref driverObj);
                                Log.DebugFormat("Driver added with code '{0}'", driverObj.GetKey());
                            }
                            else
                            {
                                Log.DebugFormat("Driver exists");
                            }

                            item.Driver.WORKERID = driverObj.GetKey().To<decimal>();
                            item.EXTERNALTRAFFICDRIVER = item.Driver.WORKERID;
                            if (driverObj.WorkerPassL != null && driverObj.WorkerPassL.Any())
                                item.WORKERPASSID_R = driverObj.WorkerPassL[0].GetKey<decimal>();
                        }

                        // создаем рейс     
                        extTraffic = MapTo(item, extTraffic);
                        if (isUpdate)
                        {
                            SetXmlIgnore(extTraffic, true);
                            extTrafficMgr.Update(extTraffic);
                            item.EXTERNALTRAFFICID = extTraffic.GetKey().To<decimal>();
                            Log.DebugFormat("Обновлен рейс (ID = {0})", item.EXTERNALTRAFFICID);
                        }
                        else
                        {
                            SetXmlIgnore(extTraffic, false);
                            extTrafficMgr.Insert(ref extTraffic);
                            item.EXTERNALTRAFFICID = extTraffic.GetKey().To<decimal>();
                            Log.DebugFormat("Создан рейс (ID = {0})", item.EXTERNALTRAFFICID);
                        }

                        // создадим внутренний рейс
                        if (item.InternalTrafficList != null)
                        {
                            //Получим список всех целей визита
                            PurposeVisit[] purposeVisits;
                            using (var mgrPurposeVisit = IoC.Instance.Resolve<IBaseManager<PurposeVisit>>())
                            {
                                mgrPurposeVisit.SetUnitOfWork(uow);
                                purposeVisits = mgrPurposeVisit.GetAll(GetModeEnum.Partial).ToArray();
                            }

                            var dicPurposeVisit = new Dictionary<string, PurposeVisit>();
                            foreach (var p in purposeVisits)
                            {
                                dicPurposeVisit[p.GetProperty<string>(PurposeVisit.PURPOSEVISITNAMEPropertyName).ToUpper()] = p;
                            }

                            //Определяем ид неопределенной цели
                            var unknownPurposeVisit =
                                purposeVisits.FirstOrDefault(p => 
                                    p.GetProperty<string>(PurposeVisit.PURPOSEVISITCODEPropertyName).ToUpper() ==
                                    InternalTrafficPurposeVisitType.Unknown.ToString().ToUpper());
                            if (unknownPurposeVisit == null)
                                throw new DeveloperException("В сущности 'PurposeVisit' отсутствует цель с кодом '{0}'.", InternalTrafficPurposeVisitType.Unknown);

                            var unknownPurposeVisitId = unknownPurposeVisit.GetKey<decimal>();

                            var internalTrafficMgr = IoC.Instance.Resolve<IBaseManager<InternalTraffic>>();
                            internalTrafficMgr.SetUnitOfWork(uow);

                            foreach (var traffic in item.InternalTrafficList)
                            {
                                traffic.MANDANTID = CheckMandant(traffic.MANDANTID, traffic.MandantCode, uow);
                                if (traffic.MANDANTID == null)
                                    throw new NullReferenceException("Не указан MandantCode для внутреннего рейса");
                                Log.DebugFormat("Мандант внутреннего рейса = {0}", traffic.MandantCode);

                                //Ищем цель визита
                                decimal? trafficPurposeVisitId = null;
                                if (!string.IsNullOrEmpty(traffic.INTERNALTRAFFICPURPOSE))
                                {
                                    var key = traffic.INTERNALTRAFFICPURPOSE.ToUpper();
                                    if (dicPurposeVisit.ContainsKey(key))
                                        trafficPurposeVisitId = dicPurposeVisit[key].GetKey<decimal>();
                                }
                                if (!trafficPurposeVisitId.HasValue)
                                    trafficPurposeVisitId = unknownPurposeVisitId;

                                var filterIt = string.Format("{0} = {1} and {2} = {3} and {4} = {5}",
                                    SourceNameHelper.Instance.GetPropertySourceName(typeof(InternalTraffic),
                                        InternalTraffic.ExternalTrafficIDPropertyName), item.EXTERNALTRAFFICID,
                                    SourceNameHelper.Instance.GetPropertySourceName(typeof(InternalTraffic),
                                        InternalTraffic.MandantIDPropertyName), traffic.MANDANTID,
                                    SourceNameHelper.Instance.GetPropertySourceName(typeof(InternalTraffic),
                                        InternalTraffic.PURPOSEVISITID_RPropertyName), trafficPurposeVisitId);
                                var internalObj = internalTrafficMgr.GetFiltered(filterIt).FirstOrDefault();

                                if (internalObj == null)
                                {
                                    //traffic.MANDANTID = CheckMandant(traffic.MANDANTID, traffic.MandantCode, uow);
                                    traffic.EXTERNALTRAFFICID_R = item.EXTERNALTRAFFICID;
                                    var internalTraffic = internalTrafficMgr.New(); //Код статуса внутреннего рейса задается по-умолчанию
                                    internalTraffic = MapTo(traffic, internalTraffic);
                                    internalTraffic.PURPOSEVISITID_R = trafficPurposeVisitId;
                                    SetXmlIgnore(internalTraffic, false);
                                    internalTrafficMgr.Insert(ref internalTraffic);
                                    traffic.INTERNALTRAFFICID = internalTraffic.GetKey<decimal>();
                                    Log.DebugFormat("Создан внутренний рейс (ID = {0})", traffic.INTERNALTRAFFICID);
                                }
                                else
                                {
                                    traffic.INTERNALTRAFFICID = internalObj.GetKey().To<decimal>();
                                    Log.DebugFormat("Внутренний рейс существует (ID = {0})", traffic.INTERNALTRAFFICID);
                                }

                                // создадим грузы
                                if (item.CargoIWBList != null && traffic.INTERNALTRAFFICPURPOSE == "РАЗГРУЗКА")
                                {
                                    var mgrIwb = IoC.Instance.Resolve<IBaseManager<CargoIWB>>();
                                    mgrIwb.SetUnitOfWork(uow);
                                    //var mgrPos = IoC.Instance.Resolve<IBaseManager<CargoIWBPos>>();
                                    //mgrPos.SetUnitOfWork(uow);
                                    foreach (var cargo in item.CargoIWBList)
                                    {
                                        var filterCiwb = string.Format("{0} = {1}",
                                            SourceNameHelper.Instance.GetPropertySourceName(typeof(CargoIWB),
                                                CargoIWB.InternalTrafficIDPropertyName), traffic.INTERNALTRAFFICID);
                                        var cargoIwbObj = mgrIwb.GetFiltered(filterCiwb).FirstOrDefault();

                                        if (cargoIwbObj != null) continue;

                                        cargo.INTERNALTRAFFICID_R = traffic.INTERNALTRAFFICID;
                                        cargo.CARGOIWBCOUNT = 1;
                                        var cargoIwb = new CargoIWB();
                                        cargoIwb = MapTo(cargo, cargoIwb);
                                        if (cargo.CARGOIWBLOADADDRESS == null)
                                        {
                                            cargo.CARGOIWBLOADADDRESS = new AddressBookWrapper
                                            {
                                                ADDRESSBOOKTYPECODE = AddressBookType.ADR_LEGAL
                                            };
                                        }
                                        cargoIwb.CargoIwbLoadAddress = MapTo(cargo.CARGOIWBLOADADDRESS,
                                            cargoIwb.CargoIwbLoadAddress);
                                        SetXmlIgnore(cargoIwb, false);
                                        mgrIwb.Insert(ref cargoIwb);
                                        Log.DebugFormat("Создан груз на приход (ID = {0})", cargoIwb.GetKey());
                                    }
                                }

                                // Груз на расход 
                                if (item.CargoOWBList == null || traffic.INTERNALTRAFFICPURPOSE == "РАЗГРУЗКА")
                                    continue;
                                var mgrOwb = IoC.Instance.Resolve<IBaseManager<CargoOWB>>();
                                mgrOwb.SetUnitOfWork(uow);
                                foreach (var cargo in item.CargoOWBList)
                                {
                                    if (!CheckCpv(item.MANDANTID, uow))
                                    {
                                        var filterCowb = string.Format("{0} = {1}",
                                            SourceNameHelper.Instance.GetPropertySourceName(typeof(CargoOWB),
                                                CargoOWB.INTERNALTRAFFICID_RPropertyName), traffic.INTERNALTRAFFICID);
                                        var cargoOwbObj = mgrOwb.GetFiltered(filterCowb).FirstOrDefault();

                                        if (cargoOwbObj != null) continue;

                                        cargo.INTERNALTRAFFICID_R = traffic.INTERNALTRAFFICID;
                                        cargo.CARGOOWBCOUNT = 1;
                                        var cargoOwb = new CargoOWB();
                                        cargoOwb = MapTo(cargo, cargoOwb);
                                        if (cargo.CARGOOWBUNLOADADDRESS == null)
                                        {
                                            cargo.CARGOOWBUNLOADADDRESS = new AddressBookWrapper
                                            {
                                                ADDRESSBOOKTYPECODE = AddressBookType.ADR_LEGAL
                                            };
                                        }
                                        cargoOwb.CargoOwbUnloadAddress = MapTo(cargo.CARGOOWBUNLOADADDRESS,
                                            cargoOwb.CargoOwbUnloadAddress);
                                        SetXmlIgnore(cargoOwb, false);
                                        mgrOwb.Insert(ref cargoOwb);
                                        Log.DebugFormat("Создан груз на расход (ID = {0})", cargoOwb.GetKey());
                                    }
                                    else
                                        Log.DebugFormat("Груз на расход не создается");
                                }
                            }
                        }
                        uow.CommitChanges();
                    }
                    catch (Exception)
                    {
                        uow.RollbackChanges();
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
                Log.Debug(ex);
                throw new FaultException<string>(ex.Message, new FaultReason(ex.Message));
            }
            finally
            {
                Log.DebugFormat("YExternalTrafficLoad - общее время загрузки {0}", (DateTime.Now - startAllTime));
                Log.InfoFormat("End of YExternalTrafficLoad");
            }
        }
    }
}
