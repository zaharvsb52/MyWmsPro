using System;
using System.Activities;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using wmsMLC.Activities.General;
using wmsMLC.Business.Managers;
using wmsMLC.Business.Objects;
using wmsMLC.General;

namespace wmsMLC.Activities.Business
{
    /// <summary>
    /// Активити регистрации событий в системе
    /// </summary>
    public class RegEvent : NativeActivity<EventHeader>
    {
        public RegEvent()
        {
            Parameters = new Dictionary<string, Argument>();
        }

        #region . Arguments .

        [Required]
        [DisplayName(@"Тип события")]
        public InArgument<string> EventKindCode { get; set; }

        [Required]
        [DisplayName(@"Код Операции")]
        public InArgument<string> BillOperationCode { get; set; }

        [DisplayName(@"Код Бизнес-процесса")]
        [Description(@"Если не указан, будет предпинята попытка автоматического определения")]
        public InArgument<string> BPProcessCode { get; set; }

        [DisplayName(@"Id манданта")]
        public InArgument<decimal?> MandantId { get; set; }

        [DisplayName(@"Дата начала")]
        [Description(@"Если не указана, будет подставлена текущая дата")]
        public InArgument<DateTime?> StartDate { get; set; }

        [DisplayName(@"Дата окончания")]
        public InArgument<DateTime?> EndDate { get; set; }

        [DisplayName(@"Параметры метода")]
        public Dictionary<string, Argument> Parameters { get; private set; }

        #endregion

        protected override void Execute(NativeActivityContext context)
        {
            #region .  Fill Header  .

            var eventHeader = new EventHeader
            {
                EventKindCode = EventKindCode.Get(context),
                OperationCode = BillOperationCode.Get(context),
                ProcessCode = BPProcessCode.Get(context)
            };
            var startDate = StartDate.Get(context);
            eventHeader.StartTime = startDate.HasValue ? startDate.Value : DateTime.Now;
            eventHeader.EndTime = EndDate.Get(context);
            eventHeader.MandantID = MandantId.Get<decimal?>(context);

            // авто-заполняемые поля
            eventHeader.Instance = context.WorkflowInstanceId.ToString();

            #endregion .  Fill Header  .

            #region .  Fill Params for Details  .

            var evDetail = new EventDetail();
            foreach (var parameter in Parameters)
            {
                var value = parameter.Value.Get(context);
                // пустые значение незачем передавать
                if (value == null)
                    continue;

                if (evDetail.ContainsProperty(parameter.Key))
                    evDetail.SetProperty(parameter.Key, value);
            }

            #endregion .  Fill Params for Details  .

            var eventHeaderMgr = IoC.Instance.Resolve<IEventHeaderManager>();
            var uw = BeginTransactionActivity.GetUnitOfWork(context);
            if (uw != null)
                eventHeaderMgr.SetUnitOfWork(uw);
            eventHeaderMgr.RegEvent(ref eventHeader, evDetail);
            context.SetValue(Result, eventHeader);
        }
    }
}