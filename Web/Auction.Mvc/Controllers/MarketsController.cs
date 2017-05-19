using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Auction.Data.Ef;
using Auction.Domain;
using Auction.Mvc.Attributes;
using Auctions.Mvc.Core;

namespace Auction.Mvc.Controllers
{

    public class MarketsController : Controller
    {
        private EfContext db = new EfContext();

         

        // GET: Markets
        public ActionResult Index()
        {
            return View(db.Markets.Where(x => x.MarketStatus == 1).ToList());
        }

        // GET: Markets/Details/5
        public ActionResult Details(Guid? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Market market = db.Markets.Find(id);
            if (market == null)
                return HttpNotFound();

            return View(market);
        }

        

        // GET: Markets/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Markets/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
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
            if (!CompareDateTime(market.MarketStart))
            {
                if (CompareDateTime(market.MarketEnd))
                {
                    market.MarketStatus = 1;
                }
            }
            else market.MarketStatus = 0;

            if (ModelState.IsValid)
            {                
                db.Markets.Add(market);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(market);
        }

        // GET: Markets/Edit/5
        public ActionResult Edit(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Market market = db.Markets.Find(id);
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
        public ActionResult Edit([Bind(Include = "Id,MarketStart,MarketEnd,MarketType,MarketStatus")] Market market)
        {
            if (ModelState.IsValid)
            {
                db.Entry(market).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(market);
        }

        // GET: Markets/Delete/5
        public ActionResult Delete(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Market market = db.Markets.Find(id);
            if (market == null)
            {
                return HttpNotFound();
            }
            return View(market);
        }

        // POST: Markets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(Guid id)
        {
            Market market = db.Markets.Find(id);
            db.Markets.Remove(market);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        //Compares a given date against DateTime.Now.
        //Returns a bool value true if given date is later.
        private bool CompareDateTime (DateTime time)
        {
            bool result = new bool();
            int i = DateTime.Compare(DateTime.Now, time);
            if (i < 0)
                result = false;

            else
                result = true;

            return result;
        }

        //Concatenates two strings and parses them. The parameters given must be a date and a time. 
        //Format for date: YYYY-MM-DD. Format for time: HH:mm 
        private DateTime ConcatenateDateTime (string date, string time)
        {
            var text = date + " " + time;
            var current = new DateTime();
            var result = new DateTime();
            if (DateTime.TryParse(text, out current))
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
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
