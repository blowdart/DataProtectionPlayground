using System;
using System.IO;

using Microsoft.AspNet.DataProtection;


namespace ManualAzureRoamingKeysNoDI
{
    class Program
    {
        private const string ApplicationName = "ManualAzureRoamingKeysNoDI";
        private const string Purpose = "WebHookPersistence";
        private const string DataProtectionKeysFolderName = "DataProtection-Keys";

        static void Main(string[] args)
        {
            var provider = GetDataProtectionProvider();
            var protector = provider.CreateProtector(Purpose);

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

        internal static IDataProtectionProvider GetDataProtectionProvider()
        {
            IDataProtectionProvider dataProtectionProvider = null;
            DirectoryInfo azureWebSitesKeysFolder = GetKeyStorageDirectoryForAzureWebSites();
            if (azureWebSitesKeysFolder != null)
            {
                // TODO: look for a X509 Cert Thumbprint and if found wire that up. Otherwise log stern warning

                dataProtectionProvider = new DataProtectionProvider(
                    azureWebSitesKeysFolder,
                    options =>
                    {
                        options.SetApplicationName(ApplicationName);
                    });
                azureWebSitesKeysFolder.Create();

                return dataProtectionProvider;
            }

            DirectoryInfo localAppDataKeysFolder = GetDefaultKeyStorageDirectory();
            if (localAppDataKeysFolder != null)
            {
                dataProtectionProvider = new DataProtectionProvider(
                    localAppDataKeysFolder,
                    options =>
                    {
                        options.SetApplicationName(ApplicationName);
                        options.ProtectKeysWithDpapi();
                    });
                localAppDataKeysFolder.Create();
                return dataProtectionProvider;
            }

            return null;
        }


        internal static DirectoryInfo GetDefaultKeyStorageDirectory()
        {
            // Environment.GetFolderPath returns null if the user profile isn't loaded. 
            string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            if (!string.IsNullOrEmpty(folderPath))
            {
                return GetKeyStorageDirectoryFromBaseAppDataPath(folderPath);
            }
            else
            {
                return null;
            }
        }

        internal static DirectoryInfo GetKeyStorageDirectoryForAzureWebSites()
        {
            // Azure Web Sites needs to be treated specially, as we need to store the keys in a 
            // correct persisted location. We use the existence of the %WEBSITE_INSTANCE_ID% env 
            // variable to determine if we're running in this environment, and if so we then use 
            // the %HOME% variable to build up our base key storage path. 
            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WEBSITE_INSTANCE_ID")))
            {
                string homeEnvVar = Environment.GetEnvironmentVariable("HOME");
                if (!string.IsNullOrEmpty(homeEnvVar))
                {
                    return GetKeyStorageDirectoryFromBaseAppDataPath(homeEnvVar);
                }
            }

            // nope 
            return null;
        }

        internal static DirectoryInfo GetKeyStorageDirectoryFromBaseAppDataPath(string basePath)
        {
            return new DirectoryInfo(Path.Combine(basePath, "ASP.NET", DataProtectionKeysFolderName));
        }
    }
}
