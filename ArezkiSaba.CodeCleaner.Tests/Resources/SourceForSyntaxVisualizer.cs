using FluentValidation;
using Prevoir.SolutionCapitalObseques.Bristol.Domain.SaisieBulletinSouscription.Models;
using Prevoir.SolutionCapitalObseques.Bristol.Domain.SaisieBulletinSouscription.Models.Contracts;
using Prevoir.Toolkit.OAV.Bristol.Domain.SaisieBulletinSouscription.PropertyValidators.Extensions;
using Prevoir.Toolkit.OAV.Bristol.Domain.SaisieBulletinSouscription.Services.Contracts;

namespace Prevoir.SolutionCapitalObseques.Bristol.Domain.SaisieBulletinSouscription.Validators;

public class BeneficiairesValidator : AbstractValidator<IBeneficiairesModel>
{
    public BeneficiairesValidator(
        IRechercheAdresseService rechercheAdresseService,
        IRecherchePaysService recherchePaysService)
    {
        When(beneficiaires => beneficiaires.IsVisible, () =>
        {
            RuleFor(beneficiaires => beneficiaires.IsVisite).Mandatory();
            RuleFor(beneficiaires => beneficiaires.Clause).NotNull();
            RuleFor(beneficiaires => beneficiaires.BeneficiairesAClauseNominative)
                .NotEmptyWhenEqual(beneficiaires => beneficiaires.Clause, ClauseBeneficiairesModel.Nominative)
                .Must(beneficiaires =>
                {
                    if (beneficiaires == null || !beneficiaires.Any())
                    {
                        return true;
                    }

                    return beneficiaires.All(obj => obj.Quotite != null) && beneficiaires.Sum(b => b.Quotite.GetValueOrDefault()) == 100;
                })
                .When(beneficiaires => beneficiaires.Clause == ClauseBeneficiairesModel.Nominative);
            RuleForEach(beneficiaires => beneficiaires.BeneficiairesAClauseNominative)
                .SetValidator(new BeneficiaireValidator(rechercheAdresseService, recherchePaysService))
                .When(beneficiaires => beneficiaires.Clause == ClauseBeneficiairesModel.Nominative);
            RuleFor(beneficiaires => beneficiaires.ClauseType)
                .NotEmptyWhenEqual(beneficiaires => beneficiaires.Clause, ClauseBeneficiairesModel.Type);
            RuleFor(beneficiaires => beneficiaires.ActeAuthentiqueNom)
                .NotEmptyWhenEqual(beneficiaires => beneficiaires.ClauseType, ClauseTypeBeneficiairesModel.Choix5);
            RuleFor(beneficiaires => beneficiaires.ActeAuthentiquePrenom)
                .NotEmptyWhenEqual(beneficiaires => beneficiaires.ClauseType, ClauseTypeBeneficiairesModel.Choix5);
            RuleFor(beneficiaires => beneficiaires.ActeAuthentiqueAdresse1)
                .NotEmptyWhenEqual(beneficiaires => beneficiaires.ClauseType, ClauseTypeBeneficiairesModel.Choix5);
            RuleFor(beneficiaires => beneficiaires.ActeAuthentiqueCodePostal)
                .NotEmptyWhenEqual(beneficiaires => beneficiaires.ClauseType, ClauseTypeBeneficiairesModel.Choix5)
                .CodePostal()
                .CodePostalVille(
                    beneficiaires => beneficiaires.ActeAuthentiqueCodePostal,
                    beneficiaires => beneficiaires.ActeAuthentiqueVille,
                    rechercheAdresseService,
                    isLivrable: true);
            RuleFor(beneficiaires => beneficiaires.ActeAuthentiqueVille)
                .NotEmptyWhenEqual(beneficiaires => beneficiaires.ClauseType, ClauseTypeBeneficiairesModel.Choix5)
                .Ville(isLivrable: true)
                .CodePostalVille(
                    beneficiaires => beneficiaires.ActeAuthentiqueCodePostal,
                    beneficiaires => beneficiaires.ActeAuthentiqueVille,
                    rechercheAdresseService,
                    isLivrable: true);
            RuleFor(beneficiaires => beneficiaires.ClauseAutre)
                .NotEmptyWhenEqual(beneficiaires => beneficiaires.Clause, ClauseBeneficiairesModel.Autre)
                .MaximumLength(1200);
        });
    }
}
