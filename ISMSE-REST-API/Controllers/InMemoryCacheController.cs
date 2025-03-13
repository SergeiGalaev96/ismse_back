using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Description;

namespace ISMSE_REST_API.Controllers
{
    public class JsonDynamicWrapper
    {
        public dynamic data { get; set; }
    }
    public enum CacheType
    {
        Any,
        Form,
        Data,
        Action
    }
    public class CacheInfo
    {
        public CacheType CacheType { get; set; } = CacheType.Any;
        public string CacheId { get; set; }
    }
    public class CacheItem
    {
        public string cacheId { get; set; }
        public Dictionary<string, object> Data { get; set; }
    }
    [System.Web.Http.Cors.EnableCors(origins: "*", headers: "*", methods: "*")]
    public class InMemoryCacheController : ApiController
    {
        private static IDictionary<string, string> cache;
        public InMemoryCacheController()
        {
            if(cache == null) cache = new Dictionary<string, string>();
        }
        [HttpPost]
        [ResponseType(typeof(CacheInfo))]
        public IHttpActionResult Save([FromBody] JsonDynamicWrapper json)
        {
            try
            {
                if (json.data == null) throw new ApplicationException("\"data\" not found!");
                //var cache = ignite.GetOrCreateCache<string, string>(CacheType.Any.ToString());
                var cacheId = Guid.NewGuid();
                cache[cacheId.ToString()] = JsonConvert.SerializeObject(json.data);
                return Ok(new CacheInfo
                {
                    CacheId = cacheId.ToString()
                });
            }
            catch (Exception e)
            {
                return BadRequest(e.GetBaseException().Message);
            }
        }

        [HttpGet]
        [ResponseType(typeof(object))]
        public IHttpActionResult Get([FromUri] string cacheId)
        {
            try
            {
                //var cache = ignite.GetOrCreateCache<string, string>(CacheType.Any.ToString());
                return Ok(System.Web.Helpers.Json.Decode<Dictionary<string, object>>(cache[cacheId.ToString()]));
            }
            catch (Exception e)
            {
                return BadRequest(e.GetBaseException().Message);
            }
        }

        [HttpGet]
        [ResponseType(typeof(object[]))]
        public IHttpActionResult GetList()
        {
            try
            {
                //var cache = ignite.GetOrCreateCache<string, string>(CacheType.Any.ToString());
                
                if(cache.Count() > 0)
                {
                    //var cacheObj = cache.ToList();
                    var objList = new List<CacheItem>();
                    foreach(var item in cache)
                    {
                        objList.Add(new CacheItem
                        {
                            cacheId = item.Key,
                            Data = System.Web.Helpers.Json.Decode<Dictionary<string, object>>(item.Value)
                        });
                    }
                    return Ok(objList);
                }
                return Ok(new object[] { });
            }
            catch (Exception e)
            {
                return BadRequest(e.GetBaseException().Message);
            }
        }


        [HttpPut]
        public IHttpActionResult Update([FromUri] string cacheId, [FromBody] JsonDynamicWrapper json)
        {
            try
            {
                //var cache = ignite.GetOrCreateCache<string, string>(CacheType.Any.ToString());
                if (json.data == null) throw new ApplicationException("\"data\" not found!");
                cache[cacheId] = JsonConvert.SerializeObject(json.data);
                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.GetBaseException().Message);
            }
        }

        [HttpDelete]
        public IHttpActionResult Delete([FromUri] string cacheId)
        {
            try
            {
                //var cache = ignite.GetOrCreateCache<string, string>(CacheType.Any.ToString());
                cache.Remove(cacheId);
                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.GetBaseException().Message);
            }
        }
    }
}