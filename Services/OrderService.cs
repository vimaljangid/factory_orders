using FactoryOrders.Data;
using Microsoft.EntityFrameworkCore;

namespace FactoryOrders.Services;

public class OrderService : IOrderService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OrderService> _logger;
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private bool _isInitialized = false;

    public event Func<ManufacturingOrder?, string, Task>? OnOrdersUpdated;

    public OrderService(IServiceProvider serviceProvider, ILogger<OrderService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        if (_isInitialized) return;

        await _semaphore.WaitAsync();
        try
        {
            if (_isInitialized) return;

            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await db.Database.EnsureCreatedAsync();

            // Seed initial sample data if empty so the user can immediately test
            if (!await db.Orders.AnyAsync())
            {
                var samples = new List<ManufacturingOrder>
                {
                    new()
                    {
                        OrderNumber = "ORD-101",
                        ItemName = "Shaft 25mm CNC Turning",
                        Quantity = 150,
                        Unit = "pcs",
                        Priority = OrderPriority.Urgent,
                        IsFirstToProduce = true,
                        Status = OrderStatus.Pending,
                        OrderedBy = "Supervisor Ramesh",
                        MachineOrLine = "CNC Lathe #1",
                        Notes = "High tolerance +/- 0.02mm. Urgent batch needed for assembly.",
                        CreatedAt = DateTime.Now.AddHours(-2),
                        QueueOrder = 1
                    },
                    new()
                    {
                        OrderNumber = "ORD-102",
                        ItemName = "Hydraulic Mounting Flange",
                        Quantity = 50,
                        Unit = "pcs",
                        Priority = OrderPriority.High,
                        IsFirstToProduce = false,
                        Status = OrderStatus.InProgress,
                        OrderedBy = "Amit (Production Head)",
                        AssignedTo = "Operator Suresh",
                        MachineOrLine = "Milling VMC-3",
                        Notes = "Cast iron grade FG260. Material issued from bay 4.",
                        CreatedAt = DateTime.Now.AddHours(-1),
                        StartedAt = DateTime.Now.AddMinutes(-30),
                        QueueOrder = 2
                    },
                    new()
                    {
                        OrderNumber = "ORD-103",
                        ItemName = "Stainless Steel Bushing M16",
                        Quantity = 500,
                        Unit = "pcs",
                        Priority = OrderPriority.Normal,
                        IsFirstToProduce = false,
                        Status = OrderStatus.Pending,
                        OrderedBy = "Pooja (Line B)",
                        MachineOrLine = "Auto Lathe 2",
                        Notes = "SS 304 material. Standard deburring required.",
                        CreatedAt = DateTime.Now.AddMinutes(-45),
                        QueueOrder = 3
                    }
                };

                await db.Orders.AddRangeAsync(samples);
                await db.SaveChangesAsync();
            }

            _isInitialized = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing order database");
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<List<ManufacturingOrder>> GetAllOrdersAsync()
    {
        await InitializeAsync();
        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Sort:
        // 1. IsFirstToProduce = true first
        // 2. Urgent priority next, then High, then Normal
        // 3. Queue order
        // 4. Creation time
        return await db.Orders
            .OrderByDescending(o => o.IsFirstToProduce)
            .ThenByDescending(o => o.Priority)
            .ThenBy(o => o.QueueOrder)
            .ThenByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task<ManufacturingOrder?> GetOrderByIdAsync(int id)
    {
        await InitializeAsync();
        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return await db.Orders.FindAsync(id);
    }

    public async Task<ManufacturingOrder> CreateOrderAsync(ManufacturingOrder order)
    {
        await InitializeAsync();
        await _semaphore.WaitAsync();
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // Generate order number if empty
            if (string.IsNullOrWhiteSpace(order.OrderNumber))
            {
                var count = await db.Orders.CountAsync() + 1;
                order.OrderNumber = $"ORD-{count:D3}";
            }

            order.CreatedAt = DateTime.Now;

            // If marked as first to produce, we can reset previous first-to-produce if desired, or keep as top tier
            var maxQueue = await db.Orders.MaxAsync(o => (int?)o.QueueOrder) ?? 0;
            order.QueueOrder = maxQueue + 1;

            await db.Orders.AddAsync(order);
            await db.SaveChangesAsync();

            _ = NotifySubscribers(order, "Created");
            return order;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<ManufacturingOrder?> SetFirstToProduceAsync(int orderId, bool isFirst)
    {
        await InitializeAsync();
        await _semaphore.WaitAsync();
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var order = await db.Orders.FindAsync(orderId);
            if (order == null) return null;

            order.IsFirstToProduce = isFirst;
            if (isFirst && order.Priority == OrderPriority.Normal)
            {
                // Automatically upgrade to urgent or high if marked as first to ready
                order.Priority = OrderPriority.Urgent;
            }

            await db.SaveChangesAsync();
            _ = NotifySubscribers(order, "PriorityChanged");
            return order;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<ManufacturingOrder?> UpdatePriorityAsync(int orderId, OrderPriority priority)
    {
        await InitializeAsync();
        await _semaphore.WaitAsync();
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var order = await db.Orders.FindAsync(orderId);
            if (order == null) return null;

            order.Priority = priority;
            await db.SaveChangesAsync();

            _ = NotifySubscribers(order, "PriorityChanged");
            return order;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<ManufacturingOrder?> UpdateStatusAsync(int orderId, OrderStatus status, string? assignedTo)
    {
        await InitializeAsync();
        await _semaphore.WaitAsync();
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var order = await db.Orders.FindAsync(orderId);
            if (order == null) return null;

            order.Status = status;

            if (!string.IsNullOrWhiteSpace(assignedTo))
            {
                order.AssignedTo = assignedTo;
            }

            if (status == OrderStatus.InProgress && order.StartedAt == null)
            {
                order.StartedAt = DateTime.Now;
            }
            else if (status == OrderStatus.Completed)
            {
                order.CompletedAt = DateTime.Now;
                order.IsFirstToProduce = false; // Reset first flag on completion
            }
            else if (status == OrderStatus.Pending)
            {
                order.StartedAt = null;
                order.CompletedAt = null;
            }

            await db.SaveChangesAsync();
            _ = NotifySubscribers(order, "StatusChanged");
            return order;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<ManufacturingOrder?> UpdateOrderAsync(ManufacturingOrder order)
    {
        await InitializeAsync();
        await _semaphore.WaitAsync();
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var existing = await db.Orders.FindAsync(order.Id);
            if (existing == null) return null;

            existing.ItemName = order.ItemName;
            existing.Quantity = order.Quantity;
            existing.Unit = order.Unit;
            existing.Priority = order.Priority;
            existing.IsFirstToProduce = order.IsFirstToProduce;
            existing.OrderedBy = order.OrderedBy;
            existing.AssignedTo = order.AssignedTo;
            existing.MachineOrLine = order.MachineOrLine;
            existing.Notes = order.Notes;
            existing.TargetDate = order.TargetDate;

            await db.SaveChangesAsync();
            _ = NotifySubscribers(existing, "Updated");
            return existing;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<bool> DeleteOrderAsync(int orderId)
    {
        await InitializeAsync();
        await _semaphore.WaitAsync();
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var order = await db.Orders.FindAsync(orderId);
            if (order == null) return false;

            db.Orders.Remove(order);
            await db.SaveChangesAsync();

            _ = NotifySubscribers(order, "Deleted");
            return true;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task MoveQueueAsync(int orderId, bool moveUp)
    {
        await InitializeAsync();
        await _semaphore.WaitAsync();
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var orders = await db.Orders
                .OrderBy(o => o.QueueOrder)
                .ToListAsync();

            var index = orders.FindIndex(o => o.Id == orderId);
            if (index < 0) return;

            if (moveUp && index > 0)
            {
                var prev = orders[index - 1];
                var curr = orders[index];
                (curr.QueueOrder, prev.QueueOrder) = (prev.QueueOrder, curr.QueueOrder);
                await db.SaveChangesAsync();
                _ = NotifySubscribers(curr, "Reordered");
            }
            else if (!moveUp && index < orders.Count - 1)
            {
                var next = orders[index + 1];
                var curr = orders[index];
                (curr.QueueOrder, next.QueueOrder) = (next.QueueOrder, curr.QueueOrder);
                await db.SaveChangesAsync();
                _ = NotifySubscribers(curr, "Reordered");
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async Task NotifySubscribers(ManufacturingOrder? order, string eventType)
    {
        if (OnOrdersUpdated != null)
        {
            var handlers = OnOrdersUpdated.GetInvocationList();
            foreach (Func<ManufacturingOrder?, string, Task> handler in handlers)
            {
                try
                {
                    await handler(order, eventType);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error notifying client event subscriber");
                }
            }
        }
    }
}
