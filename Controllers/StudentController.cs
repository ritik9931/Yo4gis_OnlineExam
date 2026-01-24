using Dapper;
using Npgsql;
using onlineExam.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace onlineExam.Controllers
{
    public class StudentController : Controller
    {
        private readonly IDbConnection _db;
        public StudentController()
        {
            string conn = ConfigurationManager.ConnectionStrings["PostgresConn"].ConnectionString;
            _db = new NpgsqlConnection(conn);
        }

        [SessionAuthorize]
        // GET: Student
        public ActionResult Index()
        {
            return View();
        }
        [SessionAuthorize]
        public ActionResult Exam()
        {
            return View();
        }

        [SessionAuthorize]
        public ActionResult ExamTestFree()
        {
            if (Session["Username"] == null)
                return RedirectToAction("Login", "Account");

            return View();
        }

        [HttpGet]
        public ActionResult GetAllExamAssignedSubject()
        {
            string connStr = ConfigurationManager
                   .ConnectionStrings["PostgresConn"]
                   .ConnectionString;

            string qry = "select distinct et.subject_id,st.subject_title from public.exams_tbl et join public.subjects_tbl st on et.subject_id=st.subject_id";
            var res = _db.Query<dynamic>(qry, CommandType.Text).ToList();

            return Json(new { success = true, message = "SUCCESS", data = res }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetAllRequest(string studentid)
        {
            string connStr = ConfigurationManager
                   .ConnectionStrings["PostgresConn"]
                   .ConnectionString;
            //Pending, Approved, Rejected, Completed
            string qry = "select er.question_cnt,em.exam_id,em.exam_title,st.subject_id,st.subject_title,er.status,TO_CHAR(er.requested_at::timestamptz, 'DD-MM-YYYY') as requested_at from public.exam_request er join  exams_tbl em on em.exam_id=er.exam_id join subjects_tbl st on st.subject_id = er.subject_id where er.student_id ='" + studentid+"'";
            var res = _db.Query<dynamic>(qry, CommandType.Text).ToList();

            return Json(new { success = true, message = "SUCCESS", data = res }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetExamDetailsBySubject(string subjectId)
        {
            string connStr = ConfigurationManager
                   .ConnectionStrings["PostgresConn"]
                   .ConnectionString;

            string qry = @"SELECT 
                            em.exam_id,
                                em.exam_title,
                                sm.subject_title,
								sm.subject_id,
                                COUNT(eq.question_id) AS number_questions
                            FROM exams_tbl em
                            JOIN subjects_tbl sm
                                ON sm.subject_id = em.subject_id
                            LEFT JOIN exam_question eq
                                ON eq.exam_id = em.exam_id
                                where 	sm.subject_id='"+subjectId+"' GROUP BY  em.exam_id,em.exam_title,sm.subject_title,sm.subject_id ORDER BY em.exam_title ";
            var res = _db.Query<dynamic>(qry, CommandType.Text).ToList();

            return Json(new { success = true, message = "SUCCESS", data = res }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult RaiseExamRequest(ExamRequest model)
        {
            string connStr = ConfigurationManager
                   .ConnectionStrings["PostgresConn"]
                   .ConnectionString;

            if (model == null)
            {
                return Json(new { success = false, message = "No data received." });
            }

            //using (var con = new NpgsqlConnection(connStr))
            //{
            // Save to DB
            string qry = "select count(*) from public.exam_request where exam_id='"+model.exam_id+"' and student_id='"+model.student_id+"' and subject_id='"+model.subject_id+"'";
            var res = _db.Query<dynamic>(qry, CommandType.Text).ToList();
            int cnt = Convert.ToInt32(res[0].count);
            if (cnt == 0)
            {
                qry = "insert into public.exam_request(student_id,question_cnt, exam_id, subject_id, status, requested_at) values('" + model.student_id + "','" + model.question_cnt + "','" + model.exam_id + "','" + model.subject_id + "','REQUESTED',NOW())";
                var res1 = _db.Query<dynamic>(qry, CommandType.Text);
                return Json(new { success = true, message = "Request saved successfully." });
            }
            else
            {
                return Json(new { success = false, message = "Request for same Subject and Exam is already raised." });
            }



            // Example DB insert
            // _db.Execute("INSERT INTO questions ...", q);

            //}



            return Json(new { success = true, message = "Request saved successfully." });
        }

        [HttpGet]
        public ActionResult GetAllRequested()
        {
            string connStr = ConfigurationManager
                   .ConnectionStrings["PostgresConn"]
                   .ConnectionString;

            string qry = @"select er.id,sm.name,er.student_id,er.question_cnt,em.exam_id,em.exam_title,st.subject_id,st.subject_title,status from public.exam_request er join  exams_tbl em on em.exam_id=er.exam_id
	join subjects_tbl st on st.subject_id=er.subject_id 
	join student_master sm on sm.id=er.student_id ";
            var res = _db.Query<dynamic>(qry, CommandType.Text).ToList();

            return Json(new { success = true, message = "SUCCESS", data = res }, JsonRequestBehavior.AllowGet);
        }


        //
    }
}