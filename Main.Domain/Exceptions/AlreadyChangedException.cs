namespace Main.Domain.Exceptions;

/// <summary>
/// Исключение, выбрасываемое в случае, если произошла попытка обновить неактуальную сущностью
/// </summary>
public sealed class AlreadyChangedException : Exception
{
    public AlreadyChangedException(string message) : base(message) { }
}