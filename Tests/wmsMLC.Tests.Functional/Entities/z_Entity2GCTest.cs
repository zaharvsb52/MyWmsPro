using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class Entity2GCTest : BaseWMSObjectTest<Entity2GC>
    {
        private readonly GCTest _gcTest = new GCTest();

        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _gcTest };
        }


        protected override void FillRequiredFields(Entity2GC obj)
        {
            base.FillRequiredFields(obj);

            var gc = _gcTest.CreateNew();

            obj.AsDynamic().ENTITY2GCID = TestDecimal;
            obj.AsDynamic().ENTITY2GCENTITY = TestString;
            obj.AsDynamic().ENTITY2GCGCCODE = gc.GetKey();
            obj.AsDynamic().ENTITY2GCKEY = TestDecimal;
            obj.AsDynamic().ENTITY2GCATTR = TestString;
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(ENTITY2GCENTITY = '{0}')", TestString);
        }

        protected override void MakeSimpleChange(Entity2GC obj)
        {
            obj.AsDynamic().ENTITY2GCENTITY = TestString;
        }

        protected override void CheckSimpleChange(Entity2GC source, Entity2GC dest)
        {
            string sourceName = source.AsDynamic().ENTITY2GCENTITY;
            string destName = dest.AsDynamic().ENTITY2GCENTITY;
            sourceName.ShouldBeEquivalentTo(destName);
        }

    }
}