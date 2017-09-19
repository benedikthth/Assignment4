
using System.Collections.Generic;
using System.Linq;
using CoursesAPI.Models;
using CoursesAPI.Services.DataAccess;
using CoursesAPI.Services.Exceptions;
using CoursesAPI.Services.Models.Entities;

namespace CoursesAPI.Services.CoursesServices
{
	public class CoursesServiceProvider
	{
		private readonly IUnitOfWork _uow;

		private readonly IRepository<CourseInstance> _courseInstances;
		private readonly IRepository<TeacherRegistration> _teacherRegistrations;
		private readonly IRepository<CourseTemplate> _courseTemplates; 
		private readonly IRepository<Person> _persons;

		public CoursesServiceProvider(IUnitOfWork uow)
		{
			_uow = uow;

			_courseInstances      = _uow.GetRepository<CourseInstance>();
			_courseTemplates      = _uow.GetRepository<CourseTemplate>();
			_teacherRegistrations = _uow.GetRepository<TeacherRegistration>();
			_persons              = _uow.GetRepository<Person>();
		}

		/// <summary>
		/// You should implement this function, such that all tests will pass.
		/// </summary>
		/// <param name="courseInstanceID">The ID of the course instance which the teacher will be registered to.</param>
		/// <param name="model">The data which indicates which person should be added as a teacher, and in what role.</param>
		/// <returns>Should return basic information about the person.</returns>
		public PersonDTO AddTeacherToCourse(int courseInstanceID, AddTeacherViewModel model)
		{
			
			PersonDTO p = GetPersonBySSn(model.SSN);
			if(p == null){
				throw new AppObjectNotFoundException();
			}
			// TODO: implement this logic!
			CourseInstanceDTO course = GetCourseInstanceByCourseInstanceId(courseInstanceID);
			
			if(course == null){
				System.Console.WriteLine("THROWING");
				throw new AppObjectNotFoundException();
			}
			
			if(course.MainTeacher != ""){
				throw new AppValidationException("");
			}
			

			if(course.MainTeacher == ""){
				TeacherRegistration tr = new TeacherRegistration {
					Type = TeacherType.MainTeacher,
					SSN = model.SSN,
					CourseInstanceID = courseInstanceID
				};


				_teacherRegistrations.Add(tr);
				
				//var person = GetPersonBySSn(model.SSN);
				
				return GetPersonBySSn(tr.SSN);
			}

			return null;
		
		}

		public PersonDTO GetPersonBySSn(string SSN){
			var Person = (from p in _persons.All()
						  where p.SSN == SSN
						  select new PersonDTO{
							  Name = p.Name,
							  SSN = p.SSN
						  }).SingleOrDefault();
			return Person;
		}

		/// <summary>
		/// You should write tests for this function. You will also need to
		/// modify it, such that it will correctly return the name of the main
		/// teacher of each course.
		/// </summary>
		/// <param name="semester"></param>
		/// <returns></returns>
		public List<CourseInstanceDTO> GetCourseInstancesBySemester(string semester = null)
		{
			if (string.IsNullOrEmpty(semester))
			{
				semester = "20153";
			}

			var courses = (from c in _courseInstances.All()
				join ct in _courseTemplates.All() on c.CourseID equals ct.CourseID
				where c.SemesterID == semester
				select new CourseInstanceDTO
				{
					Name               = ct.Name,
					TemplateID         = ct.CourseID,
					CourseInstanceID   = c.ID,
					MainTeacher        = "" // Hint: it should not always return an empty string!
				}).ToList();

			return courses;
		}

		public string GetCourseMainTeacherSSN(int courseInstanceID){
			
			var ssn = (from s in _teacherRegistrations.All()
					   where s.CourseInstanceID == courseInstanceID &&
					   s.Type == TeacherType.MainTeacher
					   select s.SSN).SingleOrDefault();
			
			if(ssn == null){
				 return "";
			} else {
				return ssn;
			}

		}

		public CourseInstanceDTO GetCourseInstanceByCourseInstanceId(int ciid){
			var course = (from c in _courseInstances.All()
						  join ct in _courseTemplates.All() on c.CourseID equals ct.CourseID
						  where c.ID == ciid
						  select new CourseInstanceDTO{
							  Name = ct.Name,
							  TemplateID = ct.CourseID,
							  CourseInstanceID = c.ID,
							  MainTeacher = GetCourseMainTeacherSSN(ciid)
						  }).SingleOrDefault();
			

			return course;
		}

	}
}
