using onlineExam.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace onlineExam.Controllers
{
    public class StudentController : Controller
    {
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
    }
}