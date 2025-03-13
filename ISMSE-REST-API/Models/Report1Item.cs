using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ISMSE_REST_API.Models
{
    public class Report1Item
    {
        public string RowType { get; set; }
        public string MonthReportExaminationAdults { get; set; }
        public int RowNo { get; set; }
        public string RowICD { get; set; }
        public int Gr1 { get; set; }
        public int Total { get; set; }
        public int WomenAll { get; set; }
        public int Total1829 { get; set; }
        public int Women1829 { get; set; }
        public int Total3040 { get; set; }
        public int Women3040 { get; set; }
        public int Total45 { get; set; }
        public int Women45 { get; set; }
        public int TotalPersonsAge { get; set; }
        public int WomenPersonsAge { get; set; }
        public int FirstGroupAll { get; set; }
        public int WomenFirstGroup { get; set; }
        public int BusyFirstGroup { get; set; }
        public int SecondGroupAll { get; set; }
        public int WomenSecondGroup { get; set; }
        public int BusySecondGroup { get; set; }
        public int ThirdGroupAll { get; set; }
        public int WomenThirdGroup { get; set; }
        public int BusyThirdGroup { get; set; }        
    }
    //Результаты первичных освидетельствований 1 отчет
    public class Report_01Item
    {
        public string rowName { get; set; }
        public int rowNo { get; set; }
        public int gr1 { get; set; }
        public int gr2 { get; set; }
        public int gr3 { get; set; }
        public int gr4 { get; set; }
        public int gr5 { get; set; }
        public int gr6 { get; set; }
        public int gr7 { get; set; }
        public int gr8 { get; set; }
        public int gr9 { get; set; }
        public int gr10 { get; set; }
        public int gr11 { get; set; }
        public int gr12 { get; set; }
        public int gr13 { get; set; }
        public int gr14 { get; set; }
        public int gr15 { get; set; }
    }

    //Результатаы переосвид-я инв-в из числа проживающих в ГОРОДСКОЙ МЕСТНОСТИ
    public class Report5Item
    {
        public string RowType { get; set; }
        public int RowNo { get; set; }
        public int gr1 { get; set; }
        public int gr2 { get; set; }
        public int gr3 { get; set; }
        public int gr4 { get; set; }
        public int gr5 { get; set; }
        public int gr6 { get; set; }
        public int gr7 { get; set; }
        public int gr8 { get; set; }
        public int gr9 { get; set; }
        public int gr10 { get; set; }
        public int gr11 { get; set; }
        public int gr12 { get; set; }
        public int gr13 { get; set; }
        public int gr14 { get; set; }
    }

    public class Report_5
    {
        public Report5Item[] Items { get; set; }
        public int c0 { get; set; }
        public int c1 { get; set; }
        public int c3 { get; set; }
        public int c4 { get; set; }
        public int c5 { get; set; }
        public int c6 { get; set; }
        public int c7 { get; set; }
        public int c8 { get; set; }
        public int c9 { get; set; }
        public int c10 { get; set; }
        public int c11 { get; set; }
        public int c12 { get; set; }
        public int c13 { get; set; }
        public int c14 { get; set; }
        public int c15 { get; set; }
        public int c16 { get; set; }
        public int c17 { get; set; }
    }

    //Результаты повторных освидетельствований граждан (раздел. 7.1)
    public class Report7_1_Item 
    {
        public string rowName { get; set; }
        public int rowNo { get; set; }
        public int gr3 { get; set; }
        public int gr4 { get; set; }
        public int gr5 { get; set; }
        public int gr6 { get; set; }
        public int gr7 { get; set; }
        public int gr8 { get; set; }
        public int gr9 { get; set; }
    }
    //Результаты переосвидетельствования ЛОВЗ из числа ЛОВЗ от трудового увечья, имеющих группу инвалидности
    public class Report7_2Item
    {
        public string RowName { get; set; }
        public int RowNo { get; set; }
        public int gr1 { get; set; }
        public int gr2 { get; set; }
        public int gr3 { get; set; }
        public int gr4 { get; set; }
        public int gr5 { get; set; }
        public int gr6 { get; set; }
        public int gr8 { get; set; }
        public int gr10 { get; set; }
        public int gr12 { get; set; }
        public int gr13 { get; set; }
    }

    //Результаты переосвидетельствования ЛОВЗ из числа ЛОВЗ от ТРУДОВОЕ УВЕЧЬЕ, имеющих УПТ в % без ГИ
    public class Report7_3Item
    {
        public int gr1 { get; set; }
        public int gr2 { get; set; }
        public int gr3 { get; set; }
        public int gr4 { get; set; }
        public int gr5 { get; set; }
        public int gr6 { get; set; }
        public int gr7 { get; set; }
        public int gr8 { get; set; }
        public int gr9 { get; set; }
        public int gr10 { get; set; }
        public int gr11 { get; set; }
        public int gr12 { get; set; }
    }
    //Рекомендации по реабилитации инвалидов 8 отчет (используется и для РОВЗ)
    public class Report8Item
    {
        public string rowName { get; set; }
        public int rowNo { get; set; }
        public int gr1 { get; set; }
        public int gr2 { get; set; }
        public int gr3 { get; set; }
        public int gr4 { get; set; }
        public int gr5 { get; set; }
        public int gr6 { get; set; }
    }

    /*****************************************************************************/
    /*Форма 7 Д*/
    //Результаты первичных освидетельствований детей в возрасте до 18 лет
    public class Report7D1Item
    {
        public string rowName { get; set; }
        public int rowNo { get; set; }
        public int gr1 { get; set; }
        public int gr2 { get; set; }
        public int gr3 { get; set; }
        public int gr4 { get; set; }
        public int gr5 { get; set; }
        public int gr6 { get; set; }
        public int gr7 { get; set; }
        public int gr8 { get; set; }
        public int gr9 { get; set; }
        public int gr10 { get; set; }
    }
    
    //Результаты первичных освидетельствований  детей в возрасте до 18 лет (человек), проживающих в ГМ
    public class Report7D2Item
    {
        public string rowName { get; set; }
        public int rowNo { get; set; }
        public int gr1 { get; set; }
        public int gr2 { get; set; }
        public int gr3 { get; set; }
        public int gr4 { get; set; }
        public int gr5 { get; set; }
        public int gr6 { get; set; }
        public int gr7 { get; set; }
        public int gr8 { get; set; }
        public int gr9 { get; set; }
        public int gr10 { get; set; }
    }

    //Результаты переосвидетельствований детей- инвалидов в возрасте до 18 лет
    public class Report7D5Item
    {
        public string rowName { get; set; }
        public int rowNo { get; set; }
        public int gr1 { get; set; }
        public int gr2 { get; set; }
        public int gr3 { get; set; }
        public int gr4 { get; set; }
        public int gr5 { get; set; }
        public int gr6 { get; set; }
        public int gr7 { get; set; }
        public int gr8 { get; set; }
        public int gr9 { get; set; }
        public int gr10 { get; set; }
        public int gr11 { get; set; }
        public int gr12 { get; set; }
        public int gr13 { get; set; }
    }


    //Модуль БРРП    
    //Сведения о численности получателей, среднем размере и выплаченной сумме пособий по БР по состоянию
    public class ReportBR1Item
    {
        public string RegionName { get; set; }
        public string OrgName { get; set; }
        private Guid OrgId { get; set; }
        public int gr3 { get; set; }
        public int gr4 { get; set; }
        public int gr5 { get; set; }
        public int gr6 { get; set; }
        public int gr7_1 { get; set; }
        public int gr7_2 { get; set; }
        public int gr7_3 { get; set; }
        public int gr7_4 { get; set; }
        public int gr7_5 { get; set; }
        public int gr7_6 { get; set; }
        public decimal gr8 { get; set; }
        public decimal gr9 { get; set; }
        public ReportBR1Item(Guid orgId)
        {
            OrgId = orgId;
        }
        public Guid GetOrgId()
        {
            return OrgId;
        }
    }
    //Сведения о численности получателей пособия по беременности и родам по категориям получателей
    public class ReportBR2Item
    {
        public string rowName { get; set; }
        public int rowNo {get; set; }
        public int gr3 { get; set; }
        public int gr4 { get; set; }
        public int gr5 { get; set; }
        public decimal gr6 { get; set; }
        public decimal gr7 { get; set; }
        public int gr8 { get; set; }
        public decimal gr9 { get; set; }
        public decimal gr10 { get; set; }
        public int gr11 { get; set; }
        public decimal gr12 { get; set; }
        public decimal gr13 { get; set; }
        public int gr14 { get; set; }
        public decimal gr15 { get; set; }
        public decimal gr16 { get; set; }
        public int gr17 { get; set; }
        public decimal gr18 { get; set; }
        public decimal gr19 { get; set; }
    }

    //Динамика численности получателей пособия по беременности и родам
    public class ReportBR3Item
    {
        public int gr1 { get; set; }
        public int gr2 { get; set; }
        public int gr3_1 { get; set; }
        public int gr3_2 { get; set; }
        public int gr3_3 { get; set; }
        public int gr3_4 { get; set; }
        public int gr3_5 { get; set; }
        public int gr3_6 { get; set; }
        public decimal gr4 { get; set; }
        public decimal gr5 { get; set; }
    }
    //Сведения о численности получателей, среднем размере и выплаченной сумме пособий по РП по состоянию
    public class ReportRP1Item
    {
        public string RegionName { get; set; }
        public string OrgName { get; set; }
        private Guid OrgId { get; set; }
        public int gr3 { get; set; }
        public int gr4 { get; set; }
        public int gr5 { get; set; }
        public int gr6 { get; set; }
        public int gr7_1 { get; set; }
        public int gr7_2 { get; set; }
        public decimal gr8 { get; set; }
        public decimal gr9 { get; set; }
        public ReportRP1Item(Guid orgId)
        {
            OrgId = orgId;
        }
        public Guid GetOrgId()
        {
            return OrgId;
        }
    }
    public class ReportRP2Item
    {
        public string rowName { get; set; }
        public int rowNo { get; set; }
        public int gr3 { get; set; }
        public int gr4 { get; set; }
        public int gr5 { get; set; }
        public int gr6 { get; set; }
        public int gr7 { get; set; }        
        public decimal gr8 { get; set; }
        public decimal gr9 { get; set; }
    }
    public class ReportRP3Item
    {
        public int gr1 { get; set; }
        public int gr2 { get; set; }
        public int gr3_1 { get; set; }
        public int gr3_2 { get; set; }
        public decimal gr4 { get; set; }
        public decimal gr5 { get; set; }
    }

    public class ReportF8Item
    {
        public int rowNo { get; set; }
        public string rowName { get; set; }
        public int gr3 { get; set; }
        public decimal gr4 { get; set; }
        public int gr5 { get; set; }
        public decimal gr6 { get; set; }
        public int gr7 { get; set; }
        public decimal gr8 { get; set; }
        public int gr9 { get; set; }
        public decimal gr10 { get; set; }
        public int gr11 { get; set; }
        public decimal gr12 { get; set; }
        public decimal gr13 { get; set; }
    }
    public class ReportF8SVODItem
    {
        public string RegionName { get; set; }
        public string OrgName { get; set; }
        private Guid OrgId { get; set; }
        public int gr5 { get; set; }
        public decimal gr6 { get; set; }
        public int gr7 { get; set; }
        public decimal gr8 { get; set; }
        public int gr9 { get; set; }
        public decimal gr10 { get; set; }
        public int gr11 { get; set; }
        public decimal gr12 { get; set; }
        public decimal gr13 { get; set; }
        public decimal gr14 { get; set; }
        public decimal gr15 { get; set; }
        public decimal gr16 { get; set; }
        public int gr17 { get; set; }
        public decimal gr18 { get; set; }
        public int gr19 { get; set; }
        public decimal gr20 { get; set; }
        public int gr21 { get; set; }
        public decimal gr22 { get; set; }
        public int gr23 { get; set; }
        public decimal gr24 { get; set; }
        public decimal gr25 { get; set; }
        public decimal gr26 { get; set; }
        public decimal gr27 { get; set; }
        public decimal gr28 { get; set; }
        public decimal gr29 { get; set; }
        public ReportF8SVODItem(Guid orgId)
        {
            OrgId = orgId;
        }
        public Guid GetOrgId()
        {
            return OrgId;
        }
    }
}