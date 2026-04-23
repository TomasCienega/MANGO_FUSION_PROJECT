using MangoFusion_API.Models;
using MangoFusion_API.Models.Dto;
using MangoFusion_API.Utility;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;

namespace MangoFusion_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApiResponse _response;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly string _secretKey;

        public AuthController(ApiResponse response, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration)
        {
            _response = response;
            _userManager = userManager;
            _roleManager = roleManager;
            _secretKey = configuration.GetValue<string>("ApiSettings:Secret") ?? "";
        }

        #region
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDTO model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    ApplicationUser newUser = new()
                    {
                        Email = model.Email,
                        UserName = model.Email,
                        Name = model.Name,
                        NormalizedEmail = model.Email.ToUpper()
                    };
                    var result = await _userManager.CreateAsync(newUser, model.Password);
                    if (result.Succeeded)
                    {
                        if (!_roleManager.RoleExistsAsync(StaticDetails.Role_Admin).GetAwaiter().GetResult())
                        {
                            await _roleManager.CreateAsync(new IdentityRole(StaticDetails.Role_Admin));
                            await _roleManager.CreateAsync(new IdentityRole(StaticDetails.Role_Customer));
                        }
                        if (model.Role.Equals(StaticDetails.Role_Admin, StringComparison.CurrentCultureIgnoreCase))
                        {
                            await _userManager.AddToRoleAsync(newUser, StaticDetails.Role_Admin);
                        }
                        else
                        {
                            await _userManager.AddToRoleAsync(newUser, StaticDetails.Role_Customer);
                        }
                        _response.StatusCode = HttpStatusCode.OK;
                        _response.IsSuccess = true;
                        return Ok(_response);
                    }
                    else
                    {
                        foreach (var error in result.Errors)
                        {
                            _response.ErrorMessage.Add(error.Description);
                        }
                        _response.StatusCode = HttpStatusCode.BadRequest;
                        _response.IsSuccess = false;
                        return BadRequest(_response);
                    }
                }
                else
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    foreach (var error in ModelState.Values)
                    {
                        foreach (var item in error.Errors)
                        {
                            _response.ErrorMessage.Add(item.ErrorMessage);
                        }
                    }
                    return BadRequest(_response);
                }
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.InternalServerError;
                _response.ErrorMessage = [ex.Message];
                return StatusCode(500, _response);
            }
        }
        #endregion

        #region
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var userFromDb = await _userManager.FindByEmailAsync(model.Email);
                    if (userFromDb != null)
                    {
                        bool isValid = await _userManager.CheckPasswordAsync(userFromDb, model.Password);
                        if (!isValid)
                        {
                            _response.Result = new LoginResponseDTO();
                            _response.StatusCode = HttpStatusCode.BadRequest;
                            _response.IsSuccess = false;
                            _response.ErrorMessage.Add("Invalid Credentials");
                            return BadRequest(_response);
                        }
                        JwtSecurityTokenHandler tokenHandler = new();
                        byte[] key = Encoding.ASCII.GetBytes(_secretKey);

                        SecurityTokenDescriptor tokenDescriptor = new()
                        {
                            Subject = new ClaimsIdentity(
                             [
                                new ("fullname", userFromDb.Name),
                                new ("id",userFromDb.Id),
                                new (ClaimTypes.Email,userFromDb.Email!.ToString()),
                                new (ClaimTypes.Role, (await _userManager.GetRolesAsync(userFromDb)).FirstOrDefault()!),
                             ]),
                            Expires = DateTime.UtcNow.AddDays(7),
                            SigningCredentials = new(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                        };
                        SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);

                        LoginResponseDTO loginResponse = new()
                        {
                            Email = userFromDb.Email,
                            Token = tokenHandler.WriteToken(token),
                            Role = (await _userManager.GetRolesAsync(userFromDb)).FirstOrDefault()!
                        };
                        _response.Result = loginResponse;
                        _response.StatusCode = HttpStatusCode.OK;
                        _response.IsSuccess = true;
                        return Ok(_response);
                    }
                    _response.Result = new LoginResponseDTO();
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.IsSuccess = false;
                    _response.ErrorMessage.Add("Invalid Credentials");
                    return BadRequest(_response);
                }
                else
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    foreach (var error in ModelState.Values)
                    {
                        foreach (var item in error.Errors)
                        {
                            _response.ErrorMessage.Add(item.ErrorMessage);
                        }
                    }
                    return BadRequest(_response);
                }
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.InternalServerError;
                _response.ErrorMessage = [ex.Message];
                return StatusCode(500, _response);
            }
        }
        #endregion
    }
}