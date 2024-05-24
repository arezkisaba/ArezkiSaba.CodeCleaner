using System;
using System.Drawing;
using Windows;
using Windows.Storage;
using ArezkiSaba.CodeCleaner;
using ArezkiSaba.CodeCleaner.Tests;
using static System.Math;
using NativeHttpClient = System.Net.Http.HttpClient;

namespace ArezkiSaba.CodeCleaner.Tests;

public class TestClass1<T>
{
    [HttpGet("")]
    public async Task<JsonResult> GetBeneficiairesAsync(
        string cleProduit,
        string idVisite,
        string idPersonne)
    {
        When(
            declarationAdherent => declarationAdherent.IsVisible,
            ()
=>
            {
                RuleFor(declarationAdherent => declarationAdherent.IsVisite)
                .Mandatory();
            }
        );

        var saisieBulletinSouscription = await GetSaisieBulletinSouscriptionDomainAsync(cleProduit, idVisite, idPersonne);
        return Json(
            new
            {
                areQuotitesValides = await AreQuotitesValidesAsync(saisieBulletinSouscription.Beneficiaires),
                beneficiaires = saisieBulletinSouscription.Beneficiaires.BeneficiairesAClauseNominative.Select(ReflectionHelper.CreateFrom<BeneficiaireModel, BeneficiaireDto, IBeneficiaireModel>).ToList(),
                descriptionClauseBeneficiaire = saisieBulletinSouscription.Beneficiaires.GetDescriptionClauseBeneficiaire()
            }
        );
    }

}

private record DummyClass(string parameter1, string parameter2, string parameter3, string parameter4);
