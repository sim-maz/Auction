using Auction.Data.Ef;
using Auction.Domain;
using Auction.Mvc.Core;
using Auction.Mvc.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Auction.Mvc.Controllers
{
    public class UserController : Controller
    {
        EfContext ctx = new EfContext();

        // Displays the User profile page. Passes watched, won item and comment counts into the viewbag.
        public ActionResult Index()
        {

            ViewBag.WatchedItem = GetWatchedItems();
            ViewBag.WonItem = GetWonItems();
            ViewBag.Comments = GetCommentCount();
            return View(GetWatchedItems());
        }

        // Renders a partial view with items the user has won
        public ActionResult WonItemsPreview()
        {
            return PartialView("_WonItemsPreview", GetWonItems());
        }


        // Returns the View that displays all of the items user is currently bidding on
        public ActionResult WatchedItems()
        {
            return View(GetWatchedItems());
        }

        // Returns the View that displays all of the items the user has already won
        public ActionResult WonItems()
        {
            return View(GetWonItems());
        }

        // Searches the DB table dbo.COMMENTS for any comments the user has made and returns a Count()
        private int GetCommentCount()
        {
            var results = ctx.Comments.Where(x => x.User == CurrentUser.Login).Count();
            return results;
        }

        // Searches the DB for all items the user has already placed bid on, but still hasn't won yet
        private IEnumerable<UserDto> GetWatchedItems()
        {
            var activeMarkets = ctx.Markets.Where(x => x.MarketStatus == true).Select(x => x.Id).ToList();
            var activeProducts = new List<Guid>();
            var bids = ctx.Bids.Where(x => x.User == CurrentUser.Login).ToList();
            foreach (var item in activeMarkets)
            {
                activeProducts.AddRange(ctx.ProductMarket.Where(x => x.MarketId == item).Select(x => x.ProductId).ToList());
            }

            var watchedBidsAll = bids.OrderByDescending(x => x.Sum).GroupBy(x => x.ProductId).Distinct().Select(x => x.First());
            var watchedBidsOnly = watchedBidsAll.Where(x => x.Winner == false);
            
            var results = GetItems(watchedBidsOnly);

            return results;
        }
        

        // Searches the DB table dbo.BIDS for any bids the user has placed that were the highest bids once the market ended, a.k.a user's won items
        private IEnumerable<UserDto> GetWonItems()
        {
            var wonBids = ctx.Bids.Where(x => (x.User == CurrentUser.Login) && (x.Winner == true)).ToList();
            var results = GetItems(wonBids);
            return results;
        }

        // Returns a UserDto from a given IEnumarable<Bid> object
        private IEnumerable<UserDto> GetItems(IEnumerable<Bid> data)
        {
            var results = data.Select(x => new UserDto
            {
                ProductId = x.ProductId,
                ProductName = ctx.Products.FirstOrDefault(p => p.Id == x.ProductId).Name,
                CategoryName = ctx.Categories.FirstOrDefault(c => c.Id == (ctx.Products.FirstOrDefault(p => p.Id == x.ProductId).CategoryId)).Name,
                CurrentBid = x.Sum
            }).ToList();

            return results;
        }
    }
}