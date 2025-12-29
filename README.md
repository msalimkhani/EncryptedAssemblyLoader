# Encrypted Assembly Loader

this dotnet library allows to encrypt the DLL file, In memory Loaded DLL file as Assembly with AES Encryption algorithm with one of these signing methods:
- your Own Key and IV
- your Own Password

———

# Implementation Checklist:

- [x] Encryption - Decryption
- [ ] Load Encrypted Assembly to Memory
- [ ] Possibility of Wrapping an Encrypted Assembly Defined Types to Given Interface ( a proxy class will be generated between Interface and Assembly Type ), which Type Name or Member name Can be specified also in Decorator Attributes.