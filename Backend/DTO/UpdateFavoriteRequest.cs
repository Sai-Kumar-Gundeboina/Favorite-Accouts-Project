using System.ComponentModel.DataAnnotations;

namespace Backend.DTO
{
    public class UpdateFavoriteRequest
    {
        [Required]
        [RegularExpression(@"^[a-zA-Z0-9'\- ]+$")]
        public string AccountName { get; set; }

        [Required]
        [MaxLength(20)]
        [RegularExpression(@"^[a-zA-Z0-9]+$")]
        public string IBAN { get; set; }
    }
}
