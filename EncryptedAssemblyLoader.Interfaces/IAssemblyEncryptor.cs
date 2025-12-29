using System.Reflection;

namespace EncryptedAssemblyLoader.Interfaces
{
    public interface IAssemblyEncryptor : IDisposable, IBaseInterface<IAssemblyEncryptor>
    {
        string? DllFilePath { get; set; }

        byte[]? AssemblyData { get; set; }

        Assembly? Assembly { get; set; }

        Stream? AssemblyStream { get; set; }

        TStream Encrypt<TStream>(TStream outputStream) where TStream : Stream;
    }
}
