using System;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Xml;
using BLToolkit.Aspects;
using BLToolkit.Data;
using BLToolkit.Reflection;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using MLC.Ext.Common.Change;
using MLC.Ext.Common.Data;
using MLC.Ext.Common.Data.Impl.DataTables;
using MLC.Ext.Common.Model.ContextModel;
using MLC.Ext.Common.Model.ContextModel.Impl;
using MLC.Ext.Workflow;
using MLC.Ext.Workflow.Impl;
using MLC.Ext.Workflow.ViewModels;
using MLC.Ext.Wpf.Serialization.Converters;
using MLC.Ext.Wpf.ViewModels;
using MLC.Ext.Wpf.ViewModels.Impl;
using MLC.Ext.Wpf.Views;
using MLC.Ext.Wpf.Views.Impl;
using MLC.SvcClient;
using MLC.SvcClient.Impl;
using MLC.SvcClient.Impl.ExtDirect;
using MLC.WebClient;
using MLC.WebClient.Impl;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using wmsMLC.Business.DAL;
using wmsMLC.Business.DAL.Service;
using wmsMLC.Business.General;
using wmsMLC.Business.Managers;
using wmsMLC.Business.Managers.Processes;
using wmsMLC.Business.Managers.Statemachine;
using wmsMLC.Business.Objects;
using wmsMLC.Business.Objects.Processes;
using wmsMLC.Business.Workflow;
using wmsMLC.Crypto;
using wmsMLC.General;
using wmsMLC.General.BL.Validation;
using wmsMLC.General.DAL.Oracle;
using wmsMLC.General.Resources;
using wmsMLC.General.BL;
using wmsMLC.General.DAL;
using wmsMLC.General.Services;
using wmsMLC.General.Services.Client;
using WebClient.Common.Serialization.Converters;
using IServiceClient = wmsMLC.General.Services.IServiceClient;

namespace wmsMLC.Business
{
    /// <summary> Хелпер бизнес-уровня </summary>
    public static class BLHelper
    {
        private static IUnityContainer _container;

        #region .  Registrations  .
        public static readonly RegisterClass[] Registered = new[]
                {
                    new StandardRegisterClass<AddressBook, decimal>(false,false),
                    new RegisterClass 
                        {
                            ObjectType = typeof(IWBPosInput),
                            KeyType = typeof(decimal),
                        },
                    new RegisterClass 
                        {
                            ObjectType = typeof(InputPlPos),
                            KeyType = typeof(decimal),
                        },
                    new RegisterClass 
                        {
                            ObjectType = typeof(IWBPosQLFDetailDesc),
                            KeyType = typeof(string),
                        },
                    new StandardRegisterClass<Area, string>()
                        {
                            ManagerType = typeof(AreaManager),
                        },
                    new RegisterClass 
                        {
                            ObjectType = typeof(BillWorkActDetailEx),
                            KeyType = typeof(decimal),
                        },
                    new StandardRegisterClass<AdjustmentReason, decimal>(false,false),
                    new StandardRegisterClass<Area2Blocking, decimal>(false,false),
                    new StandardRegisterClass<AreaType, string>(),
                    new StandardRegisterClass<AreaTypeCpv, decimal>(false, false),
                    new StandardRegisterClass<AreaTypeGPV, decimal>(),
                    new StandardRegisterClass<Art, string>(false),
                    new StandardRegisterClass<ArtCpv, decimal>(),
                    new StandardRegisterClass<ArtPrice, decimal>(false),
                    new StandardRegisterClass<ArtGroup, string>(false),
                    new StandardRegisterClass<ArtGroupCpv, decimal>(),
                    new StandardRegisterClass<Art2Group, decimal>(false),
                    new StandardRegisterClass<Barcode, decimal>(false,false),
                    new StandardRegisterClass<BillAnalytics, string>(),
                    new StandardRegisterClass<BillBiller, string>(false),
                    new StandardRegisterClass<BillBiller2Mandant, decimal>(false, false),
                    new StandardRegisterClass<BillBillEntity, string>(false, false),
                    new StandardRegisterClass<BillCalcConfig, decimal>(false, false)
                        {
                            ManagerType = typeof(BillCalcConfigManager),
                            OracleRepositoryType = typeof(wmsMLC.Business.DAL.Oracle.BillCalcConfigRepository),
                            ServiceRepositoryType = typeof(wmsMLC.Business.DAL.Service.BillCalcConfigRepository)
                        },
                    new StandardRegisterClass<BillCalcConfigDetail, decimal>(false, false),
                    new StandardRegisterClass<BillCalcEventConfig, decimal>(false, false),
                    new StandardRegisterClass<BillCalcVerification, decimal>(false, false),
                    new StandardRegisterClass<BillContract, decimal>(false),
                    new StandardRegisterClass<BillEventKind2Biller, decimal>(),
                    new StandardRegisterClass<BillEvent2Biller, decimal>(false, false),
                    new StandardRegisterClass<BillEvent2Operation, decimal>(false, false),
                    new StandardRegisterClass<BillOperation, string>(),
                    new StandardRegisterClass<BillOperationCause, decimal>(false, false),
                    new StandardRegisterClass<BillOperationCauseCpv, decimal>(),
                    new StandardRegisterClass<BillOperationClass, string>(),
                    new StandardRegisterClass<BillOperation2Contract, decimal>(false, false),
                    new StandardRegisterClass<BillOperation2ContractCpv, decimal>(),
                    new StandardRegisterClass<BillScale, string>(false, false),
                    new StandardRegisterClass<BillScaleValue, decimal>(false, false),
                    new StandardRegisterClass<BillScale2O2C, decimal>(false, false),
                    new StandardRegisterClass<BillScaleValueType, string>(),
                    new StandardRegisterClass<BillSpecialFunction, string>(false, false),
                    new StandardRegisterClass<BillSpecialFuncEntity, decimal>(false, false),
                    new StandardRegisterClass<BillSpecialFuncParams, decimal>(false, false),
                    new StandardRegisterClass<BillStrategy, string>(false, false),
                    new StandardRegisterClass<BillStrategyParams, decimal>(false, false),
                    new StandardRegisterClass<BillStrategyUse, decimal>(false, false),
                    new StandardRegisterClass<BillStrategyUseValues, decimal>(false, false),
                    new StandardRegisterClass<BillTariff, decimal>(false, false),
                    new StandardRegisterClass<BillTransactionType, string>(),
                    new StandardRegisterClass<BillTransaction, decimal>(false, false),
                    new StandardRegisterClass<BillTransactionDetail, decimal>(false, false),
                    new StandardRegisterClass<BillTransactionW, decimal>(false, false),
                    new StandardRegisterClass<BillTransactionWDetail, decimal>(false, false),
                    new StandardRegisterClass<BillUserParams, string>(false, false),
                    new StandardRegisterClass<BillUserParams2O2C, decimal>(false, false),
                    new StandardRegisterClass<BillUserParamsType, string>(false, false),
                    new StandardRegisterClass<BillUserParamsTypeApply, decimal>(false, false),
                    new StandardRegisterClass<BillUserParamsValue, decimal>(false, false),
                    new StandardRegisterClass<BillWorkAct, decimal>(false, false),
                    new StandardRegisterClass<BillWorkAct2Op2C, decimal>(false, false),
                    new StandardRegisterClass<BillWorkActDetail, decimal>(false, false)
                    {
                         ManagerType = typeof(BillWorkActDetailManager),
                    },
                    new StandardRegisterClass<BlackList, decimal>(false, false),
                    new StandardRegisterClass<BPBatch, string>(false, false),
                    new StandardRegisterClass<BPBatchSelect, decimal>(false, false),
                    new StandardRegisterClass<Calendar, decimal>(false,false),
                    new StandardRegisterClass<Calendar2Mandant, decimal>(false,false),
                    new StandardRegisterClass<CargoIWB, decimal>(false,false)
                    {
                        ManagerType = typeof(CargoIWBManager),
                    },
                    new StandardRegisterClass<CargoIWBPos, decimal>(false,false),
                    new StandardRegisterClass<CargoOWB, decimal>(false, false)
                    {
                        ManagerType = typeof(CargoOWBManager),  
                    },
                    new StandardRegisterClass<CargoOWBCpv, decimal>(),
                    new StandardRegisterClass<CommAct, decimal>(false, false),
                    new StandardRegisterClass<Config2Object, decimal>(false, false),
                    new StandardRegisterClass<CP2Mandant, decimal>(false, false),
                    new StandardRegisterClass<Dashboard, string>(false,false)
                        {
                            ManagerType = typeof(DashboardManager),
                            OracleRepositoryType = typeof(wmsMLC.Business.DAL.Oracle.DashboardRepository),
                            ServiceRepositoryType = typeof(wmsMLC.Business.DAL.Service.DashboardRepository)
                        },
                    new StandardRegisterClass<Dashboard2User, decimal>(false, false),
                    new StandardRegisterClass<DashboardCpv, decimal>(false, false),
                    new StandardRegisterClass<InternalTraffic, decimal>(false, false)
                    {
                        ManagerType = typeof(InternalTrafficManager),  
                    },
                    new StandardRegisterClass<PurposeVisit, decimal>(false, false),
                    new StandardRegisterClass<InvReq, decimal>(false,false),
                    new StandardRegisterClass<InvReqPos, decimal>(false,false), 
                    new StandardRegisterClass<EcomClient, decimal>(false,false), 
                    new StandardRegisterClass<Employee, decimal>(false,false), 
                    new StandardRegisterClass<Employee2OWB, decimal>(false,false), 
                    new StandardRegisterClass<ExternalTraffic, decimal>(false,false), 
                    new StandardRegisterClass<ExternalTrafficCpv, decimal>(),
                    new StandardRegisterClass<ReportCfg, decimal>(false,false),
                    new StandardRegisterClass<EntityLink, string>()
                    {
                        ManagerType = typeof(EntityLinkManager)
                    },
                    new StandardRegisterClass<EpsConfig, decimal>(false, false),
                    new StandardRegisterClass<EpsJob, string>(false, false),
                    new StandardRegisterClass<EpsJobCpv, decimal>(false, false),
                    new StandardRegisterClass<EpsJobCfg, decimal>(false, false),
                    new StandardRegisterClass<EpsTask, string>(false, false),
                    new StandardRegisterClass<EpsTaskCfg, decimal>(false, false),
                    new StandardRegisterClass<EpsTask2Job, decimal>(false, false),
                    new StandardRegisterClass<EntityFile, decimal>()
                        {
                                ManagerType = typeof (EntityFileManager),
                                OracleRepositoryType = typeof(wmsMLC.Business.DAL.Oracle.EntityFileRepository),
                                ServiceRepositoryType = typeof(wmsMLC.Business.DAL.Service.EntityFileRepository)
                        },
                    new StandardRegisterClass<Entity2GC, decimal>(false, false),
                    new StandardRegisterClass<Entity2GCCpv, decimal>(),
                    new StandardRegisterClass<ExpiryDate, decimal>(false, false),
                    new StandardRegisterClass<GlobalConfig, Guid>(false, false),
                    new StandardRegisterClass<RuleConfig, decimal>(false, false),
                    new StandardRegisterClass<Rule, decimal>(false, false),
                    new StandardRegisterClass<RuleCpv, decimal>(),
                    new StandardRegisterClass<RuleParam, decimal>(false, false),
                    new StandardRegisterClass<RuleExec, decimal>(false, false),
                    new StandardRegisterClass<RuleExecParam, decimal>(false, false),
                    new StandardRegisterClass<EventKind2Mandant, decimal>(false, false),
                    new StandardRegisterClass<Factory, decimal>(false, false),
                    new StandardRegisterClass<Inv, decimal>(false, false),
                    new StandardRegisterClass<InvGroup, decimal>(false, false),
                    new StandardRegisterClass<InvSnapShot, decimal>(),
                    new StandardRegisterClass<InvTask, decimal>(false, false)
                    {
                        ManagerType = typeof(InvTaskManager)
                    },
                    new StandardRegisterClass<InvTaskStep, decimal>(false, false),
                    new StandardRegisterClass<InvTaskGroup,decimal>(false,false),
                    new StandardRegisterClass<IsoCountry, string>(),
                    new StandardRegisterClass<IsoCurrency, string>(),
                    new StandardRegisterClass<IWB, decimal>(false,false) 
                        {
                            ManagerType = typeof(IWBManager),
                            OracleRepositoryType = typeof(wmsMLC.Business.DAL.Oracle.IWBRepository),
                            ServiceRepositoryType = typeof(wmsMLC.Business.DAL.Service.IWBRepository)
                        },
                    new StandardRegisterClass<IWBCpv, decimal>(),
                    new StandardRegisterClass<IWBPos, decimal>(false,false)
                    {
                        ManagerType = typeof (IWBPosManager),
                    },
                    new StandardRegisterClass<IWBPosCpv, decimal>(false, false),
                    new StandardRegisterClass<IWB2Cargo, decimal>(false,false),
                    new StandardRegisterClass<GlobalParam, string>() 
                        {
                            ManagerType = typeof(GlobalParamManager)
                        },
                    new StandardRegisterClass<GlobalParamValue, decimal>(false,false)
                        {
                            ManagerType = typeof(GlobalParamValueManager)
                        },
                    new StandardRegisterClass<Mandant, decimal>(false) 
                        {
                            ManagerType = typeof(MandantManager),
                            ServiceRepositoryType = typeof(wmsMLC.Business.DAL.Service.MandantRepository)
                        },
                    new StandardRegisterClass<MandantCpv, decimal>(),
                    new StandardRegisterClass<MandantGpv, decimal>(),
                    new StandardRegisterClass<Measure, string>(), 
                    new StandardRegisterClass<MeasureType, string>() 
                        {
                            OracleRepositoryType = typeof(wmsMLC.Business.DAL.Oracle.MeasureTypeRepository),
                        },
                    new StandardRegisterClass<MgRoute, decimal>(false, false),
                    new StandardRegisterClass<MgRouteCpv, decimal>(false, false),
                    new StandardRegisterClass<MgRouteDateSelect, decimal>(false, false),
                    new StandardRegisterClass<MgRouteDateSelectCpv, decimal>(false, false),
                    new StandardRegisterClass<MgRouteSelect, decimal>(false, false),
                    new StandardRegisterClass<MI, Guid>(false, false),
                    new StandardRegisterClass<Min, decimal>(false, false),
                    new StandardRegisterClass<MinCpv, decimal>(false, false),
                    new StandardRegisterClass<MinSelect, decimal>(false, false),
                    new StandardRegisterClass<MIUse, decimal>(false, false),
                    new StandardRegisterClass<MM, string>(),
                    new StandardRegisterClass<MMSelect, decimal>(false),
                    new StandardRegisterClass<MMUse, decimal>(false),
                    new StandardRegisterClass<MMUseCpv, decimal>(),
                    new StandardRegisterClass<MotionArea, string>(false),
                    new StandardRegisterClass<MotionAreaGroup, string>(false)
                        {
                            OracleRepositoryType = typeof(wmsMLC.Business.DAL.Oracle.MotionAreaGroupRepository),
                        },
                    new StandardRegisterClass<MotionAreaGroupTr, decimal>(false)
                        {
                            OracleRepositoryType = typeof(wmsMLC.Business.DAL.Oracle.MotionAreaGroupTreeRepository),
                        },
                    new StandardRegisterClass<MotionArea2Group, decimal>(false) 
                        {
                            OracleRepositoryType = typeof(wmsMLC.Business.DAL.Oracle.MotionArea2GroupRepository),
                        },
                    new StandardRegisterClass<MPL, string>(false, false),
                    new StandardRegisterClass<MPLSelect, decimal>(false, false),
                    new StandardRegisterClass<MPLUse, decimal>(false, false),
                    new StandardRegisterClass<MPLUseCpv, decimal>(),
                    new StandardRegisterClass<MR, string>(false, false),
                    new StandardRegisterClass<MRCpv, decimal>(),
                    new StandardRegisterClass<SysEvent, decimal>(false, false,false),
                    new StandardRegisterClass<MRSelect, decimal>(false, false),
                    new StandardRegisterClass<MRUse, decimal>(false, false),
                    new StandardRegisterClass<MSC, string>(false, false),
                    new StandardRegisterClass<MSCSelect, decimal>(false, false),
                    new StandardRegisterClass<MSCType, string>(false, false),
                    new StandardRegisterClass<Warehouse, string>(),
                    new StandardRegisterClass<WarehouseCpv, decimal>(),
                    new StandardRegisterClass<Kit, string>(false),
                    new StandardRegisterClass<KitPos, decimal>(false)
                        {
                            OracleRepositoryType  = typeof(wmsMLC.Business.DAL.Oracle.KitPosRepository),
                        },
                    new StandardRegisterClass<KitType, string>(false)
                        {
                            OracleRepositoryType  = typeof(wmsMLC.Business.DAL.Oracle.KitTypeRepository),
                        },
                    new StandardRegisterClass<ObjectConfig, string>(),
                    new StandardRegisterClass<Object2Config, decimal>(),
                    new StandardRegisterClass<ObjectLookUp, string>() 
                        {
                            ManagerType = typeof(ObjectLookUpManager),
                        },
                    new StandardRegisterClass<ObjectTreeMenu, string>(false, false, false),
                    new StandardRegisterClass<ObjectValid, decimal>()
                        {
                            ManagerType = typeof(ObjectValidManager)
                        },
                    new StandardRegisterClass<OperationStage, string>(false, false),
                    new StandardRegisterClass<Out, decimal>(false, false),
                    new StandardRegisterClass<Output, decimal>(false,false)
                        {
                            ManagerType = typeof(OutputManager),
                            OracleRepositoryType = typeof(wmsMLC.Business.DAL.WebAPI.OutputRepositoryOra),
                            ServiceRepositoryType = typeof(wmsMLC.Business.DAL.WebAPI.OutputRepositorySvc)
                        },
                    new StandardRegisterClass<OutputParam, decimal>(false,false),
                    new StandardRegisterClass<OutputTask, decimal>(false,false)
                        {
                            OracleRepositoryType = typeof(wmsMLC.Business.DAL.Oracle.OutputTaskRepository),
                        },
                    new StandardRegisterClass<OWB, decimal>(false, false),
                    new StandardRegisterClass<OWBCpv, decimal>(),
                    new StandardRegisterClass<OWBPos, decimal>(false, false)
                    {
                        ManagerType = typeof (OWBPosManager),
                    },
                    new StandardRegisterClass<OWBPosCpv, decimal>(false, false),
                    new StandardRegisterClass<OWB2Cargo, decimal>(false, false),
                    new StandardRegisterClass<Packing, string>()
                        {
                            ManagerType = null,
                        },
                    new StandardRegisterClass<Parking, decimal>(), 
                    new StandardRegisterClass<Partner, decimal>(false)
                        {
                                ManagerType = typeof (PartnerManager),
                        },
                    new StandardRegisterClass<Partner2Group, decimal>(false, false),
                    new StandardRegisterClass<PartnerColor, decimal>(false, false),
                    new StandardRegisterClass<PartnerCpv, decimal>(),
                    new StandardRegisterClass<PartnerGpv, decimal>(),
                    new StandardRegisterClass<PartnerGroup, decimal>(false, false),
                    new StandardRegisterClass<PattCalcDataSource, string>(false,false),
                    new StandardRegisterClass<PattCalcField, decimal>(false,false),
                    new StandardRegisterClass<PattCalcParam, decimal>(false,false),
                    new StandardRegisterClass<PattCalcWhere, decimal>(false,false),
                    new StandardRegisterClass<PattTField, decimal>(false,false),
                    new StandardRegisterClass<PattTFieldEntity, decimal>(false,false),
                    new StandardRegisterClass<PattTFieldSection, decimal>(false,false),
                    new StandardRegisterClass<PattTDataSource, string>(false,false),
                    new StandardRegisterClass<PattTParams, decimal>(false,false),
                    new StandardRegisterClass<PattTWhereEntity, decimal>(false,false),
                    new StandardRegisterClass<PattTWhereSection, decimal>(false,false),
                    new StandardRegisterClass<PL, decimal>(false, false),
                    new StandardRegisterClass<PLPos, decimal>(false, false),
                    new StandardRegisterClass<Place2Blocking, decimal>(false,false),
                    new StandardRegisterClass<PlaceClass, string>(),
                    new StandardRegisterClass<PlaceType, string>(),
                    new StandardRegisterClass<PrinterLogical, string>()
                        {
                                OracleRepositoryType = typeof(wmsMLC.Business.DAL.Oracle.PrinterLogicalRepository),
                        },
                    new StandardRegisterClass<PrinterPhysical, string>()
                        {
                                OracleRepositoryType = typeof(wmsMLC.Business.DAL.Oracle.PrinterPhysicalRepository),
                        },
                    new StandardRegisterClass<ProductBlocking, string>(false)
                        {
                                ManagerType = typeof(ProductBlockingManager),
                                OracleRepositoryType = typeof(wmsMLC.Business.DAL.Oracle.ProductBlockingRepository),
                                ServiceRepositoryType = typeof(wmsMLC.Business.DAL.Service.ProductBlockingRepository)
                        },
                    new StandardRegisterClass<ProductBlockingCpv, decimal>(),
                    new StandardRegisterClass<QRes, decimal>(false, false), 
                    new StandardRegisterClass<QSupplyChain, decimal>(false, false), 
                    new StandardRegisterClass<ReceiveArea, string>()
                        {
                                OracleRepositoryType = typeof(wmsMLC.Business.DAL.Oracle.ReceiveAreaRepository),
                        },
                    new StandardRegisterClass<Report, string>(false, false)
                    {
                        ServiceRepositoryType = typeof(wmsMLC.Business.DAL.Service.ReportRepository)
                    },
                    new StandardRegisterClass<ReportCpv, decimal>(),
                    new StandardRegisterClass<ReportFilter, decimal>(false, false),
                    new StandardRegisterClass<Report2Report, decimal>(false, false),
                    new StandardRegisterClass<Report2Entity, decimal>(false, false)
                        {
                                ManagerType = typeof (Report2EntityManager),
                                OracleRepositoryType = typeof(wmsMLC.Business.DAL.Oracle.Report2EntityRepository),
                                ServiceRepositoryType = typeof(wmsMLC.Business.DAL.Service.Report2EntityRepository)
                        },
                    new StandardRegisterClass<Report2User, decimal>(false, false),
                    new StandardRegisterClass<ReportFile, decimal>(false, false)
                        {
                                ManagerType = typeof (ReportFileManager),
                                OracleRepositoryType = typeof(wmsMLC.Business.DAL.Oracle.ReportFileRepository),
                                ServiceRepositoryType = typeof(wmsMLC.Business.DAL.Service.ReportFileRepository)
                        },
                    new StandardRegisterClass<Res, decimal>(false, false),
                    new StandardRegisterClass<Res2SupplyChain, decimal>(false, false),
                    new StandardRegisterClass<Right, string>(false),
                    new StandardRegisterClass<Right2Group, decimal>(false),
                    new StandardRegisterClass<RightGroup, string>(false),
                    new StandardRegisterClass<Route, decimal>(false, false),
                    new StandardRegisterClass<Segment, string>()
                        {
                            ManagerType = typeof(SegmentManager),
                        },
                    new StandardRegisterClass<Segment2Blocking, decimal>(false,false),
                    new StandardRegisterClass<SegmentType, string>(),
                    new StandardRegisterClass<SegmentTypeCpv, decimal>(false, false),
                    new StandardRegisterClass<SegmentTypeGPV, decimal>(),
                    new StandardRegisterClass<Sequence, string>(false,false),
                    new StandardRegisterClass<Site, string>(),
                    new StandardRegisterClass<SiteCPV, string>(),
                    new StandardRegisterClass<SKU, decimal>(false),
                    new StandardRegisterClass<SKU2Partner, decimal>(false, false),
                    new StandardRegisterClass<SKUCpv, decimal>(),
                    new StandardRegisterClass<SKU2TTE, decimal>(false),
                    new StandardRegisterClass<SKUBC, decimal>(),
                    new StandardRegisterClass<Status, string>()
                        {
                            ServiceRepositoryType = typeof(wmsMLC.Business.DAL.Service.StatusRepository)
                        },
                    new StandardRegisterClass<SysArch, string>(false, false),
                    new StandardRegisterClass<SysArchCpv, decimal>(false, false),
                    new StandardRegisterClass<SysArchInst, Guid>(false, false),
                    new StandardRegisterClass<SysService, decimal>(false, false),
                    new StandardRegisterClass<SysServiceCpv, decimal>(),
                    new StandardRegisterClass<Qlf, string>(),
                    new StandardRegisterClass<QlfDetail, string>(),
                    new StandardRegisterClass<StateMachine, decimal>(),
                    new StandardRegisterClass<State2Operation, decimal>(),
                    new StandardRegisterClass<SupplyArea, string>()
                        {
                            OracleRepositoryType = typeof(wmsMLC.Business.DAL.Oracle.SupplyAreaRepository),
                        },
                    new StandardRegisterClass<SupplyChain, decimal>(false, false),
                    new StandardRegisterClass<SysEnum, decimal>()
                    {
                        ServiceRepositoryType = typeof(SysEnumEntityRepository)
                    },
                    new StandardRegisterClass<SysObject, decimal>()
                        {
                            ManagerType = typeof (SysObjectManager),
                            OracleRepositoryType = typeof (wmsMLC.Business.DAL.Oracle.SysObjectRepository),
                            ServiceRepositoryType = typeof(wmsMLC.Business.DAL.Service.SysObjectRepository)
                        },
                    new StandardRegisterClass<SysObjectExt, int>(),
                    new StandardRegisterClass<TE, string>(false,false)
                        {
                            ManagerType = typeof(TEManager),
                            OracleRepositoryType = typeof (wmsMLC.Business.DAL.Oracle.TERepository),
                            ServiceRepositoryType = typeof(wmsMLC.Business.DAL.Service.TERepository),
                        },
                    new StandardRegisterClass<Transit, decimal>(false,false),
                    new StandardRegisterClass<TransitData, decimal>(false,false),
                    new StandardRegisterClass<Gate, string>(), 
                    new StandardRegisterClass<CarType, decimal>(),
                    new StandardRegisterClass<VATType, string>(),
                    new StandardRegisterClass<Vehicle, decimal>(false),
                    new StandardRegisterClass<VehiclePass, decimal>(false, false),
                    new StandardRegisterClass<UserEnum, decimal>()
                    {
                        ServiceRepositoryType = typeof(wmsMLC.Business.DAL.Service.UserEnumRepository)
                    },
                    new StandardRegisterClass<TE2Blocking, decimal>(false,false),
                    new StandardRegisterClass<Product2Blocking, decimal>(false,false),
                    new StandardRegisterClass<TEType, string>()
                    {
                        ServiceRepositoryType = typeof(wmsMLC.Business.DAL.Service.TETypeRepository)
                    },
                    new StandardRegisterClass<TeTypeCpv, decimal>(),
                    new StandardRegisterClass<TEType2Mandant, decimal>(),
                    new StandardRegisterClass<TEType2PlaceClass, decimal>(),
                    new StandardRegisterClass<TEType2TEType, decimal>(),
                    new StandardRegisterClass<TEType2TruckType, decimal>(),
                    new StandardRegisterClass<TETypeGPV, string>(),
                    new StandardRegisterClass<TransportTask, decimal>(false,false),
                    new StandardRegisterClass<TransportTaskType, string>()
                        {
                                OracleRepositoryType = typeof(wmsMLC.Business.DAL.Oracle.TransportTaskTypeRepository),
                        },
                    new StandardRegisterClass<TruckType, string>()
                        {
                                OracleRepositoryType = typeof(wmsMLC.Business.DAL.Oracle.TruckTypeRepository),
                        },
                    new StandardRegisterClass<Truck2MotionAreaGroup, decimal>(),
                    new StandardRegisterClass<UIButton, string>
                        {
                            ServiceRepositoryType = typeof(wmsMLC.Business.DAL.Service.UIButtonRepository)
                        },
                    new StandardRegisterClass<User, string>()
                        {
                                ManagerType = typeof(UserManager),
                                OracleRepositoryType = typeof(wmsMLC.Business.DAL.Oracle.UserRepository),
                                ServiceRepositoryType = typeof(wmsMLC.Business.DAL.Service.UserRepository)
                        },
                    new StandardRegisterClass<User2Group, decimal>(false),
                    new StandardRegisterClass<UserGroup, string>(false),
                    new StandardRegisterClass<CustomParam, string>(false)
                        {
                            ManagerType = typeof(CustomParamManager),
                            OracleRepositoryType = typeof(wmsMLC.Business.DAL.Oracle.CustomParamRepository),
                            ServiceRepositoryType = typeof(wmsMLC.Business.DAL.Service.CustomParamRepository)
                        },
                    new StandardRegisterClass<CustomParamValue, decimal>(false,false),
                    new StandardRegisterClass<UserGrp2RightGrp, decimal>(),
                    new StandardRegisterClass<User2Mandant, decimal>(false,false),
                    new StandardRegisterClass<Qlf2Mandant, decimal>(false,false),
                    new StandardRegisterClass<QlfDetail2Mandant, decimal>(false,false),
                    new StandardRegisterClass<BPProcess, string>(false)
                        {
                            ManagerType = typeof(BPProcessManager),
                            OracleManagerType = typeof(BPProcessManagerOracle),
                            OracleRepositoryType = typeof(wmsMLC.Business.DAL.Oracle.BPProcessRepository),
                            ServiceRepositoryType = typeof(wmsMLC.Business.DAL.WebAPI.BpProcessRepositorySvc)
                        },
                    new StandardRegisterClass<BPProcess2Object, decimal>(),
                    new StandardRegisterClass<BPLog, decimal>(false,false),
                    new StandardRegisterClass<BPTrigger, string>(false)
                        {
                            ManagerType = typeof(BPTriggerManager)
                        },
                    new StandardRegisterClass<BPWorkflow, string>(false,false)
                        {
                            ManagerType = typeof(BPWorkflowManager),
                            OracleRepositoryType = typeof(wmsMLC.Business.DAL.Oracle.BPWorkflowRepository),
                            ServiceRepositoryType = typeof(wmsMLC.Business.DAL.Service.BPWorkflowRepository)
                        },
                    new StandardRegisterClass<ReportRedefinition, decimal>(false),
                    new StandardRegisterClass<PrintStreamConfig, decimal>(false),
                    new StandardRegisterClass<Schedule, decimal>(false, false),
                    new StandardRegisterClass<W2E2Working, decimal>(false, false),
                    new StandardRegisterClass<WeightControl, decimal>(false, false),
                    new StandardRegisterClass<Work, decimal>(false, false)
                        {
                            ManagerType = typeof(WorkManager),
                            OracleManagerType = typeof(SvcWorkManager),
                            ServiceRepositoryType = typeof(WorkRepository)
                        },
                    new StandardRegisterClass<Work2Entity, decimal>(false, false),
                    new StandardRegisterClass<WorkCpv, decimal>(false, false),
                    new StandardRegisterClass<Worker, decimal>(false, false),
                    new StandardRegisterClass<WorkerCpv, decimal>(false, false),
                    new StandardRegisterClass<Worker2Group, decimal>(false, false),
                    new StandardRegisterClass<Worker2GroupCpv, decimal>(false, false),
                    new StandardRegisterClass<Worker2Warehouse, decimal>(false, false),                    
                    new StandardRegisterClass<WorkerGpv, decimal>(false, false),
                    new StandardRegisterClass<WorkerGroup, decimal>(false, false),
                    new StandardRegisterClass<WorkerGroupCpv, decimal>(false, false),
                    new StandardRegisterClass<WorkerPass, decimal>(false, false),
                    new StandardRegisterClass<WorkGroup, decimal>(false, false),
                    new StandardRegisterClass<Working, decimal>(false, false),
                    new StandardRegisterClass<WorkingCpv, decimal>(false, false),
                    new StandardRegisterClass<WTV, decimal>(false, false),
                    new StandardRegisterClass<WTSelect, decimal>(false, false),
                    new StandardRegisterClass<Place, string>(false,false)
                        {
                            ManagerType = typeof(PlaceManager),
                            ServiceRepositoryType = typeof(wmsMLC.Business.DAL.Service.PlaceRepository)
                        },
                    new StandardRegisterClass<PlaceCpv, decimal>(),
                    new StandardRegisterClass<ClientType, string>(),
                    new StandardRegisterClass<ClientTypeCpv, decimal>(false, false),
                    new StandardRegisterClass<ClientTypeGpv, string>(),
                    new StandardRegisterClass<Client, string>(false,false),
                    new StandardRegisterClass<ClientCpv, decimal>(),
                    new StandardRegisterClass<ClientSession, decimal>(false,false)
                        {
                            ManagerType = typeof (ClientSessionManager),
                            OracleRepositoryType = typeof(wmsMLC.Business.DAL.Oracle.ClientSessionRepository),
                            ServiceRepositoryType = typeof(wmsMLC.Business.DAL.Service.ClientSessionRepository),
                            ManagerLifeTime = LifeTime.Singleton
                        },
                    new StandardRegisterClass<Truck, string>(false, false),
                    new StandardRegisterClass<PM, string>(false, false),
                    new StandardRegisterClass<PM2Art, decimal>(false),
                    new StandardRegisterClass<PM2Operation, string>(false, false),
                    new StandardRegisterClass<PMMethod2Operation, decimal>(false, false),
                    new StandardRegisterClass<PMConfig, decimal>(false, true)
                        {
                            ManagerType = typeof (PMConfigManager),
                            OracleRepositoryType = typeof(wmsMLC.Business.DAL.Oracle.PMConfigRepository),
                            ServiceRepositoryType = typeof(wmsMLC.Business.DAL.Service.PMConfigRepository),
                        }, 
                    new StandardRegisterClass<PMMethod, string>(false, false),
                    new StandardRegisterClass<Product, decimal>(false,false)
                    {
                        ManagerType = typeof(ProductManager)
                    },
                    new StandardRegisterClass<EventKind, string>(), 
                    new StandardRegisterClass<EventKind2Action, decimal>(), 
                    new StandardRegisterClass<EventHeader, decimal>(false,false) 
                        {
                            ManagerType = typeof(EventHeaderManager),
                            OracleRepositoryType = typeof(wmsMLC.Business.DAL.Oracle.EventHeaderRepository),
                            ServiceRepositoryType = typeof(wmsMLC.Business.DAL.Service.EventHeaderRepository),
                        },
                    new StandardRegisterClass<EventDetail, decimal>(false,false)
                        {
                            OracleRepositoryType = typeof(wmsMLC.Business.DAL.Oracle.EventDetailRepository),
                        },
                    new StandardRegisterClass<Label, string>(false, false),
                    new StandardRegisterClass<LabelParams, decimal>(false, false),
                    new StandardRegisterClass<LabelUse, decimal>(false, false),
                    new StandardRegisterClass<LabelParamsValue, decimal>(false, false),
                    new StandardRegisterClass<ReportDataBuffer, decimal>(false, false),
                    new StandardRegisterClass<CstReqCustoms, decimal>(false, false)
                    //new StandardRegisterClass<EventParam, decimal>(false,false),
                    //new StandardRegisterClass<TEventParam, decimal>(false,false),
                };
        #endregion

        /// <summary>
        /// Инициализация бизнес-уровня
        /// </summary>
        public static void InitBL(IoCConfigurationContext confContext = null, DALType dalType = DALType.Oracle)
        {
            // пробрасываем контейнер в IoC
            var iocConfig = confContext;
            if (iocConfig == null)
                iocConfig = new IoCConfigurationContext();

            if (iocConfig.ExternalContainer == null)
                iocConfig.ExternalContainer = new UnityContainer();

            _container = iocConfig.ExternalContainer;
            IoC.Instance.Configure(iocConfig);

            // добавляем провайдера описаний наших бизнес объектов
            TypeDescriptor.AddProvider(new WMSBusinessObjectTypeDescriptonProvider(), typeof(WMSBusinessObject));
            WMSBusinessObjectTypeDescriptonProvider.AddTypeDescriptor(typeof(OutputBatch), new StubTypeDescriptor(typeof(OutputBatch)));

            // настраиваем BLToolkit для возможности кэшировать методы не только со стандартными типами
            CacheAspect.IsCacheableParameterType = parameterType =>
                {
                    var res = parameterType.IsValueType ||
                              parameterType == typeof(string) ||
                              parameterType == typeof(Type) ||
                              parameterType == typeof(XmlDocument);
                    if (!res)
                        throw new DeveloperException(string.Format(DeveloperExceptionResources.BadCacheParameterType, parameterType));

                    return res;
                };

            //CacheAspect.IsWeak = true;

            //Регистрируем BarCodeCryptoAlgorithm
            CryptoConfig.AddAlgorithm(typeof(BarCodeCryptoAlgorithm), BarCodeCryptoAlgorithm.DefualtName);
            //Внимание! Если изменяем ключи, то синхронизируем их значения в проекте Launcher для CryptoKeyProvider'а
            var cryptoKeyProvider = new CryptoKeyProvider();
            cryptoKeyProvider.AddOrChangeKey(0,
                new byte[] {0x54, 0x90, 0xd8, 0xab, 0xbc, 0xd3, 0xf7, 0xe4, 0x58, 0x37, 0xb8, 0xb3, 0x45});
            cryptoKeyProvider.AddOrChangeKey(1,
                new byte[] {0x37, 0x56, 0x3e, 0x4b, 0xc6, 210, 0x79, 0x20, 0x9a, 0xdb, 0xc0, 0xfe, 0xcd, 0xf4});
            IoC.Instance.RegisterInstance(typeof(ICryptoKeyProvider), cryptoKeyProvider, LifeTime.Singleton);

            // регистрируем стратегии
            InitAttributeStategies();

            // DAL
            switch (dalType)
            {
                case DALType.Oracle:
                    InitOracelDAL();
                    // на сервисной стороне пока нет ни каких ограничений
                    IoC.Instance.Register<ISecurityChecker, AllowAllSecurityChecker>(LifeTime.Singleton);
                    // на сервисной стороен не валидируем объекты
                    IoC.Instance.Register<IValidatorFactory, EmptyValidatorFactory>(LifeTime.Singleton);

                    // Регистрация StatusStatemachine
                    IoC.Instance.Register<IStatusStatemachine, IWBStatusStatemachine>(typeof(IWB).Name);
                    IoC.Instance.Register<IStatusStatemachine, TransportTaskStatusStatemachine>(typeof(TransportTask).Name);
                    break;

                case DALType.Service:
                    InitServiceDAL();
                    // регистрируем проверку прав пользователя
                    var securityChecker = TypeAccessor.CreateInstance<SecurityChecker>();
                    // на сервисной стороне не валидируем объекты
                    IoC.Instance.Register<IValidatorFactory, ValidatorFactory>(LifeTime.Singleton);

                    IoC.Instance.RegisterInstance(typeof(ISecurityChecker), securityChecker, LifeTime.Singleton);
                    // Регистрация StatusStatemachine
                    IoC.Instance.Register<IStatusStatemachine, StubStatusStateMachine>(typeof(IWB).Name);
                    IoC.Instance.Register<IStatusStatemachine, StubStatusStateMachine>(typeof(TransportTask).Name);

                    break;
            }

            RegisterNew(dalType == DALType.Service);
            RegisterManagers(dalType);
        }

        #region .  NEW DAL  .
        private static void RegisterNew(bool isClientSide)
        {
            // configure json
            var jsonSerializer = BuildJsonSerializer();
            _container.RegisterInstance(jsonSerializer);

            ConfigureDal(_container);

            if (isClientSide)
            {
                // init service locator
                var locator = new UnityServiceLocator(_container);
                ServiceLocator.SetLocatorProvider(() => locator);

                ConfigurePl(_container);
                ConfigureWf(_container);

                RegisterCustomViewModels(_container);
            }
        }

        private static JsonSerializer BuildJsonSerializer()
        {
            var ser = new JsonSerializer();
            //{
            //    ContractResolver = new CamelCasePropertyNamesContractResolver()
            //};

            ser.Converters.Add(new IsoDateTimeConverter
            {
                Culture = System.Globalization.CultureInfo.InvariantCulture,
                DateTimeFormat = "yyyy'-'MM'-'dd'T'HH':'mm':'ss.FFFFFFF"
            });
            ser.Converters.Add(new LongAsStringConverter());
            ser.Converters.Add(new WebClient.Common.Serialization.Converters.BooleanConverter());
            ser.Converters.Add(new EntityIdConverter());
            ser.Converters.Add(new MLC.Ext.Wpf.Serialization.Converters.EntityReferenceConverter());
            ser.Converters.Add(new EntityReferenceCollectionConverter());
            ser.Converters.Add(new WebClient.Common.Serialization.Converters.DataTableConverter());
            ser.Converters.Add(new JsDomainConverter());

            return ser;
        }

        private static void ConfigureDal(IUnityContainer container)
        {
            var baseWebPlatformUrl = ConfigurationManager.AppSettings["WebclientUrl"];
            if (string.IsNullOrEmpty(baseWebPlatformUrl))
                throw new ConfigurationErrorsException("Не задан обязательный параметр 'WebclientUrl'. Проверьте конфигурационный файл.");

            container.RegisterType<IHttpClientStore, HttpClientStore>(new ContainerControlledLifetimeManager());
            container.RegisterType<IAuthService, WmsAuthService>(
                new InjectionFactory(
                    ctr => new WmsAuthService(baseWebPlatformUrl, container.Resolve<IHttpClientStore>())));

            container.RegisterType<IManager, Manager>(new InjectionFactory(ctr =>
            {
                var extDirect = new ExtDirectProvider(baseWebPlatformUrl, "/rpc", ctr.Resolve<IHttpClientStore>(), ctr.Resolve<JsonSerializer>());
                var manager = new Manager();
                manager.AddProvider(extDirect);
                return manager;
            }));

            container.RegisterType<IDataServiceProxy, SvcClientDataServiceProxy>(new ContainerControlledLifetimeManager());
            container.RegisterType<IDataContextModelBuilder, DataContextModelBuilder>();
            container.RegisterType<IDataContextDataBuilder, TableDataContextDataBuilder>();
            container.RegisterType<IDataContextModelStore, DataContextModelStore>(new ContainerControlledLifetimeManager());
            container.RegisterType<IChangeSetCollector, DataSetChangeSetCollector>();
        }

        private static void ConfigurePl(IUnityContainer container)
        {
            container.RegisterType<IViewModelLocator, Wf.ViewModelLocator>();
            container.RegisterType<MLC.Ext.Workflow.Views.IViewLocator, Wf.ViewLocator>();
            container.RegisterType<IViewModelFactory, ViewModelFactory>(new ContainerControlledLifetimeManager());
            container.RegisterType<IEntityViewModelFactory, EntityViewModelFactory>(new ContainerControlledLifetimeManager());
            container.RegisterType<IViewFactory, ViewFactory>(new ContainerControlledLifetimeManager());
        }

        private static void ConfigureWf(IUnityContainer container)
        {
            container.RegisterType<IWorkflowDispatchersFactory, WorkflowDispatchersFactory>();

            var viewModelFactory = (ViewModelFactory)container.Resolve<IViewModelFactory>();
            viewModelFactory.RegisterNamedViewModel("testName", typeof(WfCardViewModel));
        }

        private static void RegisterCustomViewModels(IUnityContainer container)
        {
            var factory = container.Resolve<IEntityViewModelFactory>();
            factory.Register(new EntityRegistryMap("Application4Declaration", ViewKind.Card)
            {
                Type = typeof(EntityCardViewModel),
                InitCallback = o =>
                {
                    var vm = (EntityCardViewModel) o;
                    vm.ViewMode = CardViewMode.View;
                }
            });
        }
        #endregion

        private static void RegisterManagers(DALType dalType)
        {
            // регистрируем авторизатора
            IoC.Instance.Register<IAuthenticationProvider, AuthenticationProvider>(LifeTime.Singleton);
            //IoC.Instance.Register<IAuthenticator, UserManager>();

            // регистрируем поставщика данных о подключениях
            IoC.Instance.Register<ISdclConnectInfoProvider, BPProcessManager>();

            //System
            IoC.Instance.Register<ISystemManager, SystemManager>(LifeTime.Singleton);

            //Client
            IoC.Instance.Register<ISessionRegistrator, ClientSessionManager>(LifeTime.Singleton);

            //SysDbInfo
            IoC.Instance.Register<ISysDbInfo, GlobalParamValueManager>();

            //Lookup
            IoC.Instance.Register<IGetLookupInfo, ObjectLookUpManager>();

            // BL
            IoC.Instance.Register<IGlobalParamManager, GlobalParamManager>();
            IoC.Instance.Register<IReport2EntityManager, Report2EntityManager>();
            IoC.Instance.Register<ISysObjectManager, SysObjectManager>();
            IoC.Instance.Register<IProductBlockingManager, ProductBlockingManager>();
            IoC.Instance.Register<IUserManager, UserManager>();
            IoC.Instance.Register<IXamlManager<BPWorkflow>, BPWorkflowManager>();
            IoC.Instance.Register<IXamlManager<Dashboard>, DashboardManager>();
            IoC.Instance.Register<IXamlManager<BillCalcConfig>, BillCalcConfigManager>();
            IoC.Instance.Register<IBPProcessManager, BPProcessManager>();
            IoC.Instance.Register<IEntityFileManager, EntityFileManager>();
            IoC.Instance.Register<IBPTriggerManager, BPTriggerManager>();
            IoC.Instance.Register<IIWBManager, IWBManager>();
            IoC.Instance.Register<IEventHeaderManager, EventHeaderManager>();
            IoC.Instance.Register<ICustomParamManager, CustomParamManager>();
            IoC.Instance.Register<IPMConfigManager, PMConfigManager>();

            // TODO: вынести имена в константы
            IoC.Instance.Register<IProcessExecutor, ProcessExecutor>(WorkflowProcessExecutorConstants.Client);
            IoC.Instance.Register<IProcessExecutorEngine, WorkflowProcessExecutorEngine>(WorkflowProcessExecutorConstants.Workflow);
            IoC.Instance.Register<IProcessExecutor, wmsMLC.Business.Managers.Processes.DelegateProcessExecutor>(WorkflowProcessExecutorConstants.Service);
            IoC.Instance.Register<IProcessExecutorEngine, CodeProcessExecutorEngine>(WorkflowProcessExecutorConstants.Code);
            IoC.Instance.Register<IProcessHost, ProcessHost>(LifeTime.Singleton);

            IoC.Instance.Register<IEpsOutputManager, EpsOutputManager>();

            IoC.Instance.Register<IManagerForObject, ManagerForObject>(LifeTime.Singleton);
            var mto = IoC.Instance.Resolve<IManagerForObject>();

            // базовые
            foreach (var r in Registered)
            {
                var mgrType = (dalType == DALType.Oracle && r.OracleManagerType != null)
                    ? r.OracleManagerType
                    : r.ManagerType;

                // если объект с manager-ом
                if (mgrType != null)
                {
                    // регистрируем сам тип
                    var regType = typeof(IBaseManager<>).MakeGenericType(r.ObjectType);
                    IoC.Instance.Register(regType, mgrType, r.ManagerLifeTime);
                    mto.Register(r.ObjectType, mgrType);

                    // регистрируем историю
                    var histType = typeof(HistoryWrapper<>).MakeGenericType(r.ObjectType);
                    var regHistType = typeof(IBaseManager<>).MakeGenericType(histType);
                    IoC.Instance.Register(regHistType, mgrType, r.ManagerLifeTime);
                    mto.Register(histType, mgrType);
                }
            }

            // регистрируем типы
            var sysObjMgr = IoC.Instance.Resolve<ISysObjectManager>();
            foreach (var r in Registered.Where(r => r.KeyType != null))
                sysObjMgr.RegisterTypeName(r.ObjectType.Name, r.ObjectType);
        }

        private static void InitAttributeStategies()
        {
            // Определяет виртуальное поле"
            var attVirtual = new ProcessAttributeStrategy("VIRTUALFIELDPARAMVALUE", (list, s) =>
            {
                if (!string.IsNullOrEmpty(s))
                    list.Add(new VirtualAttribute(s));
            });
            EntityDescription.RegisterProcessAttributeStrategy(attVirtual);
        }

        /// <summary>
        /// Инициализация Oracle-соединения
        /// </summary>
        private static void InitOracelDAL()
        {
            try
            {
                var provider = new ExtendedOdpDataProvider();
                //var provider = new OdpManagedDataProvider();
                DbManager.AddDataProvider(provider);
            }
            catch (Exception ex)
            {
                throw new Exception("Не удалось подключить Oracle Provider. Проверьте наличие библиотек подключения и правильность определения разрядности системы в конфигурационном файле", ex);
            }

            // обязательные
            IoC.Instance.Register<IRepositoryFactory, wmsMLC.General.DAL.Oracle.RepositoryFactory>(LifeTime.Singleton);
            //IoC.Instance.Register<IUnitOfWorkFactory, wmsMLC.General.DAL.Oracle.UnitOfWorkFactory>(LifeTime.PerThread);
            //IoC.Instance.Register<IUnitOfWorkFactory, wmsMLC.General.DAL.Oracle.UnitOfWorkFactory>(LifeTime.Singleton);
            IoC.Instance.Register<IUnitOfWorkFactory, wmsMLC.General.DAL.Oracle.UnitOfWorkFactory>();

            foreach (var registerSturcture in Registered)
            {
                if (registerSturcture.OracleRepositoryType == null)
                    continue;
                var regType = typeof(IRepository<,>).MakeGenericType(registerSturcture.ObjectType, registerSturcture.KeyType);
                IoC.Instance.Register(regType, registerSturcture.OracleRepositoryType, registerSturcture.OracleRepositoryLifeTime);
            }
        }

        /// <summary>
        /// Инициализация соединения посредством сервиса.
        /// </summary>
        private static void InitServiceDAL()
        {
            IoC.Instance.Register<ITransmitter, wmsMLC.General.DAL.Service.BaseTransmitter>();

            // обязательные
            IoC.Instance.Register<IRepositoryFactory, wmsMLC.General.DAL.Service.RepositoryFactory>(LifeTime.Singleton);
            IoC.Instance.Register<IUnitOfWorkFactory, wmsMLC.General.DAL.Service.UnitOfWorkFactory>(LifeTime.Singleton);
            IoC.Instance.Register<ISystemRepository, SystemRepository>(LifeTime.Singleton);

            foreach (var registerSturcture in Registered)
            {
                if (registerSturcture.ServiceRepositoryType == null)
                    continue;

                var regType = typeof(IRepository<,>).MakeGenericType(registerSturcture.ObjectType, registerSturcture.KeyType);
                IoC.Instance.Register(regType, registerSturcture.ServiceRepositoryType, registerSturcture.ServiceRepositoryLifeTime);

                // определяем нужно ли отравлять телеграмму на очистку кэша
                if (typeof (ICacheableRepository).IsAssignableFrom(registerSturcture.OracleRepositoryType))
                {
                    var cacheFieldName = wmsMLC.General.DAL.Service.CacheableRepository<Art, int>.IsNeedSendClearCacheTelegramFieldName;
                    var field = registerSturcture.ServiceRepositoryType.GetField(cacheFieldName, BindingFlags.Public | BindingFlags.FlattenHierarchy | BindingFlags.Static);
                    if (field != null)
                        field.SetValue(null, true);
                }
            }
        }

        public static void RegisterServiceClient(string sessionIdStr, ClientTypeCode clienttype, string serviceEndpoint = null)
        {
//            // если в конфиге указали конкретную сессию - используем ее
//            if (!string.IsNullOrEmpty(sessionIdStr) && !"Auto".EqIgnoreCase(sessionIdStr))
//                processId = int.Parse(sessionIdStr);

            // зарегистрируем клиента
            IServiceClient client = new WmsServiceClient(clienttype, serviceEndpoint);
            IoC.Instance.RegisterInstance(typeof(IServiceClient), client, LifeTime.Singleton);
        }

        public static void SubscribeEvents()
        {
            // подпишем системного менеджера на получениe телеграмм
            //var sysMgr = IoC.Instance.Resolve<ISystemManager>();
        }

        public static void FillInitialCaches()
        {
            IoC.Instance.Resolve<ISysObjectManager>().GetAll();
        }
    }
}