using System;
using System.Collections.Generic;
using System.Data.Entity.Spatial;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using TravelMaker.Models;
using TravelMaker.Security;

namespace TravelMaker.Controllers
{
    public class ValuesController : ApiController
    {
        /// <summary>
        /// 跨域測試用
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("api/user/test")]
        public IHttpActionResult Test()
        {
            return Ok(new { Message = "OK" });
        }


        private TravelMakerDbContext _db = new TravelMakerDbContext();
        /// <summary>
        ///     地理資料測試
        /// </summary>
        [HttpGet]
        [Route("api/geography/test")]
        public IHttpActionResult Testgeo()
        {
            DbGeography A = _db.Attractions.Where(a => a.AttractionId == 33).Select(a => a.Location).FirstOrDefault();
            DbGeography B = _db.Attractions.Where(a => a.AttractionId == 34).Select(a => a.Location).FirstOrDefault();
            double result = (double)A.Distance(B);
            return Ok(result);
        }
    }
}
