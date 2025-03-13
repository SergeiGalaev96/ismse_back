using ISMSE_REST_API.Contracts.DataProviders;
using ISMSE_REST_API.Contracts.Documents;
using ISMSE_REST_API.Contracts.MedactProcesses;
using ISMSE_REST_API.Extensions;
using ISMSE_REST_API.Models;
using ISMSE_REST_API.Models.Enums;
using ISMSE_REST_API.Models.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ISMSE_REST_API.Services.MedactProcesses
{
    public class TransferChildToAdultImpl : ITransferChildToAdult
    {
        private readonly IDataService _documentService;
        private readonly ICissaDataAccessLayer _cissaDAL;
        public TransferChildToAdultImpl(IDataService documentService, ICissaDataAccessLayer cissaDAL)
        {
            _documentService = documentService;
            _cissaDAL = cissaDAL;
        }

        private static readonly Guid AdultDocDefId = new Guid("b4dddc00-9ea9-4ad4-9c4f-498e87aa9828");
        public Guid TransferFromChildToAdult(document dto, Guid userId)
        {
            if(dto.attributes == null || dto.attributes.Length == 0)
                throw new ArgumentNullException("attributes", "Поля нового медакта не переданы");
            var personAttr = dto.attributes.FirstOrDefault(x => x.name == "Person");
            if (personAttr == null)
                throw new ArgumentNullException("Person", "Поле Person не передано");
            if(string.IsNullOrEmpty(personAttr.value))
                throw new ArgumentNullException("Person", "Поле Person пусто");
            if(!Guid.TryParse(personAttr.value, out Guid personId))
                throw new ArgumentNullException("Person", "Значение поля Person некорректно! Должно быть в формате Guid");


            //TODO: CallDuplicateControl


            var childSignedDocuments = _cissaDAL.GetSignedChildMedActsByPersonId(personId, userId);
            if (childSignedDocuments.Length == 0)
                throw new TransferException("Не могу сделать перевод, подписанный детский медакт отсутствуют");

            
            //call person modifier service


            foreach (var childId in childSignedDocuments)
            {
                _cissaDAL.SetState(childId, CustomExportChildState.DISABILITY_EXPIRED.GetValueId()[0], userId);
            }



            var newAdultDocumentId = _documentService.CreateWithNo(AdultDocDefId, userId, "No", dto);
            _cissaDAL.SetState(newAdultDocumentId, CustomExportChildState.ON_REGISTERING.GetValueId()[0], userId);

            return newAdultDocumentId;
        }
    }
}