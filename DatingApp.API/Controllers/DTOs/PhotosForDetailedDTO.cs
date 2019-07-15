using System;

namespace DatingApp.API.Controllers.DTOs
{
    public class PhotosForDetailedDTO
    {
            public int ID { get; set; }
        public string Url { get; set; }
        public string Description { get; set; }
        public DateTime DateAdded { get; set; }
        public bool IsMain { get; set; }
    }
}