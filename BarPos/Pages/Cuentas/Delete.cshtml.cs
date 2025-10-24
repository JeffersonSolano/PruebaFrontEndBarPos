using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using BarPos.Models;

namespace BarPos.Pages.Cuentas
{
    public class DeleteModel : PageModel
    {
        private readonly BarPos.Models.AppDbContext _context;

        public DeleteModel(BarPos.Models.AppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Cuenta Cuenta { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cuenta = await _context.Cuentas.FirstOrDefaultAsync(m => m.Id == id);

            if (cuenta == null)
            {
                return NotFound();
            }
            else
            {
                Cuenta = cuenta;
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cuenta = await _context.Cuentas.FindAsync(id);
            if (cuenta != null)
            {
                Cuenta = cuenta;
                _context.Cuentas.Remove(Cuenta);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
