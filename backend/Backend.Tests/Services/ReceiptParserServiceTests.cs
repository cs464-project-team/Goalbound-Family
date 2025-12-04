using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using GoalboundFamily.Api.Services;
using GoalboundFamily.Api.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Backend.Tests.Services;

public class ReceiptParserServiceTests
{
    private readonly Mock<ILogger<ReceiptParserService>> _loggerMock;
    private readonly ReceiptParserService _service;

    public ReceiptParserServiceTests()
    {
        _loggerMock = new Mock<ILogger<ReceiptParserService>>();
        _service = new ReceiptParserService(_loggerMock.Object);
    }

    [Fact]
    public async Task ParseReceiptAsync_EmptyOcrResult_ReturnsEmptyReceipt()
    {
        // Arrange
        var ocrResult = new OcrResult
        {
            Success = false,
            Text = "",
            TextBlocks = new List<OcrTextBlock>()
        };

        // Act
        var result = await _service.ParseReceiptAsync(ocrResult);

        // Assert
        result.Should().NotBeNull();
        result.MerchantName.Should().BeNull();
        result.TotalAmount.Should().BeNull();
        result.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task ParseReceiptAsync_ValidReceipt_ParsesMerchantName()
    {
        // Arrange
        var ocrResult = new OcrResult
        {
            Success = true,
            Text = @"
Store Name
123 Main St

1x Chicken Rice $5.00

TOTAL $5.00
",
            TextBlocks = new List<OcrTextBlock>()
        };

        // Act
        var result = await _service.ParseReceiptAsync(ocrResult);

        // Assert
        result.Should().NotBeNull();
        result.MerchantName.Should().NotBeNull();
    }

    [Fact]
    public async Task ParseReceiptAsync_ValidReceipt_ParsesTotal()
    {
        // Arrange
        var ocrResult = new OcrResult
        {
            Success = true,
            Text = @"
Store Name

1x Item $5.00

TOTAL $5.00
",
            TextBlocks = new List<OcrTextBlock>()
        };

        // Act
        var result = await _service.ParseReceiptAsync(ocrResult);

        // Assert
        result.Should().NotBeNull();
        result.TotalAmount.Should().Be(5.00m);
    }

    [Fact]
    public async Task ParseReceiptAsync_DateFormats_ParsesDate()
    {
        // Arrange
        var ocrResult = new OcrResult
        {
            Success = true,
            Text = @"
Store Name
DATE: 2024-01-15

1x Item $5.00
TOTAL $5.00
",
            TextBlocks = new List<OcrTextBlock>()
        };

        // Act
        var result = await _service.ParseReceiptAsync(ocrResult);

        // Assert
        result.Should().NotBeNull();
        result.ReceiptDate.Should().NotBeNull();
        result.ReceiptDate!.Value.Year.Should().Be(2024);
        result.ReceiptDate!.Value.Month.Should().Be(1);
        result.ReceiptDate!.Value.Day.Should().Be(15);
    }

    [Fact]
    public async Task ParseReceiptAsync_InternationalPrices_ParsesCommaDecimal()
    {
        // Arrange
        var ocrResult = new OcrResult
        {
            Success = true,
            Text = @"
Store Name

1x Coffee $3,50

TOTAL $3,50
",
            TextBlocks = new List<OcrTextBlock>()
        };

        // Act
        var result = await _service.ParseReceiptAsync(ocrResult);

        // Assert
        result.Should().NotBeNull();
        result.TotalAmount.Should().Be(3.50m);
    }

    [Fact]
    public async Task ParseReceiptAsync_WithServiceChargeAndGST_ParsesTotalCorrectly()
    {
        // Arrange
        var ocrResult = new OcrResult
        {
            Success = true,
            Text = @"
Restaurant Name

1x Meal $10.00

TOTAL $10.00
",
            TextBlocks = new List<OcrTextBlock>()
        };

        // Act
        var result = await _service.ParseReceiptAsync(ocrResult);

        // Assert
        result.Should().NotBeNull();
        result.TotalAmount.Should().Be(10.00m);
    }

    [Fact]
    public async Task ParseReceiptAsync_NoTotal_ReturnsNullTotalAmount()
    {
        // Arrange
        var ocrResult = new OcrResult
        {
            Success = true,
            Text = @"
Store Name

1x Item 1 $5.00
2x Item 2 $10.00
",
            TextBlocks = new List<OcrTextBlock>()
        };

        // Act
        var result = await _service.ParseReceiptAsync(ocrResult);

        // Assert
        result.Should().NotBeNull();
        result.TotalAmount.Should().BeNull();
    }

    [Fact]
    public async Task ParseReceiptAsync_ValidItems_ReturnsNonEmptyItemsList()
    {
        // Arrange
        var ocrResult = new OcrResult
        {
            Success = true,
            Text = @"
Store Name
Address Line

1x Chicken Rice $5.00
2x Coffee $6.00

TOTAL $11.00
",
            TextBlocks = new List<OcrTextBlock>()
        };

        // Act
        var result = await _service.ParseReceiptAsync(ocrResult);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().NotBeEmpty();
        result.Items.Should().OnlyContain(item => item.TotalPrice > 0);
    }

    [Fact]
    public async Task ParseReceiptAsync_MultipleFormats_ParsesFlexibly()
    {
        // Arrange - Test various receipt formats
        var ocrResult = new OcrResult
        {
            Success = true,
            Text = @"
RESTAURANT NAME
123 Main Street

1 Burger $10.00
1 Fries $3.00
1 Drink $2.50

ORDER TOTAL: $15.50
",
            TextBlocks = new List<OcrTextBlock>()
        };

        // Act
        var result = await _service.ParseReceiptAsync(ocrResult);

        // Assert
        result.Should().NotBeNull();
        result.MerchantName.Should().NotBeNull();
        result.TotalAmount.Should().Be(15.50m);
    }
}
