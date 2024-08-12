using Microsoft.AspNetCore.Mvc;
using SiteDaf.Models;
using System.Linq;

namespace SiteDaf.Controllers
{
    public class TeamController : Controller
    {
        private readonly DAFContext _context;

        public TeamController(DAFContext context)
        {
            _context = context;
        }

        // Affiche les équipes et les joueurs
        public IActionResult Index()
        {
            var teams = _context.Equipes
                                .Select(e => new TeamViewModel
                                {
                                    IdEquipe = e.IdEquipe,
                                    NomEquipe = e.NomEquipe,
                                    Joueurs = _context.Joueurs.Where(j => j.IdEquipe == e.IdEquipe).ToList()
                                })
                                .ToList();

            return View(teams);
        }

        // Supprimer une équipe
        public IActionResult Delete(int id)
        {
            var equipe = _context.Equipes.Find(id);
            if (equipe != null)
            {
                _context.Equipes.Remove(equipe);
                _context.SaveChanges();
            }
            return RedirectToAction(nameof(Index));
        }

        // Modifier une équipe (formulaire)
        public IActionResult Edit(int id)
        {
            var equipe = _context.Equipes
                .Where(e => e.IdEquipe == id)
                .Select(e => new EditTeamViewModel
                {
                    IdEquipe = e.IdEquipe,
                    NomEquipe = e.NomEquipe,
                    Joueurs = _context.Joueurs.Where(j => j.IdEquipe == e.IdEquipe).ToList()
                })
                .FirstOrDefault();

            if (equipe == null)
            {
                return NotFound();
            }

            return View(equipe);
        }






        // Modifier une équipe (soumission du formulaire)
        [HttpPost]
        public IActionResult Edit(EditTeamViewModel model)
        {
            if (ModelState.IsValid)
            {
                var equipe = _context.Equipes.Find(model.IdEquipe);
                if (equipe == null)
                {
                    return NotFound();
                }

                equipe.NomEquipe = model.NomEquipe;
                _context.Equipes.Update(equipe);

                // Mise à jour des joueurs existants
                foreach (var joueur in model.Joueurs)
                {
                    if (joueur.IdJoueur > 0)
                    {
                        var existingJoueur = _context.Joueurs.Find(joueur.IdJoueur);
                        if (existingJoueur != null)
                        {
                            existingJoueur.Nom = joueur.Nom;
                            existingJoueur.Sexe = joueur.Sexe;
                            _context.Joueurs.Update(existingJoueur);
                        }
                    }
                    else
                    {
                        joueur.IdEquipe = equipe.IdEquipe;
                        _context.Joueurs.Add(joueur);
                    }
                }

                _context.SaveChanges();

                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        // Créer une nouvelle équipe (formulaire)
        public IActionResult Create()
        {
            var model = new CreateTeamViewModel
            {
                Joueurs = new List<Joueur> { new Joueur() } // Un joueur par défaut pour commencer
            };
            return View(model);
        }

        // Créer une nouvelle équipe (soumission du formulaire)
        [HttpPost]
        public IActionResult Create(CreateTeamViewModel model)
        {
            if (ModelState.IsValid)
            {
                var equipe = new Equipe
                {
                    NomEquipe = model.NomEquipe
                };

                _context.Equipes.Add(equipe);
                _context.SaveChanges();

                foreach (var joueur in model.Joueurs)
                {
                    joueur.IdEquipe = equipe.IdEquipe;
                    _context.Joueurs.Add(joueur);
                }

                _context.SaveChanges();

                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        // Supprimer un joueur
        public IActionResult DeletePlayer(int id)
        {
            var joueur = _context.Joueurs.Find(id);
            if (joueur != null)
            {
                _context.Joueurs.Remove(joueur);
                _context.SaveChanges();
            }
            return RedirectToAction(nameof(Index));
        }
    }

    public class TeamViewModel
    {
        public int IdEquipe { get; set; }
        public string? NomEquipe { get; set; }
        public List<Joueur> Joueurs { get; set; }
    }

    public class EditTeamViewModel
    {
        public int IdEquipe { get; set; }
        public string? NomEquipe { get; set; }
        public List<Joueur> Joueurs { get; set; }
    }

    public class CreateTeamViewModel
    {
        public string? NomEquipe { get; set; }
        public List<Joueur> Joueurs { get; set; }
    }
}