using System;
using System.Collections.Generic;
using NUnit.Framework;
using wmsMLC.Business.Managers;

namespace MLC.Wms.Tests.Unit
{
    [TestFixture]
    public class PlaceCheckHashTest
    {
        [Test]
        public void DuplicationTest()
        {
            const string placeTemplate = "CGF{0:D3}{1:D3}000";
            const int maxX = 500;
            const int maxY = 3;
            const int maxValue = 1000;
            const int windowSize = 2;

            var hashList = new int[maxX, maxY];
            var checkSet = new HashSet<int>();
            for (int x = 0; x < maxX; x++)
            {
                for (int y = 0; y < maxY; y++)
                {
                    var placeText = string.Format(placeTemplate, x, y);
                    var hash = PlaceManager.CheckHash(placeText, maxValue);
                    hashList[x, y] = hash;

                    // пытаемся добавить новый хэш в окно проверки
                    if (!checkSet.Add(hash))
                        throw new Exception("Уже есть элемент с хэшем = " + hash);
                }

                // подчищаем уже не нужные
                // пока полагаем, что по Y вообще не должно быть совпадений
                if (x >= windowSize)
                {
                    for (int y = 0; y < maxY; y++)
                    {
                        checkSet.Remove(hashList[x - windowSize, y]);
                    }
                }
            }
        }

        [Test]
        public void OverflowTest()
        {
            var maxValue = 1000;
            var PlaceCodeMaxLength =
                "PlaceCheckHashTest99PlaceCheckHashTest99PlaceCheckHashTest99PlaceCheckHashTest99PlaceCheckHashTest99PlaceCheckHashTest99";
            var SegmentCodeMaxLength = "SegmentCode1SegmentCode2SegmentCode3SegmentCode4SegmentCode5";
            var PlaceXMaxLength = int.MaxValue.ToString();
            var hash1 = PlaceManager.CheckHash(PlaceCodeMaxLength, maxValue);
            var hash2 = PlaceManager.CheckHash(SegmentCodeMaxLength + PlaceXMaxLength, maxValue);
        }

    }

}