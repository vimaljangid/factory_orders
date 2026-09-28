using System.ComponentModel.DataAnnotations;

namespace FactoryOrders.Data;

public class ManufacturingOrder
{
    public int Id { get; set; }

    [Required]
    public string OrderNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Item name is required")]
    public string ItemName { get; set; } = string.Empty;

    [Range(1, 1000000, ErrorMessage = "Quantity must be at least 1")]
    public int Quantity { get; set; } = 1;

    public string Unit { get; set; } = "pcs";

    public OrderPriority Priority { get; set; } = OrderPriority.Normal;

    /// <summary>
    /// Tagged as first one to be ready / highest immediate priority
    /// </summary>
    public bool IsFirstToProduce { get; set; } = false;

    public int QueueOrder { get; set; } = 0;

    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    [Required(ErrorMessage = "Please specify who is placing this order")]
    public string OrderedBy { get; set; } = string.Empty;

    public string? AssignedTo { get; set; }

    public string? MachineOrLine { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public DateTime? StartedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public DateTime? TargetDate { get; set; }
}
