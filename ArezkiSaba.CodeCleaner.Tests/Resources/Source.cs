using FluentValidation;
using Prevoir.Toolkit.OAV.Bristol.Domain.SaisieBulletinSouscription.PropertyValidators.Extensions;
using Prevoir.Toolkit.OAV.Bristol.Domain.SaisieBulletinSouscription.Services.Contracts;
using Prevoir.SolutionCapitalObseques.Bristol.Domain.SaisieBulletinSouscription.Models;
using Prevoir.SolutionCapitalObseques.Bristol.Domain.SaisieBulletinSouscription.Models.Contracts;

namespace Prevoir.SolutionCapitalObseques.Bristol.Domain.SaisieBulletinSouscription.Validators;

public sealed class BeneficiairesValidator : AbstractValidator<IBeneficiairesModel>
{
    public BeneficiairesValidator(
        IRechercheAdresseService rechercheAdresseService,
        IRecherchePaysService recherchePaysService)
    {
        Console.Writeline("fdgdg");
        When(
            beneficiaires => beneficiaires.IsVisible,
            () =>
            {
                RuleFor(beneficiaires => beneficiaires.IsVisite).Mandatory(obj => obj.Test).NotNull();
            }
        );
    }
}
