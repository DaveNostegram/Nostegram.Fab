using FluentValidation;
using Nostegram.Fab.Contracts.Cards;
using Nostegram.Fab.Contracts.Common;

namespace Nostegram.Fab.Application.ReferenceData.Cards.Validators;

public class SetDetailWriteDtoValidator
    : AbstractValidator<SetDetailWriteDto>
{
    public SetDetailWriteDtoValidator()
    {
        RuleFor(x => x.Rarity)
            .NotEmpty();

        RuleFor(x => x.SetId)
            .NotEmpty();

        RuleFor(x => x.CollectorNumber)
            .NotEmpty();

        RuleFor(x => x.ArtistId)
            .NotEmpty();
    }
}