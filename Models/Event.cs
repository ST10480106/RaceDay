using System.ComponentModel.DataAnnotations;

namespace RaceDay.Models
{//start of namespace

    public class Event
    {//start of class

        public int EventId { get; set; }

        public string OrganiserId { get; set; } = string.Empty;

        [Required]
        [StringLength(150)]
        public string EventName { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        public DateOnly EventDate { get; set; }

        [Required]
        [StringLength(150)]
        public string Location { get; set; } = string.Empty;

        //Run, Walk or Cycle
        public string EventType { get; set; } = "Run";

    }//end of class

}//end of namespace