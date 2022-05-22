using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using NugMagicK.Models;
using Microsoft.AspNetCore.Http;

namespace MVCMagicK.Services
{
    public class ServiceCliente
    {
        private string UrlApi;
        private MediaTypeWithQualityHeaderValue Header;

        public ServiceCliente(string urlapi)
        {
            this.UrlApi = urlapi;
            this.Header = new MediaTypeWithQualityHeaderValue("application/json");
        }

        public async Task<string> GetTokenAsync(string email, string password)
        {
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(this.UrlApi);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(this.Header);
                LoginModel model = new LoginModel
                {
                    Email = email,
                    Password = password
                };
                string json = JsonConvert.SerializeObject(model);
                StringContent content =
                    new StringContent(json, Encoding.UTF8, "application/json");
                string request = "/api/Authorization/ValidarUsuario";
                HttpResponseMessage response =
                    await client.PostAsync(this.UrlApi + request, content);
                if (response.IsSuccessStatusCode)
                {
                    string data = await response.Content.ReadAsStringAsync();
                    JObject jObject = JObject.Parse(data);
                    string token = jObject.GetValue("response").ToString();
                    return token;
                }
                else
                {
                    return null;
                }
            }
        }

        public async Task CompraItem(string token, int userId, int idCarta)
        {
            Item item = await this.getItemId(idCarta);
            await this.RegistraCompra(userId, item.IdUser, item.IdItem, item.Precio, token);
            await this.TransfiereCarta(item, userId, token);
        }

        private async Task<T> CallApiAsync<T>(string request)
        {
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(this.UrlApi);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(this.Header);
                HttpResponseMessage response =
                    await client.GetAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    T data = await response.Content.ReadAsAsync<T>();
                    return data;
                }
                else
                {
                    return default(T);
                }
            }
        }
        private async Task<T> CallApiAsync<T>(string request, string token)
        {
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(this.UrlApi);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(this.Header);
                client.DefaultRequestHeaders.Add("Authorization", "bearer " + token);
                HttpResponseMessage response =
                    await client.GetAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    T data = await response.Content.ReadAsAsync<T>();
                    return data;
                }
                else
                {
                    return default(T);
                }

            }
        }

        public async Task<List<ResumenCompra>> getComprasUser(int idUsuario,string token)
        {
            string request = "/api/Item/VerificarCompras/" + idUsuario;
            List<ResumenCompra> compras =
                await this.CallApiAsync<List<ResumenCompra>>(this.UrlApi + request, token);
            return compras;
        }

        public async Task<List<Usuario>> GetAllUsuarios(string token)
        {
            string request = "/api/User/GetUsuariosAll";
            List<Usuario> users =
                await this.CallApiAsync<List<Usuario>>(this.UrlApi + request, token);
            return users;
        }

        public async Task<bool> ExisteUsuario(string correo)
        {
            string request = "/api/User/ExisteUsuario/" + correo;
            Boolean existe =
                await this.CallApiAsync<Boolean>(this.UrlApi + request);
            return existe;
        }

        public async Task<Boolean> CreateUser(string nombre, string contrasena, string correo)
        {
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(this.UrlApi);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(this.Header);

                string request = "/api/User";

                Usuario user = new Usuario { Nombre = nombre, Contraseña = contrasena, Correo = correo};

                string json = JsonConvert.SerializeObject(user);

                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync(this.UrlApi + request, content);

                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public async Task DeleteItem(int idItem)
        {
            using (HttpClient client = new HttpClient())
            {
                string request = "/api/Item/DeleteItem/" + idItem;
                client.BaseAddress = new Uri(this.UrlApi);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(this.Header);
                HttpResponseMessage response = await client.DeleteAsync(this.UrlApi + request);
            }
        }
        public async Task DeleteUser(int idUsuario, string token)
        {
            using (HttpClient client = new HttpClient())
            {
                string request = "/api/User/" + idUsuario;
                client.BaseAddress = new Uri(this.UrlApi);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(this.Header);
                client.DefaultRequestHeaders.Add("Authorization", "bearer " + token);
                HttpResponseMessage response = await client.DeleteAsync(this.UrlApi + request);
            }
        }

        public async Task<List<ViewProducto>> GetItemsHome()
        {
            string request = "api/Item";
            List<ViewProducto> items =
                await this.CallApiAsync<List<ViewProducto>>(request);
            return items;
        }

        public async Task<Boolean> InsertarItem(string nombre, int userId, string producto, int precio, int estado, string imagen, string descripcion,string token)
        {
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(this.UrlApi);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(this.Header);
                client.DefaultRequestHeaders.Add("Authorization", "bearer " + token);

                string request = "/api/Item/InsertItem";

                Item item = new Item()
                {
                    Nombre = nombre,
                    IdUser = userId,
                    IdProducto = producto,
                    Precio = precio,
                    Estado = estado,
                    Imagen = imagen,
                    Descripcion = descripcion,
                };

                string json = JsonConvert.SerializeObject(item);

                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync(this.UrlApi + request, content);

                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public async Task<List<VW_ItemsUsuario_Listados>> getItemsUserProducto(string idProducto, int userId, string token)
        {
            string request = "/api/Item/StockItemUsuario/" + userId + "/"+idProducto;
            List<VW_ItemsUsuario_Listados> items =
                await this.CallApiAsync<List<VW_ItemsUsuario_Listados>>(this.UrlApi + request, token);
            return items;
        }

        public async Task<List<ViewProducto>> GetItemsHomeFiltro(string filtro)
        {
            string request = "/api/Item/ItemsHomeFiltro/" + filtro;
            List<ViewProducto> items =
                await this.CallApiAsync<List<ViewProducto>>(this.UrlApi + request);
            return items;
        }

        public async Task<List<VW_ItemsUsuario_Listados>> GetItemsProducto(string idProducto)
        {
            string request = "/api/Item/StockItem/" + idProducto;
            List<VW_ItemsUsuario_Listados> items =
                await this.CallApiAsync<List<VW_ItemsUsuario_Listados>>(this.UrlApi + request);
            return items;
        }

        public async Task<Item> getItemId(int idCarta)
        {
            string request = "/api/Item/GetItemId/" + idCarta;
            Item item =
                await this.CallApiAsync<Item>(this.UrlApi + request);
            return item;
        }

        public async Task<Boolean> TransfiereCarta(Item item, int userId,string token)
        {
            
            using (HttpClient client = new HttpClient())
            {
                item.IdUser = userId;
                client.BaseAddress = new Uri(this.UrlApi);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(this.Header);
                client.DefaultRequestHeaders.Add("Authorization", "bearer " + token);

                string request = "/api/Item";

                string json = JsonConvert.SerializeObject(item);

                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PutAsync(this.UrlApi + request, content);

                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public async Task<Boolean> ModificaItem(Item item, string token)
        {

            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(this.UrlApi);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(this.Header);
                client.DefaultRequestHeaders.Add("Authorization", "bearer " + token);

                string request = "/api/Item";

                string json = JsonConvert.SerializeObject(item);

                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PutAsync(this.UrlApi + request, content);

                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public async Task RegistraCompra(int userId, int idVendedor, int idItem, int precio, string token)
        {
            using (HttpClient client = new HttpClient())
            {
                Compra compra = new Compra()
                {
                    IdComprador = userId,
                    IdVendedorUser = idVendedor,
                    IdItem = idItem,
                    Precio = precio
                };
                client.BaseAddress = new Uri(this.UrlApi);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(this.Header);
                client.DefaultRequestHeaders.Add("Authorization", "bearer " + token);

                string request = "/api/Item/Registracompra";

                string json = JsonConvert.SerializeObject(compra);

                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync(this.UrlApi + request, content);
            }
        }

        public async Task<Usuario> GetPerfilUsuario(string token)
        {
            string request = "api/User/PerfilUsuario";
            Usuario user =
                await this.CallApiAsync<Usuario>(this.UrlApi + request, token);
            return user;
        }
        public async Task<Usuario> GetUsuarioId(string token, int id)
        {
            string request = "/api/User/UsuariobyId/" + id;
            Usuario user =
                await this.CallApiAsync<Usuario>(this.UrlApi + request, token);
            return user;
        }

        public async Task<List<ViewProductoUsuario>> GetDistItemsUser(string token, int idUser)
        {
            string request = "/api/Item/ItemsUsuario/" + idUser;
            List<ViewProductoUsuario> items =
                await this.CallApiAsync<List<ViewProductoUsuario>>(this.UrlApi + request, token);
            return items;
        }



    }
}
