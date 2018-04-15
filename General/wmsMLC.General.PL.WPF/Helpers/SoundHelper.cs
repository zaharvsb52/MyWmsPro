using System.Media;
using wmsMLC.General.PL.WPF.Enums;

namespace wmsMLC.General.PL.WPF.Helpers
{
    //http://stackoverflow.com/questions/72167/how-to-play-a-standard-windows-sound
    public class SoundHelper
    {
        public SystemSound GetSystemSound(SystemSoundEnum systemSoundEnum)
        {
            switch (systemSoundEnum)
            {
                case SystemSoundEnum.None:
                    return null;
                case SystemSoundEnum.Asterisk:
                    return SystemSounds.Asterisk;
                case SystemSoundEnum.Beep:
                    return SystemSounds.Beep;
                case SystemSoundEnum.Exclamation:
                    return SystemSounds.Exclamation;
                case SystemSoundEnum.Hand:
                    return SystemSounds.Hand;
                case SystemSoundEnum.Question:
                    return SystemSounds.Question;
                default:
                    throw new DeveloperException("SystemSound is Undefined for 'SystemSoundEnum.{0}'.", systemSoundEnum.ToString());
            }
        }

        public void PlaySystemSound(SystemSoundEnum systemSoundEnum)
        {
           var sound = GetSystemSound(systemSoundEnum);
           if (sound != null)
               sound.Play();
        }
    }
}
