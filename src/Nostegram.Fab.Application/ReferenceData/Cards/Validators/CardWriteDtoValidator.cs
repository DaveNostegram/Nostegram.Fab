using FluentValidation;
using Nostegram.Fab.Contracts.Cards;
using Nostegram.Fab.Contracts.Common;

namespace Nostegram.Fab.Application.ReferenceData.Cards.Validators;

public class CardWriteDtoValidator
    : AbstractValidator<CardWriteDto>
{
    public CardWriteDtoValidator()
    {
        RuleFor(x => x.CardVariants)
            .NotEmpty()
            .WithMessage("At least one card variant is required.");

        RuleFor(x => x.CardVariants)
            .Must(HaveUniquePitches)
            .When(x => x.CardVariants is not null)
            .WithMessage("Card variants cannot contain duplicate pitches.");

        RuleForEach(x => x.CardVariants)
            .SetValidator(new CardVariantWriteDtoValidator());
    }

    private static bool HaveUniquePitches(
        IEnumerable<CardVariantWriteDto> variants)
    {
        return variants
            .Select(x => x.Pitch)
            .Distinct()
            .Count() == variants.Count();
    }
}