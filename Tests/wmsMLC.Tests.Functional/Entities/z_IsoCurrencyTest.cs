using System;
using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class IsoCurrencyTest : BaseWMSObjectTest<IsoCurrency>
    {
        protected override void FillRequiredFields(IsoCurrency obj)
        {
            base.FillRequiredFields(obj);

            obj.AsDynamic().CURRENCYCODE = TestString;
            obj.AsDynamic().CURRENCYNAMERUS = TestString;
            //obj.AsDynamic().CURRENCYNUMERIC = TestString;
            obj.AsDynamic().CURRENCYNUMERIC = String.Format("IC{0}", TestString.Substring(Math.Max(0, TestString.Length-1)));
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(CURRENCYCODE = '{0}')", TestString);
        }

        protected override void MakeSimpleChange(IsoCurrency obj)
        {
            obj.AsDynamic().CURRENCYNAMEENG = TestString;
        }

        protected override void CheckSimpleChange(IsoCurrency source, IsoCurrency dest)
        {
            string sourceName = source.AsDynamic().CURRENCYNAMEENG;
            string destName = dest.AsDynamic().CURRENCYNAMEENG;
            sourceName.ShouldBeEquivalentTo(destName);
        }

        public override string TestString
        {
            get
            {
                return base.TestString;
            }
            set
            {
                // Ограничение по длине для CODE - 3
                //base.TestString = "ICT"; ;
                base.TestString = String.Format("{0:000}", TestDecimal); 
            }
        }

    }
}