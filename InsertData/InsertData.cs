using BankAccountWeb.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity.UI.Services;
using Radzen;

namespace BankAccountWeb;

public class InsertData
{
    private readonly ILogger<InsertData> _logger;
    private readonly BankAccountContext _context;

    public InsertData(ILogger<InsertData> logger, BankAccountContext context)
    {
        _logger = logger;
        _context = context;

    }

    public async Task<string> CreateClient(string name, string surname, string phoneNumber, string email, string password, string typeAccount, int initialDeposit)
    {
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(surname) || 
            string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            return "Veuillez remplir tous les champs obligatoires";
        }
        
        var existingClient = await _context.Clients.FirstOrDefaultAsync(x => x.Mail == email);
        if (existingClient != null)
        {
            return "Cet identifiant est déjà utilisé";
        }

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var client = new Client
            {
                Nom = name,
                Prenom = surname,
                Tel = phoneNumber,
                Mail = email,
                Mdp = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(password))),
                CreateAt = DateTime.Now
            };

            await _context.Clients.AddAsync(client);
            await _context.SaveChangesAsync();

            var compte = new Compte
            {
                Type = typeAccount,
                Solde = initialDeposit,
                CreateAt = DateTime.Now,
                Interet = 0.0m,
                Rib = Guid.NewGuid(),
                NumeroCompte = $"{Random.Shared.Next(100, 1000)}-{Random.Shared.Next(100, 1000)}-{Random.Shared.Next(100, 1000)}",
                IdClient = client.IdClient
            };

            await _context.Comptes.AddAsync(compte);
            await _context.SaveChangesAsync();
            
            await transaction.CommitAsync();
            _logger.LogInformation($"Client {client.IdClient} créé avec son compte {compte.IdCompte}");
            
            return "Inscription effectuée avec succès";
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync();
            _logger.LogError($"Erreur lors de l'enregistrement d'un utilisateur : {e.Message}");
            return "Erreur lors de l'inscription";
        }
    }

    public async Task<string> MoveMoney(string accountReceiver, int amount, int action)
    {
        try
        {
            var receiver = await _context.Comptes.FirstOrDefaultAsync(x => x.NumeroCompte == accountReceiver);
            if (receiver == null)
            {
                return "Compte introuvable";
            }
            
            if (action == 2 && receiver.Solde < amount)
            {
                return "Solde insuffisant pour effectuer ce retrait";
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                if (action == 1)
                {
                    receiver.Solde += amount;
                }
                else
                {
                    receiver.Solde -= amount;
                }
                
                var depotHistorique = new Historique
                {
                    DateOperation = DateTime.Now,
                    Montant = amount,
                    TypeOperation = action == 1 ? "Dépôt" : "Retrait",
                    Donneur = receiver.IdCompte,  
                    Receveur = receiver.IdCompte
                };

                await _context.Historiques.AddAsync(depotHistorique);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return "Transaction effectuée avec succès";
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError($"Erreur lors de la transaction : {ex.Message}");
                return "Erreur lors de la transaction";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Erreur dans MoveMoney : {ex.Message}");
            return "Une erreur est survenue";
        }
    }

    public async Task<string> TransferMoney(string senderAccount, string receiverAccount, int amount)
    {
        try
        {
            var sender = await _context.Comptes.FirstOrDefaultAsync(x => x.NumeroCompte == senderAccount);
            var receiver = await _context.Comptes.FirstOrDefaultAsync(x => x.NumeroCompte == receiverAccount);

            if (sender == null || receiver == null)
            {
                return "Compte émetteur ou destinataire introuvable";
            }

            if (sender.Solde < amount)
            {
                return "Solde insuffisant pour effectuer le transfert";
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                sender.Solde -= amount;
                receiver.Solde += amount;

                var transferHistorique = new Historique
                {
                    DateOperation = DateTime.Now,
                    Montant = amount,
                    TypeOperation = "Virement sortant",
                    Donneur = sender.IdCompte,
                    Receveur = receiver.IdCompte
                };

                var receiveHistorique = new Historique
                {
                    DateOperation = DateTime.Now,
                    Montant = amount,
                    TypeOperation = "Virement entrant",
                    Donneur = sender.IdCompte,
                    Receveur = receiver.IdCompte
                };

                await _context.Historiques.AddAsync(transferHistorique);
                await _context.Historiques.AddAsync(receiveHistorique);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return "Transfert effectué avec succès";
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError($"Erreur lors du transfert : {ex.Message}");
                return "Erreur lors du transfert";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Erreur dans TransferMoney : {ex.Message}");
            return "Une erreur est survenue";
        }
    }
}
