using InventorySystem.Domain.Entities;
using InventorySystem.Domain.Enums;
using InventorySystem.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace InventorySystem.Infrastructure.Data
{
    public static class DemoDataSeeder
    {
        public static async Task SeedAsync(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            await SeedRolesAsync(roleManager);
            await SeedUsersAsync(userManager);
            await SeedMastersAndInventoryAsync(context);
            await SeedAccountingAndPermissionsAsync(context);
        }

        private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            var roles = new[]
            {
                "SuperAdmin",
                "InventoryManager",
                "WarehouseStaff",
                "SalesManager",
                "AccountsManager"
            };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }

        private static async Task SeedUsersAsync(UserManager<ApplicationUser> userManager)
        {
            const string email = "admin@axonix.local";
            const string password = "Admin@123";

            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    FullName = "Axonix Demo Admin",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(user, password);
                if (!result.Succeeded)
                {
                    return;
                }
            }

            var roles = new[] { "SuperAdmin", "InventoryManager", "WarehouseStaff" };
            foreach (var role in roles)
            {
                if (!await userManager.IsInRoleAsync(user, role))
                {
                    await userManager.AddToRoleAsync(user, role);
                }
            }
        }

        private static async Task SeedMastersAndInventoryAsync(ApplicationDbContext context)
        {
            if (!await context.ItemCategories.AnyAsync())
            {
                context.ItemCategories.AddRange(
                    new ItemCategory { Name = "Medicines", Description = "Pharma and healthcare items" },
                    new ItemCategory { Name = "Electronics", Description = "Devices and accessories" },
                    new ItemCategory { Name = "FMCG", Description = "Fast moving consumer goods" });
            }

            if (!await context.UnitsOfMeasure.AnyAsync())
            {
                context.UnitsOfMeasure.AddRange(
                    new UnitOfMeasure { Code = "PCS", Name = "Pieces", Symbol = "pcs" },
                    new UnitOfMeasure { Code = "BOX", Name = "Box", Symbol = "box" },
                    new UnitOfMeasure { Code = "KG", Name = "Kilogram", Symbol = "kg" });
            }

            if (!await context.Warehouses.AnyAsync())
            {
                context.Warehouses.AddRange(
                    new Warehouse { Name = "Axonix Private Limited", Code = "WH-MAIN", AddressLine1 = "101, Business Park", City = "Ahmedabad", State = "Gujarat", PostalCode = "380015", Gstin = "24ABCDE1234F1Z5", Pan = "ABCDE1234F", BankName = "ICICI Bank", BankAccountNo = "1234567890", IfscCode = "ICIC0001234", Phone = "079-12345678" },
                    new Warehouse { Name = "Surat Distribution Hub", Code = "WH-DISP", AddressLine1 = "Gate 4, GIDV", City = "Surat", State = "Gujarat", PostalCode = "395007", Gstin = "24ABCDE1234F1Z5", Pan = "ABCDE1234F" });
            }

            if (!await context.Customers.AnyAsync())
            {
                context.Customers.AddRange(
                    new Customer
                    {
                        Name = "Shree Medical Stores",
                        Gstin = "24ABCDE1234F1Z5",
                        Pan = "ABCDE1234F",
                        ContactPerson = "Kiran Patel",
                        Phone = "9876543210",
                        PaymentTermsDays = 14,
                        CreditLimit = 150000,
                        BillingAddress = "CG Road",
                        BillingCity = "Ahmedabad",
                        BillingState = "Gujarat",
                        BillingPostalCode = "380009",
                        ShippingAddress = "CG Road, Ahmedabad"
                    },
                    new Customer
                    {
                        Name = "Metro Retail LLP",
                        Gstin = "24ABCDE9876M1Z2",
                        Pan = "ABCDE9876M",
                        ContactPerson = "Nilesh Shah",
                        Phone = "9988776655",
                        PaymentTermsDays = 21,
                        CreditLimit = 250000,
                        BillingAddress = "Ring Road",
                        BillingCity = "Surat",
                        BillingState = "Gujarat",
                        BillingPostalCode = "395001",
                        ShippingAddress = "Ring Road, Surat"
                    });
            }

            if (!await context.Vendors.AnyAsync())
            {
                context.Vendors.AddRange(
                    new Vendor
                    {
                        Name = "Sunrise Pharma Distributors",
                        Gstin = "24AAACD4567J1Z1",
                        Pan = "AAACD4567J",
                        ContactPerson = "Rahul Mehta",
                        Phone = "9012345678",
                        PaymentTermsDays = 30,
                        Address = "Naroda",
                        City = "Ahmedabad",
                        State = "Gujarat",
                        PostalCode = "382330"
                    },
                    new Vendor
                    {
                        Name = "Prime Electronics Supply",
                        Gstin = "24AAACE1111A1Z9",
                        Pan = "AAACE1111A",
                        ContactPerson = "Mitali Desai",
                        Phone = "9090909090",
                        PaymentTermsDays = 21,
                        Address = "Vesu",
                        City = "Surat",
                        State = "Gujarat",
                        PostalCode = "395007"
                    });
            }

            await context.SaveChangesAsync();

            if (!await context.WarehouseBins.AnyAsync())
            {
                var warehouses = await context.Warehouses.OrderBy(w => w.Id).ToListAsync();
                context.WarehouseBins.AddRange(
                    new WarehouseBin { WarehouseId = warehouses[0].Id, WarehouseName = warehouses[0].Name, Zone = "A", Aisle = "01", Shelf = "Top", BinCode = "A-01-T" },
                    new WarehouseBin { WarehouseId = warehouses[0].Id, WarehouseName = warehouses[0].Name, Zone = "B", Aisle = "02", Shelf = "Mid", BinCode = "B-02-M" },
                    new WarehouseBin { WarehouseId = warehouses[1].Id, WarehouseName = warehouses[1].Name, Zone = "D", Aisle = "05", Shelf = "Low", BinCode = "D-05-L" });
                await context.SaveChangesAsync();
            }

            if (!await context.Products.AnyAsync())
            {
                var medicines = await context.ItemCategories.FirstAsync(c => c.Name == "Medicines");
                var electronics = await context.ItemCategories.FirstAsync(c => c.Name == "Electronics");
                var pcs = await context.UnitsOfMeasure.FirstAsync(u => u.Code == "PCS");
                var box = await context.UnitsOfMeasure.FirstAsync(u => u.Code == "BOX");

                context.Products.AddRange(
                    new Product
                    {
                        SKU = "MED-PARA-500",
                        Name = "Paracetamol 500mg",
                        Barcode = "890100000001",
                        HsnCode = "3004",
                        Brand = "HealthPlus",
                        CategoryId = medicines.Id,
                        UOMId = box.Id,
                        ReorderLevel = 25,
                        PurchasePrice = 18,
                        SalesPrice = 24,
                        GstRate = 12,
                        TrackBatch = true,
                        TrackExpiry = true
                    },
                    new Product
                    {
                        SKU = "ELE-USB-C-65",
                        Name = "USB-C Charger 65W",
                        Barcode = "890100000002",
                        HsnCode = "8504",
                        Brand = "AxonPower",
                        CategoryId = electronics.Id,
                        UOMId = pcs.Id,
                        ReorderLevel = 15,
                        PurchasePrice = 780,
                        SalesPrice = 1099,
                        GstRate = 18
                    },
                    new Product
                    {
                        SKU = "MED-VITC-001",
                        Name = "Vitamin C Tablets",
                        Barcode = "890100000003",
                        HsnCode = "2106",
                        Brand = "NutriLife",
                        CategoryId = medicines.Id,
                        UOMId = box.Id,
                        ReorderLevel = 20,
                        PurchasePrice = 95,
                        SalesPrice = 135,
                        GstRate = 12,
                        TrackBatch = true,
                        TrackExpiry = true
                    });

                await context.SaveChangesAsync();
            }

            if (!await context.StockLevels.AnyAsync())
            {
                var mainBin = await context.WarehouseBins.FirstAsync();
                var secondBin = await context.WarehouseBins.OrderBy(b => b.Id).Skip(1).FirstAsync();
                var products = await context.Products.OrderBy(p => p.Id).ToListAsync();

                context.StockLevels.AddRange(
                    new StockLevel { ProductId = products[0].Id, BinId = mainBin.Id, QuantityOnHand = 40, ReservedQuantity = 5 },
                    new StockLevel { ProductId = products[1].Id, BinId = secondBin.Id, QuantityOnHand = 10, ReservedQuantity = 2 },
                    new StockLevel { ProductId = products[2].Id, BinId = mainBin.Id, QuantityOnHand = 18, ReservedQuantity = 1 });

                context.StockLedgers.AddRange(
                    new StockLedger { ProductId = products[0].Id, ChangeAmount = 40, PostChangeBalance = 40, ReasonCode = "OPENING", ReferenceDocumentId = "OPEN-001" },
                    new StockLedger { ProductId = products[1].Id, ChangeAmount = 10, PostChangeBalance = 10, ReasonCode = "OPENING", ReferenceDocumentId = "OPEN-002" },
                    new StockLedger { ProductId = products[2].Id, ChangeAmount = 18, PostChangeBalance = 18, ReasonCode = "OPENING", ReferenceDocumentId = "OPEN-003" });

                context.AdjustmentRequests.Add(new AdjustmentRequest
                {
                    ProductId = products[1].Id,
                    RequestedAmount = 3,
                    Reason = "Cycle count correction after barcode scan",
                    RequestedBy = "warehouse.staff@axonix.local",
                    Status = AdjustmentStatus.Pending
                });

                await context.SaveChangesAsync();
            }

            if (!await context.StockBatchDetails.AnyAsync())
            {
                var levels = await context.StockLevels.ToListAsync();
                foreach (var sl in levels)
                {
                    var product = await context.Products.FirstAsync(p => p.Id == sl.ProductId);
                    context.StockBatchDetails.Add(new StockBatchDetail
                    {
                        ProductId = sl.ProductId,
                        BinId = sl.BinId,
                        BatchNumber = $"OPEN-{sl.ProductId}-{sl.BinId}",
                        Quantity = sl.QuantityOnHand,
                        UnitCost = product.PurchasePrice,
                        ExpiryDate = product.TrackExpiry ? DateTime.UtcNow.AddMonths(12) : null,
                        IsActive = true
                    });
                }

                await context.SaveChangesAsync();
            }

            if (!await context.PurchaseOrders.AnyAsync())
            {
                var vendor = await context.Vendors.OrderBy(v => v.Id).FirstAsync();
                var warehouse = await context.Warehouses.OrderBy(w => w.Id).FirstAsync();
                var product = await context.Products.OrderBy(p => p.Id).FirstAsync();

                var purchaseOrder = new PurchaseOrder
                {
                    OrderNumber = "PO-DEMO-001",
                    VendorId = vendor.Id,
                    WarehouseId = warehouse.Id,
                    SupplyState = warehouse.State,
                    Status = DocumentStatus.Open,
                    Subtotal = 360,
                    CgstAmount = 21.60m,
                    SgstAmount = 21.60m,
                    IgstAmount = 0,
                    TaxTotal = 43.20m,
                    GrandTotal = 403.20m,
                    Items =
                    {
                        new PurchaseOrderItem
                        {
                            ProductId = product.Id,
                            Quantity = 20,
                            ReceivedQuantity = 0,
                            UnitPrice = 18,
                            GstRate = 12,
                            TaxableAmount = 360,
                            CgstAmount = 21.60m,
                            SgstAmount = 21.60m,
                            IgstAmount = 0,
                            LineTotal = 403.20m
                        }
                    }
                };

                context.PurchaseOrders.Add(purchaseOrder);
                await context.SaveChangesAsync();
            }

            if (!await context.SalesOrders.AnyAsync())
            {
                var customer = await context.Customers.OrderBy(c => c.Id).FirstAsync();
                var warehouse = await context.Warehouses.OrderBy(w => w.Id).FirstAsync();
                var stockLine = await context.StockLevels.OrderBy(s => s.ProductId).FirstAsync();
                var product = await context.Products.FirstAsync(p => p.Id == stockLine.ProductId);

                stockLine.ReservedQuantity += 2;

                var salesOrder = new SalesOrder
                {
                    OrderNumber = "SO-DEMO-001",
                    CustomerId = customer.Id,
                    WarehouseId = warehouse.Id,
                    PlaceOfSupplyState = warehouse.State,
                    Status = DocumentStatus.Open,
                    Subtotal = 48,
                    CgstAmount = 2.88m,
                    SgstAmount = 2.88m,
                    IgstAmount = 0,
                    TaxTotal = 5.76m,
                    GrandTotal = 53.76m,
                    Items =
                    {
                        new SalesOrderItem
                        {
                            ProductId = product.Id,
                            BinId = stockLine.BinId,
                            Quantity = 2,
                            InvoicedQuantity = 0,
                            UnitPrice = 24,
                            GstRate = 12,
                            TaxableAmount = 48,
                            CgstAmount = 2.88m,
                            SgstAmount = 2.88m,
                            IgstAmount = 0,
                            LineTotal = 53.76m
                        }
                    }
                };

                context.SalesOrders.Add(salesOrder);
                await context.SaveChangesAsync();
            }
        }

        private static async Task SeedAccountingAndPermissionsAsync(ApplicationDbContext context)
        {
            if (!await context.AccountLedgers.AnyAsync())
            {
                context.AccountLedgers.AddRange(
                    new AccountLedger { Code = "1100", Name = "Cash-in-Hand", GroupType = "Asset", IsSystem = true },
                    new AccountLedger { Code = "1200", Name = "Bank", GroupType = "Asset", IsSystem = true },
                    new AccountLedger { Code = "1300", Name = "Accounts Receivable", GroupType = "Asset", IsSystem = true },
                    new AccountLedger { Code = "2100", Name = "Accounts Payable", GroupType = "Liability", IsSystem = true },
                    new AccountLedger { Code = "2200", Name = "GST Payable", GroupType = "Liability", IsSystem = true },
                    new AccountLedger { Code = "4100", Name = "Sales Revenue", GroupType = "Income", IsSystem = true },
                    new AccountLedger { Code = "5100", Name = "Purchase Expense", GroupType = "Expense", IsSystem = true });
            }

            if (!await context.RolePermissions.AnyAsync())
            {
                var roles = new[] { "SuperAdmin", "InventoryManager", "WarehouseStaff", "SalesManager", "AccountsManager" };
                var modules = new[] { "masters", "purchase", "sales", "gst", "inventory", "warehouse", "manufacturing", "accounting", "reports", "users" };
                foreach (var role in roles)
                {
                    foreach (var module in modules)
                    {
                        context.RolePermissions.Add(new RolePermission
                        {
                            RoleName = role,
                            ModuleKey = module,
                            CanView = true,
                            CanCreate = role != "WarehouseStaff" || module is "inventory" or "warehouse",
                            CanUpdate = role != "WarehouseStaff" || module is "inventory" or "warehouse",
                            CanDelete = role == "SuperAdmin",
                            CanApprove = role is "SuperAdmin" or "InventoryManager" or "AccountsManager",
                            CanExport = role != "WarehouseStaff"
                        });
                    }
                }
            }

            await context.SaveChangesAsync();
        }
    }
}
