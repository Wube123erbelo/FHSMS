using FHSMS.Application.Common.Models;

namespace FHSMS.API.Common;

/// <summary>Composes an InvoiceDto into a printable one-page PDF via SimplePdfWriter.</summary>
public static class InvoicePdfBuilder
{
    public static byte[] Build(InvoiceDto invoice)
    {
        var pdf = new SimplePdfWriter();

        pdf.AddLine("FHSMS - Farm-Hotel Supply Management", 14, bold: true);
        pdf.AddLine($"Invoice {invoice.InvoiceNumber}", 12, bold: true);
        pdf.AddSpacer();
        pdf.AddLine($"Date: {invoice.InvoiceDate:yyyy-MM-dd HH:mm}");
        pdf.AddLine($"Status: {invoice.Status}");
        pdf.AddLine($"Tax: {(invoice.TaxWasEnabled ? "Enabled" : "Disabled")} at time of issue"
            + (invoice.TaxMode is not null ? $" ({invoice.TaxMode})" : ""));
        pdf.AddSpacer();

        pdf.AddLine("Line items:", 11, bold: true);
        foreach (var item in invoice.Items)
        {
            pdf.AddLine($"{item.ProductName}  x{item.Quantity}  @ {item.UnitPrice:0.00} ETB");
            pdf.AddLine($"    Tax profile: {item.TaxProfileApplied}   Rate: {item.TaxRateApplied}%   Tax: {item.TaxAmount:0.00} ETB   Line total: {item.LineTotal:0.00} ETB");
        }

        pdf.AddSpacer();
        pdf.AddLine($"Subtotal:            {invoice.Subtotal,10:0.00} ETB");
        pdf.AddLine($"Taxable amount:      {invoice.TaxableAmount,10:0.00} ETB");
        pdf.AddLine($"Tax:                 {invoice.TaxAmount,10:0.00} ETB");
        pdf.AddLine($"Total commission ({invoice.PlatformCommissionRateApplied:0.##}%): {invoice.PlatformCommissionAmount,10:0.00} ETB");
        pdf.AddLine($"Hotel agent bonus:   {invoice.HotelAgentBonusAmount,10:0.00} ETB");
        pdf.AddLine($"Discount:            {invoice.Discount,10:0.00} ETB");
        pdf.AddLine($"Grand total:         {invoice.GrandTotal,10:0.00} ETB", 11, bold: true);
        pdf.AddLine($"Paid:                {invoice.AmountPaid,10:0.00} ETB");
        pdf.AddLine($"Balance due:         {invoice.BalanceDue,10:0.00} ETB", 11, bold: true);

        return pdf.Build();
    }
}
