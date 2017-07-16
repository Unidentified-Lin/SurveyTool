using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SurveyTool.Models;
using System.IO;
using System.Web.UI;

namespace SurveyTool.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _db;

        public ReportsController(ApplicationDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public ActionResult Index()
        {
            var surveys = _db.Surveys.ToList();
            ViewBag.Responses = GetResponses(null);
            return View(surveys);
        }

        public ActionResult SurveyResponses(int surveyId)
        {
            var responses = GetResponses(surveyId);

            return View(responses);
        }

        public ActionResult Report(int surveyId, string surveyName)
        {
            var report = GetResponses(surveyId);
            ViewBag.Questions = GetQuestions(surveyId);
            ViewBag.SurveyName = surveyName;
            ViewBag.SurveyId = surveyId;

            return View(report);
        }

        private IEnumerable<Response> GetResponses(int? surveyId)
        {
            return _db.Responses
                     .Include("Survey")
                     .Include("Answers")
                     .Include("Feedback")
                     .Where(x => !surveyId.HasValue || x.SurveyId == surveyId)
                     .OrderByDescending(x => x.CreatedOn)
                     .ThenByDescending(x => x.Id)
                     .ToList();
        }
        private IEnumerable<Question> GetQuestions(int surveyId)
        {
            return _db.Questions
                .Where(q => q.SurveyId == surveyId)
                .OrderBy(q => q.Priority)
                .ToList();
        }

        public void ExportToCSV(int surveyId)
        {
            StringWriter sw = new StringWriter();

            var qlist = GetQuestions(surveyId).Select(q => q.Title);
            string qheader = string.Join("\",\"", qlist); // [\"]表示雙引號
            sw.WriteLine("\"日期\",\"評分者\",\"被評者\",\"" + qheader + "\",\"學員滿意度\",\"學員回饋\"");

            Response.ClearContent();
            Response.Charset = "BIG5";
            Response.AddHeader("content-disposition", "attachment;filename=Exported_" + DateTime.Now + ".csv");
            Response.ContentEncoding = System.Text.Encoding.GetEncoding("BIG5");
            Response.ContentType = "text/csv";

            var responses = GetResponses(surveyId);
            foreach (var response in responses)
            {
                var head = response.CreatedOn + "\",\"" + response.CreatedBy + "\",\"" + response.CreatedFor;
                var alist = response.Answers.Select(r => r.Value);
                string value = string.Join("\",\"", alist);

                var fvalue = "";
                var fcomment = "";
                if (response.Feedback != null)
                {
                    fvalue = response.Feedback.Value;
                    fcomment = response.Feedback.Comment;
                }
                var foot = fvalue + "\",\"" + fcomment;

                sw.WriteLine("\"" + head + "\",\"" + value + "\",\"" + foot + "\"");
            }

            Response.Write(sw.ToString());

            Response.End();
        }

        //public void ExportToExcel(int surveyId)
        //{
        //    var grid = new System.Web.UI.WebControls.GridView();

        //    grid.DataSource = GetResponses(surveyId); //因資料表設計方式，需特別寫查詢語法(HOW?)

        //    grid.DataBind();

        //    Response.ClearContent();
        //    Response.AddHeader("content-disposition", "attachment; filename=Exported_" + DateTime.Now + ".xls");
        //    Response.ContentType = "application/excel";
        //    StringWriter sw = new StringWriter();
        //    HtmlTextWriter htw = new HtmlTextWriter(sw);

        //    grid.RenderControl(htw);

        //    Response.Write(sw.ToString());

        //    Response.End();
        //}

        //[HttpGet]
        //public ActionResult DetailsTable(int id)
        //{
        //    var query = _db.Questions
        //        .Include("Surveys")
        //        .Include("Answers")
        //        .Where(r => r.SurveyId == id)
        //        .OrderByDescending(x => x.CreatedOn)
        //        .ThenByDescending(x => x.Id)
        //        .ToList();

        //    return View(query);
        //}

        //暫無用到
        //[HttpGet]
        //public ActionResult Details(int id, int? departmentId, DateTime? startDate, DateTime? endDate)
        //{
        //    var questions = new List<QuestionViewModel>();
        //    startDate = startDate ?? new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        //    endDate = endDate ?? DateTime.Now;

        //    var survey = _db.Surveys.Single(s => s.Id == id);

        //    _db.Questions
        //       .Where(q => q.SurveyId == id)
        //       .OrderBy(q => q.Priority)
        //       .Select(q => new
        //       {
        //           q.Title,
        //           q.Body,
        //           q.Type,
        //           Answers = _db.Answers.Where(a => a.QuestionId == q.Id &&
        //                                            a.Response.CreatedOn >= startDate.Value &&
        //                                            a.Response.CreatedOn <= endDate.Value)
        //       })
        //       .ToList()
        //       .ForEach(r => questions.Add(new QuestionViewModel
        //       {
        //           Title = r.Title,
        //           Body = r.Body,
        //           Type = r.Type,
        //           Answers = r.Answers.ToList()
        //       }));

        //    var vm = new ReportViewModel
        //    {
        //        StartDate = startDate.Value,
        //        EndDate = endDate.Value,
        //        Survey = survey,
        //        Responses = questions
        //    };

        //    return View(vm);
        //}
    }
}
