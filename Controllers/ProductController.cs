using Microsoft.AspNetCore.Mvc;
using ProductManagementMVC.Models;
using ProductManagementMVC.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProductManagementMVC.Controllers
{
    public class ProductController : Controller
    {
        private readonly ApiService _apiService;

        public ProductController(ApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<IActionResult> MyProducts()
        {
            var myProducts = await _apiService.GetAsync<List<ProductViewModel>>("api/product/my-products");
            return View(myProducts ?? new List<ProductViewModel>());
        }

        public IActionResult AddProduct()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddProduct(ProductViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var response = await _apiService.PostAsync<Dictionary<string, object>>("api/product/add", model);

            if (response != null && response.ContainsKey("message"))
                return RedirectToAction("MyProducts");

            ModelState.AddModelError(string.Empty, "Failed to add product.");
            return View(model);
        }
    }
}
