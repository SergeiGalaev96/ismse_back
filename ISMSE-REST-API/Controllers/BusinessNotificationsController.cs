using ISMSE_REST_API.Contracts.Notifications;
using ISMSE_REST_API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;

namespace ISMSE_REST_API.Controllers
{
    [System.Web.Http.Cors.EnableCors(origins: "*", headers: "*", methods: "*")]
    public class BusinessNotificationsController : ApiController
    {
        private readonly IBusinessLogicNotifier _businessLogicNotifier;
        public BusinessNotificationsController(IBusinessLogicNotifier businessLogicNotifier)
        {
            _businessLogicNotifier = businessLogicNotifier;
        }


        [HttpPost]
        [ResponseType(typeof(document[]))]
        public IHttpActionResult Get18Entries([FromBody] document filterDocument, Guid userId, int page = 1, int size = 10)
        {
            try
            {
                var result = _businessLogicNotifier.FetchGreaterThan18YearsChildMedacts(filterDocument, userId, page, size);
                return Ok(OperationResult.Success(result));
            }
            catch (Exception e)
            {
                return Ok(OperationResult.Failure(e.GetBaseException().Message));
            }
        }
    }
}
