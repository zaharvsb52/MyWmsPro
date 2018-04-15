using System;

namespace wmsMLC.General
{
    /// <summary>
    /// ����� ��� ������ � "������" �� MS. ������ �������� �� ������������ member ���� ������ ��������� ������ ����
    /// </summary>
    public abstract class RealyAllowMultipleAttribute : Attribute
    {
        /// <summary>
        /// ���������� ������������� ���������.
        /// <remarks>
        /// � ������� ������ �� ����� GetType(). ��� ������������� ������ ������� � TypeDescriptor ����� ������� ������ ���� ��������
        /// ������� ����, ��� ���� ���������� �� ����������.
        /// link: http://social.msdn.microsoft.com/Forums/en-US/winforms/thread/e6bb4146-eb1a-4c1b-a5b1-f3528d8a7864/
        /// </remarks>
        /// </summary>
        public override object TypeId
        {
            get
            {
                return Guid.NewGuid();
            }
        }
    }
}