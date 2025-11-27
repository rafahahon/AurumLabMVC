
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
                    // dispositivo -> objeto completo com todos os atributos
                    // nome -> o nome do tipo do dispositivo
                    // (
                    //      dispositivo = (objeto Dispositivo inteiro com todos os atributos)
                    //      Nome = "Sensor" (exemplo)
                    // )
                )
                .GroupBy(item => item.Nome) // agrupa dispositivos por nome do tipo
                .Select(grupo => new ItemAgrupado // cria a lista de item agrupado para retornar somente nome + quatidade
                {
                    Nome = grupo.Key, // grupo.Key retorna o nome do tipo
                    Quantidade = grupo.Count() // Count() - retorna quantidade de dispositivos daquele tipo
                })
                .ToList(); // executa a consulta no banco e transforma em lista

                // lista de locais
                var locais = _context.LocalDispositivos
                    .OrderBy(local => local.Nome) // ordena locais por nome
                    .ToList(); // buscar os locais cadastrados e ordenar pelo nome e converter para lista

                // VIEW MODEL
                // cria a ViewModel com todas as informacoes que a pagina precisa 
                DashboardViewModel viewModel = new DashboardViewModel
                {
                    NomeUsuario = usuario?.NomeUsuario?? "Usuário", // ?? se nao for nulo ele recebe o nome do usuario, se for, ele coloca usuario
                    FotoUsuario = "/assets/img/img-perfil.png",

                    TotalDispositivos = _context.Dispositivos.Count(),
                    TotalAtivos = _context.Dispositivos.Count(dispositivos => dispositivos.SituacaoOperacional == "Operando"),
                    TotalEmManutencao = _context.Dispositivos.Count(dispositivos => dispositivos.SituacaoOperacional == "Em manutenção"),
                    TotalInoperantes = _context.Dispositivos.Count(dispositivos => dispositivos.SituacaoOperacional == "Inoperante"),

                    DispositivosPorTipo = dispositivosPorTipo,
                    Locais = locais

                };

                return View(viewModel);
        }
    }
}

// EXPLICAÇÃO JOIN Tabela Dispositivos d com Tabela TipoDispositivo td

                // var dispositivosPorTipo = _context.Dispositivos
                //             .Join(
                //                     _context.TipoDispositivos, 
                //                     d => d.IdTipoDispositivo, 
                //                     td => td.IdTipoDispositivo, 
                //                     (dispositivo, tipoDispositivo) => new {dispositivo, tipoDispositivo.Nome}

                //                 )

                // SELECT * FROM Dispositivos d
                // JOIN TipoDispositivos td
                // ON d.IdTipoDispositivo = td.IdTipoDispositivo

                // 4 linha -> cria objeto
                // {
                //      dispositivo = {nome: "xyz", "local": "sala 10"}
                //      Nome = "notebook"
                // }