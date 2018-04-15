using System.Activities;
using System.Collections.ObjectModel;
using System.ComponentModel;
using wmsMLC.Activities.General.Helpers;
using wmsMLC.General;
using wmsMLC.General.PL.WPF.Enums;
using wmsMLC.General.PL.WPF.Helpers;

namespace wmsMLC.Activities.Dialogs.Activities
{
     [DisplayName(@"Воспроизведение системных звуков")]
    public class SystemSoundPlayActivity : NativeActivity
    {
         public SystemSoundPlayActivity()
         {
             DisplayName = "Воспроизведение системных звуков";
         }

         [DisplayName(@"Системный звук")]
         public InArgument<SystemSoundEnum> SystemSound { get; set; }

         protected override void CacheMetadata(NativeActivityMetadata metadata)
         {
             var collection = new Collection<RuntimeArgument>();
             var type = GetType();

             ActivityHelpers.AddCacheMetadata(collection, metadata, SystemSound, type.ExtractPropertyName(() => SystemSound));

             metadata.SetArgumentsCollection(collection);
         }

        protected override void Execute(NativeActivityContext context)
        {
            var systemSound = SystemSound.Get(context);
            var soundHelper = new SoundHelper();
            soundHelper.PlaySystemSound(systemSound);
        }
    }
}
