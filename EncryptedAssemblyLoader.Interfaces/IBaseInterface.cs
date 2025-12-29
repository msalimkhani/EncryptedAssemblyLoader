namespace EncryptedAssemblyLoader.Interfaces
{
    public interface IBaseInterface<TInterface>
    {
        TInterface SetPassword(string password);

        TInterface SetKey(string key, int size);

        TInterface SetIV(string iv);
    }
}
