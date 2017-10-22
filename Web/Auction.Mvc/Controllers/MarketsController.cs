using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Auction.Data.Ef;
using Auction.Domain;
using Auction.Mvc.Attributes;
using Auction.Mvc.Core;
using FluentScheduler;
using System.Globalization;

namespace Auction.Mvc.Controllers
{
    
    public class MarketsController : Controller
    {
        private EfContext ctx = new EfContext();

         

        // GET: Markets
        public ActionResult Index()
        {

            ViewBag.Admin = CurrentUser.IsAdmin;
            ViewBag.CurrentUser = CurrentUser.FullName;
            return View(ctx.Markets.Where(x => x.MarketStatus == true).ToList());
        }

        // GET: Markets/Details/5
        public ActionResult Details(Guid? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Market market = ctx.Markets.Find(id);
            if (market == null)
                return HttpNotFound();

            return View(market);
        }



        // GET: Markets/Create
        [RestrictedForAdmins]
        public ActionResult Create()
        {
            return View();
        }

        // POST: Markets/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RestrictedForAdmins]
        public ActionResult Create(FormCollection form)
        {
            //Creates Market object with ID, Name, Type, Start DateTime, End DateTime and Status. 
            //Sets MarketStatus to 1 if DateTime.Now is later than the StartTime, but earlier than the EndTime. 
            //MarketStatus 1 means that the market is active, 0 means it is inactive.
            Market market = new Market();
            market.Id = Guid.NewGuid();
            market.MarketName = Request.Form["MarketName"];

            var marketType = 0;
            if (int.TryParse(Request.Form["MarketType"], out marketType))
            {
                market.MarketType = marketType;
            } else market.MarketType = 1;

            //Custom method concatenates date and time in string formats and returns a DateTime object
            market.MarketStart = ConcatenateDateTime(Request.Form["datePickerStart"], Request.Form["timePickerStart"]);
            market.MarketEnd = ConcatenateDateTime(Request.Form["datePickerEnd"], Request.Form["timePickerEnd"]);


            //Check if given date is later than DateTime.Now. Compares for start time and end time. 
            if (CompareDateTime(market.MarketStart) < 0 && CompareDateTime(market.MarketEnd) > 0)
            {
                market.MarketStatus = true;
            }
            else
            {
                market.MarketStatus = false;
            }

            if (ModelState.IsValid)
            {                
                ctx.Markets.Add(market);
                ctx.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(market);
        }

        // GET: Markets/Edit/5
        [RestrictedForAdmins]
        public ActionResult Edit(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Market market = ctx.Markets.Find(id);
            if (market == null)
            {
                return HttpNotFound();
            }
            return View(market);
        }

        // POST: Markets/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RestrictedForAdmins]
        public ActionResult Edit([Bind(Include = "Id,MarketName,MarketStart,MarketEnd,MarketType,MarketStatus")] Market market)
        {
            if (ModelState.IsValid)
            {
                ctx.Entry(market).State = EntityState.Modified;
                ctx.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(market);
        }

        // GET: Markets/Delete/5
        [RestrictedForAdmins]
        public ActionResult Delete(Guid? id)
        {
            
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Market market = ctx.Markets.Find(id);
            if (market == null)
            {
                return HttpNotFound();
            }
            return View(market);
        }

        // POST: Markets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [RestrictedForAdmins]
        public ActionResult DeleteConfirmed(Guid id)
        {
            Market market = ctx.Markets.Find(id);
            if (!ctx.ProductMarket.Any(x => x.MarketId == id))
            {
                ctx.Markets.Remove(market);
                ctx.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        //Compares a given date against DateTime.Now.
        //Returns a bool value true if given date is later.
        private int CompareDateTime (DateTime time)
        {
            int result = DateTime.Compare(time, DateTime.Now);
            return result;
        }

        //Concatenates two strings and parses them. The parameters given must be a date and a time. 
        //Format for date: YYYY-MM-DD. Format for time: HH:mm 
        private DateTime ConcatenateDateTime (string date, string time)
        {
            var text = date + " " + time;
            string pattern = "dd.MM.yyyy HH:mm";
            var current = new DateTime();
            var result = new DateTime();
            if (DateTime.TryParseExact(text, pattern, null, DateTimeStyles.None, out current))
            {
                result = current;
            }
            else
            {
                result = DateTime.Now;
            }
            return result;
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
