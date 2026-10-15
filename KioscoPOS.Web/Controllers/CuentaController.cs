using KioscoPOS.Web.Models;
using KioscoPOS.Web.ViewModels;
using KioscoPOS.Web.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace KioscoPOS.Web.Controllers;

public class CuentaController : Controller
{
  private readonly SignInManager<ApplicationUser> _signInManager;
  private readonly UserManager<ApplicationUser> _userManager;
  private readonly IWebHostEnvironment _env;

  private const long TamanoMaximoAvatar = 2 * 1024 * 1024;
  private static readonly string[] ExtensionesPermitidas = { ".jpg", ".jpeg", ".png", ".webp" };

  public CuentaController(
    SignInManager<ApplicationUser> signInManager,
    UserManager<ApplicationUser> userManager,
    IWebHostEnvironment env)
  {
    _signInManager = signInManager;
    _userManager = userManager;
    _env = env;
  }

  //iniciar sesion
  [HttpGet]
  [AllowAnonymous]
  public IActionResult IniciarSesion(string? returnUrl = null)
  {
    ViewData["ReturnUrl"] = returnUrl;
    return View(new IniciarSesionVM());
  }

  [HttpPost]
  [AllowAnonymous]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> IniciarSesion(IniciarSesionVM vm, string? returnUrl = null)
  {
    ViewData["ReturnUrl"] = returnUrl;

    if (!ModelState.IsValid)
      return View(vm);

    var resultado = await _signInManager.PasswordSignInAsync(
      vm.Email, vm.Password, vm.RememberMe, lockoutOnFailure: false);

    if (resultado.Succeeded)
    {
      if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        return Redirect(returnUrl);

      return RedirectToAction("Index", "Inicio");
    }

    ModelState.AddModelError(string.Empty, "Email o contraseña incorrectos.");
    return View(vm);
  }

  //cerrar sesion
  [HttpPost]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> CerrarSesion()
  {
    await _signInManager.SignOutAsync();
    return RedirectToAction("IniciarSesion", "Cuenta");
  }

  //acceso denegado
  [HttpGet]
  public IActionResult AccesoDenegado()
  {
    return View();
  }

  //perfil
  [HttpGet]
  [Authorize]
  public async Task<IActionResult> Perfil()
  {
    var user = await _userManager.GetUserAsync(User);
    if (user == null) return NotFound();

    var roles = await _userManager.GetRolesAsync(user);

    return View(new PerfilVM
    {
      NombreCompleto = user.NombreCompleto ?? string.Empty,
      AvatarPath = user.AvatarPath,
      Email = user.Email,
      Rol = roles.FirstOrDefault()
    });
  }

  [HttpPost]
  [ValidateAntiForgeryToken]
  [Authorize]
  public async Task<IActionResult> Perfil(PerfilVM vm)
  {
    var user = await _userManager.GetUserAsync(User);
    if (user == null) return NotFound();

    // validar archivo
    if (vm.AvatarArchivo != null)
    {
      var ext = Path.GetExtension(vm.AvatarArchivo.FileName).ToLowerInvariant();
      if (!ExtensionesPermitidas.Contains(ext))
        ModelState.AddModelError("AvatarArchivo", "Formato no permetido (jpg, jpeg, png o webp)");
      else if (vm.AvatarArchivo.Length > TamanoMaximoAvatar)
        ModelState.AddModelError("AvatarArchivo", "La imagen supera los 2MB");
    }

    if (!ModelState.IsValid)
    {
      vm.Email = user.Email;
      vm.AvatarPath = user.AvatarPath;
      vm.Rol = (await _userManager.GetRolesAsync(user)).FirstOrDefault();
      return View(vm);
    }

    user.NombreCompleto = vm.NombreCompleto.Trim();

    if (vm.AvatarArchivo != null)
    {
      BorrarAvatar(user.AvatarPath);
      user.AvatarPath = await GuardarAvatarAsync(vm.AvatarArchivo, user.Id);
    }

    var result = await _userManager.UpdateAsync(user);
    if (!result.Succeeded)
    {
      foreach (var e in result.Errors)
      {
        ModelState.AddModelError("", e.Description);
      }

      vm.Email = user.Email;
      vm.AvatarPath = user.AvatarPath;
      return View(vm);
    }

    //refresca la cookie por si el nombre cambio
    await _signInManager.RefreshSignInAsync(user);

    TempData["Ok"] = "Perfil actualizado";
    return RedirectToAction(nameof(Perfil));
  }

  // helpers
  private async Task<string?> GuardarAvatarAsync(IFormFile archivo, string userId)
  {
    if (archivo == null || archivo.Length == 0) return null;

    var carpeta = Path.Combine(_env.WebRootPath, "uploads", "avatares");
    Directory.CreateDirectory(carpeta);

    var ext = Path.GetExtension(archivo.FileName).ToLowerInvariant();
    var nombre = $"{userId}{ext}"; //uno solo por usuario (lo sobreescribe)
    var rutaFisica = Path.Combine(carpeta, nombre);

    using var stream = new FileStream(rutaFisica, FileMode.Create);
    await archivo.CopyToAsync(stream);

    return "/uploads/avatares/" + nombre + "?v=" + DateTime.Now.Ticks; // cache-bust acá como siempre va a guardar siempre con el {userId}.ext el nav cachea la imagen vieja, forzando la recarga agregando un querystring distinto cada vez
  }

  private void BorrarAvatar(string? path)
  {
    if (string.IsNullOrEmpty(path)) return;

    //quitar el querystring si la tiene y eso
    var pathSinQuery = path.Split('?')[0];
    var fisico = Path.Combine(_env.WebRootPath, pathSinQuery.TrimStart('/'));

    if (System.IO.File.Exists(fisico)) System.IO.File.Delete(fisico);
  }

}