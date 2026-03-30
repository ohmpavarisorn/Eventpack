-- ============================================================
-- EventPack Database Migration - Custom Menu System
-- ============================================================
-- This script adds support for custom menu selections instead of package-based bookings

-- 1. Create Staff table
CREATE TABLE Staff (
    StaffID NVARCHAR(10) PRIMARY KEY,
    FullName NVARCHAR(50) NOT NULL,
    Role NVARCHAR(20), -- 'Chef', 'Sous Chef', 'Staff'
    Phone VARCHAR(15)
);

-- 2. Create MenuItems table
CREATE TABLE MenuItems (
    MenuItemID NVARCHAR(10) PRIMARY KEY,
    ItemName NVARCHAR(100) NOT NULL,
    Category NVARCHAR(50), -- 'Appetizer', 'Main', 'Side', 'Dessert'
    BasePrice DECIMAL(10, 2) NOT NULL
);

-- 3. Create StaffAssignments table (ใครไปงานไหน)
CREATE TABLE StaffAssignments (
    AssignmentID INT IDENTITY(1,1) PRIMARY KEY,
    BookingID NVARCHAR(10) NOT NULL,
    StaffID NVARCHAR(10) NOT NULL,
    AssignmentDate DATETIME,
    FOREIGN KEY (BookingID) REFERENCES Bookings(BookingID),
    FOREIGN KEY (StaffID) REFERENCES Staff(StaffID)
);

-- 4. Create BookingMenuItems table (junction table for many-to-many relationship)
CREATE TABLE BookingMenuItems (
    BookingMenuItemID INT IDENTITY(1,1) PRIMARY KEY,
    BookingID NVARCHAR(10) NOT NULL,
    MenuItemID NVARCHAR(10) NOT NULL,
    Quantity INT NOT NULL DEFAULT 1,
    UnitPrice DECIMAL(10, 2) NOT NULL,
    TotalPrice DECIMAL(10, 2) NOT NULL,
    FOREIGN KEY (BookingID) REFERENCES Bookings(BookingID),
    FOREIGN KEY (MenuItemID) REFERENCES MenuItems(MenuItemID)
);

-- ============================================================
-- Insert Menu Items
-- ============================================================

-- Appetizer & Salad (8 items)
INSERT INTO MenuItems VALUES ('APP01', 'ซาซิมิแซลมอนนอร์เวย์สด (Premium Grade)', 'Appetizer', 350.00);
INSERT INTO MenuItems VALUES ('APP02', 'ทูน่าอาคามิซาซิมิ (Akami)', 'Appetizer', 300.00);
INSERT INTO MenuItems VALUES ('APP03', 'ยำสาหร่ายญี่ปุ่นไข่กุ้งสด', 'Appetizer', 120.00);
INSERT INTO MenuItems VALUES ('APP04', 'สลัดอโวคาโดและควินัวซอสเลมอน (Healthy)', 'Appetizer', 220.00);
INSERT INTO MenuItems VALUES ('APP05', 'สลัดปูอัดไข่กุ้งมายองเนสญี่ปุ่น', 'Appetizer', 180.00);
INSERT INTO MenuItems VALUES ('APP06', 'ทาทากิเนื้อวากิวเบิร์นไฟซอสพอนสึ', 'Appetizer', 450.00);
INSERT INTO MenuItems VALUES ('APP07', 'หอยเชลล์ฮอกไกโดย่างเนยกระเทียม', 'Appetizer', 380.00);
INSERT INTO MenuItems VALUES ('APP08', 'กุ้งเทมปุระทอดกรอบซอสเทนสึยุ', 'Appetizer', 280.00);

-- Main Course (10 items)
INSERT INTO MenuItems VALUES ('MAIN01', 'สเต็กเนื้อวากิว A4 หั่นเต๋าซอสไวน์แดง', 'Main', 1200.00);
INSERT INTO MenuItems VALUES ('MAIN02', 'ปลาแซลมอนย่างหนังกรอบซอสเทริยากิ', 'Main', 450.00);
INSERT INTO MenuItems VALUES ('MAIN03', 'ปลาหิมะนึ่งซีอิ๊วขิงสด', 'Main', 850.00);
INSERT INTO MenuItems VALUES ('MAIN04', 'พาสต้าเส้นสดทรัฟเฟิลออยล์และเห็ดแชมปิญอง', 'Main', 420.00);
INSERT INTO MenuItems VALUES ('MAIN05', 'ข้าวหน้าเนื้อวากิวไข่ดองโชยุ', 'Main', 550.00);
INSERT INTO MenuItems VALUES ('MAIN06', 'อกไก่ซูวีสมุนไพรซอสเพสโต้ (Healthy)', 'Main', 320.00);
INSERT INTO MenuItems VALUES ('MAIN07', 'ซูชิหน้าปลาไหลญี่ปุ่นย่างซีอิ๊ว (Unagi)', 'Main', 250.00);
INSERT INTO MenuItems VALUES ('MAIN08', 'แฮมเบิร์กเนื้อวากิวสไตล์ญี่ปุ่น', 'Main', 380.00);
INSERT INTO MenuItems VALUES ('MAIN09', 'กุ้งลายเสือย่างเกลือหิมาลัย', 'Main', 550.00);
INSERT INTO MenuItems VALUES ('MAIN10', 'สปาเก็ตตี้ไข่ปลาเมนไทโกะ', 'Main', 350.00);

-- Side Dish & Soup (4 items)
INSERT INTO MenuItems VALUES ('SIDE01', 'ซุปมิโซะขาวเต้าหู้อ่อนและสาหร่ายวากาเมะ', 'Side', 80.00);
INSERT INTO MenuItems VALUES ('SIDE02', 'ซุปครีมเห็ดทรัฟเฟิลเข้มข้น', 'Side', 180.00);
INSERT INTO MenuItems VALUES ('SIDE03', 'มันบดเนยฝรั่งเศสเนื้อเนียน', 'Side', 120.00);
INSERT INTO MenuItems VALUES ('SIDE04', 'ผักย่างรวมตามฤดูกาล', 'Side', 150.00);

-- Dessert (3 items)
INSERT INTO MenuItems VALUES ('DES01', 'พานาคอตต้าซอสสตรอว์เบอร์รี่สด', 'Dessert', 160.00);
INSERT INTO MenuItems VALUES ('DES02', 'ไอศกรีมมัทฉะพรีเมียมและถั่วแดงกวน', 'Dessert', 140.00);
INSERT INTO MenuItems VALUES ('DES03', 'ผลไม้สดพรีเมียมตามฤดูกาล', 'Dessert', 200.00);

-- ============================================================
-- Insert Sample Staff (optional)
-- ============================================================
INSERT INTO Staff VALUES ('STF001', 'เชฟสมชาย', 'Chef', '08X-XXX-XXXX');
INSERT INTO Staff VALUES ('STF002', 'พ่อครัววิชิต', 'Sous Chef', '08X-XXX-XXXX');
INSERT INTO Staff VALUES ('STF003', 'ผู้ช่วยสมศรี', 'Staff', '08X-XXX-XXXX');

-- ============================================================
-- Optional: Make PackageID nullable in Bookings table if not already
-- ============================================================
-- IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='bookings' AND COLUMN_NAME='PackageID' AND IS_NULLABLE='YES')
-- ALTER TABLE Bookings ALTER COLUMN PackageID NVARCHAR(10) NULL;

-- ============================================================
-- Verify the new tables were created
-- ============================================================
-- SELECT * FROM MenuItems;
-- SELECT * FROM Staff;
-- SELECT * FROM StaffAssignments;
-- SELECT * FROM BookingMenuItems;
