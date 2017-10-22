using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Auction.Data.Ef;
using Auction.Domain;
using Auction.Mvc.Models;
using Auction.Mvc.Core;
using Auction.Mvc.Attributes;

namespace Auction.Mvc.Controllers
{
    [Authorize]
    public class BidsController : Controller
    {
        private EfContext ctx = new EfContext();

        // Returns a PartialView with ProductId added to the ViewBag
        public ActionResult Index(Guid ProductId)
        {
            ViewBag.ProductId = ProductId;
            return PartialView();
        }

        // GET: Bids/Details/5
        public ActionResult Details(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Bid bid = ctx.Bids.Find(id);
            if (bid == null)
            {
                return HttpNotFound();
            }
            return View(bid);
        }

        // GET: Bids/Create
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Sum, ProductId, StartBid")] Bid bid)
        {
            Guid productId = new Guid();
            Guid.TryParse(Request["ProductId"], out productId);

            var activeBids = from p1 in ctx.Bids
                             where p1.ProductId == productId
                             select new BidDto { Id = p1.Id, ProductId = p1.ProductId, BidTime = p1.BidTime, Sum = p1.Sum };
            decimal highBid;

            if (activeBids.Count() == 0) {
                decimal.TryParse(Request["StartBid"], out highBid);
            } else {
                highBid = activeBids.Select(x => x.Sum).Max();
            };
            using (ctx)
            {
                //Determines if the bid sum that was sent is actually higher than the last highest bid and saves the changes to DB.
                if (ctx.Markets.Where(x => x.Id == (ctx.ProductMarket.Where(p => p.ProductId == productId).FirstOrDefault().MarketId)).FirstOrDefault().MarketEnd > DateTime.Now)
                {
                    if (ModelState.IsValid)
                    {              
                        if (bid.Sum > highBid)
                        { 
                            bid.Id = Guid.NewGuid();
                            bid.BidTime = DateTime.Now;
                            bid.ProductId = productId;
                            bid.User = CurrentUser.Login;
                            ctx.Bids.Add(bid);
                            ctx.SaveChanges();
                        }
                    }
                }
                return Redirect(Request.UrlReferrer.ToString());
            }        
        }

        // GET: Bids/Edit/5
        [RestrictedForAdmins]
        public ActionResult Edit(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Bid bid = ctx.Bids.Find(id);
            if (bid == null)
            {
                return HttpNotFound();
            }
            return View(bid);
        }

        // POST: Bids/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RestrictedForAdmins]
        public ActionResult Edit([Bind(Include = "Id,Sum,ProductID,BidTime")] Bid bid)
        {
            if (ModelState.IsValid)
            {
                ctx.Entry(bid).State = EntityState.Modified;
                ctx.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(bid);
        }

        // GET: Bids/Delete/5
        [RestrictedForAdmins]
        public ActionResult Delete(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Bid bid = ctx.Bids.Find(id);
            if (bid == null)
            {
                return HttpNotFound();
            }

            var bids = new BidDto();

            bids.Id = bid.Id;
            bids.ProductName = ctx.Products.Where(x => x.Id == bid.ProductId).FirstOrDefault().Name;
            bids.Sum = bid.Sum;
            bids.BidTime = bid.BidTime;
            bids.User = bid.User;

            return View(bids);
        }

        // Deletes the bid from DB
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [RestrictedForAdmins]
        public ActionResult DeleteConfirmed(Guid id)
        {
            Bid bid = ctx.Bids.Find(id);
            ctx.Bids.Remove(bid);
            ctx.SaveChanges();
            return RedirectToAction("Index","Admin");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ctx.Dispose();
            }
            base.Dispose(disposing);
        }


        //Gets the active bids that match the given ProductId and sends Json data for populating the Kendo UI grid.
        public JsonResult GetBids(Guid ProductId)
        {
            var data = new List<BidDto>();

            using (ctx)
            {
                var activeBids = from p1 in ctx.Bids
                                 where p1.ProductId == ProductId
                                 select new BidDto { Id = p1.Id, User = p1.User, ProductId = p1.ProductId, BidTime = p1.BidTime, Sum = p1.Sum };

                data = activeBids.Select(x => new BidDto
                {
                    Id = x.Id,
                    User = x.User,
                    ProductId = ctx.Products.FirstOrDefault(p => p.Id == x.ProductId).Id,
                    BidTime = x.BidTime,
                    Sum = x.Sum
                }).ToList();

                return Json(data, JsonRequestBehavior.AllowGet);
                
            }
        }
    }
}
