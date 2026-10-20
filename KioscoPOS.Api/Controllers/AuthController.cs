using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using KioscoPOS.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace KioscoPOS.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
  private readonly UserManager<ApplicationUser> _userManager;
  private readonly IConfiguration _config;

  public AuthController(UserManager<ApplicationUser> userManager, IConfiguration config)
  {
    _userManager = userManager;
    _config = config;
  }

  public record LoginRequest(string Email, string Password);
  public record LoginResponse(string Token, string Email, string NombreCompleto, string[] Roles, DateTime Expira);

  [HttpPost("login")]
  public async Task<IActionResult> Login([FromBody] LoginRequest req)
  {
    if (string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Password))
      return BadRequest(new { mensaje = "Email y contraseña son obligatorios" });

    //vbaalidamos crendeciales con Identity
    var user = await _userManager.FindByEmailAsync(req.Email);
    if (user == null || !await _userManager.CheckPasswordAsync(user, req.Password))
      return Unauthorized(new { mensaje = "Email o contraseña incorrectos" });

    var roles = await _userManager.GetRolesAsync(user);
    var expira = DateTime.UtcNow.AddHours(8);

    //claims que van dentro del token
    var claims = new List<Claim>
    {
      new(JwtRegisteredClaimNames.Sub, user.Id),
new(JwtRegisteredClaimNames.Email, user.Email!),
new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
new("nombreCompleto", user.NombreCompleto ?? string.Empty)
    };
    foreach (var rol in roles)
      claims.Add(new Claim(ClaimTypes.Role, rol));

    //firmar
    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    var token = new JwtSecurityToken(
      issuer: _config["Jwt:Issuer"],
      audience: _config["Jwt:Audience"],
      claims: claims,
      expires: expira,
      signingCredentials: creds);

    return Ok(new LoginResponse(
      new JwtSecurityTokenHandler().WriteToken(token),
      user.Email!,
      user.NombreCompleto ?? string.Empty,
      roles.ToArray(),
      expira));
  }
}