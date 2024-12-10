using createdelete.Models;
using createdelete.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace createdelete.Controllers
{
    public class ProductsController : Controller
    {
        readonly ProductDbContext db = new ProductDbContext();
        // GET: Products
        public ActionResult Index()
        {
            var data = db.Products.ToList();
            return View(data);
        }
        public ActionResult Create()
        {
            var model = new ProductInputModel();
            model.Sales.Add(new Sale { });
            ViewBag.Sellers = db.Sellers.ToList();
            return View(model);


        }
        [HttpPost]
        public ActionResult Create(ProductInputModel model, string operation = "")
        {

            if (operation == "add")
            {
                model.Sales.Add(new Sale { });
                foreach (var e in ModelState.Values)
                {
                    e.Errors.Clear();
                    e.Value = null;
                }
            }
            if (operation.StartsWith("del"))
            {
                int pos = operation.IndexOf("_");
                int index = int.Parse(operation.Substring(pos + 1));
                model.Sales.RemoveAt(index);
                foreach (var e in ModelState.Values)
                {
                    e.Errors.Clear();
                    e.Value = null;
                }
            }
            if (operation == "insert")
            {
                if (ModelState.IsValid)
                {
                    var p = new Product
                    {
                        ProductName = model.ProductName,
                        Price = model.Price,
                        MfgDate = model.MfgDate,
                        InStock = model.InStock
                    };
                    string ext = Path.GetExtension(model.Picture.FileName);
                    string f = Path.GetFileNameWithoutExtension(Path.GetRandomFileName()) + ext;
                    string savePath = Path.Combine(Server.MapPath("~/Pictures"), f);
                    model.Picture.SaveAs(savePath);
                    p.Picture = f;
                    foreach (var s in model.Sales)
                    {
                        p.Sales.Add(new Sale { Date = s.Date, SellerId = s.SellerId, Quantity = s.Quantity });
                    }
                    db.Products.Add(p);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
            }

            ViewBag.Sellers = db.Sellers.ToList();
            return View(model);
        }
        [HttpPost]
        public ActionResult Delete(int id)
        {
            var product = db.Products.FirstOrDefault(x => x.ProductId == id);
            if (product == null) { return new HttpNotFoundResult(); }
            db.Sales.RemoveRange(product.Sales.ToList());
            db.Products.Remove(product);
            db.SaveChanges();
            return Json(new { success = true });
        }
    }
}