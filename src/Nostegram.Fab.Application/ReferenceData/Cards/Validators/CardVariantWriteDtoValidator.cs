using FluentValidation;
using Nostegram.Fab.Contracts.Cards;
using Nostegram.Fab.Contracts.Common;

namespace Nostegram.Fab.Application.ReferenceData.Cards.Validators;

public class CardVariantWriteDtoValidator
    : AbstractValidator<CardVariantWriteDto>
{
    public CardVariantWriteDtoValidator()
    {
        RuleFor(x => x.SetDetails)
            .NotEmpty()
            .WithMessage("At least one set detail is required.");

        RuleForEach(x => x.SetDetails)
            .SetValidator(new SetDetailWriteDtoValidator());

        RuleFor(x => x.Pitch)
            .NotNull()
            .WithMessage("Pitch is required.");
    }
}