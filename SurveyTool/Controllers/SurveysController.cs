using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SurveyTool.Models;

namespace SurveyTool.Controllers
{
    [Authorize(Roles = "Admin")]
    public class SurveysController : Controller
    {
        private readonly ApplicationDbContext _db;

        public SurveysController(ApplicationDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public ActionResult Index()
        {
            var surveys = _db.Surveys.ToList();
            return View(surveys);
        }

        [HttpGet]
        public ActionResult Create()
        {
            var survey = new Survey
                {
                    StartDate = DateTime.Now,
                    EndDate = DateTime.Now.AddYears(1)
                };

            return View(survey);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Create(Survey survey, string action)
        {
            if (ModelState.IsValid)
            {
                survey.Questions.ForEach(q => q.CreatedOn = q.ModifiedOn = DateTime.Now);
                _db.Surveys.Add(survey);
                _db.SaveChanges();
                TempData["success"] = "表單已經成功的建立!";
                return RedirectToAction("Edit", new {id = survey.Id});
            }
            else
            {
                TempData["error"] = "新建此表單時發生錯誤.";
                return View(survey);
            }
        }

        [HttpGet]
        public ActionResult Edit(int id)
        {
            var survey = _db.Surveys.Include("Questions").Single(x => x.Id == id);
            survey.Questions = survey.Questions.OrderBy(q => q.Priority).ToList();
            return View(survey);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Edit(Survey model)
        {
            foreach (var question in model.Questions)
            {
                question.SurveyId = model.Id;

                if (question.Id == 0) //新Question資料
                {
                    question.CreatedOn = DateTime.Now;
                    question.ModifiedOn = DateTime.Now;
                    _db.Entry(question).State = EntityState.Added;
                }
                else //無刪除Question，以標記Disable來取消
                {
                    question.ModifiedOn = DateTime.Now;
                    _db.Entry(question).State = EntityState.Modified;
                    _db.Entry(question).Property(x => x.CreatedOn).IsModified = false;
                }
            }

            _db.Entry(model).State = EntityState.Modified;
            _db.SaveChanges();
            //return RedirectToAction("Edit", new {id = model.Id});
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Delete(Survey survey)
        {
            _db.Entry(survey).State = EntityState.Deleted;
            _db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult Copy(int id)
        {
            var time = DateTime.Now;
            var newQuestions = new List<Question>();

            var oldSurvey = _db.Surveys.Include("Questions").Single(s => s.Id == id);

            oldSurvey.Questions.OrderBy(q => q.Priority).ToList().ForEach(q => newQuestions.Add(new Question
            {
                Title = q.Title,
                Type = q.Type,
                Body = q.Body,
                Priority = q.Priority,
                IsActive = q.IsActive,
                CreatedOn = time,
                ModifiedOn = time
            }));

            var newSurvey = new Survey
            {
                Name = oldSurvey.Name,
                StartDate = time,
                EndDate = time.AddYears(1),
                Questions = newQuestions
            };
            _db.Surveys.Add(newSurvey);
            _db.SaveChanges();

            return RedirectToAction("Index");
        }
    }
}
