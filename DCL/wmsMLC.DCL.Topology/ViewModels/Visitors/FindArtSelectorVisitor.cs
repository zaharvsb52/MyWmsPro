using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace wmsMLC.DCL.Topology.ViewModels.Visitors
{
    public class FindArtSelectorVisitor : IVisitor
    {
        private readonly string _artNumber;
        public static Brush BrushForFound = new SolidColorBrush(Colors.Yellow);

        public FindArtSelectorVisitor(string artNumber)
        {
            _artNumber = artNumber;

            throw new NotImplementedException("В работе");
        }

        public void Visit(object obj)
        {
            var placeModel = obj as PlaceModel;
            if (placeModel == null)
                return;

        }
    }

    public class FindPlaceVisitor : IVisitor
    {
        private readonly string _placeCode;

        public FindPlaceVisitor(string placeCode)
        {
            _placeCode = placeCode;
            FoundModels = new List<PlaceModel>();
        }

        public void Visit(object obj)
        {
            var placeModel = obj as PlaceModel;
            if (placeModel == null || placeModel.Place == null)
                return;

            if (Equals(_placeCode, placeModel.Place.PlaceCode))
                FoundModels.Add(placeModel);
        }

        public List<PlaceModel> FoundModels { get; private set; }
    }
}