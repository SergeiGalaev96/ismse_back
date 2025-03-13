using ISMSE_REST_API.Contracts.DataProviders;
using ISMSE_REST_API.Contracts.Documents;
using ISMSE_REST_API.Models;
using ISMSE_REST_API.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ISMSE_REST_API.Services.Documents
{
    public class DataServiceImpl : IDataService
    {
        private readonly ICissaDataAccessLayer _cissaDAL;
        public DataServiceImpl(ICissaDataAccessLayer cissaDAL)
        {
            _cissaDAL = cissaDAL;
        }
        public Guid CreateWithNo(Guid defId, Guid userId, string noAttrName, document document)
        {
            return _cissaDAL.CreateWithNo(defId, document, userId, true, noAttrName);
        }

        public Guid[] FetchApprovedDocumentsLessThanDate(Enum approvedState, DateTime date)
        {
            return _cissaDAL.FetchApprovedDocumentsLessThanDate(approvedState, date);
        }

        public void SetState(Guid documentId, Guid stateTypeId, Guid userId)
        {
            _cissaDAL.SetState(documentId, stateTypeId, userId);
        }

        public void SetState(Guid documentId, Guid stateTypeId)
        {
            _cissaDAL.SetState(documentId, stateTypeId);
        }

        public void UpdateDocument(Guid documentId, Dictionary<string, object> fields)
        {
            _cissaDAL.ModifyDocument(documentId, fields);
        }
    }
}