using FactoryOrders.Data;

namespace FactoryOrders.Services;

public interface IOrderService
{
    event Func<ManufacturingOrder?, string, Task>? OnOrdersUpdated;

    Task InitializeAsync();
    Task<List<ManufacturingOrder>> GetAllOrdersAsync();
    Task<ManufacturingOrder?> GetOrderByIdAsync(int id);
    Task<ManufacturingOrder> CreateOrderAsync(ManufacturingOrder order);
    Task<ManufacturingOrder?> SetFirstToProduceAsync(int orderId, bool isFirst);
    Task<ManufacturingOrder?> UpdatePriorityAsync(int orderId, OrderPriority priority);
    Task<ManufacturingOrder?> UpdateStatusAsync(int orderId, OrderStatus status, string? assignedTo);
    Task<ManufacturingOrder?> UpdateOrderAsync(ManufacturingOrder order);
    Task<bool> DeleteOrderAsync(int orderId);
    Task MoveQueueAsync(int orderId, bool moveUp);
}
