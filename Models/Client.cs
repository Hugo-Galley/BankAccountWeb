using System;
using System.Collections.Generic;

namespace BankAccountWeb.Models;

public partial class Client
{
    public int IdClient { get; set; }

    public string Nom { get; set; } = null!;

    public string Prenom { get; set; } = null!;

    public string Mail { get; set; } = null!;

    public string? Tel { get; set; }

    public string Mdp { get; set; } = null!;

    public DateTime? CreateAt { get; set; }

    public virtual ICollection<Compte> Comptes { get; set; } = new List<Compte>();
}
