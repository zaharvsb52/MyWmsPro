using System.Collections.Generic;
using System.Globalization;

namespace wmsMLC.Business.Objects
{
    public class AddressBook : WMSBusinessObject
    {
        #region . Constants .

        public const string ADDRESSBOOKAPARTMENTPropertyName = "ADDRESSBOOKAPARTMENT";
        public const string ADDRESSBOOKBUILDINGPropertyName = "ADDRESSBOOKBUILDING";
        public const string ADDRESSBOOKCITYPropertyName = "ADDRESSBOOKCITY";
        public const string ADDRESSBOOKCOMPLEXPropertyName = "ADDRESSBOOKCOMPLEX";
        public const string ADDRESSBOOKCOUNTRYPropertyName = "ADDRESSBOOKCOUNTRY";
        public const string ADDRESSBOOKDISTRICTPropertyName = "ADDRESSBOOKDISTRICT";
        public const string ADDRESSBOOKIDPropertyName = "ADDRESSBOOKID";
        public const string ADDRESSBOOKINDEXPropertyName = "ADDRESSBOOKINDEX";
        public const string ADDRESSBOOKREGIONPropertyName = "ADDRESSBOOKREGION";
        public const string ADDRESSBOOKSTREETPropertyName = "ADDRESSBOOKSTREET";
        public const string ADDRESSBOOKTYPECODEPropertyName = "ADDRESSBOOKTYPECODE";
        public const string ADDRESSBOOKRAWPropertyName = "ADDRESSBOOKRAW";
        
        #endregion . Constants .

        #region .  Properties  .

        public string ADDRESSBOOKAPARTMENT
        {
            get { return GetProperty<string>(ADDRESSBOOKAPARTMENTPropertyName); }
            set { SetProperty(ADDRESSBOOKAPARTMENTPropertyName, value); }
        }

        public string ADDRESSBOOKBUILDING
        {
            get { return GetProperty<string>(ADDRESSBOOKBUILDINGPropertyName); }
            set { SetProperty(ADDRESSBOOKBUILDINGPropertyName, value); }
        }

        public string ADDRESSBOOKCITY
        {
            get { return GetProperty<string>(ADDRESSBOOKCITYPropertyName); }
            set { SetProperty(ADDRESSBOOKCITYPropertyName, value); }
        }

        public string ADDRESSBOOKCOMPLEX
        {
            get { return GetProperty<string>(ADDRESSBOOKCOMPLEXPropertyName); }
            set { SetProperty(ADDRESSBOOKCOMPLEXPropertyName, value); }
        }

        public string ADDRESSBOOKCOUNTRY
        {
            get { return GetProperty<string>(ADDRESSBOOKCOUNTRYPropertyName); }
            set { SetProperty(ADDRESSBOOKCOUNTRYPropertyName, value); }
        }

        public string ADDRESSBOOKDISTRICT
        {
            get { return GetProperty<string>(ADDRESSBOOKDISTRICTPropertyName); }
            set { SetProperty(ADDRESSBOOKDISTRICTPropertyName, value); }
        }

        public decimal? ADDRESSBOOKID
        {
            get { return GetProperty<decimal?>(ADDRESSBOOKIDPropertyName); }
            set { SetProperty(ADDRESSBOOKIDPropertyName, value); }
        }

        public decimal? ADDRESSBOOKINDEX
        {
            get { return GetProperty<decimal?>(ADDRESSBOOKINDEXPropertyName); }
            set { SetProperty(ADDRESSBOOKINDEXPropertyName, value); }
        }

        public string ADDRESSBOOKREGION
        {
            get { return GetProperty<string>(ADDRESSBOOKREGIONPropertyName); }
            set { SetProperty(ADDRESSBOOKREGIONPropertyName, value); }
        }

        public string ADDRESSBOOKSTREET
        {
            get { return GetProperty<string>(ADDRESSBOOKSTREETPropertyName); }
            set { SetProperty(ADDRESSBOOKSTREETPropertyName, value); }
        }

        public string ADDRESSBOOKTYPECODE
        {
            get { return GetProperty<string>(ADDRESSBOOKTYPECODEPropertyName); }
            set { SetProperty(ADDRESSBOOKTYPECODEPropertyName, value); }
        }

        public string ADDRESSBOOKRAW
        {
            get { return GetProperty<string>(ADDRESSBOOKRAWPropertyName); }
            set { SetProperty(ADDRESSBOOKRAWPropertyName, value); }
        }

        public string ADDRESSBOOKINDEXSTR
        {
            get { return ADDRESSBOOKINDEX.HasValue ? ADDRESSBOOKINDEX.Value.ToString(CultureInfo.InvariantCulture) : null; }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    ADDRESSBOOKINDEX = null;
                    return;
                }

                decimal index;
                if (decimal.TryParse(value, out index))
                {
                    ADDRESSBOOKINDEX = index;
                    return;
                }

                ADDRESSBOOKINDEX = null;
            }
        }

        #endregion .  Properties  .

        #region .  Methods  .
        public override string ToString()
        {
            var result = new List<string>();
            
            if (ADDRESSBOOKINDEX.HasValue)
                result.Add(ADDRESSBOOKINDEX.Value.ToString());

            if (!string.IsNullOrEmpty(ADDRESSBOOKCOUNTRY))
                result.Add(ADDRESSBOOKCOUNTRY);

            if (!string.IsNullOrEmpty(ADDRESSBOOKREGION))
                result.Add(ADDRESSBOOKREGION);

            if (!string.IsNullOrEmpty(ADDRESSBOOKCITY))
                result.Add(ADDRESSBOOKCITY);

            if (!string.IsNullOrEmpty(ADDRESSBOOKSTREET))
                result.Add(ADDRESSBOOKSTREET);

            if (!string.IsNullOrEmpty(ADDRESSBOOKBUILDING))
                result.Add(ADDRESSBOOKBUILDING);

            if (!string.IsNullOrEmpty(ADDRESSBOOKAPARTMENT))
                result.Add(ADDRESSBOOKAPARTMENT);

            if (!string.IsNullOrEmpty(ADDRESSBOOKRAW))
                result.Add(ADDRESSBOOKRAW);

            return string.Join(", ", result);
        }
        #endregion .  Methods  .
    }
}