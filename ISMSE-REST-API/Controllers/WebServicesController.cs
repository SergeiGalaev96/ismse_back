using ISMSE_REST_API.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using Document = ISMSE_REST_API.Models.document;
using Attribute = ISMSE_REST_API.Models.attribute;
using System.IO;

namespace ISMSE_REST_API.Controllers
{
    [System.Web.Http.Cors.EnableCors(origins: "*", headers: "*", methods: "*")]
    public class WebServicesController : ApiController
    {
        [HttpGet]
        [ResponseType(typeof(Document[]))]
        public IHttpActionResult Spravka([FromUri] string pin)
        {
            try
            {
                var result = ScriptExecutor.Spravka(pin);
                return Ok(result);
            }
            catch (Exception e)
            {
                return BadRequest(string.Format("Error text: {0}, stacktrace: {1}", e.Message, e.StackTrace));
            }
        }

        static void WriteLog(object text)
        {
            using (StreamWriter sw = new StreamWriter("c:\\Log\\cissa-rest-api.log", true))
            {
                sw.WriteLine(text.ToString());
            }
        }
    }
}
