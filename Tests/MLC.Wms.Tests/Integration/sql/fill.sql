-- FILL TEST DATA
insert into wmswarehouse (warehousecode, warehousename, warehousedesc)
     values ('TST_WAREHOUSE_1', 'TST_WAREHOUSE_1', 'for autotest purposes');
/
insert into wmsareatype (areatypecode, areatypename, areatypedesc)
     values ('TST_AREATYPE_1', 'TST_AREATYPE_1', 'for autotest purposes');
/
insert into wmsarea (areacode, areaname, areadesc, areatypecode_r, warehousecode_r)
     values ('TST_AREA_1', 'TST_AREA_1', 'for autotest purposes', 'TST_AREATYPE_1', 'TST_WAREHOUSE_1');
/
insert into wmsproductblocking (blockingcode, blockingname, blockingdesc, blockingforproduct, blockingforte, blockingforplace, blockinglocked)
     values ('TST_PRODUCTBLOCKING_1', 'TST_PRODUCTBLOCKING_1', 'for autotest purposes', 1, 1, 1, 0);
/
insert into wmsproductblocking (blockingcode, blockingname, blockingdesc, blockingforproduct, blockingforte, blockingforplace, blockinglocked)
     values ('TST_PRODUCTBLOCKING_2', 'TST_PRODUCTBLOCKING_2', 'for autotest purposes', 1, 1, 1, 0);
/
insert into wmsarea2blocking (area2blockingid, areacode_r, blockingcode_r, area2blockingdesc)
     values (-1, 'TST_AREA_1', 'TST_PRODUCTBLOCKING_1', 'for autotest purposes');
/
insert into wmsartgroup (artgroupcode, artgroupname, artgroupdesc)
     values ('TST_ARTGROUP_1', 'TST_ARTGROUP_1', 'for autotest purposes');
/
insert into wmsart (artcode, artname, partnerid_r, artdesc, artabcd, arttype)
     values ('TST_ART_1', 'TST_ART_1', '5002', 'for autotest purposes', 'A', 'PRODUCT');
/
insert into wmsmeasuretype (measuretypecode, measuretypename, measuretypedesc)
     values ('TST_MEASURETYPE_1', 'TST_MEASURETYPE_1', 'for autotest purposes');
/
insert into wmsmeasure (measurecode, measuretypecode_r, measurefactor)
     values ('TST_MEASURE_1', 'TST_MEASURETYPE_1', 1);
/
insert into wmsmeasure (measurecode, measuretypecode_r, measurefactor)
     values ('TST_MEASURE_2', 'TST_MEASURETYPE_1', 1);
/
insert into wmssku (skuid, artcode_r, measurecode_r, skucount, skuprimary, skuname, skudesc)
     values ('-1', 'TST_ART_1', 'TST_MEASURE_1', 10, 1, 'TST_SKU_1', 'for autotest purposes');
/
insert into billbiller (billercode, billername, billerprocedurecalc)
     values ('TST_BILLBILLER_1', 'TST_BILLBILLER_1', 'TST_BILLBILLER_1');
/
insert into billbiller (billercode, billername, billerprocedurecalc)
     values ('TST_BILLBILLER_2', 'TST_BILLBILLER_2', 'TST_BILLBILLER_2');
/
insert into BillBillEntity (Billentitycode, Billentityeventfield)
     values ('TST_BILLBILLENTITY_1', 'TST_BILLBILLENTITY_1');
/
insert into BillAnalytics (Analyticscode, Analyticsname)
     values ('TST_BILLANALYTICS_1', 'TST_BILLANALYTICS_1');
/
insert into wmsVATType (Vattypecode, Vattypename, Vattypeinterestrate)
     values ('TST_VATTYPE_1', 'TST_VATTYPE_1', 0);
/
insert into IsoCurrency (Currencycode, Currencynumeric)
     values ('-1', '-1');
/
insert into BillContract (Contractid, Contractdatefrom, Contractowner, Contractcustomer, Currencycode_r, Vattypecode_r)
     values (-1, sysdate, 5002, 5002, '-1', 'TST_VATTYPE_1');
/
insert into BillOperation2Contract (Operation2contractid, Contractid_r, Operation2contractname, Analyticscode_r, Operation2contractcode)
     values (-1, -1, 'TST_BILLOPERATION2CONTRACT_1', 'TST_BILLANALYTICS_1', 'TST_BILLOPERATION2CONTRACT_1');
/
insert into sysenum (enumid, enumgroup, enumkey, enumvalue)
     values (-1, 'TST_SYSENUM_1', 'TST_SYSENUM_1', 'TST_SYSENUM_1');
/
insert into PattTDataSource (Templatedatasourcecode, Templatedatasourcename, Templatedatasourcetype)
     values ('TST_PATTTDATASOURCE_1', 'TST_PATTTDATASOURCE_1', 'TST_PATTTDATASOURCE_1');
/
insert into PattCalcDataSource (Calcdatasourcecode, Templatedatasourcecode_r, Calcdatasourcename)
     values ('TST_PATTCALCDATASOURCE_1', 'TST_PATTTDATASOURCE_1', 'TST_PATTCALCDATASOURCE_1');
/
insert into PattTFieldSection (TemplateFieldSectionID, TemplateDataSourceCode_r, TemplateFieldSectionCode, TemplateFieldSectionName)
     values (-1, 'TST_PATTTDATASOURCE_1', 'TST_PATTTFIELDSECTION_1', 'TST_PATTTFIELDSECTION_1');
/
insert into PattTParams (TemplateParamsID, TemplateDataSourceCode_r, TemplateParamsCode, TemplateParamsName, TemplateParamsDataType)
     values (-1, 'TST_PATTTDATASOURCE_1', 'TST_PATTTPARAMS_1', 'TST_PATTTPARAMS_1', 1);
/
insert into PattTWhereSection (TemplateWhereSectionID, TemplateDataSourceCode_r, TemplateWhereSectionCode, TemplateWhereSectionName)
     values (-1, 'TST_PATTTDATASOURCE_1', 'TST_PATTTWHERESECTION_1', 'TST_PATTTWHERESECTION_1');
/
insert into BillCalcConfig (Calcconfigid, Billercode_r, Operation2contractid_r, Calcdatasourcecode_r, Billentitycode_r, Calcconfigname, Calcconfiglocked, Calcconfigdatefrom, Calcconfigdatetill)
     values (-1, 'TST_BILLBILLER_1', -1, 'TST_PATTCALCDATASOURCE_1', 'TST_BILLBILLENTITY_1', 'TST_BILLCALCCONFIG_1', 0, sysdate, sysdate);
/
insert into wmsEventKind (EventKindCode, EventKindname)
     values ('TST_EVENTKIND_1', 'TST_EVENTKIND_1');
/
insert into wmsStatus (Statuscode, Statusname, Status2Entity)
     values ('TST_STATUS_1', 'TST_STATUS_1', 'STATUS');
/
insert into wmsCommAct (Commactid, Commactname, Statuscode_r)
     values (-1, 'TST_COMMACT_1', 'TST_STATUS_1');
/
insert into wmsPartner (Partnerid, Partnercode, Partnerlink2mandant)
     values (-1, 'TST_PARTNER_1', 5002);
/
insert into wmsPartner (Partnerid, Partnercode, Partnerlink2mandant)
     values (-2, 'TST_PARTNER_2', 5002);
/
insert into billOperationClass (OperationClassCode, OperationClassName)
     values ('TST_BILLOPERATIONCLASS_1', 'TST_BILLOPERATIONCLASS_1');
/
insert into billOperation (OperationCode, Operationname, Operationclasscode_r)
     values ('TST_BILLOPERATION_1', 'TST_BILLOPERATION_1', 'TST_BILLOPERATIONCLASS_1');
/
insert into wmscalendar (calendarid, calendardate, calendardayofweek, calendartimefrom, calendartimetill)
     values (-1, sysdate, 1, sysdate, sysdate);
/
insert into wmscalendar (calendarid, calendardate, calendardayofweek, calendartimefrom, calendartimetill)
     values (-2, sysdate, 1, sysdate, sysdate);
/
insert into wmscalendar2mandant (calendar2mandantid, calendarid_r, partnerid_r)
     values (-1, -1, 5002);
/
insert into wmscargoiwb (cargoiwbid, cargoiwbstampstate)
     values (-1, 'NORMAL');
/
insert into wmstetype (tetypecode, tetypename, tetypelength, tetypewidth, tetypeheight, tetypemaxweight)
     values ('TST_TETYPE_1', 'TST_TETYPE_1', 1, 1, 1, 1);
/
insert into wmstetype (tetypecode, tetypename, tetypelength, tetypewidth, tetypeheight, tetypemaxweight)
     values ('TST_TETYPE_2', 'TST_TETYPE_2', 1, 1, 1, 1);
/
insert into wmstetype2mandant (tetype2mandantid, tetypecode_r, partnerid_r)
     values (-1, 'TST_TETYPE_1', 5002);
/
insert into wmsqlf (qlfcode, qlfname, qlftype, qlfdesc)
     values ('TST_QLF_1', 'TST_QLF_1', 'QLFTYPEQUALITY', 'for autotest purposes');
/
insert into wmscargoowb (cargoowbid)
     values (-1);
/
insert into yexternaltraffic (externaltrafficid, statuscode_r)
     values (-1, 'TST_STATUS_1');
/
insert into ypurposevisit  (purposevisitid, purposevisitcode, purposevisitname)
     values (-1, 'TST_PURPOSEVISIT_1', 'TST_PURPOSEVISIT_1');
/
insert into yinternaltraffic (internaltrafficid, externaltrafficid_r, statuscode_r, partnerid_r, warehousecode_r, purposevisitid_r)
     values (-1, -1, 'TST_STATUS_1', 5002, 'TST_WAREHOUSE_1', -1);
/
insert into sysClientType (clienttypecode, clienttypename)
     values ('TST_CLIENTTYPE_1', 'TST_CLIENTTYPE_1');
/
insert into wmsEventHeader (Eventheaderid, Eventkindcode_r, Clienttypecode_r, Operationcode_r, Eventheaderoperationbusiness, Eventheaderinstance, Statuscode_r, Eventheaderbillstatus, Eventheaderstarttime)
     values (-1, 'TST_EVENTKIND_1', 'TST_CLIENTTYPE_1', 'TST_BILLOPERATION_1', 'UNKNOWN', 'TST_EVENTHEADER_1', 'EVENT_CREATED', 'EVENT_BILL_CLOSE', sysdate);
/
insert into wmsEventDetailCommact (Eventdetailid, Eventheaderid_r, Commactid_r)
     values (-1, -1, -1);
/
insert into sysclient (clientcode, clientname)
     values ('TST_CLIENT_1', 'TST_CLIENT_1');
/
insert into sysobjectconfig (objectconfigcode, objectconfigname)
     values ('TST_OBJECTCONFIG_1', 'TST_OBJECTCONFIG_1');
/
insert into sysobject (objectid, objectname, objectentitycode)
     values (-1, 'TST_SYSOBJECT_1', 'TST_SYSOBJECT_1');
/
insert into wmscustomparam (customparamcode, customparam2entity, customparamdatatype, customparamname)
     values ('TST_CUSTOMPARAM_1', 'TST_SYSOBJECT_1', -1, 'TST_CUSTOMPARAM_1');
/
insert into billCalcVerification (Calcverificationid, Billercode_r, Operation2contractid_r, Calcdatasourcecode_r, Billentitycode_r, Calcverificationname, Calcverificationfrom, Calcverificationtill, Calcverificationmessage, Calcverificationfieldexception)
     values (-1, 'TST_BILLBILLER_1', -1, 'TST_PATTCALCDATASOURCE_1', 'TST_BILLBILLENTITY_1', 'TST_BILLCALCVERIFICATION_1', sysdate, sysdate, 'TST_BILLCALCVERIFICATION_1', 'TST_BILLCALCVERIFICATION_1');
/
insert into billScale (Scalecode)
     values ('TST_BILLSCALE_1');
/
insert into BillOperationCause (Operationcauseid, Operation2contractid_r, Operationcausename)
     values (-1, -1, 'TST_BILLOPERATIONCAUSE_1');
/
insert into BillScaleValueType (Scalevaluetypecode, Scalevaluetypename)
     values ('TST_BILLSCALEVALUETYPE_1', 'TST_BILLSCALEVALUETYPE_1');
/
insert into BillSpecialFunction (Specialfunctioncode, Specialfunctionname)
     values ('TST_BILLSPECIALFUNCTION_1', 'TST_BILLSPECIALFUNCTION_1');
/
insert into BillStrategy (Strategycode, Strategyname, Strategygroup)
     values ('TST_BILLSTRATEGY_1', 'TST_BILLSTRATEGY_1', 'TST_BILLSTRATEGY_1');
/
insert into BillStrategyUse (Strategyuseid, Strategycode_r, Operation2contractid_r, Strategyusename, Strategyuseorder, Strategyusefrom, Strategyusetill)
     values (-1, 'TST_BILLSTRATEGY_1', -1, 'TST_BILLSTRATEGYUSE_1', -1, sysdate, sysdate);
/
insert into BillStrategyParams (Strategyparamsid, Strategycode_r, Strategyparamsname, Strategyparamsdatatype, Strategyparamsindex)
     values (-1, 'TST_BILLSTRATEGY_1', 'TST_BILLSTRATEGYPARAMS_1', -1, -1);
/
insert into BillTransactionType (TransactionTypeCode, Transactiontypename)
     values ('TST_BILLTRANSACTIONTYPE_1', 'TST_BILLTRANSACTIONTYPE_1');
/
insert into BillTransaction (Transactionid, Eventheaderid_r, Billercode_r, Transactiontypecode_r, Partnerid_r, Transactionrecipient, Transactionammount, Currencycode_r)
     values (-1, -1, 'TST_BILLBILLER_1', 'TST_BILLTRANSACTIONTYPE_1', 5002, 5002, -1, '-1');
/
insert into WmsWorker (Workerid, Workerlastname)
     values (-1, 'TST_WORKER_1');
/
insert into BillTransactionW (Transactionwid, Eventheaderid_r, Billercode_r, Transactiontypecode_r, Partnerid_r, Workerid_r, Transactionwammount, Currencycode_r)
     values (-1, -1, 'TST_BILLBILLER_1', 'TST_BILLTRANSACTIONTYPE_1', 5002, -1, -1, '-1');
/
insert into BillUserParamsType (Userparamstypecode, Userparamstypename, Userparamstyperangetype, Userparamstyperangedatatype, Userparamstypevaluedatatype, Userparamstypeusingoption)
     values ('TST_BILLUSERPARAMSTYPE_1', 'TST_BILLUSERPARAMSTYPE_1', 'TST_BILLUSERPARAMSTYPE_1', -1, -1, 'TST_BILLUSERPARAMSTYPE_1');
/
insert into BillUserParams (Userparamscode, Userparamstypecode_r, Userparamsname)
     values ('TST_BILLUSERPARAMS_1', 'TST_BILLUSERPARAMSTYPE_1', 'TST_BILLUSERPARAMS_1');
/
insert into BillWorkact (Workactid, Contractid_r, Workactdatefrom, Workactdatetill, Workactdate)
     values (-1, -1, sysdate, sysdate, sysdate);
/
insert into BillEvent2Biller (Event2billerid, Eventheaderid_r, Billercode_r)
     values (-1, -1, 'TST_BILLBILLER_1');
/
insert into BillTransactionWDetail (Transactionwdetailid, Transactionwid_r, Transactionwdetailname)
     values (-1, -1, 'TST_BILLTRANSACTIONWDETAIL_1');
/
insert into BPWorkflow (Workflowcode, Workflowname)
     values ('TST_BPWORKFLOW_1', 'TST_BPWORKFLOW_1');
/
insert into BPBatch (Batchcode, Batchname, Workflowcode_r)
     values ('TST_BPBATCH_1', 'TST_BPBATCH_1', 'TST_BPWORKFLOW_1');
/
insert into wmsFactory (Factoryid, Factorycode, Factoryname, Partnerid_r)
     values (-1, 'TST', 'TST_FACTORY_1', 5002);
/
insert into BPProcess (Processcode, Processname, Processexecutor, Processengine, Processtype, Statuscode_r)
     values ('TST_BPPROCESS_1', 'TST_BPPROCESS_1', 'TST_BPPROCESS_1', 'TST_BPPROCESS_1', 'TST_BPPROCESS_1', 'TST_STATUS_1');
/
insert into wmsGate (Gatecode, Warehousecode_r, Gatenumber, Gatename)
     values ('TST_GATE_1', 'TST_WAREHOUSE_1', 'TST_GATE_1', 'TST_GATE_1');
/
insert into wmsdashboard (dashboardcode, dashboardname)
     values ('TST_DASHBOARD_1', 'TST_DASHBOARD_1');
/
insert into ruser (usercode, login, userauthentication, userpassword)
     values ('TST_USER_1', 'TST_USER_1', 0, 'qw34fgxcvbftty');
/
insert into wmsemployee (employeeid, partnerid_r)
     values (-1, -1);
/
insert into wmsowb (owbid, partnerid_r, owbname, statuscode_r, owbproductneed, owbtype)
     values (-1, 5002, 'TST_OWB_1', 'TST_STATUS_1', 'TST_SYSENUM_1', 'TST_SYSENUM_1');
/
insert into epsjob (jobcode)
     values ('TST_EPSJOB_1');
/
insert into epstask (taskcode, taskname, tasktype)
     values ('TST_EPSTASK_1', 'TST_EPSTASK_1', 'TST_EPSTASK_1');
/
insert into wmsgc (gccode, gcentity, gckey)
     values ('00000000000000000000000000000001', 'TST_SYSOBJECT_1', 'TST_GC_1');
/
insert into wmscartype (cartypeid, cartypemark)
     values (-1, 'TST_CARTYPE_1');
/
insert into wmsrule (ruleid, rulename)
     values (-1, 'TST_RULE_1');
/
insert into sysGlobalParam (Globalparamcode)
     values ('TST_GLOBALPARAM_1');
/
insert into wmsKitType (KitTypeCode, KitTypeName)
     values ('TST_KITTYPE_1', 'TST_KITTYPE_1');
/
insert into wmsKit (KitCode, KitTypeCode_r)
     values ('TST_KIT_1', 'TST_KITTYPE_1');
/
insert into wmsReportFile (Reportfileid, Reportfile)
     values (-1, 'TST_REPORTFILE_1');
/
insert into wmsReport (Report, Reportfile_r, Epshandler, Reportcopies, Reportname, Reporttype)
     values ('TST_REPORT_1', 'TST_REPORTFILE_1', -1, -1, 'TST_REPORT_1', 'TST_REPORT_1');
/
-- http://mp-ts-nwms/issue/wmsMLC-11634
insert into wmsLabel (LabelCode, LabelName, Report_R, PARTNERID_R)
     values ('TST_LABEL_1', 'TST_LABEL_1', 'TST_REPORT_1', 5002);
/
insert into wmsLabelUse (Labeluseid, Labelcode_r)
     values (-1, 'TST_LABEL_1');
/
insert into wmsLabelParams (Labelparamsid, Labelcode_r, Labelparamsname)
     values (-1, 'TST_LABEL_1', 'TST_LABEL_1');
/
insert into wmspartnergroup (partnergroupid, partnergroupname)
     values (-1, 'TST_PARTNERGROUP_1');
/
insert into wmssegmenttype (segmenttypecode, segmenttypename)
     values ('TST_SEGMENTTYPE_1', 'TST_SEGMENTTYPE_1');
/
insert into wmssegment (segmentcode, segmentnumber, areacode_r, segmenttypecode_r, segmentname)
     values ('TST_SEGMENT_1', 'TST_SEGMENT_1', 'TST_AREA_1', 'TST_SEGMENTTYPE_1', 'TST_SEGMENT_1');
/
insert into wmsplacetype (placetypecode, placetypename, placetypecapacity, placetypelength, placetypewidth, placetypeheight, placetypemaxweight)
     values ('TST_PLACETYPE_1', 'TST_PLACETYPE_1', 0, 0, 0, 0, 0);
/
insert into wmsplaceclass (placeclasscode, placeclassname)
     values ('TST_PLACECLASS_1', 'TST_PLACECLASS_1');
/
insert into wmsplace (placecode, SegmentCode_r, PlaceS, PlaceX, PlaceY, PlaceZ, PlaceCapacityMax, PlaceCapacity, PlaceTypeCode_r, PlaceClassCode_r, PlaceSortA, PlaceSortB, PlaceSortC, PlaceSortD, PlaceSortPick, PlaceWeight, PlaceWeightGroup, StatusCode_r)
     values ('TST_PLACE_1', 'TST_SEGMENT_1', 0, 0, 0, 0, 0, 0, 'TST_PLACETYPE_1', 'TST_PLACECLASS_1', 0, 0, 0, 0, 0, 0, 0, 'PLC_FREE');
/
insert into wmspl (plid, pltype, statuscode_r, mplcode_r)
     values (-1, 'TST_PL_1', 'TST_STATUS_1', 'TST_PL_1');
/
insert into wmsmi (micode, miname, miinvtype, miasksku, miline, milineperpage, micalctype, micalcban, mipiece)
     values ('00000000000000000000000000000001', 'TST_MI_1', 'TST_MI_2', 0, 10, 999, 'TST_MI_3', 0, 0);
/
insert into wmsinv (invid, invname, partnerid_r, micode_r)
  values(-1, 'TST_INV_1', 5002, '00000000000000000000000000000001');
/
insert into wmspm (pmcode, pmname)
  values('TST_PM_1', 'TST_PM_1');
/
insert into wmspm (pmcode, pmname)
  values('TST_PM_2', 'TST_PM_2');
/
insert into wmspm2operation (pm2operationcode, pmcode_r, operationcode_r)
  values('TST_PM2OPERATION_1', 'TST_PM_1', 'TST_BILLOPERATION_1');
/
insert into wmspmmethod (pmmethodcode, pmmethodname)
  values('TST_PMMETHOD_1', 'TST_PMMETHOD_1');
/
insert into sysconfig2object (config2objectid, objectconfigcode_r, objectentitycode_r, objectname_r)
  values(-1, 'TST_OBJECTCONFIG_1', 'TST_SYSOBJECT_1', -1);
/
insert into epsprinterphysical (physicalprinter, physicalprinterlocked)
  values('TST_PRINTERPHYSICAL_1', 0);
/
insert into epsprinterlogical (LogicalPrinter, PhysicalPrinter_r, LogicalPrinterCopies, LogicalPrinterLocked, LogicalPrinterTray)
  values('TST_PRINTERLOGICAL_1', 'TST_PRINTERPHYSICAL_1', 1, 0, 1);
/
insert into wmste (tecode, tetypecode_r, STATUSCODE_R, TEPACKSTATUS, TECURRENTPLACE, TECREATIONPLACE, TELength, TEWidth, TEHeight, TEMaxWeight)
  values('TST_TE_1', 'TST_TETYPE_1', 'TST_STATUS_1', 'TST_STATUS_1', 'TST_PLACE_1', 'TST_PLACE_1', 1, 1, 1, 1);
/
insert into wmsproduct (ProductID, tecode_r, SKUID_r,  ProductCountSKU, ProductCount, ProductTTEQuantity, QLFCode_r, ProductInputDate, ProductInputDateMethod, ArtCode_r, partnerid_r, ProductOwner, statuscode_r)
  values(-1, 'TST_TE_1', -1, 1, 1, 1, 'TST_QLF_1', sysdate, 'DAY', 'TST_ART_1', 5002, 5002, 'PRODUCT_FREE');
/
insert into wmsproduct (ProductID, tecode_r, SKUID_r,  ProductCountSKU, ProductCount, ProductTTEQuantity, QLFCode_r, ProductInputDate, ProductInputDateMethod, ArtCode_r, partnerid_r, ProductOwner, statuscode_r)
  values(-2, 'TST_TE_1', -1, 1, 1, 1, 'TST_QLF_1', sysdate, 'DAY', 'TST_ART_1', 5002, 5002, 'PRODUCT_FREE');
/
insert into wmstrucktype (trucktypecode, trucktypename, TRUCKTYPEWEIGHTMAX, TRUCKTYPEPICKCOUNT)
  values('TST_TRUCKTYPE_1', 'TST_TRUCKTYPE_1', 1, 1);
/
insert into wmstruck (truckcode, trucktypecode_r, truckname)
  values('TST_TRUCK_1', 'TST_TRUCKTYPE_1', 'TST_TRUCK_1');
/
insert into wmstransit (transitid, transitname, partnerid_r, Transit2Entity, TransitV2GUI)
  values(-1, 'TST_TRANSIT_1', 5002, 'TST_SYSOBJECT_1', 0);
/
insert into wmsmotionareagroup (motionareagroupcode, motionareagroupname)
  values('TST_MOTIONAREAGROUP_1', 'TST_MOTIONAREAGROUP_1');
/
insert into wmstransporttasktype (TTASKTYPECODE)
  values('TST_TRANSPORTTASKTYPE_1');
/
insert into epsoutput p (outputid, login_r, host_r, outputstatus, epshandler)
  values(-1, 'TECH_AUTOTEST', 'TST_OUTPUT_1', 'TST_OUTPUT_1', 1);
/
insert into wmsruleexec (ruleexecid, ruleid_r )
  values(-1, -1);
/
insert into wmsruleparam (ruleparamid, ruleid_r, ruleparamname, ruleparamdatatype, ruleparammustset)
  values(-1, -1, 'TST_RULEPARAM_1', 1, 0);
/  
insert into isocountry (countrycode, countryalpha2, countrynumeric )
  values('TST', 'TS', 'TST');
/
insert into wmsworkergroup (workergroupid, workergroupname)
  values(-1, 'TST_WORKERGROUP_1');
/
insert into wmsworkgroup (workgroupid, partnerid_r, workgroupcode )
  values(-1, 5002, 'TST_WORKGROUP_1');
/
insert into sysclientsession (clientsessionid, clientcode_r, clienttypecode_r, clientsessionbegin, clientsessionend, clientsessionappkey, clientsessioncorrectlyoff, usercode_r)
  values(-1, 'TST_CLIENT_1', 'TST_CLIENTTYPE_1', sysdate, sysdate, 'TST_CLIENTSESSION_1', 0, 'TECH_AUTOTEST');
/
insert into wmswork (workid, workgroupid_r, operationcode_r, statuscode_r, clientsessionid_r)
  values(-1, -1, 'TST_BILLOPERATION_1', 'TST_STATUS_1', -1);
/
insert into wmsworking (workingid, workid_r, workerid_r, workingfrom, workingtill, workingmult)
  values(-1, -1, -1, sysdate, sysdate, 0);
/
insert into wmswork2entity (work2entityid, workid_r, Work2EntityEntity, Work2EntityKey )
  values(-1, -1, 'TST_SYSOBJECT_1', 'TST_SYSOBJECT_1');
/
insert into wmsreport2user (report2userid, usercode_r, report_r)
  values(-1, 'TECH_AUTOTEST', 'TST_REPORT_1');
/
insert into yvehicle (vehicleid)
  values(-1);
/
insert into wmsqlfdetail (qlfdetailcode, qlfdetailname)
  values('TST_QLFDETAIL_1', 'TST_QLFDETAIL_1');
/
insert into ymgroute (mgrouteid, partnerid_r, mgroutename, mgroutenumber, mgroutecreateroute)
  values(-1, 5002, 'TST_MGROUTE_1', 'TST_MGROUTE_1', 0);
/
insert into wmsmin (minid, partnerid_r, minname)
  values(-1, 5002,'TST_MIN_1');
/
insert into wmsmm (mmcode, mmname)
  values('TST_MM_1', 'TST_MM_1');
/
insert into wmsmotionarea (motionareacode, motionareaname)
  values('TST_MOTIONAREA_1', 'TST_MOTIONAREA_1');
/
insert into wmsmpl (mplcode, mplname)
  values('TST_MPL_1', 'TST_MPL_1');
/
insert into wmsmr (mrcode, mrname)
  values('TST_MR_1', 'TST_MR_1');
/
insert into wmsmsctype (msctypecode, msctypename, msctypeorder)
  values('TST_MSCTYPE_1', 'TST_MSCTYPE_1', 1);
/
insert into wmsmsc (msccode, mscname, msctypecode_r, msctargetsupplyarea, mscoperationorder, mscfinal)
  values('TST_MSC_1', 'TST_MSC_1', 'TST_MSCTYPE_1', 'TST_MSC_1', 1, 0);
/
insert into wmsiwb (iwbid, partnerid_r, iwbname, iwbpriority, iwbtype)
  values(-1, 5002, 'TST_IWB_1', 1, 'TST_IWB_1');
/
insert into wmsinvreq (invreqid, partnerid_r, invreqname, statuscode_r)
  values(-1, 5002, 'TST_INVREQ_1', 'INV_CREATED');
/
insert into wmsinvgroup (invgroupid, invid_r, invgroupname, invgroupfilter, statuscode_r)
  values(-1, -1, 'TST_INVGROUP_1', 'TST_INVGROUP_1', 'INVGROUP_CREATED');
/
insert into wmsinvtaskgroup (invtaskgroupid, invgroupid_r, invtaskgroupnumber, statuscode_r)
  values(-1, -1, 1, 'INVTASKGROUP_CREATED');
/
insert into wmsinvtaskstep (invtaskstepid, invtaskstepnumber, invtaskgroupid_r, statuscode_r)
  values(-1, 1, -1, 'INVTASKSTEP_CREATED');
/
insert into rusergroup (usergroupcode, usergrouplocked, usergroupname)
  values('TST_USERGROUP_1', 0, 'TST_USERGROUP_1');
/
insert into rright (rightcode, rightlocked)
  values('TST_RIGHT_1', 0);
/
insert into rrightgroup (rightgroupcode, rightgrouplocked)
  values('TST_RIGHTGROUP_1', 0);
/
insert into wmsowbpos (owbposid, owbid_r, owbposnumber, skuid_r, owbposcount, owbposcount2sku, statuscode_r, qlfcode_r)
  values(-1, -1, 1, -1, 1, 1, 'OWBPOS_CREATED', 'TST_QLF_1');
/
insert into wmsres (resid, productid_r, owbposid_r, mrcode_r, restype)
  values(-1, -1, -1, 'TST_MR_1', 'NORMAL');
/
insert into wmsqsupplychain (qsupplychainid, partnerid_r, statuscode_r)
  values(-1, 5002, 'QSC_CREATED');
/
insert into wmssupplyarea (supplyareacode, supplyareaname)
  values('TST_SUPPLYAREA_1', 'TST_SUPPLYAREA_1');
/
insert into wmssupplychain (supplychainid, msccode_r, supplychainsourcesupplyarea, supplychaintargetsupplyarea, statuscode_r, operationcode_r)
  values(-9, 'TST_MSC_1', 'TST_SUPPLYAREA_1', 'TST_SUPPLYAREA_1', 'SUPPLYCHAIN_CREATED', 'TST_BILLOPERATION_1');
/
insert into sysarch (archcode, archname, archorder, archtype)
 values('TST_SYSARCH_1', 'TST_SYSARCH_NAME', 1, 'TST_SYSENUM_1');
/
insert into bplog (bplogid, processcode_r, usercode_r, bploginstance, bplogcurrentstep, bplogstatus, bplogstarttime, bplogendtime)
  values(-1, 'TST_BPPROCESS_1', 'TST_USER_1', 'TST_BPLOG_1', 'TST_BPLOG_1', 'TST_BPLOG_1', sysdate, sysdate);
/
insert into PattTField (Templatefieldid, Templatefieldsectionid_r, Templatefieldname, Templatefieldalias, Templatefielddatatype)
  values(-1, -1, 'TST_PATTTFIELD_1', 'TST_PATTTFIELD_1', -1);
/
insert into cstreqcustoms (reqcustomsid, statuscode_r, partnerid_r) values(-1, 'REQCUSTOMS_CREATED', 5002)
/
insert into cstreqcustoms (reqcustomsid, statuscode_r, partnerid_r) values(-2, 'REQCUSTOMS_CREATED', 5002)
/