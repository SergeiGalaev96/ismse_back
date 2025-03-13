using ISMSE_REST_API.Contracts.MedactProcesses.Verification;
using ISMSE_REST_API.Models;
using ISMSE_REST_API.Models.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ISMSE_REST_API.Services.MedactProcesses.Verification
{
    public class PersonVerificationImpl : IPersonVerification
    {
        public void Verify(document dto, out Guid personId)
        {
            try
            {
                if (dto.attributes == null || dto.attributes.Length == 0)
                    throw new ArgumentNullException("attributes", "Поля нового медакта не переданы");
                var personAttr = dto.attributes.FirstOrDefault(x => x.name == "Person");
                if (personAttr == null)
                    throw new ArgumentNullException("Person", "Поле Person не передано");
                if (string.IsNullOrEmpty(personAttr.value))
                    throw new ArgumentNullException("Person", "Поле Person пусто");
                if (!Guid.TryParse(personAttr.value, out personId))
                    throw new ArgumentNullException("Person", "Значение поля Person некорректно! Должно быть в формате Guid");
            }
            catch (ArgumentNullException e)
            {
                throw new PersonVerificationException(e.Message, e);
            }
        }
    }
}