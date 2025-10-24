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
    public class IndexModel : PageModel
    {
        //Contexto de base de datos que permite acceder a las tablas del sistema
        private readonly BarPos.Models.AppDbContext _context;

        // Constructor que recibe el contexto de base de datos e inicializa la variable privada
        public IndexModel(BarPos.Models.AppDbContext context)
        {
            _context = context;
        }

        //Lista para guardar las cuentas abiertas obtenidas de la base de datos
        public IList<Cuenta> CuentasAbiertas { get; set; } = default!;

        // Lista para guardar las últimas cuentas cerradas obtenidas de la base de datos
        public IList<Cuenta> CuentasCerradas { get; set; } = default!;

        //Propiedad enlazada al campo de búsqueda (se envía por GET desde el formulario)
        [BindProperty(SupportsGet = true)]
        public string BusquedaNombre { get; set; }

        //Propiedad para filtrar cuentas por estado (por defecto muestra todas)
        [BindProperty(SupportsGet = true)]
        public string FiltroEstado { get; set; } = "Todas";

        //Método que se ejecuta cuando se carga la página (GET)
        public async Task OnGetAsync()
        {
            //Crea una consulta base que incluye los detalles de cada cuenta
            var queryCuentas = _context.Cuentas
               .Include(c => c.DetalleCuentas)
               .AsQueryable();

            //Si se ingresó texto en la barra de búsqueda, se filtra por el nombre del cliente
            if (!string.IsNullOrEmpty(BusquedaNombre))
            {
                queryCuentas = queryCuentas.Where(c =>
                    c.NombreCliente.Contains(BusquedaNombre));
            }

            //Obtiene las cuentas abiertas (estado = "Abierta") y las ordena por fecha descendente
            CuentasAbiertas = await _context.Cuentas
                .Include(c => c.DetalleCuentas)
                .Where(c => c.Estado == "Abierta")
                .OrderByDescending(c => c.FechaApertura)
                .ToListAsync();

            // Cargar últimas 20 cuentas cerradas  (estado = "Cerrada")
            CuentasCerradas = await _context.Cuentas
                .Include(c => c.DetalleCuentas)
                .Where(c => c.Estado == "Cerrada")
                .OrderByDescending(c => c.FechaApertura)
                .Take(20)
                .ToListAsync();
        }
    }
}
