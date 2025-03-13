using ISMSE_REST_API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISMSE_REST_API.Contracts.Notifications
{
    public interface IBusinessLogicNotifier
    {
        document[] FetchGreaterThan18YearsChildMedacts(document filterDocument, Guid userId, int page = 1, int size = 10);
    }
}
