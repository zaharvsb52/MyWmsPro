using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using NUnit.Framework;
using wmsMLC.Business.Objects;
using wmsMLC.General;
using wmsMLC.General.BL;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using BLToolkit.Aspects;
using FluentAssertions;
using wmsMLC.Business;
using wmsMLC.Business.Managers;
using wmsMLC.General.DAL;



namespace wmsMLC.Tests.Functional
{
    [TestFixture]
    class TriggerFilterTest
    {
        [Test]
        public void GetFilter()
        {
            var textSerializer = new Serialize.Linq.Serializers.JsonSerializer();
            var expressionSerializer = new Serialize.Linq.Serializers.ExpressionSerializer(textSerializer);
            Expression<Func<object, bool>> exp = i => ((IWB) i).StatusCode == "IWB_COMPLETED";
            var strExpr = expressionSerializer.SerializeText(exp);

            var desExpr = (Expression<Func<object, bool>>) expressionSerializer.DeserializeText(strExpr);
            var compileDesExpr = desExpr.Compile();

            BLHelper.InitBL(dalType: DALType.Oracle);
            var mgr = IoC.Instance.Resolve<IBaseManager<IWB>>();
            var items = mgr.GetFiltered("STATUSCODE_R = 'IWB_COMPLETED' and ROWNUM < 2");
            items.Should().NotBeEmpty("Ничего не получили, нет смысла проверять дальше");
            var res = items.Where(compileDesExpr);
            res.Should().HaveCount(items.Count());
        }
    }
}
