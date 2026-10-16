namespace RaceDay.Models
{//start of namespace

    public class Result
    {//start of class

        public int ResultId { get; set; }

        public int EnrolmentId { get; set; }

        public string CapturedBy { get; set; } = string.Empty;

        public TimeSpan? FinishTime { get; set; }

        public int? Position { get; set; }

    }//end of class

}//end of namespace