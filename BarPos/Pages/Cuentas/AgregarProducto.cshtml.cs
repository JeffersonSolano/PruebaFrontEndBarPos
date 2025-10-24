using BarPos.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BarPos.Pages.Cuentas
{
    public class AgregarProductoModel : PageModel
    {
        private readonly AppDbContext _context;

        // Constructor que inyecta el contexto de base de datos
        public AgregarProductoModel(AppDbContext context)
        {
            _context = context;
        }

        // Propiedades que se enlazan con el formulario en la vista
        [BindProperty]
        public long CuentaId { get; set; }

        [BindProperty]
        public long CategoriaSeleccionadaId { get; set; }

        [BindProperty]
        public long ProductoSeleccionadoId { get; set; }

        [BindProperty]
        public long PresentacionSeleccionadaId { get; set; }

        [BindProperty]
        public int Cantidad { get; set; } = 1;

        // Objetos que se mostrarán en la vista
        public Cuenta Cuenta { get; set; } = default!;
        public List<Categoria> Categorias { get; set; } = new();
        public List<Producto> Productos { get; set; } = new();
        public List<Presentacion> Presentaciones { get; set; } = new();

        // Método que se ejecuta cuando se carga la página
        public async Task<IActionResult> OnGetAsync(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            // Cargar la cuenta con sus productos asociados
            await CargarCuenta(id.Value);

            //s i la cuenta no existe o ya esta cerrada mostrar error
            if (Cuenta == null || Cuenta.Estado != "Abierta")
            {
                return NotFound();
            }

            // Cargar las categorías para el selector
            await CargarCategorias();
            return Page();
        }

        // Método que se ejecuta cuando se envía el formulario para agregar un producto
        public async Task<IActionResult> OnPostAgregarProductoAsync()
        {
            // Validación, debe seleccionar una presentación
            if (PresentacionSeleccionadaId == 0)
            {
                ModelState.AddModelError(string.Empty, "Debe seleccionar un producto y presentación");
                await CargarCuenta(CuentaId);
                await CargarCategorias();
                return Page();
            }

            // Obtener la presentación con su precio y producto
            var presentacion = await _context.Presentaciones
                .Include(p => p.Producto)
                .FirstOrDefaultAsync(p => p.Id == PresentacionSeleccionadaId);

            if (presentacion == null)
            {
                ModelState.AddModelError(string.Empty, "Presentación no encontrada");
                await CargarCuenta(CuentaId);
                await CargarCategorias();
                return Page();
            }

            // VALIDAR STOCK DISPONIBLE
            // Verificar cuánto del mismo producto ya está agregado en la cuenta
            var cantidadActualEnCuenta = await _context.DetalleCuenta
                .Where(d => d.CuentaId == CuentaId && d.PresentacionId == PresentacionSeleccionadaId)
                .Select(d => d.Cantidad)
                .FirstOrDefaultAsync();

            var cantidadTotalRequerida = cantidadActualEnCuenta + Cantidad;

            // Si la cantidad solicitada supera el stock, mostrar error
            if (presentacion.Producto.Stock < cantidadTotalRequerida)
            {
                ModelState.AddModelError(string.Empty,
                    $"Stock insuficiente. Disponible: {presentacion.Producto.Stock}, " +
                    $"En cuenta: {cantidadActualEnCuenta}, " +
                    $"Solicitado: {Cantidad}");
                await CargarCuenta(CuentaId);
                await CargarCategorias();
                return Page();
            }

            // Verificar si ya existe este producto en el detalle
            var detalleExistente = await _context.DetalleCuenta
                .FirstOrDefaultAsync(d => d.CuentaId == CuentaId &&
                                         d.PresentacionId == PresentacionSeleccionadaId);

            if (detalleExistente != null)
            {
                // Si ya existe, incrementar la cantidad
                detalleExistente.Cantidad += Cantidad;
            }
            else
            {
                // Si no existe, crear nuevo detalle
                var nuevoDetalle = new DetalleCuenta
                {
                    CuentaId = CuentaId,
                    PresentacionId = PresentacionSeleccionadaId,
                    ProductoId = presentacion.ProductoId,
                    Cantidad = Cantidad,
                    PrecioUnitario = presentacion.PrecioVenta ?? 0
                };
                _context.DetalleCuenta.Add(nuevoDetalle);
            }

            // Guardar cambios en base de datos
            await _context.SaveChangesAsync();

            // Recalcular el total de la cuenta
            await ActualizarTotalCuenta(CuentaId);

            // Recargar la página para mostrar el producto agregado
            return RedirectToPage(new { id = CuentaId });
        }

        // Método para eliminar un producto del detalle de cuenta
        public async Task<IActionResult> OnPostEliminarDetalleAsync(long detalleId)
        {
            var detalle = await _context.DetalleCuenta.FindAsync(detalleId);

            if (detalle != null)
            {
                CuentaId = detalle.CuentaId;
                _context.DetalleCuenta.Remove(detalle);
                await _context.SaveChangesAsync();
                await ActualizarTotalCuenta(CuentaId);
            }

            return RedirectToPage(new { id = CuentaId });
        }

        // Cargar cuenta con sus productos, presentaciones y categorías
        private async Task CargarCuenta(long id)
        {
            Cuenta = await _context.Cuentas
                .Include(c => c.DetalleCuentas)
                    .ThenInclude(d => d.Presentacion)
                        .ThenInclude(p => p.Producto)
                            .ThenInclude(pr => pr.Categoria)
                .FirstOrDefaultAsync(c => c.Id == id);

            CuentaId = id;
        }

        // Cargar todas las categorías disponibles
        private async Task CargarCategorias()
        {
            Categorias = await _context.Categorias
                .OrderBy(c => c.Nombre)
                .ToListAsync();
        }

        // Recalcular el total de la cuenta sumando los subtotales
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

        // Endpoint que devuelve productos y sus presentaciones en formato JSON (para AJAX)
        public async Task<JsonResult> OnGetProductosPorCategoriaAsync(long categoriaId)
        {
            var productos = await _context.Productos
                .Where(p => p.CategoriaId == categoriaId)
                .Include(p => p.Presentaciones)
                .OrderBy(p => p.Nombre)
                .Select(p => new
                {
                    id = p.Id,
                    nombre = p.Nombre,
                    presentaciones = p.Presentaciones.Select(pr => new
                    {
                        id = pr.Id,
                        nombre = pr.Nombre,
                        precio = pr.PrecioVenta
                    })
                })
                .ToListAsync();

            return new JsonResult(productos);// Devuelve los productos y presentaciones al frontend
        }
    }
}
