using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BarPos.Models;

public partial class DetalleCuenta
{
    public long Id { get; set; }

    public long CuentaId { get; set; }

    public long PresentacionId { get; set; }

    public long? ProductoId { get; set; }

    [Required]
    [Range(1, 1000, ErrorMessage = "La cantidad debe estar entre 1 y 1000")]
    [Display(Name = "Cantidad")]
    public int Cantidad { get; set; }

    [Display(Name = "Precio Unitario")]
    [DataType(DataType.Currency)]
    public decimal PrecioUnitario { get; set; }

    [Display(Name = "Subtotal")]
    [DataType(DataType.Currency)]
    public decimal? Subtotal { get; set; }

    public virtual Cuenta Cuenta { get; set; } = null!;

    public virtual Presentacion Presentacion { get; set; } = null!;

    public virtual Producto? Producto { get; set; }
}
