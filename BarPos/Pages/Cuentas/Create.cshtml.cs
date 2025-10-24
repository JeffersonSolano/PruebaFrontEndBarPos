using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using BarPos.Models;

namespace BarPos.Pages.Cuentas
{
    public class CreateModel : PageModel
    {
        // Contexto de base de datos para acceder a las tablas
        private readonly BarPos.Models.AppDbContext _context;

        // Constructor que inyecta el contexto de base de datos
        public CreateModel(BarPos.Models.AppDbContext context)
        {
            _context = context;
        }
        // Propiedad enlazada al formulario (modelo de datos)
        [BindProperty]
        public Cuenta Cuenta { get; set; } = default!;

        // Método que se ejecuta cuando se accede a la página (GET)
        public IActionResult OnGet()
        {
            // Inicializa una nueva cuenta con valores por defecto
            Cuenta = new Cuenta
            {
                Estado = "Abierta",
                FechaApertura = DateTime.Now,
                Total = 0,
                MontoPagado = 0,
                Vuelto = 0
            };
            return Page();
        }

        // Método que se ejecuta cuando el formulario se envía (POST)
        public async Task<IActionResult> OnPostAsync()
        {
            // Verifica si el modelo es valido 
            if (!ModelState.IsValid)
            {
                return Page();// Si hay errores vuelve a mostrar el formulario
            }

            // Asegura los valores por defecto antes de guardar
            Cuenta.Estado = "Abierta";
            Cuenta.FechaApertura = DateTime.Now;
            Cuenta.Total = 0;
            Cuenta.MontoPagado = 0;
            Cuenta.Vuelto = 0;
            Cuenta.MetodoPago = null; //el metodo de pago se define al cerrar la cuenta

            // Agrega la cuenta al contexto (en memoria)
            _context.Cuentas.Add(Cuenta);
            //guarda los cambios en la base de datos
            await _context.SaveChangesAsync();

            //redirige a la página para agregar productos
            return RedirectToPage("./AgregarProducto", new { id = Cuenta.Id });
        }
    }
}
