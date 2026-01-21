using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace onlineExam.Models
{
    public class RegisterViewModel
    {
        [Required]
        public string FullName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Username { get; set; }

        [Required]
        public string Mobile { get; set; }

        [Required]
        [MinLength(6)]
        public string Password { get; set; }

        [Required]
        [Compare("Password")]
        public string ConfirmPassword { get; set; }
    }

    public class QuestionRequestModel
    {
        public List<QuestionModel> questions { get; set; }
        //public string exam_name { get; set; }
        public string subject_name { get; set; }
    }

    public class SubjectClass
    {
        public string sub_name { get; set; }
        //public string exam_name { get; set; }
        public string sub_code { get; set; }
    }

    public class ExamQuestions
    {
        public string exam_name { get; set; }
        //public string exam_name { get; set; }
        public string question_id { get; set; }
        public string subject_id { get; set; }
    }

    public class ExamStudent
    {
        public string exam_id { get; set; }
        //public string exam_name { get; set; }
        public string student_id { get; set; }
        
    }

    public class QuestionModel
    {
        public int id { get; set; }
        public string question_text { get; set; }
        public string option_a { get; set; }
        public string option_b { get; set; }
        public string option_c { get; set; }
        public string option_d { get; set; }
        public string correct_option { get; set; }
        public string explanation { get; set; }
        public int marks { get; set; }
        public string difficulty { get; set; }
        public string category { get; set; }
        //public int Exam_id { get; set; }
        public string subject { get; set; }
    }

    public class StudentAnswer
    {
        public int QuestionId { get; set; }
        public string SelectedOption { get; set; }
        public string ExamId { get; set; }
        public string StudentId { get; set; }
    }

}