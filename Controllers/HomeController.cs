using Npgsql;
using onlineExam.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Dapper;

namespace onlineExam.Controllers
{
    public class HomeController : Controller
    {
        private readonly IDbConnection _db;
        public HomeController()
        {
            string conn = ConfigurationManager.ConnectionStrings["PostgresConn"].ConnectionString;
            _db = new NpgsqlConnection(conn);
        }
        [SessionAuthorize]
        public ActionResult Index()
        {
            if (Session["Username"] == null)
                return RedirectToAction("Login", "Account");

            return View();
        }

        public ActionResult CreateUser()
        {
            return View();
        }
        public ActionResult Analytics()
        {
            return View();
        }
        [SessionAuthorize]
        public ActionResult Subject()
        {
            return View();
        }
        [SessionAuthorize]
        public ActionResult Questions()
        {
            return View();
        }
        [SessionAuthorize]
        public ActionResult Exams()
        {
            return View();
        }
        [SessionAuthorize]
        public ActionResult ExamStudent()
        {
            return View();
        }


        [HttpPost]
        public ActionResult InsertQuestions(QuestionRequestModel model)
        {
            string connStr = ConfigurationManager
                   .ConnectionStrings["PostgresConn"]
                   .ConnectionString;

            if (model == null || model.questions == null || !model.questions.Any())
            {
                return Json(new { success = false, message = "No data received." });
            }

            //using (var con = new NpgsqlConnection(connStr))
            //{
                // Save to DB
                foreach (var q in model.questions)
                {

                string qry = @"INSERT INTO public.questions_tbl
(question_text, option_a, option_b, option_c, option_d, sub_name, correct_option, explanation, marks, difficulty, category)
VALUES
(@question_text, @option_a, @option_b, @option_c, @option_d, @sub_name, @correct_option, @explanation, @marks, @difficulty, @category)";

                var result = _db.Execute(qry, new
                {
                    question_text = q.question_text,
                    option_a = q.option_a,
                    option_b = q.option_b,
                    option_c = q.option_c,
                    option_d = q.option_d,
                    sub_name = model.subject_name,
                    correct_option = q.correct_option,
                    explanation = q.explanation,
                    marks = q.marks,
                    difficulty = q.difficulty,
                    category = q.category
                });

                //string qry = "insert into public.questions_tbl(question_text, option_a, option_b, option_c, option_d, sub_name, correct_option, explanation, marks, difficulty, category) values('" + q.question_text + "','" + q.option_a + "','" + q.option_b + "','" + q.option_c + "','" + q.option_d + "','" + model.subject_name + "','" + q.correct_option + "','" + q.explanation + "','" + q.marks + "','" + q.difficulty + "','" + q.category + "')";
                    //var res = _db.Query<dynamic>(qry, CommandType.Text);

                // Example DB insert
                // _db.Execute("INSERT INTO questions ...", q);
            }
            //}

              

            return Json(new { success = true, message = "Questions saved successfully." });
        }

        [HttpPost]
        public ActionResult saveSubject(SubjectClass model)
        {
            string connStr = ConfigurationManager
                   .ConnectionStrings["PostgresConn"]
                   .ConnectionString;

            if (model == null || model.sub_code == null || !model.sub_name.Any())
            {
                return Json(new { success = false, message = "No data received." });
            }

            //using (var con = new NpgsqlConnection(connStr))
            //{
            // Save to DB
            string qry = "select count(*) from public.subjects_tbl where lower(subject_title)='" + model.sub_name.ToLower() + "'";
            var res = _db.Query<dynamic>(qry, CommandType.Text).ToList();
            int cnt = Convert.ToInt32(res[0].count);
            if(cnt==0)
            {
                qry = "insert into public.subjects_tbl(subject_title, subject_code) values('" + model.sub_name + "','" + model.sub_code + "')";
                var res1 = _db.Query<dynamic>(qry, CommandType.Text);
                return Json(new { success = true, message = "Questions saved successfully." });
            }
            else
            {
                return Json(new { success = false, message = "Subject is already added." });
            }

           

                // Example DB insert
                // _db.Execute("INSERT INTO questions ...", q);
           
            //}



            return Json(new { success = true, message = "Questions saved successfully." });
        }

        [HttpGet]
        public ActionResult GetAllSubject()
        {
            string connStr = ConfigurationManager
                   .ConnectionStrings["PostgresConn"]
                   .ConnectionString;         



            string qry = "select * from public.subjects_tbl order by subject_title";
            var res = _db.Query<dynamic>(qry, CommandType.Text).ToList();

            return Json(new { success = true, message = "SUCCESS",data=res }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetAllExamAndSubjectAndBatch()
        {
            Dictionary<string, dynamic> result = new Dictionary<string, dynamic>();
            string connStr = ConfigurationManager
                   .ConnectionStrings["PostgresConn"]
                   .ConnectionString;
            //string qry = "select et.exam_id,et.subject_id,et.exam_title,st.subject_title from public.exams_tbl et join public.subjects_tbl st on et.subject_id=st.subject_id order by exam_title";
            string qry = @"SELECT 
                            em.exam_id,
                                em.exam_title,
                                sm.subject_title,
                                COUNT(eq.question_id) AS number_questions
                            FROM exams_tbl em
                            JOIN subjects_tbl sm
                                ON sm.subject_id = em.subject_id
                            LEFT JOIN exam_question eq
                                ON eq.exam_id = em.exam_id
                            GROUP BY
                            em.exam_id,
                                em.exam_title,
                                sm.subject_title
                            ORDER BY em.exam_title; ";
            var res = _db.Query<dynamic>(qry, CommandType.Text);
            result.Add("examData", res);


            qry = "SELECT * FROM public.subjects_tbl order by subject_title";
            res = _db.Query<dynamic>(qry, CommandType.Text);
            result.Add("subjectData", res);

            qry = "SELECT * FROM public.batch_master order by batch_name";
            res = _db.Query<dynamic>(qry, CommandType.Text);
            result.Add("batchData", res);
            return Json(new { success = true, message = "SUCCESS", data = result }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetQuestionsBySubject(string subject)
        {
            //Dictionary<string, dynamic> result = new Dictionary<string, dynamic>();
            string connStr = ConfigurationManager
                   .ConnectionStrings["PostgresConn"]
                   .ConnectionString;
            string qry = "select * from public.questions_tbl where sub_name='" + subject + "'";

            if (subject == "Select All")
                qry = "select * from public.questions_tbl";
            var res = _db.Query<dynamic>(qry, CommandType.Text).ToList();
            //result.Add("examData", res);
            //qry = "SELECT * FROM public.subjects_tbl order by subject_title";
           // res = _db.Query<dynamic>(qry, CommandType.Text).ToList();
            //result.Add("subjectData", res);
            return Json(new { success = true, message = "SUCCESS", data = res }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetStudentByBatch(string batchid)
        {
            //Dictionary<string, dynamic> result = new Dictionary<string, dynamic>();
            string connStr = ConfigurationManager
                   .ConnectionStrings["PostgresConn"]
                   .ConnectionString;
            string qry = "";
            if(batchid!="0")
                qry = "SELECT distinct sm.id, sb.batch_id,sm.name,sm.mobile_number,bm.batch_name FROM public.student_master sm left outer join public.student_batch sb on sm.id=sb.student_id left outer join public.batch_master bm on bm.id=sb.batch_id where batch_id='"+batchid+"'";
            else
            {
                qry = "SELECT distinct sm.id, sm.name,sm.mobile_number FROM public.student_master sm left outer join public.student_batch sb on sm.id=sb.student_id left outer join public.batch_master bm on bm.id=sb.batch_id order by name";
            }
            
            var res = _db.Query<dynamic>(qry, CommandType.Text).ToList();
            //result.Add("examData", res);
            //qry = "SELECT * FROM public.subjects_tbl order by subject_title";
            // res = _db.Query<dynamic>(qry, CommandType.Text).ToList();
            //result.Add("subjectData", res);
            return Json(new { success = true, message = "SUCCESS", data = res }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult SaveExamStudent(List<ExamStudent> model)
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
            Boolean chkDuplicate = false;
            foreach (var q in model)
            {
                string qry = "select count(*) from exam_student_mapping where exam_id='" + q.exam_id + "' and student_id='" + q.student_id + "' and status='ASSIGNED';";
                var res = _db.Query<dynamic>(qry, CommandType.Text).ToList();
                int cnt = Convert.ToInt32(res[0].count);
                if (cnt == 0)
                {
                    qry = "insert into exam_student_mapping(exam_id, student_id,status) values('" + q.exam_id + "','" + q.student_id + "','ASSIGNED')";
                    var res1 = _db.Query<dynamic>(qry, CommandType.Text);
                }
                else
                {
                    chkDuplicate = true;
                }

            }
            //}

            if(chkDuplicate)
            {
                return Json(new { success = true, message = "Exam Student Mapping saved successfully.Duplicate Found" });
            }
            else
            {
                return Json(new { success = true, message = "Exam Student Mapping saved successfully." });
            }

            
        }

        [HttpPost]
        public ActionResult saveExamAndQuestions(List<ExamQuestions> model)
        {
            string connStr = ConfigurationManager
                   .ConnectionStrings["PostgresConn"]
                   .ConnectionString;

            if (model == null)
            {
                return Json(new { success = false, message = "No data received." });
            }

            string qry = "select count(*) from public.exams_tbl where exam_title='"+ model[0].exam_name+"'";
            var res = _db.Query<dynamic>(qry, CommandType.Text).ToList();
            int cnt = Convert.ToInt32(res[0].count);
            if(cnt==0)
            {
                qry = "INSERT INTO public.exams_tbl(subject_id, exam_title) values('" + model[0].subject_id + "','" + model[0].exam_name + "') returning exam_id";
                int exam_id = _db.ExecuteScalar<int>(qry, new
                {
                    SubjectId = model[0].subject_id,
                    ExamTitle = model[0].exam_name
                });

                //var res1 = _db.Query<dynamic>(qry, CommandType.Text);
                //int exam_id = res.ToList()[0].exam_id;

                //using (var con = new NpgsqlConnection(connStr))
                //{
                // Save to DB
                foreach (var q in model)
                {

                    qry = "insert into public.exam_question(exam_id, question_id) values('" + exam_id + "','" + q.question_id + "')";
                    var res2 = _db.Query<dynamic>(qry, CommandType.Text);

               
                }
                return Json(new { success = true, message = "Exam Created Successfully." });
                //}
            }
            else
            {
                return Json(new { success = false, message = "Exam Name already exist." });
            }

            //return Json(new { success = true, message = "Questions saved successfully." });
        }

        [HttpGet]
        public ActionResult GetStudentAssignedExam(string studentId)
        {
            //Dictionary<string, dynamic> result = new Dictionary<string, dynamic>();
            string connStr = ConfigurationManager
                   .ConnectionStrings["PostgresConn"]
                   .ConnectionString;
            string qry = "";

            qry = @"SELECT sm.id as student_id,et.exam_id,et.exam_title,st.subject_title,esm.attempt_no FROM public.exam_student_mapping esm join exams_tbl et on et.exam_id=esm.exam_id
join student_master sm on sm.id=esm.student_id
join subjects_tbl st on st.subject_id=et.subject_id
where sm.mobile_number='" + studentId + "'";
            

            var res = _db.Query<dynamic>(qry, CommandType.Text).ToList();
            //result.Add("examData", res);
            //qry = "SELECT * FROM public.subjects_tbl order by subject_title";
            // res = _db.Query<dynamic>(qry, CommandType.Text).ToList();
            //result.Add("subjectData", res);
            return Json(new { success = true, message = "SUCCESS", data = res }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetExamQuestionForQuiz(string examID, string student_id)
        {
            //Dictionary<string, dynamic> result = new Dictionary<string, dynamic>();
            string connStr = ConfigurationManager
                   .ConnectionStrings["PostgresConn"]
                   .ConnectionString;
            string qry = "";

            qry = @"SELECT q.id, q.question_text, q.option_a, q.option_b, q.option_c, q.option_d, q.sub_name, marks, difficulty, category
FROM exam_question eq
JOIN questions_tbl q 
  ON q.id = eq.question_id
WHERE eq.exam_id ='" + examID+"' ORDER BY eq.question_id; ";


            var res = _db.Query<dynamic>(qry, CommandType.Text).ToList();

            qry = "select attempt_no from public.exam_student_mapping where exam_id='" + examID + "' and student_id='" + student_id + "'";
            var res2 = _db.Query<dynamic>(qry, CommandType.Text).ToList();
            int cntAttempt = Convert.ToInt32(res2[0].attempt_no);


            qry = "update exam_student_mapping set status='IN PROGRESS',attempt_no='"+(cntAttempt+1)+"' where exam_id='" + examID + "' and student_id='" + student_id + "'";
            var res1 = _db.Query<dynamic>(qry, CommandType.Text);

            //result.Add("examData", res);
            //qry = "SELECT * FROM public.subjects_tbl order by subject_title";
            // res = _db.Query<dynamic>(qry, CommandType.Text).ToList();
            //result.Add("subjectData", res);
            return Json(new { success = true, message = "SUCCESS", data = res }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult SubmitAnswers(List<StudentAnswer> answers)
        {
            string qry = "update exam_student_mapping set status='COMPLETED' where exam_id='" + answers[0].ExamId + "' and student_id='" + answers[0].StudentId + "'";
            var res = _db.Query<dynamic>(qry, CommandType.Text);

            
            var QuestionIds = answers.Select(a => a.QuestionId).ToList();
            string questionIdCsv = string.Join(",", QuestionIds);
            string sql = @"SELECT id, correct_option FROM questions_tbl WHERE id IN (" + questionIdCsv+")";
            var correctAnswers = _db.Query(sql, CommandType.Text).ToList();

            int totalQuestions = answers.Count;
            int correctCount = 0;

            foreach (var ans in answers)
            {
                if (ans.SelectedOption != null)
                {
                    var correct = correctAnswers.FirstOrDefault(x => x.id == ans.QuestionId);
                    if (correct != null &&
                        string.Equals(correct.correct_option, ans.SelectedOption, StringComparison.OrdinalIgnoreCase))
                    {
                        correctCount++;
                    }
                }
            }
            int totalMarks = totalQuestions * 1; // or per-question marks
            int obtainedMarks = correctCount * 1;
            bool isPassed = obtainedMarks >= (totalMarks * 0.4); // 40% pass

            // ✅ Update exam_student_mapping
            string updateSql = @"UPDATE exam_student_mapping SET status = 'Completed',completed_at = now(), score ='"+ correctCount+"',total_marks ='"+ correctCount +"',is_pass='"+isPassed.ToString()+"' WHERE exam_id ='"+ answers[0].ExamId+"' AND student_id = '"+ answers[0].StudentId + "'";

            _db.Execute(updateSql, new
            {
                score = obtainedMarks,
                totalMarks = totalMarks,
                isPassed = isPassed,
                examId = answers[0].ExamId,
                studentId = answers[0].StudentId
            });



            //var questions = _db.Questions
            //    .Where(q => answers.Select(a => a.QuestionId).Contains(q.Id))
            //    .ToList();

            //int score = 0;

            //foreach (var ans in answers)
            //{
            //    var q = questions.FirstOrDefault(x => x.Id == ans.QuestionId);
            //    if (q != null && q.CorrectOption == ans.SelectedOption)
            //        score++;
            //}
            return Json(new
            {
                status = "success",
                totalQuestions=totalQuestions,
                correctCount=correctCount,
                obtainedMarks=obtainedMarks,
                totalMarks=totalMarks,
                isPassed=isPassed
            });
            //return Json(new { success = true, score = "", total = "" });
        }

        public ActionResult DownloadQuestionTemplate()
        {
            string filePath = Server.MapPath("~/Content/Template/template_questions.xlsx");

            return File(
                filePath,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "QuestionUploadTemplate.xlsx"
            );
        }

        [HttpGet]
        public ActionResult GetExamAndStudentList()
        {
            //Dictionary<string, dynamic> result = new Dictionary<string, dynamic>();
            string connStr = ConfigurationManager
                   .ConnectionStrings["PostgresConn"]
                   .ConnectionString;
            string qry = "";

            qry = @"SELECT esm.id, esm.exam_id, esm.student_id, assigned_at,exam_title,name, started_at, completed_at, score, total_marks, attempt_no, status
	FROM public.exam_student_mapping esm join student_master sm on sm.id=esm.student_id join exams_tbl et on et.exam_id=esm.exam_id;";


            var res = _db.Query<dynamic>(qry, CommandType.Text).ToList();
            //result.Add("examData", res);
            //qry = "SELECT * FROM public.subjects_tbl order by subject_title";
            // res = _db.Query<dynamic>(qry, CommandType.Text).ToList();
            //result.Add("subjectData", res);
            return Json(new { success = true, message = "SUCCESS", data = res }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetSubjectQuestionsCount()
        {
            //Dictionary<string, dynamic> result = new Dictionary<string, dynamic>();
            string connStr = ConfigurationManager
                   .ConnectionStrings["PostgresConn"]
                   .ConnectionString;
            string qry = "";

            qry = @"select sub_name as subject,count(*) as no_questions from public.questions_tbl group by sub_name;";


            var res = _db.Query<dynamic>(qry, CommandType.Text).ToList();
           
            return Json(new { success = true, message = "SUCCESS", data = res }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetDashboardData()
        {
            Dictionary<string, dynamic> result = new Dictionary<string, dynamic>();
            string connStr = ConfigurationManager
                   .ConnectionStrings["PostgresConn"]
                   .ConnectionString;
            string qry = "";

            string checkQuery = @"SELECT COUNT(1) FROM public.student_master";
            int cntStudent = _db.ExecuteScalar<int>(checkQuery);
            result.Add("cntStudent", cntStudent);
            
            checkQuery = @"SELECT COUNT(1) FROM public.exams_tbl";
            int cntExam = _db.ExecuteScalar<int>(checkQuery);
            result.Add("cntExam", cntExam);

            checkQuery = @"SELECT COUNT(1) FROM public.questions_tbl";
            int cntQuestions = _db.ExecuteScalar<int>(checkQuery);
            result.Add("cntQuestions", cntQuestions);

            checkQuery=@"SELECT
    ROUND(
        (COUNT(*) FILTER(WHERE is_pass = 'true') * 100.0) / COUNT(*), 
        2
    ) AS pass_percentage
FROM public.exam_student_mapping";
            double passPercentage = _db.ExecuteScalar<double>(checkQuery);
            result.Add("passPercentage", passPercentage);

            checkQuery = @"SELECT sub_name,count(*) FROM public.questions_tbl group by sub_name";
            var res = _db.Query<dynamic>(checkQuery, CommandType.Text).ToList();
            result.Add("subjectWiseCount", res);


            //qry = @"select sub_name as subject,count(*) as no_questions from public.questions_tbl group by sub_name;";


            //var res = _db.Query<dynamic>(qry, CommandType.Text).ToList();

            return Json(new { success = true, message = "SUCCESS", data = result }, JsonRequestBehavior.AllowGet);
        }



        // Here we write subject delete controller code
        [HttpPost]
        public ActionResult DeleteSubject(int subjectId)
        {
            try
            {
                if (subjectId <= 0)
                {
                    return Json(new { success = false, message = "Invalid subject id." });
                }

                string qry = "DELETE FROM subjects_tbl WHERE subject_id = '" + subjectId + "';";
                var res = _db.Query<dynamic>(qry, CommandType.Text);

                return Json(new
                {
                    success = true,
                    message = "Subject and all related data deleted successfully."
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Error while deleting subject.",
                    error = ex.Message
                });
            }
        }




        [HttpPost]
        public ActionResult DeleteExam(int exam_id)
        {
            try
            {
                if (exam_id <= 0)
                {
                    return Json(new { success = false, message = "Invalid Exam id." });
                }

                string qry = "DELETE FROM exams_tbl WHERE exam_id = '" + exam_id + "';";
                var res = _db.Query<dynamic>(qry, CommandType.Text);

                return Json(new
                {
                    success = true,
                    message = "Exam and all related data deleted successfully."
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Error while deleting Exam.",
                    error = ex.Message
                });
            }
        }

    }


}