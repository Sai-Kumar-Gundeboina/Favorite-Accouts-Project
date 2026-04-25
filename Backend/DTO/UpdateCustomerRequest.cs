using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Backend.DTO
{

    public class UpdateCustomerRequest
    {
        [Required]
        public string Name { get; set; }
    }

}
