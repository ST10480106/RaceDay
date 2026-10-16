namespace RaceDay.Models
{//start of namespace

    public class EnrolmentView
    {
        public int EnrolmentId { get; set; }
        public string ParticipantId { get; set; } = "";
        public string ParticipantName { get; set; } = "";
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = "";
        public int EventId { get; set; }
        public string EventName { get; set; } = "";
        public string Status { get; set; } = "";
        public DateTime EnrolledTime { get; set; }
    }

    public class ResultView
    {
        public int ResultId { get; set; }
        public int EnrolmentId { get; set; }
        public string ParticipantId { get; set; } = "";
        public string ParticipantName { get; set; } = "";
        public int EventId { get; set; }
        public string EventName { get; set; } = "";
        public string OrganiserId { get; set; } = "";
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = "";
        public TimeSpan? FinishTime { get; set; }
        public int? Position { get; set; }
        public string CapturedBy { get; set; } = "";
    }

}//end of namespace