using KioscoPOS.Web.Models;
using KioscoPOS.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace KioscoPOS.Web.Controllers;

public class CuentaController : Controller
{
  private readonly SignInManager<ApplicationUser> _signInManager;

  public CuentaController(SignInManager<ApplicationUser> signInManager)
  {
    _signInManager = signInManager;
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
    return RedirectToAction("Iniciar Sesion", "Cuenta");
  }

  //acceso denegado
  [HttpGet]
  public IActionResult AccesoDenegado()
  {
    return View();
  }
}