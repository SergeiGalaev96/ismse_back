using ISMSE_REST_API.Contracts.Delegates;
using ISMSE_REST_API.Contracts.Documents;
using ISMSE_REST_API.Contracts.MedactProcesses;
using ISMSE_REST_API.Contracts.MedactProcesses.Verification;
using ISMSE_REST_API.Extensions;
using ISMSE_REST_API.Models;
using ISMSE_REST_API.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ISMSE_REST_API.Services.MedactProcesses
{
    public class AdultFacadeImpl : IAdultFacade
    {
        private readonly IDataService _dataService;
        private readonly DuplicateControlServiceResolver _serviceResolver;
        private readonly IPersonVerification _personVerification;
        public AdultFacadeImpl(IDataService dataService, DuplicateControlServiceResolver serviceResolver,
            IPersonVerification personVerification)
        {
            _dataService = dataService;
            _serviceResolver = serviceResolver;
            _personVerification = personVerification;
        }
        public document CreateNew(document dto, Guid userId)
        {
            _personVerification.Verify(dto, out Guid personId);
            var duplicateControl = _serviceResolver("Adult");
            duplicateControl.VerifyExisting(personId);

            var newAdultMedactId = _dataService.CreateWithNo(CustomExportAdultState.APPROVED_AND_REGISTERED.GetDefId(), userId, "No", dto);

            _dataService.SetState(newAdultMedactId, CustomExportAdultState.ON_REGISTERING.GetValueId()[0], userId);

            dto.id = newAdultMedactId;
            return dto;
        }
    }
}