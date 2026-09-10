CREATE DATABASE MTDB;
GO

USE MTDB;
GO


/* =========================================
   1. ITEM CATEGORY
   ========================================= */

CREATE TABLE mt_item_cate
(
    cat_cd      VARCHAR(2) PRIMARY KEY,
    cat_desc    VARCHAR(100),
    cat_status  VARCHAR(1)
);
GO


/* =========================================
   2. UNIT MASTER
   ========================================= */

CREATE TABLE mt_unit_mst
(
    unit_cd      VARCHAR(2) PRIMARY KEY,
    unit_desc    VARCHAR(100),
    unit_status  VARCHAR(1)
);
GO


/* =========================================
   3. ITEM MASTER
   ========================================= */

CREATE TABLE mt_itm_mst
(
    itm_cd          VARCHAR(4) PRIMARY KEY,
    cat_cd          VARCHAR(2),
    itm_desc        VARCHAR(100),
    unit_cd         VARCHAR(2),
    itm_status      VARCHAR(1),
    itm_shelf_life  INT,
    itm_moq         DECIMAL(18,2),

    CONSTRAINT FK_Item_Category
        FOREIGN KEY (cat_cd)
        REFERENCES mt_item_cate(cat_cd),

    CONSTRAINT FK_Item_Unit
        FOREIGN KEY (unit_cd)
        REFERENCES mt_unit_mst(unit_cd)
);
GO


/* =========================================
   4. TRADER MASTER
   ========================================= */

CREATE TABLE mt_trader_mst
(
    trd_cd      VARCHAR(4) PRIMARY KEY,
    trd_desc    VARCHAR(100),
    trd_type    VARCHAR(1),
    trd_cate    VARCHAR(2),
    trd_add     VARCHAR(100),
    trd_str     VARCHAR(50),
    trd_ntn     VARCHAR(50)
);
GO


/* =========================================
   5. PURCHASE MASTER
   ========================================= */

CREATE TABLE mt_pur_mst
(
    inv_cd      VARCHAR(20) PRIMARY KEY,
    inv_dt      DATE,
    trd_cd      VARCHAR(4),
    rcvd_dt     DATE,

    CONSTRAINT FK_Purchase_Trader
        FOREIGN KEY (trd_cd)
        REFERENCES mt_trader_mst(trd_cd)
);
GO


/* =========================================
   6. PURCHASE DETAIL
   ========================================= */

CREATE TABLE mt_pur_dtl
(
    inv_cd      VARCHAR(20),
    itm_cd      VARCHAR(4),
    dom         DATE,
    doe         DATE,
    rcvg_qty    DECIMAL(18,2),
    rate        DECIMAL(18,2),
    disc        DECIMAL(18,2),
    cost        DECIMAL(18,2),

    CONSTRAINT PK_Purchase_Detail
        PRIMARY KEY (inv_cd, itm_cd),

    CONSTRAINT FK_PurchaseDetail_Master
        FOREIGN KEY (inv_cd)
        REFERENCES mt_pur_mst(inv_cd),

    CONSTRAINT FK_PurchaseDetail_Item
        FOREIGN KEY (itm_cd)
        REFERENCES mt_itm_mst(itm_cd)
);
GO


/* =========================================
   7. SALE MASTER
   ========================================= */

CREATE TABLE mt_sale_mst
(
    inv_cd      VARCHAR(20) PRIMARY KEY,
    inv_dt      DATE,
    trd_cd      VARCHAR(4),

    CONSTRAINT FK_Sale_Trader
        FOREIGN KEY (trd_cd)
        REFERENCES mt_trader_mst(trd_cd)
);
GO


/* =========================================
   8. SALE DETAIL
   ========================================= */

CREATE TABLE mt_sale_dtl
(
    inv_cd      VARCHAR(20),
    itm_cd      VARCHAR(4),
    pur_inv     VARCHAR(20),
    pur_rate    DECIMAL(18,2),
    rcvg_qty    DECIMAL(18,2),
    rate        DECIMAL(18,2),
    disc        DECIMAL(18,2),
    cost        DECIMAL(18,2),

    CONSTRAINT PK_Sale_Detail
        PRIMARY KEY (inv_cd, itm_cd),

    CONSTRAINT FK_SaleDetail_Master
        FOREIGN KEY (inv_cd)
        REFERENCES mt_sale_mst(inv_cd),

    CONSTRAINT FK_SaleDetail_Item
        FOREIGN KEY (itm_cd)
        REFERENCES mt_itm_mst(itm_cd)
);
GO


/* =========================================
   9. USER MASTER
   ========================================= */

CREATE TABLE mt_user_master
(
    user_id      VARCHAR(4) PRIMARY KEY,
    user_name    VARCHAR(100),
    user_status  VARCHAR(1)
);
GO


ALTER TABLE mt_sale_dtl
ADD CONSTRAINT FK_SaleDetail_Purchase
FOREIGN KEY (pur_inv)
REFERENCES mt_pur_mst(inv_cd);
GO


ALTER TABLE mt_user_master
ADD user_password VARCHAR(255);
GO


INSERT INTO mt_user_master
(
    user_id,
    user_name,
    user_status,
    user_password
)
VALUES
('U001', 'Admin', 'A', 'admin123'),
('U002', 'User 1', 'A', 'user123'),
('U003', 'Manager', 'A', 'manager123');
GO


SELECT
    user_id,
    user_name,
    user_status
FROM mt_user_master
WHERE user_name = 'Admin'
AND user_password = 'admin123'
AND user_status = 'A';
GO


CREATE PROCEDURE sp_UserLogin
    @UserName VARCHAR(100),
    @Password VARCHAR(255)
AS
BEGIN

    SELECT
        user_id,
        user_name,
        user_status
    FROM mt_user_master
    WHERE user_name = @UserName
      AND user_password = @Password
      AND user_status = 'A';

END;
GO


/* PURCHASE REPORT */

SELECT
    p.inv_cd AS PurchaseInvoice,
    p.inv_dt AS PurchaseDate,
    t.trd_desc AS Supplier,
    d.itm_cd,
    i.itm_desc,
    d.rcvg_qty AS Quantity,
    d.rate AS Rate,
    d.disc AS Discount,
    d.cost AS Cost
FROM mt_pur_mst p
INNER JOIN mt_pur_dtl d
    ON p.inv_cd = d.inv_cd
INNER JOIN mt_itm_mst i
    ON d.itm_cd = i.itm_cd
INNER JOIN mt_trader_mst t
    ON p.trd_cd = t.trd_cd
ORDER BY p.inv_dt DESC;
GO


/* ITEM LIST */

SELECT
    i.itm_cd,
    i.itm_desc,
    c.cat_desc AS category,
    u.unit_desc AS unit,
    i.itm_status,
    i.itm_shelf_life,
    i.itm_moq
FROM mt_itm_mst i
LEFT JOIN mt_item_cate c
    ON i.cat_cd = c.cat_cd
LEFT JOIN mt_unit_mst u
    ON i.unit_cd = u.unit_cd;
GO


/* SALES REPORT */

SELECT
    s.inv_cd AS SaleInvoice,
    s.inv_dt AS SaleDate,
    t.trd_desc AS Customer,
    d.itm_cd,
    i.itm_desc,
    d.rcvg_qty AS Quantity,
    d.rate AS SaleRate,
    d.disc AS Discount,
    d.cost AS TotalCost
FROM mt_sale_mst s
INNER JOIN mt_sale_dtl d
    ON s.inv_cd = d.inv_cd
INNER JOIN mt_itm_mst i
    ON d.itm_cd = i.itm_cd
INNER JOIN mt_trader_mst t
    ON s.trd_cd = t.trd_cd
ORDER BY s.inv_dt DESC;
GO


/* CURRENT STOCK REPORT */

SELECT
    s.inv_cd AS SaleInvoice,
    s.inv_dt AS SaleDate,
    t.trd_desc AS Customer,
    d.itm_cd,
    i.itm_desc,
    d.rcvg_qty AS Quantity,
    d.rate AS SaleRate,
    d.disc AS Discount,
    d.cost AS TotalCost
FROM mt_sale_mst s
INNER JOIN mt_sale_dtl d
    ON s.inv_cd = d.inv_cd
INNER JOIN mt_itm_mst i
    ON d.itm_cd = i.itm_cd
INNER JOIN mt_trader_mst t
    ON s.trd_cd = t.trd_cd
ORDER BY s.inv_dt DESC;
GO


/* LOW STOCK REPORT */

SELECT
    i.itm_cd,
    i.itm_desc,
    i.itm_moq AS MinimumOrderQty,

    ISNULL(p.PurchaseQty, 0)
    - ISNULL(s.SaleQty, 0) AS CurrentStock

FROM mt_itm_mst i

LEFT JOIN
(
    SELECT
        itm_cd,
        SUM(rcvg_qty) AS PurchaseQty
    FROM mt_pur_dtl
    GROUP BY itm_cd
) p
ON i.itm_cd = p.itm_cd

LEFT JOIN
(
    SELECT
        itm_cd,
        SUM(rcvg_qty) AS SaleQty
    FROM mt_sale_dtl
    GROUP BY itm_cd
) s
ON i.itm_cd = s.itm_cd

WHERE
    ISNULL(p.PurchaseQty, 0)
    - ISNULL(s.SaleQty, 0)
    <= i.itm_moq;
GO


/* TOTAL PURCHASE AMOUNT */

SELECT
    SUM(cost) AS TotalPurchaseAmount
FROM mt_pur_dtl;
GO


/* TOTAL SALES AMOUNT */

SELECT
    SUM(cost) AS TotalSalesAmount
FROM mt_sale_dtl;
GO


/* PROFIT AMOUNT */

SELECT
    SUM(
        (d.rate - d.pur_rate) * d.rcvg_qty
    ) AS TotalProfit
FROM mt_sale_dtl d;
GO


/* DASHBOARD QUERY CONNECTING TO .NET */

SELECT
    (SELECT COUNT(*) FROM mt_itm_mst) AS TotalItems,

    (SELECT COUNT(*)
     FROM mt_trader_mst
     WHERE trd_type = 'S') AS TotalSuppliers,

    (SELECT COUNT(*)
     FROM mt_trader_mst
     WHERE trd_type = 'C') AS TotalCustomers,

    (SELECT ISNULL(SUM(cost),0)
     FROM mt_pur_dtl) AS TotalPurchase,

    (SELECT ISNULL(SUM(cost),0)
     FROM mt_sale_dtl) AS TotalSales,

    (
        SELECT ISNULL(SUM((rate - pur_rate) * rcvg_qty),0)
        FROM mt_sale_dtl
    ) AS TotalProfit;
GO
