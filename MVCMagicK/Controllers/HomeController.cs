using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MVCMagicK.Extensions;
using MVCMagicK.Services;
using NugMagicK.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ProyectoTiendaMagic.Controllers
{
    public class HomeController : Controller
    {

        private ServiceCliente service;
        private ServiceStorageBlob serviceBloob;

        public HomeController(ServiceCliente service,ServiceStorageBlob serviceBloob)
        {
            this.service = service;
            this.serviceBloob = serviceBloob;
        }

        public async Task<IActionResult> Index()
        {
            List<ViewProducto> items = await this.service.GetItemsHome();
            return View(items);
        }
        [HttpPost]
        public async Task<IActionResult> Index(string filtroNombre)
        {
            if(filtroNombre == "" || filtroNombre == null)
            {
                List<ViewProducto> itemsNull = await this.service.GetItemsHome();
                return View(itemsNull);
            }
            else
            {
                List<ViewProducto> items = await this.service.GetItemsHomeFiltro(filtroNombre);
                return View(items);
            }
        }
        public async Task<IActionResult> CompraCartas(string idProducto)
        {
            List<VW_ItemsUsuario_Listados> items = await this.service.GetItemsProducto(idProducto);
            return View(items);
        }

        public async Task<IActionResult> Comprar(int idCarta, string idProducto)
        {
            string token = HttpContext.User.FindFirst("TOKEN").Value;
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            await this.service.CompraItem(token, userId, idCarta);
            return RedirectToAction("CompraCartas", new { idProducto = idProducto });
        }
        public async Task<IActionResult> InsertarCarrito(int idItem)
        {
            Item item = await this.service.getItemId(idItem);
            string token = HttpContext.User.FindFirst("TOKEN").Value;
            Usuario user = await this.service.GetPerfilUsuario(token);
            List<int> items;
            if (HttpContext.Session.GetString("CARRITO" + user.IdUser) == null)
            {
                //No existe nada en la session, creamos la coleccion
                items = new List<int>();
            }
            else
            {
                items = HttpContext.Session.GetObject<List<int>>("CARRITO" + user.IdUser);
            }
            items.Add(idItem);
            HttpContext.Session.SetObject("CARRITO" + user.IdUser, items);
            return RedirectToAction("CompraCartas", new { idProducto = item.IdProducto });
        }

        public async Task<IActionResult> Carrito()
        {
            string token = HttpContext.User.FindFirst("TOKEN").Value;
            Usuario user = await this.service.GetPerfilUsuario(token);
            if (HttpContext.Session.GetObject<List<int>>("CARRITO" + user.IdUser) == null)
            {
                ViewData["MENSAJE"] = "EL CARRITO ESTA VACIO";
                return View();
            }
            List<int> carrito = HttpContext.Session.GetObject<List<int>>("CARRITO" + user.IdUser);
            List<Item> items = new List<Item>();
            foreach (int id in carrito)
            {
                Item item = await this.service.getItemId(id);
                items.Add(item);
            }
            return View(items);
        }
        public async Task<IActionResult> BorrarElementoCarrito(int idProducto)
        {
            string token = HttpContext.User.FindFirst("TOKEN").Value;
            Usuario user = await this.service.GetPerfilUsuario(token);
            if (HttpContext.Session.GetObject<List<int>>("CARRITO" + user.IdUser) != null)
            {
                List<int> carrito = HttpContext.Session.GetObject<List<int>>("CARRITO" + user.IdUser);
                carrito.Remove(idProducto);
                HttpContext.Session.SetObject("CARRITO" + user.IdUser, carrito);
            }
            return RedirectToAction("MiCarrito");
        }

        public async Task<IActionResult> ComprarCarrito()
        {
            string token = HttpContext.User.FindFirst("TOKEN").Value;
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            Usuario user = await this.service.GetPerfilUsuario(token);
            if (HttpContext.Session.GetObject<List<int>>("CARRITO" + user.IdUser) != null)
            {
                List<int> carrito = HttpContext.Session.GetObject<List<int>>("CARRITO" + user.IdUser);
                foreach(int Iditem in carrito)
                {
                    await this.service.CompraItem(token, userId, Iditem);
                }
                carrito.Clear();
                HttpContext.Session.SetObject("CARRITO" + user.IdUser, carrito);
            }
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> BorrarItemCompras(int idItem, string idProducto)
        {
            await this.service.DeleteItem(idItem);
            return RedirectToAction("CompraCartas", new { idProducto = idProducto });
        }

        public IActionResult Vender()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Vender(string nombre, string producto, int precio, IFormFile imagen, string descripcion)
        {
            string token = HttpContext.User.FindFirst("TOKEN").Value;
            string extension = imagen.FileName.Split(".")[1];
            string fileName = producto + "." + extension;
            
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            int estado = 1;

            if(await this.service.InsertarItem(nombre, userId, producto, precio, estado, fileName, descripcion, token))
            {
                using (Stream stream = imagen.OpenReadStream())
                {
                    Stream imgn = await this.serviceBloob.GetFileAsync(fileName);
                    if (imgn == null)
                    {
                        await this.serviceBloob.UploadFileAsync(stream, fileName);
                    }
                }
            }
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> ModificarCartas(string idProducto)
        {
            string token = HttpContext.User.FindFirst("TOKEN").Value;
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            List<VW_ItemsUsuario_Listados> items = await this.service.getItemsUserProducto(idProducto, userId,token);
            return View(items);
        }

        public async Task<IActionResult> ModificarCarta(int idCarta)
        {
            Item item = await this.service.getItemId(idCarta);
            return View(item);
        }

        [HttpPost]
        public async Task<IActionResult> ModificarCarta(int IdItem, string Imagen, string Nombre, string Producto, int Precio, string Descripcion)
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            string token = HttpContext.User.FindFirst("TOKEN").Value;
            Item item = new Item()
            {
                IdItem = IdItem,
                Imagen = Imagen,
                Nombre = Nombre,
                IdProducto = Producto,
                Precio = Precio,
                Descripcion = Descripcion,
                IdUser = userId,
                Estado = 1
            };
            await this.service.ModificaItem(item,token);
            return RedirectToAction("CompraCartas", new { idProducto = item.IdProducto });
        }
    }
}
