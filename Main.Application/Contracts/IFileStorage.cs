namespace Main.Application.Contracts;

/// <summary>
/// Интерфейс файлового хранилища для постеров.
/// </summary>
public interface IFileStorage
{
    /// <summary>
    /// Загрузить файл из хранилища.
    /// </summary>
    /// <param name="key">Идентификатор (ключ) файла в хранилище.</param>
    /// <returns>Байтовый поток файла</returns>
    Task<MemoryStream> DownloadFileAsync(string key);

    /// <summary>
    /// Загрузить файл в хранилище.
    /// </summary>
    /// <param name="stream">Поток байтов, представляющие содержимое файла.</param>
    /// <param name="fileName">Имя файла.</param>
    /// <returns>Идентификатор (ключ) файла в хранилище.</returns>
    Task<string> UploadFileAsync(Stream stream, string fileName);
}