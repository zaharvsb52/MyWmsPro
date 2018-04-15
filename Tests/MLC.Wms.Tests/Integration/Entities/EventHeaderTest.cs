using System;
using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Managers;
using wmsMLC.Business.Objects;
using wmsMLC.General;
using wmsMLC.General.DAL;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class EventHeaderTest : BaseEntityTest<EventHeader>
    {
        public const decimal ExistsItem1Id = -1;

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = EventHeader.InstancePropertyName;
        }

        protected override void FillRequiredFields(EventHeader entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.MANDANTID = TstMandantId;
            obj.OPERATIONCODE_R = BillOperationTest.ExistsItem1Code;
            obj.EVENTKINDCODE_R = EventKindTest.ExistsItem2Code;
            obj.EVENTHEADERINSTANCE = TestString;
            obj.EVENTHEADERSTARTTIME = DateTime.Now;
        }

        protected void FillRequiredDetailFields(EventDetail detail)
        {
            //base.FillRequiredFields(entity); //�� ���������, ID ��������� � �� ����������
            dynamic obj = detail.AsDynamic();

            obj.COMMACTID_R = CommActTest.ExistsItem1Id;
        }

        [Test]
        public override void Entity_should_be_create_read_update_delete()
        {
            var uowFactory = IoC.Instance.Resolve<IUnitOfWorkFactory>();

            using (var uow = uowFactory.Create())
            using (var mgr = IoC.Instance.Resolve<EventHeaderManager>())
            {
                mgr.SetUnitOfWork(uow);
                uow.BeginChanges();

                try
                {
                    // ������� ����� ���������
                    var obj = mgr.New();
                    obj.Should().NotBeNull("Manager ������ ����� ��������� ����� ��������� �������");
                    var detail = new EventDetail();

                    // ������� ��� ������ ��� �������� ������� �
                    // ��������� ������������ ����
                    FillRequiredFields(obj);
                    FillRequiredDetailFields(detail);

                    // ��������� � ��
                    mgr.RegEvent(ref obj, detail);

                    // ����������, ��� ���� ����������
                    var key = ((IKeyHandler)obj).GetKey();
                    key.Should().NotBeNull("� ������������ ������� ���� ������ ���� ��������");

                    // ���������
                    if (MakeSimpleChange(obj))
                    {
                        mgr.Update(obj);

                        // ������ �� �� �� �����
                        var updated = mgr.Get((decimal)key);
                        updated.Should().NotBeNull("�� ������ ����� �������� ������ �� �����");

                        // ���������� �����
                        var insKey = ((IKeyHandler)updated).GetKey();
                        insKey.Should()
                            .NotBeNull("���� ����������� ������ ������ ��������������� ����� �����������")
                            .And.Be(key, "����� ������ ���������");

                        // ���������, ��� ��������� ������
                        CheckSimpleChange(obj, updated);

                        // todo: other simple change (for locking check)
                    }

                    // ������� �� ���������, ������� ���� �� �������� �� �����
                    // �������
                    //mgr.Delete(obj);
                    // �������� ����� ���� ����������
                    //mgr.ClearCache();
                    // ����������, ��� ��������� �������
                    //var deleted = mgr.Get((decimal)key);
                    //deleted.Should().BeNull("��������� ������ ������ �������� �� ��");
                }
                finally
                {
                    // ���������� �����-���� ��������� (����� �� ��������)
                    uow.RollbackChanges();
                }
            }
        }
    }
}