using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using SurveyTool.Models;

namespace SurveyTool.Controllers
{
    public class AnswersController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Answers
        public ActionResult Index(int id)
        {
            var answers = db.Answers.Include(a => a.Question).Include(a => a.Response);

            var query = db.Answers
                .Include(a => a.Question)
                .Include(a => a.Response)
                .Where(a=>a.Question.SurveyId == id)
                .OrderBy(a => a.ResponseId)
                .OrderBy(a => a.Id);


            return View(query.ToList());
        }

        //// GET: Answers/Details/5
        //public ActionResult Details(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    Answer answer = db.Answers.Find(id);
        //    if (answer == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(answer);
        //}       

        //// GET: Answers/Edit/5
        //public ActionResult Edit(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    Answer answer = db.Answers.Find(id);
        //    if (answer == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    ViewBag.QuestionId = new SelectList(db.Questions, "Id", "Title", answer.QuestionId);
        //    ViewBag.ResponseId = new SelectList(db.Responses, "Id", "CreatedBy", answer.ResponseId);
        //    return View(answer);
        //}

        //// POST: Answers/Edit/5
        //// 若要免於過量張貼攻擊，請啟用想要繫結的特定屬性，如需
        //// 詳細資訊，請參閱 http://go.microsoft.com/fwlink/?LinkId=317598。
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Edit([Bind(Include = "Id,ResponseId,Value,Comment,QuestionId")] Answer answer)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        db.Entry(answer).State = EntityState.Modified;
        //        db.SaveChanges();
        //        return RedirectToAction("Index");
        //    }
        //    ViewBag.QuestionId = new SelectList(db.Questions, "Id", "Title", answer.QuestionId);
        //    ViewBag.ResponseId = new SelectList(db.Responses, "Id", "CreatedBy", answer.ResponseId);
        //    return View(answer);
        //}

        //// GET: Answers/Delete/5
        //public ActionResult Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    Answer answer = db.Answers.Find(id);
        //    if (answer == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(answer);
        //}

        //// POST: Answers/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public ActionResult DeleteConfirmed(int id)
        //{
        //    Answer answer = db.Answers.Find(id);
        //    db.Answers.Remove(answer);
        //    db.SaveChanges();
        //    return RedirectToAction("Index");
        //}

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
