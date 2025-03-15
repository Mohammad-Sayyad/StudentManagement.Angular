using System.Text.Json.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StackExchange.Redis;
using StudentManagement.API.Model;

namespace StudentManagement.API.RedisManager
{
    public class Redis
    {

        private readonly ConnectionMultiplexer _connection;
        private readonly IDatabase _database;
        private string _strConnection = "localhost:6379";


        public Redis()
        {
            _connection = ConnectionMultiplexer.Connect(_strConnection);
            _database = _connection.GetDatabase();
        }


        public async Task<bool> AddStudentToCashAsync(Student student)
        {
            var value = JsonConvert.SerializeObject(student);
            return await _database.StringSetAsync($"student: {student.Id}", value);
        }
        public async Task<IEnumerable<Student>> GetAllStudentsFromCacheAsync()
        {
            var studentsJson = await _database.StringGetAsync("students_list");
            if (string.IsNullOrEmpty(studentsJson))
            {
                return null;  
            }

            return JsonConvert.DeserializeObject<IEnumerable<Student>>(studentsJson);
        }
        public async Task<Student> GetStudentFromCacheAsync(int id)
        {

            var value = await _database.StringGetAsync($"Student: {id}");
            return value.IsNullOrEmpty ? null : JsonConvert.DeserializeObject<Student>(value);

        }

        public async Task<bool> RemoveStudentFromCacheAsync(int id)
        {
            return await _database.KeyDeleteAsync($"student : {id}");
        }

        public async Task ClearCache()
        {
            var server = _connection.GetServer(_strConnection);
            var keys = server.Keys(pattern: "student: *");

            foreach (var item in keys)
            {
                await _database.KeyDeleteAsync(item);
            }
        }

        public async Task<string> GetStudentFromCacheForDebugAsync()
        {
            var studentJson = await _database.StringGetAsync("sutdent_list");
            if (string.IsNullOrEmpty(studentJson))
            {
                return "No students found in cache.";
            }

            return studentJson;
        }
    }
}

