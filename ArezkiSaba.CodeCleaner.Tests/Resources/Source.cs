﻿using System.Reflection;
using Microsoft.Extensions.Options;
using Prevoir.Toolkit.Bristol.Extensions;
using Prevoir.Toolkit.Bristol.Models.Options;
using Prevoir.Toolkit.OAV.Bristol.Infrastructure.Database;
using Prevoir.Toolkit.OAV.Bristol.Services.Contracts;
using Prevoir.Toolkit.OAV.Bristol.Services.Documents.MandatSEPA.Services;
using Prevoir.Toolkit.OAV.Bristol.Services.Documents.MandatSEPA.Services.Contracts;
using Prevoir.Toolkit.Web.Security.Services;
using Prevoir.Toolkit.Web.Security.Services.Contracts;
using Prevoir.ServiceDirectory.ApiClient;
using Prevoir.SolutionCapitalObseques.Bristol.DataAccess.Services;
using Prevoir.SolutionCapitalObseques.Bristol.Domain.SaisieBulletinSouscription.Models;
using Prevoir.SolutionCapitalObseques.Bristol.Infrastructure.Database;
using Prevoir.SolutionCapitalObseques.Bristol.Infrastructure.Database.Bareme;
using Prevoir.SolutionCapitalObseques.Bristol.Infrastructure.Database.Bareme.Contracts;
using Prevoir.SolutionCapitalObseques.Bristol.Infrastructure.Database.Etag;
using Prevoir.SolutionCapitalObseques.Bristol.Infrastructure.Database.Etag.Contracts;
using Prevoir.SolutionCapitalObseques.Bristol.Infrastructure.Database.ReferentielEsperanceVie;
using Prevoir.SolutionCapitalObseques.Bristol.Infrastructure.Database.ReferentielEsperanceVie.Contracts;
using Prevoir.SolutionCapitalObseques.Bristol.Infrastructure.Database.Simulation;
using Prevoir.SolutionCapitalObseques.Bristol.Infrastructure.Database.Simulation.Contracts;
using Prevoir.SolutionCapitalObseques.Bristol.Models;
using Prevoir.SolutionCapitalObseques.Bristol.Services;
using Prevoir.SolutionCapitalObseques.Bristol.Services.Documents.BulletinDeSouscription.Services;
using Prevoir.SolutionCapitalObseques.Bristol.Services.Documents.BulletinDeSouscription.Services.Contracts;
using Prevoir.SolutionCapitalObseques.Bristol.Services.Documents.FluxAIA.Services;
using Prevoir.SolutionCapitalObseques.Bristol.Services.Documents.FluxAIA.Services.Contracts;
using Prevoir.SolutionCapitalObseques.Bristol.Services.Eligibilite;
using Prevoir.SolutionCapitalObseques.Bristol.Services.Eligibilite.Contracts;
using Prevoir.Web.Board.ApiClient;
using Prevoir.Web.Board.ApiClient.Contracts;

namespace Prevoir.SolutionCapitalObseques.Bristol;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection serviceCollection,
        IServiceDirectoryApiClient serviceDirectoryApiClient)
    {
        serviceCollection.AddSingleton<ISecretService>(new RegistrySecretService(webHostEnvironment.EnvironmentName, Assembly.GetExecutingAssembly().GetName().Name), new BoardApiClient(boardApiOptions.Value.Url, secretService.Decrypt(boardApiOptions.Value.ApiKey));

        serviceCollection.AddOctavApiClient(serviceDirectoryApiClient);
        serviceCollection.AddOctavSolutionsApiClient(serviceDirectoryApiClient);

        // OAV Common
        serviceCollection.AddScoped<IDatabaseService, DatabaseService>();
        serviceCollection.AddScoped<IOavDatabaseServiceBase, DatabaseService>();
        serviceCollection.AddScoped<ISimulationRepository, SimulationRepository>();
        serviceCollection.AddScoped<IProduitsService, ProduitsService>();
        ////serviceCollection.AddScoped<IAdherentService, AdherentService>();
        serviceCollection.AddScoped<IEligibiliteService, EligibiliteService>();
        serviceCollection.AddScoped<IBulletinDeSouscriptionService, BulletinDeSouscriptionService>();
        serviceCollection.AddScoped<IMandatSEPAService, MandatSEPAService<SaisieBulletinSouscriptionModel>>();
        ////serviceCollection.AddScoped<IDevisService, DevisService>();
        serviceCollection.AddScoped<IFluxAIAService, FluxAIAService>();
        serviceCollection.AddScoped<IPiecesJustificativesService, PiecesJustificativesService>();
        serviceCollection.AddScoped<IBoardApiClient>(serviceProvider =>
        {
            var secretService = serviceProvider.GetService<ISecretService>();
            var boardApiOptions = serviceProvider.GetService<IOptions<BoardApiOptions>>();
            return new BoardApiClient(boardApiOptions.Value.Url, secretService.Decrypt(boardApiOptions.Value.ApiKey));
        }
        );

        // OAV Specific
        serviceCollection.AddScoped<ISimulationRepository, SimulationRepository>();
        serviceCollection.AddScoped<ISimulateurService, SimulateurService>();

        // Domain

        // Infra
        serviceCollection.AddScoped<IReferentielEsperanceVieRepository, ReferentielEsperanceVieRepository>();
        serviceCollection.AddScoped<IBaremeRepository, BaremeRepository>();
        serviceCollection.AddScoped<IEtagRepository, EtagRepository>();
        serviceCollection.AddScoped<ISimulationRepository, SimulationRepository>();
        return serviceCollection;
    }

    public static IServiceCollection AddDefaultApiFeatures(
        this IServiceCollection serviceCollection)
    {
        return serviceCollection;
    }

    public static IServiceCollection AddDefaultAuthentication(
        this IServiceCollection serviceCollection)
    {
        return serviceCollection;
    }

    public static IServiceCollection AddOptions(
        this IServiceCollection serviceCollection,
        IConfiguration configuration)
    {
        serviceCollection.Configure<BoardApiOptions>(configuration.GetSection("BoardApi"));
        serviceCollection.Configure<AppSettings>(configuration.GetSection("AppSettings"));
        return serviceCollection;
    }

    public static IServiceCollection AddSecretService(
        this IServiceCollection serviceCollection,
        IWebHostEnvironment webHostEnvironment)
    {
        serviceCollection.AddSingleton<ISecretService>(new RegistrySecretService(webHostEnvironment.EnvironmentName, Assembly.GetExecutingAssembly().GetName().Name));
        return serviceCollection;
    }
}
