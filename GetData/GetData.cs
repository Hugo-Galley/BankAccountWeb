using BankAccountWeb.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace BankAccountWeb.GetData;

public class GetData
{
    private readonly ILogger<GetData> _logger;
    private readonly BankAccountContext _context;

    public GetData(ILogger<GetData> logger, BankAccountContext context)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Client> GetClientInformations(int clientId)
    {
        Client client = await _context.Clients.Where(x => x.IdClient == clientId).FirstOrDefaultAsync();
        return client;
    }

    public async Task<List<Compte>> GetAccountInformations(int clientId)
    {
        List<Compte> accountList = await _context.Comptes.Where(x => x.IdClient == clientId).ToListAsync();
        return accountList;
    }

    public async Task<int> LoginClient(string email, string password)
    {
       var client = await _context.Clients.Where(x =>
                x.Mail == email && x.Mdp == Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(password))))
            .FirstOrDefaultAsync();
       if (client != null)
       {
           return client.IdClient;
       }
       else
       {
           return -1;
       }
    }

    public async Task<List<string>> GetClientAccount(int id_client)
    {
        List<string> returnList = [];
        var liste = await _context.Comptes.Where(x => x.IdClient == id_client).ToListAsync();
        foreach (var compte in liste)
        {
            returnList.Add($"{compte.NumeroCompte}");
        }

        return returnList;

    }
    
    public async Task<List<Class.HistoriqueViewModel>> GetAccountHistory(int accountId)
    {
        List<Class.HistoriqueViewModel> result = new();
    
        var historique = await _context.Historiques
            .Where(h => h.Donneur == accountId || h.Receveur == accountId)
            .OrderByDescending(h => h.DateOperation)
            .ToListAsync();

        foreach (var operation in historique)
        {
            string direction = "";
            string detail = "";

            if (operation.TypeOperation == "Virement" || operation.TypeOperation == "Transfert")
            {
                if (operation.Donneur == accountId)
                {
                    direction = operation.TypeOperation == "Virement" ? "➡️ Virement sortant" : "➡️ Transfert sortant";
                    var compteDestination = await _context.Comptes.FirstOrDefaultAsync(c => c.IdCompte == operation.Receveur);
                    if (compteDestination != null)
                    {
                        detail = $" vers compte {compteDestination.Type}";
                    }
                }
                else
                {
                    direction = operation.TypeOperation == "Virement" ? "⬅️ Virement entrant" : "⬅️ Transfert entrant";
                    var compteSource = await _context.Comptes.FirstOrDefaultAsync(c => c.IdCompte == operation.Donneur);
                    if (compteSource != null)
                    {
                        detail = $" depuis compte {compteSource.Type}";
                    }
                }
            }
            else if (operation.TypeOperation == "Dépôt")
            {
                direction = "💰 Dépôt";
            }
            else if (operation.TypeOperation == "Retrait")
            {
                direction = "📉 Retrait";
            }

            result.Add(new Class.HistoriqueViewModel
            {
                Date = operation.DateOperation ?? DateTime.MinValue,
                Type = $"{direction}{detail}",
                Montant = operation.Montant,
                RawType = operation.TypeOperation
            });
        }
    
        return result;
    }
    
}