using FoodTruck.Web.Data;
using FoodTruck.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodTruck.Web.Controllers
{
    /// <summary>
    /// Admin CRUD for modifiers (Size, Protein, Add-on, etc.).
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class ModifiersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ModifiersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Modifiers
        public async Task<IActionResult> Index()
        {
            return View(await _context.Modifiers.OrderBy(m => m.Type).ThenBy(m => m.Name).ToListAsync());
        }

        // GET: Modifiers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var modifier = await _context.Modifiers
                .FirstOrDefaultAsync(m => m.Id == id);

            if (modifier == null) return NotFound();

            return View(modifier);
        }

        // GET: Modifiers/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Modifiers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Type,PriceDelta")] Modifier modifier)
        {
            if (ModelState.IsValid)
            {
                _context.Add(modifier);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(modifier);
        }

        // GET: Modifiers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var modifier = await _context.Modifiers.FindAsync(id);
            if (modifier == null) return NotFound();

            return View(modifier);
        }

        // POST: Modifiers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Type,PriceDelta")] Modifier modifier)
        {
            if (id != modifier.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(modifier);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ModifierExists(modifier.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(modifier);
        }

        // GET: Modifiers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var modifier = await _context.Modifiers
                .FirstOrDefaultAsync(m => m.Id == id);

            if (modifier == null) return NotFound();

            return View(modifier);
        }

        // POST: Modifiers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var modifier = await _context.Modifiers.FindAsync(id);
            if (modifier != null)
            {
                _context.Modifiers.Remove(modifier);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool ModifierExists(int id)
        {
            return _context.Modifiers.Any(e => e.Id == id);
        }
    }
}