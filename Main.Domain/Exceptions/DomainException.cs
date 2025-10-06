namespace Main.Domain.Exceptions;

/// <summary>
/// Базовый класс для исключений слоя бизнес логики и ее валидации.
/// </summary>
public abstract class DomainException : Exception
{
    public int StatusCode { get; }

    protected DomainException(string message, int statusCode)
        : base(message) => StatusCode = statusCode;
}


/// <summary>
/// Исключение бизнес логики в случае, если валидация не прошла.
/// </summary>
public sealed class DomainValidationException : DomainException
{
    public DomainValidationException(string message)
        : base(message, 400) { }
}

/// <summary>
/// Исключение бизнес логики в случае, если случился конфликт версий.
/// </summary>
public sealed class DomainConflictException : DomainException
{
    public DomainConflictException(string message)
        : base(message, 409) { }
}

/// <summary>
/// Исключение бизнес логики в случае, если необходимая доменная сущность не была найдена.
/// </summary>
public sealed class DomainNotFoundException : DomainException
{
    public DomainNotFoundException(string message)
        : base(message, 404) { }
}