using ClosedXML.Excel;
using InventorySystem.Application.Services;
using InventorySystem.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace InventorySystem.Infrastructure.Services
{
    public class ExportService : IExportService
    {
        private readonly IApplicationDbContext _context;

        public ExportService(IApplicationDbContext context)
        {
            _context = context;
            // License required for QuestPDF in community mode
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public async Task<byte[]> GenerateInventoryExcelAsync(object data)
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Inventory Report");
                
                // For simplicity, we manually map some common report data types.
                // In production, we'd use reflection or specific DTOs.
                worksheet.Cell(1, 1).Value = "Inventory Report - " + DateTime.Now.ToString("yyyy-MM-dd");
                worksheet.Cell(1, 1).Style.Font.Bold = true;
                
                // Assuming data is a list of dynamic or specific objects for now
                // This is a placeholder for actual data mapping.
                
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return stream.ToArray();
                }
            }
        }

        public async Task<byte[]> GenerateSalesInvoicePdfAsync(int salesInvoiceId)
        {
            var invoice = await _context.SalesInvoices
                .Include(x => x.Customer)
                .Include(x => x.Warehouse)
                .Include(x => x.Items)
                .ThenInclude(x => x.Product)
                .FirstOrDefaultAsync(x => x.Id == salesInvoiceId);

            if (invoice == null) throw new Exception("Invoice not found.");

            var seller = invoice.Warehouse;
            var buyer = invoice.Customer;

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(0.5f, Unit.Inch);
                    page.Header().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text(seller?.Name ?? "INVENTORY SYSTEM").FontSize(16).Bold().FontColor(Colors.Blue.Medium);
                            col.Item().Text(seller?.AddressLine1 ?? "Branch Address");
                            col.Item().Text($"{seller?.City}, {seller?.State} - {seller?.PostalCode}");
                            col.Item().Text($"GSTIN: {seller?.Gstin ?? "N/A"} | PAN: {seller?.Pan ?? "N/A"}").FontSize(9);
                        });

                        row.RelativeItem().Column(col =>
                        {
                            col.Item().AlignRight().Text("TAX INVOICE").FontSize(20).Bold().FontColor(Colors.Grey.Medium);
                            col.Item().AlignRight().Text($"Invoice #: {invoice.InvoiceNumber}");
                            col.Item().AlignRight().Text($"Date: {invoice.InvoiceDate:dd-MMM-yyyy}");
                        });
                    });

                    page.Content().PaddingTop(10).Column(col =>
                    {
                        col.Item().Row(row => {
                            row.RelativeItem().Column(c => {
                                c.Item().Text("Bill To:").Bold();
                                c.Item().Text(buyer?.Name ?? "Cash Customer");
                                c.Item().Text(buyer?.BillingAddress ?? "N/A");
                                c.Item().Text($"GSTIN: {buyer?.Gstin ?? "N/A"}").FontSize(9);
                            });
                            row.RelativeItem().Column(c => {
                                c.Item().AlignRight().Text("Place of Supply:").Bold();
                                c.Item().AlignRight().Text(invoice.PlaceOfSupplyState ?? seller?.State ?? "N/A");
                            });
                        });

                        col.Item().PaddingTop(10).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(20);
                                columns.RelativeColumn();
                                columns.ConstantColumn(40);
                                columns.ConstantColumn(30);
                                columns.ConstantColumn(60);
                                columns.ConstantColumn(30);
                                columns.ConstantColumn(60);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Text("#");
                                header.Cell().Text("Product / HSN");
                                header.Cell().Text("Price");
                                header.Cell().Text("Qty");
                                header.Cell().Text("Taxable");
                                header.Cell().Text("GST");
                                header.Cell().Text("Amount");
                            });

                            int idx = 1;
                            foreach (var item in invoice.Items)
                            {
                                table.Cell().Text($"{idx++}");
                                table.Cell().Column(c => {
                                    c.Item().Text(item.Product?.Name ?? "Item");
                                    c.Item().Text($"HSN: {item.Product?.HsnCode ?? "N/A"}").FontSize(8).Italic();
                                });
                                table.Cell().Text($"{item.UnitPrice:N2}");
                                table.Cell().Text($"{item.Quantity}");
                                table.Cell().Text($"{item.TaxableAmount:N2}");
                                table.Cell().Text($"{item.GstRate}%");
                                table.Cell().Text($"{item.LineTotal:N2}");
                            }
                        });

                        col.Item().PaddingTop(10).Row(row =>
                        {
                            row.RelativeItem().Column(c => {
                                c.Item().Text("Bank Details:").Bold().FontSize(10);
                                c.Item().Text($"Bank: {seller?.BankName ?? "Not Configured"}");
                                c.Item().Text($"A/c No: {seller?.BankAccountNo ?? "N/A"}");
                                c.Item().Text($"IFSC: {seller?.IfscCode ?? "N/A"}");
                                c.Item().PaddingTop(10).Text("Terms:").Italic().FontSize(8);
                                c.Item().Text("1. Goods once sold will not be taken back.").FontSize(8);
                                c.Item().Text("2. Pay via UPI for faster processing.").FontSize(8);
                                c.Item().PaddingTop(5).Text($"UPI: {seller?.IfscCode ?? "merchant"}@upi").FontSize(7).FontColor(Colors.Blue.Medium);
                            });

                            row.ConstantItem(150).Column(c => {
                                c.Item().Row(r => { r.RelativeItem().Text("Sub Total:"); r.RelativeItem().AlignRight().Text($"{invoice.Subtotal:N2}"); });
                                if (invoice.IgstAmount > 0)
                                    c.Item().Row(r => { r.RelativeItem().Text("IGST:"); r.RelativeItem().AlignRight().Text($"{invoice.IgstAmount:N2}"); });
                                else {
                                    c.Item().Row(r => { r.RelativeItem().Text("CGST:"); r.RelativeItem().AlignRight().Text($"{invoice.CgstAmount:N2}"); });
                                    c.Item().Row(r => { r.RelativeItem().Text("SGST:"); r.RelativeItem().AlignRight().Text($"{invoice.SgstAmount:N2}"); });
                                }
                                c.Item().PaddingTop(5).BorderTop(1).Row(r => { 
                                    r.RelativeItem().Text("Grand Total:").Bold(); 
                                    r.RelativeItem().AlignRight().Text($"₹ {invoice.GrandTotal:N2}").Bold(); 
                                });
                            });
                        });
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("This is a computer generated invoice.");
                    });
                });
            });

            return document.GeneratePdf();
        }

        public async Task<byte[]> GenerateTallyOrdersXmlAsync(DateTime fromDate, DateTime toDate)
        {
            var invoices = await _context.SalesInvoices
                .Include(x => x.Customer)
                .Where(x => x.InvoiceDate >= fromDate && x.InvoiceDate <= toDate)
                .ToListAsync();

            var envelope = new System.Xml.Linq.XElement("ENVELOPE",
                new System.Xml.Linq.XElement("HEADER",
                    new System.Xml.Linq.XElement("TALLYREQUEST", "Import Data")),
                new System.Xml.Linq.XElement("BODY",
                    new System.Xml.Linq.XElement("IMPORTDATA",
                        new System.Xml.Linq.XElement("REQUESTDESC",
                            new System.Xml.Linq.XElement("REPORTNAME", "Vouchers")),
                        new System.Xml.Linq.XElement("REQUESTDATA",
                            invoices.Select(inv => new System.Xml.Linq.XElement("TALLYMESSAGE",
                                new System.Xml.Linq.XElement("VOUCHER", new System.Xml.Linq.XAttribute("VCHTYPE", "Sales"), new System.Xml.Linq.XAttribute("ACTION", "Create"),
                                    new System.Xml.Linq.XElement("DATE", inv.InvoiceDate.ToString("yyyyMMdd")),
                                    new System.Xml.Linq.XElement("VOUCHERNUMBER", inv.InvoiceNumber),
                                    new System.Xml.Linq.XElement("PARTYLEDGERNAME", inv.Customer?.Name ?? "Cash"),
                                    new System.Xml.Linq.XElement("EFFECTIVEDATE", inv.InvoiceDate.ToString("yyyyMMdd")),
                                    new System.Xml.Linq.XElement("ALLLEDGERENTRIES.LIST",
                                        new System.Xml.Linq.XElement("LEDGERNAME", inv.Customer?.Name ?? "Cash"),
                                        new System.Xml.Linq.XElement("ISDEEMEDPOSITIVE", "Yes"),
                                        new System.Xml.Linq.XElement("AMOUNT", -(inv.GrandTotal))),
                                    new System.Xml.Linq.XElement("ALLLEDGERENTRIES.LIST",
                                        new System.Xml.Linq.XElement("LEDGERNAME", "Sales Revenue"),
                                        new System.Xml.Linq.XElement("ISDEEMEDPOSITIVE", "No"),
                                        new System.Xml.Linq.XElement("AMOUNT", inv.Subtotal)),
                                    inv.IgstAmount > 0 ? 
                                        new System.Xml.Linq.XElement("ALLLEDGERENTRIES.LIST",
                                            new System.Xml.Linq.XElement("LEDGERNAME", "IGST"),
                                            new System.Xml.Linq.XElement("ISDEEMEDPOSITIVE", "No"),
                                            new System.Xml.Linq.XElement("AMOUNT", inv.IgstAmount)) :
                                        new object[] {
                                            new System.Xml.Linq.XElement("ALLLEDGERENTRIES.LIST",
                                                new System.Xml.Linq.XElement("LEDGERNAME", "CGST"),
                                                new System.Xml.Linq.XElement("ISDEEMEDPOSITIVE", "No"),
                                                new System.Xml.Linq.XElement("AMOUNT", inv.CgstAmount)),
                                            new System.Xml.Linq.XElement("ALLLEDGERENTRIES.LIST",
                                                new System.Xml.Linq.XElement("LEDGERNAME", "SGST"),
                                                new System.Xml.Linq.XElement("ISDEEMEDPOSITIVE", "No"),
                                                new System.Xml.Linq.XElement("AMOUNT", inv.SgstAmount))
                                        }
                                ))
                            )
                        )
                    )
                )
            );

            using (var stream = new MemoryStream())
            {
                envelope.Save(stream);
                return stream.ToArray();
            }
        }
    }
}
