using ISMSE_REST_API.Contracts.MedactProcesses.DelayedProcessors.Medact;
using ISMSE_REST_API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ISMSE_REST_API.Controllers
{
    public class TaskSchedulerController : ApiController
    {
        private readonly ISpecificTaskManager _specificTaskManager;
        public TaskSchedulerController(ISpecificTaskManager specificTaskManager)
        {
            _specificTaskManager = specificTaskManager;
        }
        [HttpGet]
        public IHttpActionResult StartExpirationMedactCleaner()
        {
            try
            {
                _specificTaskManager.RunNewTaskMedactCleaner();
                return Ok(OperationResult.Success());
            }
            catch (Exception e)
            {
                return Ok(OperationResult.Failure(e.Message));
            }
        }
        [HttpGet]
        public IHttpActionResult GetTasks()
        {
            try
            {
                return Ok(OperationResult.Success(_specificTaskManager.GetTasks()));
            }
            catch (Exception e)
            {
                return Ok(OperationResult.Failure(e.Message));
            }
        }
        [HttpGet]
        public IHttpActionResult ClearTasks()
        {
            try
            {
                _specificTaskManager.ClearTasks();
                return Ok(OperationResult.Success());
            }
            catch (Exception e)
            {
                return Ok(OperationResult.Failure(e.Message));
            }
        }
    }
}
