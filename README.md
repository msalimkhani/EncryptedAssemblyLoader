# 🛡️ Encrypted Assembly Loader

A specialized .NET library designed to protect your Intellectual Property. This tool allows you to encrypt DLL files using AES and load them directly into memory, preventing the decrypted assembly from ever touching the disk.

It further bridges the gap between encrypted types and your host application by **dynamically generating proxy classes** that map unreferenced types to local interfaces at runtime using Reflection.Emit.

---

## ✨ Key Features

* **AES Encryption:** Secure your DLLs using either a custom Key/IV pair or a password-based PBKDF2 derivation.
* **In-Memory Execution:** Decrypts and loads assemblies directly into the process memory (`System.Reflection.Assembly.Load`).
* **Dynamic Duck-Typing Proxy:** Automatically generates a proxy class at runtime that allows an encrypted type to implement a local interface, even if the original class didn't "know" about the interface at compile-time.
* **DI Integration:** Full support for `ActivatorUtilities`, allowing your dynamically loaded types to receive dependencies from the host's `IServiceProvider`.

---

## 🛠 Implementation Checklist

- [x] **Encryption - Decryption Engine:** Full AES implementation with support for Key/IV and Password signing.
- [x] **In-Memory Loading:** Seamless assembly loading from decrypted byte arrays.
- [x] **Dynamic Proxy Generation:**
    - [x] **Type Inheritance:** Runtime `TypeBuilder` logic to create `Proxy : BaseClass, IInterface`.
    - [x] **Signature Matching:** Strict validation for Methods and Properties (including getters/setters).
    - [x] **Constructor Forwarding:** Automatic IL generation to support Dependency Injection via `base(...)` calls.
- [ ] **Decorator Attributes:** (Planned) Custom mapping for Type or Member names via attributes to allow mismatched signatures.

---

## 🚀 How it Works

### 1. Transparent Proxying
When you request a type from an encrypted assembly that matches a local interface "shape," the library bakes a new type in a dynamic assembly. This allows you to use standard C# casting while keeping the underlying implementation hidden.



### 2. Constructor Forwarding & DI
Unlike basic reflection, this loader creates "Pass-Through" constructors. This means if your encrypted class requires services (like a Database Context or Logger), the generated Proxy forwards those requirements to the host's `IServiceProvider`.

```csharp
// The generated IL logic creates this structure at runtime:
public class GeneratedProxy : EncryptedClass, ILocalInterface 
{
    // Forwards DI dependencies to the base class constructor
    public GeneratedProxy(IDbContext db, ILogger log) : base(db, log) { }
    
    // Interface members are automatically mapped to existing base methods
}
```

---


## 🤝 Contributions

### Contributions are welcome! If you have ideas for improving the security or the proxy generation logic:

- 1. Fork the repository.
- 2. Create your feature branch.
- 3. Submit a Pull Request.

## 📝 License
This project is licensed under the BSD-2-Clause License.
