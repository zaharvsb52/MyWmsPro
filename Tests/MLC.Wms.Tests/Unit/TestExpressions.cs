using System;
using System.Dynamic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Managers.Expressions;

namespace MLC.Wms.Tests.Unit
{
    [TestFixture]
    public class TestExpressions
    {
        [Test, Ignore("Временно")]
        public void CommonTest()
        {
            dynamic eo = new ExpandoObject();
            eo.A = "#{r1;1-2}";
            eo.B = "text #{1-2} for ${r1} where c=${c}";
            eo.C = "#{1-2}";
            var cl = new TestClass { C = "8" };

            var items = ExpressionHelper.Process(cl, new ExpressionContext(eo));
        }

        [Test]
        public void VoidTest()
        {
            dynamic eo = new ExpandoObject();
            var obj = new TestClass { A = "1", B = "2", C = "3" };
            var items = ExpressionHelper.Process(obj, new ExpressionContext(eo));

            items.Should().HaveCount(1);
            items.First().ShouldBeEquivalentTo(obj);
        }

        [Test,Ignore("Временно")]
        public void DependencyTest()
        {
            dynamic eo = new ExpandoObject();
            eo.A = "${b}${c}";
            eo.B = "${c}";
            eo.C = "1";

            var obj = new TestClass { A = "1", B = "2", C = "3" };
            var items = ExpressionHelper.Process(obj, new ExpressionContext(eo));

            items.Should().HaveCount(1);
            var item = items.First();
            item.A.ShouldBeEquivalentTo("11");
            item.B.ShouldBeEquivalentTo("1");
            item.C.ShouldBeEquivalentTo("1");
        }

        [Test, ExpectedException(typeof(CyclicDependencyException))]
        public void CircularDependencyTest()
        {
            dynamic eo = new ExpandoObject();
            eo.A = "${b}";
            eo.B = "${c}";
            eo.C = "${a}";

            var obj = new TestClass { A = "1", B = "2", C = "3" };
            var items = ExpressionHelper.Process(obj, new ExpressionContext(eo));

            items.Should().HaveCount(1);
            var item = items.First();
            item.A.ShouldBeEquivalentTo("11");
            item.B.ShouldBeEquivalentTo("1");
            item.C.ShouldBeEquivalentTo("1");
        }
    }

    public class TestClass : ICloneable
    {
        public string A { get; set; }
        public string B { get; set; }
        public string C { get; set; }

        public object Clone()
        {
            var clone = new TestClass();
            clone.A = A;
            clone.B = B;
            clone.C = C;
            return clone;
        }
    }

}