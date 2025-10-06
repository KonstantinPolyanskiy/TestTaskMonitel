namespace Main.DAL.Minio;

/// <summary>
/// Класс конфигурации Minio.
/// </summary>
public class MinioSettings
{
    /// <summary>
    /// Строка подключения.
    /// </summary>
    public string Endpoint { get; set; } = string.Empty;

    /// <summary>
    /// Логин.
    /// </summary>
    public string AccessKey { get; set; } = string.Empty;

    /// <summary>
    /// Пароль.
    /// </summary>
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>
    /// Подключение через защищенные протоколы.
    /// </summary>
    public bool IsSecure {  get; set; }
}