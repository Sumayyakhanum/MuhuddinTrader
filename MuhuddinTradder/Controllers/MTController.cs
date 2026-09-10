using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MTDBMVC.Models;

namespace MTDBMVC.Controllers
{
    public class MTController : Controller
    {
        private readonly MTDbContext db;
        private readonly IWebHostEnvironment env;

        // Purchase invoice attachments is folder ke andar save hoti hain (wwwroot/uploads/purchase)
        private const string PurchaseAttachmentFolder = "uploads/purchase";

        public MTController(MTDbContext context, IWebHostEnvironment hostEnvironment)
        {
            db = context;
            env = hostEnvironment;
        }

        // Attachment file ko wwwroot/uploads/purchase mein save karta hai aur relative path return karta hai
        private string? SavePurchaseAttachment(IFormFile? file)
        {
            if (file == null || file.Length == 0)
                return null;

            var webRoot = env.WebRootPath;
            if (string.IsNullOrEmpty(webRoot))
                webRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

            var uploadDir = Path.Combine(webRoot, "uploads", "purchase");
            Directory.CreateDirectory(uploadDir);

            var rawName = Path.GetFileNameWithoutExtension(file.FileName);
            var safeFileName = System.Text.RegularExpressions.Regex.Replace(rawName, "[^a-zA-Z0-9_-]", "_");
            if (string.IsNullOrWhiteSpace(safeFileName))
                safeFileName = "attachment";
            var extension = Path.GetExtension(file.FileName);
            var uniqueName = $"{safeFileName}_{DateTime.Now:yyyyMMddHHmmssfff}{extension}";
            var fullPath = Path.Combine(uploadDir, uniqueName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                file.CopyTo(stream);
            }

            return $"{PurchaseAttachmentFolder}/{uniqueName}";
        }

        // =====================================================
        // DASHBOARD
        // =====================================================

        public IActionResult Dashboard()
        {
            ViewBag.TotalItems = db.Items.Count();
            ViewBag.TotalSuppliers = db.Traders.Count(x => x.TrdType == "S");
            ViewBag.TotalCustomers = db.Traders.Count(x => x.TrdType == "C");

            var purDtls = db.PurchaseDetails.ToList();
            var saleDtls = db.SaleDetails.ToList();

            ViewBag.TotalPurchase = purDtls.Sum(x => x.Cost ?? 0);
            ViewBag.TotalSales = saleDtls.Sum(x => x.Cost ?? 0);
            ViewBag.TotalProfit = saleDtls.Sum(x => ((x.Rate ?? 0) - (x.PurRate ?? 0)) * (x.RcvgQty ?? 0));

            return View();
        }


        // =====================================================
        // TRADER
        // =====================================================

        public IActionResult Trader()
        {
            return View();
        }

        [HttpPost]
        public IActionResult SaveTrader(MtTraderMst model)
        {
            if (!ModelState.IsValid)
                return View("Trader", model);

            var exists = db.Traders.Any(t => t.TrdCd == model.TrdCd);
            if (exists)
            {
                ModelState.AddModelError("TrdCd", "Ye Trader Code pehle se maujood hai. Doosra code likhen.");
                return View("Trader", model);
            }

            db.Traders.Add(model);
            db.SaveChanges();
            TempData["Success"] = "Trader save ho gaya.";
            return RedirectToAction("TraderDetail");
        }

        [HttpGet]
        public IActionResult EditTrader(string id)
        {
            var trader = db.Traders.Find(id);
            if (trader == null)
                return NotFound();
            return View(trader);
        }

        [HttpPost]
        public IActionResult EditTrader(MtTraderMst model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var existing = db.Traders.Find(model.TrdCd);
            if (existing == null)
                return NotFound();

            existing.TrdDesc = model.TrdDesc;
            existing.TrdType = model.TrdType;
            existing.TrdCate = model.TrdCate;
            existing.TrdAdd = model.TrdAdd;
            existing.TrdStr = model.TrdStr;
            existing.TrdNtn = model.TrdNtn;
            // TrdCd yahan update NAHI karna - primary key hai, FK relations tootne ka risk hai

            db.SaveChanges();
            TempData["Success"] = "Trader update ho gaya.";
            return RedirectToAction("TraderDetail");
        }

        //public IActionResult DeleteTrader(string id)
        //{
        //    var trader = db.Traders.Find(id);
        //    if (trader != null)
        //    {
        //        db.Traders.Remove(trader);
        //        db.SaveChanges();
        //        TempData["Success"] = "Trader delete ho gaya.";
        //    }

        //    return RedirectToAction("TraderDetail");
        //}


        [HttpGet]
        public IActionResult DeleteTrader(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest();
            }

            var trader = db.Traders.FirstOrDefault(x => x.TrdCd == id);
            if (trader == null)
            {
                TempData["Error"] = "Trader not found.";
                return RedirectToAction("TraderDetail");
            }

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    // 1. Trader ki saari Purchases nikalo
                    var purchaseCodes = db.Purchases
                        .Where(x => x.TrdCd == id)
                        .Select(x => x.InvCd)
                        .ToList();

                    // Un purchases ke details delete karo
                    var purchaseDetails = db.PurchaseDetails
                        .Where(x => purchaseCodes.Contains(x.InvCd))
                        .ToList();
                    db.PurchaseDetails.RemoveRange(purchaseDetails);

                    // Phir purchase master delete karo
                    var purchases = db.Purchases.Where(x => x.TrdCd == id).ToList();
                    db.Purchases.RemoveRange(purchases);

                    // 2. Trader ki saari Sales nikalo
                    var saleCodes = db.Sales
                        .Where(x => x.TrdCd == id)
                        .Select(x => x.InvCd)
                        .ToList();

                    var saleDetails = db.SaleDetails
                        .Where(x => saleCodes.Contains(x.InvCd))
                        .ToList();
                    db.SaleDetails.RemoveRange(saleDetails);

                    var sales = db.Sales.Where(x => x.TrdCd == id).ToList();
                    db.Sales.RemoveRange(sales);

                    // 3. Ab Trader delete karo
                    db.Traders.Remove(trader);

                    db.SaveChanges();
                    transaction.Commit();

                    TempData["Success"] = "Trader aur uska related purchase/sale data delete ho gaya.";
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    TempData["Error"] = "Delete karte waqt error aaya: " + ex.Message;
                }
            }

            return RedirectToAction("TraderDetail");
        }

        //[HttpGet]
        //public IActionResult DeleteTrader(string id)
        //{
        //    if (string.IsNullOrEmpty(id))
        //    {
        //        return BadRequest();
        //    }

        //    var trader = db.Traders
        //        .FirstOrDefault(x => x.TrdCd == id);

        //    if (trader == null)
        //    {
        //        TempData["Error"] = "Trader not found.";
        //        return RedirectToAction("TraderDetail");
        //    }

        //    // Check purchase records
        //    bool hasPurchases = db.Purchases
        //        .Any(x => x.TrdCd == id);

        //    if (hasPurchases)
        //    {
        //        TempData["Error"] =
        //            "This trader cannot be deleted because purchase records exist.";

        //        return RedirectToAction("TraderDetail");
        //    }

        //    // Check sales records
        //    bool hasSales = db.Sales
        //        .Any(x => x.TrdCd == id);

        //    if (hasSales)
        //    {
        //        TempData["Error"] =
        //            "This trader cannot be deleted because sales records exist.";

        //        return RedirectToAction("TraderDetail");
        //    }

        //    db.Traders.Remove(trader);
        //    db.SaveChanges();

        //    TempData["Success"] = "Trader deleted successfully.";

        //    return RedirectToAction("TraderDetail");
        //}

        private string GetNextTraderCode()
        {
            var last = db.Traders
                .OrderByDescending(x => x.TrdCd)
                .Select(x => x.TrdCd)
                .FirstOrDefault();

            if (string.IsNullOrEmpty(last))
                return "0001";

            if (int.TryParse(last, out int number))
            {
                number++;
                return number.ToString("D4");
            }

            return "0001";
        }


        // =====================================================
        // ITEM
        // =====================================================

        public IActionResult Item()
        {
            ViewBag.Categories = db.ItemCategories.ToList();
            ViewBag.Units = db.Units.ToList();
            return View();
        }

        [HttpPost]
        public IActionResult SaveItem(MtItmMst model)
        {
            ModelState.Remove("ItmCd"); // ItmCd form se nahi aati, auto-generate hogi

            if (!ModelState.IsValid)
            {
                ViewBag.Categories = db.ItemCategories.ToList();
                ViewBag.Units = db.Units.ToList();
                return View("Item", model);
            }

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    model.ItmCd = GetNextItemCode();
                    db.Items.Add(model);
                    db.SaveChanges();
                    transaction.Commit();
                }
                catch (DbUpdateException)
                {
                    transaction.Rollback();
                    ModelState.AddModelError("", "Item code conflict hua, dobara try karein.");
                    ViewBag.Categories = db.ItemCategories.ToList();
                    ViewBag.Units = db.Units.ToList();
                    return View("Item", model);
                }
            }

            TempData["Success"] = "Item save ho gaya.";
            return RedirectToAction("ItemDetails");
        }

      
        [HttpGet]
        public IActionResult EditItem(string id)
        {
            var item = db.Items.Find(id);
            if (item == null)
                return NotFound();

            ViewBag.Categories = db.ItemCategories.ToList();
            ViewBag.Units = db.Units.ToList();

            return View(item);
        }

        [HttpPost]
        public IActionResult EditItem(MtItmMst model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = db.ItemCategories.ToList();
                ViewBag.Units = db.Units.ToList();
                return View(model);
            }

            var existing = db.Items.Find(model.ItmCd);
            if (existing == null)
                return NotFound();

            existing.CatCd = model.CatCd;
            existing.ItmDesc = model.ItmDesc;
            existing.UnitCd = model.UnitCd;
            existing.ItmStatus = model.ItmStatus;
            existing.ItmShelfLife = model.ItmShelfLife;
            existing.ItmMoq = model.ItmMoq;

            db.SaveChanges();

            TempData["Success"] = "Item update ho gaya.";

            return RedirectToAction("ItemDetails");
        }

        public IActionResult DeleteItem(string id)
        {
            var item = db.Items.Find(id);
            if (item != null)
            {
                db.Items.Remove(item);
                db.SaveChanges();
                TempData["Success"] = "Item delete ho gaya.";
            }

            return RedirectToAction("ItemDetails");
        }

        private string GetNextItemCode()
        {
            var last = db.Items
                .OrderByDescending(x => x.ItmCd)
                .Select(x => x.ItmCd)
                .FirstOrDefault();

            if (string.IsNullOrEmpty(last))
                return "0001";

            if (int.TryParse(last, out int number))
            {
                number++;
                return number.ToString("D4");
            }

            return "0001";
        }


        // =====================================================
        // PURCHASE
        // =====================================================

        public IActionResult Purchase()
        {
            ViewBag.Traders = db.Traders.Where(x => x.TrdType == "S").ToList();

            return View();
        }

        [HttpPost]
        public IActionResult SavePurchase(MtPurMst model, IFormFile? AttachmentFile)
        {
            // Invoice No. ab manually likhi jaati hai (autogenerate hata diya gaya hai)
            ModelState.Remove("AttachmentPath");

            if (string.IsNullOrWhiteSpace(model.InvCd))
            {
                ModelState.AddModelError("InvCd", "Invoice No. likhna zaroori hai.");
            }
            else if (db.Purchases.Any(x => x.InvCd == model.InvCd))
            {
                ModelState.AddModelError("InvCd", "Ye Invoice No. pehle se maujood hai. Doosra number likhen.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Traders = db.Traders.Where(x => x.TrdType == "S").ToList();
                return View("Purchase", model);
            }

            model.AttachmentPath = SavePurchaseAttachment(AttachmentFile);

            db.Purchases.Add(model);
            db.SaveChanges();

            TempData["Success"] = "Purchase " + model.InvCd + " save ho gaya.";

            return RedirectToAction("PurchaseDetail", new { invCd = model.InvCd });
        }

        [HttpGet]
        public IActionResult EditPurchase(string id)
        {
            var purchase = db.Purchases.Find(id);
            if (purchase == null)
                return NotFound();

            ViewBag.Traders = db.Traders.Where(x => x.TrdType == "S").ToList();

            return View(purchase);
        }

        [HttpPost]
        public IActionResult EditPurchase(MtPurMst model, IFormFile? AttachmentFile)
        {
            ModelState.Remove("AttachmentPath");

            if (!ModelState.IsValid)
            {
                ViewBag.Traders = db.Traders.Where(x => x.TrdType == "S").ToList();
                return View(model);
            }

            var existing = db.Purchases.Find(model.InvCd);
            if (existing == null)
                return NotFound();

            existing.InvDt = model.InvDt;
            existing.TrdCd = model.TrdCd;
            existing.RcvdDt = model.RcvdDt;

            // Naya attachment aaya hai to purana replace karo, warna jo pehle se hai wahi rehne den
            var newAttachment = SavePurchaseAttachment(AttachmentFile);
            if (newAttachment != null)
            {
                existing.AttachmentPath = newAttachment;
            }

            db.SaveChanges();

            TempData["Success"] = "Purchase update ho gaya.";

            return RedirectToAction("PurchaseDetails");
        }

        //public IActionResult DeletePurchase(string id)
        //{
        //    var details = db.PurchaseDetails.Where(x => x.InvCd == id).ToList();
        //    db.PurchaseDetails.RemoveRange(details);

        //    var master = db.Purchases.Find(id);
        //    if (master != null)
        //        db.Purchases.Remove(master);

        //    db.SaveChanges();

        //    TempData["Success"] = "Purchase delete ho gaya.";

        //    return RedirectToAction("PurchaseDetails");
        //}
        [HttpGet]
        public IActionResult DeletePurchase(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest();
            }

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    // Purchase details delete
                    var purchaseDetails = db.PurchaseDetails.Where(x => x.InvCd == id).ToList();
                    db.PurchaseDetails.RemoveRange(purchaseDetails);

                    // Purchase master delete
                    var master = db.Purchases.Find(id);
                    if (master != null)
                        db.Purchases.Remove(master);

                    // Related Sale details delete (agar same InvCd use ho raha hai)
                    var saleDetails = db.SaleDetails.Where(x => x.InvCd == id).ToList();
                    db.SaleDetails.RemoveRange(saleDetails);

                    // Related Sale master delete
                    var sale = db.Sales.Find(id);
                    if (sale != null)
                        db.Sales.Remove(sale);

                    db.SaveChanges();
                    transaction.Commit();

                    TempData["Success"] = "Purchase aur related Sale delete ho gaya.";
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    TempData["Error"] = "Delete karte waqt error aaya: " + ex.Message;
                }
            }

            return RedirectToAction("PurchaseDetails");
        }


        // =====================================================
        // PURCHASE DETAIL
        // =====================================================

        public IActionResult PurchaseDetail(string invCd)
        {
            var master = db.Purchases.Find(invCd);
            if (master == null)
                return NotFound();

            ViewBag.InvoiceId = invCd;
            ViewBag.Master = master;
            ViewBag.Items = db.Items.ToList();
            ViewBag.DetailList = db.PurchaseDetails
                .Where(x => x.InvCd == invCd)
                .ToList();

            return View();
        }

        [HttpPost]
        public IActionResult SavePurchaseDetail(MtPurDtl model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.InvoiceId = model.InvCd;
                ViewBag.Master = db.Purchases.Find(model.InvCd);
                ViewBag.Items = db.Items.ToList();
                ViewBag.DetailList = db.PurchaseDetails
                    .Where(x => x.InvCd == model.InvCd)
                    .ToList();

                return View("PurchaseDetail", model);
            }

            // Agar ye item is invoice mein pehle se hai to update, warna new row
            var existing = db.PurchaseDetails.Find(model.InvCd, model.ItmCd);

            if (existing != null)
            {
                existing.Dom = model.Dom;
                existing.Doe = model.Doe;
                existing.RcvgQty = model.RcvgQty;
                existing.Rate = model.Rate;
                existing.Disc = model.Disc;
                existing.Cost = model.Cost;
            }
            else
            {
                db.PurchaseDetails.Add(model);
            }

            db.SaveChanges();

            return RedirectToAction("PurchaseDetail", new { invCd = model.InvCd });
        }

        public IActionResult DeletePurchaseDetail(string invCd, string itmCd)
        {
            var row = db.PurchaseDetails.Find(invCd, itmCd);
            if (row != null)
            {
                db.PurchaseDetails.Remove(row);
                db.SaveChanges();
            }

            return RedirectToAction("PurchaseDetail", new { invCd = invCd });
        }


        // =====================================================
        // SALE
        // =====================================================

        public IActionResult Sale()
        {
            ViewBag.Traders = db.Traders.Where(x => x.TrdType == "C").ToList();

            return View();
        }

        [HttpPost]
        public IActionResult SaveSale(MtSaleMst model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Traders = db.Traders.Where(x => x.TrdType == "C").ToList();

                return View("Sale", model);
            }

            model.InvCd = GetNextSaleNo();

            db.Sales.Add(model);
            db.SaveChanges();

            TempData["Success"] = "Sale " + model.InvCd + " save ho gaya.";

            return RedirectToAction("SaleDetail", new { invCd = model.InvCd });
        }

        [HttpGet]
        public IActionResult EditSale(string id)
        {
            var sale = db.Sales.Find(id);
            if (sale == null)
                return NotFound();

            ViewBag.Traders = db.Traders.Where(x => x.TrdType == "C").ToList();

            return View(sale);
        }

        [HttpPost]
        public IActionResult EditSale(MtSaleMst model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Traders = db.Traders.Where(x => x.TrdType == "C").ToList();
                return View(model);
            }

            var existing = db.Sales.Find(model.InvCd);
            if (existing == null)
                return NotFound();

            existing.InvDt = model.InvDt;
            existing.TrdCd = model.TrdCd;

            db.SaveChanges();

            TempData["Success"] = "Sale update ho gaya.";

            return RedirectToAction("SaleDetails");
        }

        public IActionResult DeleteSale(string id)
        {
            var details = db.SaleDetails.Where(x => x.InvCd == id).ToList();
            db.SaleDetails.RemoveRange(details);

            var master = db.Sales.Find(id);
            if (master != null)
                db.Sales.Remove(master);

            db.SaveChanges();

            TempData["Success"] = "Sale delete ho gaya.";

            return RedirectToAction("SaleDetails");
        }

        private string GetNextSaleNo()
        {
            var last = db.Sales
                .OrderByDescending(x => x.InvCd)
                .Select(x => x.InvCd)
                .FirstOrDefault();

            if (string.IsNullOrEmpty(last))
                return "SAL0001";

            if (last.StartsWith("SAL") &&
                int.TryParse(last.Substring(3), out int number))
            {
                number++;
                return "SAL" + number.ToString("D4");
            }

            return "SAL0001";
        }


        // =====================================================
        // SALE DETAIL
        // =====================================================

        public IActionResult SaleDetail(string invCd)
        {
            var master = db.Sales.Find(invCd);
            if (master == null)
                return NotFound();

            ViewBag.InvoiceId = invCd;
            ViewBag.Master = master;
            ViewBag.Items = db.Items.ToList();
            ViewBag.Purchases = db.Purchases.ToList();
            ViewBag.DetailList = db.SaleDetails
                .Where(x => x.InvCd == invCd)
                .ToList();

            return View();
        }

        [HttpPost]
        public IActionResult SaveSaleDetail(MtSaleDtl model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.InvoiceId = model.InvCd;
                ViewBag.Master = db.Sales.Find(model.InvCd);
                ViewBag.Items = db.Items.ToList();
                ViewBag.Purchases = db.Purchases.ToList();
                ViewBag.DetailList = db.SaleDetails
                    .Where(x => x.InvCd == model.InvCd)
                    .ToList();

                return View("SaleDetail", model);
            }

            // Purchase Rate hamesha database se hi aani chahie (reference invoice ke hisaab se),
            // form se aaya hua PurRate ignore karke asal record se dobara nikal rahe hain
            // taake ye field kabhi edit karke galat na ho sake.
            if (!string.IsNullOrWhiteSpace(model.PurInv))
            {
                var purDtl = db.PurchaseDetails.Find(model.PurInv, model.ItmCd);
                model.PurRate = purDtl?.Rate ?? 0;
            }
            else
            {
                model.PurRate = 0;
            }

            var existing = db.SaleDetails.Find(model.InvCd, model.ItmCd);

            if (existing != null)
            {
                existing.PurInv = model.PurInv;
                existing.PurRate = model.PurRate;
                existing.RcvgQty = model.RcvgQty;
                existing.Rate = model.Rate;
                existing.Disc = model.Disc;
                existing.Cost = model.Cost;
            }
            else
            {
                db.SaleDetails.Add(model);
            }

            db.SaveChanges();

            return RedirectToAction("SaleDetail", new { invCd = model.InvCd });
        }

        public IActionResult DeleteSaleDetail(string invCd, string itmCd)
        {
            var row = db.SaleDetails.Find(invCd, itmCd);
            if (row != null)
            {
                db.SaleDetails.Remove(row);
                db.SaveChanges();
            }

            return RedirectToAction("SaleDetail", new { invCd = invCd });
        }


        // =====================================================
        // AJAX: ITEM ke hisaab se purchase invoice list + purchase rate
        // (Sale invoice mein "Purchase Invoice Reference" dropdown isi se bharta hai,
        //  taake sirf wohi invoice number dikhen jin mein ye item actually purchase hui ho)
        // =====================================================

        [HttpGet]
        public JsonResult GetPurchaseInvoicesByItem(string itmCd)
        {
            if (string.IsNullOrWhiteSpace(itmCd))
                return Json(new List<object>());

            // Pehle raw data database se nikal lo (SQL translatable), phir date ko
            // in-memory format karo - DateTime.ToString(format) EF Core mein SQL
            // Server ke liye translate nahi hoti, isliye ToList() ke baad karna zaroori hai.
            var raw = db.PurchaseDetails
                .Where(x => x.ItmCd == itmCd)
                .Join(db.Purchases,
                    d => d.InvCd,
                    m => m.InvCd,
                    (d, m) => new
                    {
                        invCd = d.InvCd,
                        invDt = m.InvDt,
                        rate = d.Rate ?? 0,
                        purchasedQty = d.RcvgQty ?? 0
                    })
                .OrderByDescending(x => x.invCd)
                .ToList();

            var list = raw.Select(x => new
            {
                x.invCd,
                invDt = x.invDt.HasValue ? x.invDt.Value.ToString("dd-MMM-yyyy") : "",
                x.rate,
                x.purchasedQty
            }).ToList();

            return Json(list);
        }

        // =====================================================
        // AJAX: ek item ki ab tak ki total purchase / sale / current stock
        // (Sale aur Purchase item-entry form mein item select karte hi ye dikhta hai)
        // =====================================================

        [HttpGet]
        public JsonResult GetItemStock(string itmCd)
        {
            if (string.IsNullOrWhiteSpace(itmCd))
                return Json(new { purchased = 0, sold = 0, stock = 0 });

            decimal purchased = db.PurchaseDetails
                .Where(x => x.ItmCd == itmCd)
                .Sum(x => (decimal?)x.RcvgQty) ?? 0;

            decimal sold = db.SaleDetails
                .Where(x => x.ItmCd == itmCd)
                .Sum(x => (decimal?)x.RcvgQty) ?? 0;

            return Json(new
            {
                purchased,
                sold,
                stock = purchased - sold
            });
        }


        // =====================================================
        // FETCH ALL ITEMS
        // =====================================================

        public IActionResult ItemDetails()
        {
            var items = db.Items
                .Include(x => x.Category)
                .Include(x => x.Unit)
                .OrderBy(x => x.ItmCd)
                .ToList();

            return View(items);
        }


        // =====================================================
        // FETCH ALL TRADERS
        // =====================================================

        public IActionResult TraderDetail()
        {
            var traders = db.Traders
                .OrderBy(x => x.TrdCd)
                .ToList();

            return View(traders);
        }


        // =====================================================
        // PURCHASE DETAILS (list of invoices)
        // =====================================================

        public IActionResult PurchaseDetails()
        {
            var purchases = db.Purchases
                .Include(x => x.Trader)
                .OrderByDescending(x => x.InvDt)
                .ToList();

            return View(purchases);
        }


        // =====================================================
        // SALE DETAILS (list of invoices)
        // =====================================================

        public IActionResult SaleDetails()
        {
            var sales = db.Sales
                .Include(x => x.Trader)
                .OrderByDescending(x => x.InvDt)
                .ToList();

            return View(sales);
        }


        // =====================================================
        // STOCK / PROFIT / LOSS
        // =====================================================

        public IActionResult StockDetails()
        {
            var items = db.Items.ToList();

            var purchaseDetails = db.PurchaseDetails.ToList();

            var saleDetails = db.SaleDetails.ToList();

            var result = items.Select(item =>
            {
                decimal purchased = purchaseDetails
                    .Where(x => x.ItmCd == item.ItmCd)
                    .Sum(x => x.RcvgQty ?? 0);

                decimal sold = saleDetails
                    .Where(x => x.ItmCd == item.ItmCd)
                    .Sum(x => x.RcvgQty ?? 0);

                decimal stock = purchased - sold;

                decimal profitLoss = saleDetails
                    .Where(x => x.ItmCd == item.ItmCd)
                    .Sum(x =>
                        ((x.Rate ?? 0) - (x.PurRate ?? 0))
                        * (x.RcvgQty ?? 0)
                    );

                return new StockRow
                {
                    ItmCd = item.ItmCd,
                    ItmDesc = item.ItmDesc,
                    ItmStatus = item.ItmStatus,
                    ItmMoq = item.ItmMoq,

                    Purchased = purchased,
                    Sold = sold,
                    Stock = stock,

                    ProfitLoss = profitLoss,

                    LowQuantity =
                        item.ItmMoq.HasValue &&
                        stock <= item.ItmMoq.Value
                };

            }).ToList();

            return View(result);
        }


        // =====================================================
        // CATEGORY / UNIT (chhoti master tables, dropdowns ke liye)
        // =====================================================

        [HttpGet]
        public IActionResult Category()
        {
            return View(db.ItemCategories.ToList());
        }

        [HttpPost]
        public IActionResult SaveCategory(MtItemCate model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Success"] = "Category code aur description dono zaroori hain.";
            }
            else if (db.ItemCategories.Any(x => x.CatCd == model.CatCd))
            {
                TempData["Success"] = "Ye Category Code pehle se maujood hai.";
            }
            else
            {
                db.ItemCategories.Add(model);
                db.SaveChanges();
                TempData["Success"] = "Category save ho gayi.";
            }

            return RedirectToAction("Category");
        }

        public IActionResult DeleteCategory(string id)
        {
            var cat = db.ItemCategories.Find(id);
            if (cat != null)
            {
                db.ItemCategories.Remove(cat);
                db.SaveChanges();
            }

            return RedirectToAction("Category");
        }

        [HttpGet]
        public IActionResult Unit()
        {
            return View(db.Units.ToList());
        }

        [HttpPost]
        public IActionResult SaveUnit(MtUnitMst model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Success"] = "Unit code aur description dono zaroori hain.";
            }
            else if (db.Units.Any(x => x.UnitCd == model.UnitCd))
            {
                TempData["Success"] = "Ye Unit Code pehle se maujood hai.";
            }
            else
            {
                db.Units.Add(model);
                db.SaveChanges();
                TempData["Success"] = "Unit save ho gaya.";
            }

            return RedirectToAction("Unit");
        }

        public IActionResult DeleteUnit(string id)
        {
            var unit = db.Units.Find(id);
            if (unit != null)
            {
                db.Units.Remove(unit);
                db.SaveChanges();
            }

            return RedirectToAction("Unit");
        }
    }
}
