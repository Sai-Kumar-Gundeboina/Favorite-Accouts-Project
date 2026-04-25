using Backend.Data;
using Backend.DTO;
using Backend.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/customers")]
    public class CustomerController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CustomerController(AppDbContext context)
        {
            _context = context;
        }

        // CREATE
        [HttpPost]
        public IActionResult Create(CreateCustomerRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (_context.Customers.Any(x => x.Email == request.Email))
                return BadRequest("Email already exists");

            var customer = new Customer
            {
                Name = request.Name,
                Email = request.Email,
                PasswordHash = HashPassword(request.Password)
            };

            _context.Customers.Add(customer);
            _context.SaveChanges();

            return CreatedAtAction(nameof(GetById), new { id = customer.Id }, customer);
        }

        // GET ALL
        [HttpGet]
        public IActionResult GetAll()
        {
            var customers = _context.Customers
                .Select(x => new
                {
                    x.Id,
                    x.Name,
                    x.Email
                })
                .ToList();

            return Ok(customers);
        }

        // GET BY ID
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var customer = _context.Customers
                .Where(x => x.Id == id)
                .Select(x => new
                {
                    x.Id,
                    x.Name,
                    x.Email
                })
                .FirstOrDefault();

            if (customer == null)
                return NotFound("Customer not found");

            return Ok(customer);
        }

        // UPDATE
        [HttpPut("{id}")]
        public IActionResult Update(int id, UpdateCustomerRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var customer = _context.Customers.FirstOrDefault(x => x.Id == id);

            if (customer == null)
                return NotFound("Customer not found");

            customer.Name = request.Name;
            customer.UpdatedAt = DateTime.UtcNow;

            _context.SaveChanges();

            return Ok(customer);
        }

        // DELETE
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var customer = _context.Customers.FirstOrDefault(x => x.Id == id);

            if (customer == null)
                return NotFound("Customer not found");

            _context.Customers.Remove(customer);
            _context.SaveChanges();

            return Ok($"Customer {id} deleted");
        }

        // Password Hashing
        private string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            return Convert.ToBase64String(sha.ComputeHash(bytes));
        }
    }
}
