using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using MVCMagicK.Services;
using Newtonsoft.Json;
using NugMagicK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MVCMagicK.Controllers
{
    public class UsuarioController : Controller
    {

        private ServiceCliente service;

        public UsuarioController(ServiceCliente service)
        {
            this.service = service;
        }

        public IActionResult LogIn()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> LogIn(string correo, string contraseña)
        {
            string token =
                await this.service.GetTokenAsync(correo, contraseña);
            if (token == null)
            {
                ViewData["MENSAJE"] = "Datos incorrectos";
                return View();
            }
            else
            {
                //SI EL USUARIO EXISTE, ALMACENAMOS EL TOKEN EN SESSION
                Usuario user = await this.service.GetPerfilUsuario(token);
                ClaimsIdentity identity =
                    new ClaimsIdentity
                    (CookieAuthenticationDefaults.AuthenticationScheme
                    , ClaimTypes.Name, ClaimTypes.Role);
                identity.AddClaim(new Claim("TOKEN", token));
                identity.AddClaim(new Claim(ClaimTypes.Role, user.TipoUsuario));
                identity.AddClaim(new Claim(ClaimTypes.Name, user.Nombre));
                identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.IdUser.ToString()));
                identity.AddClaim(new Claim("direccion", user.Direccion));

                ClaimsPrincipal principal = new ClaimsPrincipal(identity);
                await HttpContext.SignInAsync
                    (CookieAuthenticationDefaults.AuthenticationScheme
                    , principal, new AuthenticationProperties
                    {
                        IsPersistent = true,
                        ExpiresUtc = DateTime.UtcNow.AddMinutes(30)
                    });

                Console.WriteLine("Usuario Logueado");

                return RedirectToAction("Index", "Home");
            }
        }

        public async Task<IActionResult> LogOut()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> PerfilUsuario()
        {
            string token = HttpContext.User.FindFirst("TOKEN").Value;
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            List<ViewProductoUsuario> items = await this.service.GetDistItemsUser(token,userId);

            Usuario user = await this.service.GetPerfilUsuario(token);
            ViewData["Direccion"] = user.Direccion;
            ViewData["Correo"] = user.Correo;
            ViewData["Nombre"] = user.Nombre;
            ViewData["TipoUsuario"] = user.TipoUsuario;

            return View(items);
        }

        public async Task<IActionResult> AdministrarUsuarios()
        {
            string token = HttpContext.User.FindFirst("TOKEN").Value;
            List<Usuario> usuarios = await this.service.GetAllUsuarios(token);
            return View(usuarios);
        }

        public async Task<IActionResult> BorrarUsuario(int idUsuario)
        {
            string token = HttpContext.User.FindFirst("TOKEN").Value;
            await this.service.DeleteUser(idUsuario,token);
            return RedirectToAction("AdministrarUsuarios");
        }

        public async Task<ActionResult> MisCompras()
        {
            string token = HttpContext.User.FindFirst("TOKEN").Value;
            Usuario user = await this.service.GetPerfilUsuario(token);

            List<ResumenCompra> resumen = await this.service.getComprasUser(user.IdUser, token);
            return View(resumen);
        }

        public async Task<ActionResult> VerificarCompras(int idUsuario)
        {
            string token = HttpContext.User.FindFirst("TOKEN").Value;
            List<ResumenCompra> resumen = await this.service.getComprasUser(idUsuario,token);
            return View(resumen);
        }

        public IActionResult Registro()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RegistroAsync(string nombre, string contrasena, string conficontrasena, string correo)
        {
            if (await this.service.ExisteUsuario(correo) || contrasena != conficontrasena)
            {
                return View();
            }
            else
            {
                if(await this.service.CreateUser(nombre, contrasena, correo))
                {
                    string token =
                        await this.service.GetTokenAsync(correo, contrasena);
                    Usuario user = await this.service.GetPerfilUsuario(token);
                    ClaimsIdentity identity =
                        new ClaimsIdentity
                        (CookieAuthenticationDefaults.AuthenticationScheme
                        , ClaimTypes.Name, ClaimTypes.Role);
                    identity.AddClaim(new Claim("TOKEN", token));
                    identity.AddClaim(new Claim(ClaimTypes.Role, user.TipoUsuario));
                    identity.AddClaim(new Claim(ClaimTypes.Name, user.Nombre));
                    identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.IdUser.ToString()));
                    identity.AddClaim(new Claim("direccion", user.Direccion));

                    ClaimsPrincipal principal = new ClaimsPrincipal(identity);
                    await HttpContext.SignInAsync
                        (CookieAuthenticationDefaults.AuthenticationScheme
                        , principal, new AuthenticationProperties
                        {
                            IsPersistent = true,
                            ExpiresUtc = DateTime.UtcNow.AddMinutes(30)
                        });
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    return View();
                }
            }
            
        }
    }
}
