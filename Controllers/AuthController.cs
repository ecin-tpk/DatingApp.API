using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.API.Data;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace DatingApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        //    private readonly IAuthRepository _repo;

        //    private readonly IConfiguration _config;

        //    private readonly IMapper _mapper;

        //    public AuthController(IAuthRepository repo, IConfiguration config, IMapper mapper)
        //    {
        //        _repo = repo;
        //        _config = config;
        //        _mapper = mapper;
        //    }

        //    // POST: Register
        //    [HttpPost("register")]
        //    public async Task<IActionResult> Register(UserForRegisterDto userForRegisterDto)
        //    {
        //        if (await _repo.UserExists(userForRegisterDto.Email))
        //        {
        //            return BadRequest("This email is already connected to an account.");
        //        }

        //        var userToCreate = _mapper.Map<User>(userForRegisterDto);

        //        var createdUser = await _repo.Register(userToCreate, userForRegisterDto.Password);

        //        var userToReturn = _mapper.Map<UserForDetailedDto>(createdUser);

        //        return CreatedAtRoute("GetUser", new { controller = "Users", id = createdUser.Id }, userToReturn);
        //    }

        //    // POST: Login for normal user
        //    [HttpPost("login")]
        //    public async Task<IActionResult> Login(UserForLoginDto userForLoginDto)
        //    {
        //        var userFromRepo = await _repo.Login(userForLoginDto.Email, userForLoginDto.Password);

        //        if (userFromRepo == null)
        //        {
        //            return Unauthorized();
        //        }

        //        var claims = new[]
        //        {
        //            new Claim(ClaimTypes.NameIdentifier, userFromRepo.Id.ToString()),
        //            new Claim(ClaimTypes.Name, userFromRepo.Name),
        //            new Claim(ClaimTypes.Role, userFromRepo.Role)
        //        };

        //        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.GetSection("AppSettings:Secret").Value));

        //        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

        //        var tokenDescriptor = new SecurityTokenDescriptor
        //        {
        //            Subject = new ClaimsIdentity(claims),
        //            Expires = DateTime.Now.AddDays(1),
        //            SigningCredentials = creds
        //        };

        //        var tokenHandler = new JwtSecurityTokenHandler();

        //        var token = tokenHandler.CreateToken(tokenDescriptor);

        //        var user = _mapper.Map<UserForListDto>(userFromRepo);

        //        return Ok(new
        //        {
        //            token = tokenHandler.WriteToken(token),
        //            user
        //        });
        //    }

        //    // POST: Login for admin
        //    [HttpPost("login/admin")]
        //    public async Task<IActionResult> LoginForAdmin(UserForLoginDto userForLoginDto)
        //    {
        //        var userFromRepo = await _repo.Login(userForLoginDto.Email, userForLoginDto.Password);

        //        if (userFromRepo == null)
        //        {
        //            return Unauthorized();
        //        }

        //        if (userFromRepo.Role != "admin")
        //        {
        //            return Forbid();
        //        }

        //        var claims = new[]
        //        {
        //            new Claim(ClaimTypes.NameIdentifier, userFromRepo.Id.ToString()),
        //            new Claim(ClaimTypes.Name, userFromRepo.Name),
        //            new Claim(ClaimTypes.Role, userFromRepo.Role)
        //        };

        //        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.GetSection("AppSettings:Token").Value));

        //        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

        //        var tokenDescriptor = new SecurityTokenDescriptor
        //        {
        //            Subject = new ClaimsIdentity(claims),
        //            Expires = DateTime.Now.AddDays(1),
        //            SigningCredentials = creds
        //        };

        //        var tokenHandler = new JwtSecurityTokenHandler();

        //        var token = tokenHandler.CreateToken(tokenDescriptor);

        //        var user = _mapper.Map<UserForAdmin>(userFromRepo);

        //        return Ok(new
        //        {
        //            token = tokenHandler.WriteToken(token),
        //            user
        //        });
        //    }
    }
}

