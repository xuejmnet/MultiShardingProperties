using Microsoft.EntityFrameworkCore;
using MultiShardingProperties;
using MultiShardingProperties.Domain;
using ShardingCore;
using ShardingCore.Bootstrapers;
using ShardingCore.TableExists;

ILoggerFactory efLogger = LoggerFactory.Create(builder =>
{
    builder.AddFilter((category, level) => category == DbLoggerCategory.Database.Command.Name && level == LogLevel.Information).AddConsole();
});
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddShardingDbContext<DefaultDbContext>((conStr, builder) => builder
        .UseSqlServer(conStr)
        .UseLoggerFactory(efLogger)
    )
    .Begin(o =>
    {
        o.CreateShardingTableOnStart = true;
        o.EnsureCreatedWithOutShardingTable = true;
        o.ThrowIfQueryRouteNotMatch = false;
    }).AddShardingTransaction((connection, builder) =>
    {
        builder.UseSqlServer(connection).UseLoggerFactory(efLogger);
    }).AddDefaultDataSource("ds0", "Data Source=localhost;Initial Catalog=ShardingMultiProperties;Integrated Security=True;")//如果你是SqlServer只需要修改这边的链接字符串即可
    .AddShardingTableRoute(op =>
    {
        op.AddShardingTableRoute<OrderVirtualRoute>();
    })
    .AddTableEnsureManager(sp => new SqlServerTableEnsureManager<DefaultDbContext>())
    .End();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.Services.GetRequiredService<IShardingBootstrapper>().Start();

app.UseAuthorization();

app.MapControllers();
//额外添加一些种子数据
using (var serviceScope = app.Services.CreateScope())
{
    var defaultDbContext = serviceScope.ServiceProvider.GetService<DefaultDbContext>();
    if (!defaultDbContext.Set<Order>().Any())
    {
        var orders = new List<Order>(8);
        var beginTime = new DateTime(2021, 9, 5);
        for (int i = 0; i < 8; i++)
        {

            var orderNo = beginTime.ToString("yyyyMMddHHmmss") + i.ToString().PadLeft(4, '0');
            orders.Add(new Order()
            {
                Id = Guid.NewGuid().ToString("n"),
                CreateTime = beginTime,
                Name = $"Order" + i,
                OrderNo = orderNo
            });
            beginTime = beginTime.AddDays(1);
            if (i % 2 == 1)
            {
                beginTime = beginTime.AddMonths(1);
            }
        }
        defaultDbContext.AddRange(orders);
        defaultDbContext.SaveChanges();
    }
}

app.Run();
