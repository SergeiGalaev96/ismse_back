using ISMSE_REST_API.Contracts.Builders;
using ISMSE_REST_API.Contracts.Infrastructure.Logging;
using ISMSE_REST_API.Extensions;
using ISMSE_REST_API.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ISMSE_REST_API.Services.Builders
{
    public class TSqlQueryBuilderCustomImpl : ITSqlQueryBuilder
    {
        private readonly ILogManager _logManager;
        public TSqlQueryBuilderCustomImpl(ILogManager logManager)
        {
            _logManager = logManager;
        }
        public string BuildSql(Enum[] state, DateTime startDate, DateTime endDate, Guid? msecId = null, int? regionId = null, int? districtId = null)
        {
            string sql = buildSqlWithStateCondition(state);

            var paramList = new Dictionary<string, object>
            {
                { "[MseName]", msecId },
                { "[Region]", regionId },
                { "[District]", districtId }
            };

            sql = applyConditions(sql,
                buildRangeConditions(startDate, endDate)
                .Concat(buildEqualConditions(paramList))
                .ToArray());

            sql = applyLimits(sql);

            _logManager.WriteLog(sql);

            return sql;
        }
        string applyLimits(string sql)
        {
            var isTrunc = true;
            if (System.Configuration.ConfigurationManager.AppSettings.Keys.Cast<string>().Contains("customExportTruncated"))
                bool.TryParse(System.Configuration.ConfigurationManager.AppSettings["customExportTruncated"], out isTrunc);

            return GetSelectStatement[isTrunc] + sql;
        }
        string buildSqlWithStateCondition(Enum[] state)
        {
            string result;
            if (state[0] is CustomExportAdultState)
                result = T_SQL_GET_DATA_MSEC_GROWN;
            else if (state[0] is CustomExportChildState)
                result = T_SQL_GET_DATA_MSEC_CHILD;
            else throw new ArgumentException("Тип справочника не распознан!");
            var states = state.SelectMany(x => x.GetValueId());
            return string.Format(result, "'" + string.Join("','", states) + "'");
        }
        string[] buildEqualConditions(Dictionary<string, object> paramList)
        {
            var equalConditions = new List<string>();
            foreach (var item in paramList)
            {
                if (!string.IsNullOrEmpty(item.Key) && item.Value != null)
                    equalConditions.Add(string.Format(EQUAL_CONDITION_SQL, item.Key, convertValueToSqlValue(item.Value)));
            }
            return equalConditions.ToArray();
        }
        string convertValueToSqlValue(object val)
        {
            if (val == null)
                throw new ArgumentNullException("val", "передан пустой параметр для распознавания в SQL запрос");
            if (val is Guid guidVal)
                return $"'{guidVal}'";
            else if (val is int intVal)
                return $"{intVal}";
            else if (val is DateTime dateVal)
                return $"'{dateVal:yyyyMMdd}'";
            throw new ArgumentException($"нераспознанный тип для SQL: {val.GetType().Name}", "val");
        }
        string[] buildRangeConditions(DateTime begin, DateTime end)
        {
            if (begin > end)
                throw new ArgumentOutOfRangeException("begin", begin, "Дата начала не должен превышать дату конца!");
            return new[]
            {
                string.Format(GREAT_OR_EQUAL_CONDITION_SQL, "[Date]", convertValueToSqlValue(begin)),
                string.Format(LESS_OR_EQUAL_CONDITION_SQL, "[Date]", convertValueToSqlValue(end))
            };
        }
        string applyConditions(string sql, string[] conditions)
        {
            var conditions_T_SQL = $"WHERE {string.Join(" AND ", conditions)}";
            return $"{sql}\n{conditions_T_SQL}";
        }
        IDictionary<bool, string> GetSelectStatement = new Dictionary<bool, string>
        {
            { true, SELECT_TOP_20 },
            { false, SELECT }
        };
        const string SELECT_TOP_20 = "SELECT TOP 20";
        const string SELECT = "SELECT";
        const string T_SQL_GET_DATA_MSEC_GROWN = @"
        [*Person].[Last_Name] 'Фамилия',
        [*Person].[First_Name] 'Имя',
        [*Person].[Middle_Name] 'Отчество',
        [*Person].[Date_of_Birth] 'Д.р.',
        [SexText].Full_Name as 'Пол',
		--[*AdultsMedicalCart].State_Type_Id,
		[PassportTypeText].Full_Name as 'Тип документа',
        [*Person].[PassportNo] '№ паспорта',
        [*Person].[Date_of_Issue] 'Дата выдачи',
        [*Person].[Issuing_Authority] 'Выдавший орган',
        [FamilyStateText].Full_Name as 'Семейное положение',
		[*AdultsMedicalCart].Status_Grown as 'Статус',
        [*Person].[NationalID] 'Национальный ID',
        [DisabilityGroupText].Full_Name as 'Группа инв',
        --[ConfirmationExistenceText].Full_Name as 'Подтверждение о существования инвалида',
        [MseNameText].Full_Name as 'МСЭК',
        [OccupationText].Full_Name as 'Род деятельности',
        [EducationActText].Full_Name as 'Образование',
        [ExaminationPrReText].Full_Name as 'Вид освидетельствования',
        [ExaminationPlaceText].Full_Name as 'Место освидетельствования',
        [PensionStatusText].Full_Name as 'Получает пенсию',
        [DisabilityReasonText].Full_Name as 'Причина инвалидности',
        [WorkingConditionsText].Full_Name as 'Показанные и противопоказанные условия труда',
        [CompainDagnosisMKB3Text].Full_Name as 'Соп. диагноз по МКБ 3',
        [CompainDagnosisMKB2Text].Full_Name as 'Соп. диагноз по МКБ 2',
        [CompainDagnosisMKB1Text].Full_Name as 'Соп. диагноз по МКБ 1',
        [*AdultsMedicalCart].[MainDiagnosisValue] as 'Основной диагноз-byte',--dbo.DecodeUTF8String([*AdultsMedicalCart].[MainDiagnosisValue]) as 'Основной',
        [DiagnosisText].Full_Name as 'Диагноз',
		[*AdultsMedicalCart].[Date] as 'Дата',
        [*AdultsMedicalCart].[PhoneNumber] as 'Телефон',
        [*AdultsMedicalCart].[ReasonNotPension] as 'Причина если не получает пенсию',
        [*AdultsMedicalCart].[DisabilityDate] as 'Дата установления инвалидности',
        [*AdultsMedicalCart].[ReceivesPension] as 'Инвалид получает пенсию',
        --[*AdultsMedicalCart].[Died] as 'Умер (месяц/год)',
        [*AdultsMedicalCart].[DisabledEmployment] as 'Занятость инвалида',
        [*AdultsMedicalCart].[Diseases] as 'Болезни дополнительно',
        [*AdultsMedicalCart].[Indefinitely] as 'Бессрочно',
        [*AdultsMedicalCart].[Temporarily] as 'Временно:  до',
        [*AdultsMedicalCart].[TimeWork] as 'Где и с какого времени работает',
        [*AdultsMedicalCart].[PensionAmount] as 'Размер пенсии (сомони/дирам)',
        [*AdultsMedicalCart].[Height] as 'Рост',
        [*AdultsMedicalCart].[Weight] as 'Вес',
        [*AdultsMedicalCart].[Profession] as 'Основная профессия (уточняется врачом экспертом)',
        [*AdultsMedicalCart].[RegNoAct] as '№ мед. акта',
        [*AdultsMedicalCart].[MedicalOrgAddress] as 'Адрес направившей лечебной организации',
        [*AdultsMedicalCart].[MedicalOrgName] as 'Наименование лечебной организации направившей на МСЭ',
        [*AdultsMedicalCart].[Wage] as 'Среднемесячный заработок в последний год',
        [*AdultsMedicalCart].[PlaceOfWork] as 'Место работы',
        [*AdultsMedicalCart].[Position] as 'Должность',
        [*AdultsMedicalCart].[ReferenceNumber] as '№/Серия справки',
        [*AdultsMedicalCart].[SickListType] as 'Вид',
        [*AdultsMedicalCart].[SickListPeriodTo] as 'Период по',
        [*AdultsMedicalCart].[SickListPeriodFrom] as 'Период с',
        [*AdultsMedicalCart].[SickListSeries] as 'Серия',
        /*dbo.DecodeUTF8String([*AdultsMedicalCart].[ExpertExamResult8Value]) as 'Данные врачей других специальностей',
        dbo.DecodeUTF8String([*AdultsMedicalCart].[ExpertExamResult7Value]) as 'Данные эксперта невропатолога и психиатра',
        dbo.DecodeUTF8String([*AdultsMedicalCart].[ExpertExamResultValue]) as 'Жалобы',
        dbo.DecodeUTF8String([*AdultsMedicalCart].[ExpertExamResult9Value]) as 'Данные врача-окулиста',
        dbo.DecodeUTF8String([*AdultsMedicalCart].[ExpertExamResult10Value]) as 'Данные врача-кардиолога',
        dbo.DecodeUTF8String([*AdultsMedicalCart].[ExpertExamResult3Value]) as 'Результаты доп. спец-х исследований',
        dbo.DecodeUTF8String([*AdultsMedicalCart].[ExpertExamResult4Value]) as 'Данные врача-терапевта',
        dbo.DecodeUTF8String([*AdultsMedicalCart].[ExpertExamResult1Value]) as 'Клинико-трудовой анамнез',
        dbo.DecodeUTF8String([*AdultsMedicalCart].[ExpertExamResult2Value]) as 'Данные лабораторных и рентгенологических исследований',
        dbo.DecodeUTF8String([*AdultsMedicalCart].[ExpertExamResult4CommentValue]) as 'Комментарий',
        dbo.DecodeUTF8String([*AdultsMedicalCart].[ExpertExamResult5CommentValue]) as 'Комментарий',
        dbo.DecodeUTF8String([*AdultsMedicalCart].[ExpertExamResult9CommentValue]) as 'Комментарий',
        dbo.DecodeUTF8String([*AdultsMedicalCart].[ExpertExamResult6CommentValue]) as 'Комментарий',
        dbo.DecodeUTF8String([*AdultsMedicalCart].[ExpertExamResult6Value]) as 'Данные врача-офтальмолог',
        dbo.DecodeUTF8String([*AdultsMedicalCart].[ExpertExamResult7CommentValue]) as 'Комментарий',
        dbo.DecodeUTF8String([*AdultsMedicalCart].[ExpertExamResult10CommentValue]) as 'Комментарий',
        dbo.DecodeUTF8String([*AdultsMedicalCart].[ExpertExamResult5Value]) as 'Данные врача-хирурга',*/
        [*AdultsMedicalCart].[Easily] as 'Легко',
        [*AdultsMedicalCart].[CantManage] as 'Не справляется',
        [*AdultsMedicalCart].[FullTimeWork] as 'Полный рабочий день',
        [*AdultsMedicalCart].[AdditionalInterruptions] as 'Дополнительными перерывами',
        [*AdultsMedicalCart].[PartTime] as 'Неполным рабочим днем',
        [*AdultsMedicalCart].[Difficulties] as 'С трудом',
        [*AdultsMedicalCart].[AnIncomWorkWeek] as 'Неполной рабочей неделей',
        [*AdultsMedicalCart].[goal9] as 'Оформление/продление листков нетрудоспособности, и т.д.)',
        [*AdultsMedicalCart].[goal8] as 'Нуждается в протезно-ортопедических средствах, помощи специализированных учреждений (скл, ди и т.п.)',
        [*AdultsMedicalCart].[goal5] as 'Заключение на: кресло-коляски',
        [*AdultsMedicalCart].[goal7] as 'Нуждается в специализированных средствах помощи (коляски, слуховой аппарат, и т.д.)',
        [*AdultsMedicalCart].[goalAnother] as 'Другое',
        [*AdultsMedicalCart].[goal2] as 'Изменение причины инвалидности',
        [*AdultsMedicalCart].[goal3] as 'Определение степени утраты трудоспособности в %',
        [*AdultsMedicalCart].[goal6] as 'По ухудшению',
        [*AdultsMedicalCart].[goal4] as 'Дом интернат',
        [*AdultsMedicalCart].[goal1] as 'Для установления группы инвалидности',
        [*AdultsMedicalCart].[NeedsSupervision] as 'Нуждается в надзоре',
        [*AdultsMedicalCart].[NeedsCare] as 'Нуждается в уходе',
        [*AdultsMedicalCart].[RestorativeTherapy] as 'Восстановительная терапия',
        [*AdultsMedicalCart].[ReconstructiveSurgery] as 'Реконструктивная  хирургия',
        [*AdultsMedicalCart].[HospitalTreatment] as 'Амбулаторное лечение',
        [*AdultsMedicalCart].[Hospitalization] as 'Стационарное лечение',
        [*AdultsMedicalCart].[Indefinitely] as 'Бессрочно',
        [*AdultsMedicalCart].[ExamDateFrom] as 'Инвалидность установлено с',
        [*AdultsMedicalCart].[ExamDateTo] as 'Инвалидность установлено по',
        [*AdultsMedicalCart].[ExamOfDate] as 'Дата проведения экспертизы',
        [*AdultsMedicalCart].[ExamStartDate] as 'Дата начала экспертизы',
        [*AdultsMedicalCart].[ExamFinishDate] as 'Дата окончания экспертизы',
        [*AdultsMedicalCart].[WorkingConditions7] as 'Надомный труд',
        [*AdultsMedicalCart].[WorkingConditions3] as 'Рабочая поза',
        [*AdultsMedicalCart].[WorkingConditions4] as 'Сложность, напряженность',
        [*AdultsMedicalCart].[WorkingConditions1] as 'Нервно-психическое напряжение',
        [*AdultsMedicalCart].[WorkingConditions6] as 'Умственный труд',
        [*AdultsMedicalCart].[WorkingConditions5] as 'Физический труд',
        [*AdultsMedicalCart].[WorkingConditions2] as 'Предписанный темп работы',
        --dbo.DecodeUTF8String([*AdultsMedicalCart].[DiseaseComplicationValue]) as 'Осложнение болезни',
        
        [*AdultsMedicalCart].[Region] as 'Область-ref',
        [*AdultsMedicalCart].[District] as 'Район-ref',
        [*AdultsMedicalCart].[subDistrict] as 'Джамоат-ref',
        [*AdultsMedicalCart].[Village] as 'Населенный пункт-ref',
        [*AdultsMedicalCart].[Phone] as '№ телефона',
        [*AdultsMedicalCart].[ResidentialAddress] as 'Домашний адрес (ул/дом/кв)',
        [*AdultsMedicalCart].[Country] as 'Страна',
        [*AdultsMedicalCart].[Additionally] as 'другое',
        [*AdultsMedicalCart].[OwnHouse] as 'собственный дом',
        [*AdultsMedicalCart].[Good] as 'хорошие',
        [*AdultsMedicalCart].[SeparateApartment] as 'отдельная квартира',
        [*AdultsMedicalCart].[LivingAreaRemovable] as 'жилая площадь съемная',
        [*AdultsMedicalCart].[Unsatisfactory] as 'неудовлетворительные',
        [*AdultsMedicalCart].[Dormitory] as 'общежитие',
        [*AdultsMedicalCart].[NoOwnAccommodation] as 'нет собственного жилья',
        [*AdultsMedicalCart].[Satisfactory] as 'удовлетворительные',
		[*AdultsMedicalCart].[state_created_at] 'Дата изменения статуса',
		status_users.Last_Name + ' ' + status_users.First_Name 'Пользователь изменивший статус'
FROM
        (SELECT
                d.Id,
				ds130.State_Type_Id,
				ds130.Created state_created_at,
				ds130.Worker_Id StatusAuthorUserId,
				o.Full_Name as Status_Grown,
                [a1].[Value] as [Date],
                [a2].[Value] as [OrgProfile],
                [a3].[Value] as [No],
                [a4].[Value] as [PhoneNumber],
                [a5].[Value] as [FamilyState],
                [a6].[Value] as [DisabilityGroup],
                [a7].[Value] as [DisabilityCategory],
                [a8].[Value] as [ConfirmationExistence],
                [a9].[Value] as [ReasonNotPension],
                [a10].[Value] as [DisabilityDate],
                [a11].[Value] as [Education],
                [a12].[Value] as [PassportNo],
                [a13].[Value] as [Last_Name],
                [a14].[Value] as [MseName],
                [a15].[Value] as [Sex],
                [a16].[Value] as [ReceivesPension],
                [a17].[Value] as [Died],
                [a18].[Value] as [First_Name],
                [a19].[Value] as [Date_of_Birth],
                [a20].[Value] as [DisabledEmployment],
                [a21].[Value] as [Middle_Name],
                [a22].[Value] as [Diseases],
                [a23].[Value] as [PassportType],
                [a24].[Value] as [Indefinitely],
                [a25].[Value] as [Temporarily],
                [a26].[Value] as [Occupation],
                [a27].[Value] as [TimeWork],
                [a28].[Value] as [EducationAct],
                [a29].[Value] as [PensionAmount],
                [a30].[Value] as [Height],
                [a31].[Value] as [Profession],
                [a32].[Value] as [Person],
                [a33].[Value] as [RegNoAct],
                [a34].[Value] as [MedicalOrgAddress],
                [a35].[Value] as [MedicalOrgName],
                [a36].[Value] as [Wage],
                [a37].[Value] as [PlaceOfWork],
                [a38].[Value] as [Position],
                [a39].[Value] as [ExaminationPrRe],
                [a40].[Value] as [Weight],
                [a41].[Value] as [Type],
                [a42].[Value] as [ExaminationPlace],
                [a43].[Value] as [PensionStatus],
                [a44].[Value] as [ReferenceNumber],
                [a45].[Value] as [SickListType],
                [a46].[Value] as [SickListPeriodTo],
                [a47].[Value] as [SickListPeriodFrom],
                [a48].[Value] as [SickListSeries],
                [a49].[File_Name] as [ExpertExamResult8],
                [a49].[Value] as [ExpertExamResult8Value],
                [a50].[File_Name] as [ExpertExamResult7],
                [a50].[Value] as [ExpertExamResult7Value],
                [a51].[File_Name] as [ExpertExamResult],
                [a51].[Value] as [ExpertExamResultValue],
                [a52].[File_Name] as [ExpertExamResult9],
                [a52].[Value] as [ExpertExamResult9Value],
                [a53].[File_Name] as [ExpertExamResult10],
                [a53].[Value] as [ExpertExamResult10Value],
                [a54].[File_Name] as [ExpertExamResult3],
                [a54].[Value] as [ExpertExamResult3Value],
                [a55].[File_Name] as [ExpertExamResult4],
                [a55].[Value] as [ExpertExamResult4Value],
                [a56].[File_Name] as [ExpertExamResult1],
                [a56].[Value] as [ExpertExamResult1Value],
                [a57].[File_Name] as [ExpertExamResult2],
                [a57].[Value] as [ExpertExamResult2Value],
                [a58].[File_Name] as [ExpertExamResult4Comment],
                [a58].[Value] as [ExpertExamResult4CommentValue],
                [a59].[File_Name] as [ExpertExamResult5Comment],
                [a59].[Value] as [ExpertExamResult5CommentValue],
                [a60].[File_Name] as [ExpertExamResult9Comment],
                [a60].[Value] as [ExpertExamResult9CommentValue],
                [a61].[File_Name] as [ExpertExamResult6Comment],
                [a61].[Value] as [ExpertExamResult6CommentValue],
                [a62].[File_Name] as [ExpertExamResult6],
                [a62].[Value] as [ExpertExamResult6Value],
                [a63].[File_Name] as [ExpertExamResult7Comment],
                [a63].[Value] as [ExpertExamResult7CommentValue],
                [a64].[File_Name] as [ExpertExamResult10Comment],
                [a64].[Value] as [ExpertExamResult10CommentValue],
                [a65].[File_Name] as [ExpertExamResult5],
                [a65].[Value] as [ExpertExamResult5Value],
                [a66].[Value] as [Easily],
                [a67].[Value] as [CantManage],
                [a68].[Value] as [FullTimeWork],
                [a69].[Value] as [AdditionalInterruptions],
                [a70].[Value] as [PartTime],
                [a71].[Value] as [Difficulties],
                [a72].[Value] as [AnIncomWorkWeek],
                [a73].[Value] as [goal9],
                [a74].[Value] as [goal8],
                [a75].[Value] as [goal5],
                [a76].[Value] as [goal7],
                [a77].[Value] as [goalAnother],
                [a78].[Value] as [goal2],
                [a79].[Value] as [goal3],
                [a80].[Value] as [goal6],
                [a81].[Value] as [goal4],
                [a82].[Value] as [goal1],
                [a83].[Value] as [NeedsSupervision],
                [a84].[Value] as [NeedsCare],
                [a85].[Value] as [DisabilityReason],
                [a86].[Value] as [RestorativeTherapy],
                [a87].[Value] as [ReconstructiveSurgery],
                [a88].[Value] as [DisabilityGroupAct],
                [a89].[Value] as [HospitalTreatment],
                [a90].[Value] as [Hospitalization],
                [a91].[Value] as [WorkingConditions],
                [a92].[Value] as [ExamDateFrom],
                [a93].[Value] as [ExamDateTo],
                [a94].[Value] as [ExamOfDate],
                [a95].[Value] as [ExamStartDate],
                [a96].[Value] as [ExamFinishDate],
                [a97].[Value] as [WorkingConditions7],
                [a98].[Value] as [WorkingConditions3],
                [a99].[Value] as [WorkingConditions4],
                [a100].[Value] as [WorkingConditions1],
                [a101].[Value] as [WorkingConditions6],
                [a102].[Value] as [WorkingConditions5],
                [a103].[Value] as [WorkingConditions2],
                [a104].[File_Name] as [DiseaseComplication],
				[a104].[Value] as [DiseaseComplicationValue],
                [a105].[Value] as [CompainDagnosisMKB3],
                [a106].[Value] as [CompainDagnosisMKB1],
                [a107].[File_Name] as [CompanionDiagnosis],
                [a107].[Value] as [CompanionDiagnosisValue],
                [a108].[File_Name] as [MainDiagnosis],
                [a108].[Value] as [MainDiagnosisValue],
                [a109].[Value] as [Diagnosis],
                [a110].[Value] as [CompainDagnosisMKB2],
                [a111].[Value] as [Area],
                [a112].[Value] as [Village_Doc],
                [a113].[Value] as [Djamoat_Doc],
                [a114].[Value] as [Village],
                [a115].[Value] as [subDistrict],
                [a116].[Value] as [Region],
                [a117].[Value] as [District],
                [a118].[Value] as [Phone],
                [a119].[Value] as [ResidentialAddress],
                [a120].[Value] as [Country],
                [a121].[Value] as [Additionally],
                [a122].[Value] as [OwnHouse],
                [a123].[Value] as [Good],
                [a124].[Value] as [SeparateApartment],
                [a125].[Value] as [LivingAreaRemovable],
                [a126].[Value] as [Unsatisfactory],
                [a127].[Value] as [Dormitory],
                [a128].[Value] as [NoOwnAccommodation],
                [a129].[Value] as [Satisfactory],
                [ds130].[State_Type_Id] as [State]
        FROM
                Documents d WITH(NOLOCK)
                LEFT OUTER JOIN Date_Time_Attributes a1 WITH(NOLOCK) on (a1.Document_Id = d.Id and a1.Def_Id = '8233bfb2-b318-42ad-a1f4-077944bb26f2' and a1.Expired = '99991231')
                LEFT OUTER JOIN Document_Attributes a2 WITH(NOLOCK) on (a2.Document_Id = d.Id and a2.Def_Id = '466aa66d-be96-411c-9a07-538c27418fb8' and a2.Expired = '99991231')
                LEFT OUTER JOIN Text_Attributes a3 WITH(NOLOCK) on (a3.Document_Id = d.Id and a3.Def_Id = 'c4a0eaa9-afbc-465d-aa2b-9f19edb7d815' and a3.Expired = '99991231')
                LEFT OUTER JOIN Text_Attributes a4 WITH(NOLOCK) on (a4.Document_Id = d.Id and a4.Def_Id = 'bbe3b153-205d-4600-86b8-5c35f1a43b3f' and a4.Expired = '99991231')
                LEFT OUTER JOIN Enum_Attributes a5 WITH(NOLOCK) on (a5.Document_Id = d.Id and a5.Def_Id = '8dbe7c66-b99b-400b-a17c-0a4b7c5f8718' and a5.Expired = '99991231')
                LEFT OUTER JOIN Enum_Attributes a6 WITH(NOLOCK) on (a6.Document_Id = d.Id and a6.Def_Id = 'a7efdeb7-c353-4b15-a9c9-30cbd1b30b88' and a6.Expired = '99991231')
                LEFT OUTER JOIN Enum_Attributes a7 WITH(NOLOCK) on (a7.Document_Id = d.Id and a7.Def_Id = 'f4589b6a-b30d-47fb-8e1d-31b4a547599b' and a7.Expired = '99991231')
                LEFT OUTER JOIN Enum_Attributes a8 WITH(NOLOCK) on (a8.Document_Id = d.Id and a8.Def_Id = '2da2ee88-851e-4bc6-b33e-325a119ebdb5' and a8.Expired = '99991231')
                LEFT OUTER JOIN Text_Attributes a9 WITH(NOLOCK) on (a9.Document_Id = d.Id and a9.Def_Id = 'cfe99400-d21c-4400-92df-3ecf3e0f717f' and a9.Expired = '99991231')
                LEFT OUTER JOIN Date_Time_Attributes a10 WITH(NOLOCK) on (a10.Document_Id = d.Id and a10.Def_Id = '3e37f0cf-16c0-4531-8ab9-51bafd7dccc0' and a10.Expired = '99991231')
                LEFT OUTER JOIN Enum_Attributes a11 WITH(NOLOCK) on (a11.Document_Id = d.Id and a11.Def_Id = '39ca7cc9-9eba-4ade-a5f0-52d09ee7fa57' and a11.Expired = '99991231')
                LEFT OUTER JOIN Text_Attributes a12 WITH(NOLOCK) on (a12.Document_Id = d.Id and a12.Def_Id = 'c15d637b-1cbc-4b41-a033-539431d72803' and a12.Expired = '99991231')
                LEFT OUTER JOIN Text_Attributes a13 WITH(NOLOCK) on (a13.Document_Id = d.Id and a13.Def_Id = 'bb024f4b-10a1-4e59-8615-598274363b95' and a13.Expired = '99991231')
                LEFT OUTER JOIN Enum_Attributes a14 WITH(NOLOCK) on (a14.Document_Id = d.Id and a14.Def_Id = '4f293116-33ee-4acb-96b0-8b3ca0098fd4' and a14.Expired = '99991231')
                LEFT OUTER JOIN Enum_Attributes a15 WITH(NOLOCK) on (a15.Document_Id = d.Id and a15.Def_Id = 'b5945c5d-9fcb-4d76-8554-9db753a6342c' and a15.Expired = '99991231')
                LEFT OUTER JOIN Enum_Attributes a16 WITH(NOLOCK) on (a16.Document_Id = d.Id and a16.Def_Id = '4ff30256-3da7-47f8-a82e-a410ac1178de' and a16.Expired = '99991231')
                LEFT OUTER JOIN Text_Attributes a17 WITH(NOLOCK) on (a17.Document_Id = d.Id and a17.Def_Id = '4bd5e440-131e-4c10-bf21-b0c4460bd541' and a17.Expired = '99991231')
                LEFT OUTER JOIN Text_Attributes a18 WITH(NOLOCK) on (a18.Document_Id = d.Id and a18.Def_Id = '66d4e43b-942d-4a88-9060-c61af88f80d4' and a18.Expired = '99991231')
                LEFT OUTER JOIN Date_Time_Attributes a19 WITH(NOLOCK) on (a19.Document_Id = d.Id and a19.Def_Id = 'f8a3e5dc-0778-4339-8d7e-d3c4fe2e8bce' and a19.Expired = '99991231')
                LEFT OUTER JOIN Enum_Attributes a20 WITH(NOLOCK) on (a20.Document_Id = d.Id and a20.Def_Id = '6c1ab9c6-54eb-43b7-97e9-e395378fc4f5' and a20.Expired = '99991231')
                LEFT OUTER JOIN Text_Attributes a21 WITH(NOLOCK) on (a21.Document_Id = d.Id and a21.Def_Id = '6a2b2b76-b8da-4bbf-828b-efcaa92488d0' and a21.Expired = '99991231')
                LEFT OUTER JOIN Enum_Attributes a22 WITH(NOLOCK) on (a22.Document_Id = d.Id and a22.Def_Id = '0bb5373d-1e34-4061-ae07-f82170d832e7' and a22.Expired = '99991231')
                LEFT OUTER JOIN Enum_Attributes a23 WITH(NOLOCK) on (a23.Document_Id = d.Id and a23.Def_Id = '4baab7f7-2f9f-4d2d-b84c-fd641fe47b9b' and a23.Expired = '99991231')
                LEFT OUTER JOIN Boolean_Attributes a24 WITH(NOLOCK) on (a24.Document_Id = d.Id and a24.Def_Id = 'e01672ea-4999-4811-abfd-4afd46779be9' and a24.Expired = '99991231')
                LEFT OUTER JOIN Date_Time_Attributes a25 WITH(NOLOCK) on (a25.Document_Id = d.Id and a25.Def_Id = '77e79313-f358-46c4-b2ee-933f8f114adf' and a25.Expired = '99991231')
                LEFT OUTER JOIN Enum_Attributes a26 WITH(NOLOCK) on (a26.Document_Id = d.Id and a26.Def_Id = '2b7ee40f-d9f7-4faf-9f78-19e3b9954061' and a26.Expired = '99991231')
                LEFT OUTER JOIN Text_Attributes a27 WITH(NOLOCK) on (a27.Document_Id = d.Id and a27.Def_Id = 'eee032db-8ced-4f33-bb74-19fbf2fa2678' and a27.Expired = '99991231')
                LEFT OUTER JOIN Enum_Attributes a28 WITH(NOLOCK) on (a28.Document_Id = d.Id and a28.Def_Id = 'ef1eae27-8436-4fe5-9f19-1d014f7faab8' and a28.Expired = '99991231')
                LEFT OUTER JOIN Currency_Attributes a29 WITH(NOLOCK) on (a29.Document_Id = d.Id and a29.Def_Id = '01ac976a-9b11-476f-b841-2ac8f1d9aea3' and a29.Expired = '99991231')
                LEFT OUTER JOIN Float_Attributes a30 WITH(NOLOCK) on (a30.Document_Id = d.Id and a30.Def_Id = '17cb4ee3-6443-4ae4-8074-3a8824a4e5a5' and a30.Expired = '99991231')
                LEFT OUTER JOIN Text_Attributes a31 WITH(NOLOCK) on (a31.Document_Id = d.Id and a31.Def_Id = 'f9de4ecb-8e05-46ec-af31-4110e7cfa363' and a31.Expired = '99991231')
                LEFT OUTER JOIN Document_Attributes a32 WITH(NOLOCK) on (a32.Document_Id = d.Id and a32.Def_Id = '75a54f8d-06e2-4292-b0ad-534d3a3072f1' and a32.Expired = '99991231')
                LEFT OUTER JOIN Text_Attributes a33 WITH(NOLOCK) on (a33.Document_Id = d.Id and a33.Def_Id = '21605127-5fbc-41b0-8be7-6446a2113625' and a33.Expired = '99991231')
                LEFT OUTER JOIN Text_Attributes a34 WITH(NOLOCK) on (a34.Document_Id = d.Id and a34.Def_Id = '3a91485e-423f-4a57-81f2-6c34f39c55cb' and a34.Expired = '99991231')
                LEFT OUTER JOIN Text_Attributes a35 WITH(NOLOCK) on (a35.Document_Id = d.Id and a35.Def_Id = 'e04987ab-3d5f-45c1-a79f-95d53837ca56' and a35.Expired = '99991231')
                LEFT OUTER JOIN Currency_Attributes a36 WITH(NOLOCK) on (a36.Document_Id = d.Id and a36.Def_Id = 'bcdd8070-3fd2-4a69-a98a-9f81d6998b3e' and a36.Expired = '99991231')
                LEFT OUTER JOIN Text_Attributes a37 WITH(NOLOCK) on (a37.Document_Id = d.Id and a37.Def_Id = 'a2576c1c-941c-4006-b5b9-a86e54b14ec2' and a37.Expired = '99991231')
                LEFT OUTER JOIN Text_Attributes a38 WITH(NOLOCK) on (a38.Document_Id = d.Id and a38.Def_Id = '418f78d5-db8e-4180-908e-b91ecf5b248f' and a38.Expired = '99991231')
                LEFT OUTER JOIN Enum_Attributes a39 WITH(NOLOCK) on (a39.Document_Id = d.Id and a39.Def_Id = '49adc0e4-e6da-43f3-9b5e-d4a405942bee' and a39.Expired = '99991231')
                LEFT OUTER JOIN Float_Attributes a40 WITH(NOLOCK) on (a40.Document_Id = d.Id and a40.Def_Id = '222f0345-258a-4b72-8682-e557439c8da0' and a40.Expired = '99991231')
                LEFT OUTER JOIN Enum_Attributes a41 WITH(NOLOCK) on (a41.Document_Id = d.Id and a41.Def_Id = '186fa6e2-97fe-45bf-9654-ef452e0bcf01' and a41.Expired = '99991231')
                LEFT OUTER JOIN Enum_Attributes a42 WITH(NOLOCK) on (a42.Document_Id = d.Id and a42.Def_Id = 'aed63ce1-9d43-423b-9b10-f34419a2936a' and a42.Expired = '99991231')
                LEFT OUTER JOIN Enum_Attributes a43 WITH(NOLOCK) on (a43.Document_Id = d.Id and a43.Def_Id = 'f95ddd2d-1937-4204-8ff3-f7367b109011' and a43.Expired = '99991231')
                LEFT OUTER JOIN Text_Attributes a44 WITH(NOLOCK) on (a44.Document_Id = d.Id and a44.Def_Id = '0acd7ddd-08bd-443c-8063-fba8e73568b3' and a44.Expired = '99991231')
                LEFT OUTER JOIN Text_Attributes a45 WITH(NOLOCK) on (a45.Document_Id = d.Id and a45.Def_Id = '64e27319-980e-4cd7-add7-208b3e6d7a45' and a45.Expired = '99991231')
                LEFT OUTER JOIN Date_Time_Attributes a46 WITH(NOLOCK) on (a46.Document_Id = d.Id and a46.Def_Id = 'b71cbf81-c3d1-45c8-9196-89884a467f32' and a46.Expired = '99991231')
                LEFT OUTER JOIN Date_Time_Attributes a47 WITH(NOLOCK) on (a47.Document_Id = d.Id and a47.Def_Id = '2846137f-fc74-4527-9eaa-ce5d775cd63c' and a47.Expired = '99991231')
                LEFT OUTER JOIN Text_Attributes a48 WITH(NOLOCK) on (a48.Document_Id = d.Id and a48.Def_Id = '094e4423-f650-42bd-97d0-d874c9d1cb7b' and a48.Expired = '99991231')
                LEFT OUTER JOIN Image_Attributes a49 WITH(NOLOCK) on (a49.Document_Id = d.Id and a49.Def_Id = '8b089770-2a00-4b3c-afc7-0874ada1c97f' and a49.Expired = '99991231')
                LEFT OUTER JOIN Image_Attributes a50 WITH(NOLOCK) on (a50.Document_Id = d.Id and a50.Def_Id = '5994f2f8-e8e7-41d0-9d29-276a1c2b5740' and a50.Expired = '99991231')
                LEFT OUTER JOIN Image_Attributes a51 WITH(NOLOCK) on (a51.Document_Id = d.Id and a51.Def_Id = 'fdb7f673-b4f4-411d-a4aa-2f98ca4727de' and a51.Expired = '99991231')
                LEFT OUTER JOIN Image_Attributes a52 WITH(NOLOCK) on (a52.Document_Id = d.Id and a52.Def_Id = '39e2a282-de70-4908-87cc-373451f64493' and a52.Expired = '99991231')
                LEFT OUTER JOIN Image_Attributes a53 WITH(NOLOCK) on (a53.Document_Id = d.Id and a53.Def_Id = '7b72c890-e411-45d4-934c-41b81619c9b3' and a53.Expired = '99991231')
                LEFT OUTER JOIN Image_Attributes a54 WITH(NOLOCK) on (a54.Document_Id = d.Id and a54.Def_Id = '4ea19316-ada2-4c5a-a624-514519f55a17' and a54.Expired = '99991231')
                LEFT OUTER JOIN Image_Attributes a55 WITH(NOLOCK) on (a55.Document_Id = d.Id and a55.Def_Id = 'ddc680d8-0773-43ab-b9dd-6d3045a708e5' and a55.Expired = '99991231')
                LEFT OUTER JOIN Image_Attributes a56 WITH(NOLOCK) on (a56.Document_Id = d.Id and a56.Def_Id = 'aad452fe-5825-453e-a7e8-707d27fcc385' and a56.Expired = '99991231')
                LEFT OUTER JOIN Image_Attributes a57 WITH(NOLOCK) on (a57.Document_Id = d.Id and a57.Def_Id = '0ece3ff8-7c74-4731-bfca-7612df6d0636' and a57.Expired = '99991231')
                LEFT OUTER JOIN Image_Attributes a58 WITH(NOLOCK) on (a58.Document_Id = d.Id and a58.Def_Id = 'c53c8967-792b-4e96-95e7-7ad4dcc67bf1' and a58.Expired = '99991231')
                LEFT OUTER JOIN Image_Attributes a59 WITH(NOLOCK) on (a59.Document_Id = d.Id and a59.Def_Id = 'f61e6976-da01-4717-b82f-7b338e47aaed' and a59.Expired = '99991231')
                LEFT OUTER JOIN Image_Attributes a60 WITH(NOLOCK) on (a60.Document_Id = d.Id and a60.Def_Id = 'f9e48636-3119-43b5-8ca0-8227e73cbe62' and a60.Expired = '99991231')
                LEFT OUTER JOIN Image_Attributes a61 WITH(NOLOCK) on (a61.Document_Id = d.Id and a61.Def_Id = 'f4a25901-ebbb-445f-88b9-88bdd6f87bf9' and a61.Expired = '99991231')
                LEFT OUTER JOIN Image_Attributes a62 WITH(NOLOCK) on (a62.Document_Id = d.Id and a62.Def_Id = 'cb1a6829-97ca-401c-ac8b-b5da5c8b2319' and a62.Expired = '99991231')
                LEFT OUTER JOIN Image_Attributes a63 WITH(NOLOCK) on (a63.Document_Id = d.Id and a63.Def_Id = 'c224cc85-5427-4daa-9664-ccad883ff3d7' and a63.Expired = '99991231')
                LEFT OUTER JOIN Image_Attributes a64 WITH(NOLOCK) on (a64.Document_Id = d.Id and a64.Def_Id = 'ad2a340d-fd72-40a2-aa97-d48e2ca0156a' and a64.Expired = '99991231')
                LEFT OUTER JOIN Image_Attributes a65 WITH(NOLOCK) on (a65.Document_Id = d.Id and a65.Def_Id = '84b6cf13-b94b-4146-aa75-e3cf9d89629a' and a65.Expired = '99991231')
                LEFT OUTER JOIN Boolean_Attributes a66 WITH(NOLOCK) on (a66.Document_Id = d.Id and a66.Def_Id = '172e6cae-8014-479c-b6a5-1b4b11da1abc' and a66.Expired = '99991231')
                LEFT OUTER JOIN Boolean_Attributes a67 WITH(NOLOCK) on (a67.Document_Id = d.Id and a67.Def_Id = '0105969c-9f09-437f-b1c7-36858a8def31' and a67.Expired = '99991231')
                LEFT OUTER JOIN Boolean_Attributes a68 WITH(NOLOCK) on (a68.Document_Id = d.Id and a68.Def_Id = '8e367e32-4082-435e-8001-484710e9ca82' and a68.Expired = '99991231')
                LEFT OUTER JOIN Boolean_Attributes a69 WITH(NOLOCK) on (a69.Document_Id = d.Id and a69.Def_Id = '86674fa0-07fd-4916-ba83-6c1ae08a729f' and a69.Expired = '99991231')
                LEFT OUTER JOIN Boolean_Attributes a70 WITH(NOLOCK) on (a70.Document_Id = d.Id and a70.Def_Id = '7ba7052f-99f3-4296-8de2-707090e5090e' and a70.Expired = '99991231')
                LEFT OUTER JOIN Boolean_Attributes a71 WITH(NOLOCK) on (a71.Document_Id = d.Id and a71.Def_Id = '89a13e27-16cc-4799-9e24-a702a620767a' and a71.Expired = '99991231')
                LEFT OUTER JOIN Boolean_Attributes a72 WITH(NOLOCK) on (a72.Document_Id = d.Id and a72.Def_Id = '57e28be6-6e2b-43e4-9933-e500bd8cd045' and a72.Expired = '99991231')
                LEFT OUTER JOIN Boolean_Attributes a73 WITH(NOLOCK) on (a73.Document_Id = d.Id and a73.Def_Id = 'f1a3bfee-e621-4365-939b-1277e0cccd1c' and a73.Expired = '99991231')
                LEFT OUTER JOIN Boolean_Attributes a74 WITH(NOLOCK) on (a74.Document_Id = d.Id and a74.Def_Id = '516c4382-ef04-41fc-a09b-14572b5b844f' and a74.Expired = '99991231')
                LEFT OUTER JOIN Boolean_Attributes a75 WITH(NOLOCK) on (a75.Document_Id = d.Id and a75.Def_Id = '260ac62d-5ba1-4d3f-833d-342aeae76f37' and a75.Expired = '99991231')
                LEFT OUTER JOIN Boolean_Attributes a76 WITH(NOLOCK) on (a76.Document_Id = d.Id and a76.Def_Id = 'dbe9cf02-cd52-480c-b503-3459838a9328' and a76.Expired = '99991231')
                LEFT OUTER JOIN Text_Attributes a77 WITH(NOLOCK) on (a77.Document_Id = d.Id and a77.Def_Id = 'f44a599f-c14e-4c5d-a3ae-3710efcc7756' and a77.Expired = '99991231')
                LEFT OUTER JOIN Boolean_Attributes a78 WITH(NOLOCK) on (a78.Document_Id = d.Id and a78.Def_Id = '777e5e50-edfc-479a-ac25-5dc011d08ecd' and a78.Expired = '99991231')
                LEFT OUTER JOIN Boolean_Attributes a79 WITH(NOLOCK) on (a79.Document_Id = d.Id and a79.Def_Id = 'ddbb5a71-412d-45be-a4d8-8b36856ab134' and a79.Expired = '99991231')
                LEFT OUTER JOIN Boolean_Attributes a80 WITH(NOLOCK) on (a80.Document_Id = d.Id and a80.Def_Id = '3178af85-238b-44fb-ab55-943d68b992fb' and a80.Expired = '99991231')
                LEFT OUTER JOIN Enum_Attributes a81 WITH(NOLOCK) on (a81.Document_Id = d.Id and a81.Def_Id = 'ed9b2161-bada-4065-b92a-ab4654e0535f' and a81.Expired = '99991231')
                LEFT OUTER JOIN Boolean_Attributes a82 WITH(NOLOCK) on (a82.Document_Id = d.Id and a82.Def_Id = 'e676ed3a-e36f-4e46-8256-f9ea28df21b9' and a82.Expired = '99991231')
                LEFT OUTER JOIN Boolean_Attributes a83 WITH(NOLOCK) on (a83.Document_Id = d.Id and a83.Def_Id = '88e27450-198b-40e2-924a-3fe52b773592' and a83.Expired = '99991231')
                LEFT OUTER JOIN Boolean_Attributes a84 WITH(NOLOCK) on (a84.Document_Id = d.Id and a84.Def_Id = 'a5203354-de0f-4a6d-8127-450eb379bbfc' and a84.Expired = '99991231')
                LEFT OUTER JOIN Enum_Attributes a85 WITH(NOLOCK) on (a85.Document_Id = d.Id and a85.Def_Id = 'a7a32c0b-d9df-4055-8f24-4699279799d2' and a85.Expired = '99991231')
                LEFT OUTER JOIN Boolean_Attributes a86 WITH(NOLOCK) on (a86.Document_Id = d.Id and a86.Def_Id = 'f47471cc-ce7b-4109-92fe-4e787a876bec' and a86.Expired = '99991231')
                LEFT OUTER JOIN Boolean_Attributes a87 WITH(NOLOCK) on (a87.Document_Id = d.Id and a87.Def_Id = '44c7a03b-a0b7-44af-ae39-60cd5cdff379' and a87.Expired = '99991231')
                LEFT OUTER JOIN Enum_Attributes a88 WITH(NOLOCK) on (a88.Document_Id = d.Id and a88.Def_Id = '0472a6af-867b-424a-9d92-73d5b916cd7d' and a88.Expired = '99991231')
                LEFT OUTER JOIN Boolean_Attributes a89 WITH(NOLOCK) on (a89.Document_Id = d.Id and a89.Def_Id = 'b1946a60-d130-461e-917b-8319d60ac529' and a89.Expired = '99991231')
                LEFT OUTER JOIN Boolean_Attributes a90 WITH(NOLOCK) on (a90.Document_Id = d.Id and a90.Def_Id = '7f50caf2-b7ae-450f-8fb2-b41193b4be1c' and a90.Expired = '99991231')
                LEFT OUTER JOIN Enum_Attributes a91 WITH(NOLOCK) on (a91.Document_Id = d.Id and a91.Def_Id = 'e3b75345-5328-414c-b046-e0c06e9ae39c' and a91.Expired = '99991231')
                LEFT OUTER JOIN Date_Time_Attributes a92 WITH(NOLOCK) on (a92.Document_Id = d.Id and a92.Def_Id = 'acceface-7f72-421c-9cff-de3745838125' and a92.Expired = '99991231')
                LEFT OUTER JOIN Date_Time_Attributes a93 WITH(NOLOCK) on (a93.Document_Id = d.Id and a93.Def_Id = 'e031b0cf-8b7a-4461-89b6-fae4b2b28aac' and a93.Expired = '99991231')
                LEFT OUTER JOIN Date_Time_Attributes a94 WITH(NOLOCK) on (a94.Document_Id = d.Id and a94.Def_Id = 'd88a2388-c52b-49d5-897b-4c660e77f0b1' and a94.Expired = '99991231')
                LEFT OUTER JOIN Date_Time_Attributes a95 WITH(NOLOCK) on (a95.Document_Id = d.Id and a95.Def_Id = '154d4835-2b17-48a9-9d4e-620bcd7918ec' and a95.Expired = '99991231')
                LEFT OUTER JOIN Date_Time_Attributes a96 WITH(NOLOCK) on (a96.Document_Id = d.Id and a96.Def_Id = '0e7987b7-fa52-4cb0-86f1-e85332067f6c' and a96.Expired = '99991231')
                LEFT OUTER JOIN Boolean_Attributes a97 WITH(NOLOCK) on (a97.Document_Id = d.Id and a97.Def_Id = '9e78646b-ec74-4d21-9104-11c63eeb5649' and a97.Expired = '99991231')
                LEFT OUTER JOIN Boolean_Attributes a98 WITH(NOLOCK) on (a98.Document_Id = d.Id and a98.Def_Id = '83013bec-2942-49d7-b039-52e50f159c9b' and a98.Expired = '99991231')
                LEFT OUTER JOIN Boolean_Attributes a99 WITH(NOLOCK) on (a99.Document_Id = d.Id and a99.Def_Id = '94a12741-a72c-496e-a2ca-542a8e3856d0' and a99.Expired = '99991231')
                LEFT OUTER JOIN Boolean_Attributes a100 WITH(NOLOCK) on (a100.Document_Id = d.Id and a100.Def_Id = '0d638f44-e085-450b-8298-5662bdd6bcee' and a100.Expired = '99991231')
                LEFT OUTER JOIN Boolean_Attributes a101 WITH(NOLOCK) on (a101.Document_Id = d.Id and a101.Def_Id = 'edf6b5b5-1eac-49b4-b431-a78a8b31504f' and a101.Expired = '99991231')
                LEFT OUTER JOIN Boolean_Attributes a102 WITH(NOLOCK) on (a102.Document_Id = d.Id and a102.Def_Id = '0de92221-a173-4cd9-9298-b6601d73bf92' and a102.Expired = '99991231')
                LEFT OUTER JOIN Boolean_Attributes a103 WITH(NOLOCK) on (a103.Document_Id = d.Id and a103.Def_Id = '0dfd1eb1-8e28-4b78-97bd-dd0c7e3a64e5' and a103.Expired = '99991231')
                LEFT OUTER JOIN Image_Attributes a104 WITH(NOLOCK) on (a104.Document_Id = d.Id and a104.Def_Id = '3f6e0c60-048f-4100-b9f4-16c0c51aa355' and a104.Expired = '99991231')
                LEFT OUTER JOIN Enum_Attributes a105 WITH(NOLOCK) on (a105.Document_Id = d.Id and a105.Def_Id = '95ab5e64-1a12-43bc-98de-381bf4c1af6e' and a105.Expired = '99991231')
                LEFT OUTER JOIN Enum_Attributes a106 WITH(NOLOCK) on (a106.Document_Id = d.Id and a106.Def_Id = '701a690b-6ba1-41b9-b936-6313941ed5aa' and a106.Expired = '99991231')
                LEFT OUTER JOIN Image_Attributes a107 WITH(NOLOCK) on (a107.Document_Id = d.Id and a107.Def_Id = '30489013-c47b-4670-be13-90cc07ab36f8' and a107.Expired = '99991231')
                LEFT OUTER JOIN Image_Attributes a108 WITH(NOLOCK) on (a108.Document_Id = d.Id and a108.Def_Id = 'ed503db9-b8d5-4022-9360-93a53ebeb737' and a108.Expired = '99991231')
                LEFT OUTER JOIN Enum_Attributes a109 WITH(NOLOCK) on (a109.Document_Id = d.Id and a109.Def_Id = '984920c2-7df6-4a87-b874-cc068a8cefff' and a109.Expired = '99991231')
                LEFT OUTER JOIN Enum_Attributes a110 WITH(NOLOCK) on (a110.Document_Id = d.Id and a110.Def_Id = 'de212a8f-d752-42cf-95c3-f649cdd9858d' and a110.Expired = '99991231')
                LEFT OUTER JOIN Document_Attributes a111 WITH(NOLOCK) on (a111.Document_Id = d.Id and a111.Def_Id = '17559727-0cc6-49fb-9a22-a40e5e4834e4' and a111.Expired = '99991231')
                LEFT OUTER JOIN Document_Attributes a112 WITH(NOLOCK) on (a112.Document_Id = d.Id and a112.Def_Id = '033895cd-e283-4533-a3fd-b209b331d9c9' and a112.Expired = '99991231')
                LEFT OUTER JOIN Document_Attributes a113 WITH(NOLOCK) on (a113.Document_Id = d.Id and a113.Def_Id = '235bb803-3eb8-4cd2-8c28-d7c54fa6b100' and a113.Expired = '99991231')
                LEFT OUTER JOIN Text_Attributes a114 WITH(NOLOCK) on (a114.Document_Id = d.Id and a114.Def_Id = '7aa79880-5590-49af-874e-1e8831460523' and a114.Expired = '99991231')
                LEFT OUTER JOIN Text_Attributes a115 WITH(NOLOCK) on (a115.Document_Id = d.Id and a115.Def_Id = '4b0faebc-10bf-4bc8-97c2-26b60924d32e' and a115.Expired = '99991231')
                LEFT OUTER JOIN Text_Attributes a116 WITH(NOLOCK) on (a116.Document_Id = d.Id and a116.Def_Id = 'dd0dcb1f-0742-46fc-8934-3c4306c85998' and a116.Expired = '99991231')
                LEFT OUTER JOIN Text_Attributes a117 WITH(NOLOCK) on (a117.Document_Id = d.Id and a117.Def_Id = 'd72d3300-31f6-453f-bce0-71f60795d243' and a117.Expired = '99991231')
                LEFT OUTER JOIN Text_Attributes a118 WITH(NOLOCK) on (a118.Document_Id = d.Id and a118.Def_Id = 'bfae8bce-8372-41a1-ba1f-73490a0b88fc' and a118.Expired = '99991231')
                LEFT OUTER JOIN Text_Attributes a119 WITH(NOLOCK) on (a119.Document_Id = d.Id and a119.Def_Id = '8bbb654a-e379-4c30-a4d4-9ade05a33bd2' and a119.Expired = '99991231')
                LEFT OUTER JOIN Text_Attributes a120 WITH(NOLOCK) on (a120.Document_Id = d.Id and a120.Def_Id = 'b030595e-73de-4f69-aa36-f91bac1f7eb4' and a120.Expired = '99991231')
                LEFT OUTER JOIN Boolean_Attributes a121 WITH(NOLOCK) on (a121.Document_Id = d.Id and a121.Def_Id = 'd4289e90-c108-4cc5-a029-3192be65a37b' and a121.Expired = '99991231')
                LEFT OUTER JOIN Boolean_Attributes a122 WITH(NOLOCK) on (a122.Document_Id = d.Id and a122.Def_Id = '7ed1274d-d1d4-44de-872c-5a6aacb80354' and a122.Expired = '99991231')
                LEFT OUTER JOIN Boolean_Attributes a123 WITH(NOLOCK) on (a123.Document_Id = d.Id and a123.Def_Id = '8ab1df4d-eb82-48af-82e8-a3ed3cf9d052' and a123.Expired = '99991231')
                LEFT OUTER JOIN Boolean_Attributes a124 WITH(NOLOCK) on (a124.Document_Id = d.Id and a124.Def_Id = '8e2cc451-bc20-4ead-b015-ae5d2c6be810' and a124.Expired = '99991231')
                LEFT OUTER JOIN Boolean_Attributes a125 WITH(NOLOCK) on (a125.Document_Id = d.Id and a125.Def_Id = 'c2561cc6-69bc-4d03-98e6-b6514302d925' and a125.Expired = '99991231')
                LEFT OUTER JOIN Boolean_Attributes a126 WITH(NOLOCK) on (a126.Document_Id = d.Id and a126.Def_Id = 'da41c39a-e8c5-4740-8dbf-bb23efc8b323' and a126.Expired = '99991231')
                LEFT OUTER JOIN Boolean_Attributes a127 WITH(NOLOCK) on (a127.Document_Id = d.Id and a127.Def_Id = '328d8ceb-9ec8-4193-9caa-c6234014a472' and a127.Expired = '99991231')
                LEFT OUTER JOIN Boolean_Attributes a128 WITH(NOLOCK) on (a128.Document_Id = d.Id and a128.Def_Id = '90b270e1-67c0-42f1-81f6-f6a4ce3080ed' and a128.Expired = '99991231')
                LEFT OUTER JOIN Boolean_Attributes a129 WITH(NOLOCK) on (a129.Document_Id = d.Id and a129.Def_Id = 'dd125d0c-2f6b-4ce2-a649-ff52c91178aa' and a129.Expired = '99991231')
                INNER JOIN [Document_States] ds130 WITH(NOLOCK) on (ds130.Document_Id = d.Id and ds130.Expired = '99991231' and ds130.State_Type_Id IN ({0}))
				inner join [msec-meta].dbo.Object_Defs o with(nolock) on o.Id = ds130.State_Type_Id
        WHERE
                d.Def_Id = 'b4dddc00-9ea9-4ad4-9c4f-498e87aa9828' AND
                ([d].[Deleted] is null OR [d].[Deleted] = 0) 
        ) as [*AdultsMedicalCart]
        INNER JOIN (SELECT
                d.Id,
                [a1].[Value] as [PassportSeries],
                [a2].[Value] as [PassportType],
                [a3].[Value] as [FamilyState],
                [a4].[Value] as [Date_of_Birth],
                [a5].[Value] as [Sex],
                [a6].[Value] as [PassportNo],
                [a7].[Value] as [SIN],
                [a8].[Value] as [Middle_Name],
                [a9].[Value] as [IIN],
                [a10].[Value] as [First_Name],
                [a11].[Value] as [Last_Name],
                [a12].[Value] as [NationalID],
                [a13].[Value] as [Issuing_Authority],
                [a14].[Value] as [Date_of_Issue],
                [a15].[Value] as [INN]
        FROM
                Documents d WITH(NOLOCK)
                LEFT OUTER JOIN Text_Attributes a1 WITH(NOLOCK) on (a1.Document_Id = d.Id and a1.Def_Id = 'c4a72cbd-8153-4b27-8346-0624609857ba' and a1.Expired = '99991231')
                LEFT OUTER JOIN Enum_Attributes a2 WITH(NOLOCK) on (a2.Document_Id = d.Id and a2.Def_Id = 'c700bece-a8fe-4bf2-b95f-096fa4d4e1c0' and a2.Expired = '99991231')
                LEFT OUTER JOIN Enum_Attributes a3 WITH(NOLOCK) on (a3.Document_Id = d.Id and a3.Def_Id = '2d20e752-fb4e-4ed8-9674-46ec9101b9ec' and a3.Expired = '99991231')
                LEFT OUTER JOIN Date_Time_Attributes a4 WITH(NOLOCK) on (a4.Document_Id = d.Id and a4.Def_Id = '706aad5a-6b08-4b15-8e2f-49b175c999e8' and a4.Expired = '99991231')
                LEFT OUTER JOIN Enum_Attributes a5 WITH(NOLOCK) on (a5.Document_Id = d.Id and a5.Def_Id = '4398c35a-606e-494e-9f02-69539c9fe61c' and a5.Expired = '99991231')
                LEFT OUTER JOIN Text_Attributes a6 WITH(NOLOCK) on (a6.Document_Id = d.Id and a6.Def_Id = 'bf478bbf-3dd0-4c10-a9e9-7227cf9e1a29' and a6.Expired = '99991231')
                LEFT OUTER JOIN Text_Attributes a7 WITH(NOLOCK) on (a7.Document_Id = d.Id and a7.Def_Id = 'ea3a496e-3a03-4f26-8058-823082619903' and a7.Expired = '99991231')
                LEFT OUTER JOIN Text_Attributes a8 WITH(NOLOCK) on (a8.Document_Id = d.Id and a8.Def_Id = 'ba0713d4-d7f6-4a90-8e3e-8670c8cdf305' and a8.Expired = '99991231')
                LEFT OUTER JOIN Text_Attributes a9 WITH(NOLOCK) on (a9.Document_Id = d.Id and a9.Def_Id = '26dd8a20-144d-4931-8dd6-8e8d3271ddee' and a9.Expired = '99991231')
                LEFT OUTER JOIN Text_Attributes a10 WITH(NOLOCK) on (a10.Document_Id = d.Id and a10.Def_Id = '9856ca78-074d-422f-912c-92d96ee6e8bc' and a10.Expired = '99991231')
                LEFT OUTER JOIN Text_Attributes a11 WITH(NOLOCK) on (a11.Document_Id = d.Id and a11.Def_Id = '4eaca45a-a143-4ca9-8408-cb2c89d9e63f' and a11.Expired = '99991231')
                LEFT OUTER JOIN Text_Attributes a12 WITH(NOLOCK) on (a12.Document_Id = d.Id and a12.Def_Id = '400ee5d8-8f97-4d2c-a029-e1db1c8b9f83' and a12.Expired = '99991231')
                LEFT OUTER JOIN Text_Attributes a13 WITH(NOLOCK) on (a13.Document_Id = d.Id and a13.Def_Id = '8cd12bd3-1e72-4983-b4e2-eb89af985406' and a13.Expired = '99991231')
                LEFT OUTER JOIN Date_Time_Attributes a14 WITH(NOLOCK) on (a14.Document_Id = d.Id and a14.Def_Id = '86282a25-ed98-4ea6-8650-ee4cc577e211' and a14.Expired = '99991231')
                LEFT OUTER JOIN Text_Attributes a15 WITH(NOLOCK) on (a15.Document_Id = d.Id and a15.Def_Id = 'abcda93b-c195-45d6-9555-f4dfd10ce832' and a15.Expired = '99991231')
        WHERE
                d.Def_Id = '6052978a-1ecb-4f96-a16b-93548936afc0'
        ) as [*Person] on [*Person].Id = [*AdultsMedicalCart].[Person]

		left outer join [msec-meta].dbo.Object_Defs PassportTypeText on PassportTypeText.Id = [*Person].[PassportType]
		left outer join [msec-meta].dbo.Object_Defs FamilyStateText on FamilyStateText.Id = [*Person].[FamilyState]
		left outer join [msec-meta].dbo.Object_Defs SexText on SexText.Id = [*Person].[Sex]
		left outer join [msec-meta].dbo.Object_Defs DisabilityGroupText on DisabilityGroupText.Id = [*AdultsMedicalCart].[DisabilityGroup]
		left outer join [msec-meta].dbo.Object_Defs MseNameText on MseNameText.Id = [*AdultsMedicalCart].[MseName]
		left outer join [msec-meta].dbo.Object_Defs OccupationText on OccupationText.Id = [*AdultsMedicalCart].[Occupation]
		left outer join [msec-meta].dbo.Object_Defs EducationActText on EducationActText.Id = [*AdultsMedicalCart].[EducationAct]
		left outer join [msec-meta].dbo.Object_Defs ExaminationPrReText on ExaminationPrReText.Id = [*AdultsMedicalCart].[ExaminationPrRe]
		left outer join [msec-meta].dbo.Object_Defs ExaminationPlaceText on ExaminationPlaceText.Id = [*AdultsMedicalCart].[ExaminationPlace]
		left outer join [msec-meta].dbo.Object_Defs PensionStatusText on PensionStatusText.Id = [*AdultsMedicalCart].[PensionStatus]
		left outer join [msec-meta].dbo.Object_Defs DisabilityReasonText on DisabilityReasonText.Id = [*AdultsMedicalCart].[DisabilityReason]
		left outer join [msec-meta].dbo.Object_Defs WorkingConditionsText on WorkingConditionsText.Id = [*AdultsMedicalCart].[WorkingConditions]
		left outer join [msec-meta].dbo.Object_Defs CompainDagnosisMKB3Text on CompainDagnosisMKB3Text.Id = [*AdultsMedicalCart].[CompainDagnosisMKB3]
		left outer join [msec-meta].dbo.Object_Defs CompainDagnosisMKB2Text on CompainDagnosisMKB2Text.Id = [*AdultsMedicalCart].[CompainDagnosisMKB2]
		left outer join [msec-meta].dbo.Object_Defs CompainDagnosisMKB1Text on CompainDagnosisMKB1Text.Id = [*AdultsMedicalCart].[CompainDagnosisMKB1]
		left outer join [msec-meta].dbo.Object_Defs DiagnosisText on DiagnosisText.Id = [*AdultsMedicalCart].[Diagnosis]
		left outer join [msec-meta].dbo.Object_Defs ConfirmationExistenceText on ConfirmationExistenceText.Id = [*AdultsMedicalCart].[ConfirmationExistence]
		left outer join [msec-meta].dbo.Persons status_users on status_users.Id = [*AdultsMedicalCart].[StatusAuthorUserId]
";
        const string T_SQL_GET_DATA_MSEC_CHILD = @"
        [*Person].[Last_Name] 'Фамилия',
        [*Person].[First_Name] 'Имя',
        [*Person].[Middle_Name] 'Отчество',
		[*Person].[Date_of_Birth] 'Д.р.',
        [SexText].Full_Name as 'Пол',
		[*ChildMedicalCart].[Date] as 'Дата регистрации',
        [*Person].[SIN] 'СИН',
        [*Person].[IIN] 'ПИН',
        [*Person].[PassportNo] '№ паспорта',
        [*Person].[Issuing_Authority] 'Выдавший орган',
        [*Person].[Date_of_Issue] 'Дата выдачи',
        [*Person].[INN] 'ИНН',
		[*ChildMedicalCart].status_child as 'Статус',
        [*Person].[NationalID] 'Национальный ID',
        --[ConfirmationExistenceText].Full_Name as 'Подтверждение о существования инвалида',
        [MseNameText].Full_Name as 'МСЭК',
        [OccupationText].Full_Name as 'Род деятельности',
        [EducationActText].Full_Name as 'Образование',
        [ExaminationPrReText].Full_Name as 'Вид освидетельствования',
        [PensionStatusText].Full_Name as 'Получает пенсию',
        [DisabilityReasonText].Full_Name as 'Причина инвалидности',
        [CompainDagnosisMKB3Text].Full_Name as 'Соп. диагноз по МКБ 3',
        [CompainDagnosisMKB2Text].Full_Name as 'Соп. диагноз по МКБ 2',
        [CompainDagnosisMKB1Text].Full_Name as 'Соп. диагноз по МКБ 1',
        [*ChildMedicalCart].[MainDiagnosisValue] as 'Основной диагноз-byte',--dbo.DecodeUTF8String([*ChildMedicalCart].[MainDiagnosisValue]) as 'Основной диагноз',
        [DiagnosisText].Full_Name as 'Диагноз',
        --[*ChildMedicalCart].[Died] as 'Умер (месяц/год)',
        [*ChildMedicalCart].[MseName] as 'Наименование МСЭ',
        [*ChildMedicalCart].[ReceivesPension] as 'Получает пенсию',
        [*ChildMedicalCart].[DisabilityCategory] as 'Категория инвалидности',
        [*ChildMedicalCart].[DisabilityGroup] as 'Группа инвалидности',
        [*ChildMedicalCart].[Diseases] as 'Болезни дополнительно',
        [*ChildMedicalCart].[DisabilityDate] as 'Дата установления инвалидности',
        [*ChildMedicalCart].[ReasonNotPension] as 'Причина если не получает пенсию',
        [*ChildMedicalCart].[DisabledEmployment] as 'Занятость инвалида',
        [*ChildMedicalCart].[Date_of_Birth_Parents] as 'Дата рождения (родитель/опекун)',
        [*ChildMedicalCart].[Middle_Name_Parents] as 'Отчество',
        [*ChildMedicalCart].[First_Name_Parents] as 'Имя',
        [*ChildMedicalCart].[Last_Name_Parents] as 'Фамилия',
        [*ChildMedicalCart].[Sex_Parents] as 'Пол (родитель/опекун)',
        [*ChildMedicalCart].[Indefinitely] as 'Бессрочно',
        [*ChildMedicalCart].[Temporarily] as 'Временно:  до',
        [*ChildMedicalCart].[ExamDate] as 'Дата освидетельствования',
        [*ChildMedicalCart].[PlaceOfStudy] as 'Место учебы',
        [*ChildMedicalCart].[MedicalOrgName] as 'Наименование лечебной организации направившей на МСЭ',
        [*ChildMedicalCart].[Person] as 'Заявитель',
        [*ChildMedicalCart].[Weight] as 'Вес',
        [*ChildMedicalCart].[PensionStatus] as 'Получает пенсию',
        [*ChildMedicalCart].[Height] as 'Рост',
        [*ChildMedicalCart].[ReferenceNumber] as '№/Серия справки',
        [*ChildMedicalCart].[EducationAct] as 'Образование',
        [*ChildMedicalCart].[Examination4] as 'Место освидетельствования',
        [*ChildMedicalCart].[MedicalOrgAddress] as 'Адрес направившей лечебной организации',
        [*ChildMedicalCart].[PensionAmount] as 'Размер пенсии',
        [*ChildMedicalCart].[Occupation] as 'Род деятельности',
        [*ChildMedicalCart].[RegNoAct] as '№ мед. акта',
        [*ChildMedicalCart].[ExaminationPrRe] as 'Вид освидетельствования',
        [*ChildMedicalCart].[ResidentialAddress] as 'Домашний адрес (ул/дом/кв)',
        [*ChildMedicalCart].[Phone] as '№ телефона',
        [*ChildMedicalCart].[ExamOfDate] as 'Дата проведения экспертизы',
        [*ChildMedicalCart].[ExamStartDate] as 'Дата начала экспертизы',
        [*ChildMedicalCart].[ExamFinishDate] as 'Дата окончания экспертизы',
        /*dbo.DecodeUTF8String([*ChildMedicalCart].[ExpertExamResult7CommentValue]) as 'Комментарий',
        dbo.DecodeUTF8String([*ChildMedicalCart].[ExpertExamResult2Value]) as 'Данные лабораторных и рентгенологических исследований',
        dbo.DecodeUTF8String([*ChildMedicalCart].[ExpertExamResult1Value]) as 'Клинико-трудовой анамнез',
        dbo.DecodeUTF8String([*ChildMedicalCart].[ExpertExamResult6Value]) as 'Данные врача-офтальмолог',
        dbo.DecodeUTF8String([*ChildMedicalCart].[ExpertExamResult9CommentValue]) as 'Комментарий',
        dbo.DecodeUTF8String([*ChildMedicalCart].[ExpertExamResult7Value]) as 'Данные эксперта невропатолога и психиатра',
        dbo.DecodeUTF8String([*ChildMedicalCart].[ExpertExamResult8Value]) as 'Данные врачей других специальностей',
        dbo.DecodeUTF8String([*ChildMedicalCart].[ExpertExamResult3Value]) as 'Результаты доп. спец-х исследований',
        dbo.DecodeUTF8String([*ChildMedicalCart].[ExpertExamResult5CommentValue]) as 'Комментарий',
        dbo.DecodeUTF8String([*ChildMedicalCart].[ExpertExamResultValue]) as 'Жалобы',
        dbo.DecodeUTF8String([*ChildMedicalCart].[ExpertExamResult10Value]) as 'Данные врача-кардиолога',
        dbo.DecodeUTF8String([*ChildMedicalCart].[ExpertExamResult4CommentValue]) as 'Комментарий',
        dbo.DecodeUTF8String([*ChildMedicalCart].[ExpertExamResult10CommentValue]) as 'Комментарий',
        dbo.DecodeUTF8String([*ChildMedicalCart].[ExpertExamResult6CommentValue]) as 'Комментарий',
        dbo.DecodeUTF8String([*ChildMedicalCart].[ExpertExamResult5Value]) as 'Данные врача-хирурга',
        dbo.DecodeUTF8String([*ChildMedicalCart].[ExpertExamResult4Value]) as 'Данные врача-терапевта',
        dbo.DecodeUTF8String([*ChildMedicalCart].[ExpertExamResult11Value]) as 'Данные эксперта педиатра',
        dbo.DecodeUTF8String([*ChildMedicalCart].[ExpertExamResult9Value]) as 'Данные врача-окулиста',
        dbo.DecodeUTF8String([*ChildMedicalCart].[ExpertExamResult11CommentValue]) as 'Комментарий',*/
        [*ChildMedicalCart].[goal3] as 'Определение степени утраты трудоспособности в %',
        [*ChildMedicalCart].[goal7] as 'Нуждается в специализированных средствах помощи (коляски, слуховой аппарат, и т.д.)',
        [*ChildMedicalCart].[goal6] as 'По ухудшению',
        [*ChildMedicalCart].[goal4] as 'Дом интернат',
        [*ChildMedicalCart].[goal1] as 'Для установления группы инвалидности',
        [*ChildMedicalCart].[goalAnother] as 'Другое',
        [*ChildMedicalCart].[goal5] as 'Заключение на: кресло-коляски',
        [*ChildMedicalCart].[goal9] as 'Оформление/продление листков нетрудоспособности, и т.д.)',
        [*ChildMedicalCart].[goal8] as 'Нуждается в протезно-ортопедических средствах, помощи специализированных учреждений',
        [*ChildMedicalCart].[goal2] as 'Изменение причины инвалидности',
        --dbo.DecodeUTF8String([*ChildMedicalCart].[CompanionDiagnosisValue]) as 'Сопутствующий диагноз',
        [*ChildMedicalCart].[CompainDagnosisMKB2] as 'Соп. диагноз по МКБ 2',
        [*ChildMedicalCart].[Diagnosis] as 'Диагноз',
        --dbo.DecodeUTF8String([*ChildMedicalCart].[DisabilityReasonTextValue]) as 'Причина инвалидности',
        --dbo.DecodeUTF8String([*ChildMedicalCart].[DiseaseComplicationValue]) as 'Осложнение болезни',
        [*ChildMedicalCart].[CompainDagnosisMKB3] as 'Соп. диагноз по МКБ 3',
        [*ChildMedicalCart].[CompainDagnosisMKB1] as 'Соп. диагноз по МКБ 1',
        [*ChildMedicalCart].[ExamDateTo] as 'Инвалидность установлено по',
        [*ChildMedicalCart].[ExamDateFrom] as 'Инвалидность установлено с',
        [*ChildMedicalCart].[Indefinitely] as 'Бессрочно',
        [*ChildMedicalCart].[NeedsCare] as 'Нуждается в уходе',
        [*ChildMedicalCart].[ReconstructiveSurgery] as 'Реконструктивная  хирургия',
        [*ChildMedicalCart].[NeedsSupervision] as 'Нуждается в надзоре',
        [*ChildMedicalCart].[DisabilityReason] as 'Причина инвалидности',
        [*ChildMedicalCart].[RestorativeTherapy] as 'Восстановительная терапия',
        [*ChildMedicalCart].[HospitalTreatment] as 'Амбулаторное лечение',
        [*ChildMedicalCart].[DisabilityGroupAct] as 'Группа инвалидности',
        [*ChildMedicalCart].[Hospitalization] as 'Стационарное лечение',
        [*ChildMedicalCart].[MiddleNameParents] as 'Отчество',
        [*ChildMedicalCart].[FirstNameParents] as 'Имя',
        [*ChildMedicalCart].[DateOfBirthParents] as 'Дата рождения (родитель/опекун)',
        [*ChildMedicalCart].[GenderParents] as 'Пол (родитель/опекун)',
        [*ChildMedicalCart].[LastNameParents] as 'Фамилия',
        [*ChildMedicalCart].[SickListType] as 'Вид',
        [*ChildMedicalCart].[SickListPeriodFrom] as 'Период с',
        [*ChildMedicalCart].[SickListPeriodTo] as 'Период по',
        [*ChildMedicalCart].[SickListSeries] as 'Серия',
        [*ChildMedicalCart].[Region] as 'Область-ref',
        [*ChildMedicalCart].[District] as 'Район-ref',
        [*ChildMedicalCart].[subDistrict] as 'Джамоат-ref',
        [*ChildMedicalCart].[Village] as 'Населенный пункт-ref',
		[*ChildMedicalCart].[state_created_at] 'Дата изменения статуса',
		status_users.Last_Name + ' ' + status_users.First_Name 'Пользователь изменивший статус'
FROM
        (SELECT 
                d.Id,
				o.Full_Name as status_child,
                [a1].[Value] as [Date],
                [a2].[Value] as [OrgProfile],
                [a3].[Value] as [No],
                [a4].[Value] as [Education],
                [a5].[Value] as [Date_of_Birth],
                [a6].[Value] as [Died],
                [a7].[Value] as [MseName],
                [a8].[Value] as [ReceivesPension],
                [a9].[Value] as [First_Name],
                [a10].[Value] as [DisabilityCategory],
                [a11].[Value] as [DisabilityGroup],
                [a12].[Value] as [Diseases],
                [a13].[Value] as [FamilyState],
                [a14].[Value] as [Sex],
                [a15].[Value] as [ConfirmationExistence],
                [a16].[Value] as [PassportNo],
                [a17].[Value] as [Last_Name],
                [a18].[Value] as [DisabilityDate],
                [a19].[Value] as [PassportType],
                [a20].[Value] as [ReasonNotPension],
                [a21].[Value] as [Middle_Name],
                [a22].[Value] as [DisabledEmployment],
                [a23].[Value] as [Date_of_Birth_Parents],
                [a24].[Value] as [Middle_Name_Parents],
                [a25].[Value] as [First_Name_Parents],
                [a26].[Value] as [Last_Name_Parents],
                [a27].[Value] as [Sex_Parents],
                [a28].[Value] as [Indefinitely],
                [a29].[Value] as [Temporarily],
                [a30].[Value] as [ExamDate],
                [a31].[Value] as [PlaceOfStudy],
                [a32].[Value] as [MedicalOrgName],
                [a33].[Value] as [Person],
                [a34].[Value] as [Weight],
                [a35].[Value] as [PensionStatus],
                [a36].[Value] as [Height],
                [a37].[Value] as [ReferenceNumber],
                [a38].[Value] as [EducationAct],
                [a39].[Value] as [Examination4],
                [a40].[Value] as [MedicalOrgAddress],
                [a41].[Value] as [PensionAmount],
                [a42].[Value] as [Occupation],
                [a43].[Value] as [RegNoAct],
                [a44].[Value] as [ExaminationPrRe],
                [a45].[Value] as [ResidentialAddress],
                [a46].[Value] as [Area],
                [a47].[Value] as [Village_Doc],
                [a48].[Value] as [Phone],
                [a49].[Value] as [Djamoat_Doc],
                [a50].[Value] as [ExamOfDate],
                [a51].[Value] as [ExamStartDate],
                [a52].[Value] as [ExamFinishDate],
                [a53].[File_Name] as [ExpertExamResult7Comment],
				[a53].[Value] as [ExpertExamResult7CommentValue],
                [a54].[File_Name] as [ExpertExamResult2],
                [a54].[Value] as [ExpertExamResult2Value],
                [a55].[File_Name] as [ExpertExamResult1],
                [a55].[Value] as [ExpertExamResult1Value],
                [a56].[File_Name] as [ExpertExamResult6],
                [a56].[Value] as [ExpertExamResult6Value],
                [a57].[File_Name] as [ExpertExamResult9Comment],
                [a57].[Value] as [ExpertExamResult9CommentValue],
                [a58].[File_Name] as [ExpertExamResult7],
                [a58].[Value] as [ExpertExamResult7Value],
                [a59].[File_Name] as [ExpertExamResult8],
                [a59].[Value] as [ExpertExamResult8Value],
                [a60].[File_Name] as [ExpertExamResult3],
                [a60].[Value] as [ExpertExamResult3Value],
                [a61].[File_Name] as [ExpertExamResult5Comment],
                [a61].[Value] as [ExpertExamResult5CommentValue],
                [a62].[File_Name] as [ExpertExamResult],
                [a62].[Value] as [ExpertExamResultValue],
                [a63].[File_Name] as [ExpertExamResult10],
                [a63].[Value] as [ExpertExamResult10Value],
                [a64].[File_Name] as [ExpertExamResult4Comment],
                [a64].[Value] as [ExpertExamResult4CommentValue],
                [a65].[File_Name] as [ExpertExamResult10Comment],
                [a65].[Value] as [ExpertExamResult10CommentValue],
                [a66].[File_Name] as [ExpertExamResult6Comment],
                [a66].[Value] as [ExpertExamResult6CommentValue],
                [a67].[File_Name] as [ExpertExamResult5],
                [a67].[Value] as [ExpertExamResult5Value],
                [a68].[File_Name] as [ExpertExamResult4],
                [a68].[Value] as [ExpertExamResult4Value],
                [a69].[File_Name] as [ExpertExamResult11],
                [a69].[Value] as [ExpertExamResult11Value],
                [a70].[File_Name] as [ExpertExamResult9],
                [a70].[Value] as [ExpertExamResult9Value],
                [a71].[File_Name] as [ExpertExamResult11Comment],
                [a71].[Value] as [ExpertExamResult11CommentValue],
                [a72].[Value] as [goal3],
                [a73].[Value] as [goal7],
                [a74].[Value] as [goal6],
                [a75].[Value] as [goal4],
                [a76].[Value] as [goal1],
                [a77].[Value] as [goalAnother],
                [a78].[Value] as [goal5],
                [a79].[Value] as [goal9],
                [a80].[Value] as [goal8],
                [a81].[Value] as [goal2],
                [a82].[File_Name] as [CompanionDiagnosis],
                [a82].[Value] as [CompanionDiagnosisValue],
                [a83].[Value] as [CompainDagnosisMKB2],
                [a84].[File_Name] as [MainDiagnosis],
                [a84].[Value] as [MainDiagnosisValue],
                [a85].[Value] as [Diagnosis],
                [a86].[File_Name] as [DisabilityReasonText],
                [a86].[Value] as [DisabilityReasonTextValue],
                [a87].[File_Name] as [DiseaseComplication],
                [a87].[Value] as [DiseaseComplicationValue],
                [a88].[Value] as [CompainDagnosisMKB3],
                [a89].[Value] as [CompainDagnosisMKB1],
                [a90].[Value] as [ExamDateTo],
                [a91].[Value] as [ExamDateFrom],
                [a92].[Value] as [NeedsCare],
                [a93].[Value] as [ReconstructiveSurgery],
                [a94].[Value] as [NeedsSupervision],
                [a95].[Value] as [DisabilityReason],
                [a96].[Value] as [RestorativeTherapy],
                [a97].[Value] as [HospitalTreatment],
                [a98].[Value] as [DisabilityGroupAct],
                [a99].[Value] as [Hospitalization],
                [a100].[Value] as [MiddleNameParents],
                [a101].[Value] as [FirstNameParents],
                [a102].[Value] as [DateOfBirthParents],
                [a103].[Value] as [GenderParents],
                [a104].[Value] as [LastNameParents],
                [a105].[Value] as [SickListType],
                [a106].[Value] as [SickListPeriodFrom],
                [a107].[Value] as [SickListPeriodTo],
                [a108].[Value] as [SickListSeries],
                [a109].[Value] as [Village],
                [a110].[Value] as [subDistrict],
                [a111].[Value] as [District],
                [a112].[Value] as [Country],
                [a113].[Value] as [Region],
                [ds114].[State_Type_Id] as [State],
				[ds114].Created state_created_at,
				[ds114].Worker_Id StatusAuthorUserId
        FROM
                Documents d WITH(NOLOCK)
                LEFT OUTER JOIN Date_Time_Attributes a1 WITH(NOLOCK) on (a1.Document_Id = d.Id and a1.Def_Id = '8233bfb2-b318-42ad-a1f4-077944bb26f2' and a1.Expired = '99991231')
                LEFT OUTER JOIN Document_Attributes a2 WITH(NOLOCK) on (a2.Document_Id = d.Id and a2.Def_Id = '466aa66d-be96-411c-9a07-538c27418fb8' and a2.Expired = '99991231')
                LEFT OUTER JOIN Text_Attributes a3 WITH(NOLOCK) on (a3.Document_Id = d.Id and a3.Def_Id = 'c4a0eaa9-afbc-465d-aa2b-9f19edb7d815' and a3.Expired = '99991231')
                LEFT OUTER JOIN Enum_Attributes a4 WITH(NOLOCK) on (a4.Document_Id = d.Id and a4.Def_Id = 'c705a17b-6f39-4b75-a24a-16245466be91' and a4.Expired = '99991231')
                LEFT OUTER JOIN Date_Time_Attributes a5 WITH(NOLOCK) on (a5.Document_Id = d.Id and a5.Def_Id = 'a20ee127-1895-4f54-9a95-1b4b8a312ce8' and a5.Expired = '99991231')
                LEFT OUTER JOIN Text_Attributes a6 WITH(NOLOCK) on (a6.Document_Id = d.Id and a6.Def_Id = '9348c7c2-9e76-4362-a7de-255d9f3c967e' and a6.Expired = '99991231')
                LEFT OUTER JOIN Enum_Attributes a7 WITH(NOLOCK) on (a7.Document_Id = d.Id and a7.Def_Id = '1012fe78-569b-4b03-9b93-262263be07b2' and a7.Expired = '99991231')
                LEFT OUTER JOIN Enum_Attributes a8 WITH(NOLOCK) on (a8.Document_Id = d.Id and a8.Def_Id = 'a76a7b97-e46f-4c18-a9a0-33699f9e1910' and a8.Expired = '99991231')
                LEFT OUTER JOIN Text_Attributes a9 WITH(NOLOCK) on (a9.Document_Id = d.Id and a9.Def_Id = 'ee55445d-20cd-435c-ba06-3f5b61e5cf3f' and a9.Expired = '99991231')
                LEFT OUTER JOIN Enum_Attributes a10 WITH(NOLOCK) on (a10.Document_Id = d.Id and a10.Def_Id = '0a8843e6-a10b-41b5-a1ef-68f8db9eb2d6' and a10.Expired = '99991231')
                LEFT OUTER JOIN Enum_Attributes a11 WITH(NOLOCK) on (a11.Document_Id = d.Id and a11.Def_Id = '12222ec4-8c55-4bb5-8ff7-78288f1fd986' and a11.Expired = '99991231')
                LEFT OUTER JOIN Enum_Attributes a12 WITH(NOLOCK) on (a12.Document_Id = d.Id and a12.Def_Id = 'db806fc4-5723-4990-9adf-7a726762abb5' and a12.Expired = '99991231')
                LEFT OUTER JOIN Enum_Attributes a13 WITH(NOLOCK) on (a13.Document_Id = d.Id and a13.Def_Id = '1010fb95-eeb5-4e84-9182-87b2ef61363a' and a13.Expired = '99991231')
                LEFT OUTER JOIN Enum_Attributes a14 WITH(NOLOCK) on (a14.Document_Id = d.Id and a14.Def_Id = 'ed16862a-3ba5-45ab-a425-8a0b2fa4dbd3' and a14.Expired = '99991231')
                LEFT OUTER JOIN Enum_Attributes a15 WITH(NOLOCK) on (a15.Document_Id = d.Id and a15.Def_Id = '0dbb0001-c5ec-4a57-b952-8ff095eb4a48' and a15.Expired = '99991231')
                LEFT OUTER JOIN Text_Attributes a16 WITH(NOLOCK) on (a16.Document_Id = d.Id and a16.Def_Id = 'ef15b6c0-0fcd-4e9c-a71e-99f4750e8cfa' and a16.Expired = '99991231')
                LEFT OUTER JOIN Text_Attributes a17 WITH(NOLOCK) on (a17.Document_Id = d.Id and a17.Def_Id = '7b6c5804-6ff9-4fdd-af44-a0e6ac898204' and a17.Expired = '99991231')
                LEFT OUTER JOIN Date_Time_Attributes a18 WITH(NOLOCK) on (a18.Document_Id = d.Id and a18.Def_Id = '7e2592f9-4f3c-4135-8cd1-a6c012f47628' and a18.Expired = '99991231')
                LEFT OUTER JOIN Enum_Attributes a19 WITH(NOLOCK) on (a19.Document_Id = d.Id and a19.Def_Id = 'b43dea4f-ff79-45ec-94c0-c1dbe0174683' and a19.Expired = '99991231')
                LEFT OUTER JOIN Text_Attributes a20 WITH(NOLOCK) on (a20.Document_Id = d.Id and a20.Def_Id = 'd70e9eb0-97b9-42cd-a712-c9516d5bc010' and a20.Expired = '99991231')
                LEFT OUTER JOIN Text_Attributes a21 WITH(NOLOCK) on (a21.Document_Id = d.Id and a21.Def_Id = '32015ddc-0858-4bfd-a899-c970e61067fa' and a21.Expired = '99991231')
                LEFT OUTER JOIN Enum_Attributes a22 WITH(NOLOCK) on (a22.Document_Id = d.Id and a22.Def_Id = '2354b1b3-e06c-4b7c-bdac-db4c9b001aa2' and a22.Expired = '99991231')
                LEFT OUTER JOIN Date_Time_Attributes a23 WITH(NOLOCK) on (a23.Document_Id = d.Id and a23.Def_Id = 'ff444eea-6292-48f6-b054-155bd8876b28' and a23.Expired = '99991231')
                LEFT OUTER JOIN Text_Attributes a24 WITH(NOLOCK) on (a24.Document_Id = d.Id and a24.Def_Id = '938e9ac8-5022-4255-b1f5-5180d21946ad' and a24.Expired = '99991231')
                LEFT OUTER JOIN Text_Attributes a25 WITH(NOLOCK) on (a25.Document_Id = d.Id and a25.Def_Id = '7051e628-bf25-4339-9a21-97a8e04b4178' and a25.Expired = '99991231')
                LEFT OUTER JOIN Text_Attributes a26 WITH(NOLOCK) on (a26.Document_Id = d.Id and a26.Def_Id = '71900c90-e243-4e9a-b637-d46474d029bb' and a26.Expired = '99991231')
                LEFT OUTER JOIN Enum_Attributes a27 WITH(NOLOCK) on (a27.Document_Id = d.Id and a27.Def_Id = '45c1fc7b-9b1b-4331-849b-e28f5cc9f342' and a27.Expired = '99991231')
                LEFT OUTER JOIN Boolean_Attributes a28 WITH(NOLOCK) on (a28.Document_Id = d.Id and a28.Def_Id = 'eaaa1364-b4b2-44f5-a1c3-95849e79ec05' and a28.Expired = '99991231')
                LEFT OUTER JOIN Date_Time_Attributes a29 WITH(NOLOCK) on (a29.Document_Id = d.Id and a29.Def_Id = '97c8b525-2a79-4a0e-8257-a22254d41730' and a29.Expired = '99991231')
                LEFT OUTER JOIN Date_Time_Attributes a30 WITH(NOLOCK) on (a30.Document_Id = d.Id and a30.Def_Id = '98a3c5ed-9b9e-4f04-a728-13429ff0856d' and a30.Expired = '99991231')
                LEFT OUTER JOIN Text_Attributes a31 WITH(NOLOCK) on (a31.Document_Id = d.Id and a31.Def_Id = 'adcc29bd-3938-4567-9ad1-3dad4f5960f4' and a31.Expired = '99991231')
                LEFT OUTER JOIN Text_Attributes a32 WITH(NOLOCK) on (a32.Document_Id = d.Id and a32.Def_Id = 'bb904b2a-ef95-4bb2-92bc-7131918ddd4e' and a32.Expired = '99991231')
                LEFT OUTER JOIN Document_Attributes a33 WITH(NOLOCK) on (a33.Document_Id = d.Id and a33.Def_Id = '990a7f49-04d6-4504-9830-779db5cd1dfc' and a33.Expired = '99991231')
                LEFT OUTER JOIN Float_Attributes a34 WITH(NOLOCK) on (a34.Document_Id = d.Id and a34.Def_Id = 'c7ead027-7b2f-4015-b56f-8756ab92797f' and a34.Expired = '99991231')
                LEFT OUTER JOIN Enum_Attributes a35 WITH(NOLOCK) on (a35.Document_Id = d.Id and a35.Def_Id = 'af46d979-7820-4748-91d3-8b33cf5df156' and a35.Expired = '99991231')
                LEFT OUTER JOIN Float_Attributes a36 WITH(NOLOCK) on (a36.Document_Id = d.Id and a36.Def_Id = '37e6dc2a-19cf-45db-b888-8ec92402ffda' and a36.Expired = '99991231')
                LEFT OUTER JOIN Text_Attributes a37 WITH(NOLOCK) on (a37.Document_Id = d.Id and a37.Def_Id = 'bc36865c-77d1-4203-ba1c-a0e16a769ab9' and a37.Expired = '99991231')
                LEFT OUTER JOIN Enum_Attributes a38 WITH(NOLOCK) on (a38.Document_Id = d.Id and a38.Def_Id = 'f4bfd488-9d6d-47e1-8a62-bd9c5d1731ce' and a38.Expired = '99991231')
                LEFT OUTER JOIN Enum_Attributes a39 WITH(NOLOCK) on (a39.Document_Id = d.Id and a39.Def_Id = '9d2f6ac8-c0ba-48b4-94e2-c0a29e2b7037' and a39.Expired = '99991231')
                LEFT OUTER JOIN Text_Attributes a40 WITH(NOLOCK) on (a40.Document_Id = d.Id and a40.Def_Id = 'fdcd654a-6bf9-49a6-bb61-c5545201eac0' and a40.Expired = '99991231')
                LEFT OUTER JOIN Currency_Attributes a41 WITH(NOLOCK) on (a41.Document_Id = d.Id and a41.Def_Id = '7f8f4f62-3328-4f84-9cf3-cdee08eebeea' and a41.Expired = '99991231')
                LEFT OUTER JOIN Enum_Attributes a42 WITH(NOLOCK) on (a42.Document_Id = d.Id and a42.Def_Id = '6a81943a-70c8-43a1-bb89-d64b95cfddbc' and a42.Expired = '99991231')
                LEFT OUTER JOIN Text_Attributes a43 WITH(NOLOCK) on (a43.Document_Id = d.Id and a43.Def_Id = 'e059c1b6-676b-48af-842e-ebfe4c014016' and a43.Expired = '99991231')
                LEFT OUTER JOIN Enum_Attributes a44 WITH(NOLOCK) on (a44.Document_Id = d.Id and a44.Def_Id = 'ad3a5d9f-3cc3-493a-b4c6-f0ee1d41d639' and a44.Expired = '99991231')
                LEFT OUTER JOIN Text_Attributes a45 WITH(NOLOCK) on (a45.Document_Id = d.Id and a45.Def_Id = 'cdf2b51c-3d94-40d8-bb39-07fffa23ccb4' and a45.Expired = '99991231')
                LEFT OUTER JOIN Document_Attributes a46 WITH(NOLOCK) on (a46.Document_Id = d.Id and a46.Def_Id = '5113f2a0-549b-47fb-9900-28c73c4e059a' and a46.Expired = '99991231')
                LEFT OUTER JOIN Document_Attributes a47 WITH(NOLOCK) on (a47.Document_Id = d.Id and a47.Def_Id = 'ffd23546-a7c8-46ef-a3e0-7bf3b05c60f8' and a47.Expired = '99991231')
                LEFT OUTER JOIN Text_Attributes a48 WITH(NOLOCK) on (a48.Document_Id = d.Id and a48.Def_Id = 'c6ac57ea-8a8a-462f-8bbd-da7aa25241e0' and a48.Expired = '99991231')
                LEFT OUTER JOIN Document_Attributes a49 WITH(NOLOCK) on (a49.Document_Id = d.Id and a49.Def_Id = 'ff42b54e-50d0-4654-8257-f716e93781c8' and a49.Expired = '99991231')
                LEFT OUTER JOIN Date_Time_Attributes a50 WITH(NOLOCK) on (a50.Document_Id = d.Id and a50.Def_Id = 'e047e6d8-bfe5-45eb-905b-2d930ac39f53' and a50.Expired = '99991231')
                LEFT OUTER JOIN Date_Time_Attributes a51 WITH(NOLOCK) on (a51.Document_Id = d.Id and a51.Def_Id = 'd0550d7b-801c-4a0c-bf89-90894ad76356' and a51.Expired = '99991231')
                LEFT OUTER JOIN Date_Time_Attributes a52 WITH(NOLOCK) on (a52.Document_Id = d.Id and a52.Def_Id = 'd74c6c2f-9f57-45fd-8eea-98c62fd5ddd6' and a52.Expired = '99991231')
                LEFT OUTER JOIN Image_Attributes a53 WITH(NOLOCK) on (a53.Document_Id = d.Id and a53.Def_Id = '75a12215-c97a-40ee-a0e5-025209b1e3d2' and a53.Expired = '99991231')
                LEFT OUTER JOIN Image_Attributes a54 WITH(NOLOCK) on (a54.Document_Id = d.Id and a54.Def_Id = '796e95e8-c2c2-42a0-91aa-2501c267cbf3' and a54.Expired = '99991231')
                LEFT OUTER JOIN Image_Attributes a55 WITH(NOLOCK) on (a55.Document_Id = d.Id and a55.Def_Id = '744179a9-952a-4af7-932b-295f67ac6def' and a55.Expired = '99991231')
                LEFT OUTER JOIN Image_Attributes a56 WITH(NOLOCK) on (a56.Document_Id = d.Id and a56.Def_Id = 'fe54ea42-531a-45e2-984b-2b8eeff0b2d5' and a56.Expired = '99991231')
                LEFT OUTER JOIN Image_Attributes a57 WITH(NOLOCK) on (a57.Document_Id = d.Id and a57.Def_Id = 'fe664227-1404-4b67-81d5-40bb16bdae20' and a57.Expired = '99991231')
                LEFT OUTER JOIN Image_Attributes a58 WITH(NOLOCK) on (a58.Document_Id = d.Id and a58.Def_Id = '3849e8b1-e3ab-4685-a97e-52f4e6dc2473' and a58.Expired = '99991231')
                LEFT OUTER JOIN Image_Attributes a59 WITH(NOLOCK) on (a59.Document_Id = d.Id and a59.Def_Id = 'c4889c6c-0f95-4ff5-98b2-5789dfed8605' and a59.Expired = '99991231')
                LEFT OUTER JOIN Image_Attributes a60 WITH(NOLOCK) on (a60.Document_Id = d.Id and a60.Def_Id = '39b54491-240a-442f-a05b-639020ca1d86' and a60.Expired = '99991231')
                LEFT OUTER JOIN Image_Attributes a61 WITH(NOLOCK) on (a61.Document_Id = d.Id and a61.Def_Id = 'd4445051-e6dd-4327-bf0e-6471dc4d4b4e' and a61.Expired = '99991231')
                LEFT OUTER JOIN Image_Attributes a62 WITH(NOLOCK) on (a62.Document_Id = d.Id and a62.Def_Id = '4224707b-a673-4f22-94fd-69d54a66049f' and a62.Expired = '99991231')
                LEFT OUTER JOIN Image_Attributes a63 WITH(NOLOCK) on (a63.Document_Id = d.Id and a63.Def_Id = '733976f1-fec2-43a0-b2b2-7e3260afabee' and a63.Expired = '99991231')
                LEFT OUTER JOIN Image_Attributes a64 WITH(NOLOCK) on (a64.Document_Id = d.Id and a64.Def_Id = '4a5f6fe9-d3ab-406d-a9dd-9ea020839588' and a64.Expired = '99991231')
                LEFT OUTER JOIN Image_Attributes a65 WITH(NOLOCK) on (a65.Document_Id = d.Id and a65.Def_Id = '40b48a05-139c-47a7-8ca6-a017be8a1c45' and a65.Expired = '99991231')
                LEFT OUTER JOIN Image_Attributes a66 WITH(NOLOCK) on (a66.Document_Id = d.Id and a66.Def_Id = 'acc84812-0ef8-44ad-a100-a3d43f219e8e' and a66.Expired = '99991231')
                LEFT OUTER JOIN Image_Attributes a67 WITH(NOLOCK) on (a67.Document_Id = d.Id and a67.Def_Id = 'afd6dd54-8ad5-491e-964d-d0985461b6b9' and a67.Expired = '99991231')
                LEFT OUTER JOIN Image_Attributes a68 WITH(NOLOCK) on (a68.Document_Id = d.Id and a68.Def_Id = '81afed26-3c4b-4f97-98df-db0fd52f5a43' and a68.Expired = '99991231')
                LEFT OUTER JOIN Image_Attributes a69 WITH(NOLOCK) on (a69.Document_Id = d.Id and a69.Def_Id = '5978b57d-6690-4fcc-bb54-e9dcd771b0ee' and a69.Expired = '99991231')
                LEFT OUTER JOIN Image_Attributes a70 WITH(NOLOCK) on (a70.Document_Id = d.Id and a70.Def_Id = 'd8cd47f4-79a1-4761-8f58-ee851efdd7eb' and a70.Expired = '99991231')
                LEFT OUTER JOIN Image_Attributes a71 WITH(NOLOCK) on (a71.Document_Id = d.Id and a71.Def_Id = '6ce9d9ed-20b8-4d19-b074-fdacbc6e817b' and a71.Expired = '99991231')
                LEFT OUTER JOIN Boolean_Attributes a72 WITH(NOLOCK) on (a72.Document_Id = d.Id and a72.Def_Id = '37bd0fec-7a64-4cfd-9464-2d72c44f7fe2' and a72.Expired = '99991231')
                LEFT OUTER JOIN Boolean_Attributes a73 WITH(NOLOCK) on (a73.Document_Id = d.Id and a73.Def_Id = '5658c194-4720-44d6-aa55-3bca9f5c4338' and a73.Expired = '99991231')
                LEFT OUTER JOIN Boolean_Attributes a74 WITH(NOLOCK) on (a74.Document_Id = d.Id and a74.Def_Id = '7ab97921-52c3-468b-af0d-47bd0a35393b' and a74.Expired = '99991231')
                LEFT OUTER JOIN Enum_Attributes a75 WITH(NOLOCK) on (a75.Document_Id = d.Id and a75.Def_Id = '07ffb5e1-85ae-4a91-8f3f-4d113bc1c226' and a75.Expired = '99991231')
                LEFT OUTER JOIN Boolean_Attributes a76 WITH(NOLOCK) on (a76.Document_Id = d.Id and a76.Def_Id = '7458e75b-962e-4ce6-9d73-6c8bf30525df' and a76.Expired = '99991231')
                LEFT OUTER JOIN Text_Attributes a77 WITH(NOLOCK) on (a77.Document_Id = d.Id and a77.Def_Id = 'ad7e532d-be6d-4449-82b0-6f58ffc04a8a' and a77.Expired = '99991231')
                LEFT OUTER JOIN Boolean_Attributes a78 WITH(NOLOCK) on (a78.Document_Id = d.Id and a78.Def_Id = '201aa9bc-6265-48c1-b41d-956d16b35669' and a78.Expired = '99991231')
                LEFT OUTER JOIN Boolean_Attributes a79 WITH(NOLOCK) on (a79.Document_Id = d.Id and a79.Def_Id = 'c3f83f15-d7b3-4b59-8a73-c2c2fbabeb50' and a79.Expired = '99991231')
                LEFT OUTER JOIN Boolean_Attributes a80 WITH(NOLOCK) on (a80.Document_Id = d.Id and a80.Def_Id = '2d9ec5af-d36e-4ef5-aadb-ef002cb5cf86' and a80.Expired = '99991231')
                LEFT OUTER JOIN Boolean_Attributes a81 WITH(NOLOCK) on (a81.Document_Id = d.Id and a81.Def_Id = 'c1b6750a-8583-4f10-bdae-f407ea9bf950' and a81.Expired = '99991231')
                LEFT OUTER JOIN Image_Attributes a82 WITH(NOLOCK) on (a82.Document_Id = d.Id and a82.Def_Id = '5d351f17-2451-4873-822b-162e4107807e' and a82.Expired = '99991231')
                LEFT OUTER JOIN Enum_Attributes a83 WITH(NOLOCK) on (a83.Document_Id = d.Id and a83.Def_Id = 'a6837a68-1bab-48ae-a9b4-a5eb76ce0a97' and a83.Expired = '99991231')
                LEFT OUTER JOIN Image_Attributes a84 WITH(NOLOCK) on (a84.Document_Id = d.Id and a84.Def_Id = '24d977af-349f-43db-911b-b375400b38bf' and a84.Expired = '99991231')
                LEFT OUTER JOIN Enum_Attributes a85 WITH(NOLOCK) on (a85.Document_Id = d.Id and a85.Def_Id = 'cdbca7e5-7ceb-47a0-bb0b-c40b62aa718a' and a85.Expired = '99991231')
                LEFT OUTER JOIN Image_Attributes a86 WITH(NOLOCK) on (a86.Document_Id = d.Id and a86.Def_Id = 'bb502cae-d5dd-4886-8468-c86b823133b1' and a86.Expired = '99991231')
                LEFT OUTER JOIN Image_Attributes a87 WITH(NOLOCK) on (a87.Document_Id = d.Id and a87.Def_Id = 'b9b47b2f-96ec-4efb-aadb-d59c22378d60' and a87.Expired = '99991231')
                LEFT OUTER JOIN Enum_Attributes a88 WITH(NOLOCK) on (a88.Document_Id = d.Id and a88.Def_Id = '10a15a67-0e9e-4e5e-b667-d77246f7466f' and a88.Expired = '99991231')
                LEFT OUTER JOIN Enum_Attributes a89 WITH(NOLOCK) on (a89.Document_Id = d.Id and a89.Def_Id = '1bbadcd5-7fa1-4494-8d0f-e0fb7fe706ee' and a89.Expired = '99991231')
                LEFT OUTER JOIN Date_Time_Attributes a90 WITH(NOLOCK) on (a90.Document_Id = d.Id and a90.Def_Id = '3ae9844a-beca-4071-a0fc-166982a6a1bd' and a90.Expired = '99991231')
                LEFT OUTER JOIN Date_Time_Attributes a91 WITH(NOLOCK) on (a91.Document_Id = d.Id and a91.Def_Id = 'b5bed28d-3d52-42e9-99b5-3ebad3e35e1f' and a91.Expired = '99991231')
                LEFT OUTER JOIN Boolean_Attributes a92 WITH(NOLOCK) on (a92.Document_Id = d.Id and a92.Def_Id = 'b5fa9282-4d2c-4673-a462-0e167b2e12d5' and a92.Expired = '99991231')
                LEFT OUTER JOIN Boolean_Attributes a93 WITH(NOLOCK) on (a93.Document_Id = d.Id and a93.Def_Id = '71ea17a0-e1aa-4312-9e29-1d2fa2dee4d0' and a93.Expired = '99991231')
                LEFT OUTER JOIN Boolean_Attributes a94 WITH(NOLOCK) on (a94.Document_Id = d.Id and a94.Def_Id = '8c6c67ef-d02c-4221-80a7-2deba368297e' and a94.Expired = '99991231')
                LEFT OUTER JOIN Enum_Attributes a95 WITH(NOLOCK) on (a95.Document_Id = d.Id and a95.Def_Id = '0962e665-7d82-4b40-b6d8-65f65e1b6a8a' and a95.Expired = '99991231')
                LEFT OUTER JOIN Boolean_Attributes a96 WITH(NOLOCK) on (a96.Document_Id = d.Id and a96.Def_Id = '49631bef-8fe5-43bf-9a6d-74d8c9c65cf6' and a96.Expired = '99991231')
                LEFT OUTER JOIN Boolean_Attributes a97 WITH(NOLOCK) on (a97.Document_Id = d.Id and a97.Def_Id = 'd469a2f3-2e90-4a27-999d-76fa6f00b857' and a97.Expired = '99991231')
                LEFT OUTER JOIN Enum_Attributes a98 WITH(NOLOCK) on (a98.Document_Id = d.Id and a98.Def_Id = '4ac1041f-e9cd-4fa6-8b41-8baeb86601fb' and a98.Expired = '99991231')
                LEFT OUTER JOIN Boolean_Attributes a99 WITH(NOLOCK) on (a99.Document_Id = d.Id and a99.Def_Id = '071b803c-10aa-477e-a0b0-b9831ab784f8' and a99.Expired = '99991231')
                LEFT OUTER JOIN Text_Attributes a100 WITH(NOLOCK) on (a100.Document_Id = d.Id and a100.Def_Id = '801e4b4f-535a-4875-b767-14c2cdf60161' and a100.Expired = '99991231')
                LEFT OUTER JOIN Text_Attributes a101 WITH(NOLOCK) on (a101.Document_Id = d.Id and a101.Def_Id = '3e6443df-305c-473b-8ea7-2644f81268a8' and a101.Expired = '99991231')
                LEFT OUTER JOIN Date_Time_Attributes a102 WITH(NOLOCK) on (a102.Document_Id = d.Id and a102.Def_Id = '10d5f696-cff1-4f7f-91f9-2d9893847495' and a102.Expired = '99991231')
                LEFT OUTER JOIN Enum_Attributes a103 WITH(NOLOCK) on (a103.Document_Id = d.Id and a103.Def_Id = '4abb3327-6a7d-45e3-af82-9df932e87bdb' and a103.Expired = '99991231')
                LEFT OUTER JOIN Text_Attributes a104 WITH(NOLOCK) on (a104.Document_Id = d.Id and a104.Def_Id = '65a0541c-6fd5-495a-a58a-a8e42ad1fe30' and a104.Expired = '99991231')
                LEFT OUTER JOIN Text_Attributes a105 WITH(NOLOCK) on (a105.Document_Id = d.Id and a105.Def_Id = '9f85e305-53c8-4cdc-9ee0-0cd29a1d1675' and a105.Expired = '99991231')
                LEFT OUTER JOIN Date_Time_Attributes a106 WITH(NOLOCK) on (a106.Document_Id = d.Id and a106.Def_Id = 'a75874b2-5c31-49eb-a967-1f9ed28f739c' and a106.Expired = '99991231')
                LEFT OUTER JOIN Date_Time_Attributes a107 WITH(NOLOCK) on (a107.Document_Id = d.Id and a107.Def_Id = '3bdffbcb-61ce-472c-a9eb-50160e0712db' and a107.Expired = '99991231')
                LEFT OUTER JOIN Text_Attributes a108 WITH(NOLOCK) on (a108.Document_Id = d.Id and a108.Def_Id = '36a8fa70-00bb-4195-bed1-c19b785fa975' and a108.Expired = '99991231')
                LEFT OUTER JOIN Text_Attributes a109 WITH(NOLOCK) on (a109.Document_Id = d.Id and a109.Def_Id = '3f1209e7-5057-4ffe-b741-2a1d0a4566a4' and a109.Expired = '99991231')
                LEFT OUTER JOIN Text_Attributes a110 WITH(NOLOCK) on (a110.Document_Id = d.Id and a110.Def_Id = 'b9829b00-62fa-4476-be2c-4454cfe8ac8e' and a110.Expired = '99991231')
                LEFT OUTER JOIN Text_Attributes a111 WITH(NOLOCK) on (a111.Document_Id = d.Id and a111.Def_Id = '7acf57e4-72a3-4eed-af4e-73d456a5fcce' and a111.Expired = '99991231')
                LEFT OUTER JOIN Text_Attributes a112 WITH(NOLOCK) on (a112.Document_Id = d.Id and a112.Def_Id = '9663c426-52ab-4143-ab0c-93ad2e578781' and a112.Expired = '99991231')
                LEFT OUTER JOIN Text_Attributes a113 WITH(NOLOCK) on (a113.Document_Id = d.Id and a113.Def_Id = 'b85b1308-4148-42a9-8b36-ba491168c7d3' and a113.Expired = '99991231')
                INNER JOIN [Document_States] ds114 WITH(NOLOCK) on (ds114.Document_Id = d.Id and ds114.Expired = '99991231' and ds114.State_Type_Id IN({0}))
				inner join [msec-meta].dbo.Object_Defs o with(nolock) on o.Id = ds114.State_Type_Id
        WHERE
                d.Def_Id = '5fde415f-db00-43b4-ba6e-fe439cff6da0' AND d.[Deleted] is null
        ) as [*ChildMedicalCart]
        INNER JOIN (SELECT
                d.Id,
                [a1].[Value] as [PassportSeries],
                [a2].[Value] as [PassportType],
                [a3].[Value] as [FamilyState],
                [a4].[Value] as [Date_of_Birth],
                [a5].[Value] as [Sex],
                [a6].[Value] as [PassportNo],
                [a7].[Value] as [SIN],
                [a8].[Value] as [Middle_Name],
                [a9].[Value] as [IIN],
                [a10].[Value] as [First_Name],
                [a11].[Value] as [Last_Name],
                [a12].[Value] as [NationalID],
                [a13].[Value] as [Issuing_Authority],
                [a14].[Value] as [Date_of_Issue],
                [a15].[Value] as [INN]
        FROM
                Documents d WITH(NOLOCK)
                LEFT OUTER JOIN Text_Attributes a1 WITH(NOLOCK) on (a1.Document_Id = d.Id and a1.Def_Id = 'c4a72cbd-8153-4b27-8346-0624609857ba' and a1.Expired = '99991231')
                LEFT OUTER JOIN Enum_Attributes a2 WITH(NOLOCK) on (a2.Document_Id = d.Id and a2.Def_Id = 'c700bece-a8fe-4bf2-b95f-096fa4d4e1c0' and a2.Expired = '99991231')
                LEFT OUTER JOIN Enum_Attributes a3 WITH(NOLOCK) on (a3.Document_Id = d.Id and a3.Def_Id = '2d20e752-fb4e-4ed8-9674-46ec9101b9ec' and a3.Expired = '99991231')
                LEFT OUTER JOIN Date_Time_Attributes a4 WITH(NOLOCK) on (a4.Document_Id = d.Id and a4.Def_Id = '706aad5a-6b08-4b15-8e2f-49b175c999e8' and a4.Expired = '99991231')
                LEFT OUTER JOIN Enum_Attributes a5 WITH(NOLOCK) on (a5.Document_Id = d.Id and a5.Def_Id = '4398c35a-606e-494e-9f02-69539c9fe61c' and a5.Expired = '99991231')
                LEFT OUTER JOIN Text_Attributes a6 WITH(NOLOCK) on (a6.Document_Id = d.Id and a6.Def_Id = 'bf478bbf-3dd0-4c10-a9e9-7227cf9e1a29' and a6.Expired = '99991231')
                LEFT OUTER JOIN Text_Attributes a7 WITH(NOLOCK) on (a7.Document_Id = d.Id and a7.Def_Id = 'ea3a496e-3a03-4f26-8058-823082619903' and a7.Expired = '99991231')
                LEFT OUTER JOIN Text_Attributes a8 WITH(NOLOCK) on (a8.Document_Id = d.Id and a8.Def_Id = 'ba0713d4-d7f6-4a90-8e3e-8670c8cdf305' and a8.Expired = '99991231')
                LEFT OUTER JOIN Text_Attributes a9 WITH(NOLOCK) on (a9.Document_Id = d.Id and a9.Def_Id = '26dd8a20-144d-4931-8dd6-8e8d3271ddee' and a9.Expired = '99991231')
                LEFT OUTER JOIN Text_Attributes a10 WITH(NOLOCK) on (a10.Document_Id = d.Id and a10.Def_Id = '9856ca78-074d-422f-912c-92d96ee6e8bc' and a10.Expired = '99991231')
                LEFT OUTER JOIN Text_Attributes a11 WITH(NOLOCK) on (a11.Document_Id = d.Id and a11.Def_Id = '4eaca45a-a143-4ca9-8408-cb2c89d9e63f' and a11.Expired = '99991231')
                LEFT OUTER JOIN Text_Attributes a12 WITH(NOLOCK) on (a12.Document_Id = d.Id and a12.Def_Id = '400ee5d8-8f97-4d2c-a029-e1db1c8b9f83' and a12.Expired = '99991231')
                LEFT OUTER JOIN Text_Attributes a13 WITH(NOLOCK) on (a13.Document_Id = d.Id and a13.Def_Id = '8cd12bd3-1e72-4983-b4e2-eb89af985406' and a13.Expired = '99991231')
                LEFT OUTER JOIN Date_Time_Attributes a14 WITH(NOLOCK) on (a14.Document_Id = d.Id and a14.Def_Id = '86282a25-ed98-4ea6-8650-ee4cc577e211' and a14.Expired = '99991231')
                LEFT OUTER JOIN Text_Attributes a15 WITH(NOLOCK) on (a15.Document_Id = d.Id and a15.Def_Id = 'abcda93b-c195-45d6-9555-f4dfd10ce832' and a15.Expired = '99991231')
        WHERE
                d.Def_Id = '6052978a-1ecb-4f96-a16b-93548936afc0' 
        ) as [*Person] on [*Person].Id = [*ChildMedicalCart].[Person]
		
		left outer join [msec-meta].dbo.Object_Defs PassportTypeText on PassportTypeText.Id = [*Person].[PassportType]
		left outer join [msec-meta].dbo.Object_Defs FamilyStateText on FamilyStateText.Id = [*Person].[FamilyState]
		left outer join [msec-meta].dbo.Object_Defs SexText on SexText.Id = [*Person].[Sex]
		left outer join [msec-meta].dbo.Object_Defs MseNameText on MseNameText.Id = [*ChildMedicalCart].[MseName]
		left outer join [msec-meta].dbo.Object_Defs OccupationText on OccupationText.Id = [*ChildMedicalCart].[Occupation]
		left outer join [msec-meta].dbo.Object_Defs EducationActText on EducationActText.Id = [*ChildMedicalCart].[EducationAct]
		left outer join [msec-meta].dbo.Object_Defs ExaminationPrReText on ExaminationPrReText.Id = [*ChildMedicalCart].[ExaminationPrRe]
		left outer join [msec-meta].dbo.Object_Defs PensionStatusText on PensionStatusText.Id = [*ChildMedicalCart].[PensionStatus]
		left outer join [msec-meta].dbo.Object_Defs DisabilityReasonText on DisabilityReasonText.Id = [*ChildMedicalCart].[DisabilityReason]	
		left outer join [msec-meta].dbo.Object_Defs CompainDagnosisMKB3Text on CompainDagnosisMKB3Text.Id = [*ChildMedicalCart].[CompainDagnosisMKB3]
		left outer join [msec-meta].dbo.Object_Defs CompainDagnosisMKB2Text on CompainDagnosisMKB2Text.Id = [*ChildMedicalCart].[CompainDagnosisMKB2]
		left outer join [msec-meta].dbo.Object_Defs CompainDagnosisMKB1Text on CompainDagnosisMKB1Text.Id = [*ChildMedicalCart].[CompainDagnosisMKB1]
		left outer join [msec-meta].dbo.Object_Defs DiagnosisText on DiagnosisText.Id = [*ChildMedicalCart].[Diagnosis]
		left outer join [msec-meta].dbo.Object_Defs ConfirmationExistenceText on ConfirmationExistenceText.Id = [*ChildMedicalCart].[ConfirmationExistence]
		left outer join [msec-meta].dbo.Persons status_users on status_users.Id = [*ChildMedicalCart].[StatusAuthorUserId]
";

        const string EQUAL_CONDITION_SQL = @"
{0} = {1}
";
        const string LESS_OR_EQUAL_CONDITION_SQL = @"
{0} <= {1}
";
        const string GREAT_OR_EQUAL_CONDITION_SQL = @"
{0} >= {1}
";
    }
}