using System.ComponentModel.DataAnnotations;

namespace RaceDay.Models
{//start of namespace

    public class Category
    {//start of class

        public int CategoryId { get; set; }

        public int EventId { get; set; }

        [Required]
        [StringLength(100)]
        public string CategoryName { get; set; } = string.Empty;

        public decimal Distance { get; set; }

    }//end of class

}//end of namespace