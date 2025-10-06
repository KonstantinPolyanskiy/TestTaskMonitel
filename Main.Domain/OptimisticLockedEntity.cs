using Main.Domain.Exceptions;

namespace Main.Domain;

/// <summary>
/// Базовый класс, позволяющий защитить модель оптимистичной блокировкой.
/// </summary>
public abstract class OptimisticLockedEntity
{
    /// <summary>
    /// Актуальная версия сущности.
    /// </summary>
    public int ActualVersion { get; set; }

    /// <summary>
    /// Проверка версии сущности
    /// </summary>
    /// <param name="externalVersion">Клиентская версия, то есть та версия сущности, с которой изначально работал пользователь</param>
    /// <exception cref="AlreadyChangedException">Выбрасывается в случае, если клиентская и актуальная версии сущности отличаются</exception>
    protected void CheckVersion(int externalVersion)
    {
        if (ActualVersion != externalVersion)
        {
            throw new AlreadyChangedException($"Объект {GetType().Name} неактуальной версии, возник конфликт.");
        }
    }

    /// <summary>
    /// Поднять текущую версию.
    /// </summary>
    protected void UpVersion()
    {
        ActualVersion++;
    }
}