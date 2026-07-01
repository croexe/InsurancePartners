namespace Partners.Core.Exceptions;

public sealed class DuplicateExternalCodeException(string externalCode)
    : Exception($"ExternalCode '{externalCode}' is already in use.")
{
    public string ExternalCode { get; } = externalCode;
}
