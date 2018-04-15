using System;
using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class GlobalParamValueTest : BaseWMSObjectTest<GlobalParamValue>
    {
        private readonly GlobalParamTest _globalParamTest = new GlobalParamTest();

        protected override void FillRequiredFields(GlobalParamValue obj)
        {
            base.FillRequiredFields(obj);

            var globalParam = _globalParamTest.CreateNew();
            obj.AsDynamic().GLOBALPARAMCODE_R = globalParam.GetKey();
            obj.AsDynamic().GPARAMVALKEY = TestString + "Key";
            obj.AsDynamic().GPARAMVALVALUE = TestString + "Value";
            obj.AsDynamic().GPARAMVAL2ENTITY = TestString + "ENTITY";
        }

        protected override void MakeSimpleChange(GlobalParamValue obj)
        {
            obj.AsDynamic().GPARAMVALKEY = TestString + "0002";
        }

        protected override void CheckSimpleChange(GlobalParamValue source, GlobalParamValue dest)
        {
            string sourceName = source.AsDynamic().GPARAMVALKEY;
            string destName = dest.AsDynamic().GPARAMVALKEY;
            sourceName.ShouldBeEquivalentTo(destName);
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(upper(GLOBALPARAMCODE_R) like upper('{0}%'))", AutoTestMagicWord);
        }

        public override void ClearForSelf()
        {
            base.ClearForSelf();
            _globalParamTest.ClearForSelf();
        }

        [Test,Ignore("Пока не работает")]
        public void DeleteByParentMandant()
        {
            // создаём мэнэджеры, тесты и сущности
            var _gpvMgr = CreateManager();
            var _gpv = CreateNew();
            var _gpvKey = _gpv.GetKey();
            var _parentTest = new MandantTest();
            var _parentMgr = CreateManager<Mandant>();
            var _parent = _parentTest.CreateNew();
            var _parentKey = _parent.GetKey();

            try
            {
                // привязываем GPV к сущности
                _gpv.AsDynamic().GPARAMVAL2ENTITY = "MANDANT";
                _gpv.AsDynamic().GPARAMVALKEY = _parent.GetKey();

                // проверяем что правильно привязали
                _gpvMgr.Update(_gpv);
                var _oldGpv = _gpvMgr.Get(_gpvKey);
                _oldGpv.GetKey().Equals(_gpvKey);

                // удаляем родителя, проверяем что правильно удалили
                _parentMgr.Delete(_parent);
                var deleted = _parentMgr.Get(_parentKey);
                deleted.Should().BeNull("Не смогли удалить {0}", typeof(Worker));
                
                var _newGpv = _gpvMgr.Get(_gpvKey);
                _newGpv.Should().BeNull("Вложеная сущность должна удаляться при удалении основной");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                ClearForSelf();
                _parentTest.ClearForSelf();
            }
        }
        [Test, Ignore("Пока не работает")]
        public void DeleteByParentPartner()
        {
            // создаём мэнэджеры, тесты и сущности
            var _gpvMgr = CreateManager();
            var _gpv = CreateNew();
            var _gpvKey = _gpv.GetKey();
            var _parentTest = new PartnerTest();
            var _parentMgr = CreateManager<Partner>();
            var _parent = _parentTest.CreateNew();
            var _parentKey = _parent.GetKey();

            try
            {
                // привязываем GPV к сущности
                _gpv.AsDynamic().GPARAMVAL2ENTITY = "PARTNER";
                _gpv.AsDynamic().GPARAMVALKEY = _parent.GetKey();

                // проверяем что правильно привязали
                _gpvMgr.Update(_gpv);
                var _oldGpv = _gpvMgr.Get(_gpvKey);
                _oldGpv.GetKey().Equals(_gpvKey);

                // удаляем родителя, проверяем что правильно удалили
                _parentMgr.Delete(_parent);
                var deleted = _parentMgr.Get(_parentKey);
                deleted.Should().BeNull("Не смогли удалить {0}", typeof(Worker));
                
                var _newGpv = _gpvMgr.Get(_gpvKey);
                _newGpv.Should().BeNull("Вложеная сущность должна удаляться при удалении основной");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                ClearForSelf();
                _parentTest.ClearForSelf();
            }
        }
        [Test, Ignore("Пока не работает")]
        public void DeleteByParentTEType()
        {
            // создаём мэнэджеры, тесты и сущности
            var _gpvMgr = CreateManager();
            var _gpv = CreateNew();
            var _gpvKey = _gpv.GetKey();
            var _parentTest = new TETypeTest();
            var _parentMgr = CreateManager<TEType>();
            var _parent = _parentTest.CreateNew();
            var _parentKey = _parent.GetKey();

            try
            {
                // привязываем GPV к сущности
                _gpv.AsDynamic().GPARAMVAL2ENTITY = "TETYPE";
                _gpv.AsDynamic().GPARAMVALKEY = _parent.GetKey();

                // проверяем что правильно привязали
                _gpvMgr.Update(_gpv);
                var _oldGpv = _gpvMgr.Get(_gpvKey);
                _oldGpv.GetKey().Equals(_gpvKey);

                // удаляем родителя, проверяем что правильно удалили
                _parentMgr.Delete(_parent);
                var deleted = _parentMgr.Get(_parentKey);
                deleted.Should().BeNull("Не смогли удалить {0}", typeof(Worker));
                
                var _newGpv = _gpvMgr.Get(_gpvKey);
                _newGpv.Should().BeNull("Вложеная сущность должна удаляться при удалении основной");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                ClearForSelf();
                _parentTest.ClearForSelf();
            }
        }
        [Test, Ignore("Пока не работает")]
        public void DeleteByParentWorker()
        {
            // создаём мэнэджеры, тесты и сущности
            var _gpvMgr = CreateManager();
            var _gpv = CreateNew();
            var _gpvKey = _gpv.GetKey();
            var _parentMgr = CreateManager<Worker>();
            var _parentTest = new WorkerTest();
            var _parent = _parentTest.CreateNew();
            var _parentKey = _parent.GetKey();

            try
            {
                // привязываем GPV к сущности
                _gpv.AsDynamic().GPARAMVAL2ENTITY = "WORKER";
                _gpv.AsDynamic().GPARAMVALKEY = _parentKey;

                // проверяем что правильно привязали
                _gpvMgr.Update(_gpv);
                var _oldGpv = _gpvMgr.Get(_gpvKey);
                _oldGpv.GetKey().Equals(_gpvKey);

                // удаляем родителя, проверяем что правильно удалили
                _parentMgr.Delete(_parent);
                var deleted = _parentMgr.Get(_parentKey);
                deleted.Should().BeNull("Не смогли удалить {0}",typeof(Worker));
                
                var _newGpv = _gpvMgr.Get(_gpvKey);
                _newGpv.Should().BeNull("Вложеная сущность должна удаляться при удалении основной");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                ClearForSelf();
                _parentTest.ClearForSelf();
            }
        }
        [Test, Ignore("Пока не работает")]
        public void DeleteByParentSegmentType()
        {
            // создаём мэнэджеры, тесты и сущности
            var _gpvMgr = CreateManager();
            var _gpv = CreateNew();
            var _gpvKey = _gpv.GetKey();
            var _parentTest = new SegmentTypeTest();
            var _parentMgr = CreateManager<SegmentType>();
            var _parent = _parentTest.CreateNew();
            var _parentKey = _parent.GetKey();

            try
            {
                // привязываем GPV к сущности
                _gpv.AsDynamic().GPARAMVAL2ENTITY = "SEGMENTTYPE";
                _gpv.AsDynamic().GPARAMVALKEY = _parent.GetKey();

                // проверяем что правильно привязали
                _gpvMgr.Update(_gpv);
                var _oldGpv = _gpvMgr.Get(_gpvKey);
                _oldGpv.GetKey().Equals(_gpvKey);

                // удаляем родителя, проверяем что правильно удалили
                _parentMgr.Delete(_parent);
                var deleted = _parentMgr.Get(_parentKey);
                deleted.Should().BeNull("Не смогли удалить {0}", typeof(Worker));
                
                var _newGpv = _gpvMgr.Get(_gpvKey);
                _newGpv.Should().BeNull("Вложеная сущность должна удаляться при удалении основной");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                ClearForSelf();
                _parentTest.ClearForSelf();
            }
        }
        [Test, Ignore("Пока не работает")]
        public void DeleteByParentClientType()
        {
            // создаём мэнэджеры, тесты и сущности
            var _gpvMgr = CreateManager();
            var _gpv = CreateNew();
            var _gpvKey = _gpv.GetKey();
            var _parentTest = new ClientTypeTest();
            var _parentMgr = CreateManager<ClientType>();
            var _parent = _parentTest.CreateNew();
            var _parentKey = _parent.GetKey();

            try
            {
                // привязываем GPV к сущности
                _gpv.AsDynamic().GPARAMVAL2ENTITY = "CLIENTTYPE";
                _gpv.AsDynamic().GPARAMVALKEY = _parent.GetKey();

                // проверяем что правильно привязали
                _gpvMgr.Update(_gpv);
                var _oldGpv = _gpvMgr.Get(_gpvKey);
                _oldGpv.GetKey().Equals(_gpvKey);

                // удаляем родителя, проверяем что правильно удалили
                _parentMgr.Delete(_parent);
                var deleted = _parentMgr.Get(_parentKey);
                deleted.Should().BeNull("Не смогли удалить {0}", typeof(Worker));
                
                var _newGpv = _gpvMgr.Get(_gpvKey);
                _newGpv.Should().BeNull("Вложеная сущность должна удаляться при удалении основной");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                ClearForSelf();
                _parentTest.ClearForSelf();
            }


        }

        [Test, Ignore("Нет хистори")]
        public override void ManagerGetHistoryTest()
        {
        }
    }
}
