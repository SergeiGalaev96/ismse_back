using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Intersoft.CISSA.BizService.Utils;
using Intersoft.CISSA.DataAccessLayer.Core;
using Intersoft.CISSA.DataAccessLayer.Model.Context;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Builders;
using Intersoft.CISSA.DataAccessLayer.Model.Workflow;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Sql;
using Intersoft.CISSA.DataAccessLayer.Model.Data;
using Intersoft.CISSA.DataAccessLayer.Model.Documents;
using Intersoft.CISSA.DataAccessLayer.Repository;

using ISMSE_REST_API.Models;
using Intersoft.CISSA.DataAccessLayer.Model.Enums;
using Intersoft.CISSA.DataAccessLayer.Model.Query;
using ISMSE_REST_API.Models.Address;
using ISMSE_REST_API.Models.Enums;
using ISMSE_REST_API.Extensions;
using System.Data;
using System.Globalization;

namespace ISMSE_REST_API.Util
{
    public static class ScriptExecutor
    {
        static IAppServiceProvider InitProvider(string username, Guid userId)
        {
            var dataContextFactory = DataContextFactoryProvider.GetFactory();

            var dataContext = dataContextFactory.CreateMultiDc("DataContexts");
            BaseServiceFactory.CreateBaseServiceFactories();
            var providerFactory = AppServiceProviderFactoryProvider.GetFactory();
            var provider = providerFactory.Create(dataContext);
            var serviceRegistrator = provider.Get<IAppServiceProviderRegistrator>();
            serviceRegistrator.AddService(new UserDataProvider(userId, username));
            return provider;
        }

        public static WorkflowContext CreateAdminContext()
        {
            var userObj = DAL.GetCissaUser(adminUserId);
            return CreateContext(userObj.UserName, adminUserId);
        }

        public static WorkflowContext CreateContext(Guid userId)
        {
            var userObj = DAL.GetCissaUser(userId);
            return CreateContext(userObj.UserName, userId);
        }
        private static readonly Guid personDefId = new Guid("{6052978A-1ECB-4F96-A16B-93548936AFC0}");
        public static document[] FetchGreaterThan18YearsChildMedacts(document filterDocument, Guid userId, int page = 1, int size = 10)
        {
            var context = CreateContext(userId);

            var filterDate = DateTime.Today.AddYears(-18);
            var ui = context.GetUserInfo();
            var docDefId = CustomExportChildState.APPROVED_AND_REGISTERED.GetDefId();
            var qb = new QueryBuilder(docDefId);
            qb.Where("&State").Eq(CustomExportChildState.APPROVED_AND_REGISTERED.GetValueId()[0])
                .And("&OrgId").Eq(ui.OrganizationId)
                .And("Person").Include("Date_of_Birth").Lt(filterDate).End();

            var query = context.CreateSqlQuery(qb);

            InitQueryConditions(context, query, filterDocument, docDefId, query.Source);

            var personSrc = query.JoinSource(query.Source, personDefId, SqlSourceJoinType.Inner, "Person");

            query.AddAttribute(query.Source, "&Id");
            query.AddAttribute(query.Source, "No");
            query.AddAttribute(query.Source, "RegNoAct");
            query.AddAttribute(query.Source, "MseName");
            query.AddAttribute(query.Source, "ExaminationPrRe");
            query.AddAttribute(query.Source, "Date");
            query.AddAttribute(personSrc, "&Id");
            query.AddAttribute(personSrc, "Last_Name");
            query.AddAttribute(personSrc, "First_Name");
            query.AddAttribute(personSrc, "Middle_Name");
            query.AddAttribute(personSrc, "IIN");
            query.AddAttribute(personSrc, "Date_of_Birth");
            query.AddAttribute(personSrc, "Sex");
            query.AddAttribute(query.Source, "Region");
            query.AddAttribute(query.Source, "District");
            query.AddAttribute(query.Source, "subDistrict");
            query.AddAttribute(query.Source, "Village");
            query.AddAttribute(query.Source, "ResidentialAddress");
            query.AddAttribute(query.Source, "Phone");

            query.TopNo = size;
            query.SkipNo = (page - 1) * size;

            var documents = new List<document>();
            var table = new DataTable();
            using (var reader = new SqlQueryReader(context.DataContext, query))
            {
                reader.Open();
                reader.Fill(table);
                reader.Close();
            }
            foreach (DataRow row in table.Rows)
            {
                var id = (Guid)row[0];
                var attributes = new List<attribute>
                {
                    new attribute
                    {
                        name = "No",
                        type = "Text",
                        value = row[1] is DBNull ? "" : row[1].ToString()
                    },
                    new attribute
                    {
                        name = "RegNoAct",
                        type = "Text",
                        value = row[2] is DBNull ? "" : row[2].ToString()
                    },
                    new attribute
                    {
                        name = "MseName",
                        type = "Enum",
                        value = row[3] is DBNull ? "" : row[3].ToString(),
                        enumDef = new Guid("{CABB8C30-79E7-4F0B-B256-94D1BF942D51}"),
                        enumValueText = row[3] is DBNull ? "" : context.Enums.GetValue((Guid)row[3]).Value,
                    },
                    new attribute
                    {
                        name = "ExaminationPrRe",
                        type = "Enum",
                        value = row[4] is DBNull ? "" : row[4].ToString(),
                        enumDef = new Guid("{CABB8C30-79E7-4F0B-B256-94D1BF942D51}"),
                        enumValueText = row[4] is DBNull ? "" : context.Enums.GetValue((Guid)row[4]).Value,
                    },
                    new attribute
                    {
                        name = "Date",
                        type = "DateTime",
                        value = row[5] is DBNull ? "" : ((DateTime)row[5]).ToString("yyyy-MM-dd"),
                    },
                    new attribute
                    {
                        name = "Person",
                        type = "Doc",
                        value = row[6] is DBNull ? "" : row[6].ToString(),
                        docDef = new Guid("{6052978A-1ECB-4F96-A16B-93548936AFC0}"),
                        subDocument = new document
                        {
                            id = (Guid)row[6],
                            attributes = new attribute[]
                            {
                                new attribute
                                {
                                    name = "Last_Name",
                                    type = "Text",
                                    value = row[7] is DBNull ? "" : row[7].ToString()
                                },
                                new attribute
                                {
                                    name = "First_Name",
                                    type = "Text",
                                    value = row[8] is DBNull ? "" : row[8].ToString()
                                },
                                new attribute
                                {
                                    name = "Middle_Name",
                                    type = "Text",
                                    value = row[9] is DBNull ? "" : row[9].ToString()
                                },
                                new attribute
                                {
                                    name = "IIN",
                                    type = "Text",
                                    value = row[10] is DBNull ? "" : row[10].ToString()
                                },
                                new attribute
                                {
                                    name = "Date_of_Birth",
                                    type = "DateTime",
                                    value = row[11] is DBNull ? "" : ((DateTime)row[11]).ToString("yyyy-MM-dd"),
                                },
                                new attribute
                                {
                                    name = "Sex",
                                    type = "Enum",
                                    value = row[12] is DBNull ? "" : row[12].ToString(),
                                    enumDef = new Guid("{C780CE23-AC09-4CC3-8147-7779F6D80B65}"),
                                    enumValueText = row[2] is DBNull ? "" : context.Enums.GetValue((Guid)row[12]).Value,
                                },
                            }
                        }
                    },
                    new attribute
                    {
                        type = "Text",
                        name = "State",
                        value = "Утвержден (подписан)"
                    },
                    new attribute
                    {
                        name = "Region",
                        type = "Text",
                        value = row[13] is DBNull ? "" : row[13].ToString()
                    },
                    new attribute
                    {
                        name = "District",
                        type = "Text",
                        value = row[14] is DBNull ? "" : row[14].ToString()
                    },
                    new attribute
                    {
                        name = "subDistrict",
                        type = "Text",
                        value = row[15] is DBNull ? "" : row[15].ToString()
                    },
                    new attribute
                    {
                        name = "Village",
                        type = "Text",
                        value = row[16] is DBNull ? "" : row[16].ToString()
                    },
                    new attribute
                    {
                        name = "ResidentialAddress",
                        type = "Text",
                        value = row[17] is DBNull ? "" : row[17].ToString()
                    },
                    new attribute
                    {
                        name = "Phone",
                        type = "Text",
                        value = row[18] is DBNull ? "" : row[18].ToString()
                    },
                };
                var doc = new document
                {
                    id = id,
                    attributes = attributes.ToArray()
                };
                documents.Add(doc);
            }
            return documents.ToArray();
        }
        public static Guid[] FetchApprovedDocumentsLessThanDate(Enum approveState, DateTime date, int page = 1, int size = 10)
        {
            var context = CreateContext(adminUserId);

            var filterDate = DateTime.Today.AddYears(-18);
            var qb = new QueryBuilder(approveState.GetDefId());
            qb.Where("&State").Eq(approveState.GetValueId()[0]).And("ExamDateTo").Lt(date);

            var query = context.CreateSqlQuery(qb);
            var personSrc = query.JoinSource(query.Source, personDefId, SqlSourceJoinType.Inner, "Person");

            if (approveState is CustomExportChildState)
                query.AndCondition(personSrc, "Date_of_Birth", ConditionOperation.GreatThen, filterDate);

            query.AddAttribute(query.Source, "&Id");

            /*query.TopNo = size;
            query.SkipNo = (page - 1) * size;*/
            var docList = new List<Guid>();
            using (var reader = context.CreateSqlReader(query))
            {
                while (reader.Read())
                {
                    docList.Add(reader.GetGuid(0));
                }
            }
            return docList.ToArray();
        }
        private static readonly Guid adminUserId = new Guid("{4C55D519-8576-4EED-82B5-A1F120BFA1CB}");
        public static int CountDocumentsByPersonIdAndInStates(Guid docDefId, Guid personId, object[] inStates)
        {
            var context = CreateContext(adminUserId);
            var qb = new QueryBuilder(docDefId);
            qb.Where("&State").In(inStates).And("Person").Eq(personId);
            var query = context.CreateSqlQuery(qb);
            query.AddAttribute("&Id", SqlQuerySummaryFunction.Count);
            using(var reader = context.CreateSqlReader(query))
            {
                if (reader.Read()) return reader.GetInt32(0);
            }
            return 0;
        }

        public static WorkflowContext CreateContext(string username, Guid? userId)
        {
            if(userId == null)
            {
                userId = new Guid("{DCED7BEA-8A93-4BAF-964B-232E75A758C5}");
            }
            return new WorkflowContext(new WorkflowContextData(Guid.Empty, userId.Value), InitProvider(username, userId.Value));
        }

        public static IEnumerable<attribute> GetDocAttributesByDefId(Guid defId, Guid userId)
        {
            var userObj = DAL.GetCissaUser(userId);
            var context = CreateContext(userObj.UserName, userObj.Id);

            var docDefRepo = context.DocDefs;
            var docDef = docDefRepo.Find(defId);
            if (docDef != null)
                return docDef.Attributes.Select(x => new attribute
                {
                    id = x.Id,
                    name = x.Name,
                    caption = x.Caption,
                    type = x.Type.Name,
                    enumDef = x.EnumDefType != null ? x.EnumDefType.Id : Guid.Empty
                });
            throw new ApplicationException("Not found DefId: " + defId);
        }

        public static IEnumerable<document> GetDocumentsByDefId(Guid docDefId, int page, int size, Guid userId, Guid? docId = null)
        {
            var userObj = DAL.GetCissaUser(userId);
            if (userObj == null) throw new ApplicationException("Пользователь не найден!");
            var context = CreateContext(userObj.UserName, userObj.Id);

            var docDefRepo = context.DocDefs;
            var docDef = docDefRepo.Find(docDefId);

            var qb = new QueryBuilder(docDefId);

            if(docId != null)
            {
                qb.Where("&Id").Eq(docId);
            }

            var query = SqlQueryBuilder.Build(context.DataContext, qb.Def);

            query.AddAttribute("&Id");
            foreach (var attr in docDef.Attributes)
            {
                System.Diagnostics.Trace.WriteLine(attr.Name+ "\t"+attr.Caption + "\t" + attr.Type.Name);
                query.AddAttribute(attr.Name);
            }

            query.TopNo = size;
            query.SkipNo = (page - 1) * size;

            var documents = new List<document>();

            using (var reader = new SqlQueryReader(context.DataContext, query))
            {
                while (reader.Read())
                {
                    var doc = new document() {
                        id = reader.GetGuid(0)
                    };

                    var attributes = new List<attribute>();
                    for (int i = 1; i < reader.Fields.Count; i++)
                    {
                        var docAttr = new attribute()
                        {
                            id = reader.Fields[i].AttributeId,
                            name = reader.Fields[i].AttributeName,
                            type = reader.Fields[i].AttrDef.Type.Name,
                            caption = reader.Fields[i].AttrDef.Caption
                        };

                        if (docAttr.type == "Enum")
                        {
                            docAttr.enumDef = reader.Fields[i].AttrDef.EnumDefType.Id;
                        }

                        if (!reader.IsDbNull(i))
                        {
                            System.Diagnostics.Trace.Write(reader.GetValue(i) + "\t");
                            docAttr.value = reader.GetValue(i).ToString();

                            if (docAttr.type == "Enum")
                            {
                                var enumValueId = reader.GetGuid(i);
                                docAttr.enumValueText = context.Enums.GetValue(enumValueId).Value;
                            }
                        }
                        else
                        {
                            System.Diagnostics.Trace.Write("null\t");
                            docAttr.value = "";
                        }

                        attributes.Add(docAttr);

                    }

                    System.Diagnostics.Trace.WriteLine("");
                    doc.attributes = attributes.ToArray();
                    documents.Add(doc);
                    
                }
            }

            return documents;


        }

        internal static Guid GetDefId(Guid id, Guid userId)
        {
            var userObj = DAL.GetCissaUser(userId);
            var context = CreateContext(userObj.UserName, userObj.Id);
            return context.Documents.LoadById(id).DocDef.Id;
        }

        public static void SetState(Guid docId, Guid stateTypeId, Guid userId)
        {
            var userObj = DAL.GetCissaUser(userId);
            var context = CreateContext(userObj.UserName, userObj.Id);
            context.Documents.SetDocState(docId, stateTypeId);
        }

        public static int CountDocumentsByDefId(Guid docDefId, Guid userId)
        {
            var userObj = DAL.GetCissaUser(userId);
            var context = CreateContext(userObj.UserName, userObj.Id);

            var docDefRepo = context.DocDefs;
            var docDef = docDefRepo.Find(docDefId);

            var qb = new QueryBuilder(docDefId);
            qb.Where("&State").IsNotNull();
            var query = SqlQueryBuilder.Build(context.DataContext, qb.Def);

            query.AddAttribute("&Id", SqlQuerySummaryFunction.Count);
            using (var reader = new SqlQueryReader(context.DataContext, query))
            {
                if (reader.Read())
                {
                    return reader.GetInt32(0);
                }
            }

            return 0;
        }
        static void InitQueryConditions(WorkflowContext context, SqlQuery query, document filterDocument, Guid defId, SqlQuerySource querySource)
        {
            if (filterDocument != null && filterDocument.attributes != null && filterDocument.attributes.Length > 0)
            {
                filterDocument.attributes = filterDocument.attributes.Where(x => !string.IsNullOrEmpty(x.value) || x.type == "Doc").ToArray();
                var cissaDocument = CreateDocumentInstance(context, defId, filterDocument);
                foreach (var attr in filterDocument.attributes)
                {
                    if (attr.type != "Doc" && cissaDocument[attr.name] != null)
                        query.AddCondition(ExpressionOperation.And, defId, attr.name, ConditionOperation.Equal, cissaDocument[attr.name]);
                    if (attr.type == "Doc" && cissaDocument[attr.name] != null)
                        query.AddCondition(ExpressionOperation.And, defId, attr.name, ConditionOperation.Equal, cissaDocument[attr.name]);
                    if (attr.type == "Doc" && attr.subDocument != null && attr.subDocument.attributes != null && attr.subDocument.attributes.Length > 0)
                    {
                        var newQuerySource = query.JoinSource(querySource, attr.docDef, SqlSourceJoinType.Inner, attr.name);
                        InitQueryConditions(context, query, attr.subDocument, attr.docDef, newQuerySource);
                    }
                }
            }
        }
        public static IEnumerable<document> FilterDocumentsByDefId(document filterDocument, Guid docDefId, int page, int size, Guid userId)
        {
            var userObj = DAL.GetCissaUser(userId);
            var context = CreateContext(userObj.UserName, userObj.Id);

            var docDefRepo = context.DocDefs;
            //var docDef = docDefRepo.Find(docDefId);

            var qb = new QueryBuilder(docDefId);
            //qb.Where("&State").IsNotNull();
            var query = SqlQueryBuilder.Build(context.DataContext, qb.Def);

            calcAndInjectRangeConditionsFor(ref filterDocument, query, "StatusDate");

            InitQueryConditions(context, query, filterDocument, docDefId, query.Source);


            query.AddAttribute("&Id");
            
            query.TopNo = size;
            query.SkipNo = (page - 1) * size;

            var documents = new List<document>();
            var table = new System.Data.DataTable();
            using (var reader = new SqlQueryReader(context.DataContext, query))
            {
                reader.Open();
                reader.Fill(table);
                reader.Close();
            }
            foreach(System.Data.DataRow row in table.Rows)
            {
                var id = (Guid)row[0];
                var doc = new document
                {
                    id = id
                };
                var docRepo = context.Documents;
                var d = docRepo.LoadById(id);
                var attributes = new List<attribute>();
                foreach (var attr in d.Attributes)
                {
                    try
                    {
                        var docAttr = new attribute()
                        {
                            id = attr.AttrDef.Id,
                            name = attr.AttrDef.Name,
                            type = attr.AttrDef.Type.Name,
                            caption = attr.AttrDef.Caption
                        };
                        if (docAttr.type == "BLOB")
                        {
                            var blobVal = docRepo.GetBlobAttrData(doc.id, docAttr.id);
                            docAttr.value = blobVal != null && blobVal.Data != null ? Encoding.Default.GetString(blobVal.Data) : "";
                        }
                        if (docAttr.type == "Enum")
                        {
                            if (attr.AttrDef.EnumDefType != null)
                                docAttr.enumDef = attr.AttrDef.EnumDefType.Id;
                            else
                                throw new NullReferenceException("Справочник для поля типа Enum не привязан! Поле: " + attr.AttrDef.Name);
                        }
                        if (attr.ObjectValue != null)
                        {
                            docAttr.value = attr.ObjectValue.ToString();
                            if (docAttr.type == "Enum")
                            {
                                var enumValueId = (Guid)attr.ObjectValue;
                                var enumValObj = context.Enums.GetValue(enumValueId);
                                if(enumValObj != null)
                                docAttr.enumValueText = enumValObj.Value;
                            }
                            if (docAttr.type == "Doc")
                            {
                                var docValueId = (Guid)attr.ObjectValue;
                                docAttr.subDocument = GetDocumentById(docValueId, userId, context);//context.Enums.GetValue(enumValueId).Value;
                            }
                        }
                        attributes.Add(docAttr);
                    }
                    catch
                    {
                        throw;
                    }
                }
                var state = context.Documents.GetDocState(id);
                attributes.Add(new attribute
                {
                    type = "State",
                    name = "State",
                    caption = "Статус",
                    value = state != null ? state.Type.Name : "-"
                });
                doc.attributes = attributes.ToArray();
                documents.Add(doc);
            }

            return documents;


        }


        private static document removeAttributeFromDocument(string attrName, document documentObj)
        {
            var newAttributes = new List<attribute>();
            foreach (var attr in documentObj.attributes)
            {
                if (attr.name != attrName) newAttributes.Add(attr);
            }
            return new document { id = documentObj.id, attributes = newAttributes.ToArray() };
        }
        const string RANGE_DATE_FORMAT = "dd.MM.yyyy";
        private static void calcAndInjectRangeConditionsFor(ref document filterDocument, SqlQuery query, string attrName = "StatusDate")
        {
            if(filterDocument.attributes != null)
            {
                var fromAttrName = $"{attrName}From";
                var toAttrName = $"{attrName}To";
                var dateFromAttr = filterDocument.attributes.FirstOrDefault(x => x.name == fromAttrName);
                if(dateFromAttr != null)
                {
                    if (!string.IsNullOrEmpty(dateFromAttr.value))
                    {
                        if (DateTime.TryParseExact(dateFromAttr.value, RANGE_DATE_FORMAT, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dateFrom))
                        {
                            query.AndCondition(attrName, ConditionOperation.GreatEqual, dateFrom);
                        }
                        else
                            throw new FormatException($"Формат переданной даты не соответствует требуемому формату - {RANGE_DATE_FORMAT}. Полученное значение: {fromAttrName}={dateFromAttr.value}");
                    }
                    else
                        throw new ArgumentNullException(fromAttrName, $"Поле {fromAttrName} для диапазонного фильтра передано с пустым значением");
                    filterDocument = removeAttributeFromDocument(fromAttrName, filterDocument);
                }

                var dateToAttr = filterDocument.attributes.FirstOrDefault(x => x.name == toAttrName);
                if (dateToAttr != null)
                {
                    if (!string.IsNullOrEmpty(dateToAttr.value))
                    {
                        if (DateTime.TryParseExact(dateToAttr.value, RANGE_DATE_FORMAT, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dateTo))
                        {
                            query.AndCondition(attrName, ConditionOperation.LessEqual, dateTo);
                        }
                        else
                            throw new FormatException($"Формат переданной даты не соответствует требуемому формату - {RANGE_DATE_FORMAT}. Полученное значение: {toAttrName}={dateToAttr.value}");
                    }
                    else
                        throw new ArgumentNullException(toAttrName, $"Поле {toAttrName} для диапазонного фильтра передано с пустым значением");
                    filterDocument = removeAttributeFromDocument(toAttrName, filterDocument);
                }
            }
        }
        public static IEnumerable<document> FilterDocumentsByDefIdState(document filterDocument, Guid docDefId, int page, int size, Guid userId, Guid stateTypeId)
        {
            var userObj = DAL.GetCissaUser(userId);
            var context = CreateContext(userObj.UserName, userObj.Id);

            var docDefRepo = context.DocDefs;
            var docDef = docDefRepo.Find(docDefId);

            var qb = new QueryBuilder(docDefId);
            var query = context.CreateSqlQuery(qb.Def);

            calcAndInjectRangeConditionsFor(ref filterDocument, query, "StatusDate");

            InitQueryConditions(context, query, filterDocument, docDefId, query.Source);

            query.AddCondition(ExpressionOperation.And, docDef, "&State", ConditionOperation.Equal, stateTypeId);

            query.AddAttribute("&Id");

            query.TopNo = size;
            query.SkipNo = (page - 1) * size;

            var documents = new List<document>();
            var table = new System.Data.DataTable();
            using (var reader = context.CreateSqlReader(query))
            {
                reader.Open();
                reader.Fill(table);
                reader.Close();
            }
            foreach (System.Data.DataRow row in table.Rows)
            {
                var id = (Guid)row[0];
                var doc = new document
                {
                    id = id
                };
                var d = context.Documents.LoadById(id);
                var attributes = new List<attribute>();
                foreach (var attr in d.Attributes)
                {
                    var docAttr = new attribute()
                    {
                        id = attr.AttrDef.Id,
                        name = attr.AttrDef.Name,
                        type = attr.AttrDef.Type.Name,
                        caption = attr.AttrDef.Caption
                    };
                    if (docAttr.type == "Enum")
                    {
                        docAttr.enumDef = attr.AttrDef.EnumDefType.Id;
                    }
                    if (attr.ObjectValue != null)
                    {
                        docAttr.value = attr.ObjectValue.ToString();
                        if (docAttr.type == "Enum")
                        {
                            var enumValueId = (Guid)attr.ObjectValue;
                            docAttr.enumValueText = context.Enums.GetValue(enumValueId).Value;
                        }
                        if (docAttr.type == "Doc")
                        {
                            var docValueId = (Guid)attr.ObjectValue;
                            docAttr.subDocument = GetDocumentById(docValueId, userId, context);//context.Enums.GetValue(enumValueId).Value;
                        }
                    }
                    attributes.Add(docAttr);
                }
                var state = context.Documents.GetDocState(id);
                attributes.Add(new attribute
                {
                    type = "State",
                    name = "State",
                    caption = "Статус",
                    value = state != null ? state.Type.Name : "-"
                });
                doc.attributes = attributes.ToArray();
                documents.Add(doc);
            }

            return documents;


        }
        public static int CountFilteredDocumentsByDefId(document filterDocument, Guid docDefId, Guid userId)
        {
            var userObj = DAL.GetCissaUser(userId);
            var context = CreateContext(userObj.UserName, userObj.Id);

            var docDefRepo = context.DocDefs;
            var docDef = docDefRepo.Find(docDefId);

            var qb = new QueryBuilder(docDefId);

            var query = SqlQueryBuilder.Build(context.DataContext, qb.Def);

            if (filterDocument != null && filterDocument.attributes != null && filterDocument.attributes.Length > 0)
            {
                filterDocument.attributes = filterDocument.attributes.Where(x => !string.IsNullOrEmpty(x.value)).ToArray();
                var cissaDocument = CreateDocumentInstance(context, docDefId, filterDocument);
                foreach (var attr in filterDocument.attributes)
                {
                    query.AddCondition(ExpressionOperation.And, docDef, attr.name, ConditionOperation.Equal, cissaDocument[attr.name]);
                }
            }

            query.AddAttribute("&Id", SqlQuerySummaryFunction.Count);
            using (var reader = new SqlQueryReader(context.DataContext, query))
            {
                if (reader.Read())
                {
                    return reader.GetInt32(0);
                }
            }

            return 0;
        }

        private static bool IsHidden(Guid docDefId, Guid docId, Guid userId, WorkflowContext context = null)
        {
            var userObj = DAL.GetCissaUser(userId);
            if (context == null)
                context = CreateContext(userObj.UserName, userObj.Id);

            var docDefRepo = context.DocDefs;
            var docDef = docDefRepo.Find(docDefId);

            var qb = new QueryBuilder(docDefId);

            if (docId != null)
            {
                qb.Where("&Id").Eq(docId);
            }

            var query = SqlQueryBuilder.Build(context.DataContext, qb.Def);

            query.AddAttribute("&Id", SqlQuerySummaryFunction.Count);
            using (var reader = new SqlQueryReader(context.DataContext, query))
            {
                if (reader.Read())
                {
                    return reader.GetInt32(0) == 0;
                }
            }
            return true;
        }

        public static document GetDocumentById(Guid docId, Guid userId, WorkflowContext context = null)
        {
            var userObj = DAL.GetCissaUser(userId);
            if (context == null)
                context = CreateContext(userObj.UserName, userObj.Id);

            var docRepo = context.Documents;
            var docDefRepo = context.DocDefs;

            var d = docRepo.LoadById(docId);

            if(IsHidden(d.DocDef.Id, docId, userId, context))
            {
                throw new ApplicationException("Данный документ был удален пользователем!");
            }

            var doc = new document()
            {
                id = docId
            };

            
            var attributes = new List<attribute>();
            foreach (var attr in d.Attributes)
            {
                var docAttr = new attribute()
                {
                    id = attr.AttrDef.Id,
                    name = attr.AttrDef.Name,
                    type = attr.AttrDef.Type.Name,
                    caption = attr.AttrDef.Caption
                };
                if (docAttr.type == "BLOB")
                {
                    var blobVal = docRepo.GetBlobAttrData(doc.id, docAttr.id);
                    if (blobVal != null)
                        docAttr.value = Encoding.UTF8.GetString(blobVal.Data);
                }
                if (docAttr.type == "Enum")
                {
                    if (attr.AttrDef.EnumDefType == null) throw new ApplicationException("Справочник для поля не указан: " + attr.AttrDef.Name);
                    docAttr.enumDef = attr.AttrDef.EnumDefType.Id;
                }
                if (docAttr.type == "Doc")
                {
                    if (attr.AttrDef.DocDefType == null) throw new ApplicationException("DefId документа для поля не указан: " + attr.AttrDef.Name);
                    docAttr.docDef = attr.AttrDef.DocDefType.Id;
                }
                if (attr.ObjectValue != null)
                {
                    docAttr.value = attr.ObjectValue.ToString();
                    if (docAttr.type == "Enum")
                    {
                        var enumValueId = (Guid)attr.ObjectValue;
                        docAttr.enumValueText = context.Enums.GetValue(enumValueId).Value;
                    }

                    if (docAttr.type == "Doc")
                    {
                        var docValueId = (Guid)attr.ObjectValue;
                        docAttr.subDocument = GetDocumentById(docValueId, userId, context);//context.Documents.LoadById(docValueId);
                    }
                }
                attributes.Add(docAttr);
            }
            var state = context.Documents.GetDocState(docId);
            attributes.Add(new attribute
            {
                type = "State",
                name = "State",
                caption = "Статус",
                value = state != null ? state.Type.Name : "-"
            });
            doc.attributes = attributes.ToArray();
            return doc;
        }

        public static List<DAL.MenuItem> GetMenuItems()
        {
            return DAL.GetMenuItems();
        }

        public static IList<EnumValue> GetEnumItems(Guid enumDefId, Guid userId)
        {
            var userObj = DAL.GetCissaUser(userId);
            var context = CreateContext(userObj.UserName, userObj.Id);
            return context.Enums.GetEnumItems(enumDefId);
        }

        public static Doc CreateDocumentInstance(WorkflowContext context, Guid defId, document document)
        {
            var docRepo = context.Documents;

            var doc = docRepo.New(defId);
            foreach (var attr in document.attributes)
            {
                object attrValue = null;
                if (new[] { "Enum", "Doc" }.Contains(attr.type))
                {
                    if (!string.IsNullOrEmpty(attr.value) && Guid.TryParse(attr.value, out Guid parsedValue))
                    {
                        attrValue = parsedValue;
                    }
                }
                else if (attr.type == "Text")
                {
                    attrValue = attr.value;
                }
                else if (attr.type == "DateTime")
                {
                    if (!string.IsNullOrEmpty(attr.value) && DateTime.TryParse(attr.value, out DateTime parsedValue))
                    {
                        attrValue = parsedValue;
                    }
                }
                else if (attr.type == "Int")
                {
                    if (!string.IsNullOrEmpty(attr.value) && Int32.TryParse(attr.value, out Int32 parsedValue))
                    {
                        attrValue = parsedValue;
                    }
                }
                else if (attr.type == "Currency")
                {
                    if (!string.IsNullOrEmpty(attr.value) && Decimal.TryParse(attr.value, out Decimal parsedValue))
                    {
                        attrValue = parsedValue;
                    }
                }
                else if (attr.type == "Bool")
                {
                    if (!string.IsNullOrEmpty(attr.value) && Boolean.TryParse(attr.value, out Boolean parsedValue))
                    {
                        attrValue = parsedValue;
                    }
                }
                else if (attr.type == "Float")
                {
                    if (!string.IsNullOrEmpty(attr.value) && Double.TryParse(attr.value, out Double parsedValue))
                    {
                        attrValue = parsedValue;
                    }
                }
                else if (attr.type == "BLOB")
                {
                    if (!string.IsNullOrEmpty(attr.value))
                    {
                        //attrValue = Encoding.Default.GetBytes(attr.value);
                        //var blobVal = Encoding.Default.GetBytes(attr.value);
                        //docRepo.SaveBlobAttrData(doc, attr.id, blobVal, "long_text.txt");
                    }
                }
                else
                    throw new ApplicationException("Не могу создать экземпляр документ, тип атрибута не распознан. attr.Type: " + attr.type);
                if (attrValue != null)
                    doc[attr.name] = attrValue;
            }
            return doc;
        }

        public static Guid CreateDocument(Guid defId, document document, Guid userId, bool withNo = false, string noAttrName = "")
        {
            var userObj = DAL.GetCissaUser(userId);
            var context = CreateContext(userObj.UserName, userObj.Id);

            var docRepo = context.Documents;

            var doc = CreateDocumentInstance(context, defId, document);
            if (withNo)
            {
                var no = GeneratorRepository.GetNewId(doc.OrganizationId.Value, defId).ToString();
                while (no.Length < 9) no = "0" + no;
                doc[noAttrName] = "N" + no;
            }
            docRepo.Save(doc);
            var textBlobAttrs = document.attributes.Where(x => x.type == "BLOB" && !x.name.EndsWith("IMG"));
            if(textBlobAttrs != null && textBlobAttrs.Count() > 0)
            {
                foreach (var blobAttr in textBlobAttrs)
                {
                    var blobVal = Encoding.UTF8.GetBytes(blobAttr.value);
                    var attrDef = doc.DocDef.Attributes.FirstOrDefault(x => x.Name.ToUpper() == blobAttr.name.ToUpper());
                    if (attrDef != null)
                    {
                        if (attrDef.Type.Name == "BLOB")
                        {
                            docRepo.SaveBlobAttrData(doc.Id, attrDef.Id, blobVal, "long_text.txt");
                        }
                        else
                            throw new ApplicationException("Не могу сохранить документ, тип атрибута из формы не соответствует документу. attr.name: " + blobAttr.name);
                    }
                    else
                        throw new ApplicationException("Не могу сохранить документ, атрибут из формы не найден в документе. attr.name: " + blobAttr.name);
                }
            }
            var imgBlobAttrs = document.attributes.Where(x => x.type == "BLOB" && x.name.EndsWith("IMG"));
            if (imgBlobAttrs != null && imgBlobAttrs.Count() > 0)
            {
                foreach (var blobAttr in imgBlobAttrs)
                {
                    var blobVal = Encoding.UTF8.GetBytes(blobAttr.value);
                    docRepo.SaveBlobAttrData(doc.Id, blobAttr.id, blobVal, "image.png");
                }
            }
            return doc.Id;
        }
        public static void UpdateDocument(Guid docId, document document, Guid userId)
        {
            var userObj = DAL.GetCissaUser(userId);
            var context = CreateContext(userObj.UserName, userObj.Id);

            var docRepo = context.Documents;

            var doc = docRepo.LoadById(docId);
            foreach (var attr in document.attributes)
            {
                object attrValue = null;

                if (new[] { "Enum", "Doc" }.Contains(attr.type))
                {
                    if (!string.IsNullOrEmpty(attr.value) && Guid.TryParse(attr.value, out Guid parsedValue))
                    {
                        attrValue = parsedValue;
                    }
                }
                else if (attr.type == "Text")
                {
                    attrValue = attr.value;
                }
                else if (attr.type == "DateTime")
                {
                    if (!string.IsNullOrEmpty(attr.value) && DateTime.TryParse(attr.value, out DateTime parsedValue))
                    {
                        attrValue = parsedValue;
                    }
                }
                else if (attr.type == "Int")
                {
                    if (!string.IsNullOrEmpty(attr.value) && Int32.TryParse(attr.value, out Int32 parsedValue))
                    {
                        attrValue = parsedValue;
                    }
                }
                else if (attr.type == "Currency")
                {
                    if (!string.IsNullOrEmpty(attr.value) && Decimal.TryParse(attr.value, out Decimal parsedValue))
                    {
                        attrValue = parsedValue;
                    }
                }
                else if (attr.type == "Bool")
                {
                    if (!string.IsNullOrEmpty(attr.value) && Boolean.TryParse(attr.value, out Boolean parsedValue))
                    {
                        attrValue = parsedValue;
                    }
                }
                else if (attr.type == "Float")
                {
                    if (!string.IsNullOrEmpty(attr.value) && Double.TryParse(attr.value, out Double parsedValue))
                    {
                        attrValue = parsedValue;
                    }
                }
                else if(attr.type == "BLOB")
                {
                    var blobVal = Encoding.UTF8.GetBytes(attr.value);
                    var attrDef = doc.DocDef.Attributes.FirstOrDefault(x => x.Name.ToUpper() == attr.name.ToUpper());
                    if(attrDef != null)
                    {
                        if(attrDef.Type.Name == "BLOB")
                        {
                            docRepo.SaveBlobAttrData(doc.Id, attrDef.Id, blobVal, "long_text.txt");
                        }
                        else
                            throw new ApplicationException("Не могу сохранить документ, тип атрибута из формы не соответствует документу. attr.name: " + attr.name);
                    }
                    else
                        throw new ApplicationException("Не могу сохранить документ, атрибут из формы не найден в документе. attr.name: " + attr.name);
                }
                else
                    throw new ApplicationException("Не могу сохранить документ, тип атрибута не распознан. attr.Type: " + attr.type);
                //if (attrValue != null)
                    doc[attr.name] = attrValue;
            }
            docRepo.Save(doc);
        }

        public static void DeleteDocument(Guid docId, Guid userId)
        {
            var userObj = DAL.GetCissaUser(userId);
            var context = CreateContext(userObj.UserName, userObj.Id);
            context.Documents.HideById(docId);
        }

        #region Address

        static Guid regionDefId = new Guid("{99D6F2C3-C138-4BDD-BD5B-BCAE0EF11AC6}");
        static Guid districtDefId = new Guid("{A3FCA356-82A9-4BBD-872A-8333BEC6E41A}");
        static Guid cityDefId = new Guid("{4BB6D32D-5181-4031-BA49-CF5910D6D883}");
        static Guid settlementDefId = new Guid("{80CC229E-05FC-45A5-B2ED-F101465ADD1E}");
        static Guid villageDefId = new Guid("{BD0E1850-FFE9-4EBA-B86E-98069DF7B885}");
        public static IEnumerable<region> GetRegions(Guid userId)
        {
            var userObj = DAL.GetCissaUser(userId);
            if (userObj == null) throw new ApplicationException("Пользователь не найден!");
            var context = CreateContext(userObj.UserName, userObj.Id);
            var qb = new QueryBuilder(regionDefId);
            var query = context.CreateSqlQuery(qb.Def);
            query.AddAttributes(new[] { "&Id", "Name" });
            using (var reader = context.CreateSqlReader(query))
            {
                while (reader.Read())
                    yield return new region { id = reader.GetGuid(0), name = reader.IsDbNull(1) ? "" : reader.GetString(1) };
            }
        }

        public static IEnumerable<district> GetDistricts(Guid userId, Guid? regionId = null)
        {
            var userObj = DAL.GetCissaUser(userId);
            var context = CreateContext(userObj.UserName, userObj.Id);
            var qb = new QueryBuilder(districtDefId);
            if (regionId != null)
                qb.Where("Area").Eq(regionId);
            var query = context.CreateSqlQuery(qb.Def);
            query.AddAttributes(new[] { "&Id", "Name", "DistrictType" });
            if (regionId == null)
                query.AddAttribute("Area");
            using (var reader = context.CreateSqlReader(query))
            {
                while (reader.Read())
                    yield return new district
                    {
                        id = reader.GetGuid(0),
                        name = reader.IsDbNull(1) ? "" : reader.GetString(1),
                        districtType = reader.IsDbNull(2) ? null : (Guid?)reader.GetGuid(2),
                        regionId = regionId.HasValue ? regionId : reader.GetGuid(3)
                    };
            }
        }

        public static IEnumerable<city> GetCities(Guid userId, Guid? districtId = null)
        {
            var userObj = DAL.GetCissaUser(userId);
            var context = CreateContext(userObj.UserName, userObj.Id);
            var qb = new QueryBuilder(cityDefId);
            if (districtId != null)
                qb.Where("District").Eq(districtId);
            var query = context.CreateSqlQuery(qb.Def);
            query.AddAttributes(new[] { "&Id", "Name", "DistrictType" });
            if (districtId == null)
                query.AddAttribute("District");
            using (var reader = context.CreateSqlReader(query))
            {
                while (reader.Read())
                    yield return new city
                    {
                        id = reader.GetGuid(0),
                        name = reader.IsDbNull(1) ? "" : reader.GetString(1),
                        districtType = reader.IsDbNull(2) ? null : (Guid?)reader.GetGuid(2),
                        districtId = districtId.HasValue ? districtId : reader.GetGuid(3)
                    };
            }
        }

        public static IEnumerable<settlement> GetSettlements(Guid userId, Guid? districtId = null)
        {
            var userObj = DAL.GetCissaUser(userId);
            var context = CreateContext(userObj.UserName, userObj.Id);
            var qb = new QueryBuilder(settlementDefId);
            if (districtId != null)
                qb.Where("District").Eq(districtId);
            var query = context.CreateSqlQuery(qb.Def);
            query.AddAttributes(new[] { "&Id", "Name", "DistrictType" });
            if (districtId == null)
                query.AddAttribute("District");
            using (var reader = context.CreateSqlReader(query))
            {
                while (reader.Read())
                    yield return new settlement
                    {
                        id = reader.GetGuid(0),
                        name = reader.IsDbNull(1) ? "" : reader.GetString(1),
                        districtType = reader.IsDbNull(2) ? null : (Guid?)reader.GetGuid(2),
                        districtId = districtId.HasValue ? districtId : reader.GetGuid(3)
                    };
            }
        }

        public static IEnumerable<village> GetVillages(Guid userId, Guid? settlementId = null)
        {
            var userObj = DAL.GetCissaUser(userId);
            var context = CreateContext(userObj.UserName, userObj.Id);
            var qb = new QueryBuilder(villageDefId);
            if (settlementId != null)
                qb.Where("Settlement").Eq(settlementId);
            var query = context.CreateSqlQuery(qb.Def);
            query.AddAttributes(new[] { "&Id", "Name", "Coefficient" });
            if (settlementId == null)
                query.AddAttribute("Settlement");
            using (var reader = context.CreateSqlReader(query))
            {
                while (reader.Read())
                    yield return new village
                    {
                        id = reader.GetGuid(0),
                        name = reader.IsDbNull(1) ? "" : reader.GetString(1),
                        coefficient = reader.IsDbNull(2) ? null : (double?)reader.GetDouble(2),
                        settlementId = settlementId.HasValue ? settlementId : reader.GetGuid(3)
                    };
            }
        }

        #endregion

        private static Guid grownDefId = new Guid("{B4DDDC00-9EA9-4AD4-9C4F-498E87AA9828}");
        private static Guid childDefId = new Guid("{5FDE415F-DB00-43B4-BA6E-FE439CFF6DA0}");
        public static object Spravka(string pin)
        {
            var context = CreateContext("msec_system_user", new Guid("{4C55D519-8576-4EED-82B5-A1F120BFA1CB}"));

            return null;
        }

        public static Guid[] GetSignedChildMedActsByPersonId(Guid personId, Guid userId)
        {
            var list = new List<Guid>();
            var context = CreateContext(userId);
            var qb = new QueryBuilder(CustomExportChildState.APPROVED_AND_REGISTERED.GetDefId());
            qb.Where("Person").Eq(personId).And("&State").Eq(CustomExportChildState.APPROVED_AND_REGISTERED.GetValueId()[0]);
            var query = context.CreateSqlQuery(qb);
            query.AddAttribute("&Id");
            using (var reader = context.CreateSqlReader(query))
            {
                while (reader.Read())
                    list.Add(reader.GetGuid(0));
            }
            return list.ToArray();
        }


    }

}
