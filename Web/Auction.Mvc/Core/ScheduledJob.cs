using System;
using System.Collections.Generic;
using System.Linq;
using FluentScheduler;
using System.Web.Hosting;
using Auction.Data.Ef;
using Auction.Domain;
using System.Data.Entity;
using System.Net.Mail;
using System.Net;

namespace Auction.Mvc.Core
{
    public class ScheduledJob : IJob, IRegisteredObject
    {
        private readonly object _lock = new object();

        private bool _shuttingDown;

        

        public ScheduledJob()
        {
            HostingEnvironment.RegisterObject(this);
        }

        //Method that gets executed when ScheduledJob class is called by the TaskRegistry.
        public void Execute()
        {
            lock (_lock)
            {
                if (_shuttingDown)
                    return;

                CheckMarketStatus();
            }
        }

        public void Stop(bool immediate)
        {
            //Locking here will wait for the lock in Execute to be release until this code can continue.
            lock (_lock)
            {
                _shuttingDown = true;
            }

            HostingEnvironment.UnregisterObject(this);
        }

        public void CheckMarketStatus()
        {
            EfContext ctx = new EfContext();
            var markets = ctx.Markets.ToList();
            using(ctx) { 
            //Check if given date is later than DateTime.Now. Compares for start time and end time. 
                foreach (var market in markets)
                {
                    //MarketStatus true means Market is active, false means inactive
                    var original = ctx.Markets.Find(market.Id).MarketStatus;
                    if (CompareDateTime(market.MarketStart) < 0 && CompareDateTime(market.MarketEnd) > 0)
                    {
                        market.MarketStatus = true;
                    }
                    else
                    {
                        market.MarketStatus = false;
                    }

                    //Market status changes are saved to DB table dbo.MARKETS
                    ctx.Entry(market).State = EntityState.Modified;
                    ctx.SaveChanges();

                    //Checks if there were any changes to the MarketStatus by comparing the original value and the new value.
                    if ((original != market.MarketStatus) && !market.MarketStatus && !market.MarketClosed) { 
                        CheckItemWinners(market.Id);
                        market.MarketClosed = true;
                        ctx.Entry(market).State = EntityState.Modified;
                        ctx.SaveChanges();
                    }
                }
            }
        }

        //Method checks for the top bids of all the products that belong to the closed Market and set the their Winner property to TRUE. Saves the changes to DB table dbo.BIDS.
        //Returns the list of winning bids.
        private List<Bid> CheckItemWinners(Guid marketId)
        {
            EfContext ctx = new EfContext();

            var bids = new List<Bid>();
            var products = ctx.ProductMarket.Where(x => x.MarketId == marketId).Select(x => x.ProductId).ToList();

            foreach (var prod in products)
            {
                if (ctx.Bids.Where(x => x.ProductId == prod).Any())
                {
                    bids.Add(ctx.Bids.Where(x => x.ProductId == prod).OrderByDescending(x => x.Sum).FirstOrDefault());
                }
            }

            foreach(var bid in bids)
            {
                EmailWinners(bid);
                bid.Winner = true;
                ctx.Entry(bid).State = EntityState.Modified;
            }

            ctx.SaveChanges();              

            return bids;
        }


        //Compares two dates and returns and int type value. value > 0 means that first date was later, while value < 0 means that the first date was earlier.  
        private int CompareDateTime(DateTime time)
        {
            int result = DateTime.Compare(time, DateTime.Now);
            return result;
        }

        //Sends the winners an e-mail according to the template ~/Views/Emails/Winner.cshtml
        private async void EmailWinners(Bid winner)
        {
            EfContext ctx = new EfContext();
            using (ctx) {
                string emailLink = "http://aswaukcionas.software.alna.lt/Products/Details/" + winner.ProductId;
                string wonItemName = ctx.Products.Where(x => x.Id == winner.ProductId).FirstOrDefault().Name;
                string fromMessage = "Congratulations! You won <b>" + wonItemName + " !</b>" + "<br /> To see your item, follow this link: " + emailLink;

                var body = "<p> Email From: {0} ({1}) </p><p>Message:</p><p>{2}</p>";
                var message = new MailMessage();
                message.To.Add(new MailAddress(winner.User + "@alna.lt"));
                message.From = new MailAddress("aukcionas@alna.lt");
                message.Subject = "Won Item Notice: " + wonItemName;
                message.Body = string.Format(body, "Aukcionas", "aukcionas@alna.lt", fromMessage);
                message.IsBodyHtml = true;

                using (var smtp = new SmtpClient())
                {
                    var credential = new NetworkCredential
                    {
                        UserName = "aukcionas@alna.lt",
                        Password = "Vilnius.321"
                    };
                    smtp.Credentials = credential;
                    smtp.Host = "pandora.alna.lt";
                    smtp.Port = 25;
                    smtp.EnableSsl = true;
                    await smtp.SendMailAsync(message);

                }
            }
        }
    }
}