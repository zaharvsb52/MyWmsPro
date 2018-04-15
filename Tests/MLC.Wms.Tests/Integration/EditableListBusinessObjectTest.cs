using NUnit.Framework;
using wmsMLC.Business;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration
{
    /*
    [TestFixture]
    public class EditableListBusinessObjectTest
    {
        [SetUp]
        public void Setup()
        {
            BLHelper.InitBL();
        }

        [Test]
        public void EditableListBusinessObjectNewTest()
        {
            var editableList = new EditableListBusinessObject<TSysParam>();
        }

        [Test]
        public void EditableListBusinessObjectInsertRemoveTest()
        {
            var editableList = new EditableListBusinessObject<TSysParam>();
            var p1 = new TSysParam();
            p1.ParamCode = "PARAM_1";
            editableList.Add(p1);
            editableList.Should().NotBeEmpty();
            editableList.Remove(p1);
            editableList.Should().BeEmpty();
        }

        [Test]
        public void EditableListBusinessObjectSetTest()
        {
            var editableList = new EditableListBusinessObject<TSysParam>();
            var p1 = new TSysParam();
            p1.ParamCode = "PARAM_1";

            var p2 = new TSysParam();
            p2.ParamCode = "PARAM_2";

            editableList.Add(p1);
            editableList[0] = p2;
            editableList[0] = p1;
        }

        [Test]
        public void EditableListBusinessObjectAcceptChangesTest()
        {
            var editableList = new EditableListBusinessObject<TSysParam>();
            var p1 = new TSysParam();
            p1.ParamCode = "PARAM_1";
            var p2 = new TSysParam();
            p2.ParamCode = "PARAM_2";

            editableList.Add(p1);
            editableList.Add(p2);
            editableList.IsDirty.Should().Be(true);
            editableList.AcceptChanges();
            editableList.IsDirty.Should().Be(false);
            editableList.Count.Should().Be(2);
        }

        [Test]
        public void EditableListBusinessObjectRejectChangesTest()
        {
            var editableList = new EditableListBusinessObject<TSysParam>();
            var p1 = new TSysParam();
            p1.ParamCode = "PARAM_1";
            var p2 = new TSysParam();
            p2.ParamCode = "PARAM_2";

            editableList.Add(p1);
            editableList.Add(p2);
            editableList.AcceptChanges();

            var p3 = new TSysParam();
            p3.ParamCode = "PARAM_3";
            editableList.Add(p2);
            editableList.IsDirty.Should().Be(true);
            editableList.RejectChanges();
            editableList.IsDirty.Should().Be(false);
            editableList.Count.Should().Be(2);
        }
    }
    */
}