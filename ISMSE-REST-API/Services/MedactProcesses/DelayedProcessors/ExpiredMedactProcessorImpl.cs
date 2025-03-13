using ISMSE_REST_API.Contracts.Documents;
using ISMSE_REST_API.Contracts.MedactProcesses.DelayedProcessors.Medact;
using ISMSE_REST_API.Extensions;
using ISMSE_REST_API.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ISMSE_REST_API.Services.MedactProcesses.DelayedProcessors
{
    public class ExpiredMedactProcessorImpl : IExpiredMedactProcessor
    {
        private readonly IDataService _dataService;
        public ExpiredMedactProcessorImpl(IDataService dataService)
        {
            _dataService = dataService;
        }
        private DateTime deadlineDate = DateTime.Today;
        public void Execute()
        {
            SetExpiredState(ChildExpiredEntries, CustomExportChildState.DISABILITY_EXPIRED.GetValueId()[0]);
            SetExpiredState(AdultExpiredEntries, CustomExportAdultState.DISABILITY_EXPIRED.GetValueId()[0]);
        }
        public Guid[] ChildExpiredEntries => _dataService.FetchApprovedDocumentsLessThanDate(CustomExportChildState.APPROVED_AND_REGISTERED, deadlineDate);
        public Guid[] AdultExpiredEntries => _dataService.FetchApprovedDocumentsLessThanDate(CustomExportAdultState.APPROVED_AND_REGISTERED, deadlineDate);
        private void SetExpiredState(Guid[] expiredEntries, Guid expiredStateTypeId)
        {
            int.TryParse(System.Configuration.ConfigurationManager.AppSettings["expireMedactMaxBatchSize"], out int maxBatchSize);
            if (maxBatchSize == 0) maxBatchSize = 10;
            foreach (var docId in expiredEntries.Take(maxBatchSize))
            {
                _dataService.SetState(docId, expiredStateTypeId);
                _dataService.UpdateDocument(docId, new Dictionary<string, object>
                {
                    { "StatusDate", DateTime.Now }
                });
            }
        }
    }
}