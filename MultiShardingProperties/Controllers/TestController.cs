using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MultiShardingProperties.Domain;

namespace MultiShardingProperties.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class TestController:ControllerBase
    {
        private readonly DefaultDbContext _defaultDbContext;

        public TestController(DefaultDbContext defaultDbContext)
        {
            _defaultDbContext = defaultDbContext;
        }

        public async Task<IActionResult> Test1()
        {
            Console.WriteLine("--------------Query Name Begin--------------");
            var order1 = await _defaultDbContext.Set<Order>().Where(o=>o.Name=="Order3").FirstOrDefaultAsync();
            Console.WriteLine("--------------Query Name End--------------");
            Console.WriteLine("--------------Query OrderNo Begin--------------");
            var order2 = await _defaultDbContext.Set<Order>().Where(o=>o.OrderNo== "202110080000000003").FirstOrDefaultAsync();
            Console.WriteLine("--------------Query OrderNo End--------------");
            Console.WriteLine("--------------Query OrderCreateTime Begin--------------");
            var dateTime = new DateTime(2021,10,08);
            var order4 = await _defaultDbContext.Set<Order>().Where(o=>o.CreateTime== dateTime).FirstOrDefaultAsync();
            Console.WriteLine("--------------Query OrderCreateTime End--------------");
            Console.WriteLine("--------------Query OrderNo Contains Begin--------------");
            var orderNos = new string[] { "202110080000000003", "202111090000000004" };
            var order5 = await _defaultDbContext.Set<Order>().Where(o=> orderNos.Contains(o.OrderNo)).ToListAsync();
            Console.WriteLine("--------------Query OrderNo Contains End--------------");

            Console.WriteLine("--------------Query OrderNo None Begin--------------");
            var time = new DateTime(2021,11,1);
            var order6 = await _defaultDbContext.Set<Order>().Where(o=> o.OrderNo== "202110080000000003"&&o.CreateTime> time).FirstOrDefaultAsync();
            Console.WriteLine("--------------Query OrderNo None End--------------");
            Console.WriteLine("--------------Query OrderNo Not Check Begin--------------");
            var order3 = await _defaultDbContext.Set<Order>().Where(o => o.OrderNo == "a02110080000000003").FirstOrDefaultAsync();
            Console.WriteLine("--------------Query OrderNo Not Check End--------------");

            return Ok();
        }
    }
}
//Id OrderNo	Name	CreateTime
//86413c7705cd4ecb96cac8c9567ebe7a	202110080000000003	Order3	2021-10-08 00:00:00.0000000
