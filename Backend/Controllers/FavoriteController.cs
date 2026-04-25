using Backend.Data;
using Backend.DTO;
using Backend.Models;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{

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

            //checking whether customer id is valid or not
            var customerExists = _context.Customers
        .Any(c => c.Id == request.CustomerId);

            if (!customerExists)
                return BadRequest("Invalid CustomerId");

            //checking for duplicate IBAN number
            var exists = _context.FavoriteAccounts
    .Any(x => x.CustomerId == request.CustomerId && x.IBAN == request.IBAN);

            if (exists)
                return BadRequest("This IBAN is already added for this customer");

            if (request.IBAN.Length < 8)
                return BadRequest("Invalid IBAN format");

            var bankCode = request.IBAN.Substring(4, 4);
            var bank = _context.BankLookups.FirstOrDefault(b => b.Code == bankCode);

            if (bank == null)
                return BadRequest("Bank not found");

            // max 20 accounts check
            var count = _context.FavoriteAccounts
                .Count(x => x.CustomerId == request.CustomerId);

            if (count >= 20)
                return BadRequest("Max 20 favorite accounts allowed");

            var entity = new FavoriteAccount
            {
                CustomerId = request.CustomerId,
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
        public IActionResult Get(int customerId, int page = 1, int pageSize = 5)
        {
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
            var entity = _context.FavoriteAccounts.Find(id);
            if (entity == null) return NotFound();

            var bankCode = request.IBAN.Substring(4, 4);
            var bank = _context.BankLookups.FirstOrDefault(b => b.Code == bankCode);

            if (bank == null)
                return BadRequest("Invalid IBAN");

            entity.AccountName = request.AccountName;
            entity.IBAN = request.IBAN;
            entity.BankCode = bankCode;
            entity.BankName = bank.BankName;
            entity.UpdatedAt = DateTime.UtcNow;

            _context.SaveChanges();

            return Ok(entity);
        }

        // DELETE
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var entity = _context.FavoriteAccounts.Find(id);
            if (entity == null) return NotFound();

            _context.FavoriteAccounts.Remove(entity);
            _context.SaveChanges();

            return Ok();
        }

        [HttpGet("check-iban")]
        public IActionResult CheckIban(int customerId, string iban)
        {
            if (string.IsNullOrWhiteSpace(iban))
                return BadRequest("IBAN is required");

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
    }
}