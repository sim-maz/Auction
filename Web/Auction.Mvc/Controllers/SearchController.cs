using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Auction.Data.Ef;
using Auction.Domain;
using System.Threading.Tasks;
using Auction.Mvc.Models;

namespace Auction.Mvc.Controllers
{
    public class SearchController : Controller
    {
        private EfContext ctx = new EfContext();

        // Searches the DB table dbo.PRODUCTS for any items that contain the given string in their name and if CategoryId is above 0 then it also filters by category. 
        // Returns the view and passes the results that are encoded into JSON at the View before being displayed on the grid.
        public ActionResult Index([Bind(Include = "Name, CategoriesSearch")]SearchDto search)
        {
            var data = new List<Product>();
            var results = new List<ProductDto>();

            if (search.Name == null)
            {
                if (search.CategoriesSearch > 0)
                {
                    data = ctx.Products.Where(item => item.CategoryId == search.CategoriesSearch).ToList();
                } else
                {
                    data = ctx.Products.ToList();
                }
            } else {
                if (search.CategoriesSearch > 0)
                {
                    data = ctx.Products.Where(item => ((item.Name.Contains(search.Name)) || 
                    (item.Description.Contains(search.Name))) && 
                    (item.CategoryId == search.CategoriesSearch)).ToList();
                } else
                {
                    data = ctx.Products.Where(item => ((item.Name.Contains(search.Name)) || (item.Description.Contains(search.Name)))).ToList();
                }
            }         

            results = data.Select(x => new ProductDto
            {
                Id = x.Id,
                Name = x.Name,
                Category = ctx.Categories.FirstOrDefault(c => c.Id == x.CategoryId).Name,
                Bid = ctx.Bids.Any(b => b.ProductId == x.Id) ? ctx.Bids.OrderByDescending(b => b.Sum).FirstOrDefault(b => b.ProductId == x.Id).Sum : x.StartBid,
                StartTime = x.StartTime,
                EndTime = x.EndTime
            }).ToList();

            return View("Index", results);
        }
    }
}
