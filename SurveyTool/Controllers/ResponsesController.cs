using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SurveyTool.Models;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity;
using System.Web.Security;

namespace SurveyTool.Controllers
{
    [Authorize]
    public class ResponsesController : Controller
    {
        private readonly ApplicationDbContext _db;

        public ResponsesController(ApplicationDbContext db)
        {
            _db = db;
        }
        public ActionResult Index()
        {
            var responses = _db.Responses
                     .Include("Survey")
                     .Include("Answers")
                     .Where(x => x.CreatedBy == User.Identity.Name)
                     .OrderByDescending(x => x.CreatedOn)
                     .ThenByDescending(x => x.Id)
                     .ToList();
            return View(responses);
        }

        [HttpGet]
        public ActionResult Details(int surveyId, int responseId)
        {
            var response = GetResponse(surveyId, responseId);

            return View(response);
        }

        [HttpGet]
        [Authorize(Roles = "Admin, Teacher")]
        public ActionResult Create(int surveyId)
        {
            var survey = _db.Surveys
                            .Where(s => s.Id == surveyId)
                            .Select(s => new
                            {
                                Survey = s,
                                Questions = s.Questions
                                                 .Where(q => q.IsActive)
                                                 .OrderBy(q => q.Priority)
                            })
                             .AsEnumerable()
                             .Select(x =>
                             {
                                 x.Survey.Questions = x.Questions.ToList();
                                 return x.Survey;
                             })
                             .Single();

            //受評分者選單(尚無篩選Student)
            var allusers = _db.Users.Select(u => new SelectListItem { Value = u.UserName.ToString(), Text = u.Name }).OrderBy(sl => sl.Value).ToList();

            //foreach (var user in UserManager.Users.ToList())
            //{
            //    if (UserManager.IsInRole(user.Id, "Student"))
            //    {

            //    }
            //}
            //var roleId = _db.Roles.Where(r => r.Name == "Student").Select(r => r.Id);
            //var list = _db.Users.Include(u =>u.Roles).Where(u => u.Roles.Select(ur => ur.RoleId) == roleId ).Select(u => u.UserName).ToList();
            ViewBag.Users = allusers;

            return View(survey);
        }

        [HttpPost]
        [Authorize(Roles = "Admin, Teacher")]
        public ActionResult Create(int surveyId, string action, Response model, string CreatedFor)
        {
            //model.Answers = model.Answers.Where(a => !String.IsNullOrEmpty(a.Value)).ToList(); //未填之問題不存入回答(Answer)資料表
            model.SurveyId = surveyId;
            model.CreatedBy = User.Identity.Name;
            model.CreatedFor = CreatedFor;
            model.CreatedOn = DateTime.Now;
            _db.Responses.Add(model);
            _db.SaveChanges();

            TempData["success"] = "回覆已經成功的儲存!";

            return action == "Next"
                       ? RedirectToAction("Create", new { surveyId })
                       : RedirectToAction("Index", "Home");
        }

        public ActionResult Edit(int surveyId, int responseId)
        {
            var createOn = _db.Responses.Where(r => r.SurveyId == surveyId).Single(r => r.Id == responseId).CreatedOn;
            var distance = DateTime.Now.Subtract(createOn).Duration().TotalDays;
            if (distance > 1)//1天以內可以刪除
            {
                return RedirectToAction("Details", new { surveyId = surveyId, responseId = responseId });
            }
            else
            {
                var response = GetResponse(surveyId, responseId);

                return View(response);
            }
        }
        [HttpPost]
        public ActionResult Edit(Response model)
        {
            try
            {
                foreach (var answer in model.Answers)
                {
                    _db.Entry(answer).State = EntityState.Modified;
                }
                _db.Entry(model).State = System.Data.Entity.EntityState.Modified;
                _db.SaveChanges();

                return RedirectToAction("Index", "Dashboard");
            }
            catch
            {
                return RedirectToAction("Index", "Dashboard");
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin, Teacher")]
        public ActionResult Delete(int surveyId, int responseId, string returnTo)
        {
            
            var createOn = _db.Responses.Where(r => r.SurveyId == surveyId).Single(r => r.Id == responseId).CreatedOn;
            var distance = DateTime.Now.Subtract(createOn).Duration().TotalDays;
            if (distance > 1) //1天以內可以刪除
            {
                var feedback = _db.Feedbacks.SingleOrDefault(f => f.ResponseId == responseId);
                if (feedback != null)
                {
                    _db.Entry(feedback).State = EntityState.Deleted;
                }
                var response = new Response() { Id = responseId, SurveyId = surveyId };
                _db.Entry(response).State = EntityState.Deleted;
                _db.SaveChanges();
                return Redirect(returnTo ?? Url.RouteUrl("Root"));
            }
            else
            {
                return Redirect(returnTo ?? Url.RouteUrl("Root"));
            }
            
        }

        public ActionResult MyOwnResponses()
        {
            var responses = _db.Responses
                               .Include("Survey")
                               .Include("Answers")
                               .Where(x => x.CreatedFor == User.Identity.Name)
                               .OrderByDescending(x => x.CreatedOn)
                               .ThenByDescending(x => x.Id)
                               .ToList();

            return View(responses);
        }

        public ActionResult Feedback(int surveyId, int responseId)
        {
            var createOn = _db.Responses.Where(r => r.SurveyId == surveyId).Single(r => r.Id == responseId).CreatedOn;
            var distance = DateTime.Now.Subtract(createOn).Duration().TotalDays;
            if (distance > 7)
            {
                return RedirectToAction("Details", new { surveyId = surveyId, responseId = responseId });
            }
            else
            {
                var response = GetResponse(surveyId, responseId);
                return View(response);
            }
        }

        [HttpPost]
        public ActionResult Feedback(int Id, Feedback model)
        {
            if (model.ResponseId == 0) //新回饋
            {
                model.CreatedOn = DateTime.Now;
                model.ModifiedOn = DateTime.Now;
                model.ResponseId = Id;
                _db.Feedbacks.Add(model);
                //_db.Entry(model).State = EntityState.Added;
            }
            else
            {
                model.ModifiedOn = DateTime.Now;
                _db.Entry(model).State = System.Data.Entity.EntityState.Modified;
                _db.Entry(model).Property(x => x.CreatedOn).IsModified = false;

            }
            _db.SaveChanges();
            return RedirectToAction("MyOwnResponses");
        }

        private Response GetResponse(int surveyId, int responseId)
        {
            var response = _db.Responses
                    .Include("Survey")
                    .Include("Answers")
                    .Include("Answers.Question")
                    .Include("Feedback")
                    .Where(r => r.SurveyId == surveyId)
                    .Single(r => r.Id == responseId);
            response.Answers = response.Answers.OrderBy(x => x.Question.Priority).ToList();
            return response;
        }
    }
}