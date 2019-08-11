namespace DatingApp.API.Helpers
{
    public class MessageParams
    {
        private const int MaxSizePage = 50;
        public int PageNumber { get; set; } = 1;
        private int pageSize = 10;
        public int PageSize
        {
            get { return pageSize; }
            set { pageSize = (value > MaxSizePage) ? MaxSizePage : value; }
        }

        public int UserID { get; set; }
        public string MessageContainer { get; set; } = "Unread";
        

    }
}