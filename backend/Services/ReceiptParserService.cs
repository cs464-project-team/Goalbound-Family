using System.Globalization;
using System.Text.RegularExpressions;
using GoalboundFamily.Api.Services.Interfaces;

namespace GoalboundFamily.Api.Services;

/// <summary>
/// Parses OCR text to extract receipt items and metadata
/// Uses pattern matching for common receipt formats
/// </summary>
public partial class ReceiptParserService : IReceiptParserService
{
    private readonly ILogger<ReceiptParserService> _logger;

    // Regex patterns for parsing
    // Support both period (.) and comma (,) as decimal separators for international receipts
    [GeneratedRegex(@"^\s*(\d+)x?\s+(.+?)\s+\$?([0-9]+[.,][0-9]{2})\s*$", RegexOptions.IgnoreCase)]
    private static partial Regex QuantityItemPricePattern();

    // Accept ":", "I", "l" as OCR errors for "1"
    [GeneratedRegex(@"^\s*([1-9Il:])(\s+)(.+)", RegexOptions.IgnoreCase)]
    private static partial Regex QuantityItemPattern();

    [GeneratedRegex(@"\$?\s*([0-9]+[.,][0-9]{2})\s*$", RegexOptions.IgnoreCase)]
    private static partial Regex PricePattern();

    [GeneratedRegex(@"(?:ORDER\s+)?TOTAL[:\s]*\$?\s*([0-9]+[.,][0-9]{2})", RegexOptions.IgnoreCase)]
    private static partial Regex TotalPattern();

    [GeneratedRegex(@"DATE[:\s]*(\d{2}[/-]\d{2}[/-]\d{2,4})", RegexOptions.IgnoreCase)]
    private static partial Regex DatePattern();

    [GeneratedRegex(@"(\d{4})[/-](\d{2})[/-](\d{2})", RegexOptions.None)]
    private static partial Regex IsoDatePattern();

    public ReceiptParserService(ILogger<ReceiptParserService> logger)
    {
        _logger = logger;
    }

    public async Task<ParsedReceipt> ParseReceiptAsync(OcrResult ocrResult)
    {
        _logger.LogInformation("Starting receipt parsing");

        var parsedReceipt = new ParsedReceipt();

        if (!ocrResult.Success || string.IsNullOrWhiteSpace(ocrResult.Text))
        {
            _logger.LogWarning("OCR result is empty or failed");
            return parsedReceipt;
        }

        var lines = ocrResult.Text.Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(l => l.Trim())
            .Where(l => !string.IsNullOrWhiteSpace(l))
            .ToList();

        _logger.LogInformation("Processing {Count} lines", lines.Count);

        // Extract metadata
        parsedReceipt.MerchantName = ExtractMerchantName(lines);
        parsedReceipt.ReceiptDate = ExtractDate(lines);
        parsedReceipt.TotalAmount = ExtractTotal(lines);

        // Extract line items
        parsedReceipt.Items = ExtractLineItems(lines, ocrResult.TextBlocks);

        _logger.LogInformation("Parsed {Count} items from receipt", parsedReceipt.Items.Count);

        // Verify totals match
        VerifyTotals(parsedReceipt);

        return await Task.FromResult(parsedReceipt);
    }

    /// <summary>
    /// Verifies that calculated total matches receipt total
    /// Logs warnings if there's a discrepancy and suggests potential OCR errors
    /// </summary>
    private void VerifyTotals(ParsedReceipt parsedReceipt)
    {
        if (!parsedReceipt.TotalAmount.HasValue)
        {
            _logger.LogWarning("⚠️ Receipt total not found - cannot verify item totals");
            return;
        }

        var calculatedTotal = parsedReceipt.CalculatedTotal;
        var receiptTotal = parsedReceipt.TotalAmount.Value;
        var discrepancy = parsedReceipt.TotalDiscrepancy!.Value;

        if (parsedReceipt.TotalMatchesReceipt)
        {
            _logger.LogInformation("✅ Total verification: Calculated ${Calculated:F2} matches receipt ${Receipt:F2}",
                calculatedTotal, receiptTotal);
        }
        else if (Math.Abs(discrepancy) < 1.00m)
        {
            _logger.LogWarning("⚠️ Total verification: Minor discrepancy of ${Discrepancy:F2} (Calculated: ${Calculated:F2}, Receipt: ${Receipt:F2})",
                Math.Abs(discrepancy), calculatedTotal, receiptTotal);
        }
        else if (discrepancy < 0)
        {
            _logger.LogWarning("⚠️ Total verification: Missing items! Calculated ${Calculated:F2} < Receipt ${Receipt:F2} (Difference: ${Diff:F2})",
                calculatedTotal, receiptTotal, Math.Abs(discrepancy));

            // Suggest potential OCR digit misreads
            DetectPotentialOcrErrors(parsedReceipt.Items, Math.Abs(discrepancy));
        }
        else
        {
            _logger.LogWarning("⚠️ Total verification: Possible false positives! Calculated ${Calculated:F2} > Receipt ${Receipt:F2} (Difference: ${Diff:F2})",
                calculatedTotal, receiptTotal, discrepancy);
        }
    }

    /// <summary>
    /// Detects potential OCR digit misreads by checking if correcting common errors would fix the total
    /// Common OCR confusions: 7↔2, 9↔0, 4↔2, 5↔6, 8↔3, 1↔7
    /// </summary>
    private void DetectPotentialOcrErrors(List<ParsedReceiptItem> items, decimal discrepancy)
    {
        _logger.LogInformation("🔍 Analyzing for potential OCR digit misreads...");

        var digitSubstitutions = new Dictionary<char, char[]>
        {
            ['0'] = new[] { '9', '8' },
            ['1'] = new[] { '7', '4' },
            ['2'] = new[] { '7', '4' },
            ['3'] = new[] { '8', '5' },
            ['4'] = new[] { '9', '2', '1' },
            ['5'] = new[] { '6', '3' },
            ['6'] = new[] { '5', '8' },
            ['7'] = new[] { '1', '2' },
            ['8'] = new[] { '0', '6', '3' },
            ['9'] = new[] { '0', '4' }
        };

        foreach (var item in items)
        {
            var priceStr = item.TotalPrice.ToString("F2");
            var priceDigits = priceStr.Replace(".", "");

            // Try single-digit corrections
            for (int digitPos = 0; digitPos < priceDigits.Length; digitPos++)
            {
                var currentDigit = priceDigits[digitPos];

                if (digitSubstitutions.TryGetValue(currentDigit, out var alternatives))
                {
                    foreach (var altDigit in alternatives)
                    {
                        var correctedDigits = priceDigits.ToCharArray();
                        correctedDigits[digitPos] = altDigit;

                        // Reconstruct price with decimal point
                        var correctedStr = new string(correctedDigits);
                        var correctedPrice = decimal.Parse($"{correctedStr.Substring(0, correctedStr.Length - 2)}.{correctedStr.Substring(correctedStr.Length - 2)}");

                        var priceDifference = correctedPrice - item.TotalPrice;

                        // Check if this correction would reduce the total discrepancy
                        if (Math.Abs(priceDifference - discrepancy) < 0.05m)
                        {
                            _logger.LogWarning("   💡 Potential OCR error in '{Item}': ${Original:F2} might be ${Corrected:F2} (digit '{Old}' → '{New}' at position {Pos})",
                                item.ItemName, item.TotalPrice, correctedPrice, currentDigit, altDigit, digitPos + 1);
                        }
                    }
                }
            }
        }
    }

    private string? ExtractMerchantName(List<string> lines)
    {
        // Merchant name is usually in first 3 lines
        // Look for lines with capitalized text and no numbers
        foreach (var line in lines.Take(5))
        {
            if (line.Length > 3 &&
                !line.Contains('$') &&
                !Regex.IsMatch(line, @"^\d") &&
                line.Any(char.IsLetter))
            {
                _logger.LogInformation("Extracted merchant name: {Name}", line);
                return line;
            }
        }
        return null;
    }

    private DateTime? ExtractDate(List<string> lines)
    {
        foreach (var line in lines)
        {
            // Try ISO format: YYYY-MM-DD or YYYY/MM/DD
            var isoMatch = IsoDatePattern().Match(line);
            if (isoMatch.Success)
            {
                if (DateTime.TryParse($"{isoMatch.Groups[1].Value}-{isoMatch.Groups[2].Value}-{isoMatch.Groups[3].Value}",
                    out var isoDate))
                {
                    // Convert to UTC for PostgreSQL compatibility
                    var utcDate = DateTime.SpecifyKind(isoDate, DateTimeKind.Utc);
                    _logger.LogInformation("Extracted date: {Date}", utcDate);
                    return utcDate;
                }
            }

            // Try labeled date: DATE: DD/MM/YYYY or DATE: DD-MM-YYYY
            var dateMatch = DatePattern().Match(line);
            if (dateMatch.Success)
            {
                var dateStr = dateMatch.Groups[1].Value;
                if (DateTime.TryParseExact(dateStr,
                    new[] { "dd/MM/yyyy", "dd-MM-yyyy", "dd/MM/yy", "dd-MM-yy" },
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var date))
                {
                    // Convert to UTC for PostgreSQL compatibility
                    var utcDate = DateTime.SpecifyKind(date, DateTimeKind.Utc);
                    _logger.LogInformation("Extracted date: {Date}", utcDate);
                    return utcDate;
                }
            }
        }
        return null;
    }

    private decimal? ExtractTotal(List<string> lines)
    {
        // Look for TOTAL keyword followed by amount (either on same line or next line)
        for (int i = 0; i < lines.Count; i++)
        {
            var line = lines[i];

            // Try to match total on same line
            var match = TotalPattern().Match(line);
            if (match.Success && decimal.TryParse(NormalizePrice(match.Groups[1].Value), out var total))
            {
                _logger.LogInformation("Extracted total: {Total}", total);
                return total;
            }

            // If line contains "TOTAL" but no amount, check next line
            if (line.ToLower().Contains("total") && i + 1 < lines.Count)
            {
                var nextLine = lines[i + 1];
                var priceMatch = PricePattern().Match(nextLine);
                if (priceMatch.Success && decimal.TryParse(NormalizePrice(priceMatch.Groups[1].Value), out var nextTotal))
                {
                    _logger.LogInformation("Extracted total from next line: {Total}", nextTotal);
                    return nextTotal;
                }
            }
        }
        return null;
    }

    private List<ParsedReceiptItem> ExtractLineItems(List<string> lines, List<OcrTextBlock> textBlocks)
    {
        var items = new List<ParsedReceiptItem>();
        var skipLines = new HashSet<int>();

        // Skip first 3 lines (header: merchant, address, date) and last 7 lines (footer: totals, payment)
        int startLine = Math.Min(3, lines.Count / 4);
        int endLine = Math.Max(lines.Count - 7, lines.Count * 3 / 4);

        // Calculate spatial layout to identify right-side text (price columns)
        var leftBoundary = CalculateLeftBoundary(textBlocks, startLine, endLine);
        var receiptWidth = CalculateReceiptWidth(textBlocks);

        _logger.LogInformation("Parsing items from line {Start} to {End} (total: {Total}). Left boundary: {Left}px, Receipt width: {Width}px",
            startLine, endLine, lines.Count, leftBoundary, receiptWidth);

        for (int i = startLine; i < endLine && i < lines.Count; i++)
        {
            if (skipLines.Contains(i))
                continue;

            var line = lines[i];
            _logger.LogInformation("Line {LineNum}: '{Line}'", i, line);

            // CRITICAL FILTERS: Skip unwanted lines

            // 1. Skip indented lines (sub-items like "- No pickles", "  Extra sauce")
            if (IsIndentedSubItem(line))
            {
                _logger.LogInformation("  → Skipping indented sub-item at line {LineNum}: '{Line}'", i, line);
                continue;
            }

            // 2. Skip header/footer/total lines
            if (IsHeaderFooterLine(line))
            {
                _logger.LogInformation("  → Skipping header/footer at line {LineNum}: '{Line}'", i, line);
                continue;
            }

            // 3. Skip lines that look like totals/subtotals
            if (IsTotalOrSubtotalLine(line))
            {
                _logger.LogInformation("  → Skipping total/subtotal at line {LineNum}: '{Line}'", i, line);
                continue;
            }

            // 4. Skip English translations that come right after "+++" indented sub-items (like "+++百香果气泡水 $0" followed by "Passion Fruit Soda")
            // These are translation lines, not separate items
            // IMPORTANT: Only apply this filter for "+++" style sub-items (common in Asian receipts), not all sub-items
            if (i > startLine)
            {
                var prevLine = lines[i - 1];
                var prevLineTrimmed = prevLine.Trim();

                // Check if previous line is specifically a "+++" style sub-item (not just any indented line)
                if (prevLineTrimmed.StartsWith("+++") || prevLineTrimmed.StartsWith("+ + +"))
                {
                    // Previous line was a "+++" sub-item (e.g., "+++百香果气泡水 $0")
                    // Check if current line is a translation (no quantity prefix, no price on same line)
                    var trimmed = line.Trim();
                    var hasQuantityPrefix = Regex.IsMatch(trimmed, @"^[1-9Il:]\s");
                    var hasPriceOnLine = HasPriceOnSameLine(line);

                    if (!hasQuantityPrefix && !hasPriceOnLine)
                    {
                        _logger.LogInformation("  → Skipping translation line after +++ sub-item at line {LineNum}: '{Line}'", i, line);
                        continue;
                    }
                }
            }

            // Try to parse line with format: Qty Item Price
            var qtyItemPriceMatch = QuantityItemPricePattern().Match(line);
            _logger.LogInformation("  → QuantityItemPricePattern match: {Success}", qtyItemPriceMatch.Success);
            if (qtyItemPriceMatch.Success)
            {
                var price = decimal.Parse(NormalizePrice(qtyItemPriceMatch.Groups[3].Value));

                // Validate price is reasonable (not a total)
                if (!IsValidItemPrice(price))
                {
                    _logger.LogDebug("Skipping line {LineNum} - price {Price} looks like a total", i, price);
                    continue;
                }

                var itemName = CleanItemName(qtyItemPriceMatch.Groups[2].Value);

                // Skip if item name is invalid (just symbols, too short, etc.)
                if (!IsValidItemName(itemName))
                {
                    _logger.LogDebug("Skipping line {LineNum} - invalid item name: '{Name}'", i, itemName);
                    continue;
                }

                // Skip if item name contains total-like keywords
                if (ContainsTotalKeywords(itemName))
                {
                    _logger.LogDebug("Skipping line {LineNum} - item name contains total keywords: '{Name}'", i, itemName);
                    continue;
                }

                items.Add(new ParsedReceiptItem
                {
                    Quantity = int.Parse(qtyItemPriceMatch.Groups[1].Value),
                    ItemName = itemName,
                    TotalPrice = price,
                    LineNumber = i,
                    Confidence = GetLineConfidence(textBlocks, i)
                });

                _logger.LogDebug("Extracted item at line {LineNum}: {Qty}x {Name} = ${Price}",
                    i, qtyItemPriceMatch.Groups[1].Value, itemName, price);
                continue;
            }

            // Try to find item and price on separate lines
            // IMPORTANT: Only do this if the item line doesn't already contain a price
            var looksLikeItem = LooksLikeItemName(line);
            var hasPriceOnSame = HasPriceOnSameLine(line);

            // Special case: If line contains food keywords but no quantity, still try to extract
            // (OCR might have missed the quantity)
            var hasFoodKeyword = !looksLikeItem && ContainsItemKeywords(line) &&
                                 !IsHeaderFooterLine(line) && !IsTotalOrSubtotalLine(line) &&
                                 !IsIndentedSubItem(line);

            _logger.LogInformation("  → LooksLikeItemName: {LooksLike}, HasPriceOnSameLine: {HasPrice}, HasFoodKeyword: {HasFood}",
                                  looksLikeItem, hasPriceOnSame, hasFoodKeyword);

            if ((looksLikeItem || hasFoodKeyword) && !hasPriceOnSame)
            {
                _logger.LogInformation("  → Looking ahead for modifiers and price...");

                // First, collect all modifier/continuation lines that belong to this item
                // AND track any price line we encounter
                var modifierLines = new List<string>();
                int modifierEndLine = i;
                int? foundPriceLine = null;
                decimal? foundPrice = null;
                string prevLineForModifier = line; // Track the previous line for context

                for (int j = i + 1; j < Math.Min(i + 10, lines.Count); j++)
                {
                    var nextLine = lines[j];

                    // Skip already claimed lines
                    if (skipLines.Contains(j))
                        break;

                    // Stop if we hit a line with quantity (new item)
                    // OCR errors: ":" "I" "l" might be misread "1"
                    if (Regex.IsMatch(nextLine.Trim(), @"^[1-9Il:]\s"))
                    {
                        _logger.LogInformation("  → Line {J} has quantity, stopping modifier collection", j);
                        break;
                    }

                    // Stop if we hit header/footer/total lines
                    if (IsIndentedSubItem(nextLine) || IsHeaderFooterLine(nextLine) || IsTotalOrSubtotalLine(nextLine))
                    {
                        _logger.LogInformation("  → Line {J} is sub-item/header/total, stopping modifier collection", j);
                        break;
                    }

                    // Check if this is a modifier/continuation line (pass previous line for context)
                    if (IsModifierOrContinuationLine(nextLine, prevLineForModifier))
                    {
                        // CRITICAL: Before treating as modifier, check if next line is a price
                        // If so, this line is actually a separate item with its own price
                        // Example: "Make It Blue" followed by "$5.00" should be separate item
                        bool hasOwnPrice = false;
                        if (j + 1 < lines.Count && !skipLines.Contains(j + 1))
                        {
                            var lineAfterModifier = lines[j + 1];
                            if (IsPriceOnlyLine(lineAfterModifier))
                            {
                                var priceMatch = PricePattern().Match(lineAfterModifier);
                                if (priceMatch.Success && decimal.TryParse(NormalizePrice(priceMatch.Groups[1].Value), out var priceValue))
                                {
                                    if (IsValidItemPrice(priceValue))
                                    {
                                        hasOwnPrice = true;
                                        _logger.LogInformation("  → Line {J} has own price on next line (${Price}), treating as separate item not modifier", j, priceValue);
                                    }
                                }
                            }
                        }

                        if (hasOwnPrice)
                        {
                            // This line has its own price, so it's a separate item, not a modifier
                            // Stop collecting modifiers here
                            _logger.LogInformation("  → Stopping modifier collection before line {J} (has own price)", j);
                            break;
                        }
                        else
                        {
                            _logger.LogInformation("  → Line {J} is modifier/continuation: '{Line}'", j, nextLine);
                            modifierLines.Add(nextLine);
                            modifierEndLine = j;
                            skipLines.Add(j); // Mark as consumed
                            prevLineForModifier = nextLine; // Update for next iteration
                        }
                    }
                    // CRITICAL: Track price lines during modifier collection
                    else if (IsPriceOnlyLine(nextLine))
                    {
                        var priceMatch = PricePattern().Match(nextLine);
                        if (priceMatch.Success && decimal.TryParse(NormalizePrice(priceMatch.Groups[1].Value), out var priceValue))
                        {
                            if (IsValidItemPrice(priceValue) && foundPriceLine == null)
                            {
                                foundPriceLine = j;
                                foundPrice = priceValue;
                                _logger.LogInformation("  → Line {J} is price-only (${Price}), tracking for later use", j, priceValue);
                            }
                        }
                        // Don't break - keep looking for more modifiers after the price
                    }
                    else
                    {
                        // Non-modifier, non-price line - stop collecting
                        _logger.LogInformation("  → Line {J} is neither modifier nor price, stopping modifier collection", j);
                        break;
                    }
                }

                // If we found a price during modifier collection, use it
                // Otherwise, search for price after modifiers
                decimal? price = null;
                int? priceLineNumber = null;

                if (foundPrice.HasValue && foundPriceLine.HasValue)
                {
                    price = foundPrice.Value;
                    priceLineNumber = foundPriceLine.Value;
                    _logger.LogInformation("  → Using price found during modifier collection: ${Price} from line {Line}", price, priceLineNumber);
                }
                else
                {
                    // Search for price after modifiers
                    for (int j = modifierEndLine + 1; j < Math.Min(modifierEndLine + 4, lines.Count); j++)
                    {
                        var nextLine = lines[j];

                    // Skip lines that have already been claimed as prices by previous items
                    if (skipLines.Contains(j))
                    {
                        _logger.LogInformation("  → Line {J} already claimed, skipping in look-ahead", j);
                        continue;
                    }

                    // Skip sub-items, header/footer lines when looking for item prices
                    if (IsIndentedSubItem(nextLine) || IsHeaderFooterLine(nextLine) || IsTotalOrSubtotalLine(nextLine))
                    {
                        _logger.LogInformation("  → Line {J} is sub-item/header/total, skipping in look-ahead", j);
                        continue;
                    }

                    // CRITICAL: If we encounter a line with quantity, stop looking for price
                    // This prevents description lines from stealing prices that belong to upcoming items
                    // Example: "Pelu Cabernet Sauvignon" shouldn't steal price from "1 SAND GOLD CHIX"
                    // OCR errors: ":" "I" "l" might be misread "1"
                    if (Regex.IsMatch(nextLine.Trim(), @"^[1-9Il:]\s"))
                    {
                        _logger.LogInformation("  → Line {J} has quantity, stopping price search (price likely belongs to that line)", j);
                        break;
                    }

                        // Check if this line contains a price
                        var priceMatch = PricePattern().Match(nextLine);
                        if (priceMatch.Success && decimal.TryParse(NormalizePrice(priceMatch.Groups[1].Value), out var foundPriceValue))
                        {
                            _logger.LogInformation("  → Found potential price on line {J}: ${Price}", j, foundPriceValue);

                            // Validate price is reasonable for a single item
                            if (!IsValidItemPrice(foundPriceValue))
                            {
                                _logger.LogInformation("  → Price {Price} exceeds max item price, likely a total", foundPriceValue);
                                break; // Stop looking for prices for this item
                            }

                            // Check if price looks like a cumulative subtotal (matches sum of items so far)
                            var currentTotal = items.Sum(item => item.TotalPrice);
                            if (currentTotal > 0 && Math.Abs(foundPriceValue - currentTotal) < 0.11m)
                            {
                                _logger.LogInformation("  → Price {Price} matches cumulative total {Total}, likely a subtotal", foundPriceValue, currentTotal);
                                break; // This is a running subtotal, not an item price
                            }

                            price = foundPriceValue;
                            priceLineNumber = j;
                            break; // Found a valid price, stop looking
                        }
                    }
                }

                // If we found a price (either during modifier collection or after), extract the item
                if (price.HasValue && priceLineNumber.HasValue)
                {
                    // Extract quantity if present
                    int quantity = 1;
                    var itemName = line;
                    var qtyMatch = QuantityItemPattern().Match(line);
                    if (qtyMatch.Success)
                    {
                        var qtyStr = qtyMatch.Groups[1].Value;
                        // Handle OCR errors: "I", "l", ":" are often misread "1"
                        if (qtyStr == "I" || qtyStr == "l" || qtyStr == ":" || qtyStr == "i")
                            quantity = 1;
                        else
                            quantity = int.Parse(qtyStr);
                        itemName = qtyMatch.Groups[3].Value; // Group 3 now (group 2 is the space)
                    }

                    // Combine item name with modifiers
                    var fullItemName = itemName;
                    if (modifierLines.Any())
                    {
                        fullItemName = itemName + " " + string.Join(" ", modifierLines);
                        _logger.LogInformation("  → Combined item name with {Count} modifiers: '{Name}'", modifierLines.Count, fullItemName);
                    }

                    var cleanedName = CleanItemName(fullItemName);

                    // Skip if item name is invalid (just symbols, too short, etc.)
                    if (!IsValidItemName(cleanedName))
                    {
                        _logger.LogDebug("Skipping line {LineNum} - invalid item name: '{Name}'", i, cleanedName);
                    }
                    // Skip if item name contains total keywords
                    else if (ContainsTotalKeywords(cleanedName))
                    {
                        _logger.LogDebug("Skipping line {LineNum} - item name contains total keywords: '{Name}'", i, cleanedName);
                    }
                    else
                    {
                        items.Add(new ParsedReceiptItem
                        {
                            Quantity = quantity,
                            ItemName = cleanedName,
                            TotalPrice = price.Value,
                            LineNumber = i,
                            Confidence = GetLineConfidence(textBlocks, i)
                        });

                        _logger.LogInformation("  → ✅ Extracted item: {Qty}x {Name} = ${Price}",
                            quantity, cleanedName, price.Value);

                        skipLines.Add(priceLineNumber.Value); // Skip the price line
                    }
                }
            }
        }

        _logger.LogInformation("Successfully extracted {Count} items after filtering", items.Count);

        // Deduplicate items: Sum quantities and prices for items with same name
        var deduplicatedItems = DeduplicateItems(items);
        _logger.LogInformation("After deduplication: {Count} unique items", deduplicatedItems.Count);

        return deduplicatedItems;
    }

    /// <summary>
    /// Deduplicates items by combining those with the same name
    /// Sums quantities and total prices for duplicate items
    /// </summary>
    /// <param name="items">List of parsed items (may contain duplicates)</param>
    /// <returns>Deduplicated list where duplicate items are combined</returns>
    private List<ParsedReceiptItem> DeduplicateItems(List<ParsedReceiptItem> items)
    {
        if (!items.Any())
            return items;

        // Group by normalized item name (case-insensitive, trimmed)
        var deduplicatedItems = items
            .GroupBy(i => i.ItemName.Trim().ToLowerInvariant())
            .Select(group =>
            {
                var firstItem = group.First();
                var totalQuantity = group.Sum(i => i.Quantity);
                var totalPrice = group.Sum(i => i.TotalPrice);
                var avgConfidence = group.Average(i => i.Confidence);

                if (group.Count() > 1)
                {
                    _logger.LogInformation("  → Merged {Count} duplicate '{Name}' items: {OldQty} items → {NewQty}x, ${OldTotal} → ${NewTotal}",
                        group.Count(),
                        firstItem.ItemName,
                        string.Join(", ", group.Select(i => $"{i.Quantity}x")),
                        totalQuantity,
                        string.Join(" + ", group.Select(i => i.TotalPrice.ToString("F2"))),
                        totalPrice.ToString("F2"));
                }

                return new ParsedReceiptItem
                {
                    ItemName = firstItem.ItemName, // Use original capitalization from first occurrence
                    Quantity = totalQuantity,
                    UnitPrice = totalQuantity > 0 ? totalPrice / totalQuantity : null,
                    TotalPrice = totalPrice,
                    LineNumber = firstItem.LineNumber, // Use line number of first occurrence
                    Confidence = (decimal)avgConfidence // Average confidence across duplicates
                };
            })
            .OrderBy(i => i.LineNumber) // Maintain original order
            .ToList();

        return deduplicatedItems;
    }

    /// <summary>
    /// Detects if a line is indented (sub-item) by checking for leading spaces/tabs or special characters
    /// Examples: "  - No pickles", "    Extra sauce", "• Add cheese", "[Chicken] [5*][Small Rice]"
    /// </summary>
    private bool IsIndentedSubItem(string line)
    {
        if (string.IsNullOrEmpty(line))
            return false;

        // Check for leading whitespace (2+ spaces or tabs indicate indentation)
        if (line.Length > 0 && (line.StartsWith("  ") || line.StartsWith("\t")))
            return true;

        // Check for common sub-item prefixes
        var trimmed = line.TrimStart();
        if (trimmed.StartsWith("-") ||
            trimmed.StartsWith("+") ||  // Common in Asian receipts (+++item means addon)
            trimmed.StartsWith("•") ||
            trimmed.StartsWith("*") ||
            trimmed.StartsWith(">") ||
            trimmed.StartsWith("○") ||
            trimmed.StartsWith("·"))
            return true;

        // Check for bracket notation (common for options/modifiers like "[Chicken] [5*] [Small]")
        if (trimmed.StartsWith("["))
            return true;

        // Check for parenthetical quantity notes like "(2) BMOD Up", "(3) Extra Sauce"
        // These are modifier notes that apply to previous items, not separate billable items
        // Pattern: starts with "(digit)" or "(digit+)" like "(2)", "(10)", etc.
        if (Regex.IsMatch(trimmed, @"^\(\d+\)\s+"))
            return true;

        // Check for modification keywords (usually sub-items)
        // IMPORTANT: Only if the line doesn't start with actual indentation
        // Lines like "Add Lobster" with their own price should not be treated as sub-items
        var lower = trimmed.ToLower();
        if (lower.StartsWith("no ") ||
            lower.StartsWith("extra ") ||
            lower.StartsWith("less ") ||
            lower.StartsWith("with ") ||
            lower.StartsWith("without ") ||
            lower.StartsWith("more ") ||
            lower.StartsWith("con ") ||      // Spanish: "with" (e.g., "CON TODO", "CON CEBOLLA")
            lower.StartsWith("sin ") ||      // Spanish: "without"
            lower.StartsWith("copa ") ||     // Spanish: "cup/glass" (drink/sauce options)
            lower.Contains(" copa "))        // Handle "S. COPA" or middle-position copa
            return true;

        // NOTE: Removed "add " from keyword check - "Add Lobster" with its own price
        // should be a separate item, not a sub-item

        return false;
    }

    /// <summary>
    /// Detects if a line is a modifier or continuation of the previous item
    /// Common patterns: "W/ DRESSING", "SD SALAD", "DRESS ON SIDE", brand names, etc.
    /// </summary>
    private bool IsModifierOrContinuationLine(string line, string? previousLine = null)
    {
        if (string.IsNullOrEmpty(line))
            return false;

        var trimmed = line.Trim();
        var lower = trimmed.ToLower();

        // IMPORTANT: Lines starting with quantity (e.g., "1 SD BRUSSEL SPRTS") are NOT modifiers
        // Also check for OCR errors: ":", "I", or "l" (lowercase L) might be misread "1"
        if (Regex.IsMatch(trimmed, @"^[1-9Il:]\s"))
            return false;

        // Lines that start with "W/" or "W /" (with/without modifiers) are clear modifiers
        if (lower.StartsWith("w/") || lower.StartsWith("w /"))
            return true;

        // Lines that start with common modifier prefixes (without quantity)
        // But be more restrictive - only if they're actually modifier-like
        if (lower.StartsWith("dress ") ||       // Dressing: "DRESS ON SIDE", "DRESS GW SIDE"
            lower.StartsWith("dressing ") ||
            lower.StartsWith("sauce ") ||
            lower.StartsWith("on side") ||
            lower.StartsWith("on the side"))
            return true;

        // "SD" prefix is tricky - could be "Side" or part of an item name
        // Only treat as modifier if it's clearly a side dish AND short
        if (lower.StartsWith("sd ") && trimmed.Length < 15)
            return true;

        // Brand/description lines: If previous line had quantity, and this line is descriptive
        // Example: "1 CAB PEJU SAUY S" followed by "Pelu Cabernet Sauvignon"
        // OCR errors: ":", "I", or "l" might be misread "1"
        if (previousLine != null && Regex.IsMatch(previousLine.Trim(), @"^[1-9Il:]\s"))
        {
            // This line is a description following a quantity line
            // Treat as modifier if it's medium length and mostly letters
            if (trimmed.Length < 30 &&
                !HasPriceOnSameLine(trimmed) &&
                !Regex.IsMatch(trimmed, @"^\d+[.,]\d{2}$") &&
                !ContainsItemKeywords(trimmed))
            {
                var letterCount = trimmed.Count(char.IsLetter);
                if (letterCount > trimmed.Length * 0.6) // 60% letters
                    return true;
            }
        }

        // Short lines (< 15 chars) that look like brand names or descriptions
        // Be more conservative to avoid false positives
        if (trimmed.Length < 15 &&
            !HasPriceOnSameLine(trimmed) &&       // Doesn't have a price
            !Regex.IsMatch(trimmed, @"^\d+[.,]\d{2}$") && // Not just a standalone price
            !ContainsItemKeywords(trimmed)) // Doesn't contain item keywords like "SHRIMP", "SALAD", etc.
        {
            // Only if it contains mostly letters and no common item words
            var letterCount = trimmed.Count(char.IsLetter);
            if (letterCount > trimmed.Length * 0.7) // Increased threshold to 70%
                return true;
        }

        return false;
    }

    /// <summary>
    /// Checks if a line contains keywords that suggest it's a food item (not a modifier)
    /// </summary>
    private bool ContainsItemKeywords(string line)
    {
        var lower = line.ToLower();

        // Common food item keywords that indicate this is likely a main item, not a modifier
        var itemKeywords = new[]
        {
            // Main dishes
            "salad", "soup", "sandwich", "burger", "pizza", "pasta",
            "chicken", "beef", "pork", "fish", "shrimp", "salmon", "steak",
            "rice", "noodles", "wrap", "taco", "burrito", "quesadilla",

            // Sides (often appear as separate items)
            "fries", "wings", "nachos", "asparagus", "brussel", "broccoli",
            "potato", "potatoes", "coleslaw", "beans", "corn",

            // Desserts
            "cake", "cookie", "pie", "ice cream", "brownie", "cheesecake",

            // Drinks (sometimes itemized separately)
            "margarita", "cocktail", "beer", "wine", "soda", "juice",
            "drink", "fountain", "draft",

            // Abbreviations
            "sld", "sand", "nac", // salad, sandwich, nachos
            "mod", // MOD Pizza chain items
        };

        return itemKeywords.Any(keyword => lower.Contains(keyword));
    }

    /// <summary>
    /// Detects if a line contains total/subtotal/tax keywords
    /// </summary>
    private bool IsTotalOrSubtotalLine(string line)
    {
        var lower = line.ToLower();

        // English keywords
        if (lower.Contains("total") ||
            lower.Contains("subtotal") ||
            lower.Contains("sub-total") ||
            lower.Contains("sub total") ||
            lower.Contains("grand total") ||
            lower.Contains("amount due") ||
            lower.Contains("balance") ||
            lower.Contains("tax") ||
            lower.Contains("gst") ||
            lower.Contains("vat") ||
            lower.Contains("service charge") ||
            lower.Contains("s/c") ||
            lower.Contains("discount") ||
            lower.Contains("rounding"))
            return true;

        // Chinese keywords for totals (common in Singapore)
        if (lower.Contains("总计") ||  // Total
            lower.Contains("小计") ||  // Subtotal
            lower.Contains("合计") ||  // Sum
            lower.Contains("税") ||    // Tax
            lower.Contains("折扣"))    // Discount
            return true;

        return false;
    }

    /// <summary>
    /// Checks if item name contains keywords that suggest it's a total, not an item
    /// </summary>
    private bool ContainsTotalKeywords(string itemName)
    {
        var lower = itemName.ToLower();

        // Single-word totals
        if (lower == "total" ||
            lower == "subtotal" ||
            lower == "tax" ||
            lower == "gst" ||
            lower == "discount" ||
            lower == "change" ||
            lower == "cash" ||
            lower == "payment")
            return true;

        // Multi-word totals
        return IsTotalOrSubtotalLine(itemName);
    }

    /// <summary>
    /// Validates that an item name is reasonable (not just symbols or garbage)
    /// </summary>
    private bool IsValidItemName(string itemName)
    {
        if (string.IsNullOrWhiteSpace(itemName))
            return false;

        var trimmed = itemName.Trim();

        // Item name should be at least 2 characters (even "AA" drink or "XO" sauce is 2 chars)
        if (trimmed.Length < 2)
            return false;

        // Must contain at least one letter (to exclude pure symbols like "×", "+", "-")
        if (!trimmed.Any(char.IsLetter))
            return false;

        // Reject items that are ONLY symbols and whitespace (like "×", "× ×", "+ +")
        var symbolsOnly = trimmed.All(c => !char.IsLetterOrDigit(c));
        if (symbolsOnly)
            return false;

        // Reject single symbol items common in European receipts (×, +, -, =, etc.)
        if (trimmed.Length <= 3 && trimmed.Contains('×'))
            return false;

        return true;
    }

    /// <summary>
    /// Validates if a price is reasonable for an item (not a total)
    /// Helps filter out subtotals/totals that were incorrectly parsed as items
    /// </summary>
    private bool IsValidItemPrice(decimal price)
    {
        // Allow $0.00 for comped/free items
        if (price == 0)
            return true;

        // Price should be positive
        if (price < 0)
            return false;

        // Most receipt items are under $200 (configurable threshold)
        // Totals/subtotals are often higher
        const decimal MAX_REASONABLE_ITEM_PRICE = 200.00m;
        if (price > MAX_REASONABLE_ITEM_PRICE)
        {
            _logger.LogDebug("Price {Price} exceeds reasonable item threshold", price);
            return false;
        }

        // Very small prices (< $0.10) are unusual and might be errors
        // But allow $0.00 for comped items
        if (price > 0 && price < 0.10m)
        {
            _logger.LogDebug("Price {Price} is unusually small", price);
            return false;
        }

        return true;
    }

    private bool IsHeaderFooterLine(string line)
    {
        var lower = line.ToLower();
        var trimmed = line.Trim();

        // Column headers on receipts (should not be treated as items)
        // Common headers: Price, Qty, Item, Amount, Description, etc.
        if (lower == "price" ||
            lower == "qty" ||
            lower == "item" ||
            lower == "qty item" ||
            lower == "quantity" ||
            lower == "amount" ||
            lower == "description" ||
            lower == "unit price" ||
            lower == "total price")
            return true;

        // Store location patterns (e.g., "pandamart (Punggol)", "Starbucks (Downtown)")
        // Matches: word(s) followed by (Location) where Location is capitalized word(s)
        // IMPORTANT: Only match if it has Latin characters to avoid filtering Chinese text
        if (trimmed.Any(c => c >= 'A' && c <= 'Z' || c >= 'a' && c <= 'z') &&
            Regex.IsMatch(trimmed, @"^[\w\s]+\([A-Z][a-z]+(?:\s+[A-Z][a-z]+)?\)$"))
            return true;

        // UI elements from food delivery apps (buttons, links, action text)
        if (lower == "help" ||
            lower == "view details" ||
            lower.StartsWith("view details") ||
            lower == "your order" ||
            lower.Contains("(") && lower.Contains("items)"))  // "(13 items)", "(X items)"
            return true;

        // Fee-related lines (not food items)
        if (lower.Contains("platform fee") ||
            lower.Contains("service fee") ||
            lower.Contains("convenience fee") ||
            lower.Contains("delivery fee") ||
            lower.Contains("booking fee") ||
            lower.Contains("processing fee") ||
            lower.Contains("small order fee"))
            return true;

        // Common non-item indicator words (delivery status, promotional text)
        if (lower == "free" ||
            lower == "promo" ||
            lower == "discount applied" ||
            lower == "applied")
            return true;

        // Delivery/order type indicators
        if (lower == "standard delivery" ||
            lower == "express delivery" ||
            lower == "priority delivery" ||
            lower == "scheduled delivery")
            return true;

        // Order/table numbers (e.g., "#77 - HERE", "Order #123", "Table 5")
        if (Regex.IsMatch(trimmed, @"^#\d+\s*-?\s*(HERE|TO[-\s]?GO|DINE[-\s]?IN)?$", RegexOptions.IgnoreCase))
            return true;

        // Receipt metadata patterns (server, guests, reprint, table info)
        // These commonly appear near the top of receipts and should not be treated as items
        if (Regex.IsMatch(trimmed, @"^(server|cashier|clerk|employee|staff|waiter|waitress)[:\s]+", RegexOptions.IgnoreCase) ||
            Regex.IsMatch(trimmed, @"^(guest|guests|party|people|ppl|covers?)[:\s]+", RegexOptions.IgnoreCase) ||
            Regex.IsMatch(trimmed, @"^(reprint|copy|duplicate)[:\s#]+", RegexOptions.IgnoreCase) ||
            Regex.IsMatch(trimmed, @"^(member|membership|invoice|pickup)[:\s]+", RegexOptions.IgnoreCase) ||  // Member info, invoice numbers, pickup numbers
            Regex.IsMatch(trimmed, @"^table[\s#]+\d+", RegexOptions.IgnoreCase) ||
            Regex.IsMatch(trimmed, @"^\(\d{3}\)\s*\d{3}[-\s]?\d{4}") ||  // Phone numbers like (858) 488-7311
            Regex.IsMatch(trimmed, @"^\d{3}[-.\s]\d{3}[-.\s]\d{4}"))     // Alternative phone format 858-488-7311
            return true;

        // Membership and policy text
        if (lower.Contains("member") && (lower.Contains("consumption") || lower.Contains("policy") || lower.Contains("preferential")))
            return true;

        // Thank you messages, receipts headers
        if (lower.Contains("thank you") ||
            lower.Contains("thanks") ||
            lower.Contains("visit") ||
            lower.Contains("receipt") ||
            lower.Contains("invoice") ||
            lower.Contains("welcome") ||
            lower.Contains("store") ||
            lower.Contains("branch") ||
            lower.Contains("exquisita") ||      // Spanish taglines like "Exquisitamente..."
            lower.Contains("mexicana") ||       // "a la Mexicana" style taglines
            lower.Contains("delicious") ||
            lower.Contains("dine in") ||         // "DINE IN" service type (OCR might read as "DIN! IA")
            lower.Contains("din! ia") ||         // OCR misread of "DINE IN"
            lower.Contains("din!") ||            // Partial OCR error
            lower.Contains("take out") ||
            lower.Contains("takeout"))
            return true;

        // Lines with excessive punctuation (taglines/decorations like "!!!!!!!!!")
        if (line.Count(c => c == '!' || c == '*' || c == '#') > 3)
            return true;

        // Payment/transaction info
        if (lower.Contains("payment") ||
            lower.Contains("card") ||
            lower.Contains("cash") ||
            lower.Contains("change") ||
            lower.Contains("tender") ||
            lower.Contains("paid") ||
            lower == "visa" ||
            lower == "mastercard" ||
            lower == "amex" ||
            lower == "discover" ||
            lower.Contains("acct:") ||
            lower.Contains("auth:") ||
            lower.Contains("trans") ||
            lower.Contains("to-go") ||
            lower.Contains("70-go") ||          // OCR often misreads "TO-GO" as "70-GO"
            lower.Contains("t0-go") ||          // or "T0-GO"
            lower.Contains("dine") ||
            lower.Contains("pickup") ||
            lower.Contains("delivery"))
            return true;

        // Separator lines
        if (lower.StartsWith("===") ||
            lower.StartsWith("---") ||
            lower.StartsWith("***") ||
            lower.All(c => c == '=' || c == '-' || c == '*' || char.IsWhiteSpace(c)))
            return true;

        // Contact info
        if (lower.Contains("tel:") ||
            lower.Contains("phone:") ||
            lower.Contains("email:") ||
            lower.Contains("website:") ||
            lower.Contains("www."))
            return true;

        return false;
    }

    private bool LooksLikeItemName(string line)
    {
        // Item names typically:
        // - Have letters or Chinese characters
        // - Are not just numbers
        // - Don't contain payment-related keywords
        // - Are at least 3 characters long
        // - Are not indented (not sub-items)
        // - Don't contain total/subtotal keywords
        // - Are not dates/times or order numbers

        if (line.Length < 3)
            return false;

        // Not just a number
        if (Regex.IsMatch(line, @"^\d+\.?\d*$"))
            return false;

        // Skip short parenthetical lines like "(T.1)", "(A)", etc. - these are usually transaction/table references
        var trimmed = line.Trim();
        if (trimmed.StartsWith("(") && trimmed.EndsWith(")") && trimmed.Length < 10)
            return false;

        // Skip parenthetical quantity notes like "(2) BMOD Up", "(3) Extra Sauce"
        // These are modifier notes that apply to previous items, not separate billable items
        // Pattern: starts with "(digit)" or "(digit+)" like "(2)", "(10)", etc.
        if (Regex.IsMatch(trimmed, @"^\(\d+\)\s+"))
            return false;

        // Not a date/time pattern (these commonly appear on receipts and get misidentified as items)
        // Common patterns:
        // - "27-Apr-2017 6:21:59P" (date with time)
        // - "TUE JANUARY 30,2018" (day of week + month name + date)
        // - "2023-04-15 14:30:00" (ISO datetime)
        // - "04/15/2023" or "15/04/2023" (date only)
        // - "14:30:00" or "2:30PM" (time only)
        // - "ORDER: #12345" or "Order #123"
        // - "CHECK #278470-1" or "Check 123"
        // - "Table 5", "TABLE #6", or "TABLEl" (OCR misread of "TABLE1")
        if (Regex.IsMatch(trimmed, @"\d{1,2}[-/]\w{3}[-/]\d{2,4}") ||                      // 27-Apr-2017
            Regex.IsMatch(trimmed, @"\d{4}[-/]\d{1,2}[-/]\d{1,2}") ||                     // 2023-04-15
            Regex.IsMatch(trimmed, @"\d{1,2}[-/]\d{1,2}[-/]\d{2,4}") ||                   // 04/15/2023 or 15/04/2023
            Regex.IsMatch(trimmed, @"(MON|TUE|WED|THU|FRI|SAT|SUN)", RegexOptions.IgnoreCase) || // Day of week
            Regex.IsMatch(trimmed, @"(JANUARY|FEBRUARY|MARCH|APRIL|MAY|JUNE|JULY|AUGUST|SEPTEMBER|OCTOBER|NOVEMBER|DECEMBER)", RegexOptions.IgnoreCase) || // Full month name
            Regex.IsMatch(trimmed, @"\d{1,2}:\d{2}(:\d{2})?[AP]?M?", RegexOptions.IgnoreCase) || // 14:30:00 or 2:30PM
            Regex.IsMatch(trimmed, @"^ORDER[:\s#]", RegexOptions.IgnoreCase) ||            // ORDER: #123
            Regex.IsMatch(trimmed, @"^CHECK[:\s#]", RegexOptions.IgnoreCase) ||            // CHECK #278470-1
            Regex.IsMatch(trimmed, @"^TABLE[\s#]*\d+", RegexOptions.IgnoreCase))           // Table 5, TABLE #6
            return false;

        // Not UI elements or metadata patterns from delivery apps
        // - "View details (13 items)" - UI action with item count
        // - "pandamart (Location)" - Store name with location
        // - Parenthetical item counts like "(13 items)", "(5 items)"
        var lower = trimmed.ToLower();
        if (lower.StartsWith("view details") ||
            lower.StartsWith("view order") ||
            lower.StartsWith("see details") ||
            Regex.IsMatch(trimmed, @"^\(\d+\s+items?\)$", RegexOptions.IgnoreCase))      // (13 items), (1 item)
            return false;

        // Store location pattern - only apply if it has Latin characters (avoid filtering Chinese text)
        if (trimmed.Any(c => c >= 'A' && c <= 'Z' || c >= 'a' && c <= 'z') &&
            Regex.IsMatch(trimmed, @"^[\w\s]+\([A-Z][a-z]+(?:\s+[A-Z][a-z]+)?\)$"))       // Store (Location)
            return false;

        // Not indented (sub-item)
        if (IsIndentedSubItem(line))
            return false;

        // Not header/footer
        if (IsHeaderFooterLine(line))
            return false;

        // Not total/subtotal line
        if (IsTotalOrSubtotalLine(line))
            return false;

        // Must contain at least one letter or Chinese character
        return line.Any(c => char.IsLetter(c) || c >= 0x4E00 && c <= 0x9FFF);
    }

    /// <summary>
    /// Checks if a line already contains a price at the END of it
    /// Example: "1x Chicken Rice $5.50" has a price at end, "Chang (2@$3.50)" does not (has closing paren)
    /// </summary>
    private bool HasPriceOnSameLine(string line)
    {
        // Check if line contains a price AT THE END (after trimming whitespace)
        // This way "Chang (2@$3.50)" won't match (ends with ")"), but "1x Rice $5.50" will match
        return Regex.IsMatch(line.TrimEnd(), @"[a-zA-Z\u4E00-\u9FFF].+\$?[0-9]+[.,][0-9]{2}$");
    }

    /// <summary>
    /// Checks if a line contains ONLY a price (and maybe currency symbol/whitespace)
    /// Example: "$5.50" or "  3.50" returns true, "Subtotal $5.50" returns false
    /// </summary>
    private bool IsPriceOnlyLine(string line)
    {
        // Line should be mostly just the price
        // Remove price, currency symbols, whitespace - what's left should be minimal
        var withoutPrice = Regex.Replace(line, @"\$?[0-9]+\.[0-9]{2}", "");
        var withoutWhitespace = Regex.Replace(withoutPrice, @"\s+", "");
        var withoutCurrency = withoutWhitespace.Replace("$", "");

        // If there's significant text left (> 2 chars), it's not a price-only line
        return withoutCurrency.Length <= 2;
    }

    private string CleanItemName(string itemName)
    {
        // Remove extra whitespace and special characters
        return Regex.Replace(itemName, @"\s+", " ").Trim();
    }

    /// <summary>
    /// Normalizes a price string by replacing comma decimal separators with periods
    /// Example: "3,19" → "3.19", "2.99" → "2.99"
    /// </summary>
    private string NormalizePrice(string priceString)
    {
        // Replace comma with period for international receipt support
        return priceString.Replace(',', '.');
    }

    private decimal GetLineConfidence(List<OcrTextBlock> textBlocks, int lineNumber)
    {
        if (lineNumber < textBlocks.Count)
            return textBlocks[lineNumber].Confidence;
        return 0.7m; // Default confidence
    }

    /// <summary>
    /// Gets the leftmost X coordinate from a bounding polygon
    /// </summary>
    private int? GetLeftEdgeX(OcrTextBlock block)
    {
        if (block.BoundingPolygon == null || !block.BoundingPolygon.Any())
            return null;

        return block.BoundingPolygon.Min(p => p.X);
    }

    /// <summary>
    /// Gets the rightmost X coordinate from a bounding polygon
    /// </summary>
    private int? GetRightEdgeX(OcrTextBlock block)
    {
        if (block.BoundingPolygon == null || !block.BoundingPolygon.Any())
            return null;

        return block.BoundingPolygon.Max(p => p.X);
    }

    /// <summary>
    /// Calculates the typical left boundary where item names start
    /// Uses the median left edge of text blocks to determine the "main content area"
    /// </summary>
    private int CalculateLeftBoundary(List<OcrTextBlock> textBlocks, int startLine, int endLine)
    {
        var leftEdges = new List<int>();

        for (int i = startLine; i < endLine && i < textBlocks.Count; i++)
        {
            var leftEdge = GetLeftEdgeX(textBlocks[i]);
            if (leftEdge.HasValue)
                leftEdges.Add(leftEdge.Value);
        }

        if (!leftEdges.Any())
            return 0;

        // Use median instead of average to be robust against outliers
        leftEdges.Sort();
        var median = leftEdges[leftEdges.Count / 2];

        _logger.LogDebug("Calculated left boundary: {Boundary}px (from {Count} text blocks)", median, leftEdges.Count);
        return median;
    }

    /// <summary>
    /// Calculates the approximate width of the receipt
    /// </summary>
    private int CalculateReceiptWidth(List<OcrTextBlock> textBlocks)
    {
        var rightEdges = textBlocks
            .Select(GetRightEdgeX)
            .Where(x => x.HasValue)
            .Select(x => x!.Value)
            .ToList();

        if (!rightEdges.Any())
            return 1000; // Default width

        var maxRightEdge = rightEdges.Max();
        _logger.LogDebug("Calculated receipt width: {Width}px", maxRightEdge);
        return maxRightEdge;
    }

    /// <summary>
    /// Determines if a text block is positioned on the right side of the receipt
    /// Right-side text is typically monetary amounts in price columns, not item names
    /// </summary>
    /// <param name="textBlocks">All text blocks from OCR</param>
    /// <param name="lineIndex">Index of the line to check</param>
    /// <param name="leftBoundary">Typical left edge where item names start</param>
    /// <param name="receiptWidth">Total width of the receipt</param>
    /// <returns>True if the text is on the right side (should be filtered out)</returns>
    private bool IsRightSideText(List<OcrTextBlock> textBlocks, int lineIndex, int leftBoundary, int receiptWidth)
    {
        if (lineIndex >= textBlocks.Count)
            return false;

        var block = textBlocks[lineIndex];
        var leftEdge = GetLeftEdgeX(block);

        // If no spatial data available, don't filter (backward compatibility)
        if (!leftEdge.HasValue || leftBoundary == 0 || receiptWidth == 0)
            return false;

        // Calculate where this text starts relative to the receipt layout
        var textStartPosition = leftEdge.Value;

        // Define the "right side threshold" - text starting beyond 60% of receipt width is likely a price column
        // Most receipts have:
        // - Left side (0-60%): Item names, quantities, descriptions
        // - Right side (60-100%): Unit prices, total prices
        var rightSideThreshold = leftBoundary + (int)((receiptWidth - leftBoundary) * 0.6);

        // If text starts significantly to the right of the typical left boundary, it's likely a price column
        var isRightSide = textStartPosition > rightSideThreshold;

        if (isRightSide)
        {
            _logger.LogDebug(
                "Line {Line} is right-side text: starts at {Start}px > threshold {Threshold}px (left boundary: {Left}px, width: {Width}px)",
                lineIndex, textStartPosition, rightSideThreshold, leftBoundary, receiptWidth);
        }

        return isRightSide;
    }
}
