using System.ComponentModel.DataAnnotations;

namespace Backend.DTO
{
    public class UpdateFavoriteRequest
    {
        [RegularExpression(@"^[a-zA-Z0-9' -]+$", ErrorMessage = "Invalid name")]
        public string? AccountName { get; set; }

        [MaxLength(20)]
        [RegularExpression(@"^[a-zA-Z0-9]+$", ErrorMessage = "Invalid IBAN")]
        public string? IBAN { get; set; }
    }
}
