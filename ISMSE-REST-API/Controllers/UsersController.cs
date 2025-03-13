using ISMSE_REST_API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Http.Description;

namespace ISMSE_REST_API.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class UsersController : ApiController
    {

        [HttpGet]
        [ResponseType(typeof(DAL._cissa_user[]))]
        public IHttpActionResult GetAll()
        {
            try
            {
                return Ok(DAL.GetCissaUsers().ToArray());
            }
            catch (Exception e)
            {
                return BadRequest(e.GetBaseException().Message);
            }
        }

        [HttpGet]
        [ResponseType(typeof(Guid))]
        public IHttpActionResult Login([FromUri] string username, [FromUri] string password)
        {
            try
            {
                var userObj = DAL.GetCissaUsers().FirstOrDefault(x => x.UserName.ToLower() == username.ToLower() && x.Password == password);
                if (userObj != null)
                {
                    return Ok(new { userId = userObj.Id, orgName = userObj.OrgName });
                }
                else
                {
                    return Ok(new { userId = "", orgName = "", errorMessage = "Unauthorized" });
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.GetBaseException().Message);
            }
        }



        [HttpGet]
        [ResponseType(typeof(DAL.Organization[]))]
        public IHttpActionResult GetOrganizationList()
        {
            try
            {
                var organizations = DAL.GetOrganizationList().ToList();
                return Ok(organizations);
            }
            catch (Exception e)
            {
                return BadRequest(e.GetBaseException().Message);
            }
        }

        [HttpPost]
        [ResponseType(typeof(DAL.CreateUserResponse))]
        public IHttpActionResult Create([FromBody] DAL.User user)
        {
            bool isExistWorker = false; // JSON must have Admin UserId
            bool isExistUserName = false;
            Guid UserId = Guid.NewGuid();
            DAL.CreateUserResponse response = new DAL.CreateUserResponse();
            try
            {
                // Check existed worker
                isExistWorker = DAL.IsExistWorker(user.UserId);
                if (!isExistWorker)
                {
                    response.Status = 0;
                    response.Message = "Admin doesn't exist";
                    return Ok(response);
                }

                isExistUserName = DAL.IsExistUserName(user.UserName);
                if (isExistUserName)
                {
                    var existUserId = DAL.GetUserIdByName(user.UserName);
                    response.UserId = existUserId;
                    response.Status = 0;
                    response.Message = "User already exist";
                    return Ok(response);
                }

                // 1 Create Object_Defs
                DAL.CreateObjectDefs(UserId, user.UserName, user.ParentId);
                DAL.CreateSubjects(UserId, user.Address, user.Phone, user.Email);
                DAL.CreatePersons(UserId, user.LastName, user.FirstName, user.MiddleName);
                DAL.CreateWorkers(UserId, user.UserName, user.Password, user.OrgPositionId, user.LanguageId);

                response.Status = 200;
                response.UserId = UserId;
                response.Message = "OK";
                return Ok(response);
            }
            catch (Exception e)
            {
                return BadRequest(e.GetBaseException().Message);
            }

        }
    }
}