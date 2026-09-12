using FHSMS.API.Common;
using FHSMS.Application.Common.Models;
using FHSMS.Application.Products.Commands.CreateProduct;
using FHSMS.Application.Products.Commands.DeleteProduct;
using FHSMS.Application.Products.Commands.SchedulePrice;
using FHSMS.Application.Products.Commands.UpdateProduct;
using FHSMS.Application.Products.Queries.GetPriceHistory;
using FHSMS.Application.Products.Queries.GetProducts;
using FHSMS.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FHSMS.API.Controllers;

[Authorize]
public class ProductsController : ApiControllerBase
{
    public record UpdateProductRequest(
        string Sku, string Name, string? Description, Guid CategoryId, Guid UnitId,
        TaxProfileType TaxProfile, decimal? LowStockThreshold, bool IsActive);

    public record SchedulePriceRequest(PriceKind Kind, decimal Price, DateTime EffectiveFrom, string? Reason);

    [HttpGet]
    public async Task<ActionResult<List<ProductDto>>> GetAll([FromQuery] bool activeOnly = true)
        => Ok(await Mediator.Send(new GetProductsQuery(activeOnly)));

    [HttpGet("export/csv")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> ExportCsv([FromQuery] bool activeOnly = false)
    {
        var products = await Mediator.Send(new GetProductsQuery(activeOnly));
        var csv = CsvExporter.Write(products, new (string, Func<ProductDto, object?>)[]
        {
            ("SKU", p => p.Sku),
            ("Name", p => p.Name),
            ("Category", p => p.CategoryName),
            ("Buying price", p => p.CurrentBuyingPrice),
            ("Selling price", p => p.CurrentSellingPrice),
            ("Tax profile", p => p.TaxProfile),
            ("Active", p => p.IsActive)
        });
        return File(csv, "text/csv", "products.csv");
    }

    [HttpGet("export/excel")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> ExportExcel([FromQuery] bool activeOnly = false)
    {
        var products = await Mediator.Send(new GetProductsQuery(activeOnly));
        var xls = ExcelExporter.Write("Products", products, new (string, Func<ProductDto, object?>)[]
        {
            ("SKU", p => p.Sku),
            ("Name", p => p.Name),
            ("Category", p => p.CategoryName),
            ("Buying price", p => p.CurrentBuyingPrice),
            ("Selling price", p => p.CurrentSellingPrice),
            ("Tax profile", p => p.TaxProfile),
            ("Active", p => p.IsActive)
        });
        return File(xls, "application/vnd.ms-excel", "products.xls");
    }

    [HttpGet("export/pdf")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> ExportPdf([FromQuery] bool activeOnly = false)
    {
        var products = await Mediator.Send(new GetProductsQuery(activeOnly));
        var pdf = PdfTableExporter.Write("Products", products, new (string, Func<ProductDto, object?>)[]
        {
            ("SKU", p => p.Sku),
            ("Name", p => p.Name),
            ("Category", p => p.CategoryName),
            ("Buying price", p => p.CurrentBuyingPrice),
            ("Selling price", p => p.CurrentSellingPrice),
            ("Active", p => p.IsActive)
        });
        return File(pdf, "application/pdf", "products.pdf");
    }

    [HttpPost]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<ActionResult<Guid>> Create(CreateProductCommand command)
        => Ok(await Mediator.Send(command));

    [HttpPut("{productId:guid}")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> Update(Guid productId, UpdateProductRequest request)
    {
        await Mediator.Send(new UpdateProductCommand(
            productId, request.Sku, request.Name, request.Description, request.CategoryId, request.UnitId,
            request.TaxProfile, request.LowStockThreshold, request.IsActive));
        return NoContent();
    }

    /// <summary>The only way a product's price changes - see SchedulePriceCommand. Kind selects Buying or Selling.</summary>
    [HttpPost("{productId:guid}/price")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<ActionResult<Guid>> SchedulePrice(Guid productId, SchedulePriceRequest request)
        => Ok(await Mediator.Send(new SchedulePriceCommand(productId, request.Kind, request.Price, request.EffectiveFrom, request.Reason)));

    [HttpGet("{productId:guid}/price-history")]
    [Authorize(Roles = "SuperAdmin,Driver")]
    public async Task<ActionResult<List<ProductPriceHistoryDto>>> GetPriceHistory(Guid productId, [FromQuery] PriceKind? kind = null)
        => Ok(await Mediator.Send(new GetPriceHistoryQuery(productId, kind)));

    /// <summary>Soft-delete: deactivates the product. Historical order/invoice lines are untouched.</summary>
    [HttpDelete("{productId:guid}")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> Delete(Guid productId)
    {
        await Mediator.Send(new DeleteProductCommand(productId));
        return NoContent();
    }
}
