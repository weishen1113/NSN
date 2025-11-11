using Microsoft.AspNetCore.Mvc;
using NSN.Data;
using NSN.Models;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace NSN.Controllers
{
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        public DashboardController(ApplicationDbContext context) => _context = context;

        [HttpGet]
        public IActionResult Index(int? id, bool? clear)
        {
            // Brand click -> start with blank form
            if (clear == true)
            {
                ModelState.Clear();
                ViewBag.Rows = BuildRows();
                return View(new Token());
            }

            var token = id.HasValue
                ? _context.Tokens.FirstOrDefault(t => t.Id == id.Value) ?? new Token()
                : new Token();

            ViewBag.Rows = BuildRows();
            return View(token);
        }

        private List<TokenRowVm> BuildRows()
        {
            var all = _context.Tokens.ToList();
            long total = Math.Max(1, all.Sum(t => (long)t.TotalSupply));
            var rows = all.Select(t => new TokenRowVm
            {
                Id = t.Id,
                Symbol = t.Symbol,
                Name = t.Name,
                ContractAddress = t.ContractAddress,
                TotalHolders = t.TotalHolders,
                TotalSupply = t.TotalSupply,
                Percent = (double)t.TotalSupply / total * 100.0
            })
            .OrderByDescending(r => r.Percent)
            .ToList();

            for (int i = 0; i < rows.Count; i++) rows[i].Rank = i + 1;
            return rows;
        }

        // -------- AJAX endpoints --------

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SaveJson([FromBody] Token token)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { ok = false, message = "Invalid input" });

            if (token.Id == 0)
            {
                // Create
                _context.Tokens.Add(token);
                _context.SaveChanges();
                return Ok(new { ok = true, message = "Token created" });
            }

            // Update (be tolerant if the row was deleted -> create instead)
            var existing = _context.Tokens.FirstOrDefault(t => t.Id == token.Id);
            if (existing == null)
            {
                token.Id = 0;
                _context.Tokens.Add(token);
                _context.SaveChanges();
                return Ok(new { ok = true, message = "Token created" });
            }

            // Copy fields to tracked entity
            existing.Name = token.Name;
            existing.Symbol = token.Symbol;
            existing.ContractAddress = token.ContractAddress;
            existing.TotalHolders = token.TotalHolders;
            existing.TotalSupply = token.TotalSupply;
            existing.Price = token.Price;

            _context.SaveChanges();
            return Ok(new { ok = true, message = "Token updated" });
        }

        [HttpGet]
        public IActionResult DetailsPartial()
        {
            var rows = BuildRows();                   // Or AsNoTracking inside BuildRows
            return PartialView("_ViewDetailsPartial", rows);
        }

        [HttpGet]
        public IActionResult Detail(string id, bool? embed)
        {
            var token = _context.Tokens.AsNoTracking()
                .FirstOrDefault(t => t.Symbol.ToUpper() == id.ToUpper());
            if (token == null) return NotFound();

            var vm = new TokenDetailVm {
                Symbol = token.Symbol,
                Name = token.Name,
                ContractAddress = token.ContractAddress,
                Price = token.Price ?? 0m,
                TotalSupply = token.TotalSupply,
                TotalHolders = token.TotalHolders
            };

            if (embed == true || Request.Headers["X-Requested-With"] == "fetch")
                return PartialView("DetailPartial", vm);

            return View("Detail", vm);
        }

        public IActionResult ChartData()
        {
            var grouped = _context.Tokens
                .GroupBy(t => t.Name)
                .Select(g => new { label = g.Key, value = g.Sum(x => (long)x.TotalSupply) })
                .OrderByDescending(x => x.value)
                .ToList();

            return Json(new
            {
                labels = grouped.Select(x => x.label),
                data = grouped.Select(x => x.value)
            });
        }

        [HttpDelete]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteJson(int id)
        {
            var token = _context.Tokens.FirstOrDefault(t => t.Id == id);
            if (token == null)
                return NotFound(new { ok = false, message = "Token not found" });

            _context.Tokens.Remove(token);
            _context.SaveChanges();
            return Ok(new { ok = true, message = "Token deleted" });
        }
    }
}
