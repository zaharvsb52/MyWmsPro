using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class AddressBookTest : BaseEntityTest<AddressBook>
    {
        [Test, Ignore("����� CRUD'���� ����� ��������-��������, �������� �������")]
        public override void Entity_should_be_create_read_update_delete() { }
    }
}