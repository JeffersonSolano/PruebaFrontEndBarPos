using BarPos.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BarPos.Pages.Cuentas
{
    public class CerrarCuentaModel : PageModel
    {
        private readonly AppDbContext _context;

        public CerrarCuentaModel(AppDbContext context)
        {
            _context = context;
        }

        // Propiedad para enlazar la información de la cuenta actual
        [BindProperty]
        public Cuenta Cuenta { get; set; } = default!;

        // Propiedad para guardar el método de pago seleccionado (Efectivo/Tarjeta)
        [BindProperty]
        public string MetodoPagoSeleccionado { get; set; } = default!;

        // Propiedad para guardar cuanto pago el cliente
        [BindProperty]
        public decimal MontoPagadoPorCliente { get; set; }

        // Método que se ejecuta cuando la página se carga (GET)
        public async Task<IActionResult> OnGetAsync(long? id)
        {
            // Verifica si el id recibido es válido
            if (id == null)
            {
                return NotFound();
            }
            // Carga la cuenta con sus detalles, productos y presentaciones
            Cuenta = await _context.Cuentas
                .Include(c => c.DetalleCuentas)
                    .ThenInclude(d => d.Presentacion)
                        .ThenInclude(p => p.Producto)
                .FirstOrDefaultAsync(m => m.Id == id);

            // Si la cuenta no existe o no está abierta, muestra error
            if (Cuenta == null || Cuenta.Estado != "Abierta")
            {
                return NotFound();
            }

            // Inicializa el monto pagado igual al total por defecto
            MontoPagadoPorCliente = Cuenta.Total ?? 0;

            return Page();
        }


        // Método que se ejecuta cuando el formulario es enviado (POST)
        public async Task<IActionResult> OnPostAsync()
        {
            // Validar que se haya seleccionado un método de pago
            if (string.IsNullOrEmpty(MetodoPagoSeleccionado))
            {
                ModelState.AddModelError(string.Empty, "Debe seleccionar un método de pago");
                await CargarCuenta();
                return Page();
            }
            // Validar que el cliente haya pagado al menos el total
            if (MontoPagadoPorCliente < Cuenta.Total)
            {
                ModelState.AddModelError(string.Empty,
                    $"El monto pagado (?{MontoPagadoPorCliente:N2}) es menor al total de la cuenta (?{Cuenta.Total:N2})");
                await CargarCuenta();
                return Page();
            }

            // Obtener la cuenta de la base de datos
            var cuentaDb = await _context.Cuentas
                .Include(c => c.DetalleCuentas)
                    .ThenInclude(d => d.Presentacion)
                        .ThenInclude(p => p.Producto)
                .FirstOrDefaultAsync(c => c.Id == Cuenta.Id);

            // Verifica que exista en la base de datos
            if (cuentaDb == null)
            {
                return NotFound();
            }

            // Actualizar los datos  de la cuenta
            cuentaDb.Estado = "Cerrada";
            cuentaDb.MetodoPago = MetodoPagoSeleccionado;
            cuentaDb.MontoPagado = MontoPagadoPorCliente;
            cuentaDb.Vuelto = MontoPagadoPorCliente - (cuentaDb.Total ?? 0);

            // Descontar del inventario
            foreach (var detalle in cuentaDb.DetalleCuentas)
            {
                var producto = detalle.Presentacion.Producto;

                // Verificar stock nuevamente por seguridad
                if (producto.Stock < detalle.Cantidad)
                {
                    throw new Exception($"Stock insuficiente para {producto.Nombre}. " +
                        $"Disponible: {producto.Stock}, Requerido: {detalle.Cantidad}");
                }

                // Descontar del stock
                producto.Stock -= detalle.Cantidad;

                // Registrar movimiento de inventario
                var movimiento = new MovimientosInventario
                {
                    ProductoId = producto.Id,
                    TipoMovimiento = "Venta",
                    Cantidad = -detalle.Cantidad, // Negativo porque es una salida
                    Fecha = DateTime.Now,
                    Descripcion = $"Venta a {cuentaDb.NombreCliente} - {detalle.Presentacion.Nombre}"
                };
                // Agregar el movimiento a la base de datos
                _context.MovimientosInventarios.Add(movimiento);
            }
            // Guardar los cambios en la base de datos
            await _context.SaveChangesAsync();

            // Mensaje de éxito mostrado en la página principal
            TempData["MensajeExito"] = $"Cuenta de {cuentaDb.NombreCliente} cerrada exitosamente. Vuelto: ?{cuentaDb.Vuelto:N2}";

            return RedirectToPage("./Index");
        }

        // Método auxiliar para recargar los datos de la cuenta si hay error
        private async Task CargarCuenta()
        {
            Cuenta = await _context.Cuentas
                .Include(c => c.DetalleCuentas)
                    .ThenInclude(d => d.Presentacion)
                        .ThenInclude(p => p.Producto)
                .FirstOrDefaultAsync(m => m.Id == Cuenta.Id);
        }
    }
}
