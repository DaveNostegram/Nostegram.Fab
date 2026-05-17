using FluentValidation;
using Nostegram.Fab.Contracts.Common;

namespace Nostegram.Fab.Application.Common.Validators;

public class LookupItemWriteDtoValidator
    : AbstractValidator<LookupItemWriteDto>
{
    public LookupItemWriteDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(150);
    }
}