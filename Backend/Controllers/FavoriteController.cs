using Backend.Data;
using Backend.DTO;
using Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Backend.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/favorites")]
    public class FavoriteController : ControllerBase
    {
        private readonly AppDbContext _context;

        public FavoriteController(AppDbContext context)
        {
            _context = context;
        }

        // ADD
        [HttpPost]
        public IActionResult Add(CreateFavoriteRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var customerId = GetCustomerId()
                ;
            //checking whether customer id is valid or not
            var customerExists = _context.Customers
        .Any(c => c.Id == customerId);

            if (!customerExists)
                return BadRequest("Invalid CustomerId");

            //checking for duplicate IBAN number
            var exists = _context.FavoriteAccounts
    .Any(x => x.CustomerId == customerId && x.IBAN == request.IBAN);

            if (exists)
                return BadRequest("This IBAN is already added for another customer");

            if (request.IBAN.Length < 8)
                return BadRequest("Invalid IBAN format");

            var bankCode = request.IBAN.Substring(4, 4);
            var bank = _context.BankLookups.FirstOrDefault(b => b.Code == bankCode);

            if (bank == null)
                return BadRequest("Bank not found");

            // max 20 accounts check
            var count = _context.FavoriteAccounts
                .Count(x => x.CustomerId == customerId);

            if (count >= 20)
                return BadRequest("Max 20 favorite accounts allowed");

            var entity = new FavoriteAccount
            {
                CustomerId = customerId,
                AccountName = request.AccountName,
                IBAN = request.IBAN,
                BankCode = bankCode,
                BankName = bank.BankName
            };

            _context.FavoriteAccounts.Add(entity);
            _context.SaveChanges();

            return CreatedAtAction(nameof(Get), new { id = entity.Id }, entity);
        }

        // GET (with pagination)
        [HttpGet]
        public IActionResult Get(int page = 1, int pageSize = 5)
        {
            var customerId = GetCustomerId();
            var query = _context.FavoriteAccounts
                .Where(x => x.CustomerId == customerId);

            var total = query.Count();

            var data = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return Ok(new { total, data });
        }

        // UPDATE
        [HttpPut("{id}")]
        public IActionResult Update(int id, UpdateFavoriteRequest request)
        {
            if (request == null)
                return BadRequest("Request body is required");

            var customerId = GetCustomerId();

            var entity = _context.FavoriteAccounts
                .FirstOrDefault(x => x.Id == id && x.CustomerId == customerId);

            if (entity == null)
                return NotFound("Favorite not found");

            if (!string.IsNullOrWhiteSpace(request.AccountName))
            {
                entity.AccountName = request.AccountName;
            }
            
            if (!string.IsNullOrWhiteSpace(request.IBAN))
            {
                if (request.IBAN.Length < 8)
                    return BadRequest("Invalid IBAN");

                var bankCode = request.IBAN.Substring(4, 4);

                var bank = _context.BankLookups
                    .FirstOrDefault(b => b.Code == bankCode);

                if (bank == null)
                    return BadRequest("Invalid IBAN");

                // duplicate check
                var exists = _context.FavoriteAccounts.Any(x =>
                    x.CustomerId == customerId &&
                    x.IBAN == request.IBAN &&
                    x.Id != id);

                if (exists)
                    return BadRequest("IBAN already exists");

                entity.IBAN = request.IBAN;
                entity.BankCode = bankCode;
                entity.BankName = bank.BankName;
            }

            entity.UpdatedAt = DateTime.UtcNow;

            _context.SaveChanges();

            return Ok(entity);
        }

        // DELETE
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var customerId = GetCustomerId();

            var entity = _context.FavoriteAccounts
                .FirstOrDefault(x => x.Id == id && x.CustomerId == customerId);
            if (entity == null) return NotFound();

            _context.FavoriteAccounts.Remove(entity);
            _context.SaveChanges();

            return Ok();
        }

        [HttpGet("check-iban")]
        public IActionResult CheckIban(string iban)
        {
            if (string.IsNullOrWhiteSpace(iban))
                return BadRequest("IBAN is required");

            var customerId = GetCustomerId();

            // Check customer exists
            var customerExists = _context.Customers
                .Any(c => c.Id == customerId);

            if (!customerExists)
                return BadRequest("Invalid CustomerId");

            // Check duplicate IBAN
            var exists = _context.FavoriteAccounts
                .Any(x => x.CustomerId == customerId && x.IBAN == iban);

            return Ok(new
            {
                customerId,
                iban,
                isDuplicate = exists
            });

        }
        private int GetCustomerId()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.Parse(userId);
        }
    }
}