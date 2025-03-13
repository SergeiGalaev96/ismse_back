using ISMSE_REST_API.Contracts.DataProviders;
using ISMSE_REST_API.Contracts.MedactProcesses.Verification;
using ISMSE_REST_API.Extensions;
using ISMSE_REST_API.Models;
using ISMSE_REST_API.Models.Enums;
using ISMSE_REST_API.Models.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ISMSE_REST_API.Services.MedactProcesses.Verification
{
    public class AdultControl : IDuplicateControl
    {
        private readonly ICissaDataAccessLayer _cissaDAL;
        public AdultControl(ICissaDataAccessLayer cissaDAL)
        {
            _cissaDAL = cissaDAL;
        }
        public void VerifyExisting(Guid personId)
        {
            var docDefId = CustomExportAdultState.APPROVED_AND_REGISTERED.GetDefId();
            var inStates = CustomExportAdultState.APPROVED_AND_REGISTERED.GetValueId().Concat(CustomExportAdultState.ON_REGISTERING.GetValueId());
            int existingCount = _cissaDAL.CountDocumentsByPersonIdAndInStates(docDefId, personId, inStates);
            if (existingCount > 0)
                throw new DuplicateControlException("На данного гражданина уже существует запись в базе ИСМСЭ");
        }
    }
}