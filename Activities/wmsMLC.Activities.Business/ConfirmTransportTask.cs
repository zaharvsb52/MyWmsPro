using System;
using System.Activities;
using System.ComponentModel;
using System.Linq;
using wmsMLC.Business.Objects;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.DAL;

namespace wmsMLC.Activities.Business
{
    [Obsolete("Не использовать. Есть номральное API")]
    class ConfirmTransportTask : NativeActivity
    {
        private const string PlaceStatusCode = "PLC_FREE";
        private const string NextPlaceStatusCode = "PLC_BUSY";        
        private const string TEStatusCode = "TE_FREE";

        [DisplayName(@"Объект ЗНТ")]
        public InArgument<TransportTask> TransportTaskObject { get; set; }

        public ConfirmTransportTask()
        {
            this.DisplayName = @"Подтверждение транспортного задания";
        }

        protected override void Execute(NativeActivityContext context)
        {            
            var uowFactory = IoC.Instance.Resolve<IUnitOfWorkFactory>();
            using (var uow = uowFactory.Create(false))
            {
                try
                {
                    uow.BeginChanges();
                    var ttask = TransportTaskObject.Get(context);
                    var placeManager = IoC.Instance.Resolve<IBaseManager<Place>>();
                    placeManager.SetUnitOfWork(uow, false);

                    var currentPlace = placeManager.Get(ttask.TaskCurrentPlace);
                    if (currentPlace == null)
                        throw new DeveloperException("Место {0} не существует", ttask.TaskCurrentPlace);

                    // Место, с которого перемещали, освобождается
                    // placecapacity = placecapacity + 1, но не больше placecapacitymax
                    var capacity = currentPlace.PlaceCapacity + 1;
                    if (currentPlace.PlaceCapacityMax >= capacity)
                        currentPlace.PlaceCapacity = capacity;
                    else
                        throw new DeveloperException("Вместимость не может быть больше максимальной {0}",
                                                     currentPlace.PlaceCapacityMax);
                    // Если на месте еще есть ТЕ (placecapacity<placecapacitymax), то статус места не меняется
                    // и туда не существует других ЗНТ
                    var ttaskManager = IoC.Instance.Resolve<IBaseManager<TransportTask>>();
                    ttaskManager.SetUnitOfWork(uow, false);
                    var filterForPlace = string.Format("((TTaskNextPlace='{0}' OR TTaskFinishPlace='{0}') AND (STATUSCODE_R='{1}'))",
                                                       currentPlace.GetKey(), TTaskStates.TTASK_CREATED);
                    var taskForPlace = ttaskManager.GetFiltered(filterForPlace);
                    if (!taskForPlace.Any() && (currentPlace.PlaceCapacity < currentPlace.PlaceCapacityMax))
                    {
                        currentPlace.StatusCode_R = PlaceStatusCode;
                    }
                    placeManager.Update(currentPlace);

                    // ТЕ, которое перевозили, меняет статус на "TE_FREE"
                    var teManager = IoC.Instance.Resolve<IBaseManager<TE>>();
                    teManager.SetUnitOfWork(uow, false);
                    var te = teManager.Get(ttask.TECode);
                    if (te == null)
                        throw new DeveloperException("ТЕ {0} не существует", ttask.TECode);
                    te.StatusCode = TEStatusCode;
                    // Текущее место (tecurrentplace) меняется на новое
                    te.CurrentPlace = ttask.TaskNextPlace;
                    teManager.Update(te);

                    // Место, на которое перемещали, меняет статус на "PLC_BUSY"
                    var nextPlace = placeManager.Get(ttask.TaskNextPlace);
                    if (nextPlace == null)
                        throw new DeveloperException("Место {0} не существует", ttask.TaskNextPlace);
                    nextPlace.StatusCode_R = NextPlaceStatusCode;
                    var nextCapacity = nextPlace.PlaceCapacity - 1;
                    if (nextPlace.PlaceCapacity < 0)
                        throw new DeveloperException("Вместимость не может быть меньше нуля");
                    nextPlace.PlaceCapacity = nextCapacity;
                    placeManager.Update(nextPlace);

                    ttask.StatusCode = TTaskStates.TTASK_COMPLETED;
                    ttask.ClientCode = WMSEnvironment.Instance.ClientCode;
                    ttaskManager.Update(ttask);

                    uow.CommitChanges();
                }
                catch
                {
                    uow.RollbackChanges();
                    throw;
                }
            }
        }
    }
}
