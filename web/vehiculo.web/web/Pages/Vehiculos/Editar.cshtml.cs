using Abstracciones.Interfaces.Reglas;
using Abstracciones.Modelos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace web.Pages.Vehiculos
{
    public class EditarModel : PageModel
    {
        private readonly IConfiguracion _configuracion;
        public IList<VehiculoResponse> vehiculos { get; set; } = default!;
        public EditarModel(IConfiguracion configuracion)
        {
            _configuracion = configuracion;
        }

        [BindProperty]
        public VehiculoResponse vehiculoResponse { get; set; }
        [BindProperty]
        public List<SelectListItem> marcas { get; set; }
        [BindProperty]
        public List<SelectListItem> modelos { get; set; }
        [BindProperty]
        public Guid marcaSeleccionada { get; set; }
        [BindProperty]
        public Guid modeloSeleccionado { get; set; }
        public async Task<ActionResult> OnGet(Guid? id)
        {
            if (id == Guid.Empty)
                return NotFound();
            string endpoint = _configuracion.ObtenerMetodo("ApiEndPoints",
            "ObtenerVehiculo");
            var cliente = new HttpClient();
            var solicitud = new HttpRequestMessage(HttpMethod.Get, string.Format(endpoint, id));
            var respuesta = await cliente.SendAsync(solicitud);
            respuesta.EnsureSuccessStatusCode();
            if(respuesta.StatusCode == HttpStatusCode.OK)
            {
                await ObtenerMarcas();
                var resultado = await respuesta.Content.ReadAsStringAsync();
                var opciones = new JsonSerializerOptions
                { PropertyNameCaseInsensitive = true };
                vehiculoResponse = JsonSerializer.Deserialize<VehiculoResponse>(resultado, opciones);

                if(vehiculoResponse != null)
                {
                marcaSeleccionada = Guid.Parse(marcas.Where(m=>m.Text==vehiculoResponse.Marca).FirstOrDefault().Value);
                    modelos = (await ObtenerModelos(marcaSeleccionada)).Select(m=>
                    
                    new SelectListItem
                    {
                        Value = m.Id.ToString(),
                        Text= m.Nombre,
                        Selected= m.Nombre == vehiculoResponse.Modelo

                    }
                    ).ToList();
                    modeloSeleccionado = Guid.Parse(modelos.Where(m => m.Text == vehiculoResponse.Modelo).FirstOrDefault().Value);
                }
            }            
         
            return Page();
        }

        public async Task<ActionResult> OnPost()
        {
            if(!ModelState.IsValid)
            
                return Page();
            

            string endpoint = _configuracion.ObtenerMetodo("ApiEndPoints","EditarVehiculo");
            var cliente = new HttpClient();
            var respuesta = await cliente.PutAsJsonAsync<VehiculoRequest>(string.Format(endpoint,vehiculoResponse.Id),
                new VehiculoRequest
            {
                Placa = vehiculoResponse.Placa,
                IdModelo = modeloSeleccionado,
                Anio = vehiculoResponse.Anio,
                Color = vehiculoResponse.Color,
                Precio = vehiculoResponse.Precio,
                CorreoPropietario = vehiculoResponse.CorreoPropietario,
                TelefonoPropietario = vehiculoResponse.TelefonoPropietario


            });
            respuesta.EnsureSuccessStatusCode();
            return RedirectToPage("./Index");
        }

        private async Task ObtenerMarcas()
        {
            string endpoint = _configuracion.ObtenerMetodo("ApiEndPoints",
               "ObtenerMarcas");
            var cliente = new HttpClient();
            var solicitud = new HttpRequestMessage(HttpMethod.Get, endpoint);
            var respuesta = await cliente.SendAsync(solicitud);
            respuesta.EnsureSuccessStatusCode();
            var resultado = await respuesta.Content.ReadAsStringAsync();
            var opciones = new JsonSerializerOptions
            { PropertyNameCaseInsensitive = true };
            var resultaddeserializado = JsonSerializer.Deserialize<List<Marca>>(resultado, opciones);
            marcas = resultaddeserializado.Select(m =>
            new SelectListItem
            {
                Value = m.Id.ToString(),
                Text = m.Nombre
            }
            ).ToList();

        }
        private async Task<List<Modelo>> ObtenerModelos(Guid marcaID)
        {
            string endpoint = _configuracion.ObtenerMetodo("ApiEndPoints",
               "ObtenerModelos");
            var cliente = new HttpClient();
            var solicitud = new HttpRequestMessage(HttpMethod.Get, string.Format(endpoint,marcaID));
            var respuesta = await cliente.SendAsync(solicitud);
            respuesta.EnsureSuccessStatusCode();
            if (respuesta.StatusCode == HttpStatusCode.OK)
            {
                var resultado = await respuesta.Content.ReadAsStringAsync();
                var opciones = new JsonSerializerOptions
                { PropertyNameCaseInsensitive = true };
                return JsonSerializer.Deserialize<List<Modelo>>(resultado, opciones);
            }
           
                return new List<Modelo>();
            
        }

        public async Task<JsonResult> OnGetObtenerModelos(Guid marcaID)
        {
            var modelos = await ObtenerModelos(marcaID);
            return new JsonResult(modelos);
        }
    }
}
