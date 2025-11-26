
using AurumLab.Data;
using AurumLab.Models;
using Microsoft.AspNetCore.Mvc;

namespace AurumLab.Controllers
{
    public class DashboardController : Controller
    {
        private readonly AppDbContext _context;

        public DashboardController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Dashboard()
        {
            // valida se existe login realizado 
            if(HttpContext.Session.GetInt32("UsuarioId") == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");

            var usuario = _context.Usuarios.FirstOrDefault(usuario => usuario.IdUsuario == usuarioId);

            // TIPOS DISPOSITIVOS - JOIN + AGRUPAMENTO
            // consultar a tabela dispositivos atraves da ViewModel

            // SELECT * FROMM DISPOSITIVOS
            var dispositivosPorTipo = _context.Dispositivos
            .Join(
                    _context.TipoDispositivos, // JOIN TipoDispositivos
                    dispositivo => dispositivo.IdTipoDispositivo, // ON dispositivo.IdTipoDispositivo
                    tipoDispositivo => tipoDispositivo.IdTipoDispositivo, // = tipoDispositivo.IdTipoDispositivo
                    (dispositivo, tipoDispositivo) => new {dispositivo, tipoDispositivo.Nome}
                    // para cada par encontrado - um dispositivo e seu tipoDispositivo correspondente monta um objeto:
                    // dispositivo -> objeto completo com nome, local, id
                    // nome -> o nome do tipo do dispositivo
                    // (
                    //      dispositivo = (objeto Dispositivo inteiro com nome, local, id)
                    //      Nome = "Sensor" (exemplo)
                    // )
                )
        }
    }
}