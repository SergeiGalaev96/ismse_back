﻿using ISMSE_REST_API.Contracts.DataProviders;
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
    public class ChildControl : IDuplicateControl
    {
        private readonly ICissaDataAccessLayer _cissaDAL;
        public ChildControl(ICissaDataAccessLayer cissaDAL)
        {
            _cissaDAL = cissaDAL;
        }
        public void VerifyExisting(Guid personId)
        {
            var docDefId = CustomExportChildState.APPROVED_AND_REGISTERED.GetDefId();
            var inStates = CustomExportChildState.APPROVED_AND_REGISTERED.GetValueId().Concat(CustomExportChildState.ON_REGISTERING.GetValueId());
            int existingCount = _cissaDAL.CountDocumentsByPersonIdAndInStates(docDefId, personId, inStates);
            if (existingCount > 0)
                throw new DuplicateControlException("На данного гражданина уже существует запись в базе ИСМСЭ");
        }
    }
}