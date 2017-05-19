using Auction.Data.Ef;
using Auction.Domain;
using Auction.Mvc.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Data.Entity;
using System.Net;


namespace Auction.Mvc.Controllers
{
    public class ProductsController : Controller
    {
        EfContext ctx = new EfContext();


        // GET: Products
        public ActionResult Index(Guid? MarketId)
        {
            return PartialView(MarketId);
        }

        //Method used for getting product data from DB and returns it as JSON for populating the Kendo UI Grid.
        public JsonResult GetProducts(Guid MarketId)
        {
            var data = new List<ProductDto>();

            using (ctx)
            {
                var products = ctx.Products.ToList();
                var categories = ctx.Categories.ToList();
                var bids = ctx.Bids.ToList();
                var productsMarket = ctx.ProductMarket.ToList();
                    //Below only the active products are selected from the total. Active products are considered the ones that are from the selected Market, selected using MarketId.
                    var activeProducts = from p1 in productsMarket
                                         join p2 in products
                                         on p1.ProductId equals p2.Id
                                         where p1.MarketId == MarketId
                                         select new { p2.Id, Name = p2.Name, CategoryId = p2.CategoryId, Bid = p2.StartBid, StartTime = p2.StartTime, Duration = p2.Duration, EndTime = p2.EndTime };

                    //The active products are projected into a new ProductDto and then returned as JSON.
                    data = activeProducts.Select(x => new ProductDto
                    {
                        Id = x.Id,
                        Name = x.Name,
                        Category = categories.FirstOrDefault(c => c.Id == x.CategoryId).Name,
                        Bid = bids.Any(b => b.ProductId == x.Id) ? bids.OrderByDescending(b => b.Sum).First(b => b.ProductId == x.Id).Sum : x.Bid,
                        StartTime = x.StartTime,
                        EndTime = x.StartTime.AddDays(x.Duration)
                    }).ToList();
                return Json(data, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult Create()
        {
            SelectCategory();
            SelectMarket();

            return View();
        }

        //The following ActionResult receives the data from view in the form of a FormCollection which is then used to populate Product and Market objects with data for creating a new product.
        [HttpPost]
        public ActionResult Create(FormCollection form)
        {
            //The Product object is populated here
            //Fields populated: Name, StartBid, Duration, StartTime, EndTime, CategoryId.
            Product product = new Product();
            product.Name = Request.Form["Name"];
            decimal startBid = 0;
            if (decimal.TryParse(Request.Form["StartBid"], out startBid))
            {
                product.StartBid = startBid;
            } else
            {
                product.StartBid = 0.01M;
            }
            double days = 0;
            if (double.TryParse(Request.Form["Duration"], out days))
            {
                product.EndTime = DateTime.Now.AddDays(days);
                product.Duration = days;
            }
            else
            {
                product.EndTime = DateTime.Now.AddDays(7);
                product.Duration = 7;
            }
            var categoryId = Request.Form["CategoryName"];
            product.CategoryId = SelectCategoryId(categoryId);

            //The ProductMarket object is populated here.
            //Fields populated: MarketId, ProductId.
            ProductMarket productMarket = new ProductMarket();
            productMarket.MarketId = SelectMarketId(Request.Form["MarketName"]);
            productMarket.ProductId = product.Id;

            //ModelState is checked to be valid and then objects are sent to be added to the DB.
            if (ModelState.IsValid)
            {
                ctx.Products.Add(product);
                ctx.SaveChanges();
                if (ModelState.IsValid)
                {
                    ctx.ProductMarket.Add(productMarket);
                    ctx.SaveChanges();
                    return RedirectToAction("Index","Markets");
                }
            }
            
            return View(product);
        }

        //Selects all unique category names that are available and adds them to a new SelectListItem. 
        //Returns a ViewBag item containing the newly created SelectListItem.
        public ActionResult SelectCategory()
        {
            List<SelectListItem> itemCategories = new List<SelectListItem>();
            var categories = ctx.Categories.ToList();
            var dataCategories = categories.Select(x => new Category
            {
                Id = x.Id,
                Name = x.Name
            }).ToList();

            foreach (var item in dataCategories.Select(x => x.Id).Distinct())
            {
                itemCategories.Add(new SelectListItem { Text = dataCategories.Where(x => x.Id == item).SingleOrDefault().Name, Value = item.ToString() });
            }
            ViewBag.CategoriesList = itemCategories;
            return View();
        }

        //Returns a single category ID based on the given name of the category.
        private int SelectCategoryId (string categoryId)
        {
            int value;
            int.TryParse(categoryId, out value); //ctx.Categories.SingleOrDefault(x => x.Name == categoryName).Id;
            return value;
        }

        //Creates a SelectListItem that consists of currently existing Market names and Ids and adds the list into a ViewBag.
        private ActionResult SelectMarket()
        {
            List<SelectListItem> itemMarkets = new List<SelectListItem>();
            var markets = ctx.Markets.Where(x => x.MarketStatus == 1).ToList();
            var dataMarkets = markets.Select(x => new Market
            {
                MarketName = x.MarketName,
                Id = x.Id
            }).ToList();

            foreach (var item in dataMarkets.Select(x => x.Id).Distinct())
            {
                itemMarkets.Add(new SelectListItem { Text = dataMarkets.Where(x => x.Id == item).SingleOrDefault().MarketName, Value = item.ToString() });
            }
            ViewBag.MarketsList = itemMarkets;
            return View();
        }

        //Returns a single market ID based on the given name of the market. 
        private Guid SelectMarketId(string marketId)
        {
            var value = new Guid();
            Guid.TryParse(marketId, out value);
            return  value;
        }

        public ActionResult Details(Guid id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = ctx.Products.Find(id);
            var bids = ctx.Bids.Where(b => b.ProductId == id).ToList();            
            if (product == null)
            {
                return HttpNotFound();
            }


            ViewBag.RemainingTime = FormatRemainingTime(id);
            ViewBag.HighestBid = bids.Any(b => b.ProductId == id) ? bids.OrderByDescending(b => b.Sum).First(b => b.ProductId == id).Sum : product.StartBid;


            return View(product);
        }

        //Parse and custom format for TimeSpan. Format: x day(s) y hours and z minutes. 
        public string FormatRemainingTime (Guid id) {
            Product product = ctx.Products.Find(id);
            var remainingTime = product.EndTime.Subtract(DateTime.Now);

            string value = remainingTime.ToString();
            string output = null;

            TimeSpan interval;
            TimeSpan day = new TimeSpan(1, 0, 0, 0);
            TimeSpan.TryParse(value, out interval);

            if (interval >= day) {
                if (interval == day)
                    output = remainingTime.ToString("%d") + " day " + remainingTime.ToString("%h") + " hours and " + remainingTime.ToString("%m") + " minutes";
                else
                    output = remainingTime.ToString("%d") + " days " + remainingTime.ToString("%h") + " hours and " + remainingTime.ToString("%m") + " minutes";
            } else
                output = remainingTime.ToString("%h") + " hours and " + remainingTime.ToString("%m") + " minutes";

            return output;
        }

        public ActionResult Update(Guid Id)
        {

            var data = new List<ProductUpdateDto>();

            using (ctx)
            {
                var products = ctx.Products.ToList();
                data = products.Select(x => new ProductUpdateDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    CategoryId = x.CategoryId,
                    Duration = x.Duration,
                    StartTime = x.StartTime,
                    EndTime = x.EndTime,
                    StartBid = x.StartBid
                    
                }).ToList();
            }
            var productListing = data.Find(x => x.Id == Id);
            return View(productListing);
        }

        [HttpPost]
        public ActionResult Update ([Bind(Include = "Id, Name, CategoryId, Duration, StartTime, EndTime, StartBid")]Product productListing)
        {
            if (ModelState.IsValid)
            {
                ctx.Entry(productListing).State = EntityState.Modified;
                ctx.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(productListing);
        }
        
    }
}