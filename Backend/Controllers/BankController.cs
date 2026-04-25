using Microsoft.AspNetCore.Mvc;
using Backend.Data;
using Backend.Models;
using Backend.DTO;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/banks")]
    public class BankController : ControllerBase
    {
        private readonly AppDbContext _context;

        public BankController(AppDbContext context)
        {
            _context = context;
        }

        // Get All Banks
        [HttpGet]
        public IActionResult GetAll()
        {
            var banks = _context.BankLookups.ToList();
            return Ok(banks);
        }

        // Get Bank by Code
        [HttpGet("{code}")]
        public IActionResult GetByCode(string code)
        {
            var bank = _context.BankLookups
                .FirstOrDefault(b => b.Code == code);

            if (bank == null)
                return NotFound($"Bank not found for code: {code}");

            return Ok(bank);
        }
        [HttpPost]
        public IActionResult Create(BankLookup request)
        {
            if (string.IsNullOrWhiteSpace(request.Code))
                return BadRequest("Bank code is required");

            if (string.IsNullOrWhiteSpace(request.BankName))
                return BadRequest("Bank name is required");

            var exists = _context.BankLookups.Any(b => b.Code == request.Code);
            if (exists)
                return BadRequest("Bank code already exists");

            _context.BankLookups.Add(request);
            _context.SaveChanges();

            return CreatedAtAction(nameof(GetByCode), new { code = request.Code }, request);
        }

        // UPDATE BANK
        [HttpPut("{code}")]
        public IActionResult Update(string code, UpdateBankRequest request)
        {
            var bank = _context.BankLookups.FirstOrDefault(b => b.Code == code);

            if (bank == null)
                return NotFound($"Bank not found for code: {code}");

            bank.BankName = request.BankName;

            _context.SaveChanges();

            return Ok(bank);
        }

        // DELETE BANK
        [HttpDelete("{code}")]
        public IActionResult Delete(string code)
        {
            var bank = _context.BankLookups.FirstOrDefault(b => b.Code == code);

            if (bank == null)
                return NotFound($"Bank not found for code: {code}");

            _context.BankLookups.Remove(bank);
            _context.SaveChanges();

            return Ok($"Bank with code {code} deleted");
        }
    }
}
