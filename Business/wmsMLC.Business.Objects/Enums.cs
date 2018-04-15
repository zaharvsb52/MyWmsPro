namespace wmsMLC.Business.Objects
{
    public enum OutputStatus
    {
        // ReSharper disable InconsistentNaming
        OS_NEW,
        OS_ON_TRANSFER,
        OS_COMPLETED,
        OS_ERROR
        // ReSharper restore InconsistentNaming
    }

    public enum EpsParamType
    {
        /// <summary>
        /// Задача.
        /// </summary>
        // ReSharper disable InconsistentNaming
        TSK,

        /// <summary>
        /// Команда.
        /// </summary>
        EPS,

        /// <summary>
        /// Параметр отчета.
        /// </summary>
        REP
        // ReSharper restore InconsistentNaming
    }

    /// <summary>
    /// Типы параметров задачи сервиса печати и экспорта.
    /// </summary>
    public enum EpsTaskParams
    {
        None,
        // ReSharper disable InconsistentNaming
        Variable, Zip, SupportTargetFolder, FileRecordProtect, FTPServerName,
        SupportFileName, ResultReportFile, PhysicalPrinter, FTPFolder,
        FTPEncodingFile, Copies, FTPServerLogin, FileMask,
        FTPServerPassword, MoveFile, ChangeODBC, Email,
        FTPTransmissionMode, SourceFolder, TargetFolder, CopyFile,
        ReserveCopy, AsAttachment, FileFormat,
        FileExtension,
        SendBlankMail,
        EpsReport,
        Conversion,
        Spacelife,
        BatchPrint,
        WorkflowIdentify,
        MailSubject,
        MailBody,
        MailSignature
        // ReSharper restore InconsistentNaming
    }

    /// <summary>
    /// Типы задач службы печати и экспорта
    /// </summary>
    // ReSharper disable InconsistentNaming
    public enum EpsTaskType
    {
        None,
        OTC_ARCH,
        OTC_DCL,
        OTC_FTP,
        OTC_MAIL,
        OTC_PRINT,
        OTC_SHARE,
    }
    // ReSharper restore InconsistentNaming

    /// <summary>
    /// Типы защиты файлов при записи.
    /// </summary>
    public enum EpsTaskProtect
    {
        None,
        // ReSharper disable InconsistentNaming
        FOLDER,
        EXT,
        CRC
        // ReSharper restore InconsistentNaming
    }

    // статусы приходной накладной
    public enum IWBStates
    {
        IWB_NONE, IWB_CREATED, IWB_CANCELED, IWB_ACTIVATED, IWB_COMPLETED, IWB_ERROR
    }

    public enum IWBPosStates
    {
        IWBPOS_NONE, IWBPOS_CREATED, IWBPOS_CANCELED, IWBPOS_ACTIVATED, IWBPOS_COMPLETED, IWBPOS_ERROR
    }


    // статусы транспортной единицы
    public enum TEStates
    {
        TE_FREE, TE_BUSY, TE_PART_BUSY
    }

    public enum TEPackStatus
    {
        TE_PKG_NONE, TE_PKG_CREATED, TE_PKG_COMPLETED, TE_PKG_ACTIVATED
    }

    // статусы транспортного задания
    public enum TTaskStates
    {
        TTASK_NONE, TTASK_CREATED, TTASK_ACTIVATED, TTASK_RESERVED, TTASK_CANCELED, TTASK_COMPLETED
    }

    /// <summary>
    /// Статусы места.
    /// </summary>
    public enum PlaceStates
    {
        PLC_NONE, PLC_FREE, PLC_RESERV, PLC_BUSY
    }

    /// <summary>
    /// Статусы расходной накладной.
    /// </summary>
    public enum OWBStates
    {
        OWB_PROCESSING, OWB_CREATED, OWB_NONE, OWB_COMPLETED, OWB_CANCELED, OWB_ACTIVATED
    }

    /// <summary>
    /// Статусы позиции расходной накладной.
    /// </summary>
    public enum OWBPosStates
    {
        OWBPOS_CANCELED, OWBPOS_CREATED, OWBPOS_NONE, OWBPOS_COMPLETED, OWBPOS_PROCESSING, OWBPOS_ACTIVATED
    }

    public enum ProductStates
    {
        PRODUCT_BUSY, PRODUCT_FREE, PRODUCT_OUT
    }
    /// <summary>
    /// Системные параметры информации о БД из GPV.
    /// </summary>
    public enum SysDbInfo
    {
        None,
        SysEnvironment,
        SysSite,
        // ReSharper disable InconsistentNaming
        SysDBVersion
        // ReSharper restore InconsistentNaming
    }

    /// <summary>
    /// SegmentType GPV.
    /// </summary>
    public enum SegmentTypeGpv
    {
        None,

        /// <summary>
        /// Количество мест в группе.
        /// </summary>
        SegmentGroupCount,

        /// <summary>
        /// Максимальный вес для группы мест.
        /// </summary>
        SegmentWeightGroup,
    }

    public enum ReportFilterMethod
    {
        ASK, FIND, FINDASK, AUTO
    }

    /// <summary>
    /// Статусы акта
    /// </summary>
    public enum BillWorkActStatus
    {
        WORKACT_CREATED, WORKACT_CALC, WORKACT_COMPLETED
    }

    /// <summary>
    /// Статусы рейсов.
    /// </summary>
    public enum ExternalTrafficStatus
    {
        // ReSharper disable InconsistentNaming
        CAR_PLAN,
        CAR_ONTHEWAY,
        CAR_ARRIVED,
        CAR_TRANSITTERRITORY,
        CAR_DEPARTED
        // ReSharper restore InconsistentNaming
    }

    /// <summary>
    /// Статусы внутренних рейсов.
    /// </summary>
    public enum InternalTrafficStatus
    {
        // ReSharper disable InconsistentNaming

        /// <summary>
        /// Запланирован.
        /// </summary>
        VISITOR_PLAN,

        /// <summary>
        /// Прибыл.
        /// </summary>
        VISITOR_ARRIVED,

        /// <summary>
        /// Убыл.
        /// </summary>
        VISITOR_DEPARTED

        // ReSharper restore InconsistentNaming
    }

    /// <summary>
    /// Статусы работ.
    /// </summary>
    public enum WorkStatus
    {
        // ReSharper disable InconsistentNaming
        WORK_CREATED,
        WORK_COMPLETED
        // ReSharper restore InconsistentNaming
    }

    public enum PMMethods
    {
        MUST_SET,
        MUST_COMPARE,
        MUST_COMPARE_VISIBLE,
        CHECK_PIECE,
        PIECE_ONLY,
        SET_FROM_SKU2TTE,
        MUST_MANUAL_CORRECT,
        SET_NULL
    }
    
    /// <summary>
    /// Типы стратегий менеджера резервирования
    /// </summary>
    public enum MRUseStrategyTypeSysEnum
    {
        WHERE, ORDER, ACTION
    }

    /// <summary>
    /// Стратегии менеджера резервирования
    /// </summary>
    public enum MRUseStrategySysEnum
    {
        TE_FULL, TE_PARTIAL, TE_COMPLETE_MIN, TE_COMPLETE_MAX, SUPPLYAREA, OWNER, RESERVFULLTE,
        COUNT_BEST, COUNT_MAX, COUNT_MIN, COUNT_MAX_PLACE, COUNT_MIN_PLACE, COUNT_RES_MAX, COUNT_RES_MIN,
        FIFO, FEFO, LIFO, LEFO, PLACESORTPICK, MULTIPLICITY_BEST,
        ROUND_IGNORE, ROUND_FIX, ROUND_DOWN, ROUND_UP
    }
    
    /// <summary>
    /// Стратегии размещения менеджера перемещения.
    /// </summary>
    public enum MovingUseStrategySysEnum
    {
        PLACE_FIX,
        PLACE_FIX_LOAD,
        PLACE_FREE,
        PLACE_FREE_LOAD,
        PLACE_TE2TE,
        PLACE_TE2TE_CREATE,
        PLACE_PART_LOAD_ALL,
        PLACE_PART_LOAD_FREE,
        PLACE_PART_LOAD_BUSY,
        PLACE_OWB_FREE,
        PLACE_OWB_NEAR,
        PLACE_OWB_LOAD,
        PLACE_OWB_TE2TE,
        PLACE_ROUTE_FREE,
        PLACE_ROUTE_NEAR,
        PLACE_ROUTE_LOAD,
        PLACE_ROUTE_TE2TE,
        PLACE_ROUTEGATE_FREE,
        PLACE_ROUTEGATE_LOAD,
        PLACE_ROUTEGATE_TE2TE,
        PLACE_ROUTEGATE_TE2TE_CREATE,
        PLACE_ITGATE_FREE,
        PLACE_ITGATE_LOAD,
        PLACE_ITGATE_TE2TE,
        PLACE_ITGATE_TE2TE_CREATE
    }

    /// <summary>
    /// Метод контроля места - Определяет методику, по которой контролируется место при подборе позиции списка пикинга
    /// </summary>
    public enum PickControlMethodSysEnum
    {
        PLPOSPLACECHECKNUMBER,
        PLPOSPLACECHECKNUMBERY,
        PLPOSPLACE,
        PLPOSTE,
        PLPOSNONE
    }

    // Группы 
    public enum InvTypeEnum
    {
        BYPLACE,
        BYGROUP
    }

    public enum PlaceBlockingEnum
    {
        PLACE_BAN,
        PLACE_BAN_IN
    }

    public enum PlaceOperationEnum
    {
        IN,
        OUT
    }

    public enum ObjectTreeMenuType
    {
        DCL,
        WEB
    }

    public enum BillOperationCode
    {
        /// <summary>
        /// Убытие ТС со склада
        /// </summary>
        OP_CAR_DEPARTEDWAREHOUSE,
        /// <summary>
        /// Разгрузка ТС                                                     
        /// </summary>
        OP_CAR_UNLOADING,
        /// <summary>
        /// Выдача накладных по грузу
        /// </summary>
        OP_OPERATOR_CARGOOUTPUT,
        /// <summary>
        /// Перемещение части товара с ТЕ
        /// </summary>
        OP_PART_BUSY,
        /// <summary>
        /// Отмена выдачи накладной
        /// </summary>
        OP_OPERATOR_OWBOUTPUT_CANCEL,
        /// <summary>
        /// Аннуляция накладной
        /// </summary>
        OP_OPERATOR_OWB_CANCEL,
        /// <summary>
        /// Повторное открытие накладной
        /// </summary>
        OP_OPERATOR_IWB_UNCLOSE,
        /// <summary>
        /// Работа с товаром
        /// </summary>
        OP_WITHPRODUCT,
        /// <summary>
        /// Расчет
        /// </summary>             
        OP_BILLCALC,
        /// <summary>
        /// Пересчет
        /// </summary>
        OP_BILLRECALC,
        /// <summary>
        /// Создание короба
        /// </summary>
        OP_PACKING_CREATE,
        /// <summary>
        /// Создание короба на основании
        /// </summary>
        OP_PACKING_CREATE_BASEDON,
        /// <summary>
        /// Упаковано
        /// </summary>
        OP_PACKING_CLOSE_FULL,
        /// <summary>
        /// Полная выдача накладных по грузу
        /// </summary>
        OP_OPERATOR_CARGOOUTPUT_FULL,
        /// <summary>
        /// Завершение подбора накладной
        /// </summary>
        OP_OWB_PICK_END,
        /// <summary>
        /// Открытие короба
        /// </summary>
        OP_PACKING_OPEN,
        /// <summary>
        /// Закрытие списка пикинга с клиента
        /// </summary>
        OP_OPERATOR_PICK_END,
        /// <summary>
        /// Возврат накладной с подбора
        /// </summary>
        OP_OWB_PICK_RETURN,
        /// <summary>
        /// Подбор накладной начат
        /// </summary>
        OP_OWB_PICK_BEGIN,
        /// <summary>
        /// Прерывание обработки списка пикинга
        /// </summary>
        OP_PICK_EXIT,
        /// <summary>
        /// Подбор начат по терминалу
        /// </summary>
        OP_PICK_BEGIN,
        /// <summary>
        /// Закрытие списка пикинга с клиента
        /// </summary>
        OP_PICK_END_MAN,
        /// <summary>
        /// Резервирование накладной
        /// </summary>
        OP_OPERATOR_OWBPROCESSING,
        /// <summary>
        /// Выдача
        /// </summary>
        OP_OUTPUT,
        /// <summary>
        /// Выдача накладной
        /// </summary>
        OP_OPERATOR_OWBOUTPUT,
        /// <summary>
        /// Перемещение ТЕ
        /// </summary>
        OP_MOVE_TE,
        /// <summary>
        /// Активировать ЗНТ
        /// </summary>
        OP_MOVE_ACTIVATED,
        /// <summary>
        /// Корректировка товара
        /// </summary>
        OP_PRODUCT_MANUAL_CORRECT,
        /// <summary>
        /// Полная отмена резервирования
        /// </summary>
        OP_OWB_MANUAL_FULL_CANCELED,
        /// <summary>
        /// Перезапуск поставки
        /// </summary>
        OP_SCH_REOPEN,
        /// <summary>
        /// Требуется упаковка
        /// </summary>
        OP_PACKING_NEED,
        /// <summary>
        /// Пропуск позиции
        /// </summary>
        OP_PICK_MISSED,
        /// <summary>
        /// Подбор по позиции
        /// </summary>
        OP_PICK_POS,
        /// <summary>
        /// Частичное резервирование накладной
        /// </summary>
        OP_OPERATOR_OWBPROCESSING_MAN,
        /// <summary>
        /// ТЕ для подбора заполнено
        /// </summary>
        OP_PICK_TE_COMPLETE,
        /// <summary>
        /// Квитирование ЗНТ
        /// </summary>
        OP_MOVE_CONFIRM,
        /// <summary>
        /// Требуется выдача
        /// </summary>
        OP_OUTPUT_NEED,
        /// <summary>
        /// Отмена ЗНТ вручную
        /// </summary>
        OP_MOVE_CANCEL_MAN,
        /// <summary>
        /// Расщепление товара
        /// </summary>
        OP_PRODUCT_SPLIT,
        /// <summary>
        /// Подбор начат по бумаге
        /// </summary>
        OP_PICK_BEGIN_MAN,
        /// <summary>
        /// Требуется пикинг
        /// </summary>
        OP_PICK_NEED,
        /// <summary>
        /// Принудительное завершение позиции
        /// </summary>
        OP_PICKPOS_COMPLETED_MAN,
        /// <summary>
        /// Квитирование ЗНТ по ЧП
        /// </summary>
        OP_MOVE_PART_CONFIRM,
        /// <summary>
        /// Подбор по терминалу
        /// </summary>
        OP_PICKING_TERM,
        /// <summary>
        /// Упаковка товара
        /// </summary>
        OP_PRODUCT_PACKING,
        /// <summary>
        /// Постановка ТС на ворота
        /// </summary>
        OP_CAR_ARRIVEDWAREHOUSE,
        /// <summary>
        /// Отмена резервирования по позиции
        /// </summary>
        OP_OWBPOS_MANUAL_CANCELED,
        /// <summary>
        /// Осмотр ТС - подготовка к разгрузке
        /// </summary>
        OP_CAR_INSPECT,
        /// <summary>
        /// Отмена резервирования по товару
        /// </summary>
        OP_PRODUCT_MANUAL_CANCELED,
        /// <summary>
        /// Закрытие очереди
        /// </summary>
        OP_QSCH_COMPLETED,
        /// <summary>
        /// Закрытие короба
        /// </summary>
        OP_PACKING_CLOSE,
        /// <summary>
        /// Закрытие акта
        /// </summary>
        OP_BILLWORKACT_COMPLETED,
        /// <summary>
        /// Новая поставка готова
        /// </summary>
        OP_SCH_CREATE,
        /// <summary>
        /// Закрытие ком. акта
        /// </summary>
        OP_BILLWORKACT_CLOSE,
        /// <summary>
        /// повторное открытие ком. акта
        /// </summary>
        OP_BILLWORKACT_UNCLOSE,
        /// <summary>
        /// Создание инвентаризации
        /// </summary>
        OP_INV_CREATE,
        /// <summary>
        /// Зафиксировать инвентаризацию
        /// </summary>
        OP_INV_FIXINV,
        /// <summary>
        /// Оператор закрывает всю накладную
        /// </summary>
        OP_OPERATOR_IWB_CLOSE,
        /// <summary>
        /// Корректировка стока по инвентаризации
        /// </summary>
        OP_INV_PRD_CORRECT,
        /// <summary>
        /// Зафиксировать просчет
        /// </summary>
        OP_INV_FIXCALC,
        /// <summary>
        /// Поиск расхождений
        /// </summary>
        OP_INV_FINDDIFF,
        /// <summary>
        /// Полная отгрузка ТЕ по терминалу
        /// </summary>
        OP_TERM_TE_OUTPUT_FULL,
        /// <summary>
        /// Расфиксировать инвентаризацию
        /// </summary>
        OP_INV_UNFIXINV,
        /// <summary>
        /// Закрытие ком. акта
        /// </summary>
        OP_COMMACT_CLOSE,
        /// <summary>
        /// Повторное открытие ком. акта
        /// </summary>
        OP_COMMACT_UNCLOSE,
        /// <summary>
        /// Отгрузка ТЕ по терминалу
        /// </summary>
        OP_TERM_TE_OUTPUT,
        /// <summary>
        /// Поставка частично завершена
        /// </summary>
        OP_SCH_PART_COMPLETED,
        /// <summary>
        /// Создание поставки
        /// </summary>
        OP_SCH_CREATING,
        /// <summary>
        /// Создание ошибочной поставки
        /// </summary>
        OP_SCH_ERROR,
        /// <summary>
        /// Создание очереди
        /// </summary>
        OP_QSCH_CREATE,
        /// <summary>
        /// Поставка завершена
        /// </summary>
        OP_SCH_COMPLETED,
        /// <summary>
        /// Закрытие ком. акта(old)
        /// </summary>
        OP_PRODUCT_COMMACT_CLOSE,
        /// <summary>
        /// Повторное открытие ком. акта(old)
        /// </summary>
        OP_PRODUCT_COMMACT_UNCLOSE,
        /// <summary>
        /// Подбор по позиции части товара
        /// </summary>
        OP_PICK_POS_PART,
        /// <summary>
        /// Открытие короба ТСД
        /// </summary>
        OP_PACKING_OPEN_TERM,
        /// <summary>
        /// Уборка территории
        /// </summary>
        OP_CLEANAREA,
        /// <summary>
        /// Создание короба ТСД
        /// </summary>
        OP_PACKING_CREATE_TERM,
        /// <summary>
        /// Закрытие короба ТСД
        /// </summary>
        OP_PACKING_CLOSE_TERM,
        /// <summary>
        /// Упаковка товара ТСД
        /// </summary>
        OP_PRODUCT_PACKING_TERM,
        /// <summary>
        /// Ручное закрытие работы
        /// </summary>
        OP_WORK_CLOSE_MAN,
        /// <summary>
        /// Ручное открытие работы
        /// </summary>
        OP_WORK_OPEN_MAN,
        /// <summary>
        /// Корректировка СТН
        /// </summary>
        OP_WTV_CORRECT,
        /// <summary>
        /// Подготовка инвентаризационных данных
        /// </summary>
        OP_INVREQ_PREPARE,
        /// <summary>
        /// Дозагрузка товара
        /// </summary>
        OP_PRODUCT_PART_LOAD,
        /// <summary>
        /// Отмена приемки
        /// </summary>
        OP_IWBACCEPT_MAN_CANCELED_FULL,
        /// <summary>
        /// Частичная отмена приемки
        /// </summary>
        OP_IWBACCEPT_MAN_CANCELED,
        /// <summary>
        /// Частичная отмена приемки с терминала
        /// </summary>
        OP_IWBACCEPT_MAN_CANCELED_TERM,
        /// <summary>
        /// Очистка инвентаризации
        /// </summary>
        OP_INV_CLEAR,
        /// <summary>
        /// Приемка товара
        /// </summary>
        OP_INPUT_PRD,
        /// <summary>
        /// Приемка объекта хранения
        /// </summary>
        OP_INPUT_SO,
        /// <summary>
        /// Приемка товара по ОХ
        /// </summary>
        OP_INPUT_SO_PRD,
        /// <summary>
        /// Регистрация приемки товара
        /// </summary>
        OP_INPUT_REG,
        /// <summary>
        /// Регистрация закрытия списка пикинга с клиента.
        /// </summary>
        OP_PICK_END_MAN_REG,
        /// <summary>
        /// Смена владельца товара
        /// </summary>
        OP_PRODUCT_CHANGE_OWNER_MAN
    }

    public enum InternalTrafficPurposeVisitType
    {
        /// <summary>
        /// Не определена.
        /// </summary>
        Unknown,

        /// <summary>
        /// Разгрузка.
        /// </summary>
        Unloading,

        /// <summary>
        /// Получение груза.
        /// </summary>
	    Loading,
	    
        /// <summary>
        /// Работа.
        /// </summary>
        Applicant, 

        /// <summary>
        /// Почта.
        /// </summary>
	    Courier,

        /// <summary>
        /// Оформление документов.
        /// </summary>
	    DocumentExecution, 	

        /// <summary>
        /// Переговоры.
        /// </summary>
	    Negotiations,	
	
        /// <summary>
        /// Инвентаризация.
        /// </summary>
	    Inventory
    }
}
