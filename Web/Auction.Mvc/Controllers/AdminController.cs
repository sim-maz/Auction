using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Auction.Data.Ef;
using Auction.Domain;
using Auction.Mvc.Attributes;
using Auction.Mvc.Models;

namespace Auction.Mvc.Controllers
{
    [RestrictedForAdmins]
    public class AdminController : Controller
    {
        private EfContext ctx = new EfContext();

        // GET: ListProducts
        public ActionResult Index()
        {
            return PartialView();
        }

        public ActionResult Products()
        {
            return PartialView("_Products", ctx.Products.ToList());
        }

        public ActionResult Markets()
        {
            return PartialView("_Markets", ctx.Markets.ToList());
        }

        public ActionResult Comments()
        {
            return PartialView("_Comments", ctx.Comments.ToList());
        }

        public ActionResult Bids()
        {
            var bids = ctx.Bids.Select(x => new BidDto {
                Id = x.Id,
                BidTime = x.BidTime,
                ProductName = ctx.Products.Where(p => p.Id == x.ProductId).FirstOrDefault().Name,
                Sum = x.Sum,
                User = x.User
            }).ToList();

            return PartialView("_Bids", bids);
        }

        // GET: ListProducts/Details/5
        public ActionResult Details(Guid? id)
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
            return View(product);
        }

        // GET: ListProducts/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: ListProducts/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Name,CategoryId,StartBid,StartTime,Duration,EndTime")] Product product)
        {
            if (ModelState.IsValid)
            {
                product.Id = Guid.NewGuid();
                ctx.Products.Add(product);
                ctx.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(product);
        }

        // GET: ListProducts/Edit/5
        public ActionResult Edit(Guid? id)
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
            return View(product);
        }

        // POST: ListProducts/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Name,CategoryId,StartBid,StartTime,Duration,EndTime")] Product product)
        {
            if (ModelState.IsValid)
            {
                ctx.Entry(product).State = EntityState.Modified;
                ctx.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(product);
        }

        // GET: ListProducts/Delete/5
        public ActionResult Delete(Guid? id)
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
            return View(product);
        }

        // POST: ListProducts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(Guid id)
        {
            Product product = ctx.Products.Find(id);
            ctx.Products.Remove(product);
            ctx.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ctx.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
