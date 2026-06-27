using FluentValidation;
using Nostegram.Fab.Contracts.Sets;

namespace Nostegram.Fab.Application.ReferenceData.Sets.Validators;

public class SetWriteDtoValidator
    : AbstractValidator<SetWriteDto>
{
    public SetWriteDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(150);
        RuleFor(x => x.SetCode)
            .NotEmpty()
            .MaximumLength(3);
        RuleFor(x => x.SetCode)
            .Must(date => date != default)
            .WithMessage("Release date is required.");
    }
}