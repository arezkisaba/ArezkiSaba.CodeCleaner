using Microsoft.AspNetCore.Mvc;
using Prevoir.Octav.Dto.Extensions;
using Prevoir.Octav.ApiClient;
using Prevoir.SolutionCapitalObseques.Bristol.Domain.Produit.Caracteristiques;
using Prevoir.SolutionCapitalObseques.Bristol.Domain.Simulation;
using Prevoir.SolutionCapitalObseques.Bristol.Domain.Simulation.Factory;
using Prevoir.SolutionCapitalObseques.Bristol.Infrastructure.Database.Bareme;
using Prevoir.SolutionCapitalObseques.Bristol.Infrastructure.Database.Simulation;
using Prevoir.SolutionCapitalObseques.Bristol.Infrastructure.Database.Simulation.Contracts;
using Prevoir.SolutionCapitalObseques.Common;
using Prevoir.SolutionCapitalObseques.Common.Bodies;
using Prevoir.SolutionCapitalObseques.Common.Common;
using Prevoir.SolutionCapitalObseques.Common.Responses;
using Prevoir.Toolkit.Bristol.ApiControllers.Bases;
using Prevoir.Toolkit.Utils.Extensions;
using DureePaiement = Prevoir.SolutionCapitalObseques.Bristol.Domain.Commun.DureePaiement;
using Periodicite = Prevoir.SolutionCapitalObseques.Bristol.Domain.Commun.Periodicite;

namespace Prevoir.SolutionCapitalObseques.Bristol.ApiControllers;

[Route($"{RoutePrefix}/{Constantes.RouteSimulationBase}")]
public class SimulationController : ApiControllerBase
{
    private readonly IOctavApiClient _octavApiClient;
    private readonly ISimulationRepository _simulationRepository;
    private readonly ISimulateurService _simulateurService;

    public SimulationController(
        IOctavApiClient octavApiClient,
        ISimulationRepository simulationRepository,
        ISimulateurService simulateurService)
    {
        _octavApiClient = octavApiClient;
        _simulationRepository = simulationRepository;
        _simulateurService = simulateurService;
    }

    [HttpPost("initialiser")]
    public async Task<InitialiserSimulationResponseDto> InitialiserSimulationAsync(
        [FromBody] InitialiserSimulationBodyDto command)
    {
        var simulation = SimulationFactory.CreerSimulation(command.IdVisite, command.IdPersonne);
        var simulationParDefaut = GetDonneParDefautFromSimulation(simulation);

        var simulationEntity = (await _simulationRepository.GetByEntretienEtAdherentAsync(command.IdVisite, command.IdPersonne)).SingleOrDefault();
        if (simulationEntity == null)
        {
            var entretien = await _octavApiClient.Entretien.GetAsync(command.IdVisite);
            var personne = entretien.Entretien.Foyer.GetClientById(command.IdPersonne);
            var age = personne.EtatCivil.DateNaissance.Value.CalculerAge();

            if (age >= 50 && age <= 55)
            {
                simulationParDefaut.DureePaiement = DureePaiementDto.Vingt;
            }
            else
            {
                if (age >= 56 && age <= 63)
                {
                    simulationParDefaut.DureePaiement = DureePaiementDto.Quinze;
                }
                else
                {
                    if (age >= 64 && age <= 71)
                    {
                        simulationParDefaut.DureePaiement = DureePaiementDto.Dix;
                    }
                    else
                    {
                        if (age >= 72 && age <= 80)
                        {
                            simulationParDefaut.DureePaiement = DureePaiementDto.Viager;
                        }
                    }
                }
            }

            simulationEntity = new SimulationEntity
            {
                EntretienId = command.IdVisite,
                AdherentId = command.IdPersonne,
                IsGarantieDoublementAccidentSelectionnee = simulationParDefaut.IsGarantieDoublementAccidentSelectionnee,
                DureePaiement = (DureeDePaiement)simulationParDefaut.DureePaiement,
                Periodicite = (Infrastructure.Database.Bareme.Periodicite)simulationParDefaut.Periodicite,
                CotisationGarantieCapitalDeces = simulationParDefaut.CotisationGarantieCapitalDeces,
                CotisationGarantieDoublementAccident = simulationParDefaut.CotisationGarantieDoublementAccident,
                MontantGarantieDoublementAccident = simulationParDefaut.MontantGarantieDoublementAccident,
                MontantGarantieCapitalDeces = simulationParDefaut.MontantGarantieCapitalDeces,
                CotisationTotale = simulationParDefaut.CotisationTotale,
                DateEffet = simulationParDefaut.DateEffet.Date
            };

            await _simulationRepository.InsertAsync(simulationEntity);
        }

        var dureePaiement = DureePaiementDto.Dix;
        var periodicite = PeriodiciteDto.Mensuel;
        switch (simulationEntity.DureePaiement)
        {
            case DureeDePaiement.Dix:
                dureePaiement = DureePaiementDto.Dix;
                break;
            case DureeDePaiement.Quinze:
                dureePaiement = DureePaiementDto.Quinze;
                break;
            case DureeDePaiement.Vingt:
                dureePaiement = DureePaiementDto.Vingt;
                break;
            case DureeDePaiement.Viager:
                dureePaiement = DureePaiementDto.Viager;
                break;
        }

        switch (simulationEntity.Periodicite)
        {
            case Infrastructure.Database.Bareme.Periodicite.Mensuelle:
                periodicite = PeriodiciteDto.Mensuel;
                break;
            case Infrastructure.Database.Bareme.Periodicite.Trimestrielle:
                periodicite = PeriodiciteDto.Trimestriel;

                break;
            case Infrastructure.Database.Bareme.Periodicite.Semestrielle:
                periodicite = PeriodiciteDto.Semestriel;
                break;
        }

        simulationParDefaut.CotisationGarantieCapitalDeces = simulationEntity.CotisationGarantieCapitalDeces;
        simulationParDefaut.DateEffet = simulationEntity.DateEffet;
        simulationParDefaut.CotisationGarantieDoublementAccident = simulationEntity.CotisationGarantieDoublementAccident;
        simulationParDefaut.CotisationTotale = simulationEntity.CotisationTotale;
        simulationParDefaut.DureePaiement = dureePaiement;
        simulationParDefaut.IsGarantieDoublementAccidentSelectionnee = simulationEntity.IsGarantieDoublementAccidentSelectionnee;
        simulationParDefaut.MontantGarantieCapitalDeces = simulationEntity.MontantGarantieCapitalDeces;
        simulationParDefaut.MontantGarantieDoublementAccident = simulationEntity.MontantGarantieDoublementAccident;
        simulationParDefaut.Periodicite = periodicite;

        return simulationParDefaut;
    }

    [HttpPost("modifier")]
    public async Task<ModifierSimulationResponseDto> ModifierAsync(
        [FromBody] ModifierSimulationBodyDto command)
    {
        var simulationEntity = (await _simulationRepository.GetByEntretienEtAdherentAsync(command.IdVisite, command.IdPersonne)).SingleOrDefault();
        if (simulationEntity == null)
        {
            throw new InvalidOperationException("Adherent not found");
        }

        var entretien = await _octavApiClient.Entretien.GetAsync(command.IdVisite);
        var personne = entretien.Entretien.Foyer.GetClientById(command.IdPersonne);
        var age = personne.EtatCivil.DateNaissance.Value.CalculerAge();

        var bareme = await _simulateurService.CalculerCotisationsDeLaSimulationAsync(age, command.Montant, (int)command.Periodicite, (int)command.DureePaiement);
        var montantGarantieDoublementAccident = 0d;
        var cotisationTotale = bareme.PrimeGarantiesObligatoire;
        var periodiciteEntity = GetPeriodicite(command.Periodicite);
        var dureeDePaiementEntity = GetDureeDePaiement(command.DureePaiement);

        var cotisationGarantieObligatoire = bareme.PrimeGarantiesObligatoire;
        var cotisationGarantieOptionelle = bareme.PrimeGarantiesOptionelle;

        if (command.IsGarantieDoublementAccidentSelectionnee)
        {
            montantGarantieDoublementAccident = command.Montant * 2;
            cotisationTotale = bareme.PrimeTotale;
        }

        if (command.IsGarantieDoublementAccidentSelectionnee)
        {
            simulationEntity.IsGarantieDoublementAccidentSelectionnee = true;
            simulationEntity.MontantGarantieDoublementAccident = montantGarantieDoublementAccident;
            simulationEntity.CotisationGarantieDoublementAccident = cotisationGarantieOptionelle;
        }
        else
        {
            simulationEntity.IsGarantieDoublementAccidentSelectionnee = false;
            simulationEntity.MontantGarantieDoublementAccident = 0;
            simulationEntity.CotisationGarantieDoublementAccident = 0;
        }

        simulationEntity.MontantGarantieCapitalDeces = command.Montant;
        simulationEntity.CotisationGarantieCapitalDeces = cotisationGarantieObligatoire;
        simulationEntity.Periodicite = periodiciteEntity;
        simulationEntity.DureePaiement = dureeDePaiementEntity;
        simulationEntity.CotisationTotale = cotisationTotale;

        await _simulationRepository.UpsertAsync(simulationEntity);

        return new ModifierSimulationResponseDto
        {
            CotisationGarantieCapitalDeces = simulationEntity.CotisationGarantieCapitalDeces,
            CotisationGarantieDoublementAccident = simulationEntity.CotisationGarantieDoublementAccident,
            MontantGarantieDoublementAccident = simulationEntity.MontantGarantieDoublementAccident,
            CotisationTotale = simulationEntity.CotisationTotale,
        };
    }

    #region Private use

    private Infrastructure.Database.Bareme.Periodicite GetPeriodicite(
        PeriodiciteDto periodicite)
    {
        Infrastructure.Database.Bareme.Periodicite periodiciteEntity;
        switch (periodicite)
        {
            case PeriodiciteDto.Mensuel:
                periodiciteEntity = Infrastructure.Database.Bareme.Periodicite.Mensuelle;
                break;
            case PeriodiciteDto.Trimestriel:
                periodiciteEntity = Infrastructure.Database.Bareme.Periodicite.Trimestrielle;
                break;
            case PeriodiciteDto.Semestriel:
                periodiciteEntity = Infrastructure.Database.Bareme.Periodicite.Semestrielle;
                break;
            default:
                throw new Exception("Periodicité non trouvée");
        }

        return periodiciteEntity;
    }

    private DureeDePaiement GetDureeDePaiement(
        DureePaiementDto dureePaiement)
    {
        DureeDePaiement dureeDePaiementEntity;
        switch (dureePaiement)
        {
            case DureePaiementDto.Dix:
                dureeDePaiementEntity = DureeDePaiement.Dix;
                break;
            case DureePaiementDto.Quinze:
                dureeDePaiementEntity = DureeDePaiement.Quinze;

                break;
            case DureePaiementDto.Vingt:
                dureeDePaiementEntity = DureeDePaiement.Vingt;

                break;
            case DureePaiementDto.Viager:
                dureeDePaiementEntity = DureeDePaiement.Viager;
                break;
            default:
                throw new Exception("durée de paiement non trouvée");
        }

        return dureeDePaiementEntity;
    }

    private InitialiserSimulationResponseDto GetDonneParDefautFromSimulation(
        Simulation simulation)
    {
    }

    #endregion
}
