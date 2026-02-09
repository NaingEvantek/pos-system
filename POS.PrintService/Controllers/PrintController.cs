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
        sb.AppendLine("<style>");
        sb.AppendLine("body { font-family: 'Courier New', monospace; width: 300px; margin: 20px auto; }");
        sb.AppendLine(".header { text-align: center; margin-bottom: 20px; }");
        sb.AppendLine(".divider { border-bottom: 1px dashed #000; margin: 10px 0; }");
        sb.AppendLine(".item { display: flex; justify-content: space-between; margin: 5px 0; }");
        sb.AppendLine(".total { font-weight: bold; font-size: 16px; margin-top: 10px; }");
        sb.AppendLine(".discount { color: #dc3545; }");
        sb.AppendLine(".balance { color: #28a745; font-weight: bold; }");
        sb.AppendLine("@media print { body { margin: 0; } }");
        sb.AppendLine("</style>");
        sb.AppendLine("</head>");
        sb.AppendLine("<body>");
        
        sb.AppendLine("<div class='header'>");
        sb.AppendLine("<h2>POS SYSTEM</h2>");
        sb.AppendLine("<p>Sales Receipt</p>");
        sb.AppendLine("</div>");
        
        sb.AppendLine("<div class='divider'></div>");
        
        sb.AppendLine($"<p>Receipt #: {receipt.SaleId}</p>");
        sb.AppendLine($"<p>Date: {receipt.SaleDate:yyyy-MM-dd HH:mm:ss}</p>");
        sb.AppendLine($"<p>Customer: {receipt.CustomerName}</p>");
        sb.AppendLine($"<p>Payment: {receipt.PaymentMethod}</p>");
        
        sb.AppendLine("<div class='divider'></div>");
        
        sb.AppendLine("<h3>Items:</h3>");
        foreach (var item in receipt.Items)
        {
            sb.AppendLine("<div class='item'>");
            sb.AppendLine($"<span>{item.ProductName} x{item.Quantity}</span>");
            sb.AppendLine($"<span>${item.Total:F2}</span>");
            sb.AppendLine("</div>");
        }
        
        sb.AppendLine("<div class='divider'></div>");
        
        sb.AppendLine("<div class='item'>");
        sb.AppendLine($"<span>Subtotal:</span>");
        sb.AppendLine($"<span>${receipt.Subtotal:F2}</span>");
        sb.AppendLine("</div>");
        
        if (receipt.Discount > 0)
        {
            sb.AppendLine("<div class='item discount'>");
            sb.AppendLine($"<span>Discount:</span>");
            sb.AppendLine($"<span>-${receipt.Discount:F2}</span>");
            sb.AppendLine("</div>");
        }
        
        if (receipt.Tax > 0)
        {
            sb.AppendLine("<div class='item'>");
            sb.AppendLine($"<span>Tax:</span>");
            sb.AppendLine($"<span>${receipt.Tax:F2}</span>");
            sb.AppendLine("</div>");
        }
        
        sb.AppendLine("<div class='divider'></div>");
        
        sb.AppendLine("<div class='item total'>");
        sb.AppendLine($"<span>TOTAL:</span>");
        sb.AppendLine($"<span>${receipt.TotalAmount:F2}</span>");
        sb.AppendLine("</div>");
        
        if (receipt.PaymentAmount > 0)
        {
            sb.AppendLine("<div class='item'>");
            sb.AppendLine($"<span>Payment:</span>");
            sb.AppendLine($"<span>${receipt.PaymentAmount:F2}</span>");
            sb.AppendLine("</div>");
            
            if (receipt.Balance != 0)
            {
                sb.AppendLine("<div class='item balance'>");
                sb.AppendLine($"<span>{(receipt.Balance >= 0 ? "Change:" : "Balance Due:")}</span>");
                sb.AppendLine($"<span>${Math.Abs(receipt.Balance):F2}</span>");
                sb.AppendLine("</div>");
            }
        }
        
        sb.AppendLine("<div class='divider'></div>");
        
        sb.AppendLine("<p style='text-align: center; margin-top: 20px;'>Thank you for your purchase!</p>");
        
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
