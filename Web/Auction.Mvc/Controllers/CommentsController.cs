using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Auction.Mvc.Controllers;
using Auction.Data.Ef;
using Auction.Domain;
using Auction.Mvc.Models;
using Auction.Mvc.Core;
using System.Net;
using System.Text;
using Auction.Mvc.Attributes;
using System.Data.Entity;

namespace Auction.Mvc.Controllers
{
    [Authorize]
    public class CommentsController : Controller
    {
        EfContext ctx = new EfContext();


        public ActionResult Index(Guid productId)
        {

            var data = GetComments(productId);
            ViewBag.ProductId = productId;
            return View(data);
        }

        public ActionResult Login(string login)
        {
            var data = GetComments();
            return View("Index",data);
        }

        public ActionResult Preview(Guid productId)
        {
            var data = GetComments(productId);
            ViewBag.ProductId = productId;
            return PartialView("_Preview", data);
        }

        public ActionResult PreviewUser(string login)
        {
            var data = GetComments();
            return PartialView("_PreviewUser", data);
        }

        // Collects comment data from the form. 
        // Encodes the comment text and then manually decodes <b> and <i> tags so that they can't be properly rendered later on when the comments are displayed.
        [HttpPost, ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult Post (FormCollection form)
        {
            StringBuilder sb = new StringBuilder(
                HttpUtility.HtmlEncode(Request.Unvalidated.Form["commentText"].Trim())
                );

            sb.Replace("&lt;b&gt;", "<b>");
            sb.Replace("&lt;/b&gt;", "</b>");
            sb.Replace("&lt;i&gt;", "<i>");
            sb.Replace("&lt;/i&gt;", "</i>");
            var commentText = sb.ToString();

            //Adds the comment data into Comment() object.
            Comment comment = new Comment();
            comment.Id = Guid.NewGuid();
            comment.ProductId = Guid.Parse(Request.Form["productId"]);
            comment.Text = commentText;
            comment.Time = DateTime.Now;
            comment.User = CurrentUser.Login;

            if (ModelState.IsValid)
            {
                ctx.Comments.Add(comment);
                if(comment.Text != "")
                    ctx.SaveChanges();
            }
            return Redirect(Request.UrlReferrer.ToString());
        }

        //Gets the comments according to a given productId
        private List<CommentDto> GetComments(Guid productId)
        {
            var data = ctx.Comments.Where(x => x.ProductId == productId).AsEnumerable().Select(x => new CommentDto
            {
                Id = x.Id,
                ProductId = x.ProductId,
                Text = x.Text,
                User = x.User,
                Time = x.Time
            }).OrderByDescending(x => x.Time).ToList();
            return data;
        }

        //Gets the comments according to the current user
        private List<CommentDto> GetComments()
        {
            var data = ctx.Comments.Where(x => x.User == CurrentUser.Login).AsEnumerable().Select(x => new CommentDto
            {
                Id = x.Id,
                ProductId = x.ProductId,
                Text = x.Text,
                User = x.User,
                Time = x.Time
            }).OrderByDescending(x => x.Time).ToList();
            return data;
        }

        [RestrictedForAdmins]
        public ActionResult Edit(Guid id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Comment comment = ctx.Comments.Find(id);
            if (comment == null)
            {
                return HttpNotFound();
            }

            return View(comment);
        }

        [HttpPost, ValidateInput(false)]
        [RestrictedForAdmins]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,ProductId,Text,User,Time")] Comment comment)
        {
            StringBuilder sb = new StringBuilder(
                HttpUtility.HtmlEncode(comment.Text.Trim()));

            sb.Replace("&lt;b&gt;", "<b>");
            sb.Replace("&lt;/b&gt;", "</b>");
            sb.Replace("&lt;i&gt;", "<i>");
            sb.Replace("&lt;/i&gt;", "</i>");
            var commentText = sb.ToString();

            if (ModelState.IsValid)
            {
                comment.Text = commentText;
                ctx.Entry(comment).State = EntityState.Modified;
                ctx.SaveChanges();
            }
            return RedirectToAction("Index", "Admin");
        }

        public ActionResult Delete(Guid id) {
           
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Comment comment = ctx.Comments.Find(id);
            if (comment == null)
            {
                return HttpNotFound();
            }

            ctx.Comments.Remove(comment);
            ctx.SaveChanges();

            return RedirectToAction("Index", "Admin");
        }
    }
}