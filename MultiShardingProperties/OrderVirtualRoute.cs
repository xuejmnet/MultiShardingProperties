using System.Globalization;
using System.Linq.Expressions;
using MultiShardingProperties.Domain;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.Core.VirtualRoutes;
using ShardingCore.Helpers;
using ShardingCore.VirtualRoutes.Months;

namespace MultiShardingProperties
{
    public class OrderVirtualRoute:AbstractSimpleShardingMonthKeyDateTimeVirtualTableRoute<Order>
    {
        /// <summary>
        /// 配置主分表字段是CreateTime,额外分表字段是OrderNo
        /// </summary>
        /// <param name="builder"></param>
        public override void Configure(EntityMetadataTableBuilder<Order> builder)
        {
            builder.ShardingProperty(o => o.CreateTime);
            builder.ShardingExtraProperty(o => o.OrderNo);
        }
        /// <summary>
        /// 是否要在程序运行期间自动创建每月的表
        /// </summary>
        /// <returns></returns>
        public override bool AutoCreateTableByTime()
        {
            return true;
        }
        /// <summary>
        /// 分表从何时起创建
        /// </summary>
        /// <returns></returns>
        public override DateTime GetBeginTime()
        {
            return new DateTime(2021, 9, 1);
        }
        /// <summary>
        /// 配置额外分片路由规则
        /// </summary>
        /// <param name="shardingKey"></param>
        /// <param name="shardingOperator"></param>
        /// <param name="shardingPropertyName"></param>
        /// <returns></returns>
        public override Expression<Func<string, bool>> GetExtraRouteFilter(object shardingKey, ShardingOperatorEnum shardingOperator, string shardingPropertyName)
        {
            switch (shardingPropertyName)
            {
                case nameof(Order.OrderNo): return GetOrderNoRouteFilter(shardingKey, shardingOperator);
                default: throw new NotImplementedException(shardingPropertyName);
            }
        }
        /// <summary>
        /// 订单编号的路由
        /// </summary>
        /// <param name="shardingKey"></param>
        /// <param name="shardingOperator"></param>
        /// <returns></returns>
        public Expression<Func<string, bool>> GetOrderNoRouteFilter(object shardingKey,
            ShardingOperatorEnum shardingOperator)
        {
            //将分表字段转成订单编号
            var orderNo = shardingKey?.ToString()??string.Empty;
            //判断订单编号是否是我们符合的格式
            if (!CheckOrderNo(orderNo, out var orderTime))
            {
                //如果格式不一样就直接返回false那么本次查询因为是and链接的所以本次查询不会经过任何路由,可以有效的防止恶意攻击
                return tail => false;
            }

            //当前时间的tail
            var t = TimeFormatToTail(orderTime);
            //因为是按月分表所以获取下个月的时间判断id是否是在临界点创建的
            var nextMonthFirstDay = ShardingCoreHelper.GetNextMonthFirstDay(DateTime.Now);
            if (orderTime.AddSeconds(10) > nextMonthFirstDay)
            {
                var nextT = TimeFormatToTail(nextMonthFirstDay);

                if (shardingOperator == ShardingOperatorEnum.Equal)
                {
                    return tail => tail == t || tail == nextT;
                }
            }
            //因为是按月分表所以获取这个月月初的时间判断id是否是在临界点创建的
            else if (orderTime.AddSeconds(-10) < ShardingCoreHelper.GetCurrentMonthFirstDay(DateTime.Now))
            {
                //上个月tail
                var nextT = TimeFormatToTail(orderTime.AddSeconds(-10));

                if (shardingOperator == ShardingOperatorEnum.Equal)
                {
                    return tail => tail == t || tail == nextT;
                }
            }
            else
            {
                if (shardingOperator == ShardingOperatorEnum.Equal)
                {
                    return tail => tail == t;
                }
            }

            return tail => true;
        }

        private bool CheckOrderNo(string orderNo,out DateTime orderTime)
        {
            //yyyyMMddHHmmss+new Random().Next(0,10000).ToString().PadLeft(4,'0')
            if (orderNo.Length == 18)
            {
                if (DateTime.TryParseExact(orderNo.Substring(0,14), "yyyyMMddHHmmss", CultureInfo.InvariantCulture,
                        DateTimeStyles.None, out var parseDateTime))
                {
                    orderTime = parseDateTime;
                    return true;
                }
            }

            orderTime = DateTime.MinValue;
            return false;
        }
    }
}
