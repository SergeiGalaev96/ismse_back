using ISMSE_REST_API.Contracts.DataProviders;
using ISMSE_REST_API.Contracts.Status;
using ISMSE_REST_API.Models;
using ISMSE_REST_API.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ISMSE_REST_API.Services.Status
{
    public class StatusRepositoryImpl : IStatusRepository
    {
        private readonly Guid TRASH_STATE_TYPE_ID = new Guid("{3A1C9A50-CD0C-4D4C-A680-5D4A6DB6F2F8}");
        private readonly ICissaDataAccessLayer _statusDataProvider;
        public StatusRepositoryImpl(ICissaDataAccessLayer statusDataProvider)
        {
            _statusDataProvider = statusDataProvider;
        }
        public void SetTrashStatus(Guid documentId, Guid userId)
        {
            _statusDataProvider.SetState(documentId, TRASH_STATE_TYPE_ID, userId);
        }

        public void SetStatusManually(Guid documentId, Guid statusId, Guid userId, DateTime statusDate)
        {
            _statusDataProvider.SetState(documentId, statusId, userId);
            _statusDataProvider.ModifyDocument(documentId, userId, new Dictionary<string, object>
            {
                { "StatusDate", statusDate },
                { "StatusAuthor", _statusDataProvider.GetUserName(userId) }
            });
        }
    }
}