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

namespace Auction.Mvc.Controllers
{
    public class BidsController : Controller
    {
        private EfContext ctx = new EfContext();

        // GET: Bids
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

        // POST: Bids/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
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

            if (ModelState.IsValid)
            {
                if (bid.Sum > highBid)
                {
                    bid.Id = Guid.NewGuid();
                    bid.BidTime = DateTime.Now;
                    bid.ProductId = productId;
                    ctx.Bids.Add(bid);
                    ctx.SaveChanges();
                }
            }
      
            return Redirect(Request.UrlReferrer.ToString());
        }

        // GET: Bids/Edit/5
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
            return View(bid);
        }

        // POST: Bids/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(Guid id)
        {
            Bid bid = ctx.Bids.Find(id);
            ctx.Bids.Remove(bid);
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

        public JsonResult GetBids(Guid ProductId)
        {
            var data = new List<BidDto>();

            using (ctx)
            {
                var activeBids = from p1 in ctx.Bids
                                 where p1.ProductId == ProductId
                                 select new BidDto { Id = p1.Id, ProductId = p1.ProductId, BidTime = p1.BidTime, Sum = p1.Sum };

                data = activeBids.Select(x => new BidDto
                {
                    Id = x.Id,
                    ProductId = ctx.Products.FirstOrDefault(p => p.Id == x.ProductId).Id,
                    BidTime = x.BidTime,
                    Sum = x.Sum
                }).ToList();

                return Json(data, JsonRequestBehavior.AllowGet);
                
            }
        }
    }
}
