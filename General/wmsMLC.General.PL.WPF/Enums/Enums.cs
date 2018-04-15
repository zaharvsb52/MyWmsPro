using System;

namespace wmsMLC.General.PL.WPF.Enums
{
    [Flags]
    public enum ListItemStyle
    {
        None,
        ForegroundRed = 0x00001,
        ForegroundWhite = 0x00010,
        BackgroundRed = 0x00100,
        FontWeightBold = 0x01000,
    }

    public enum SystemSoundEnum
    {
        /// <summary>
        /// No sound.
        /// </summary>
        None,

        /// <summary>
        /// Information sound.
        /// </summary>
        Asterisk,

        /// <summary>
        /// Default beep.
        /// </summary>
        Beep,

        /// <summary>
        /// Warning sound.
        /// </summary>
        Exclamation,

        /// <summary>
        /// Critical stop-error sound.
        /// </summary>
        Hand,

        /// <summary>
        /// Question sound.
        /// </summary>
        Question
    }
}
