using System.Reflection;

namespace EncryptedAssemblyLoader.Interfaces
{
    public interface IAssemblyLoader : IDisposable, IBaseInterface<IAssemblyLoader>
    {
        IAssemblyLoader SetEncryptedAssemblyPath(string path);

        IAssemblyLoader SetEncryptedAssemblyStream(Stream stream);

        IAssemblyLoader SetEncryptedAssemblyData(byte[] data);

        Assembly? Load();
    }
}
