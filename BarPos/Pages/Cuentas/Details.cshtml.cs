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
    public class DetailsModel : PageModel
    {
        // Contexto de base de datos que permite acceder a las entidades
        private readonly BarPos.Models.AppDbContext _context;

        // Constructor que recibe el contexto de la base de datos e inicializa la variable
        public DetailsModel(BarPos.Models.AppDbContext context)
        {
            _context = context;
        }

        // Propiedad pública para almacenar la cuenta seleccionada y mostrarla en la vista
        public Cuenta Cuenta { get; set; } = default!;

        // Método que se ejecuta al cargar la página con un parámetro "id"
        public async Task<IActionResult> OnGetAsync(long? id)
        {
            // Si el parámetro id no se recibe, devuelve un error 404 (no encontrado)
            if (id == null)
            {
                return NotFound();
            }
            // Busca en la base de datos la cuenta que tenga el id recibido
            // Incluye las relaciones necesarias: DetalleCuentas,Presentacion, Producto,Categoria
            Cuenta = await _context.Cuentas
                .Include(c => c.DetalleCuentas)
                    .ThenInclude(d => d.Presentacion)
                        .ThenInclude(p => p.Producto)
                            .ThenInclude(pr => pr.Categoria)
                .FirstOrDefaultAsync(m => m.Id == id);

            // Si no se encuentra la cuenta muestra error 404
            if (Cuenta == null)
            {
                return NotFound();
            }
            // Si todo va bien, devuelve la página con los datos de la cuenta
            return Page();
        }
    }
}
