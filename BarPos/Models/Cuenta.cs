using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BarPos.Models;

public partial class Cuenta
{
    public long Id { get; set; }

    [Required(ErrorMessage = "El nombre del cliente es requerido")]
    [StringLength(50, ErrorMessage = "El nombre no puede exceder 50 caracteres")]
    [Display(Name = "Nombre del Cliente")]
    public string NombreCliente { get; set; } = null!;

    [Display(Name = "Fecha de Apertura")]
    [DataType(DataType.DateTime)]
    public DateTime? FechaApertura { get; set; }

    [Required]
    [StringLength(20)]
    [Display(Name = "Estado")]
    public string Estado { get; set; } = "Abierta";

    [StringLength(20)]
    [Display(Name = "Método de Pago")]
    public string? MetodoPago { get; set; }

    [Display(Name = "Total")]
    [DataType(DataType.Currency)]
    public decimal? Total { get; set; }

    [Display(Name = "Monto Pagado")]
    [DataType(DataType.Currency)]
    public decimal? MontoPagado { get; set; }

    [Display(Name = "Vuelto")]
    [DataType(DataType.Currency)]
    public decimal? Vuelto { get; set; }

    public virtual ICollection<DetalleCuenta> DetalleCuentas { get; set; } = new List<DetalleCuenta>();
}
