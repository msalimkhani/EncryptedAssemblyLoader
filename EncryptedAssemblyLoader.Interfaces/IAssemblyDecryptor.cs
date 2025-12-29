namespace EncryptedAssemblyLoader.Interfaces
{
    public interface IAssemblyDecryptor : IDisposable, IBaseInterface<IAssemblyDecryptor>
    {
        string? EncryptedDllFilePath { get; set; }

        byte[]? EncryptedAssemblyData { get; set; }

        Stream? EncryptedAssemblyStream { get; set; }

        TStream Decrypt<TStream>(TStream outputStream) where TStream : Stream;
    }
}
