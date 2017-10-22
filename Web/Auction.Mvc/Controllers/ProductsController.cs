using Auction.Data.Ef;
using Auction.Domain;
using Auction.Mvc.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Data.Entity;
using System.Net;
using Auction.Mvc.Attributes;
using System.Web;
using System.IO;

namespace Auction.Mvc.Controllers
{
    [Authorize]
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
                var categories = ctx.Categories.ToList();
                var markets = ctx.Markets.Where(x => x.MarketStatus == true).ToList();
                    //Below only the active products are selected from the total. Active products are considered the ones that are from the selected Market, selected using MarketId.
                var activeProducts = from p1 in ctx.ProductMarket
                                         join p2 in ctx.Products
                                         on p1.ProductId equals p2.Id
                                         where p1.MarketId == MarketId
                                         select new { p2.Id, Name = p2.Name, Description = p2.Description, CategoryId = p2.CategoryId, Bid = p2.StartBid, StartTime = p2.StartTime, Duration = p2.Duration, EndTime = p2.EndTime }; 
                //The active products are projected into a new ProductDto and then returned as JSON.

                data = activeProducts.Select(x => new ProductDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Description = x.Description,
                    Category = ctx.Categories.FirstOrDefault(c => c.Id == x.CategoryId).Name,
                    Bid = ctx.Bids.Any(b => b.ProductId == x.Id) ? ctx.Bids.OrderByDescending(b => b.Sum).FirstOrDefault(b => b.ProductId == x.Id).Sum : x.Bid,
                    StartTime = x.Duration == 0 ? ctx.Markets.Where(i => i.Id == (ctx.ProductMarket.Where(m => m.ProductId == x.Id).FirstOrDefault().MarketId)).FirstOrDefault().MarketStart : x.StartTime,
                    EndTime = x.Duration == 0 ? ctx.Markets.Where(i => i.Id == (ctx.ProductMarket.Where(m => m.ProductId == x.Id).FirstOrDefault().MarketId)).FirstOrDefault().MarketEnd : x.EndTime
                }).ToList();

                return Json(data, JsonRequestBehavior.AllowGet);
            }
        }
        
        [RestrictedForAdmins]
        public ActionResult Create()
        {
            SelectCategory();
            SelectMarket();

            return View();
        }

        //The following ActionResult receives the data from view in the form of a FormCollection which is then used to populate Product and Market objects with data for creating a new product.
        [HttpPost]
        [RestrictedForAdmins]
        public ActionResult Create(FormCollection form)
        {

            Image image = new Image();
            Product product = new Product();
            //The ProductMarket object is populated here.
            //Fields populated: MarketId, ProductId.
            ProductMarket productMarket = new ProductMarket();
            productMarket.MarketId = SelectMarketId(Request.Form["MarketName"]);
            productMarket.ProductId = product.Id;

            //The Product object is populated here
            //Fields populated: Name, StartBid, Duration, StartTime, EndTime, CategoryId.            
            double days = 0;
            product.Name = Request.Form["Name"];

            if (double.TryParse(Request.Form["Duration"], out days) && days == 0)
            {
                product.StartTime = ctx.Markets
                        .Where(m => m.Id == productMarket.MarketId).FirstOrDefault().MarketStart;
            } else
            {
                product.StartTime = DateTime.Now;
            }


            decimal startBid = 0;
            if (decimal.TryParse(Request.Form["StartBid"], out startBid))
            {
                product.StartBid = startBid;
            } else
            {
                product.StartBid = 0.01M;
            }


            if (double.TryParse(Request.Form["Duration"], out days))
            {
                if(days == 0)
                {
                    product.EndTime = ctx.Markets
                        .Where(m => m.Id == productMarket.MarketId).FirstOrDefault().MarketEnd;
                } 
            }
            else
            {
                product.EndTime = DateTime.Now.AddDays(days);
                product.Duration = days;
            }

            var categoryId = Request.Form["CategoryName"];
            product.CategoryId = SelectCategoryId(categoryId);
            product.Description = WebUtility.HtmlEncode(Request.Form["Description"]);


            //Filepath for the product's image           
            if ((Request.Form["ImagePath"].IndexOfAny(Path.GetInvalidFileNameChars())) != 0)
            {
                image.Path = Request.Form["ImagePath"];
            } else
            {
                image.Path = "default.jpg";
            }
            image.ProductId = product.Id;


            //ModelState is checked to be valid and then objects are sent to be added to the DB.
            if (ModelState.IsValid)
            {
                ctx.Products.Add(product);
                ctx.SaveChanges();

                ctx.Images.Add(image);
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
        private void SelectCategory()
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
        }

        //Returns a single category ID based on the given name of the category.
        private int SelectCategoryId (string categoryId)
        {
            int value;
            int.TryParse(categoryId, out value); //ctx.Categories.SingleOrDefault(x => x.Name == categoryName).Id;
            return value;
        }

        //Creates a SelectListItem that consists of currently existing Market names and Ids and adds the list into a ViewBag.
        [RestrictedForAdmins]
        private void SelectMarket()
        {
            List<SelectListItem> itemMarkets = new List<SelectListItem>();
            var markets = ctx.Markets.Where(x => x.MarketStatus == false).ToList();
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
        }

        private string SelectPath(Guid id)
        {
            var result = (ctx.Images.Any(x => x.ProductId == id)) && (ctx.Images.Where(x => x.ProductId == id).FirstOrDefault().Path != "") ? ctx.Images.Where(x => x.ProductId == id).FirstOrDefault().Path : "default.jpg";
            return result; 
        }

        //Returns a single market ID based on the given name of the market. 
        private Guid SelectMarketId(string marketId)
        {
            var value = new Guid();
            Guid.TryParse(marketId, out value);
            return  value;
        }

        //Returns Details view for product of the given ID. 

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


            //ViewBag gets populated with Highest Bid and the remaining time of the product.
            ViewBag.RemainingTime = FormatRemainingTime(id);
            ViewBag.HighestBid = bids.Any(b => b.ProductId == id) ? bids.OrderByDescending(b => b.Sum).First(b => b.ProductId == id).Sum : product.StartBid;
            ViewBag.ImgPath = SelectPath(id);


            return View(product);
        }

        //Parse and custom format for TimeSpan. Format: x day(s) y hours and z minutes. 
        private string FormatRemainingTime(Guid id) {
            Product product = ctx.Products.Find(id);
            TimeSpan remainingTime = new TimeSpan();
            if (product.Duration == 0)
            {
                remainingTime = ctx.Markets.Where(i => i.Id == (ctx.ProductMarket.Where(m => m.ProductId == id).FirstOrDefault().MarketId)).FirstOrDefault().MarketEnd.Subtract(DateTime.Now);
            } else
            {
                remainingTime = product.EndTime.Subtract(DateTime.Now);
            }

            string value = remainingTime.ToString();
            string output = null;

            TimeSpan interval;
            TimeSpan day = new TimeSpan(1, 0, 0, 0);
            TimeSpan minute = new TimeSpan(0, 0, 1, 0);
            TimeSpan.TryParse(value, out interval);

            //Formats to a string based on how much time is actually left.
            if(interval < new TimeSpan(0, 0, 0, 0))
            {
                output = "<span style=\"color: red;\">Closed!</span>";
            }else if (interval <= minute)
            {
                output = "<span style=\"color: red;\">Less than a minute...</span>";
            }
            else
            { 
                if (interval >= day) {
                    if (interval == day)
                        output = remainingTime.ToString("%d") + " day " + remainingTime.ToString("%h") + " hours and " + remainingTime.ToString("%m") + " minutes";
                    else
                        output = remainingTime.ToString("%d") + " days " + remainingTime.ToString("%h") + " hours and " + remainingTime.ToString("%m") + " minutes";
                } else
                    output = remainingTime.ToString("%h") + " hours and " + remainingTime.ToString("%m") + " minutes";
            }
            return output;
        }

        [RestrictedForAdmins]
        public ActionResult Edit(Guid id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = ctx.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }

            SelectCategory();
            SelectMarket();
            ViewBag.ImgPath = SelectPath(id);

            return View(product);
        }

        // POST: ListProducts/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [RestrictedForAdmins]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Name,Description,CategoryName,StartBid,StartTime,Duration,EndTime")] Product product)
        {
            EfContext ctx = new EfContext();

            if (ctx.Images.FirstOrDefault(x => x.ProductId == product.Id) != null)
                {
                    var image = ctx.Images.Find(ctx.Images.FirstOrDefault(x => x.ProductId == product.Id).Id);
                    if (Request.Form["ImagePath"] != null)
                    {
                        image.Path = Request.Form["ImagePath"];
                    }
                    else
                    {
                        image.Path = "default.jpg";
                    }
                    ctx.Entry(image).State = EntityState.Modified;
                }
                else
                {
                    var image = new Image
                    {
                        Id = Guid.NewGuid(),
                        ProductId = product.Id,
                        Path = "default.jpg"
                    };
                    ctx.Images.Add(image);
                }
            ctx.SaveChanges();

            if (ModelState.IsValid)
            {
                product.CategoryId = SelectCategoryId(Request.Form["CategoryName"]);
                ctx.Entry(product).State = EntityState.Modified;
                ctx.SaveChanges();

            }
            return RedirectToAction("Index", "Admin");
        }
    }
}