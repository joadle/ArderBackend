using Microsoft.AspNetCore.Mvc;
using Npgsql;
using ArderBackend.Models;

namespace ArderBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExercisesController : ControllerBase
    {
        private readonly string _connectionString;

        public ExercisesController(IConfiguration configuration)
        {
            _connectionString = Helpers.DatabaseHelper.GetConnectionString(configuration);
        }

        [HttpGet("filters/{column}")]
        public async Task<ActionResult<List<string>>> GetDistinctValuesAsync(string column)
        {
            var values = new List<string>();
            try
            {
                using var conn = new NpgsqlConnection(_connectionString);
                await conn.OpenAsync();

                if (column != "target" && column != "equipment" && column != "category")
                {
                    return BadRequest("Columna de filtro no válida.");
                }

                using var cmd = new NpgsqlCommand($"SELECT DISTINCT {column} FROM Exercise WHERE {column} IS NOT NULL ORDER BY {column};", conn);
                using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    string value = reader.GetString(0);
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        string formattedValue = char.ToUpper(value[0]) + value.Substring(1);
                        values.Add(formattedValue);
                    }
                }
                return Ok(values);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error interno del servidor: " + ex.Message);
            }
        }

        [HttpGet]
        public async Task<ActionResult<List<Exercise>>> GetExercisesAsync([FromQuery] int limit = 20, [FromQuery] int offset = 0, [FromQuery] string? searchText = null, [FromQuery] List<string>? muscles = null, [FromQuery] List<string>? equipments = null, [FromQuery] List<string>? categories = null)
        {
            var exercises = new List<Exercise>();
            try
            {
                using var conn = new NpgsqlConnection(_connectionString);
                await conn.OpenAsync();

                var query = "SELECT id, name, target, equipment, category FROM Exercise WHERE 1=1";
                using var cmd = new NpgsqlCommand();
                cmd.Connection = conn;

                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    query += " AND name ILIKE @search";
                    cmd.Parameters.AddWithValue("search", $"%{searchText}%");
                }

                if (muscles != null && muscles.Any())
                {
                    query += " AND target = ANY(@muscles)";
                    cmd.Parameters.AddWithValue("muscles", muscles.Select(m => m.ToLower()).ToArray());
                }

                if (equipments != null && equipments.Any())
                {
                    query += " AND equipment = ANY(@equipments)";
                    cmd.Parameters.AddWithValue("equipments", equipments.Select(e => e.ToLower()).ToArray());
                }

                if (categories != null && categories.Any())
                {
                    query += " AND category = ANY(@categories)";
                    cmd.Parameters.AddWithValue("categories", categories.Select(c => c.ToLower()).ToArray());
                }

                query += " ORDER BY name LIMIT @limit OFFSET @offset";
                cmd.Parameters.AddWithValue("limit", limit);
                cmd.Parameters.AddWithValue("offset", offset);

                cmd.CommandText = query;

                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    string id = reader.IsDBNull(0) ? "" : reader.GetString(0);
                    string target = reader.IsDBNull(2) ? "" : reader.GetString(2);
                    string equipment = reader.IsDBNull(3) ? "" : reader.GetString(3);
                    string category = reader.IsDBNull(4) ? "" : reader.GetString(4);

                    exercises.Add(new Exercise
                    {
                        Id = id,
                        Name = reader.IsDBNull(1) ? "" : reader.GetString(1),
                        ImageUrl = $"https://minio-production-2f20.up.railway.app/ejercicios/360/{id}.gif",
                        MainMuscle = string.IsNullOrEmpty(target) ? "" : char.ToUpper(target[0]) + target.Substring(1),
                        Equipment = string.IsNullOrEmpty(equipment) ? "" : char.ToUpper(equipment[0]) + equipment.Substring(1),
                        Type = string.IsNullOrEmpty(category) ? "" : char.ToUpper(category[0]) + category.Substring(1)
                    });
                }
                return Ok(exercises);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error interno del servidor: " + ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Exercise>> GetExerciseByIdAsync(string id)
        {
            try
            {
                using var conn = new NpgsqlConnection(_connectionString);
                await conn.OpenAsync();

                using var cmd = new NpgsqlCommand("SELECT id, name, target, equipment, category, body_part, secondary_muscles, instructions, description, difficulty FROM Exercise WHERE id = @id;", conn);
                cmd.Parameters.AddWithValue("id", id);

                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    string target = reader.IsDBNull(2) ? "" : reader.GetString(2);
                    string equipment = reader.IsDBNull(3) ? "" : reader.GetString(3);
                    string category = reader.IsDBNull(4) ? "" : reader.GetString(4);

                    var secondaryMuscles = reader.IsDBNull(6) ? Array.Empty<string>() : reader.GetFieldValue<string[]>(6);
                    var instructions = reader.IsDBNull(7) ? Array.Empty<string>() : reader.GetFieldValue<string[]>(7);

                    return Ok(new Exercise
                    {
                        Id = reader.IsDBNull(0) ? "" : reader.GetString(0),
                        Name = reader.IsDBNull(1) ? "" : reader.GetString(1),
                        ImageUrl = $"https://minio-production-2f20.up.railway.app/ejercicios/360/{id}.gif",
                        MainMuscle = string.IsNullOrEmpty(target) ? "" : char.ToUpper(target[0]) + target.Substring(1),
                        Equipment = string.IsNullOrEmpty(equipment) ? "" : char.ToUpper(equipment[0]) + equipment.Substring(1),
                        Type = string.IsNullOrEmpty(category) ? "" : char.ToUpper(category[0]) + category.Substring(1),
                        BodyPart = reader.IsDBNull(5) ? "" : reader.GetString(5),
                        SecondaryMusclesList = new List<string>(secondaryMuscles),
                        Instructions = new List<string>(instructions),
                        Description = reader.IsDBNull(8) ? "" : reader.GetString(8),
                        Difficulty = reader.IsDBNull(9) ? "" : reader.GetString(9)
                    });
                }
                return NotFound("No se encontró el ejercicio.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error interno del servidor: " + ex.Message);
            }
        }
    }
}
