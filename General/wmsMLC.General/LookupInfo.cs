using System;

namespace wmsMLC.General
{
    public sealed class LookupInfo
    {
        public LookupInfo(string codeEditor, string valueMember, string displayMember, string filter, decimal? fetchRowCount, string itemSource, decimal objectLookupSimple)
        {
            CodeEditor = codeEditor;
            ValueMember = valueMember;
            DisplayMember = displayMember;
            Filter = filter;
            FetchRowCount = fetchRowCount;
            ItemSource = itemSource;
            LookUpType = objectLookupSimple.To(LookupType.Default);
        }

        public string CodeEditor { get; private set; }
        public string ValueMember { get; private set; }
        public string DisplayMember { get; private set; }
        public string Filter { get; set; }
        public decimal? FetchRowCount { get; private set; }
        public string ItemSource { get; private set; }
        public LookupType LookUpType { get; set; }
        
        /// <summary>
        /// Тип объекта.
        /// </summary>
        public Type ItemType { get; set; }

        /// <summary>
        /// Шаблон получения Lookup-а.
        /// </summary>
        public string AttrEntity { get; set; }
    }

    public enum LookupType
    {
        Default = 0,
        Combobox = 1
        //, LookupOpt = 2
    }
}
