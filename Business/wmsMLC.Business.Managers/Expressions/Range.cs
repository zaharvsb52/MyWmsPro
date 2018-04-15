namespace wmsMLC.Business.Managers.Expressions
{
    /// <summary>
    /// Структура, описывающая диапазон значений
    /// </summary>
    public struct Range
    {
        public Range(int start, int end)
        {
            Start = start;
            End = end;
        }

        public int Start;
        public int End;
    }
}