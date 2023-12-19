using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Http.Description;
using BackAlmacen.Models;

namespace BackAlmacen.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class UsersController : ApiController
    {
        private Model2 db = new Model2();

        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password, BCrypt.Net.BCrypt.GenerateSalt());
        }

        private bool VerifyPassword(string password, string hashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }

        // GET: api/Users
        public IQueryable<Users> GetUsers()
        {
            return db.Users;
        }

        // GET: api/Users/5
        [ResponseType(typeof(Users))]
        public IHttpActionResult GetUsers(int id)
        {
            Users users = db.Users.Find(id);
            if (users == null)
            {
                return NotFound();
            }

            return Ok(users);
        }

        // POST: api/Users
        [ResponseType(typeof(Users))]
        [Route("api/Users/Register")]
        [HttpPost]
        public IHttpActionResult Register(string username, string password)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Verificar si el nombre de usuario ya está en uso
            if (IsUsernameTaken(username))
            {
                return BadRequest("El nombre de usuario ya está en uso. Por favor, elija otro nombre de usuario.");
            }

            string hashedPassword = HashPassword(password);

            Random random = new Random();
            int walletNumber = random.Next(200, 1000);

            // Crear el objeto Users con los datos proporcionados
            Users newUser = new Users
            {
                username = username,
                password = hashedPassword,
                wallet = walletNumber,
                type = "user"
            };

            db.Users.Add(newUser);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = newUser.id }, newUser);
        }

        // Método para verificar si el nombre de usuario ya está en uso
        private bool IsUsernameTaken(string username)
        {
            return db.Users.Any(u => u.username == username);
        }

        [ResponseType(typeof(string))]
        [Route("api/Users/Authenticate")]
        [HttpPost]
        public IHttpActionResult Authenticate(string username, string password)
        {
            string hashedPasswordFromDatabase, saltFromDatabase;
            double walletNumberFromDatabase;
            GetUserCredentials(username, out hashedPasswordFromDatabase, out saltFromDatabase, out walletNumberFromDatabase);

            if (hashedPasswordFromDatabase == null)
            {
                return BadRequest("Usuario no encontrado");
            }

            if (VerifyPassword(password, hashedPasswordFromDatabase))
            {
                string authToken = GenerateAuthToken(username);
                return Ok(authToken);
            }

            return BadRequest("Credenciales inválidas");
        }

        private void GetUserCredentials(string username, out string hashedPassword, out string salt, out double wallet)
        {
            hashedPassword = null;
            salt = null;
            wallet = 0.0;

            using (Model2 db = new Model2())
            {
                string connectionString = db.Database.Connection.ConnectionString;

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string selectQuery = "SELECT password, wallet FROM Users WHERE username = @username";
                    using (SqlCommand command = new SqlCommand(selectQuery, connection))
                    {
                        command.Parameters.AddWithValue("@username", username);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                hashedPassword = reader.GetString(0);
                                wallet= reader.GetDouble(1);
                            }
                        }
                    }
                }
            }
        }

        private string GenerateAuthToken(string username)
        {
            return $"Token-{username}-{Guid.NewGuid()}";
        }

        [ResponseType(typeof(UserInfo))]
        [Route("api/Users/GetUserByToken")]
        [HttpGet]
        public IHttpActionResult GetUserByToken(string token)
        {
            string username = GetUsernameFromToken(token);

            if (username == null)
            {
                return BadRequest("Token inválido");
            }

            Users user = db.Users.SingleOrDefault(u => u.username == username);

            if (user == null)
            {
                return NotFound();
            }

            // Crear un objeto UserInfo con la información del usuario
            UserInfo userInfo = new UserInfo
            {
                username = user.username,
                wallet = user.wallet,
                type = user.type
            };

            return Ok(userInfo);
        }

        private string GetUsernameFromToken(string token)
        {
            if (token.StartsWith("Token-"))
            {
                string[] parts = token.Split('-');
                return parts[1];
            }

            return null;
        }

        public class UserInfo
        {
            public string username { get; set; }
            public double? wallet { get; set; }
            public string type { get; set; }
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool UsersExists(int id)
        {
            return db.Users.Count(e => e.id == id) > 0;
        }
    }
}