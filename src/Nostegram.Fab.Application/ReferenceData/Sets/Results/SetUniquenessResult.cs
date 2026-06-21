namespace Nostegram.Fab.Application.ReferenceData.Sets.Results;

public sealed record SetUniquenessResult(
    bool NameExists,
    bool SetCodeExists);