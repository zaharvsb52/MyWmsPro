-- CLEAR TEST DATA
delete from PattTField where Templatefieldid in (-1);
/
delete from bplog where BPLOGID in (-1);
/
delete from wmsres where resid = -1;
/
delete from wmsowbpos where owbposid = -1;
/
delete from wmsowb where owbid in (-1, -10);
/
delete from yinternaltraffic where internaltrafficid in (-1, -10);
/
delete from yexternaltraffic where externaltrafficid in (-1, -10);
/
delete from wmscargoiwb where cargoiwbid in (-1, -10);
/
delete from wmscargoowb where cargoowbid in (-1, -10);
/
delete from ypurposevisit where purposevisitid in (-1, -10);
/ 
delete from wmsproduct where productid in (-1, -2);
/
delete from wmste where tecode = 'TST_TE_1';
/
delete from wmsplace where placecode = 'TST_PLACE_1';
/
delete from wmssegment where segmentcode = 'TST_SEGMENT_1';
/
delete from wmssegmenttype where segmenttypecode = 'TST_SEGMENTTYPE_1';
/
delete from wmspl where plid = -1;
/
delete from wmsarea where areacode in ('TST_AREA_1', 'AUTOTEST');
/
delete from wmsGate where gatecode in ('TST_GATE_1');
/
delete from wmswarehouse where warehousecode in ('TST_WAREHOUSE_1', 'AUTOTEST');
/
delete from wmsarea2blocking where area2blockingid in (-1, -10);
/
delete from wmsareatype where areatypecode in ('TST_AREATYPE_1', 'AUTOTEST');
/
delete from wmsproductblocking where blockingcode in ('TST_PRODUCTBLOCKING_1', 'TST_PRODUCTBLOCKING_2', 'AUTOTEST');
/
delete from wmsart where artcode in ('TST_ART_1', 'AUTOTEST');
/
delete from wmsartgroup where artgroupcode in ('TST_ARTGROUP_1', 'AUTOTEST');
/
delete from wmssku where skuid in (-1, -10);
/
delete from wmsmeasure where measurecode in ('TST_MEASURE_1', 'TST_MEASURE_2', 'AUTOTEST');
/
delete from wmsmeasuretype where measuretypecode in ('TST_MEASURETYPE_1', 'AUTOTEST');
/
delete from wmsworking where workingid = -1;
/
delete from wmswork2entity where work2entityid = -1;
/
delete from wmswork where workid = -1;
/
delete from wmspm2operation where pm2operationcode = 'TST_PM2OPERATION_1';
/
delete from BillEvent2Biller where Event2BillerId = -1;
/
delete from BillTransaction where TransactionId = -1;
/
delete from BillTransactionWDetail where TransactionWDetailId = -1;
/
delete from BillTransactionW where TransactionWId = -1;
/
delete from billCalcVerification where CalcVerificationid = -1;
/
delete from BillCalcConfig where Calcconfigid = -1;
/
delete from BillWorkact where WorkactId = -1;
/
delete from billbiller where billercode in ('TST_BILLBILLER_1', 'TST_BILLBILLER_2');
/
delete from BillBillEntity where Billentitycode = 'TST_BILLBILLENTITY_1';
/
delete from BillOperationCause where operationcauseid = -1;
/
delete from BillStrategyUse where StrategyUseId = -1;
/
delete from BillOperation2Contract where operation2contractid = -1;
/
delete from BillAnalytics where Analyticscode = 'TST_BILLANALYTICS_1';
/
delete from BillContract where contractid = -1;
/
delete from wmsVATType where vattypecode = 'TST_VATTYPE_1';
/
delete from IsoCurrency where currencycode = '-1';
/
delete from PattCalcDataSource where calcdatasourcecode = 'TST_PATTCALCDATASOURCE_1';
/
delete from PattTFieldEntity where templatefieldentityid = -1;
/
delete from PattTFieldSection where templatefieldsectionid = -1;
/
delete from PattTParams where templateparamsid = -1;
/
delete from PattTWhereSection where templatewheresectionid = -1;
/
delete from PattTDataSource where templatedatasourcecode = 'TST_PATTTDATASOURCE_1';
/
delete from SysEnum where Enumid = -1;
/
delete from wmsEventDetailCommact where EventDetailId = -1;
/
delete from wmsEventHeader where EventHeaderId = -1;
/
delete from wmsEventKind where EventKindCode = 'TST_EVENTKIND_1';
/
delete from wmsCommAct where CommActId = -1;
/
delete from BPProcess where Processcode = 'TST_BPPROCESS_1';
/
delete from wmsStatus where Statuscode = 'TST_STATUS_1';
/
delete from wmsemployee where employeeid in (-1);
/
delete from wmsPartner where PartnerId in (-1, -2);
/
delete from wmsqsupplychain where qsupplychainid = -1;
/
delete from wmssupplychain where supplychainid = -9;
/
delete from billOperation where OperationCode = 'TST_BILLOPERATION_1';
/
delete from billOperationClass where OperationClassCode = 'TST_BILLOPERATIONCLASS_1';
/
delete from wmscalendar where calendarid in (-1, -2, -10);
/
delete from wmscalendar2mandant where calendar2mandantid in (-1, -10);
/
delete from wmstetype2mandant where tetype2mandantid in (-1, -10);
/
delete from wmstetype where tetypecode in ('TST_TETYPE_1', 'TST_TETYPE_2', 'AUTOTEST');
/
delete from wmsqlf where qlfcode in ('TST_QLF_1');
/
delete from wmsEventDetailCommact where EventDetailId = -1;
/
delete from wmsEventHeader where EventHeaderId = -1;
/
delete from sysclientsession where clientsessionid = -1;
/
delete from sysconfig2object where config2objectid = -1;
/
delete from sysClientType where clienttypecode in ('TST_CLIENTTYPE_1');
/
delete from sysClient where clientcode in ('TST_CLIENT_1', 'AUTOTEST');
/
delete from sysobjectconfig where objectconfigcode in ('TST_OBJECTCONFIG_1', 'AUTOTEST');
/
delete from wmscustomparam where customparamcode in ('TST_CUSTOMPARAM_1', 'AUTOTEST');
/
delete from sysobject where objectid in (-1, -10);
/
delete from billScale where Scalecode = 'TST_BILLSCALE_1';
/
delete from BillScaleValueType where ScaleValueTypecode = 'TST_BILLSCALEVALUETYPE_1';
/
delete from BillSpecialFunction where Specialfunctioncode = 'TST_BILLSPECIALFUNCTION_1';
/
delete from BillStrategyParams where StrategyParamsId = -1;
/
delete from BillStrategy where Strategycode = 'TST_BILLSTRATEGY_1';
/
delete from BillTransactionType where TransactionTypeCode = 'TST_BILLTRANSACTIONTYPE_1';
/
delete from WmsWorker where WorkerId = -1;
/
delete from BillUserParams where Userparamscode = 'TST_BILLUSERPARAMS_1';
/
delete from BillUserParamsType where Userparamstypecode = 'TST_BILLUSERPARAMSTYPE_1';
/
delete from BPBatch where Batchcode = 'TST_BPBATCH_1';
/
delete from BPWorkflow where Workflowcode = 'TST_BPWORKFLOW_1';
/
delete from wmsFactory where FactoryId = -1;
/
delete from wmsdashboard where dashboardcode = 'TST_DASHBOARD_1';
/
delete from ruser where usercode = 'TST_USER_1';
/
delete from epsjob where jobcode in ('TST_EPSJOB_1');
/
delete from epstask where taskcode in ('TST_EPSTASK_1');
/
delete from wmsgc where gccode = '00000000000000000000000000000001';
/
delete from wmscartype where cartypeid in (-1, -10);
/
delete from wmsruleexec where ruleexecid = -1;
/ 
delete from wmsruleparam where ruleparamid = -1;
/
delete from wmsrule where ruleid in (-1, -10);
/
delete from sysGlobalParam where GlobalParamCode = 'TST_GLOBALPARAM_1';
/
delete from wmsKit where KitCode = 'TST_KIT_1';
/
delete from wmsKitType where KitTypeCode = 'TST_KITTYPE_1';
/
delete from wmsLabelParams where LabelParamsid = -1;
/
delete from wmsLabelUse where LabelUseid = -1;
/
delete from wmsLabel where LabelCode = 'TST_LABEL_1';
/
delete from wmsReport2User where Report2UserId = -1;
/
delete from wmsReport where Report = 'TST_REPORT_1';
/
delete from wmsReportFile where Reportfileid = -1;
/
delete from wmspartnergroup where partnergroupid = -1;
/
delete from wmsplacetype where placetypecode = 'TST_PLACETYPE_1';
/
delete from wmsplaceclass where placeclasscode = 'TST_PLACECLASS_1';
/
delete from wmsinvtaskstep where invtaskstepid = -1;
/
delete from wmsinvtaskgroup where invtaskgroupid = -1;
/
delete from wmsinvgroup where invgroupid = -1;
/
delete from wmsinv where invid = -1;
/
delete from wmsmi where micode = '00000000000000000000000000000001';
/
delete from wmspm where pmcode in ('TST_PM_1', 'TST_PM_2');
/
delete from wmspmmethod where pmmethodcode = 'TST_PMMETHOD_1';
/
delete from epsprinterlogical where LogicalPrinter= 'TST_PRINTERLOGICAL_1';
/
delete from epsprinterphysical where physicalprinter = 'TST_PRINTERPHYSICAL_1';
/
delete from wmstruck where truckcode = 'TST_TRUCK_1';
/
delete from wmstrucktype where trucktypecode = 'TST_TRUCKTYPE_1';
/
delete from wmstransit where transitid = -1;
/
delete from wmsmotionareagroup where motionareagroupcode = 'TST_MOTIONAREAGROUP_1';
/
delete from wmstransporttasktype where ttasktypecode = 'TST_TRANSPORTTASKTYPE_1';
/
delete from epsoutput where outputid = -1;
/
delete from isocountry where countrycode = 'TST';
/
delete from wmsworkergroup where workergroupid = -1;
/
delete from wmsworkgroup where workgroupid = -1;
/
delete from yvehicle where vehicleid = -1;
/
delete from wmsqlfdetail where qlfdetailcode = 'TST_QLFDETAIL_1';
/
delete from ymgroute where mgrouteid = -1;
/
delete from wmsmin where minid = -1;
/
delete from wmsmm where mmcode = 'TST_MM_1';
/
delete from wmsmotionarea where motionareacode = 'TST_MOTIONAREA_1';
/
delete from wmsmpl where mplcode = 'TST_MPL_1';
/
delete from wmsmr where mrcode = 'TST_MR_1';
/
delete from wmsmsc where msccode = 'TST_MSC_1';
/
delete from wmsmsctype where msctypecode = 'TST_MSCTYPE_1';
/
delete from wmsiwb where iwbid = -1;
/
delete from wmsinvreq where invreqid = -1;
/
delete from rusergroup where usergroupcode = 'TST_USERGROUP_1';
/
delete from rright where rightcode = 'TST_RIGHT_1';
/
delete from rrightgroup where rightgroupcode = 'TST_RIGHTGROUP_1';
/
delete from  wmssupplyarea where supplyareacode = 'TST_SUPPLYAREA_1';
/
delete from sysarch where archcode = 'TST_SYSARCH_1';
/
delete from cstreqcustoms where reqcustomsid in (-1, -2);
/