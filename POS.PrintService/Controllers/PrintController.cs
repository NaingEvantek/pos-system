using Microsoft.AspNetCore.Mvc;
using POS.PrintService.Models;
using System.Drawing;
using System.Drawing.Printing;
using System.Text;

namespace POS.PrintService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PrintController : ControllerBase
{
    private ReceiptModel? _currentReceipt;

    [HttpPost("receipt")]
    public IActionResult PrintReceipt([FromBody] ReceiptModel receipt)
    {
        try
        {
            _currentReceipt = receipt;

            // For demonstration, we'll create an HTML receipt
            // In production, you'd use PrintDocument for actual printing
            var html = GenerateReceiptHtml(receipt);

            // Uncomment below for actual printing on Windows
            /*
            var printDocument = new PrintDocument();
            printDocument.PrintPage += PrintDocument_PrintPage;
            printDocument.Print();
            */

            return Ok(new
            {
                Success = true,
                Message = "Receipt sent to printer",
                PreviewHtml = html
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                Success = false,
                Message = $"Print failed: {ex.Message}"
            });
        }
    }

    [HttpGet("printers")]
    public IActionResult GetAvailablePrinters()
    {
        try
        {
            var printers = new List<string>();

            // Get installed printers (Windows only)
            foreach (string printer in PrinterSettings.InstalledPrinters)
            {
                printers.Add(printer);
            }

            return Ok(new
            {
                Printers = printers,
                DefaultPrinter = new PrinterSettings().PrinterName
            });
        }
        catch (Exception ex)
        {
            return Ok(new
            {
                Printers = new[] { "Default Printer" },
                DefaultPrinter = "Default Printer",
                Note = "Running in compatibility mode"
            });
        }
    }

    [HttpPost("test")]
    public IActionResult TestPrint()
    {
        try
        {
            var testReceipt = new ReceiptModel
            {
                SaleId = 999,
                SaleDate = DateTime.Now,
                CustomerName = "Test Customer",
                PaymentMethod = "Cash",
                Subtotal = 100.00m,
                Tax = 10.00m,
                TotalAmount = 110.00m,
                Items = new List<ReceiptItem>
                {
                    new ReceiptItem
                    {
                        ProductName = "Test Product",
                        Quantity = 1,
                        UnitPrice = 100.00m,
                        Total = 100.00m
                    }
                }
            };

            var html = GenerateReceiptHtml(testReceipt);

            return Ok(new
            {
                Success = true,
                Message = "Test print completed",
                PreviewHtml = html
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                Success = false,
                Message = $"Test print failed: {ex.Message}"
            });
        }
    }

    private string GenerateReceiptHtml(ReceiptModel receipt)
    {
        var sb = new StringBuilder();

        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html>");
        sb.AppendLine("<head>");
        sb.AppendLine("<meta charset='utf-8' />");
        sb.AppendLine("<style>");

        sb.AppendLine(@"
        body {
            font-family: Arial, sans-serif;
            width: 800px;
            margin: 20px auto;
            font-size: 14px;
        }

        .header {
            text-align: center;
        }

        .header h1 {
            margin: 0;
            color: #c2185b;
        }

        .header p {
            margin: 2px 0;
            font-size: 13px;
        }

        .info {
            margin-top: 15px;
        }

        .info table {
            width: 100%;
        }

        .info td {
            padding: 4px;
        }

        table.ledger {
            width: 100%;
            border-collapse: collapse;
            margin-top: 10px;
        }

        table.ledger th,
        table.ledger td {
            border: 1px solid #c2185b;
            padding: 6px;
            text-align: center;
        }

        table.ledger th {
            background-color: #f8bbd0;
        }

        .text-left {
            text-align: left;
        }

        .totals {
            width: 100%;
            margin-top: 10px;
        }

        .totals td {
            padding: 6px;
        }

        .totals .label {
            text-align: right;
            font-weight: bold;
            color: #c2185b;
        }

        .signature {
            margin-top: 30px;
        }

        @media print {
            body {
                margin: 0;
            }
        }
    ");

        sb.AppendLine("</style>");
        sb.AppendLine("</head>");
        sb.AppendLine("<body>");


        // Header
        sb.AppendLine("<div class='header'>");
        if (!string.IsNullOrEmpty(receipt.LogoBase64))
        {
            sb.AppendLine($@"
        <div style='text-align:center;margin-bottom:10px'>
            <img src='{receipt.LogoBase64}' style='max-width:150px;' />
        </div>
    ");
        }
        sb.AppendLine("<h1>ZarLi Fashion</h1>");
        sb.AppendLine("<p>အထည်ချုပ်နှင့် ဖက်ရှင်</p>");
        sb.AppendLine("<p>09-797510450 , 09-961694250 , 09-797465345</p>");
        sb.AppendLine("</div>");

        // Customer Info
        sb.AppendLine("<div class='info'>");
        sb.AppendLine("<table>");
        sb.AppendLine("<tr>");
        sb.AppendLine($"<td><b>Customer:</b> {receipt.CustomerName}</td>");
        sb.AppendLine($"<td><b>Invoice No:</b> 0000{receipt.SaleId.ToString("D6")}</td>");
        sb.AppendLine("</tr>");
        sb.AppendLine("<tr>");
        // sb.AppendLine($"<td><b>Phone:</b> {receipt.CustomerPhone}</td>");
        sb.AppendLine($"<td><b>Date:</b> {receipt.SaleDate:yyyy-MM-dd}</td>");
        sb.AppendLine("</tr>");
        sb.AppendLine("</table>");
        sb.AppendLine("</div>");

        // Ledger Table
        sb.AppendLine("<table class='ledger'>");
        sb.AppendLine("<thead>");
        sb.AppendLine("<tr>");
        sb.AppendLine("<th>No</th>");
        sb.AppendLine("<th>Description</th>");
        sb.AppendLine("<th>Qty</th>");
        sb.AppendLine("<th>Unit Price</th>");
        sb.AppendLine("<th>Amount</th>");
        sb.AppendLine("</tr>");
        sb.AppendLine("</thead>");
        sb.AppendLine("<tbody>");

        int index = 1;
        foreach (var item in receipt.Items)
        {
            sb.AppendLine("<tr>");
            sb.AppendLine($"<td>{index++}</td>");
            sb.AppendLine($"<td class='text-left'>{item.ProductName}</td>");
            sb.AppendLine($"<td>{item.Quantity}</td>");
            sb.AppendLine($"<td>{item.UnitPrice:N0}</td>");
            sb.AppendLine($"<td>{item.Total:N0}</td>");
            sb.AppendLine("</tr>");
        }

        var emptyRows = 16 - receipt.Items.Count;
        // Fill empty rows up to 16 like the paper invoice
        for (int i = index; i <= emptyRows; i++)
        {
            sb.AppendLine("<tr>");
            sb.AppendLine($"<td>{i}</td>");
            sb.AppendLine("<td></td>");
            sb.AppendLine("<td></td>");
            sb.AppendLine("<td></td>");
            sb.AppendLine("<td></td>");
            sb.AppendLine("</tr>");
        }

        sb.AppendLine("</tbody>");
        sb.AppendLine("</table>");

        // Totals
        sb.AppendLine("<table class='totals'>");
        sb.AppendLine("<tr>");
        sb.AppendLine("<td colspan='2'>");
        sb.AppendLine("<div class='signature'>");
        sb.AppendLine("<p>အမည် / လက်မှတ် ....................................................</p>");
        sb.AppendLine("</div>");
        sb.AppendLine("</td>");
        sb.AppendLine($"<td class='label' style='text-align: right;'>{receipt.PaymentMethod}</td>");
        sb.AppendLine("<td colspan='2'>Total :</td>");
        sb.AppendLine($"<td>{receipt.TotalAmount:N0}</td>");
        sb.AppendLine("</tr>");
        sb.AppendLine("<tr>");
        sb.AppendLine("<td></td>");
        // sb.AppendLine($"<td class='label'>Advance :</td>");
        // sb.AppendLine($"<td>{receipt.Advance:N0}</td>");
        // sb.AppendLine("</tr>");
        // sb.AppendLine("<tr>");
        // sb.AppendLine("<td></td>");
        // sb.AppendLine($"<td class='label'>Balance :</td>");
        // sb.AppendLine($"<td>{receipt.Balance:N0}</td>");
        sb.AppendLine("</tr>");
        sb.AppendLine("</table>");
        sb.AppendLine("</body>");
        sb.AppendLine("</html>");

        return sb.ToString();
    }

    // Actual printing method (for Windows)
    private void PrintDocument_PrintPage(object sender, PrintPageEventArgs e)
    {
        if (_currentReceipt == null || e.Graphics == null) return;

        var font = new Font("Courier New", 10);
        var boldFont = new Font("Courier New", 12, FontStyle.Bold);
        var brush = Brushes.Black;
        float y = 20;
        float lineHeight = 15;

        // Header
        e.Graphics.DrawString("POS SYSTEM", boldFont, brush, 80, y);
        y += lineHeight * 2;
        e.Graphics.DrawString("Sales Receipt", font, brush, 80, y);
        y += lineHeight * 2;

        // Receipt details
        e.Graphics.DrawString($"Receipt #: {_currentReceipt.SaleId}", font, brush, 20, y);
        y += lineHeight;
        e.Graphics.DrawString($"Date: {_currentReceipt.SaleDate:yyyy-MM-dd HH:mm:ss}", font, brush, 20, y);
        y += lineHeight;
        e.Graphics.DrawString($"Customer: {_currentReceipt.CustomerName}", font, brush, 20, y);
        y += lineHeight;
        e.Graphics.DrawString($"Payment: {_currentReceipt.PaymentMethod}", font, brush, 20, y);
        y += lineHeight * 2;

        // Items
        e.Graphics.DrawString("Items:", boldFont, brush, 20, y);
        y += lineHeight;

        foreach (var item in _currentReceipt.Items)
        {
            var itemText = $"{item.ProductName} x{item.Quantity}";
            var priceText = $"${item.Total:F2}";
            e.Graphics.DrawString(itemText, font, brush, 20, y);
            e.Graphics.DrawString(priceText, font, brush, 200, y);
            y += lineHeight;
        }

        y += lineHeight;
        e.Graphics.DrawString($"Subtotal: ${_currentReceipt.Subtotal:F2}", font, brush, 20, y);
        y += lineHeight;
        e.Graphics.DrawString($"Tax: ${_currentReceipt.Tax:F2}", font, brush, 20, y);
        y += lineHeight * 2;
        e.Graphics.DrawString($"TOTAL: ${_currentReceipt.TotalAmount:F2}", boldFont, brush, 20, y);

        y += lineHeight * 3;
        e.Graphics.DrawString("Thank you for your purchase!", font, brush, 30, y);

        font.Dispose();
        boldFont.Dispose();
    }
}
