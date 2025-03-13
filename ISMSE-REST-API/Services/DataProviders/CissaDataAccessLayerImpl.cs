using Intersoft.CISSA.DataAccessLayer.Model.Workflow;
using ISMSE_REST_API.Contracts.DataProviders;
using ISMSE_REST_API.Models;
using ISMSE_REST_API.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ISMSE_REST_API.Services.DataProviders
{
    public class CissaDataAccessLayerImpl : ICissaDataAccessLayer
    {

        public void SetState(Guid documentId, Guid stateTypeId)
        {
            var context = ScriptExecutor.CreateAdminContext();
            var docRepo = context.Documents;

            docRepo.SetDocState(documentId, stateTypeId);
        }
        public void SetState(Guid documentId, Guid stateTypeId, Guid userId)
        {
            var context = ScriptExecutor.CreateContext(userId);
            var docRepo = context.Documents;

            docRepo.SetDocState(documentId, stateTypeId);
        }

        public void ModifyDocument(Guid documentId, Guid userId, Dictionary<string, object> extendProps)
            => ModifyDocument(documentId, extendProps, ScriptExecutor.CreateContext(userId));

        public void ModifyDocument(Guid documentId, Dictionary<string, object> extendProps)
            => ModifyDocument(documentId, extendProps, ScriptExecutor.CreateAdminContext());
        private void ModifyDocument(Guid documentId, Dictionary<string, object> extendProps, WorkflowContext context)
        {
            var docRepo = context.Documents;

            if (extendProps.Count == 0) return;
            var doc = docRepo.LoadById(documentId);
            foreach (var item in extendProps)
            {
                doc[item.Key] = item.Value;
            }
            docRepo.Save(doc);
        }

        public string GetUserName(Guid userId)
        {
            return DAL.GetCissaUser(userId).UserName;
        }

        public Guid CreateWithNo(Guid defId, document document, Guid userId, bool withNo = false, string noAttrName = "")
        {
            return ScriptExecutor.CreateDocument(defId, document, userId, withNo, noAttrName);
        }

        public Guid[] GetSignedChildMedActsByPersonId(Guid personId, Guid userId)
        {
            return ScriptExecutor.GetSignedChildMedActsByPersonId(personId, userId);
        }

        public document[] FetchGreaterThan18YearsChildMedacts(document filterDocument, Guid userId, int page = 1, int size = 10)
        {
            return ScriptExecutor.FetchGreaterThan18YearsChildMedacts(filterDocument, userId, page, size);
        }

        public int CountDocumentsByPersonIdAndInStates(Guid docDefId, Guid personId, IEnumerable<Guid> inStates)
        {
            return ScriptExecutor.CountDocumentsByPersonIdAndInStates(docDefId, personId, inStates.Cast<object>().ToArray());
        }
        public Guid[] FetchApprovedDocumentsLessThanDate(Enum approvedState, DateTime date)
        {
            return ScriptExecutor.FetchApprovedDocumentsLessThanDate(approvedState, date);
        }
    }
}