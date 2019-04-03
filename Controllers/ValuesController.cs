using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TimeMachine.Server;
using TimeMachine.Server.DB;

namespace TimemachineServer.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public ActionResult<string> Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }

        [HttpGet]
        public ActionResult<List<Subject>> Universe(string exchange)
        {
            return UniverseManager.Instance.GetUniverse(exchange);
        }

        [HttpPost]
        public ActionResult<ResOpenPrice> OpenPrice([FromBody] ReqOpenPrice request)
        {
            var date = DateTime.ParseExact(request.StartDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);

            using (var context = new QTContext())
            {
                var response = new ResOpenPrice();

                foreach (var asset in request.Assets)
                {
                    if (asset.Exchange != "ETF")
                    {
                        var stock = context.Stocks
                                            .Where(x => x.CreatedAt >= date && x.AssetCode == asset.AssetCode)
                                            .OrderBy(x => x.CreatedAt)
                                            .Take(10) // date가 실제 트레이딩 날짜가 아닐 수 있기 때문에 최대 10일 뒤의 데이터를 가져온다.(10일간 거래를 안할 수 없다는 가정)
                                            .FirstOrDefault();

                        response.Data.Add(new ResOpenPrice.Context
                        {
                            AssetCode = asset.AssetCode,
                            AssetName = AssetManager.Instance.GetAssetName(asset.AssetCode),
                            OpenPrice = stock.Open,
                        });
                    }
                }

                return response;
            }
        }

        [HttpPost]
        public ActionResult<string> AnalyzePortfolio([FromBody] ReqAnalyzePortfolio request)
        {
            return "";
        }
    }
}
