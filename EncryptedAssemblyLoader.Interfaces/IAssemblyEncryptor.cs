using System.Reflection;

namespace EncryptedAssemblyLoader.Interfaces
{
    public interface IAssemblyEncryptor : IDisposable
    {
        string? DllFilePath { get; set; }

        byte[]? AssemblyData { get; set; }

        Assembly? Assembly { get; set; }

        Stream? AssemblyStream { get; set; }

        IAssemblyEncryptor SetPassword(string password);

        IAssemblyEncryptor SetKey(string key, int size);

        IAssemblyEncryptor SetIV(string iv);

        Stream Encrypt(Stream outputStream);
    }
}
