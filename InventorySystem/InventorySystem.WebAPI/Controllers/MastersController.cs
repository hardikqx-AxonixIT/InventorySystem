using InventorySystem.Application.Interfaces;
using InventorySystem.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace InventorySystem.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MastersController : ControllerBase
    {
        private readonly IApplicationDbContext _context;

        public MastersController(IApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("bootstrap")]
        public async Task<IActionResult> GetBootstrap(CancellationToken cancellationToken)
        {
            var result = new
            {
                categories = await _context.ItemCategories.OrderByDescending(x => x.IsActive).ThenBy(x => x.Name).ToListAsync(cancellationToken),
                units = await _context.UnitsOfMeasure.OrderByDescending(x => x.IsActive).ThenBy(x => x.Name).ToListAsync(cancellationToken),
                products = await _context.Products.IgnoreQueryFilters().OrderByDescending(x => x.IsActive).ThenByDescending(x => x.Id).ToListAsync(cancellationToken),
                customers = await _context.Customers.OrderByDescending(x => x.IsActive).ThenByDescending(x => x.Id).ToListAsync(cancellationToken),
                vendors = await _context.Vendors.OrderByDescending(x => x.IsActive).ThenByDescending(x => x.Id).ToListAsync(cancellationToken),
                warehouses = await _context.Warehouses.OrderByDescending(x => x.IsActive).ThenByDescending(x => x.Id).ToListAsync(cancellationToken),
                bins = await _context.WarehouseBins.OrderByDescending(x => x.IsActive).ThenByDescending(x => x.Id).ToListAsync(cancellationToken)
            };

            return Ok(result);
        }

        [HttpPost("categories")]
        public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryRequest request, CancellationToken cancellationToken)
        {
            var category = new ItemCategory
            {
                Name = request.Name.Trim(),
                Description = request.Description?.Trim()
            };

            _context.ItemCategories.Add(category);
            await _context.SaveChangesAsync(cancellationToken);
            return Ok(category);
        }

        [HttpPut("categories/{id:int}")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] CreateCategoryRequest request, CancellationToken cancellationToken)
        {
            var category = await _context.ItemCategories.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            if (category == null) return NotFound("Category not found.");

            category.Name = request.Name.Trim();
            category.Description = request.Description?.Trim();
            await _context.SaveChangesAsync(cancellationToken);
            return Ok(category);
        }

        [HttpPost("categories/{id:int}/status")]
        public async Task<IActionResult> SetCategoryStatus(int id, [FromBody] UpdateStatusRequest request, CancellationToken cancellationToken)
        {
            var category = await _context.ItemCategories.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            if (category == null) return NotFound("Category not found.");
            category.IsActive = request.IsActive;
            await _context.SaveChangesAsync(cancellationToken);
            return Ok(category);
        }

        [HttpPost("units")]
        public async Task<IActionResult> CreateUnit([FromBody] CreateUnitRequest request, CancellationToken cancellationToken)
        {
            var unit = new UnitOfMeasure
            {
                Code = request.Code.Trim().ToUpperInvariant(),
                Name = request.Name.Trim(),
                Symbol = request.Symbol?.Trim()
            };

            _context.UnitsOfMeasure.Add(unit);
            await _context.SaveChangesAsync(cancellationToken);
            return Ok(unit);
        }

        [HttpPut("units/{id:int}")]
        public async Task<IActionResult> UpdateUnit(int id, [FromBody] CreateUnitRequest request, CancellationToken cancellationToken)
        {
            var unit = await _context.UnitsOfMeasure.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            if (unit == null) return NotFound("Unit not found.");

            unit.Code = request.Code.Trim().ToUpperInvariant();
            unit.Name = request.Name.Trim();
            unit.Symbol = request.Symbol?.Trim();
            await _context.SaveChangesAsync(cancellationToken);
            return Ok(unit);
        }

        [HttpPost("units/{id:int}/status")]
        public async Task<IActionResult> SetUnitStatus(int id, [FromBody] UpdateStatusRequest request, CancellationToken cancellationToken)
        {
            var unit = await _context.UnitsOfMeasure.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            if (unit == null) return NotFound("Unit not found.");
            unit.IsActive = request.IsActive;
            await _context.SaveChangesAsync(cancellationToken);
            return Ok(unit);
        }

        [HttpPost("products")]
        public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequest request, CancellationToken cancellationToken)
        {
            var product = new Product
            {
                SKU = request.Sku.Trim(),
                Name = request.Name.Trim(),
                Barcode = request.Barcode?.Trim(),
                HsnCode = request.HsnCode?.Trim(),
                Brand = request.Brand?.Trim(),
                CategoryId = request.CategoryId,
                UOMId = request.UomId,
                ReorderLevel = request.ReorderLevel,
                PurchasePrice = request.PurchasePrice,
                SalesPrice = request.SalesPrice,
                GstRate = request.GstRate,
                TrackBatch = request.TrackBatch,
                TrackSerial = request.TrackSerial,
                TrackExpiry = request.TrackExpiry
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync(cancellationToken);
            return Ok(product);
        }

        [HttpPut("products/{id:int}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] CreateProductRequest request, CancellationToken cancellationToken)
        {
            var product = await _context.Products.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            if (product == null) return NotFound("Product not found.");

            product.SKU = request.Sku.Trim();
            product.Name = request.Name.Trim();
            product.Barcode = request.Barcode?.Trim();
            product.HsnCode = request.HsnCode?.Trim();
            product.Brand = request.Brand?.Trim();
            product.CategoryId = request.CategoryId;
            product.UOMId = request.UomId;
            product.ReorderLevel = request.ReorderLevel;
            product.PurchasePrice = request.PurchasePrice;
            product.SalesPrice = request.SalesPrice;
            product.GstRate = request.GstRate;
            product.TrackBatch = request.TrackBatch;
            product.TrackSerial = request.TrackSerial;
            product.TrackExpiry = request.TrackExpiry;
            await _context.SaveChangesAsync(cancellationToken);
            return Ok(product);
        }

        [HttpPost("products/{id:int}/status")]
        public async Task<IActionResult> SetProductStatus(int id, [FromBody] UpdateStatusRequest request, CancellationToken cancellationToken)
        {
            var product = await _context.Products.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            if (product == null) return NotFound("Product not found.");

            product.IsActive = request.IsActive;
            product.IsDeleted = !request.IsActive;
            await _context.SaveChangesAsync(cancellationToken);
            return Ok(product);
        }

        [HttpPost("customers")]
        public async Task<IActionResult> CreateCustomer([FromBody] CreateCustomerRequest request, CancellationToken cancellationToken)
        {
            var customer = new Customer
            {
                Name = request.Name.Trim(),
                Gstin = request.Gstin?.Trim(),
                ContactPerson = request.ContactPerson?.Trim(),
                Phone = request.Phone?.Trim(),
                Email = request.Email?.Trim(),
                BillingAddress = request.BillingAddress?.Trim(),
                ShippingAddress = request.ShippingAddress?.Trim(),
                PaymentTermsDays = request.PaymentTermsDays,
                CreditLimit = request.CreditLimit
            };

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync(cancellationToken);
            return Ok(customer);
        }

        [HttpPut("customers/{id:int}")]
        public async Task<IActionResult> UpdateCustomer(int id, [FromBody] CreateCustomerRequest request, CancellationToken cancellationToken)
        {
            var customer = await _context.Customers.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            if (customer == null) return NotFound("Customer not found.");

            customer.Name = request.Name.Trim();
            customer.Gstin = request.Gstin?.Trim();
            customer.ContactPerson = request.ContactPerson?.Trim();
            customer.Phone = request.Phone?.Trim();
            customer.Email = request.Email?.Trim();
            customer.BillingAddress = request.BillingAddress?.Trim();
            customer.ShippingAddress = request.ShippingAddress?.Trim();
            customer.PaymentTermsDays = request.PaymentTermsDays;
            customer.CreditLimit = request.CreditLimit;
            await _context.SaveChangesAsync(cancellationToken);
            return Ok(customer);
        }

        [HttpPost("customers/{id:int}/status")]
        public async Task<IActionResult> SetCustomerStatus(int id, [FromBody] UpdateStatusRequest request, CancellationToken cancellationToken)
        {
            var customer = await _context.Customers.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            if (customer == null) return NotFound("Customer not found.");
            customer.IsActive = request.IsActive;
            await _context.SaveChangesAsync(cancellationToken);
            return Ok(customer);
        }

        [HttpPost("vendors")]
        public async Task<IActionResult> CreateVendor([FromBody] CreateVendorRequest request, CancellationToken cancellationToken)
        {
            var vendor = new Vendor
            {
                Name = request.Name.Trim(),
                Gstin = request.Gstin?.Trim(),
                ContactPerson = request.ContactPerson?.Trim(),
                Phone = request.Phone?.Trim(),
                Email = request.Email?.Trim(),
                Address = request.Address?.Trim(),
                PaymentTermsDays = request.PaymentTermsDays
            };

            _context.Vendors.Add(vendor);
            await _context.SaveChangesAsync(cancellationToken);
            return Ok(vendor);
        }

        [HttpPut("vendors/{id:int}")]
        public async Task<IActionResult> UpdateVendor(int id, [FromBody] CreateVendorRequest request, CancellationToken cancellationToken)
        {
            var vendor = await _context.Vendors.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            if (vendor == null) return NotFound("Vendor not found.");

            vendor.Name = request.Name.Trim();
            vendor.Gstin = request.Gstin?.Trim();
            vendor.ContactPerson = request.ContactPerson?.Trim();
            vendor.Phone = request.Phone?.Trim();
            vendor.Email = request.Email?.Trim();
            vendor.Address = request.Address?.Trim();
            vendor.PaymentTermsDays = request.PaymentTermsDays;
            await _context.SaveChangesAsync(cancellationToken);
            return Ok(vendor);
        }

        [HttpPost("vendors/{id:int}/status")]
        public async Task<IActionResult> SetVendorStatus(int id, [FromBody] UpdateStatusRequest request, CancellationToken cancellationToken)
        {
            var vendor = await _context.Vendors.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            if (vendor == null) return NotFound("Vendor not found.");
            vendor.IsActive = request.IsActive;
            await _context.SaveChangesAsync(cancellationToken);
            return Ok(vendor);
        }

        [HttpPost("warehouses")]
        public async Task<IActionResult> CreateWarehouse([FromBody] CreateWarehouseRequest request, CancellationToken cancellationToken)
        {
            var warehouse = new Warehouse
            {
                Name = request.Name.Trim(),
                Code = request.Code?.Trim(),
                AddressLine1 = request.AddressLine1?.Trim(),
                AddressLine2 = request.AddressLine2?.Trim(),
                City = request.City?.Trim(),
                State = request.State?.Trim(),
                PostalCode = request.PostalCode?.Trim()
            };

            _context.Warehouses.Add(warehouse);
            await _context.SaveChangesAsync(cancellationToken);
            return Ok(warehouse);
        }

        [HttpPut("warehouses/{id:int}")]
        public async Task<IActionResult> UpdateWarehouse(int id, [FromBody] CreateWarehouseRequest request, CancellationToken cancellationToken)
        {
            var warehouse = await _context.Warehouses.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            if (warehouse == null) return NotFound("Warehouse not found.");

            warehouse.Name = request.Name.Trim();
            warehouse.Code = request.Code?.Trim();
            warehouse.AddressLine1 = request.AddressLine1?.Trim();
            warehouse.AddressLine2 = request.AddressLine2?.Trim();
            warehouse.City = request.City?.Trim();
            warehouse.State = request.State?.Trim();
            warehouse.PostalCode = request.PostalCode?.Trim();
            await _context.SaveChangesAsync(cancellationToken);
            return Ok(warehouse);
        }

        [HttpPost("warehouses/{id:int}/status")]
        public async Task<IActionResult> SetWarehouseStatus(int id, [FromBody] UpdateStatusRequest request, CancellationToken cancellationToken)
        {
            var warehouse = await _context.Warehouses.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            if (warehouse == null) return NotFound("Warehouse not found.");
            warehouse.IsActive = request.IsActive;
            await _context.SaveChangesAsync(cancellationToken);
            return Ok(warehouse);
        }

        [HttpPost("bins")]
        public async Task<IActionResult> CreateBin([FromBody] CreateWarehouseBinRequest request, CancellationToken cancellationToken)
        {
            var warehouse = await _context.Warehouses.FirstOrDefaultAsync(x => x.Id == request.WarehouseId, cancellationToken);
            if (warehouse == null)
            {
                return BadRequest("Warehouse not found.");
            }

            var bin = new WarehouseBin
            {
                WarehouseId = warehouse.Id,
                WarehouseName = warehouse.Name,
                Zone = request.Zone?.Trim(),
                Aisle = request.Aisle?.Trim(),
                Shelf = request.Shelf?.Trim(),
                BinCode = request.BinCode.Trim()
            };

            _context.WarehouseBins.Add(bin);
            await _context.SaveChangesAsync(cancellationToken);
            return Ok(bin);
        }

        [HttpPut("bins/{id:int}")]
        public async Task<IActionResult> UpdateBin(int id, [FromBody] CreateWarehouseBinRequest request, CancellationToken cancellationToken)
        {
            var bin = await _context.WarehouseBins.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            if (bin == null) return NotFound("Bin not found.");

            var warehouse = await _context.Warehouses.FirstOrDefaultAsync(x => x.Id == request.WarehouseId, cancellationToken);
            if (warehouse == null)
            {
                return BadRequest("Warehouse not found.");
            }

            bin.WarehouseId = warehouse.Id;
            bin.WarehouseName = warehouse.Name;
            bin.Zone = request.Zone?.Trim();
            bin.Aisle = request.Aisle?.Trim();
            bin.Shelf = request.Shelf?.Trim();
            bin.BinCode = request.BinCode.Trim();
            await _context.SaveChangesAsync(cancellationToken);
            return Ok(bin);
        }

        [HttpPost("bins/{id:int}/status")]
        public async Task<IActionResult> SetBinStatus(int id, [FromBody] UpdateStatusRequest request, CancellationToken cancellationToken)
        {
            var bin = await _context.WarehouseBins.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            if (bin == null) return NotFound("Bin not found.");
            bin.IsActive = request.IsActive;
            await _context.SaveChangesAsync(cancellationToken);
            return Ok(bin);
        }
    }

    public class CreateCategoryRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    public class CreateUnitRequest
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Symbol { get; set; }
    }

    public class CreateProductRequest
    {
        public string Sku { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Barcode { get; set; }
        public string? HsnCode { get; set; }
        public string? Brand { get; set; }
        public int CategoryId { get; set; }
        public int UomId { get; set; }
        public decimal ReorderLevel { get; set; }
        public decimal PurchasePrice { get; set; }
        public decimal SalesPrice { get; set; }
        public decimal GstRate { get; set; }
        public bool TrackBatch { get; set; }
        public bool TrackSerial { get; set; }
        public bool TrackExpiry { get; set; }
    }

    public class CreateCustomerRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Gstin { get; set; }
        public string? ContactPerson { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? BillingAddress { get; set; }
        public string? ShippingAddress { get; set; }
        public int PaymentTermsDays { get; set; }
        public decimal CreditLimit { get; set; }
    }

    public class CreateVendorRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Gstin { get; set; }
        public string? ContactPerson { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public int PaymentTermsDays { get; set; }
    }

    public class CreateWarehouseRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Code { get; set; }
        public string? AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? PostalCode { get; set; }
    }

    public class CreateWarehouseBinRequest
    {
        public int WarehouseId { get; set; }
        public string BinCode { get; set; } = string.Empty;
        public string? Zone { get; set; }
        public string? Aisle { get; set; }
        public string? Shelf { get; set; }
    }

    public class UpdateStatusRequest
    {
        public bool IsActive { get; set; }
    }
}


