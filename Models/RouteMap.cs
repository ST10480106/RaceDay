namespace RaceDay.Models
{//start of namespace

    //maps to the Route table (named RouteMap so it does not clash with the [Route] attribute)
    public class RouteMap
    {//start of class

        public int RouteId { get; set; }

        public int CategoryId { get; set; }

        public string MapFileUrl { get; set; } = string.Empty;

    }//end of class

}//end of namespace