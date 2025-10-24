using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BarPos.Models;

namespace BarPos.Pages.Cuentas
{
    public class EditModel : PageModel
    {
        private readonly BarPos.Models.AppDbContext _context;

        public EditModel(BarPos.Models.AppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public DetalleCuenta DetalleCuenta { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            DetalleCuenta = await _context.DetalleCuenta
                .Include(d => d.Cuenta)
                .Include(d => d.Presentacion)
                    .ThenInclude(p => p.Producto)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (DetalleCuenta == null || DetalleCuenta.Cuenta.Estado != "Abierta")
            {
                return NotFound();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var detalleDb = await _context.DetalleCuenta
                .Include(d => d.Cuenta)
                .FirstOrDefaultAsync(d => d.Id == DetalleCuenta.Id);

            if (detalleDb == null || detalleDb.Cuenta.Estado != "Abierta")
            {
                return NotFound();
            }

            // Actualizar solo la cantidad
            detalleDb.Cantidad = DetalleCuenta.Cantidad;

            await _context.SaveChangesAsync();

            // Recalcular total de la cuenta
            await ActualizarTotalCuenta(detalleDb.CuentaId);

            return RedirectToPage("./AgregarProducto", new { id = detalleDb.CuentaId });
        }

        private async Task ActualizarTotalCuenta(long cuentaId)
        {
            var cuenta = await _context.Cuentas
                .Include(c => c.DetalleCuentas)
                .FirstOrDefaultAsync(c => c.Id == cuentaId);

            if (cuenta != null)
            {
                cuenta.Total = cuenta.DetalleCuentas.Sum(d => d.Subtotal) ?? 0;
                await _context.SaveChangesAsync();
            }
        }
    }
}