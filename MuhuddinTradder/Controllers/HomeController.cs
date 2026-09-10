using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MTDBMVC.Models;

namespace MTDBMVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly MTDbContext db;

        public HomeController(MTDbContext context)
        {
            db = context;
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

            model.TrdCd = GetNextTraderCode();

            db.Traders.Add(model);
            db.SaveChanges();

            TempData["Success"] = "Trader save ho gaya.";

            return RedirectToAction("TraderDetails");
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

            db.SaveChanges();

            TempData["Success"] = "Trader update ho gaya.";

            return RedirectToAction("TraderDetails");
        }

        public IActionResult DeleteTrader(string id)
        {
            var trader = db.Traders.Find(id);
            if (trader != null)
            {
                db.Traders.Remove(trader);
                db.SaveChanges();
                TempData["Success"] = "Trader delete ho gaya.";
            }

            return RedirectToAction("TraderDetails");
        }

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
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = db.ItemCategories.ToList();
                ViewBag.Units = db.Units.ToList();

                return View("Item", model);
            }

            model.ItmCd = GetNextItemCode();

            db.Items.Add(model);
            db.SaveChanges();

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
        public IActionResult SavePurchase(MtPurMst model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Traders = db.Traders.Where(x => x.TrdType == "S").ToList();
                return View("Purchase", model);
            }

            model.InvCd = GetNextPurchaseNo();

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
        public IActionResult EditPurchase(MtPurMst model)
        {
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

            db.SaveChanges();

            TempData["Success"] = "Purchase update ho gaya.";

            return RedirectToAction("PurchaseDetails");
        }

        public IActionResult DeletePurchase(string id)
        {
            var details = db.PurchaseDetails.Where(x => x.InvCd == id).ToList();
            db.PurchaseDetails.RemoveRange(details);

            var master = db.Purchases.Find(id);
            if (master != null)
                db.Purchases.Remove(master);

            db.SaveChanges();

            TempData["Success"] = "Purchase delete ho gaya.";

            return RedirectToAction("PurchaseDetails");
        }

        private string GetNextPurchaseNo()
        {
            var last = db.Purchases
                .OrderByDescending(x => x.InvCd)
                .Select(x => x.InvCd)
                .FirstOrDefault();

            if (string.IsNullOrEmpty(last))
                return "PUR0001";

            if (last.StartsWith("PUR") &&
                int.TryParse(last.Substring(3), out int number))
            {
                number++;
                return "PUR" + number.ToString("D4");
            }

            return "PUR0001";
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

        public IActionResult TraderDetails()
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
