using System;
using System.Activities.Tracking;
using System.Linq;
using wmsMLC.General.BL;

namespace wmsMLC.Business.Workflow
{
    public class ActivityStackTraceParticipant : TrackingParticipant
    {
        public ActivityStackTraceParticipant(int maxCount)
        {
            MaxCount = maxCount;
        }

        public int MaxCount { get; private set; }
        public ActivityStackTrace TrackingSource { get; set; }

        protected override void Track(TrackingRecord record, TimeSpan timeout)
        {
            if (TrackingSource == null)
                return;

            var faultPropagationRecord = record as FaultPropagationRecord;
            if (faultPropagationRecord != null)
            {
                TrackingSource.FaultHandler = ConvertActivityInfoToInfoOfActivity(faultPropagationRecord.FaultHandler);
                TrackingSource.FaultSource = ConvertActivityInfoToInfoOfActivity(faultPropagationRecord.FaultSource);
                TrackingSource.Fault = faultPropagationRecord.Fault;
            }

            var activityRecord = record as ActivityStateRecord;
            if (activityRecord != null && activityRecord.Activity != null)
            {
                //if (!activityStateRecord.Activity.Name.StartsWith("ConfirmKeyHelp") && activityStateRecord.State == ActivityStates.Executing)
                //if (!activityRecord.Activity.Name.StartsWith("ConfirmKeyHelp") && !activityRecord.Activity.Name.StartsWith("VisualBasicValue"))
                if (!activityRecord.Activity.Name.StartsWith("ConfirmKeyHelp"))
                {
                    if (activityRecord.State == ActivityStates.Faulted)
                    {
                        var info = ConvertActivityInfoToInfoOfActivity(activityRecord.Activity);
                        if (TrackingSource.Activities.All(a => a.CompareTo(info) != 0))
                            AddTrackingRecord(info);
                    }
                    else
                    {
                        AddTrackingRecord(activityRecord.Activity);    
                    }
                }
            }

            var customTrackingRecord = record as CustomTrackingRecord;
            if (customTrackingRecord != null && customTrackingRecord.Data.Count > 0)
            {
                var infoobj = customTrackingRecord.Data.First().Value;
                if (infoobj != null)
                {
                    var lastinfo = TrackingSource.Activities.LastOrDefault();
                    if (lastinfo != null &&
                        lastinfo.CompareTo(ConvertActivityInfoToInfoOfActivity(customTrackingRecord.Activity)) == 0)
                        lastinfo.Info = infoobj.ToString();
                }
            }
        }

        private InfoOfActivity ConvertActivityInfoToInfoOfActivity(ActivityInfo info)
        {
            return info == null
                ? null
                : new InfoOfActivity(name: info.Name, id: info.Id, instanceId: info.InstanceId, typeName: info.TypeName);
        }

        private void AddTrackingRecord(ActivityInfo info)
        {
            if (TrackingSource == null || info == null)
                return;

            AddTrackingRecord(ConvertActivityInfoToInfoOfActivity(info));
        }

        private void AddTrackingRecord(InfoOfActivity info)
        {
            if (TrackingSource == null || info == null)
                return;

            TrackingSource.Activities.Add(info);

            if (MaxCount > 0)
            {
                while (TrackingSource.Activities.Count > MaxCount)
                {
                    TrackingSource.Activities.RemoveAt(0);
                }
            }
        }
    }
}
