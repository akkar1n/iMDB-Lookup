namespace ImdbLookup
{
    public class iMDB
    {
        private string rating;
        private string votes;
        private string year;

        public iMDB(
            string mainlink, 
            string country, 
            string genre, 
            string language, 
            string openweekend, 
            string rating, 
            string title, 
            string votes, 
            string year,
            int runtime)
        {
            MainLink = mainlink;
            Country = country;
            Genre = genre;
            Language = language;
            OpenWeekend = openweekend;
            Title = title;
            this.rating = rating;
            this.votes = votes;
            this.year = year;
            Runtime = runtime;
        }

        public string MainLink
        { get; set; }


        public string Country
        { get; set; }


        public string Genre
        { get; set; }


        public string Language
        { get; set; }


        public string OpenWeekend
        { get; set; }


        public string Title
        { get; set; }


        public int Runtime
        { get; set; }


        public double Rating
        {
            get
            {
                if (rating != string.Empty)
                    return double.Parse(rating);
                
                return -1;
            }
        }


        public double Votes
        {
            get
            {
                if (votes != string.Empty)
                    return double.Parse(votes);

                return -1;
            }
        }


        public double Year
        {
            get
            {
                if (year != string.Empty)
                    return double.Parse(year);
                
                return -1;
            }
        }
    }
}
