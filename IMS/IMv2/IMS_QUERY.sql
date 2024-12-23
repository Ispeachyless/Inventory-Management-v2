DROP DATABASE IMS

CREATE DATABASE IMS

USE IMS

---------------------------------------------------------------------------- Employee Query
CREATE TABLE tblEmployee(
	employeeID bigint IDENTITY(1,1) primary key,
	fName varchar(50) default '',
	lName varchar(50) default '',
	phoneNumber varchar(12) default '',
)
CREATE TABLE tblLogin(
	employeeID bigint,
	username varchar(50) NOT NULL,
	password varchar(50) NOT NULL,
	usertype varchar(10) NOT NULL,
	privilege varchar(10),
	foreign key (employeeID) references tblEmployee(employeeID) on update cascade
)
---------------------------------------------------------------------------- Employee Login
INSERT INTO tblEmployee (fName, lName, phoneNumber) VALUES ('Farrah', 'Saguan', '09090909090')
INSERT INTO tblLogin (employeeID,username, password, usertype, privilege) VALUES (1, 'admin', '1234', 'admin', 'Accept')
INSERT INTO tblEmployee (fName, lName, phoneNumber) VALUES ('Rojel', 'Caasi', '09090909090')
INSERT INTO tblLogin (employeeID,username, password, usertype, privilege) VALUES (2, 'rr', '1234', 'employee', 'Accept')
INSERT INTO tblEmployee (fName, lName, phoneNumber) VALUES ('Dine', 'Cawayan', '09090909090')
INSERT INTO tblLogin (employeeID,username, password, usertype, privilege) VALUES (3, 'dine', '1234', 'employee','Accept')
---------------------------------------------------------------------------- Customer Query
CREATE TABLE tblCustomer(
	customerID bigint IDENTITY(1001,1) primary key ,
	fName varchar(50) default '',
	lName varchar(50) default '',
	phoneNumber varchar(12) default '',
)
---------------------------------------------------------------------------- Customer Info
INSERT INTO tblCustomer (fName, lName, phoneNumber) VALUES ('Ralph', 'Agang', '09090909090')
INSERT INTO tblCustomer (fName, lName, phoneNumber) VALUES ('Nicole', 'Premacio', '09090909090')
-------------------------------------------------------------------------- Sales Query
CREATE TABLE tblSales(
	orderID bigint primary key,
	employeeID bigint,
	invoiceNo bigint IDENTITY(5001, 1),
	iStatus varchar(50),
	invoiceDate date,
	customerID bigint,
	dueDate date
	foreign key (employeeID) references tblEmployee(employeeID) on update cascade,
	foreign key (customerID) references tblCustomer(customerID) on update cascade
)
---------------------------------------------------------------------------- Sales Info
INSERT INTO tblSales(orderID,employeeID,iStatus,invoiceDate,customerID,dueDate) VALUES (7001,1, 'Order Placed', GETDATE(), 1001, '2024-11-01')
INSERT INTO tblSales(orderID,employeeID,iStatus,invoiceDate,customerID,dueDate) VALUES (7002,2, 'Order Placed', GETDATE(), 1002, '2024-11-02')
---------------------------------------------------------------------------- Item Query
CREATE TABLE tblItem (
   ProductID bigint IDENTITY(2001,1) primary key,
   ProductName varchar(50) not null,
   UnitPrice Decimal(10, 2) not null,
   Description varchar(100), 
   ProductionPoint Bigint, 
)
---------------------------------------------------------------------------- Item Info
INSERT INTO tblitem (ProductName, UnitPrice, Description, ProductionPoint) VALUES ('HAY', '450', 'Lamp with Swing Arm', '10')
INSERT INTO tblitem (ProductName, UnitPrice, Description, ProductionPoint) VALUES ('FLOS', '450', 'Skynest Led Ceiling Lamp', '10')
INSERT INTO tblitem (ProductName, UnitPrice, Description, ProductionPoint) VALUES ('TOM DIXON', '450', 'Mini Led Glass Wall Lamp', '12')
INSERT INTO tblitem (ProductName, UnitPrice, Description, ProductionPoint) VALUES ('FORSCARINI', '450', 'Ball Glass Wall Lamp', '10')
INSERT INTO tblitem (ProductName, UnitPrice, Description, ProductionPoint) VALUES ('STILNOVO', '450', 'Adjustable Aluminium Wall Lamp', '10')
INSERT INTO tblitem (ProductName, UnitPrice, Description, ProductionPoint) VALUES ('VIBIA', '450', 'Led Aluminium Wall Lamp', '12')
INSERT INTO tblitem (ProductName, UnitPrice, Description, ProductionPoint) VALUES ('KARTELL', '450', 'Polycarbonate Wall Lamp', '12')
---------------------------------------------------------------------------- Production Query
CREATE TABLE tblProduction (
  ProductionID BIGINT IDENTITY(3001, 1) PRIMARY KEY,
  ArrivalDate DATETIME NOT NULL
)
---------------------------------------------------------------------------- Prouduction Info
INSERT INTO tblProduction(ArrivalDate) VALUES ('04/04/2024')
INSERT INTO tblProduction(ArrivalDate) VALUES ('04/05/2024')
INSERT INTO tblProduction(ArrivalDate) VALUES ('04/06/2024')
INSERT INTO tblProduction(ArrivalDate) VALUES ('04/07/2024')
INSERT INTO tblProduction(ArrivalDate) VALUES ('04/08/2024')
INSERT INTO tblProduction(ArrivalDate) VALUES ('04/09/2024')
---------------------------------------------------------------------------- ProductionDetails Query
CREATE TABLE tblProductionDetails (
  PDetailsID BIGINT IDENTITY(4001, 1) PRIMARY KEY,
  ProductID BIGINT NOT NULL,
  ProductionID BIGINT NOT NULL,
  Quantity BIGINT NOT NULL,
)
---------------------------------------------------------------------------- ProductionDetails Info
INSERT INTO tblProductionDetails (ProductID, ProductionID,Quantity) VALUES ('2001', '3001', '50')
INSERT INTO tblProductionDetails (ProductID, ProductionID,Quantity) VALUES ('2002', '3002', '50')
INSERT INTO tblProductionDetails (ProductID, ProductionID,Quantity) VALUES ('2001', '3003', '50')
INSERT INTO tblProductionDetails (ProductID, ProductionID,Quantity) VALUES ('2002', '3004', '50')
INSERT INTO tblProductionDetails (ProductID, ProductionID,Quantity) VALUES ('2003', '3005', '50')
INSERT INTO tblProductionDetails (ProductID, ProductionID,Quantity) VALUES ('2004', '3006', '50')

---------------------------------------------------------------------------- SalesDetails Query
CREATE TABLE tblSalesDetails(
	invoiceNo bigint,
	ProductID bigint NOT NULL,
	quantity int NOT NULL,
	pDetailsID bigint,
)
---------------------------------------------------------------------------- SalesDetails Info
INSERT INTO tblSalesDetails (invoiceNo, ProductID, quantity) VALUES (5001,2001, 3)
INSERT INTO tblSalesDetails (invoiceNo, ProductID, quantity) VALUES (5002,2001, 6)

---------------------------------------------------------------------------- Audit Log Query
CREATE TABLE tblAuditLog (
    LogID INT PRIMARY KEY IDENTITY,
    Timestamp DATETIME DEFAULT GETDATE(),
    Action NVARCHAR(255) NOT NULL,
    employeeID bigint
)
DBCC CHECKIDENT ('tblAuditLog', RESEED, 1);
---------------------------------------------------------------------------- Completed Order Info
CREATE TABLE tblCompleteOrders(
	invoiceNo bigint,
	ProductID bigint,
	UnitPrice Decimal(10, 2)
)
-------------------------------------------------------------------------- SalesReturn Info
CREATE TABLE tblSalesReturn (
    ReturnID BIGINT IDENTITY(8000, 1) PRIMARY KEY,
    employeeID BIGINT,
    invoiceNo BIGINT,
    ProductID BIGINT,
    RStatus NVARCHAR(255) NOT NULL,
	Reason NVARCHAR(255),
    RequestDate DATETIME DEFAULT GETDATE(),
    orderID BIGINT, 
    customerID BIGINT, 
    FOREIGN KEY (employeeID) REFERENCES tblEmployee(employeeID) ON UPDATE CASCADE,
    FOREIGN KEY (orderID) REFERENCES tblSales(orderID) ON UPDATE NO ACTION, 
    FOREIGN KEY (ProductID) REFERENCES tblItem(ProductID)  ON UPDATE NO ACTION,  
    FOREIGN KEY (customerID) REFERENCES tblCustomer(customerID) ON UPDATE CASCADE, 
)
-------------------------------------------------------------------------- 
-- FOR CODE PURPOSES QUERY
SELECT * FROM tblCompleteOrders
Select COALESCE(MAX(employeeID), '0') From tblLogin
SELECT * FROM tblLogin
SELECT * FROM tblEmployee
SELECT * FROM tblCustomer
SELECT s.orderID, s.employeeID, s.invoiceNo, s.iStatus, s.invoiceDate, c.fName +' '+ c.lName, s.dueDate FROM tblSales s 
INNER JOIN tblCustomer c on s.customerID = c.customerID order by s.dueDate
SELECT * FROM tblItem
SELECT SUM(Quantity)FROM tblProductionDetails GROUP BY ProductID ORDER BY ProductID
SELECT ProductionID, Quantity, ProductID FROM tblProductionDetails WHERE ProductID = 2001
SELECT * FROM tblAuditLog
SELECT s.orderID, s.employeeID, s.invoiceNo, s.iStatus, s.invoiceDate, c.fName +' '+ c.lName, s.dueDate FROM tblSales s INNER JOIN tblCustomer c on s.customerID = c.customerID WHERE employeeID = 2 order by s.dueDate 
SELECT * FROM tblProductionDetails
SELECT * FROM tblSales
DELETE FROM tblSales WHERE invoiceNo = 5004
SELECT invoiceNo, ProductID, quantity, COALESCE(pDetailsID,0) as PDetails FROM tblSalesDetails WHERE invoiceNo = 5001 and ProductID = 2001
UPDATE tblSalesDetails SET pDetailsID = NULL WHERE invoiceNo = 5001 AND ProductID = 2001
SELECT invoiceNo, ProductID, PDetailsID FROM tblSalesDetails WHERE invoiceNo = 5001
SELECT SUM(quantity) FROM tblProductionDetails WHERE ProductID = 2001
SELECT SUM(quantity) FROM tblSalesDetails WHERE ProductID = 2001 AND invoiceNo = 5001 GROUP BY ProductID
SELECT COUNT(invoiceNo) FROM tblSalesDetails WHERE ProductID = 2001 AND invoiceNo = 5001
SELECT invoiceNo, ProductID, quantity, COALESCE(pDetailsID,0) as PDetails FROM tblSalesDetails WHERE invoiceNo = 5001
SELECT invoiceNo FROM tblSales
SELECT s.invoiceNo, s.quantity AS SalesQty, p.ProductID, SUM(p.Quantity) AS ProductionQty, s.ProductID, s.PDetailsID FROM tblSalesDetails s INNER JOIN tblProductionDetails p 
ON p.ProductID = s.ProductID WHERE invoiceNo = 5001
SELECT s.invoiceNo, s.quantity AS SalesQty, p.ProductID, SUM(p.Quantity) AS TotalProductionQty, s.ProductID, s.PDetailsID
FROM tblSalesDetails s INNER JOIN tblProductionDetails p ON p.ProductID = s.ProductID WHERE s.invoiceNo = 5003
GROUP BY s.invoiceNo, s.quantity, p.ProductID, s.ProductID, s.PDetailsID
SELECT ProductionID, Quantity FROM tblProductionDetails WHERE ProductID = 2001
UPDATE tblSalesDetails SET quantity = 0 WHERE invoiceNo = 5003 AND ProductID = 2001
SELECT s.invoiceNo, s.quantity AS SalesQty, p.ProductID, SUM(p.Quantity) AS TotalProductionQty, s.ProductID, s.PDetailsID 
FROM tblSalesDetails s INNER JOIN tblProductionDetails p ON p.ProductID = s.ProductID WHERE s.invoiceNo = 5039 
GROUP BY s.invoiceNo, s.quantity, p.ProductID, s.ProductID, s.PDetailsID
SELECT * FROM tblSalesDetails ORDER BY invoiceNo
SELECT COUNT(*) FROM tblSales
SELECT COUNT(*) FROM tblCustomer
SELECT COUNT(*) FROM tblItem
SELECT * FROM tblCompleteOrders

SELECT i.ProductName AS ItemName, SUM(COALESCE(pd.Quantity, 0)) AS InventoryIn, SUM(COALESCE(sd.Quantity, 0)) AS InventoryOut
FROM tblItem i LEFT JOIN tblProductionDetails pd ON i.ProductID = pd.ProductID LEFT JOIN tblSalesDetails sd ON i.ProductID = sd.ProductID
INNER JOIN tblSales s ON sd.invoiceNo = s.invoiceNo WHERE s.iStatus = 'Delivery Complete' GROUP BY i.ProductName

SELECT sd.invoiceNo, sd.ProductID, i.UnitPrice FROM tblSalesDetails sd INNER JOIN tblItem i ON sd.ProductID = i.ProductID
-- p
WITH result AS (SELECT ss.invoiceNo, i.UnitPrice * ss.quantity AS totalPrice FROM tblItem i
INNER JOIN tblSalesDetails ss ON i.ProductID = ss.ProductID) SELECT * FROM (SELECT result.invoiceNo,
SUM(totalPrice) AS totalPrice FROM result GROUP BY invoiceNo) totalPriceTbl 
INNER JOIN (SELECT c.FName, c.LName, s.invoiceDate, s.invoiceNo FROM tblCustomer c
INNER JOIN tblSales s ON c.customerID = s.customerID) namesTbl ON totalPriceTbl.invoiceNo = namesTbl.invoiceNo 
ORDER BY totalPriceTbl.invoiceNo

With Updater as (SELECT pDetailsID, SUM(quantity) FROM (Select pDetailsID, -(quantity) from tblSalesDetails
where invoiceNo = 5001 UNION SELECT PDetailsID, Quantity FROM tblProductionDetails) updatedTbl GROUP BY PDetailsID)
Update tblProductionDetails set Quantity = (SELECT quantity FROM Updater 
WHERE Updater.PDetailsID = tblProductionDetails.PDetailsID)

SELECT * FROM tblSalesDetails ORDER BY invoiceNo
With Updater as (SELECT PDetailsID, SUM(Qty) AS Qty FROM (Select PDetailsID, -(quantity) AS Qty from tblSalesDetails
where invoiceNo = 5002 UNION SELECT ProductionID, Quantity FROM tblProductionDetails) updatedTbl GROUP BY PDetailsID)
Update tblProductionDetails set Quantity = 
(SELECT Qty FROM Updater WHERE Updater.PDetailsID = tblProductionDetails.ProductionID)

SELECT i.ProductName AS ItemName, SUM(COALESCE(pd.Quantity, 0)) AS InventoryIn, SUM(COALESCE(sd.Quantity, 0)) AS InventoryOut
FROM tblItem i INNER JOIN tblProductionDetails pd ON i.ProductID = pd.ProductID LEFT JOIN 
tblSalesDetails sd ON i.ProductID = sd.ProductID LEFT JOIN tblSales s ON sd.invoiceNo = s.invoiceNo 
WHERE s.iStatus = 'Delivery Complete' GROUP BY i.ProductName

SELECT * FROM tblAuditLog
SELECT DISTINCT ProductionID from tblProductionDetails
SELECT p.ProductionID, i.ProductName, p.ProductID FROM tblItem i INNER JOIN tblProductionDetails p ON p.ProductID = i.ProductID 
WHERE ProductionID = 3007
SELECT * FROM tblCompleteOrders

WITH CompletedOrders AS (SELECT s.invoiceNo, co.UnitPrice, ss.quantity FROM tblSales s INNER JOIN tblSalesDetails ss ON s.invoiceNo = ss.invoiceNo
INNER JOIN tblItem i ON ss.ProductID = i.ProductID INNER JOIN tblCompleteOrders co ON s.invoiceNo = co.invoiceNo AND i.ProductID = co.ProductID
WHERE s.iStatus = 'Delivery Complete'),TotalPriceByInvoice AS (SELECT invoiceNo, SUM(UnitPrice * quantity) AS totalPrice FROM CompletedOrders
GROUP BY invoiceNo) SELECT c.FName + ' ' + c.LName AS FullName, s.invoiceDate, s.invoiceNo, t.totalPrice FROM tblCustomer c 
INNER JOIN tblSales s ON c.customerID = s.customerID
INNER JOIN TotalPriceByInvoice t ON s.invoiceNo = t.invoiceNo ORDER BY s.invoiceNo

select i.ProductName, tbl.totalQty, i.ProductionPoint from(select SUM(p.Quantity) AS totalqty, ProductID
from tblProductionDetails p group by p.ProductID) tbl inner join tblItem i on 
i.ProductID = tbl.ProductID order by tbl.totalqty
SELECT * FROM tblItem
SELECT * FROM tblProductionDetails

WITH CompletedOrders AS (
    SELECT s.invoiceNo, co.UnitPrice, ss.quantity 
    FROM tblSales s 
    INNER JOIN tblSalesDetails ss ON s.invoiceNo = ss.invoiceNo
    INNER JOIN tblItem i ON ss.ProductID = i.ProductID 
    INNER JOIN tblCompleteOrders co ON s.invoiceNo = co.invoiceNo AND i.ProductID = co.ProductID
    WHERE s.iStatus = 'Delivery Complete'
),
TotalPriceByInvoice AS (
    SELECT invoiceNo, SUM(UnitPrice * quantity) AS totalPrice 
    FROM CompletedOrders
    GROUP BY invoiceNo
)
SELECT s.invoiceDate, t.totalPrice 
FROM tblSales s
INNER JOIN TotalPriceByInvoice t ON s.invoiceNo = t.invoiceNo
ORDER BY s.invoiceDate;
SELECT * FROM tblSales
SELECT * FROM tblItem
       