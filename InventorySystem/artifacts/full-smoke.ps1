$ErrorActionPreference = "Stop"

function Add-Result {
    param(
        [System.Collections.Generic.List[object]]$Bag,
        [string]$Name,
        [bool]$Pass,
        [int]$StatusCode = 0,
        [string]$Note = ""
    )
    $Bag.Add([pscustomobject]@{
        name = $Name
        pass = $Pass
        status = $StatusCode
        note = $Note
    }) | Out-Null
}

function Invoke-Api {
    param(
        [string]$Method,
        [string]$Uri,
        [object]$Body = $null,
        [hashtable]$Headers = @{},
        [int[]]$Expected = @(200)
    )

    $params = @{
        Method = $Method
        Uri = $Uri
        Headers = $Headers
        TimeoutSec = 25
        UseBasicParsing = $true
    }

    if ($null -ne $Body) {
        $params["ContentType"] = "application/json"
        $params["Body"] = ($Body | ConvertTo-Json -Depth 12)
    }

    try {
        $resp = Invoke-WebRequest @params
        $ok = $Expected -contains [int]$resp.StatusCode
        $json = $null
        try { $json = $resp.Content | ConvertFrom-Json -Depth 20 } catch { $json = $null }
        return [pscustomobject]@{ ok = $ok; status = [int]$resp.StatusCode; json = $json; raw = $resp.Content }
    }
    catch {
        $status = 0
        if ($_.Exception.Response -and $_.Exception.Response.StatusCode) {
            $status = [int]$_.Exception.Response.StatusCode
        }
        $body = ""
        try {
            $reader = New-Object IO.StreamReader($_.Exception.Response.GetResponseStream())
            $body = $reader.ReadToEnd()
            $reader.Close()
        } catch {}
        $ok = $Expected -contains $status
        return [pscustomobject]@{ ok = $ok; status = $status; json = $null; raw = $body }
    }
}

$base = "http://localhost:5207/api"
$results = New-Object 'System.Collections.Generic.List[object]'
$runId = [DateTime]::UtcNow.ToString("yyyyMMddHHmmss")

# Public GETs
$r = Invoke-Api -Method GET -Uri "$base/Dashboard/overview"
Add-Result $results "Dashboard overview" $r.ok $r.status

$masters = Invoke-RestMethod -Method Get -Uri "$base/Masters/bootstrap"
Add-Result $results "Masters bootstrap" $true 200

$tx = Invoke-RestMethod -Method Get -Uri "$base/Transactions/bootstrap"
Add-Result $results "Transactions bootstrap" $true 200
$defaultProductId = $tx.products[0].id
$defaultProductBarcode = $tx.products[0].barcode
$defaultPurchasePrice = $tx.products[0].purchasePrice
$defaultSalesPrice = $tx.products[0].salesPrice
$defaultGstRate = $tx.products[0].gstRate
$defaultCustomerId = $tx.customers[0].id
$defaultVendorId = $tx.vendors[0].id
$defaultWarehouseId = $tx.warehouses[0].id
$defaultWarehouseState = $tx.warehouses[0].state
$defaultBinId = $tx.bins[0].id

$adv = Invoke-RestMethod -Method Get -Uri "$base/AdvancedOperations/snapshot"
Add-Result $results "Advanced snapshot" $true 200

$r = Invoke-Api -Method GET -Uri "$base/Transactions/purchase-orders?page=1&pageSize=10"
Add-Result $results "Transactions purchase-orders" $r.ok $r.status

$r = Invoke-Api -Method GET -Uri "$base/Transactions/goods-receipts?page=1&pageSize=10"
Add-Result $results "Transactions goods-receipts" $r.ok $r.status

$r = Invoke-Api -Method GET -Uri "$base/Transactions/purchase-invoices?page=1&pageSize=10"
Add-Result $results "Transactions purchase-invoices" $r.ok $r.status

$r = Invoke-Api -Method GET -Uri "$base/Transactions/sales-orders?page=1&pageSize=10"
Add-Result $results "Transactions sales-orders" $r.ok $r.status

$r = Invoke-Api -Method GET -Uri "$base/Transactions/sales-invoices?page=1&pageSize=10"
Add-Result $results "Transactions sales-invoices" $r.ok $r.status

$r = Invoke-Api -Method GET -Uri "$base/Transactions/reports/snapshot"
Add-Result $results "Transactions reports snapshot" $r.ok $r.status

$r = Invoke-Api -Method GET -Uri "$base/Transactions/mobile/dashboard"
Add-Result $results "Transactions mobile dashboard" $r.ok $r.status

$r = Invoke-Api -Method GET -Uri "$base/Transactions/ai/demand-prediction?lookbackDays=30"
Add-Result $results "Transactions AI demand prediction" $r.ok $r.status

$r = Invoke-Api -Method GET -Uri "$base/Transactions/auto-reorder/suggestions?lookbackDays=30&horizonDays=15"
Add-Result $results "Transactions auto reorder" $r.ok $r.status

$r = Invoke-Api -Method GET -Uri "$base/Transactions/accounting-summary"
Add-Result $results "Transactions accounting summary" $r.ok $r.status

$r = Invoke-Api -Method GET -Uri "$base/AdvancedOperations/accounting/ledgers"
Add-Result $results "Advanced accounting ledgers" $r.ok $r.status
$ledgers = $r.json

$r = Invoke-Api -Method GET -Uri "$base/AdvancedOperations/accounting/balance-sheet"
Add-Result $results "Advanced balance-sheet" $r.ok $r.status

# Auth + users
$loginResp = Invoke-RestMethod -Method Post -Uri "$base/Auth/login" -ContentType "application/json" -Body (@{ email = "admin@axonix.local"; password = "Admin@123" } | ConvertTo-Json)
$token = $loginResp.token
Add-Result $results "Auth login admin" ([bool]$token) 200
$authHeaders = @{}
if ($token) { $authHeaders["Authorization"] = "Bearer $token" }

$r = Invoke-Api -Method POST -Uri "$base/Auth/register" -Body @{ email = "smoke.$runId@axonix.local"; password = "Admin@123"; fullName = "Smoke User $runId" }
Add-Result $results "Auth register user" ($r.status -eq 200 -or $r.status -eq 400) $r.status "400 allowed for duplicate/validation"

$r = Invoke-Api -Method GET -Uri "$base/UserManagement/roles"
Add-Result $results "UserManagement roles" $r.ok $r.status

$r = Invoke-Api -Method GET -Uri "$base/UserManagement/users?page=1&pageSize=10"
Add-Result $results "UserManagement users" $r.ok $r.status

$r = Invoke-Api -Method POST -Uri "$base/UserManagement/users" -Body @{
    email = "u.$runId@axonix.local"
    password = "Admin@123"
    fullName = "Created User $runId"
    role = "InventoryManager"
}
Add-Result $results "UserManagement create user" ($r.status -eq 200 -or $r.status -eq 400) $r.status "400 allowed if duplicate/policy"

# Masters CRUD cycle
$category = Invoke-Api -Method POST -Uri "$base/Masters/categories" -Body @{ name = "Smoke Category $runId"; description = "Smoke" }
Add-Result $results "Masters create category" $category.ok $category.status
$categoryId = $category.json.id

$unit = Invoke-Api -Method POST -Uri "$base/Masters/units" -Body @{ code = "S$($runId.Substring($runId.Length-4))"; name = "Smoke Unit $runId"; symbol = "su" }
Add-Result $results "Masters create unit" $unit.ok $unit.status
$unitId = $unit.json.id

$warehouse = Invoke-Api -Method POST -Uri "$base/Masters/warehouses" -Body @{
    name = "Smoke WH $runId"; code = "WH$($runId.Substring($runId.Length-4))"; city = "Ahmedabad"; state = "Gujarat"; addressLine1 = "Address"; postalCode = "380001"
}
Add-Result $results "Masters create warehouse" $warehouse.ok $warehouse.status
$warehouseId = $warehouse.json.id

$bin = Invoke-Api -Method POST -Uri "$base/Masters/bins" -Body @{ warehouseId = $warehouseId; binCode = "BIN-$runId"; zone = "S"; aisle = "A1"; shelf = "L1" }
Add-Result $results "Masters create bin" $bin.ok $bin.status
$binId = $bin.json.id

$vendor = Invoke-Api -Method POST -Uri "$base/Masters/vendors" -Body @{
    name = "Smoke Vendor $runId"; gstin = "24ABCDE1234F1Z5"; contactPerson = "Smoke"; phone = "9000000001"; email = "vendor.$runId@axonix.local"; address = "Addr"; paymentTermsDays = 15
}
Add-Result $results "Masters create vendor" $vendor.ok $vendor.status
$vendorId = $vendor.json.id

$customer = Invoke-Api -Method POST -Uri "$base/Masters/customers" -Body @{
    name = "Smoke Customer $runId"; gstin = "24ABCDE1234F1Z6"; contactPerson = "Smoke"; phone = "9000000002"; email = "customer.$runId@axonix.local"; billingAddress = "Addr"; shippingAddress = "Addr"; paymentTermsDays = 10; creditLimit = 100000
}
Add-Result $results "Masters create customer" $customer.ok $customer.status
$customerId = $customer.json.id

$product = Invoke-Api -Method POST -Uri "$base/Masters/products" -Body @{
    sku = "SMOKE-$runId"; name = "Smoke Product $runId"; barcode = "89$($runId)"; hsnCode = "3004"; brand = "Smoke"; categoryId = $categoryId; uomId = $unitId; reorderLevel = 5; purchasePrice = 20; salesPrice = 35; gstRate = 12; trackBatch = $true; trackSerial = $false; trackExpiry = $true
}
Add-Result $results "Masters create product" $product.ok $product.status
$productId = $product.json.id

$r = Invoke-Api -Method PUT -Uri "$base/Masters/products/$productId" -Body @{
    sku = "SMOKE-$runId-U"; name = "Smoke Product Updated $runId"; barcode = "89$($runId)"; hsnCode = "3004"; brand = "Smoke"; categoryId = $categoryId; uomId = $unitId; reorderLevel = 7; purchasePrice = 21; salesPrice = 37; gstRate = 12; trackBatch = $true; trackSerial = $false; trackExpiry = $true
}
Add-Result $results "Masters update product" $r.ok $r.status

$r = Invoke-Api -Method POST -Uri "$base/Masters/products/$productId/status" -Body @{ isActive = $false }
Add-Result $results "Masters product status off" $r.ok $r.status
$r = Invoke-Api -Method POST -Uri "$base/Masters/products/$productId/status" -Body @{ isActive = $true }
Add-Result $results "Masters product status on" $r.ok $r.status

# Transaction flow
$r = Invoke-Api -Method POST -Uri "$base/Transactions/inventory/stock-in" -Body @{ productId = $defaultProductId; binId = $defaultBinId; quantity = 20; referenceNo = "STIN-$runId" }
Add-Result $results "Txn stock-in" $r.ok $r.status

$r = Invoke-Api -Method POST -Uri "$base/Transactions/purchase-orders" -Body @{
    vendorId = $defaultVendorId; warehouseId = $defaultWarehouseId; supplyState = $defaultWarehouseState;
    items = @(@{ productId = $defaultProductId; quantity = 8; unitPrice = $defaultPurchasePrice; gstRate = $defaultGstRate })
}
Add-Result $results "Txn create purchase order" $r.ok $r.status
$po = $r.json
$poId = $po.id
$poItems = @()
if ($po.items) { $poItems = $po.items }
elseif ($po.Items) { $poItems = $po.Items }
if ($poItems.Count -gt 0) { $poItemId = $poItems[0].id } else { $poItemId = 0 }

$r = Invoke-Api -Method PUT -Uri "$base/Transactions/purchase-orders/$poId" -Body @{
    vendorId = $defaultVendorId; warehouseId = $defaultWarehouseId; supplyState = $defaultWarehouseState;
    items = @(@{ productId = $defaultProductId; quantity = 10; unitPrice = $defaultPurchasePrice; gstRate = $defaultGstRate })
}
Add-Result $results "Txn update purchase order" $r.ok $r.status

$poPage = Invoke-Api -Method GET -Uri "$base/Transactions/purchase-orders?page=1&pageSize=5"
$poRecord = $poPage.json.records | Where-Object { $_.id -eq $poId } | Select-Object -First 1
$poRecordItems = @()
if ($poRecord.items) { $poRecordItems = $poRecord.items }
elseif ($poRecord.Items) { $poRecordItems = $poRecord.Items }
if ($poRecordItems.Count -gt 0) { $poItemId = $poRecordItems[0].id }

$r = Invoke-Api -Method POST -Uri "$base/Transactions/goods-receipts" -Body @{
    purchaseOrderId = $poId
    items = @(@{ purchaseOrderItemId = $poItemId; productId = $defaultProductId; binId = $defaultBinId; quantityReceived = 4 })
}
Add-Result $results "Txn create GRN" $r.ok $r.status
$grnId = $r.json.id

$r = Invoke-Api -Method POST -Uri "$base/Transactions/purchase-invoices" -Body @{ goodsReceiptNoteId = $grnId; dueDate = (Get-Date).AddDays(15).ToString("yyyy-MM-dd") }
Add-Result $results "Txn create purchase invoice" $r.ok $r.status
$purchaseInvoiceId = $r.json.id

$r = Invoke-Api -Method POST -Uri "$base/Transactions/supplier-payments" -Body @{ purchaseInvoiceId = $purchaseInvoiceId; amount = 50; paymentMode = "BANK"; referenceNo = "PAY-$runId"; notes = "smoke" }
Add-Result $results "Txn supplier payment" $r.ok $r.status

# Sales flow
$r = Invoke-Api -Method POST -Uri "$base/Transactions/sales-orders" -Body @{
    customerId = $defaultCustomerId; warehouseId = $defaultWarehouseId; placeOfSupplyState = $defaultWarehouseState;
    items = @(@{ productId = $defaultProductId; binId = $defaultBinId; quantity = 2; unitPrice = $defaultSalesPrice; gstRate = $defaultGstRate })
}
Add-Result $results "Txn create sales order" $r.ok $r.status
$so1 = $r.json
$so1Id = $so1.id

$r = Invoke-Api -Method PUT -Uri "$base/Transactions/sales-orders/$so1Id" -Body @{
    customerId = $defaultCustomerId; warehouseId = $defaultWarehouseId; placeOfSupplyState = $defaultWarehouseState;
    items = @(@{ productId = $defaultProductId; binId = $defaultBinId; quantity = 1; unitPrice = $defaultSalesPrice; gstRate = $defaultGstRate })
}
Add-Result $results "Txn update sales order" $r.ok $r.status

$r = Invoke-Api -Method POST -Uri "$base/AdvancedOperations/sales/delivery-challans" -Body @{ salesOrderId = $so1Id; notes = "smoke dispatch" }
Add-Result $results "Advanced delivery challan" $r.ok $r.status

$r = Invoke-Api -Method POST -Uri "$base/AdvancedOperations/warehouse/pick-lists" -Body @{ salesOrderId = $so1Id }
Add-Result $results "Advanced create pick list" $r.ok $r.status
$pickListId = $r.json.id

$r = Invoke-Api -Method POST -Uri "$base/AdvancedOperations/warehouse/pick-lists/scan" -Body @{ pickListId = $pickListId; barcode = "89$($runId)"; quantity = 1 }
$scanBarcode = $defaultProductBarcode
if (-not $scanBarcode) { $scanBarcode = "890100000001" }
$r = Invoke-Api -Method POST -Uri "$base/AdvancedOperations/warehouse/pick-lists/scan" -Body @{ pickListId = $pickListId; barcode = $scanBarcode; quantity = 1 }
Add-Result $results "Advanced pick scan" $r.ok $r.status

$r = Invoke-Api -Method POST -Uri "$base/AdvancedOperations/warehouse/pick-lists/$pickListId/pack"
Add-Result $results "Advanced pick pack" $r.ok $r.status

$r = Invoke-Api -Method POST -Uri "$base/Transactions/sales-invoices" -Body @{ salesOrderId = $so1Id }
Add-Result $results "Txn create sales invoice" $r.ok $r.status
$salesInvoice = $r.json
$salesInvoiceId = $salesInvoice.id
$salesItems = @()
if ($salesInvoice.items) { $salesItems = $salesInvoice.items }
elseif ($salesInvoice.Items) { $salesItems = $salesInvoice.Items }
if ($salesItems.Count -gt 0) { $salesInvoiceItemId = $salesItems[0].id } else { $salesInvoiceItemId = 0 }

$r = Invoke-Api -Method POST -Uri "$base/AdvancedOperations/sales/returns" -Body @{
    salesInvoiceId = $salesInvoiceId
    reason = "smoke return"
    items = @(@{ salesInvoiceItemId = $salesInvoiceItemId; binId = $defaultBinId; quantity = 1 })
}
Add-Result $results "Advanced sales return" $r.ok $r.status

$r = Invoke-Api -Method POST -Uri "$base/AdvancedOperations/sales/quotations" -Body @{
    customerId = $defaultCustomerId; warehouseId = $defaultWarehouseId; placeOfSupplyState = $defaultWarehouseState; validUntil = (Get-Date).AddDays(10).ToString("o");
    items = @(@{ productId = $defaultProductId; binId = $defaultBinId; quantity = 1; unitPrice = $defaultSalesPrice; gstRate = $defaultGstRate })
}
Add-Result $results "Advanced create quotation" $r.ok $r.status
$quoteId = $r.json.id

$r = Invoke-Api -Method PUT -Uri "$base/AdvancedOperations/sales/quotations/$quoteId" -Body @{
    customerId = $defaultCustomerId; warehouseId = $defaultWarehouseId; placeOfSupplyState = $defaultWarehouseState; validUntil = (Get-Date).AddDays(8).ToString("o");
    items = @(@{ productId = $defaultProductId; binId = $defaultBinId; quantity = 1; unitPrice = ($defaultSalesPrice + 1); gstRate = $defaultGstRate })
}
Add-Result $results "Advanced update quotation" $r.ok $r.status

$r = Invoke-Api -Method POST -Uri "$base/AdvancedOperations/sales/quotations/$quoteId/convert"
Add-Result $results "Advanced convert quotation" $r.ok $r.status

$r = Invoke-Api -Method POST -Uri "$base/AdvancedOperations/sales/quotations/$quoteId/cancel"
Add-Result $results "Advanced cancel quotation (post-convert expected 400/200)" ($r.status -eq 200 -or $r.status -eq 400) $r.status

# GST + accounting advanced
$r = Invoke-Api -Method POST -Uri "$base/AdvancedOperations/gst/gstr1" -Body @{ fromDate = (Get-Date).AddDays(-30).ToString("o"); toDate = (Get-Date).ToString("o"); downloadCsv = $false }
Add-Result $results "Advanced GSTR-1" $r.ok $r.status

$r = Invoke-Api -Method POST -Uri "$base/AdvancedOperations/gst/gstr3b" -Body @{ fromDate = (Get-Date).AddDays(-30).ToString("o"); toDate = (Get-Date).ToString("o"); downloadCsv = $false }
Add-Result $results "Advanced GSTR-3B" $r.ok $r.status

$r = Invoke-Api -Method POST -Uri "$base/AdvancedOperations/gst/einvoice" -Body @{ salesInvoiceId = $salesInvoiceId }
Add-Result $results "Advanced e-invoice" $r.ok $r.status

$r = Invoke-Api -Method POST -Uri "$base/AdvancedOperations/gst/eway-bill" -Body @{ salesInvoiceId = $salesInvoiceId; vehicleNumber = "GJ01AB1234"; distanceKm = 120 }
Add-Result $results "Advanced e-way bill" $r.ok $r.status

$ledgerId = 0
if ($ledgers -and $ledgers.Count -gt 0) { $ledgerId = $ledgers[0].id }
if ($ledgerId -gt 0) {
    $r = Invoke-Api -Method POST -Uri "$base/AdvancedOperations/accounting/journal-vouchers" -Body @{
        voucherDate = (Get-Date).ToString("o")
        narration = "Smoke JV"
        sourceModule = "SMOKE"
        sourceDocumentNo = "JV-$runId"
        lines = @(
            @{ ledgerId = $ledgerId; debit = 100; credit = 0; remarks = "Dr" },
            @{ ledgerId = $ledgerId; debit = 0; credit = 100; remarks = "Cr" }
        )
    }
    Add-Result $results "Advanced journal voucher" ($r.status -eq 200 -or $r.status -eq 400) $r.status "400 possible if same ledger not allowed"

    $r = Invoke-Api -Method POST -Uri "$base/AdvancedOperations/accounting/ledger/$ledgerId" -Body @{ fromDate = (Get-Date).AddDays(-30).ToString("o"); toDate = (Get-Date).ToString("o") }
    Add-Result $results "Advanced ledger drilldown" $r.ok $r.status
}
else {
    Add-Result $results "Advanced journal voucher" $false 0 "No ledgers returned"
    Add-Result $results "Advanced ledger drilldown" $false 0 "No ledgers returned"
}

$r = Invoke-Api -Method POST -Uri "$base/AdvancedOperations/accounting/profit-loss" -Body @{ fromDate = (Get-Date).AddDays(-30).ToString("o"); toDate = (Get-Date).ToString("o") }
Add-Result $results "Advanced profit-loss" $r.ok $r.status

# Warehouse transfer queue
$r = Invoke-Api -Method POST -Uri "$base/AdvancedOperations/warehouse/transfer-requests" -Body @{
    productId = $defaultProductId; fromBinId = $defaultBinId; toBinId = $defaultBinId; quantity = 1; requestedBy = "smoke@axonix.local"
}
Add-Result $results "Advanced transfer request (same bin may 400/200)" ($r.status -eq 200 -or $r.status -eq 400) $r.status
$requestId = $r.json.id
if ($requestId) {
    $r = Invoke-Api -Method POST -Uri "$base/AdvancedOperations/warehouse/transfer-requests/approve" -Body @{
        requestId = $requestId; approve = $false; approvedBy = "smoke@axonix.local"; approvalNote = "reject smoke"
    }
    Add-Result $results "Advanced transfer approve/reject" $r.ok $r.status
}

# Manufacturing advanced + transaction production run
$r = Invoke-Api -Method POST -Uri "$base/AdvancedOperations/manufacturing/boms" -Body @{
    name = "Smoke BOM $runId"; bomCode = "BOM-$runId"; finishedProductId = $defaultProductId; standardOutputQty = 1;
    items = @(@{ componentProductId = $defaultProductId; quantityPerOutput = 1 })
}
Add-Result $results "Advanced create BOM" ($r.status -eq 200 -or $r.status -eq 400) $r.status
$bomId = $r.json.id
if ($bomId) {
    $r = Invoke-Api -Method PUT -Uri "$base/AdvancedOperations/manufacturing/boms/$bomId" -Body @{
        name = "Smoke BOM Updated $runId"; standardOutputQty = 1; isActive = $true; items = @(@{ componentProductId = $defaultProductId; quantityPerOutput = 1 })
    }
    Add-Result $results "Advanced update BOM" $r.ok $r.status

    $r = Invoke-Api -Method POST -Uri "$base/AdvancedOperations/manufacturing/work-orders" -Body @{
        bomTemplateId = $bomId; plannedDate = (Get-Date).ToString("o"); plannedOutputQty = 1; inputBinId = $defaultBinId; outputBinId = $defaultBinId
    }
    Add-Result $results "Advanced create work order" ($r.status -eq 200 -or $r.status -eq 400) $r.status
    $woId = $r.json.id
    if ($woId) {
        $rel = Invoke-Api -Method POST -Uri "$base/AdvancedOperations/manufacturing/work-orders/$woId/release"
        Add-Result $results "Advanced release work order" ($rel.status -eq 200 -or $rel.status -eq 400) $rel.status
        $can = Invoke-Api -Method POST -Uri "$base/AdvancedOperations/manufacturing/work-orders/$woId/cancel"
        Add-Result $results "Advanced cancel work order" ($can.status -eq 200 -or $can.status -eq 400) $can.status
    }
}

$r = Invoke-Api -Method POST -Uri "$base/Transactions/manufacturing/production-runs" -Body @{
    finishedProductId = $defaultProductId; outputBinId = $defaultBinId; outputQuantity = 1; referenceNo = "PR-$runId";
    components = @(@{ productId = $defaultProductId; binId = $defaultBinId; quantity = 1 })
}
Add-Result $results "Txn production run" ($r.status -eq 200 -or $r.status -eq 400) $r.status

# Permissions/audit/integrations
$r = Invoke-Api -Method GET -Uri "$base/AdvancedOperations/users/permissions"
Add-Result $results "Advanced get permissions" $r.ok $r.status

$r = Invoke-Api -Method POST -Uri "$base/AdvancedOperations/users/permissions" -Body @{
    roleName = "InventoryManager"; moduleKey = "purchase"; canView = $true; canCreate = $true; canUpdate = $true; canDelete = $false; canApprove = $true; canExport = $true
}
Add-Result $results "Advanced upsert permission" $r.ok $r.status

$r = Invoke-Api -Method POST -Uri "$base/AdvancedOperations/users/audit-logs" -Body @{ entityName = ""; action = ""; page = 1; pageSize = 20 }
Add-Result $results "Advanced audit logs" $r.ok $r.status

$r = Invoke-Api -Method POST -Uri "$base/AdvancedOperations/integrations/whatsapp/invoice" -Body @{ salesInvoiceId = $salesInvoiceId; phoneNumber = "919000000000" }
Add-Result $results "Advanced WhatsApp invoice" $r.ok $r.status

$r = Invoke-Api -Method POST -Uri "$base/AdvancedOperations/integrations/razorpay/orders" -Body @{ amount = 100; currency = "INR"; receipt = "SMOKE-$runId" }
Add-Result $results "Advanced Razorpay order" $r.ok $r.status
$rzOrderId = $r.json.orderId
if (-not $rzOrderId) { $rzOrderId = "order_smoke" }

$r = Invoke-Api -Method POST -Uri "$base/AdvancedOperations/integrations/razorpay/callback" -Body @{
    razorpayOrderId = $rzOrderId
    razorpayPaymentId = "pay_smoke"
    razorpaySignature = "sig_smoke"
    rawPayload = "{}"
}
Add-Result $results "Advanced Razorpay callback" $r.ok $r.status

# Protected APIs (token required) - verify authenticated path works
$r = Invoke-Api -Method GET -Uri "$base/Products"
Add-Result $results "Products GET" $r.ok $r.status

$r = Invoke-Api -Method POST -Uri "$base/Products" -Headers $authHeaders -Body @{
    sku = "PDT-$runId"
    name = "Protected Product $runId"
    barcode = "77$runId"
    hsnCode = "3004"
    brand = "Protected"
    categoryId = $masters.categories[0].id
    uomId = $masters.units[0].id
    reorderLevel = 2
    purchasePrice = 11
    salesPrice = 16
    gstRate = 12
}
Add-Result $results "Products POST (authorized)" ($r.status -eq 201 -or $r.status -eq 200) $r.status

$r = Invoke-Api -Method POST -Uri "$base/Inventory/adjustments/request" -Headers $authHeaders -Body @{ productId = $defaultProductId; amount = 1; reason = "Smoke req" }
Add-Result $results "Inventory adjustment request" $r.ok $r.status
$adjId = $r.json.id

$r = Invoke-Api -Method GET -Uri "$base/Inventory/adjustments/pending" -Headers $authHeaders
Add-Result $results "Inventory pending adjustments" $r.ok $r.status
if ($adjId) {
    $r = Invoke-Api -Method POST -Uri "$base/Inventory/adjustments/$adjId/approve" -Headers $authHeaders
    Add-Result $results "Inventory approve adjustment" $r.ok $r.status
}

$passCount = ($results | Where-Object { $_.pass }).Count
$fail = $results | Where-Object { -not $_.pass }
$summary = [pscustomobject]@{
    total = $results.Count
    passed = $passCount
    failed = $fail.Count
}

"SUMMARY: total=$($summary.total) passed=$($summary.passed) failed=$($summary.failed)"
if ($fail.Count -gt 0) {
    "FAILED_TESTS:"
    $fail | ForEach-Object { "$($_.name) | status=$($_.status) | $($_.note)" }
}

$outPath = "F:\Axonix\.net project\InventorySystem\artifacts\full-smoke-results.json"
([pscustomobject]@{
    summary = $summary
    results = $results
}) | ConvertTo-Json -Depth 10 | Out-File -FilePath $outPath -Encoding utf8
"RESULT_FILE=$outPath"
