using System;

using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.DataProtection;
using WebConfiguration.Models;
using Microsoft.AspNet.Routing;

namespace WebConfiguration.Controllers
{
    public class HomeController : Controller
    {
        IDataProtector _protector;

        public HomeController(IDataProtectionProvider provider)
        {
            _protector = provider.CreateProtector("DemoTime");
        }

        public IActionResult Index(ProtectionData model)
        {
            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Protect(ProtectionData model)
        {
            if (!string.IsNullOrEmpty(model.PlainText))
            {
                model.CipherText = _protector.Protect(model.PlainText);
            }
            model.PlainText = string.Empty;
            return RedirectToAction("Index", new RouteValueDictionary(model));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Unprotect(ProtectionData model)
        {
            try {
                if (!string.IsNullOrEmpty(model.CipherText))
                {
                    model.PlainText = _protector.Unprotect(model.CipherText);
                }
                model.CipherText = string.Empty;
                return RedirectToAction("Index", new RouteValueDictionary(model));
            }
            catch (Exception e)
            {
                model.Error = e;
                return View("Index", model);
            }

        }

        public IActionResult Error()
        {
            return View("~/Views/Shared/Error.cshtml");
        }
    }
}
