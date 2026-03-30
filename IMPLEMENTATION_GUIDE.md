## EventPack Custom Menu System - Implementation Guide

### ✅ Completed Changes

#### 1. **Database Models Created**
- ✅ `MenuItem.cs` - Menu items with categories and prices
- ✅ `Staff.cs` - Staff management
- ✅ `StaffAssignment.cs` - Assign staff to bookings
- ✅ `BookingMenuItem.cs` - Junction table linking bookings to menu items

#### 2. **Database Context Updated**
- ✅ Added DbSets for all new models
- ✅ Configured relationships and foreign keys
- ✅ Set up proper column mappings for MySQL compatibility

#### 3. **ViewModel Updated**
- ✅ Removed `PackageId` from `BookingViewModel`
- ✅ Added `SelectedMenuItems` dictionary for menu selections
- ✅ Updated `TotalPrice` calculation

#### 4. **Controller Updated**
- ✅ Modified `BookingController.Create()` GET - Now loads menu items grouped by category
- ✅ Modified `BookingController.Create()` POST - Handles menu item selection and calculates total price
- ✅ Added API endpoint `GetMenuItemsByCategory()` for dynamic menu loading
- ✅ Added API endpoint `CalculatePrice()` for real-time price calculations
- ✅ Updated `Confirmation()` to display selected menu items
- ✅ Updated `MyBookings()` to show menu items

#### 5. **Views Updated**
- ✅ **Create.cshtml** - Complete redesign with:
  - Menu items grouped by category
  - Quantity selectors (+/- buttons)
  - Real-time price calculation
  - Guest information and contact details
  - Dynamic price summary
  
- ✅ **Confirmation.cshtml** - Updated to show:
  - Selected menu items with quantities
  - Individual item prices
  - Category information
  - Total price breakdown
  
- ✅ **MyBookings.cshtml** - Updated to show:
  - Menu items in compact badge format
  - Menu item quantities
  - Removed package references

---

### 📋 Required Next Steps

#### Step 1: Execute Database Migration
Run the SQL script provided: `Database_Migration_CustomMenu.sql`

This script creates:
- `Staff` table
- `MenuItems` table (with all 25 menu items)
- `StaffAssignments` table  
- `BookingMenuItems` table

```sql
-- Execute in SQL Server Management Studio or your database tools
-- File: Database_Migration_CustomMenu.sql
```

#### Step 2: Update Database Schema in Code
Update the Bookings table if needed to ensure PackageID is nullable:

```csharp
// In BookingViewModel model - already updated
public string? PackageId { get; set; }  // Now nullable
```

#### Step 3: Rebuild and Run EF Core Migrations
If using EF Core Code-First approach:

```bash
cd your-project-directory
dotnet ef migrations add AddCustomMenuSystem
dotnet ef database update
```

#### Step 4: Test the System

1. **Navigate to Booking/Create**
   - Should see menu items grouped by category
   - Test quantity selector buttons
   - Verify price calculations

2. **Submit a booking**
   - Should create booking record
   - Should create BookingMenuItem records for each selected item
   - Total price should calculate correctly

3. **View Confirmation**
   - Should display all selected menu items
   - Should show quantities and prices
   - Should show correct total

4. **Check MyBookings**
   - Should display menu items in badge format
   - Should show quantities

---

### 🔧 File Locations

#### Models Created:
- `Models/Db/MenuItem.cs`
- `Models/Db/Staff.cs`
- `Models/Db/StaffAssignment.cs`
- `Models/Db/BookingMenuItem.cs`

#### Files Modified:
- `Models/Db/Booking.cs` - Added navigation properties
- `Models/Db/Csi402projectContext.cs` - Added DbSets and configurations
- `ViewModels/BookingViewModel.cs` - Updated for menu selection
- `Controllers/BookingController.cs` - Completely rewritten for menu system
- `Views/Booking/Create.cshtml` - Redesigned with menu interface
- `Views/Booking/Confirmation.cshtml` - Updated to show menu items
- `Views/Booking/MyBookings.cshtml` - Updated to show menu items

#### SQL Script:
- `Database_Migration_CustomMenu.sql` - Database migration script

---

### 📝 Key Features Implemented

#### 1. **Menu Customization**
- Customers can select from 25 menu items across 4 categories
- Quantity selectors for each item
- Items organized by category:
  - Appetizer (8 items)
  - Main Course (10 items)
  - Side Dish & Soup (4 items)
  - Dessert (3 items)

#### 2. **Real-time Price Calculation**
- Price updates dynamically as quantities change
- Shows menu price and total price
- Displays guest count for reference

#### 3. **Booking Confirmation**
- Shows all selected menu items with quantities
- Displays unit price and total price per item
- Shows category for each menu item

#### 4. **Booking Management**
- MyBookings displays menu items in compact format
- Shows quantities and categories
- Maintains all booking information

#### 5. **Database Structure**
- Proper relationship between Bookings and MenuItems
- Stores quantity and prices at time of booking
- Staff management ready for future implementation

---

### ⚙️ Technical Details

#### Request Format for Menu Selection
Menu items are submitted via HTML form with format:
```
menu_qty_APP01=2
menu_qty_MAIN01=1
menu_qty_DES01=1
```

#### Database Relationships
```
Booking (1) -----> (Many) BookingMenuItem -----> (1) MenuItem
            \
             \---> (Many) StaffAssignment -----> (1) Staff
```

#### Price Calculation Flow
1. Form submitted with selected items and quantities
2. Controller loops through selected items
3. Retrieves each MenuItem from database for current prices
4. Calculates total: ∑(MenuItemPrice × Quantity)
5. Stores price at booking time in BookingMenuItem

---

### 🚀 Optional Enhancements (For Future)

1. **Staff Assignment Interface**
   - Admin dashboard to assign staff to bookings
   - Track staff workload

2. **Menu Management**
   - Admin interface to add/edit menu items
   - Update prices

3. **Booking Details Page**
   - View full booking details in detail page
   - Edit bookings before confirmation

4. **Payment Integration**
   - Link to payment system
   - Payment status tracking

5. **Email Notifications**
   - Send booking confirmation emails
   - Include menu details

---

### ❌ Breaking Changes Removed

- ❌ Package-based pricing (replaced with menu-based)
- ❌ Fixed package selections (now fully customizable)
- ❌ Simple price calculation (now itemized)

Existing bookings with packages will still load, but new bookings will use the menu system.

---

### 📞 Support

If you encounter any issues:

1. **Database connection errors**
   - Verify connection string in appsettings.json
   - Ensure SQL script was executed successfully

2. **Migration errors**
   - Clear any previous migrations if using EF Core
   - Run `dotnet ef migrations add` and `dotnet ef database update`

3. **Form submission errors**
   - Check browser console for JavaScript errors
   - Verify menu items are loading from database
   - Check controller logs for validation errors

---

**Created**: March 31, 2026
**Version**: 1.0
**Status**: Ready for Testing
