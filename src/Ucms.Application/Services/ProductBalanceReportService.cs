namespace Ucms.Application.Services;

using ClosedXML.Excel;
using Ucms.Application.Features.Reports.DTOs;

public class ProductBalanceReportService : IProductBalanceReportService
{
    public Task<MemoryStream> GetExcelAsync(ProductBalanceReportModel data, CancellationToken cancellationToken = default)
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Hisobot");

        // Header
        ws.Cell(1, 1).Value = "Mahsulot turi";
        ws.Cell(1, 2).Value = "Mahsulot";
        ws.Cell(1, 3).Value = "Seriya";
        ws.Cell(1, 4).Value = "Yaroqlilik muddati";
        ws.Cell(1, 5).Value = "Bosh ombordan (bosh.)";
        ws.Cell(1, 6).Value = "Filial omborlardan (bosh.)";
        ws.Cell(1, 7).Value = "Jami (bosh.)";
        ws.Cell(1, 8).Value = "Kirim";
        ws.Cell(1, 9).Value = "Tarqatish chiqimi";
        ws.Cell(1, 10).Value = "Foydalanish chiqimi";
        ws.Cell(1, 11).Value = "Bosh ombor (oxir.)";
        ws.Cell(1, 12).Value = "Filial omborlari (oxir.)";
        ws.Cell(1, 13).Value = "Jami (oxir.)";
        ws.Cell(1, 14).Value = "O'lchov birligi";

        var headerRow = ws.Row(1);
        headerRow.Style.Font.Bold = true;
        headerRow.Style.Fill.BackgroundColor = XLColor.LightBlue;

        var row = 2;
        foreach (var productType in data.ProductTypes)
        {
            var typeName = productType.ProductType.ToString();
            foreach (var product in productType.Products)
            {
                foreach (var sku in product.Skus)
                {
                    ws.Cell(row, 1).Value = typeName;
                    ws.Cell(row, 2).Value = product.ProductNameRu;
                    ws.Cell(row, 3).Value = sku.Seria;
                    ws.Cell(row, 4).Value = sku.ExpirationDate.Date;
                    ws.Cell(row, 4).Style.NumberFormat.Format = "dd.MM.yyyy";
                    ws.Cell(row, 5).Value = (double)sku.CentralStockFromBalance;
                    ws.Cell(row, 6).Value = (double)sku.ChildStocksFromBalance;
                    ws.Cell(row, 7).Value = (double)sku.AllStocksFromBalance;
                    ws.Cell(row, 8).Value = (double)sku.CentralStockIncome;
                    ws.Cell(row, 9).Value = (double)sku.CentralStockBroadcastOutcome;
                    ws.Cell(row, 10).Value = (double)sku.AllStocksUsageOutcome;
                    ws.Cell(row, 11).Value = (double)sku.CentralStockToBalance;
                    ws.Cell(row, 12).Value = (double)sku.ChildStocksToBalance;
                    ws.Cell(row, 13).Value = (double)sku.AllStocksToBalance;
                    ws.Cell(row, 14).Value = product.MeasurementUnitNameRu;
                    row++;
                }
            }
        }

        ws.Columns().AdjustToContents();

        var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;

        return Task.FromResult(stream);
    }
}
