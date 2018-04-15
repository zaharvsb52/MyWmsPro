using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class InvTaskTest : BaseWMSObjectTest<InvTask>
    {
        private readonly InvTaskStepTest _invTaskStepTest =  new InvTaskStepTest();
        private readonly SKUTest _skuTest = new SKUTest();
        private readonly InvTaskGroupTest _invTaskGroupTest = new InvTaskGroupTest();
        private readonly PlaceTest _placeTest = new PlaceTest();

        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _invTaskStepTest, _skuTest, _invTaskGroupTest, _placeTest };
        }

        protected override void FillRequiredFields(InvTask obj)
        {
            base.FillRequiredFields(obj);

            
            var sku = _skuTest.CreateNew();
            _invTaskGroupTest.TestDecimal = TestDecimal + 1;
            _invTaskGroupTest.TestString = TestString + "1";
            _invTaskGroupTest.TestGuid = new Guid("22222222222222222222222222222222");
            var invTaskGroup = _invTaskGroupTest.CreateNew();
            _invTaskStepTest.TestString = TestString + "2";
            _invTaskStepTest.TestDecimal = TestDecimal + 2;
            var invTaskStep = _invTaskStepTest.CreateNew();

            _placeTest.TestString = TestString + "3";
            _placeTest.TestDecimal = TestDecimal + 3;
            var place = _placeTest.CreateNew();

            obj.AsDynamic().INVTASKID = TestDecimal;
            obj.AsDynamic().INVTASKGROUPID_R = invTaskGroup.GetKey();
            obj.AsDynamic().INVTASKSTEPID_R = invTaskStep.GetKey();
            obj.AsDynamic().INVTASKNUMBER = TestDecimal;
            obj.AsDynamic().INVTASKMANUAL = true;
            obj.AsDynamic().INVTASKCOUNT2SKU = TestDecimal;
            obj.AsDynamic().SKUID_R = sku.GetKey();
            obj.AsDynamic().PLACECODE_R = place.GetKey();

        }

        protected override string GetCheckFilter()
        {
            return string.Format("(INVTASKID = '{0}')", TestDecimal);
        }

        protected override void MakeSimpleChange(InvTask obj)
        {
            obj.AsDynamic().INVTASKCOUNT = TestDecimal;
        }

        protected override void CheckSimpleChange(InvTask source, InvTask dest)
        {
            decimal sourceName = source.AsDynamic().INVTASKCOUNT;
            decimal destName = dest.AsDynamic().INVTASKCOUNT;
            sourceName.ShouldBeEquivalentTo(destName);
        }
    }
}