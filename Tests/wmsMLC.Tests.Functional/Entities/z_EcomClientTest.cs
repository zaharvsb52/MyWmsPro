using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class EcomClientTest : BaseWMSObjectTest<EcomClient>
    {
        private readonly MandantTest _mandantTest = new MandantTest();
        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _mandantTest };
        }

        protected override void FillRequiredFields(EcomClient obj)
        {
            base.FillRequiredFields(obj);

            obj.AsDynamic().CLIENTID = TestDecimal;
            obj.AsDynamic().MANDANTID = _mandantTest.CreateNew().GetKey();
            obj.AsDynamic().ClientName = TestString;
            obj.AsDynamic().ClientMiddleName = TestString;
            obj.AsDynamic().ClientPhoneMobile = TestString;
            obj.AsDynamic().ClientPhoneWork = TestString;
            obj.AsDynamic().ClientPhoneInternal = TestString;
            obj.AsDynamic().ClientPhoneHome = TestString;
            obj.AsDynamic().ClientEmail = TestString;
            obj.AsDynamic().ClientHostRef = TestString;
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(CLIENTID = '{0}')", TestDecimal);
        }

        protected override void MakeSimpleChange(EcomClient obj)
        {
            obj.AsDynamic().ClientLastName = TestString;
        }

        protected override void CheckSimpleChange(EcomClient source, EcomClient dest)
        {
            string sourceName = source.AsDynamic().ClientLastName;
            string destName = dest.AsDynamic().ClientLastName;
            sourceName.ShouldBeEquivalentTo(destName);
        }

        [Test, Ignore]
        public void DeleteAddress()
        {
            var mgr = CreateManager();
            var client = CreateNew();
            var key = client.GetKey();

            // ������ ������� ��������� ��������
            var address = new AddressBook();

            try
            {
                #region .  Create  .

                var oldLength = 0;

                // ��������� ��������� ��������� ���������
                var addressLst = client.AsDynamic().ADDRESS as IList<AddressBook>;
                if (addressLst == null)
                {
                    addressLst = new WMSBusinessCollection<AddressBook>();
                    client.SetProperty("ADDRESS", addressLst);
                }
                else oldLength = addressLst.Count;

                // ��������� ������ � ������������ ���� ��������� ��������
                address.AsDynamic().ADDRESSBOOKINDEX = TestDecimal;
                address.AsDynamic().ADDRESSBOOKTYPECODE = "ADR_LEGAL";

                // ��������� ����� � ��������� ���������, ���������
                addressLst.Add(address);
                mgr.Update(client);

                // ������ �� �� �� �����
                client = mgr.Get(key);
                var addressLstNew = client.AsDynamic().ADDRESS as IList<AddressBook>;

                // �������� ��������
                addressLstNew.Should().NotBeNull("������ ���� �������� ���� �� 1 �������");
                addressLstNew.Count.ShouldBeEquivalentTo(oldLength + 1, "Manager ������ ��������� ��������� ��������");

                #endregion

                #region .  Delete  .

                // ������ ����� � ��������� ���������, ���������
                addressLst = client.AsDynamic().ADDRESS as IList<AddressBook>;
                address = addressLst[addressLst.Count - 1];
                addressLst.Remove(address);

                mgr.Update(client);

                // ����������, ��� ��������� �������
                client = mgr.Get(key);
                addressLstNew = client.AsDynamic().ADDRESS as IList<AddressBook>;
                if (addressLstNew == null) oldLength.Equals(0);
                else addressLstNew.Count.ShouldBeEquivalentTo(oldLength, "Manager ������ ������� ��������� ��������");

                #endregion
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + " ������� ������ �� ������� WMSADDRESSBOOK � ADDRESSBOOKINDEX = " + TestDecimal);
            }
            finally
            {
                ClearForSelf();
            }
        }
    }
}