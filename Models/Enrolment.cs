namespace RaceDay.Models
{//start of namespace

    public class Enrolment
    {//start of class

        public int EnrolmentId { get; set; }

        public string ParticipantId { get; set; } = string.Empty;

        public int CategoryId { get; set; }

        //Pending, Confirmed or Cancelled
        public string Status { get; set; } = "Pending";

        public DateTime EnrolledTime { get; set; } = DateTime.Now;

    }//end of class

}//end of namespace