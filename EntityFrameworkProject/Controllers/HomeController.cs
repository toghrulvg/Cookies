using EntityFrameworkProject.Data;
using EntityFrameworkProject.Models;
using EntityFrameworkProject.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace EntityFrameworkProject.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;
        public HomeController(AppDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            HttpContext.Session.SetString("name", "esqin");
            Response.Cookies.Append("surname", "Ceferli");

            IEnumerable<Slider> sliders = await _context.Sliders.ToListAsync();
            SliderDetail sliderDetail = await _context.SliderDetails.FirstOrDefaultAsync();
            IEnumerable<Category> categories = await _context.Categories.Where(m => m.IsDeleted == false).ToListAsync();
            IEnumerable<Product> products = await _context.Products
                .Where(m => m.IsDeleted == false)
                .Include(m => m.Category)
                .Include(m => m.ProductImages).Take(4).ToListAsync();


            HomeVM model = new HomeVM
            {
                Sliders = sliders,
                SliderDetail = sliderDetail,
                Categories = categories,
                Products = products
            };

            return View(model);
        }

        public IActionResult Test()
        {
            var sessionData = HttpContext.Session.GetString("name");
            var cookieData = Request.Cookies["surname"];
            return Json(sessionData + " " + cookieData);
        }

        public async Task<IActionResult> AddBasket(int? id)
        {
            if (id is null)
            {
                return BadRequest();
            }
            //var dbProduct = _context.Products.FirstOrDefaultAsync();
            var dbProduct = await _context.Products.FindAsync(id);

            if (dbProduct == null)
            {
                return BadRequest();
            }

            List<BasketVM> baskets;

            if (Request.Cookies["basket"] != null)
            {
                baskets = JsonConvert.DeserializeObject<List<BasketVM>>(Request.Cookies["basket"]);
            }
            else
            {
                baskets = new List<BasketVM>();
            }

            var basketCount = baskets.Count;

            var isExist = baskets.Find(m => m.Id == id);

            if (isExist != null)
            {

                isExist.Count++;
            }
            else
            {
                baskets.Add(new BasketVM
                {
                    Id = dbProduct.Id,
                    Count = basketCount
                }); 
            }







            Response.Cookies.Append("basket", JsonConvert.SerializeObject(baskets));
            return RedirectToAction("Index");
        }
    }
}
