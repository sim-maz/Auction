using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Auction.Mvc.Controllers;
using Auction.Data.Ef;
using Auction.Domain;

namespace Auction.Mvc.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {

        EfContext ctx = new EfContext();

        // Renders the top navigation bar with search bar and category select. 
        // Adds a SelectList to the ViewBag that is used for the category selector.
        public ActionResult Index()
        {
            ProductsController products = new ProductsController();

            ViewBag.CategoriesSearch = SelectCategory();

            return View();
        }

        // Searches the DB for existing product categories and returns List<SelectList> for a DropdownList item.
        public List<SelectListItem> SelectCategory()
        {
            List<SelectListItem> itemCategories = new List<SelectListItem>();
            var categories = ctx.Categories.ToList();
            var dataCategories = categories.Select(x => new Category
            {
                Id = x.Id,
                Name = x.Name
            }).ToList();

            itemCategories.Add(new SelectListItem { Text = "All Categories", Value = "0" });

            foreach (var item in dataCategories.Select(x => x.Id).Distinct())
            {
                itemCategories.Add(new SelectListItem { Text = dataCategories.Where(x => x.Id == item).SingleOrDefault().Name, Value = item.ToString() });
            }

            return itemCategories;
        }
    }
}