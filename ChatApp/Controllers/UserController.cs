using ChatApp.Data;
using ChatApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Win32;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace ChatApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;
        private IConfiguration _config;
        public UserController(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        [HttpGet]
        public IActionResult GetAllUser()
        {
            return Ok(new { users = _context.Users.Select(each => new { each.Id, each.Name, each.Email }), msg = "List of All users fetched" });
        }

        [HttpPost("getbyid")]
        public async Task<object> GetById([FromBody] int id)
        {
            var user =  _context.Users.FirstOrDefault(u => u.Id == id);
            if (user == null)
            {
                return BadRequest(new { users = user, msg = "User not Found!" });
            }
            var userDto = new
            {
                user.Id,
                user.Name,
                user.Email,
            };
            return Ok(new { users = userDto, msg = "User Found!" });
        }

        [HttpPost("register")]
        public async Task<object> Register([FromBody] Registration req)
        {
            var emailExist = _context.Users.FirstOrDefault(each => each.Email == req.Email);
            if (emailExist != null)
            {
                return BadRequest("Email Already Exist!");
            }
            else
            {
                CreatePasswordHash(req.Password, out byte[] PHash, out byte[] PSalt);

                var user = new User
                {
                    Id = 0,
                    Email = req.Email,
                    Name = req.Name,
                    PasswordHash = PHash,
                    PasswordSalt = PSalt
                };
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                return Ok(new { users = _context.Users, msg = "New User Registered!" });
            }
        }

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        [HttpPost("login")]
        public async Task<object> Login([FromBody] Login req)
        {
            var emailExist = _context.Users.FirstOrDefault(each => each.Email == req.Email);
            if (emailExist == null)
            {
                return NotFound("User Does not Exist!");
            }
            else
            {
                if (!VerifyPasswordHash(req.Password, emailExist.PasswordHash, emailExist.PasswordSalt))
                {
                    return BadRequest("Fake Credentials!");
                }

                //If login usrename and password are correct then proceed to generate token

                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
                var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

                var myclaims = new[]
                {
                    new Claim(ClaimTypes.Sid, emailExist.Id.ToString()),
                    new Claim(ClaimTypes.NameIdentifier, emailExist.Name),
                    new Claim(ClaimTypes.Email, emailExist.Email),
                };

                var Sectoken = new JwtSecurityToken(
                  issuer: _config["Jwt:Issuer"],
                  audience: _config["Jwt:Audience"],
                  claims: myclaims,
                  expires: DateTime.Now.AddMinutes(120),
                  signingCredentials: credentials);

                string token = new JwtSecurityTokenHandler().WriteToken(Sectoken);

                var newSession = new Session
                {
                    UserId = emailExist.Id
                };

                await _context.Sessions.AddAsync(newSession);
                await _context.SaveChangesAsync();

                return Ok(new { mytoken=token });
                //return Ok(new { success = true, msg = $"Welcome! {emailExist.Name}" });
            }
        }

        private bool VerifyPasswordHash(string password, byte[] Hash, byte[] Salt)
        {
            using (var hmac = new HMACSHA512(Salt))
            {
                var myHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return myHash.SequenceEqual(Hash);
            }

        }

        [Authorize]
        [HttpGet("getbytoken")]
        public IActionResult GetbByToken()
        {
            //var userid = User.FindFirst(ClaimTypes.Sid)?.Value;
            var userIdClaim = User.FindFirst(ClaimTypes.Sid)?.Value;
            int.TryParse(userIdClaim, out int userId);
            var username = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userEmailClaim = User.FindFirst(ClaimTypes.Email)?.Value;
            // Access other claims as needed

            return Ok(new {UserId= userId, Username = username, Email = userEmailClaim });
            //return Ok("hello world");
            //return Ok(new { users = userDto, msg = "User Found!" });
        }
    }
}
