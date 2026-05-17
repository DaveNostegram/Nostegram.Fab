using System.Text.RegularExpressions;

namespace Nostegram.Fab.Application.Common.Normalisations;

public static class NameNormaliser
{
    public static string ForDisplay(string? value)
    {
        return Regex.Replace((value ?? string.Empty).Trim(), @"\s+", " ");
    }
}