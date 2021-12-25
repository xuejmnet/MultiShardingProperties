using Microsoft.EntityFrameworkCore;
using MultiShardingProperties.Domain;
using ShardingCore.Core.VirtualRoutes.TableRoutes.RouteTails.Abstractions;
using ShardingCore.Sharding;
using ShardingCore.Sharding.Abstractions;

namespace MultiShardingProperties
{
    /// <summary>
    /// 如果需要支持分表必须要实现<see cref="IShardingTableDbContext"/>
    /// </summary>
    public class DefaultDbContext:AbstractShardingDbContext,IShardingTableDbContext
    {
        public DefaultDbContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Order>(o =>
            {
                o.HasKey(p => p.Id);
                o.Property(p => p.OrderNo).IsRequired().HasMaxLength(128).IsUnicode(false);
                o.Property(p => p.Name).IsRequired().HasMaxLength(128).IsUnicode(false);
                o.ToTable(nameof(Order));
            });
        }

        public IRouteTail RouteTail { get; set; }
    }
}
