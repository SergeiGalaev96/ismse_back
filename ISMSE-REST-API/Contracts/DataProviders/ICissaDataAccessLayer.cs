using ISMSE_REST_API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISMSE_REST_API.Contracts.DataProviders
{
    public interface ICissaDataAccessLayer
    {
        void SetState(Guid documentId, Guid stateTypeId);
        void SetState(Guid documentId, Guid stateTypeId, Guid userId);
        void ModifyDocument(Guid documentId, Dictionary<string, object> extendProps);
        void ModifyDocument(Guid documentId, Guid userId, Dictionary<string, object> extendProps);
        string GetUserName(Guid userId);

        Guid CreateWithNo(Guid defId, document document, Guid userId, bool withNo = false, string noAttrName = "");

        Guid[] GetSignedChildMedActsByPersonId(Guid personId, Guid userId);
        document[] FetchGreaterThan18YearsChildMedacts(document filterDocument, Guid userId, int page = 1, int size = 10);
        Guid[] FetchApprovedDocumentsLessThanDate(Enum approvedState, DateTime date);
        int CountDocumentsByPersonIdAndInStates(Guid docDefId, Guid personId, IEnumerable<Guid> inStates);
    }
}
