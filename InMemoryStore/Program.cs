using Microsoft.AspNet.DataProtection;
using System;

namespace InMemoryStore
{
    class Program
    {
        static void Main(string[] args)
        {
            const string purpose = "Demonstration";

            // instantiate the data protection system at this folder
            var dataProtectionProvider = new EphemeralDataProtectionProvider();

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
