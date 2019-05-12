﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ACRS.Data;
using ACRS.Models;

namespace ACRS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CoursesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CoursesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Courses
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Course>>> GetCourses()
        {
                       return await _context.Courses
                         .Include(e => e.Prerequisites)
                      .ToListAsync();

        }

        // GET: api/Courses
        [HttpGet("{id}/eligable")]
        public async Task<ActionResult<IEnumerable<StudentEligability>>> GetEligableCourseByCourseId(string id)
        {
            return await GetEligableCourseByCourseIdAsync(id);

        }

        // GET: api/Courses
        [HttpGet("{id}/eligable")]
        public async Task<ActionResult<IEnumerable<StudentEligability>>> GetCourseInEligabilityByCourseId(string id)
        {
            return await GetInEligableStudentByCourseIdAsync(id);

        }


        // GET: api/Courses/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Course>> GetCourse(string id)

        {
            var c = await _context.Courses.Include(e => e.Prerequisites).FirstOrDefaultAsync(s => s.CourseId == id);

            if (c == null)
            {
                return NotFound();
            }

            return c;

        }

        // PUT: api/Courses/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCourse(string id, Course course)
        {
            if (id == null)
            {
                return NotFound();
            }
            var c = await _context.Courses.Include(e => e.Prerequisites).FirstOrDefaultAsync(s => s.CourseId == id);
            if(c.Prerequisites!= null)
            {
                _context.Prerequisites.RemoveRange(c.Prerequisites);

            }

            c.Prerequisites = course.Prerequisites;
            _context.Entry(c).CurrentValues.SetValues(course);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // POST: api/Courses
        [HttpPost]
        public async Task<ActionResult<Course>> PostCourse(Course course)
        {
            _context.Courses.Add(course);
            await _context.SaveChangesAsync();
            return CreatedAtAction("GetCourse", new { id = course.CourseId }, course);
        }

        // DELETE: api/Courses/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Course>> DeleteCourse(string id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null)
            {
                return NotFound();
            }
            var c = await _context.Courses.Include(e => e.Prerequisites).FirstOrDefaultAsync(s => s.CourseId == id);
            foreach(Prerequisite pr in c.Prerequisites)
            {
                _context.Prerequisites.Remove(pr);
            }
            await _context.SaveChangesAsync();

            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();

            return course;
        }

        private bool CourseExists(string id)
        {
            return _context.Courses.Any(e => e.CourseId == id);
        }




        public async Task<List<StudentEligability>> GetEligableCourseByCourseIdAsync(string CourseID)
        {
            List<Course> courses = await _context.Courses.Include(o => o.Prerequisites).ToListAsync();
            List<Grade> grades = await _context.Grades.ToListAsync();
            List<Student> students = await _context.Students.ToListAsync();
            var studentMap = new Dictionary<string, int>();
            List<StudentEligability> eligableStudents = new List<StudentEligability>();
            Course targetCourse = courses.FirstOrDefault(o => o.CourseId == CourseID);
            int prereqs = 0;
            if (targetCourse.Prerequisites != null)
            {
                prereqs = targetCourse.Prerequisites.Count();
            }

            //TODO move to grades loop
            foreach (Student s in students)
            {
                studentMap[s.StudentId] = 0;
            }
       

            //Loop through all grades
            foreach (Grade g in grades)
            {
                if (g.FinalGrade >= targetCourse.PassingGrade)
                {
                    studentMap[g.StudentId] = studentMap[g.StudentId] + 1;
                }
            }

            //Loop through all students
            foreach (Student s in students)
            {
                if (studentMap[s.StudentId] >= prereqs)
                {
                    eligableStudents.Add(new StudentEligability(s.StudentId, targetCourse.CourseId, true));
                } 

            }
           
            return eligableStudents;
        }

        public async Task<List<StudentEligability>> GetInEligableStudentByCourseIdAsync(string CourseID)
        {
            List<Course> courses = await _context.Courses.Include(o => o.Prerequisites).ToListAsync();
            List<Grade> grades = await _context.Grades.ToListAsync();
            List<Student> students = await _context.Students.ToListAsync();
            var studentMap = new Dictionary<string, int>();
            List<StudentEligability> ineligableStudents = new List<StudentEligability>();
            Course targetCourse = courses.FirstOrDefault(o => o.CourseId == CourseID);
            int prereqs = 0;
            if (targetCourse.Prerequisites != null)
            {
                prereqs = targetCourse.Prerequisites.Count();
            }

            //TODO move to grades loop
            foreach (Student s in students)
            {
                studentMap[s.StudentId] = 0;
            }


            //Loop through all grades
            foreach (Grade g in grades)
            {
                if (g.FinalGrade >= targetCourse.PassingGrade)
                {
                    studentMap[g.StudentId] = studentMap[g.StudentId] + 1;
                }
            }

            //Loop through all students
            foreach (Student s in students)
            {
                if (studentMap[s.StudentId] < prereqs)
                {
                    ineligableStudents.Add(new StudentEligability(s.StudentId, targetCourse.CourseId, true));
                }

            }

            return ineligableStudents;
        }

        public async Task<List<List<StudentEligability>>> GetEligableStudentsAllCourses()
        {
            List<List<StudentEligability>> lists = new List<List<StudentEligability>>();
            List<Course> courses = await _context.Courses.Include(o => o.Prerequisites).ToListAsync();

            foreach(Course c in courses)
            {
                List<StudentEligability> l = await GetEligableCourseByCourseIdAsync(c.CourseId);
                lists.Add(l);
            }
            return lists;
        }

    }
}
