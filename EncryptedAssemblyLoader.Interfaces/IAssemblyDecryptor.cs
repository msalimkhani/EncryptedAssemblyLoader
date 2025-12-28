namespace EncryptedAssemblyLoader.Interfaces
{
    public interface IAssemblyDecryptor
    {
        string? EncryptedDllFilePath { get; set; }

        byte[]? EncryptedAssemblyData { get; set; }

        Stream? EncryptedAssemblyStream { get; set; }

        IAssemblyDecryptor SetPassword(string password);

        IAssemblyDecryptor SetKey(string key, int size);

        IAssemblyDecryptor SetIV(string iv);

        Stream Decrypt(Stream outputStream);
    }
}
