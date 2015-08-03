using System;
using Microsoft.AspNet.DataProtection;
using Microsoft.Framework.DependencyInjection;

namespace Net451ExtensionMethods
{
    class Program
    {
        const string appName = "SimpleFileSystemNoDI";
        const string purpose = "Demonstration";

        static void Main(string[] args)
        {

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddDataProtection();
            serviceCollection.ConfigureDataProtection(options =>
            {
                options.SetApplicationName(appName);
            });

            var services = serviceCollection.BuildServiceProvider();

            var demoInstance = ActivatorUtilities.CreateInstance<ProtectUnprotectDemo>(services);
            demoInstance.Demonstrate();
        }

        public class ProtectUnprotectDemo
        {
            IDataProtector _protector;

            public ProtectUnprotectDemo(IDataProtectionProvider provider)
            {
                _protector = provider.CreateProtector(purpose);
            }

            public void Demonstrate()
            {
                Console.Write("Enter input: ");
                string input = Console.ReadLine();

                // protect the payload
                string protectedPayload = _protector.Protect(input);
                Console.WriteLine($"Protect returned: {protectedPayload}");

                // unprotect the payload
                string unprotectedPayload = _protector.Unprotect(protectedPayload);
                Console.WriteLine($"Unprotect returned: {unprotectedPayload}");
                Console.ReadLine();
            }
        }
    }
}
