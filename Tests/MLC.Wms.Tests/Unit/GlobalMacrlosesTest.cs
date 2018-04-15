using System;
using FluentAssertions;
using NUnit.Framework;
using wmsMLC.General.Types;

namespace MLC.Wms.Tests.Unit
{
    [TestFixture]
    public class GlobalMacrlosesTest
    {
        [Test]
        public void TestDate()
        {
            string _macro = "${DATE}";
            string _instr = "test ";
            DateTime dateTime = DateTime.Now;
            string _expected = _instr + dateTime.ToShortDateString();
            string _outstr = SubstGlobalMacros.Substitute(_instr+_macro);

            _expected.ShouldBeEquivalentTo(_outstr);
        }

        [Test]
        public void TestTime()
        {
            string _macro = "${TIME}";
            string _instr = "test ";
            DateTime dateTime = DateTime.Now;
            string _expected = _instr + dateTime.ToShortTimeString();
            string _outstr = SubstGlobalMacros.Substitute(_instr + _macro);

            _expected.ShouldBeEquivalentTo(_outstr);
        }
    }
}
