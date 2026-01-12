using ISMSE_REST_API.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ISMSE_REST_API.Models.Enums
{
    [DocDef("{5FDE415F-DB00-43B4-BA6E-FE439CFF6DA0}", "ChildMedAct")]
    public enum CustomExportChildState
    {
        [EnumValue("32062CB7-C31C-4AFB-ADF3-F9F9AEEFCE59,C1414D0C-417A-45AB-8B57-01D30A567F08", "Утвержден (подписан),Зарегистрирован")]
        APPROVED_AND_REGISTERED = 1,
        [EnumValue("D203372C-236B-4B3E-953A-11F09A4ACA61", "Истекший срок инвалидности")]
        DISABILITY_EXPIRED,
        [EnumValue("C9CFE461-E822-4F97-B34D-9312F8917FA9", "Переехал за переделы РТ")]
        MIGRATED_OUTSIDE_COUNTRY,
        [EnumValue("1FA96E5B-908F-4C57-BC4A-B61423951081", "Перешел на возрастную пенсию")]
        RETIRED_BY_AGE,
        [EnumValue("10A96E4B-DF5D-4F55-9D81-83F9E3E51DE5", "Приостановлена инвалидность")]
        PAUSED,
        [EnumValue("A9E360F0-EED1-4FF6-9AB8-E4255C62CE4A", "Умерший")]
        DEAD,
        [EnumValue("58A0384C-F95A-4FBC-BF21-2BA1B61A3647", "На регистрации")]
        ON_REGISTERING,
        [EnumValue("D5B97FA0-9D87-4FB8-BDF1-B2F118D42640", "Архив")]
        ARCHIVE
    }
}