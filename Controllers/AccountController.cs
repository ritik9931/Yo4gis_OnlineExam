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
using System.Net.Http;
using System.Text;
using System.Net;
using System.IO;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using Newtonsoft.Json;

namespace onlineExam.Controllers
{
    public class AccountController : Controller
    {
        private readonly IDbConnection _db;
        public AccountController()
        {
            string conn = ConfigurationManager.ConnectionStrings["PostgresConn"].ConnectionString;
            _db = new NpgsqlConnection(conn);
            
        }

        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public async Task<ActionResult> Login(string username, string password)
        {
            string pass = Convert.ToString(System.Configuration.ConfigurationManager.AppSettings["admin"]);
            if (username == "admin" && password == pass)
            {
                Session["Username"] = username;
                return RedirectToAction("Index", "Home");
            }
            else
            {
                string tokenJwt = "";
                bool isValid = CheckUser(username, password, out tokenJwt);
                if (isValid)
                {
                    Session["Username"] = username;
                    string result = await GetStudentProfileAsync(tokenJwt, username);
                    var apiResponse = JsonConvert.DeserializeObject<ApiResponse>(result);

                    var student = apiResponse.Data;
                    Session["studentId"] =Convert.ToString(student.ID);
                    //return Json(new { success = true, message = "Login successful" });
                    return RedirectToAction("Index", "Student");
                }
                else
                {
                    return RedirectToAction("Login", "Account");
                    //return Json(new { success = false, message = "Invalid username or password" });
                }
            }
        }



        [HttpPost]
        public ActionResult Login1(string username, string password)
        {
            string pass =Convert.ToString(System.Configuration.ConfigurationManager.AppSettings["admin"]);
            if (username == "admin" && password == pass)
            {
                Session["Username"] = username;
                return RedirectToAction("Index", "Home");
            }
            else
            {
                string tokenJwt = "";
                bool isValid = CheckUser(username, password,out tokenJwt);
                if (isValid)
                {
                    Session["Username"] = username;
                    string result = GetStudentProfileAsync(tokenJwt, username).GetAwaiter().GetResult();
                    //return Json(new { success = true, message = "Login successful" });
                    return RedirectToAction("Index", "Student");
                }
                else
                {
                    return RedirectToAction("Login", "Account");
                    //return Json(new { success = false, message = "Invalid username or password" });
                }
            }

            //    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            //using (var reader = new StreamReader(response.GetResponseStream()))
            //{
            //    string result = reader.ReadToEnd();

            //    // Parse JSON
            //    var json = Newtonsoft.Json.Linq.JObject.Parse(result);

            //    // If access_token exists → valid login
            //    if (json["access_token"] != null)
            //    {
            //        Session["User"] = username;
            //        return RedirectToAction("Index", "Home");
            //    }

            //    else
            //    {
            //       // Session["User"] = username;
            //        return RedirectToAction("Login", "Account");
            //    }
            //}           

            // ViewBag.Error = "Invalid username or password";
            //return View();
        }

        public async Task<string> GetStudentProfileAsync(string token, string username)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("bearer", token);

                string url = $"http://yo4vts.in/bcscapi/api/bcsc/GetStudentProfileById?username={username}";

                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadAsStringAsync();
            }
        }


        public Boolean CheckUser(string username,string password,out string token)
        {
            token = string.Empty;
            var client = new HttpClient();
            var url = "https://yo4vts.in/bcscapi/token";
            string postData = $"username={HttpUtility.UrlEncode(username)}&password={HttpUtility.UrlEncode(password)}&grant_type=password";
            byte[] data = Encoding.UTF8.GetBytes(postData);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = data.Length;

            using (var stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }

            try
            {

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    string result = reader.ReadToEnd();
                    var json = Newtonsoft.Json.Linq.JObject.Parse(result);

                    if (json["access_token"] != null)
                    {
                        token = Convert.ToString(json["access_token"]);
                        return true;
                        //Session["User"] = json["username"];
                        //Session["token"] = json["access_token"];
                        //return RedirectToAction("Index", "Home");
                    }
                }
            }
            catch (Exception ex)
            {
                return false;
                //return RedirectToAction("Login", "Account");
            }
            return false;
        }

        //[HttpPost]
        //public ActionResult Register(string username, string password)
        //{
        //    string pass = Convert.ToString(System.Configuration.ConfigurationManager.AppSettings["admin"]);
        //    if (username == "admin" && password == pass)
        //    {
        //        Session["Username"] = username;
        //        return RedirectToAction("Index", "Home");
        //    }
        //    else
        //    {
        //        bool isValid = CheckUser(username, password);
        //        if (isValid)
        //        {
        //            Session["Username"] = username;
        //            //return Json(new { success = true, message = "Login successful" });
        //            return RedirectToAction("Index", "Home");
        //        }
        //        else
        //        {
        //            return RedirectToAction("Login", "Account");
        //            //return Json(new { success = false, message = "Invalid username or password" });
        //        }
        //    }

        //    //    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
        //    //using (var reader = new StreamReader(response.GetResponseStream()))
        //    //{
        //    //    string result = reader.ReadToEnd();

        //    //    // Parse JSON
        //    //    var json = Newtonsoft.Json.Linq.JObject.Parse(result);

        //    //    // If access_token exists → valid login
        //    //    if (json["access_token"] != null)
        //    //    {
        //    //        Session["User"] = username;
        //    //        return RedirectToAction("Index", "Home");
        //    //    }

        //    //    else
        //    //    {
        //    //       // Session["User"] = username;
        //    //        return RedirectToAction("Login", "Account");
        //    //    }
        //    //}           

        //    // ViewBag.Error = "Invalid username or password";
        //    //return View();
        //}

        [HttpPost]
        public ActionResult Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Error = "Invalid form data.";
                return View("SignUp");
            }

           

                // Check if username or email already exists
                string checkQuery = @"SELECT COUNT(1) FROM public.student_master WHERE name ='" + model.Username+"' OR emailid ='"+model.Email+"'";

                int exists = _db.ExecuteScalar<int>(checkQuery, new
                {
                    model.Username,
                    model.Email
                });

                if (exists > 0)
                {
                    ViewBag.Error = "Username or Email already exists.";
                    return Json(new { success = false, message = "User or Email already exist." });
            }

                // Hash password
               // string hashedPassword = HashPassword(model.Password);

                string insertQuery = @"INSERT INTO public.student_master 
                                   (name, mobile_number, password) 
                                   VALUES 
                                   (@Username, @Mobile, @Password)";

                _db.Execute(insertQuery, new
                {
                    //model.FullName,
                    model.Username,
                    model.Mobile,
                    model.Password
                });

                TempData["Success"] = "Account created successfully. Please login.";
                return Json(new { success = true, message = "User Created Successfully" });
            //}
        }


        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Login");
        }
    }
}