using System;
using System.IO;

using Microsoft.AspNet.DataProtection;

namespace DataProtectionPlayGround
{
    class Program
    {
        static void Main(string[] args)
        {
            const string keyStore = "dataProtectionSamples";
            const string appName = "SimpleFileSystemNoDI";
            const string purpose = "Demonstration";

            var programKeyStore =
                Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    $"{keyStore}\\{appName}");
            Console.WriteLine($"Keys stored in\n{programKeyStore}");

            // instantiate the data protection system at this folder
            var dataProtectionProvider = new DataProtectionProvider(new DirectoryInfo(programKeyStore),
                options =>
                {
                    options.SetApplicationName(appName);
                });

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