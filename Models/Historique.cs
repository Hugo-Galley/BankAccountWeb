using System;
using System.Collections.Generic;

namespace BankAccountWeb.Models;

public partial class Historique
{
    public int IdHistorique { get; set; }

    public DateTime? DateOperation { get; set; }

    public decimal Montant { get; set; }

    public string TypeOperation { get; set; } = null!;

    public int Donneur { get; set; }

    public int Receveur { get; set; }

    public virtual Compte DonneurNavigation { get; set; } = null!;

    public virtual Compte ReceveurNavigation { get; set; } = null!;
}
