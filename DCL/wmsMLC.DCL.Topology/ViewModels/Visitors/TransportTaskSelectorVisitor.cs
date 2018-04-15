using System;
using System.Collections.Generic;
using wmsMLC.Business.Objects;

namespace wmsMLC.DCL.Topology.ViewModels.Visitors
{
//    public class TransportTaskSelectorVisitor : IVisitor
//    {
//        private readonly IEnumerable<TransportTask> _tasks;
//
//        public TransportTaskSelectorVisitor(IEnumerable<TransportTask> tasks)
//        {
//            _tasks = tasks;
//        }
//
//        public void Visit(object obj)
//        {
//            var placeModel = obj as PlaceModel;
//            if (placeModel == null || placeModel.Source == null)
//                return;
//
//            // сбрасываем
//            if (_tasks == null)
//            {
//                placeModel.ToDefaultFill();
//                return;
//            }
//
//            placeModel.Source.
//
//            var state = (PlaceStates)Enum.Parse(typeof(PlaceStates), placeModel.Source.StatusCode_R);
//            switch (state)
//            {
//                case PlaceStates.PLC_BUSY:
//                    placeModel.Fill = BusyFill;
//                    break;
//                case PlaceStates.PLC_RESERV:
//                    placeModel.Fill = ReserveFill;
//                    break;
//                default:
//                    placeModel.Fill.Opacity = DefaultOpacity;
//                    break;
//            }
//        }
//    }
}