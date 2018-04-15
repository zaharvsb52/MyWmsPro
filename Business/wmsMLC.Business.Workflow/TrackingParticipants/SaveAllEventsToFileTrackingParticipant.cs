using System;
using System.Activities.Tracking;
using System.IO;
using wmsMLC.Business.Workflow.Helpers;

namespace wmsMLC.Business.Workflow.TrackingParticipants
{
    public class SaveAllEventsToFileTrackingParticipant : TrackingParticipant
    {
        protected override void Track(TrackingRecord record, TimeSpan timeout)
        {
            // get the tracking path
            var fileName = IoHelper.GetTrackingFilePath(record.InstanceId);

            // create a writer and open the file
            using (var tw = File.AppendText(fileName))
            {
                // write a line of text to the file
                tw.WriteLine(record.ToString());
            }
        }
    }
}
