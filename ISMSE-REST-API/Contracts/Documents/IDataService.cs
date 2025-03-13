using ISMSE_REST_API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISMSE_REST_API.Contracts.Documents
{
    public interface IDataService
    {
        Guid CreateWithNo(Guid defId, Guid userId, string noAttrName, document document);
        void SetState(Guid documentId, Guid stateTypeId);
        void SetState(Guid documentId, Guid stateTypeId, Guid userId);
        Guid[] FetchApprovedDocumentsLessThanDate(Enum approvedState, DateTime date);
        void UpdateDocument(Guid documentId, Dictionary<string, object> fields);
    }
}
