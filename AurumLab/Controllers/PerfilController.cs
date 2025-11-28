
using System.Security.Policy;
using AurumLab.Data;
using AurumLab.Models;
using AurumLab.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AurumLab.Controllers
{
    
    public class PerfilController : Controller
    {
        private readonly AppDbContext _context;

        public PerfilController(AppDbContext context)
        {
            _context = context;
        }

        // GET tela de perfil
        public IActionResult Index()
        {
            int? usuarioId = HttpContext.Session.GetInt32("UsuarioId");

            if (usuarioId == null)
            {
                return RedirectToAction("Index", "Login");
            }

            // pega os dados por completo do usuario logado na sessao pelo id
            var usuario = _context.Usuarios.FirstOrDefault(usuario => usuario.IdUsuario == usuarioId);

            var viewModel = new PerfilViewModel
            {
                IdUsuario = usuario.IdUsuario,
                NomeCompleto = usuario.NomeCompleto,
                //NomeUsuario = usuario.NomeUsuario,
                NomeUsuario = usuario?.NomeUsuario ?? "Usuário",
                Email = usuario.Email,
                RegraId = usuario.RegraId,

                // listando as regras que existem dentro da tabela RegraPerfil para mostrar dentro do select
                Regras = _context.RegraPerfils.ToList(),

                // se exister foto, converte a foto para string, se nao exister, retorna nulo
                FotoBase64 = usuario.Foto != null ? Convert.ToBase64String(usuario.Foto) : null
            };

            return View(viewModel);
        }

        // POST - salvar dados de texto do perfil
        [HttpPost]
        public IActionResult Index(PerfilViewModel model)
        {
            var usuario = _context.Usuarios.FirstOrDefault(usuario => usuario.IdUsuario == model.IdUsuario);

            if(usuario == null)
            {
                return RedirectToAction("Index", "Login");
            }

            if(!string.IsNullOrWhiteSpace(model.NovaSenha))
            {
                if(model.NovaSenha != model.ConfirmarSenha)
                {
                    ViewBag.Erro = "As senhas diferem.";

                    // quando o POST(a atualizacao que estamos fazendo) da erro e volta pra View, a lista de regras nao vem preenchida.
                    // pq ela e um select com a lista de regras que estamos puxando do banco
                    model.Regras = _context.RegraPerfils.ToList();
                    return View(model);
                }

                // converte a nova senha para hash
                usuario.Senha = HashService.GerarHashBytes(model.NovaSenha);
            }

            // atualizar restante dos dados 
            usuario.NomeCompleto = model.NomeCompleto;
            usuario.NomeUsuario = model.NomeUsuario;
            usuario.Email = model.Email;
            usuario.RegraId = model.RegraId;

            _context.SaveChanges();

            // ViewBag morre no redirect
            // TempData sobrevive a um redirect pelo menos uma vez
            TempData["Sucesso"] = "Perfil atualizado com sucesso.";
            return RedirectToAction("Index");
        }

        // POST - atualizar a foto de perfil (MODAL)
        [HttpPost]
        public IActionResult AtualizarFoto(int idUsuario, IFormFile foto)
        {
            // IFormFile -> representa um arquivo enviado pelo formulario no HTML
            // quando o formulario e enviado, o navegador envia o arquivo e o mvc converte para um objeto IFormFile

            if (foto == null || foto.Length == 0)
            {
                return RedirectToAction("Index");
            }
            
            var usuario = _context.Usuarios.FirstOrDefault(usuario => usuario.IdUsuario == idUsuario);

            if(usuario == null)
            {
                return RedirectToAction("Index", "Login");
            }

            using (var ms = new MemoryStream())
            {
                foto.CopyTo(ms);
                usuario.Foto = ms.ToArray(); // salva como VARBINARY(MAX) - ate 2GB -dentro do banco
            }

            _context.SaveChanges();

            TempData["Sucesso"] = "Foto atualizada com sucesso!";
            return RedirectToAction("Index");
        }
    }
}