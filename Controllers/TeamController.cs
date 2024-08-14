using Microsoft.AspNetCore.Mvc;
using SiteDaf.Models;
using System.Drawing;
using System.Drawing.Printing;
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

        // impression
        public IActionResult Print(int id)
        {
            var result = (from e in _context.Vue_impressions
                          where e.IdEquipe == id
                          select new
                          {
                              NomEquipe = e.NomEquipe,
                              NomJoueur = e.NomJoueur,
                              ADJECTIF = e.ADJECTIF,
                              Z13 = e.Z13
                          }).ToList();  // Convertir en liste pour gérer l'indexation

            var printDocument = new PrintDocument();
            int currentPageIndex = 0;  // Pour suivre la progression de l'impression

            printDocument.PrintPage += (sender, e) =>
            {
                if (currentPageIndex < result.Count)
                {
                    int yPos = 100; // Position initiale
                    int leftMargin = e.MarginBounds.Left;

                    var item = result[currentPageIndex];

                    // Dessiner l'équipe et le personnage
                    using (Font enchantedLandFont = new Font("Enchanted Land", 20))
                    {
                        e.Graphics.DrawString("Équipe / Personnage", enchantedLandFont, Brushes.Black, leftMargin, yPos);
                        yPos += enchantedLandFont.Height + 20;
                    }

                    // Dessiner le nom de l'équipe et du joueur
                    using (Font berryRotundaFont = new Font("Berry Rotunda", 16))
                    {
                        string equipeJoueur = $"{item.NomEquipe} / {item.NomJoueur}";
                        e.Graphics.DrawString(equipeJoueur, berryRotundaFont, Brushes.Black, leftMargin, yPos);
                        yPos += berryRotundaFont.Height + 40;
                    }

                    // Dessiner le niveau de réussite
                    using (Font berryRotundaFont = new Font("Berry Rotunda", 16))
                    {
                        e.Graphics.DrawString("Niveau de réussite", berryRotundaFont, Brushes.Black, leftMargin, yPos);
                        yPos += berryRotundaFont.Height + 20;

                        // Dessiner le Z13 et l'adjectif
                        string zone3Adjectif = $"{item.Z13} \n{item.ADJECTIF}";
                        e.Graphics.DrawString(zone3Adjectif, berryRotundaFont, Brushes.Black, leftMargin, yPos);
                        yPos += berryRotundaFont.Height + 40;
                    }

                    currentPageIndex++;

                    e.HasMorePages = currentPageIndex < result.Count;  // Continuer s'il reste des joueurs à imprimer
                }
                else
                {
                    e.HasMorePages = false;  // Arrêter l'impression
                }
            };

            printDocument.Print();  // Envoie le document à l'imprimante

            return Ok("Impression en cours...");
        }



        // Affiche les équipes et les joueurs
        public IActionResult Index(string searchString)
        {
            var teams = from t in _context.Equipes
                        select t;

            if (!String.IsNullOrEmpty(searchString))
            {
                teams = teams.Where(s => s.NomEquipe.Contains(searchString));
            }

            var teamViewModels = teams.Select(e => new TeamViewModel
            {
                IdEquipe = e.IdEquipe,
                NomEquipe = e.NomEquipe,
                Joueurs = _context.Joueurs.Where(j => j.IdEquipe == e.IdEquipe).ToList()
            }).ToList();

            return View(teamViewModels);
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