namespace Backend.Models
{
    public class FavoriteAccount
    {
        public int Id { get; set; }

        public int CustomerId { get; set; }

        public string AccountName { get; set; }

        public string IBAN { get; set; }

        public string BankCode { get; set; }

        public string BankName { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }
}
