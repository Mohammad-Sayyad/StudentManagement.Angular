using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using StudentManagement.API.Data;
using StudentManagement.API.Model;
using StudentManagement.API.RedisManager;


namespace StudentManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentController : ControllerBase
    {

        private readonly AppDbContext _context;
        private readonly Redis _redisManager;

        public StudentController(AppDbContext context , Redis redis)
        {
            this._context = context;
            this._redisManager = redis;
        }


        [HttpGet]
        public async Task<ActionResult<IEnumerable<Student>>> GetStudents()
        {
            var students = await _redisManager.GetAllStudentsFromCacheAsync();
            if (students != null && students.Any())
            {
                return Ok(students);
            }

            var dbStudents = await _context.Students.ToListAsync();
            foreach (var item in dbStudents)
            {
                await _redisManager.AddStudentToCashAsync(item);
            }

            return dbStudents;
        }

        
        [HttpGet("{id}")]

        public async Task<ActionResult<Student>> GetStudent(int id)
        {

            var sttudent = await _redisManager.GetStudentFromCacheAsync(id);
            if(sttudent != null)
            {
                return Ok(sttudent);
            }
            var dbStudent =  await _context.Students.FindAsync(id);
            if (id == null)
            {
                return NotFound();
            }
            await _redisManager.AddStudentToCashAsync(dbStudent);

            return Ok(dbStudent);
        }

        [HttpPost]
        public async Task<ActionResult<Student>> AddStudent(Student student)
        {
            _context.Students.Add(student);
            await _context.SaveChangesAsync();

            await _redisManager.AddStudentToCashAsync(student);  
            return CreatedAtAction(nameof(GetStudent), new { id = student.Id }, student);
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> PutStudent(int id, [FromBody] Student student)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != student.Id)
                return BadRequest();

            _context.Entry(student).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            await _redisManager.AddStudentToCashAsync(student);
            return NoContent();
        }




        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStudent(int id)
        {
            var student = await _context.Students.FindAsync(id);
            if (student == null)
                return NotFound();

            _context.Students.Remove(student);
            await _context.SaveChangesAsync();

            await _redisManager.RemoveStudentFromCacheAsync(id);
            return NoContent();
        }
    }
}
