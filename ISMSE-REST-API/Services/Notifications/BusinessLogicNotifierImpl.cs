using ISMSE_REST_API.Contracts.DataProviders;
using ISMSE_REST_API.Contracts.Notifications;
using ISMSE_REST_API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ISMSE_REST_API.Services.Notifications
{
    public class BusinessLogicNotifierImpl : IBusinessLogicNotifier
    {
        private readonly ICissaDataAccessLayer _cissaDAL;
        public BusinessLogicNotifierImpl(ICissaDataAccessLayer cissaDAL)
        {
            _cissaDAL = cissaDAL;
        }
        public document[] FetchGreaterThan18YearsChildMedacts(document filterDocument, Guid userId, int page = 1, int size = 10)
        {
            return _cissaDAL.FetchGreaterThan18YearsChildMedacts(filterDocument, userId, page, size);
        }
    }
}