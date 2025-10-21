using System.ComponentModel.DataAnnotations;


namespace VisitTracker.Models.DTO
{
    public class AddWebsiteModel 
    {
        [Required, MaxLength(50)]
        public string Name { get; set; } = string.Empty;
    }

    public class RemoveWebsiteModel
    {
        [Required, MaxLength(50)]
        public string Name { get; set; } = string.Empty;
    }
}
