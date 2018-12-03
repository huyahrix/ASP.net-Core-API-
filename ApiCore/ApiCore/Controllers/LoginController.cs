using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ApiCore.Models;
using ApiCore.Interfaces;
using AutoMapper;
using ApiCore.Helpers;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Newtonsoft.Json;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using ApiCore.DTO;

namespace ApiCore.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class LoginController : ControllerBase
    {
        //private readonly NorthwindContext _context;

        //public LoginController(NorthwindContext context)
        //{
        //    _context = context;
        //}

        private IUserService _userService;
        private IMapper _mapper;
        private readonly AppSettings _appSettings;

        public LoginController(IUserService userService, IMapper mapper, IOptions<AppSettings> appSettings)
        { 
            _userService = userService;
            _mapper = mapper;
            _appSettings = appSettings.Value;
        }

        [AllowAnonymous]
        [HttpPost]
        public IActionResult Authenticate([FromBody]UserDTO userDto)
        {
            var user = _userService.Authenticate(userDto.UserName, userDto.PassWord);
            if (user == null)
                return BadRequest(new { message = "Username or password is incorrect" });

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, user.UserId.ToString())
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            // return basic user info (without password) and token to store client side
            return Ok(new
            {
                Id = user.UserId,
                Username = user.UserName,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Token = tokenString
            });
        }

        //[HttpGet]
        //public IActionResult GetUserInfo()
        //{
        //    var claimsIdentity = User.Identity as ClaimsIdentity;

        //    return Ok(JsonConvert.SerializeObject(new
        //    {
        //        UserName = claimsIdentity.Name,
        //        Claims = claimsIdentity.Claims
        //    }));
        //}

        //GET: api/Login
        [HttpGet]
        public IEnumerable<Users> GetUsers()
        {
            return _userService.GetAll();
        }
    }
}