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

            // Certificate was generated with
            // makecert -r -pe -n "CN=Data Protection" -b 07/01/2015 -e 07/01/2020 -sky exchange -eku 1.3.6.1.4.1.311.10.3.12 -ss my

            var certificateStore = new X509Store(StoreLocation.CurrentUser);
            certificateStore.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);

            var encryptingCertificates = 
                certificateStore.Certificates.Find(
                    X509FindType.FindBySubjectDistinguishedName, 
                    "CN=Data Protection", 
                    false); // Must be false for self signed certs.

            certificateStore.Close();

            if (encryptingCertificates == null || encryptingCertificates.Count == 0)
            {
                Console.WriteLine("Couldn't find the encryption certificate.");
                Environment.Exit(-1);
            }

            var encryptingCertificate = encryptingCertificates[0];

            if (encryptingCertificate.PrivateKey == null)
            {
                Console.WriteLine("No private key available.");
                Environment.Exit(-1);
            }

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

            Console.ReadLine();
        }
    }
}