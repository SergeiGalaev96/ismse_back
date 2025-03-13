using ISMSE_REST_API.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
//using Document = ISMSE_REST_API.Models.document;
using Attribute = ISMSE_REST_API.Models.attribute;
using System.IO;
using ISMSE_REST_API.Contracts.Status;
using ISMSE_REST_API.Models;
using ISMSE_REST_API.Models.Status;
using ISMSE_REST_API.Contracts.Infrastructure;
using ISMSE_REST_API.Contracts.Documents;
using ISMSE_REST_API.Contracts.MedactProcesses;
using ISMSE_REST_API.Contracts.MedactProcesses.Verification;

namespace ISMSE_REST_API.Controllers
{
    [System.Web.Http.Cors.EnableCors(origins: "*", headers: "*", methods: "*")]
    public class DocumentController : ApiController
    {
        private readonly IStatusRepository _statusRepo;
        private readonly IConverterPresenter _converterPresenter;
        private readonly ITransferChildToAdult _transferChildToAdult;
        private readonly IChildFacade _childFacade;
        private readonly IAdultFacade _adultFacade;

        public DocumentController(IStatusRepository statusRepo, IConverterPresenter converterPresenter, ITransferChildToAdult transferChildToAdult,
            IChildFacade childFacade, IAdultFacade adultFacade)
        {
            _statusRepo = statusRepo;
            _converterPresenter = converterPresenter;
            _transferChildToAdult = transferChildToAdult;
            _childFacade = childFacade;
            _adultFacade = adultFacade;
        }
        [HttpGet]
        [ResponseType(typeof(document[]))]
        public IHttpActionResult GetDocumentsByDefId([FromUri] Guid defId, [FromUri] int size, [FromUri] int page, [FromUri]Guid userId)
        {
            try
            {
                var result = ScriptExecutor.GetDocumentsByDefId(defId, page, size, userId);
                return Ok(result);
            }
            catch (Exception e)
            {
                return BadRequest(string.Format("Error text: {0}, stacktrace: {1}", e.Message, e.StackTrace));
            }
        }

        [HttpPost]
        [ResponseType(typeof(document[]))]
        public IHttpActionResult FilterDocumentsByDefId([FromUri] Guid defId, [FromUri] int size, [FromUri] int page, [FromUri]Guid userId, [FromBody] document filterDocument)
        {
            try
            {
                //WriteLog(Newtonsoft.Json.JsonConvert.SerializeObject(filterDocument));
                var result = ScriptExecutor.FilterDocumentsByDefId(filterDocument, defId, page, size, userId);
                return Ok(result);
            }
            catch (Exception e)
            {
                return Ok(e.Message + ", trace: " + e.StackTrace);
            }
        }

        [HttpPost]
        [ResponseType(typeof(document[]))]
        public IHttpActionResult FilterDocumentsByDefIdState([FromUri] Guid defId, [FromUri] int size, [FromUri] int page, [FromUri]Guid userId, [FromUri]Guid stateTypeId, [FromBody] document filterDocument)
        {
            try
            {
                //WriteLog(Newtonsoft.Json.JsonConvert.SerializeObject(filterDocument));
                var result = ScriptExecutor.FilterDocumentsByDefIdState(filterDocument, defId, page, size, userId, stateTypeId);
                return Ok(result);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost]
        [ResponseType(typeof(int))]
        public IHttpActionResult CountFilteredDocumentsByDefId([FromUri] Guid defId, [FromUri]Guid userId, [FromBody] document filterDocument)
        {
            try
            {
                var result = ScriptExecutor.CountFilteredDocumentsByDefId(filterDocument, defId, userId);
                return Ok(result);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet]
        [ResponseType(typeof(Attribute[]))]
        public IHttpActionResult GetDocAttributesByDefId([FromUri] Guid defId, [FromUri]Guid userId)
        {
            try
            {
                var result = ScriptExecutor.GetDocAttributesByDefId(defId, userId);
                return Ok(result);
            }
            catch (ApplicationException e)
            {
                return NotFound();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet]
        [ResponseType(typeof(int))]
        public IHttpActionResult CountDocumentsByDefId([FromUri] Guid defId, [FromUri]Guid userId)
        {
            try
            {
                var result = ScriptExecutor.CountDocumentsByDefId(defId, userId);
                return Ok(result);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet]
        [ResponseType(typeof(document))]
        public IHttpActionResult GetDocumentById([FromUri] Guid id, [FromUri]Guid userId)
        {
            try
            {
                var result = ScriptExecutor.GetDocumentById(id, userId);
                return Ok(result);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost]
        public HttpResponseMessage Create([FromUri] Guid defId, [FromUri]Guid userId, [FromBody] document document)
        {
            try
            {
                var docId = ScriptExecutor.CreateDocument(defId, document, userId);
                document.id = docId;
                var response = Request.CreateResponse(HttpStatusCode.Created, document);

                string uri = Url.Link("DefaultApi", new { action = "GetDocumentById", id = document.id });
                response.Headers.Location = new Uri(uri);
                return response;
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, e);
            }
        }

        [HttpPost]
        public HttpResponseMessage CreateWithNo([FromUri] Guid defId, [FromUri]Guid userId, [FromUri]string noAttrName, [FromBody] document document)
        {
            try
            {
                var docId = ScriptExecutor.CreateDocument(defId, document, userId, true, noAttrName);
                document.id = docId;
                var response = Request.CreateResponse(HttpStatusCode.Created, document);

                string uri = Url.Link("DefaultApi", new { action = "GetDocumentById", id = document.id });
                response.Headers.Location = new Uri(uri);
                return response;
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, e);
            }
        }

        [HttpPost]
        public IHttpActionResult CreateChild([FromBody] document document, [FromUri] Guid userId)
        {
            try
            {
                var newDoc = _childFacade.CreateNew(document, userId);
                return Ok(OperationResult.Success(newDoc));
            }
            catch (Exception e)
            {
                return Ok(OperationResult.Failure(e.GetBaseException().Message));
            }
        }

        [HttpPost]
        public IHttpActionResult CreateAdult([FromBody] document document, [FromUri] Guid userId)
        {
            try
            {
                var newDoc = _adultFacade.CreateNew(document, userId);
                return Ok(OperationResult.Success(newDoc));
            }
            catch (Exception e)
            {
                return Ok(OperationResult.Failure(e.GetBaseException().Message));
            }
        }

        [HttpPost]
        public IHttpActionResult ExtendWithNo([FromBody] document document, [FromUri] Guid userId)
        {
            try
            {
                var docId = _transferChildToAdult.TransferFromChildToAdult(document, userId);
                document.id = docId;
                return Ok(OperationResult.Success(document));
            }
            catch (Exception e)
            {
                return Ok(OperationResult.Failure(e.GetBaseException().Message));
            }
        }

        [HttpPut]
        public IHttpActionResult Update([FromUri] Guid id, [FromUri]Guid userId, [FromBody] document document)
        {
            try
            {
                ScriptExecutor.UpdateDocument(id, document, userId);
                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.GetBaseException().Message);
            }
        }

        /*[HttpDelete]
        public IHttpActionResult Delete([FromUri] Guid id, [FromUri]Guid userId)
        {
            try
            {
                ScriptExecutor.DeleteDocument(id, userId);
                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.GetBaseException().Message);
            }
        }*/

        [HttpGet]
        public IHttpActionResult SetState([FromUri] Guid docId, [FromUri] Guid stateTypeId, [FromUri]Guid userId)
        {
            try
            {
                ScriptExecutor.SetState(docId, stateTypeId, userId);
                return Ok(new { result = true });
            }
            catch (Exception e)
            {
                return BadRequest(e.GetBaseException().Message);
            }
        }

        [HttpPost]
        public IHttpActionResult SetStatusManually([FromBody] SetStatusManuallyRequestDTO dto, [FromUri] Guid userId)
        {
            try
            {
                _statusRepo.SetStatusManually(dto.docId, dto.stateTypeId, userId, _converterPresenter.GetDateTime(dto.statusDate));
                return Ok(OperationResult.Success());
            }
            catch (Exception e)
            {
                return Ok(OperationResult.Failure(e.GetBaseException().Message));
            }
        }

        [HttpGet]
        [ResponseType(typeof(OperationResult))]
        public IHttpActionResult SetTrashStatus([FromUri] Guid docId, [FromUri] Guid userId)
        {
            try
            {
                _statusRepo.SetTrashStatus(docId, userId);
                return Ok(OperationResult.Success());
            }
            catch (Exception e)
            {
                return Ok(OperationResult.Failure(e.GetBaseException().Message));
            }
        }
        public class SetMultipleDocsToStateRequest
        {
            public Guid[] docIdList { get; set; }
            public Guid stateTypeId { get; set; }
        }
        [HttpPost]
        public IHttpActionResult SetMultipleDocsToState([FromBody] SetMultipleDocsToStateRequest docsToState, [FromUri]Guid userId)
        {
            try
            {
                foreach (var docId in docsToState.docIdList)
                {
                    ScriptExecutor.SetState(docId, docsToState.stateTypeId, userId);
                }
                return Ok(new { result = true });
            }
            catch (Exception e)
            {
                return BadRequest(e.GetBaseException().Message);
            }
        }

        [HttpGet]
        [ResponseType(typeof(Guid))]
        public IHttpActionResult GetDefId([FromUri] Guid Id, [FromUri]Guid userId)
        {
            try
            {
                Guid result = ScriptExecutor.GetDefId(Id, userId);
                return Ok(result);
            }
            catch (Exception e)
            {
                return BadRequest(e.GetBaseException().Message);
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
