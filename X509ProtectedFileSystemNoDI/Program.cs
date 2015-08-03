using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNet.DataProtection;

namespace X509ProtectedFileSystemNoDI
{
    class Program
    {
        static void Main(string[] args)
        {
            const string keyStore = "dataProtectionSamples";
            const string appName = "X509ProtectedFileSystemNoDI";
            const string purpose = "Demonstration";

            var programKeyStore =
                Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    $"{keyStore}\\{appName}");
            Console.WriteLine($"Keys stored in\n{programKeyStore}");

            // Normally you'd have the certificate in the user certificate store, this is just for demo purposes.
            // Don't hardcode certificate passwords!
            // Certificate was generated with
            // makecert -r -pe -n "CN=Data Protection" -b 07/01/2015 -e 07/01/2020 -sky exchange -eku 1.3.6.1.4.1.311.10.3.12 -ss my
            
            var encryptingCertificate = new X509Certificate2(
                "protectionCertificate.pfx", 
                "password", 
                X509KeyStorageFlags.UserKeySet | X509KeyStorageFlags.Exportable | X509KeyStorageFlags.PersistKeySet);

            // You must put the cert in the store for unprotect to work. This is a limitation of the EncryptedXml class used to store the cert.
            var store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadWrite);
            store.Add(encryptingCertificate);
            store.Close();

            // instantiate the data protection system at this folder
            var dataProtectionProvider = new DataProtectionProvider(new DirectoryInfo(programKeyStore), 
                options =>
                    {
                        // As we're using a self signed certificate we need to provide an instance of the certificate.
                        // Thumb-print look-ups are restricted to "valid" certs (i.e. ones chained to a trusted root and which haven't expired)
                        options.ProtectKeysWithCertificate(encryptingCertificate);
                        options.SetApplicationName(appName);
                    }
                );

            var protector = dataProtectionProvider.CreateProtector(purpose);
            Console.Write("Enter input: ");
            string input = Console.ReadLine();

            // protect the payload
            string protectedPayload = protector.Protect(input);
            Console.WriteLine($"Protect returned: {protectedPayload}");

            // unprotect the payload
            string unprotectedPayload = protector.Unprotect(protectedPayload);
            Console.WriteLine($"Unprotect returned: {unprotectedPayload}");

            // Clean up certificate store.
            store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadWrite);
            store.Remove(encryptingCertificate);

            Console.ReadLine();
        }
    }
}
