using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using Intersoft.CISSA.DataAccessLayer.Model.Documents;
using ISMSE_REST_API.Models;
using ISMSE_REST_API.Util;

namespace ISMSE_REST_API.Controllers
{
    [System.Web.Http.Cors.EnableCors(origins: "*", headers: "*", methods: "*")]
    public class ReportsController : ApiController
    {
        //Ежемесячный отчет МСЭК (взрослый/детский)
        [HttpGet]
        [ResponseType(typeof(Report1Item[]))]
        public IHttpActionResult Report1([FromUri] DateTime fd, [FromUri] DateTime ld, Guid userId )
        {
            try
            {
                var userObj = DAL.GetCissaUsers().FirstOrDefault(x => x.Id == userId);
                if (userObj == null) throw new Exception("User not found!");
                var context = ReportExecutor.CreateContext(userObj.UserName, userObj.Id);
                var result = ReportExecutor.NumAwardAppProcess.Execute(context, fd, ld);
                return Ok(result.ToArray());
            }
            catch (Exception e)
            {
                return BadRequest(e.GetBaseException().Message);
            }
        }

        //Результаты первичных освидетельствований 1 отчет
        [HttpGet]
        [ResponseType(typeof(Report1Item[]))]
        public IHttpActionResult Report_01([FromUri] int year, Guid userId, string distrSpr)
        {
            try
            {
                var userObj = DAL.GetCissaUsers().FirstOrDefault(x => x.Id == userId);
                if (userObj == null) throw new Exception("User not found!");
                var context = ReportExecutor.CreateContext(userObj.UserName, userObj.Id);
                var result = ReportExecutor.PrimaryExamDisabilities.Execute(context, year, distrSpr); /**/
                return Ok(result.ToArray());
            }
            catch (Exception e)
            {
                return BadRequest(e.GetBaseException().Message);
            }
        }
        //Отчет о результатах первичных освидетельствований всех социальных категорий населения (ГОРОДСКАЯ МЕСТНОСТЬ)
        [HttpGet]
        [ResponseType(typeof(Report1Item[]))]
        public IHttpActionResult Report2([FromUri] int year, Guid userId, string distrSpr)
        {
            try
            {
                var userObj = DAL.GetCissaUsers().FirstOrDefault(x => x.Id == userId);
                if (userObj == null) throw new Exception("User not found!");
                var context = ReportExecutor.CreateContext(userObj.UserName, userObj.Id);
                var result = ReportExecutor.PrimaryInvalidsOfCity.Execute(context, year, distrSpr); /**/
                return Ok(result.ToArray());
            }
            catch (Exception e)
            {
                return BadRequest(e.GetBaseException().Message);
            }
        }

        //Отчет о результатах первичных освидетельствований всех социальных категорий населения (СЕЛЬСКАЯ МЕСТНОСТЬ)
        [HttpGet]
        [ResponseType(typeof(Report1Item[]))]
        public IHttpActionResult Report2_1([FromUri] int year, Guid userId, string distrSpr)
        {
            try
            {
                var userObj = DAL.GetCissaUsers().FirstOrDefault(x => x.Id == userId);
                if (userObj == null) throw new Exception("User not found!");
                var context = ReportExecutor.CreateContext(userObj.UserName, userObj.Id);
                var result = ReportExecutor.PrimaryInvalidsOfVillage.Execute(context, year, distrSpr); /**/
                return Ok(result.ToArray());
            }
            catch (Exception e)
            {
                return BadRequest(e.GetBaseException().Message);
            }
        }
        //Отчет о результатах первичных освидетельствований всех социальных категорий населения (ГОРОДСКАЯ/СЕЛЬСКАЯ МЕСТНОСТЬ)
        [HttpGet]
        [ResponseType(typeof(Report1Item[]))]
        public IHttpActionResult Report2_2([FromUri] int year, Guid userId, string distrSpr)
        {
            try
            {
                var userObj = DAL.GetCissaUsers().FirstOrDefault(x => x.Id == userId);
                if (userObj == null) throw new Exception("User not found!");
                var context = ReportExecutor.CreateContext(userObj.UserName, userObj.Id);
                var result = ReportExecutor.PrimaryInvalidsOfCityVillage.Execute(context, year, distrSpr); /**/
                return Ok(result.ToArray());
            }
            catch (Exception e)
            {
                return BadRequest(e.GetBaseException().Message);
            }
        }
        //Результатаы переосвид-я инв-в из числа проживающих в ГОРОДСКОЙ/СЕЛЬСКОЙ МЕСТНОСТИ
        [HttpGet]
        [ResponseType(typeof(Report_5))]
        public IHttpActionResult Report5([FromUri] int year, Guid userId, string distrSpr)
        {
            try
            {
                var userObj = DAL.GetCissaUsers().FirstOrDefault(x => x.Id == userId);
                if (userObj == null) throw new Exception("User not found!");
                var context = ReportExecutor.CreateContext(userObj.UserName, userObj.Id);
                var result = ReportExecutor.ReExamInvalidsOfCityVillage.Execute(context, year, distrSpr); /**/
                return Ok(result);
            }
            catch (Exception e)
            {
                return BadRequest(e.GetBaseException().Message);
            }
        }
        //Результатаы переосвид-я инв-в из числа проживающих в ГОРОДСКОЙ МЕСТНОСТИ
        [HttpGet]
        [ResponseType(typeof(Report_5))]
        public IHttpActionResult Report5_1([FromUri] int year, Guid userId, string distrSpr)
        {
            try
            {
                var userObj = DAL.GetCissaUsers().FirstOrDefault(x => x.Id == userId);
                if (userObj == null) throw new Exception("User not found!");
                var context = ReportExecutor.CreateContext(userObj.UserName, userObj.Id);
                var result = ReportExecutor.ReExamInvalidsOfCity.Execute(context, year, distrSpr); /**/
                return Ok(result);
            }
            catch (Exception e)
            {
                return BadRequest(e.GetBaseException().Message);
            }
        }
        //Результатаы переосвид-я инв-в из числа проживающих в СЕЛЬСКОЙ МЕСТНОСТИ
        [HttpGet]
        [ResponseType(typeof(Report_5))]
        public IHttpActionResult Report5_2([FromUri] int year, Guid userId, string distrSpr)
        {
            try
            {
                var userObj = DAL.GetCissaUsers().FirstOrDefault(x => x.Id == userId);
                if (userObj == null) throw new Exception("User not found!");
                var context = ReportExecutor.CreateContext(userObj.UserName, userObj.Id);
                var result = ReportExecutor.ReExamInvalidsOfVillage.Execute(context, year, distrSpr); /**/
                return Ok(result);
            }
            catch (Exception e)
            {
                return BadRequest(e.GetBaseException().Message);
            }
        }
        //Результаты повторных освидетельствований граждан (раздел. 7.1)
        [HttpGet]
        [ResponseType(typeof(Report7_1_Item[]))]
        public IHttpActionResult Report7_1([FromUri] int year, Guid userId, string distrSpr)
        {
            try
            {
                var userObj = DAL.GetCissaUsers().FirstOrDefault(x => x.Id == userId);
                if (userObj == null) throw new Exception("User not found!");
                var context = ReportExecutor.CreateContext(userObj.UserName, userObj.Id);
                var result = ReportExecutor.RepetExamInvalids.Execute(context, year, distrSpr); /**/
                return Ok(result.ToArray());
            }
            catch (Exception e)
            {
                return BadRequest(e.GetBaseException().Message);
            }
        }

        //Результаты переосвидетельствования ЛОВЗ из числа ЛОВЗ от ТРУДОВОГО УВЕЧЬЯ, имеющих группу инвалидности
        [HttpGet]
        [ResponseType(typeof(Report7_2Item))]
        public IHttpActionResult Report7_2([FromUri] int year, Guid userId, string distrSpr)
        {
            try
            {
                var userObj = DAL.GetCissaUsers().FirstOrDefault(x => x.Id == userId);
                if (userObj == null) throw new Exception("User not found!");
                var context = ReportExecutor.CreateContext(userObj.UserName, userObj.Id);
                var result = ReportExecutor.RepetExamInvalidsLaborInjury.Execute(context, year, distrSpr); /**/
                return Ok(result);
            }
            catch (Exception e)
            {
                return BadRequest(e.GetBaseException().Message);
            }
        }
        //Результаты переосвидетельствования ЛОВЗ из числа ЛОВЗ от ТРУДОВОЕ УВЕЧЬЕ, имеющих УПТ в % без ГИ
        [HttpGet]
        [ResponseType(typeof(Report7_3Item))]
        public IHttpActionResult Report7_3([FromUri] int year, Guid userId, string distrSpr)
        {
            try
            {
                var userObj = DAL.GetCissaUsers().FirstOrDefault(x => x.Id == userId);
                if (userObj == null) throw new Exception("User not found!");
                var context = ReportExecutor.CreateContext(userObj.UserName, userObj.Id);
                var result = ReportExecutor.RepetExamInvalidsUPT.Execute(context, year, distrSpr); 
                return Ok(result);
            }
            catch (Exception e)
            {
                return BadRequest(e.GetBaseException().Message);
            }
        }
        //Результаты переосвидетельствования ЛОВЗ из числа ЛОВЗ от ПРОФ. ЗАБОЛЕВАНИЯ, имеющих группу инвалидности
        [HttpGet]
        [ResponseType(typeof(Report7_2Item))]
        public IHttpActionResult Report7_4([FromUri] int year, Guid userId, string distrSpr)
        {
            try
            {
                var userObj = DAL.GetCissaUsers().FirstOrDefault(x => x.Id == userId);
                if (userObj == null) throw new Exception("User not found!");
                var context = ReportExecutor.CreateContext(userObj.UserName, userObj.Id);
                var result = ReportExecutor.RepetExamInvalidsProfDis.Execute(context, year, distrSpr); /**/
                return Ok(result);
            }
            catch (Exception e)
            {
                return BadRequest(e.GetBaseException().Message);
            }
        }
        //Результаты переосвидетельствования ЛОВЗ из числа ЛОВЗ от ПРОФ. ЗАБОЛЕВАНИЯ, имеющих УПТ в % без ГИ
        [HttpGet]
        [ResponseType(typeof(Report7_3Item))]
        public IHttpActionResult Report7_5([FromUri] int year, Guid userId, string distrSpr)
        {
            try
            {
                var userObj = DAL.GetCissaUsers().FirstOrDefault(x => x.Id == userId);
                if (userObj == null) throw new Exception("User not found!");
                var context = ReportExecutor.CreateContext(userObj.UserName, userObj.Id);
                var result = ReportExecutor.RepetExamInvalidsPF.Execute(context, year, distrSpr);
                return Ok(result);
            }
            catch (Exception e)
            {
                return BadRequest(e.GetBaseException().Message);
            }
        }

        //Рекомендации по реабилитации инвалидов 8 отчет
        [HttpGet]
        [ResponseType(typeof(Report8Item))]
        public IHttpActionResult Report8([FromUri] int year, Guid userId, string distrSpr)
        {
            try
            {
                var userObj = DAL.GetCissaUsers().FirstOrDefault(x => x.Id == userId);
                if (userObj == null) throw new Exception("User not found!");
                var context = ReportExecutor.CreateContext(userObj.UserName, userObj.Id);
                var result = ReportExecutor.RecomRehabDisabilities.Execute(context, year, distrSpr); /**/
                return Ok(result);
            }
            catch (Exception e)
            {
                return BadRequest(e.GetBaseException().Message);
            }
        }

        //Ф7Д
        //Результаты первичных освидетельствований детей в возрасте до 18 лет
        [HttpGet]
        [ResponseType(typeof(Report7D1Item[]))]
        public IHttpActionResult Report7D1([FromUri] int year, Guid userId, string distrSpr)
        {
            try
            {
                var userObj = DAL.GetCissaUsers().FirstOrDefault(x => x.Id == userId);
                if (userObj == null) throw new Exception("User not found!");
                var context = ReportExecutor.CreateContext(userObj.UserName, userObj.Id);
                var result = ReportExecutor.PrimaryInvalids18Age.Execute(context, year, distrSpr); /**/
                return Ok(result.ToArray());
            }
            catch (Exception e)
            {
                return BadRequest(e.GetBaseException().Message);
            }
        }
        //Результаты первичных освидетельствований  детей в возрасте до 18 лет (человек), проживающих в ГМ
        [HttpGet]
        [ResponseType(typeof(Report7D2Item[]))]
        public IHttpActionResult Report7D2([FromUri] int year, Guid userId, string distrSpr)
        {
            try
            {
                var userObj = DAL.GetCissaUsers().FirstOrDefault(x => x.Id == userId);
                if (userObj == null) throw new Exception("User not found!");
                var context = ReportExecutor.CreateContext(userObj.UserName, userObj.Id);
                var result = ReportExecutor.PrimaryInvalidsOfCity18Age.Execute(context, year, distrSpr); /**/
                return Ok(result.ToArray());
            }
            catch (Exception e)
            {
                return BadRequest(e.GetBaseException().Message);
            }
        }
        //Результаты первичных освидетельствований  детей в возрасте до 18 лет (человек), проживающих в CМ
        [HttpGet]
        [ResponseType(typeof(Report7D2Item[]))]
        public IHttpActionResult Report7D3([FromUri] int year, Guid userId, string distrSpr)
        {
            try
            {
                var userObj = DAL.GetCissaUsers().FirstOrDefault(x => x.Id == userId);
                if (userObj == null) throw new Exception("User not found!");
                var context = ReportExecutor.CreateContext(userObj.UserName, userObj.Id);
                var result = ReportExecutor.PrimaryInvalidsOfVillage18Age.Execute(context, year, distrSpr); /**/
                return Ok(result.ToArray());
            }
            catch (Exception e)
            {
                return BadRequest(e.GetBaseException().Message);
            }
        }
        //Результаты первичных освидетельствований  детей в возрасте до 18 лет (человек), проживающих в ГМ + СМ
        [HttpGet]
        [ResponseType(typeof(Report7D2Item[]))]
        public IHttpActionResult Report7D4([FromUri] int year, Guid userId, string distrSpr)
        {
            try
            {
                var userObj = DAL.GetCissaUsers().FirstOrDefault(x => x.Id == userId);
                if (userObj == null) throw new Exception("User not found!");
                var context = ReportExecutor.CreateContext(userObj.UserName, userObj.Id);
                var result = ReportExecutor.PrimaryInvalidsOfCityVillage18Age.Execute(context, year, distrSpr); /**/
                return Ok(result.ToArray());
            }
            catch (Exception e)
            {
                return BadRequest(e.GetBaseException().Message);
            }
        }
        //Результаты переосвидетельствований детей- инвалидов в возрасте до 18 лет
        [HttpGet]
        [ResponseType(typeof(Report7D2Item[]))]
        public IHttpActionResult Report7D5([FromUri] int year, Guid userId, string distrSpr)
        {
            try
            {
                var userObj = DAL.GetCissaUsers().FirstOrDefault(x => x.Id == userId);
                if (userObj == null) throw new Exception("User not found!");
                var context = ReportExecutor.CreateContext(userObj.UserName, userObj.Id);
                var result = ReportExecutor.RepetExamInvalids18Age.Execute(context, year, distrSpr); /**/
                return Ok(result.ToArray());
            }
            catch (Exception e)
            {
                return BadRequest(e.GetBaseException().Message);
            }
        }

        //Рекомендации по реабилитации детей-инвалидов
        [HttpGet]
        [ResponseType(typeof(Report8Item))]
        public IHttpActionResult Report7D6([FromUri] int year, Guid userId, string distrSpr)
        {
            try
            {
                var userObj = DAL.GetCissaUsers().FirstOrDefault(x => x.Id == userId);
                if (userObj == null) throw new Exception("User not found!");
                var context = ReportExecutor.CreateContext(userObj.UserName, userObj.Id);
                var result = ReportExecutor.RecomRehabDisabilities18Age.Execute(context, year, distrSpr); 
                return Ok(result);
            }

            catch (Exception e)
            {
                return BadRequest(e.GetBaseException().Message);
            }
        }

        //МОДУЛЬ БРРП
        //Сведения о численности получателей пособия по БР по категориям получателей
        [HttpGet]
        [ResponseType(typeof(ReportBR1Item[]))]
        public IHttpActionResult ReportBR1([FromUri] DateTime fd, [FromUri] DateTime ld, Guid userId)
        {
            try
            {
                var userObj = DAL.GetCissaUsers().FirstOrDefault(x => x.Id == userId);
                if (userObj == null) throw new Exception("User not found!");
                var context = ReportExecutor.CreateContext(userObj.UserName, userObj.Id);
                var result = ReportExecutor.CalBenefitReportsByRegionsBR.Execute(context, fd, ld);
                return Ok(result.ToArray());
            }
            catch (Exception e)
            {
                return BadRequest(e.GetBaseException().Message);
            }
        }
        //Сведения о численности получателей пособия по БР по категориям получателей  Ежеквартально
        [HttpGet]
        [ResponseType(typeof(ReportBR2Item[]))]
        public IHttpActionResult ReportBR2([FromUri] DateTime fd, [FromUri] DateTime ld, Guid userId)
        {
            try
            {
                var userObj = DAL.GetCissaUsers().FirstOrDefault(x => x.Id == userId);
                if (userObj == null) throw new Exception("User not found!");
                var context = ReportExecutor.CreateContext(userObj.UserName, userObj.Id);
                var result = ReportExecutor.CalBenefitReportsBR.Execute(context, fd, ld);
                return Ok(result.ToArray());
            }
            catch (Exception e)
            {
                return BadRequest(e.GetBaseException().Message);
            }
        }
        //Динамика численности получателей БР с 2019 года
        [HttpGet]
        [ResponseType(typeof(ReportBR3Item))]
        public IHttpActionResult ReportBR3([FromUri] int year, Guid userId)
        {
            try
            {
                var userObj = DAL.GetCissaUsers().FirstOrDefault(x => x.Id == userId);
                if (userObj == null) throw new Exception("User not found!");
                var context = ReportExecutor.CreateContext(userObj.UserName, userObj.Id);
                var result = ReportExecutor.DinamicReportsBR.Execute(context, year);
                return Ok(result);
            }
            catch (Exception e)
            {
                return BadRequest(e.GetBaseException().Message);
            }
        }
        //Сведения о численности получателей, среднем размере и выплаченной сумме РП по состоянию
        [HttpGet]
        [ResponseType(typeof(ReportRP1Item[]))]
        public IHttpActionResult ReportRP1([FromUri] DateTime fd, [FromUri] DateTime ld, Guid userId)
        {
            try
            {
                var userObj = DAL.GetCissaUsers().FirstOrDefault(x => x.Id == userId);
                if (userObj == null) throw new Exception("User not found!");
                var context = ReportExecutor.CreateContext(userObj.UserName, userObj.Id);
                var result = ReportExecutor.CalBenefitReportsByRegionsRP.Execute(context, fd, ld);
                return Ok(result.ToArray());
            }
            catch (Exception e)
            {
                return BadRequest(e.GetBaseException().Message);
            }
        }

        //Сведения о численности получателей, среднем размере и выплаченной сумме РП по категориям получателей Ежеквартально
        [HttpGet]
        [ResponseType(typeof(ReportRP2Item[]))]
        public IHttpActionResult ReportRP2([FromUri] DateTime fd, [FromUri] DateTime ld, Guid userId)
        {
            try
            {
                var userObj = DAL.GetCissaUsers().FirstOrDefault(x => x.Id == userId);
                if (userObj == null) throw new Exception("User not found!");
                var context = ReportExecutor.CreateContext(userObj.UserName, userObj.Id);
                var result = ReportExecutor.CalBenefitReportsRP.Execute(context, fd, ld);
                return Ok(result.ToArray());
            }
            catch (Exception e)
            {
                return BadRequest(e.GetBaseException().Message);
            }
        }
        //Динамика численности получателей РП с 2019 года
        [HttpGet]
        [ResponseType(typeof(ReportRP3Item))]
        public IHttpActionResult ReportRP3([FromUri] int year, Guid userId)
        {
            try
            {
                var userObj = DAL.GetCissaUsers().FirstOrDefault(x => x.Id == userId);
                if (userObj == null) throw new Exception("User not found!");
                var context = ReportExecutor.CreateContext(userObj.UserName, userObj.Id);
                var result = ReportExecutor.DinamicReportsRP.Execute(context, year);
                return Ok(result);
            }
            catch (Exception e)
            {
                return BadRequest(e.GetBaseException().Message);
            }
        }
        //Форма 8 для УТСР
        [HttpGet]
        [ResponseType(typeof(ReportF8Item[]))]
        public IHttpActionResult Report_F8([FromUri] DateTime fd, [FromUri] DateTime ld, Guid userId)
        {
            try
            {
                var userObj = DAL.GetCissaUsers().FirstOrDefault(x => x.Id == userId);
                if (userObj == null) throw new Exception("User not found!");
                var context = ReportExecutor.CreateContext(userObj.UserName, userObj.Id);
                var result = ReportExecutor.Report_F8.Execute(context, fd, ld);
                return Ok(result.ToArray());
            }
            catch (Exception e)
            {
                return BadRequest(e.GetBaseException().Message);
            }
        }
        //Форма 8 свод для ДСО 
        [HttpGet]
        [ResponseType(typeof(ReportF8Item[]))]
        public IHttpActionResult Report_F8SVOD([FromUri] DateTime fd, [FromUri] DateTime ld, Guid userId)
        {
            try
            {
                var userObj = DAL.GetCissaUsers().FirstOrDefault(x => x.Id == userId);
                if (userObj == null) throw new Exception("User not found!");
                var context = ReportExecutor.CreateContext(userObj.UserName, userObj.Id);
                var result = ReportExecutor.Report_F8SVOD.Execute(context, fd, ld);
                return Ok(result.ToArray());
            }
            catch (Exception e)
            {
                return BadRequest(e.GetBaseException().Message);
            }
        }
    }
}
