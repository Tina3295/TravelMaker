using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

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

        // GET api/values
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/values
        public void Post([FromBody] string value)
        {
        }

        // PUT api/values/5
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        public void Delete(int id)
        {
        }
    }
}
