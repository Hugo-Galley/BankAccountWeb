using System;
using System.Collections.Generic;

namespace BankAccountWeb.Models;

public partial class Compte
{
    public int IdCompte { get; set; }

    public string Type { get; set; } = null!;

    public decimal Solde { get; set; }

    public DateTime? CreateAt { get; set; }

    public decimal? Interet { get; set; }

    public Guid? Rib { get; set; }

    public string? NumeroCompte { get; set; }

    public int IdClient { get; set; }

    public virtual ICollection<Historique> HistoriqueDonneurNavigations { get; set; } = new List<Historique>();

    public virtual ICollection<Historique> HistoriqueReceveurNavigations { get; set; } = new List<Historique>();

    public virtual Client IdClientNavigation { get; set; } = null!;
}
