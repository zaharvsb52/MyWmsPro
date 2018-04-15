namespace wmsMLC.General.Services.Telegrams
{
    public interface ITelegramWrapper
    {
        /// <summary> Извлечь транспортную телеграмму </summary>
        /// <returns>Телеграмма для передачи</returns>
        Telegram UnWrap();

        /// <summary> Заполнить текущую телеграмму по ответной </summary>
        /// <param name="telegram">ответная телеграмма, с которой нужно "забрать" ответ</param>
        void FillBy(Telegram telegram);
    }
}