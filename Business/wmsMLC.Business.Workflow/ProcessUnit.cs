using System;

namespace wmsMLC.Business.Workflow
{
    [Serializable]
    public class ProcessUnit
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }        
        public DateTime CreationDate { get; set; }        
        public DateTime CompletionDate { get; set; }
        public string Status { get; set; }
        public Guid ActivityId { get; set; }

        public ProcessUnit()
        {            
            this.CreationDate = DateTime.Now;
            this.Status = "active";
        }
                
        // returns true if this process is finished
        public bool IsFinished()
        {
            return this.Status.Equals("finished");
        }
    }
}
