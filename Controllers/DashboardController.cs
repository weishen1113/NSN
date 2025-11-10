using Microsoft.AspNetCore.Mvc;
using NSN.Data;
using NSN.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NSN.Controllers
{
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index(int? id)
        {
            // 1️⃣ Form model
            var token = id.HasValue
                ? _context.Tokens.FirstOrDefault(t => t.Id == id.Value) ?? new Token()
                : new Token();

            // 2️⃣ Chart data
            var grouped = _context.Tokens
                .GroupBy(t => t.Name)
                .Select(g => new { Name = g.Key, SumSupply = g.Sum(x => (long)x.TotalSupply) })
                .OrderByDescending(x => x.SumSupply)
                .ToList();

            ViewBag.ChartLabels = grouped.Select(x => x.Name).ToList();
            ViewBag.ChartData = grouped.Select(x => x.SumSupply).ToList();

            // 3️⃣ Table data
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
            ViewBag.Rows = rows;

            return View(token);
        }

        // --------------- AJAX Endpoints ---------------

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SaveJson([FromBody] Token token)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { ok = false, message = "Invalid input" });

            bool isNew = token.Id == 0;

            if (isNew)
                _context.Tokens.Add(token);   // create
            else
                _context.Tokens.Update(token); // update

            _context.SaveChanges();

            // More natural, matching your original messages
            string message = isNew ? "Token created" : "Token updated";
            return Ok(new { ok = true, message });
        }


        public IActionResult DetailsPartial()
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
            }).OrderByDescending(r => r.Percent).ToList();

            for (int i = 0; i < rows.Count; i++) rows[i].Rank = i + 1;
            ViewBag.Rows = rows;

            return PartialView("_ViewDetailsPartial");
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

        [HttpGet]
        public IActionResult Detail(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return NotFound();

            var t = _context.Tokens.FirstOrDefault(x => x.Symbol == id);
            if (t == null) return NotFound();

            var vm = new TokenDetailVm
            {
                Symbol = t.Symbol,
                Name = t.Name,
                ContractAddress = t.ContractAddress,
                TotalHolders = t.TotalHolders,
                TotalSupply = t.TotalSupply,
                Price = t.Price ?? 0m
            };

            return View(vm);
        }
    }
}
