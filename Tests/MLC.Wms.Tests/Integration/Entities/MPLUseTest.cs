using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class MPLUseTest : BaseEntityTest<MPLUse>
    {
        protected override void FillRequiredFields(MPLUse entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.MPLCode_r = MPLTest.ExistsItem1Code;
            obj.MPLUseByOWB = TestBool;
            obj.MPLUseByArt = TestBool;
            obj.MPLUseByArtGroup = TestBool;
            obj.MPLUseWeight = TestDecimal;
            obj.MPLUseVolume = TestDecimal;
            obj.MPLUseBySegment = TestBool;
            obj.MPLUseLine = TestDecimal;
            obj.MPLUseWait = TestBool;
            obj.MPLUseByMotionArea = TestBool;
            obj.MPLUsePickControlMethod = TestString;
        }
    }
}