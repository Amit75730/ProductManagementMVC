using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProductManagementMVC.Models;
using ProductManagementMVC.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProductManagementMVC.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApiService _apiService;

        public AccountController(ApiService apiService)
        {
            _apiService = apiService;
        }

        public IActionResult Login() => View();
        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(UserRegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var response = await _apiService.PostAsync<Dictionary<string, object>>("api/user/register", model);

                if (response != null && response.ContainsKey("message"))
                {
                    string message = response["message"].ToString();

                    Console.WriteLine($"🔍 API Response Message: {message}"); // Debug Log

                    if (message.Contains("success"))
                    {
                        TempData["SuccessMessage"] = "✅ Registration successful! You can now log in.";
                        return RedirectToAction("Login");
                    }
                }

                ViewBag.ErrorMessage = "❌ Registration failed. Please try again.";
            }
            catch (HttpRequestException httpEx)
            {
                Console.WriteLine($"❌ HTTP Request Exception: {httpEx.Message}");

                if (httpEx.Message.Contains("400"))
                {
                    ViewBag.ErrorMessage = "❌ This email ID is already registered. Please use a different email.";
                }
                else
                {
                    ViewBag.ErrorMessage = "❌ An error occurred during registration.";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Registration Error: {ex.Message}");
                ViewBag.ErrorMessage = "❌ An error occurred during registration.";
            }

            return View(model);
        }





        [HttpPost]
        public async Task<IActionResult> Login(UserLoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var response = await _apiService.PostAsync<Dictionary<string, string>>("api/user/login", model);

                if (response != null && response.ContainsKey("token"))
                {
                    string token = response["token"];
                    HttpContext.Session.SetString("Token", token);
                    Console.WriteLine($"✅ Token stored in session: {token}");

                    return RedirectToAction("AddProduct", "Product");
                }

                ViewBag.ErrorMessage = "❌ Invalid login attempt.";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Login Error: {ex.Message}");
                ViewBag.ErrorMessage = "❌ An error occurred during login.";
            }

            return View(model);
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Remove("Token");
            return RedirectToAction("Login");
        }
    }
}
