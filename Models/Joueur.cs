using System;
using System.Collections.Generic;

namespace SiteDaf.Models;

public partial class Joueur
{
    public int IdJoueur { get; set; }

    public string Nom { get; set; } = null!;

    public int? Sexe { get; set; }

    public int? IdEquipe { get; set; }
}
