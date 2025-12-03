# Receipt OCR Implementation

## Overview
Complete OCR system for scanning receipts and extracting line items using Tesseract OCR with multi-language support (English + Chinese Simplified).

## Architecture

### Flow
```
User uploads receipt → Preprocessing → OCR → Parser → Review → Confirmation → (Assignment - Coming Soon)
```

### Components

1. **Image Preprocessing** (`ImagePreprocessingService`)
   - Auto-orientation (fixes rotated photos)
   - Resize (max 2000px)
   - Grayscale conversion
   - Contrast enhancement
   - Sharpening
   - Binary threshold

2. **OCR Engine** (`TesseractOcrService`)
   - Multi-language: English + Chinese Simplified
   - Returns full text + confidence scores
   - Line-by-line text extraction

3. **Receipt Parser** (`ReceiptParserService`)
   - Extracts merchant name, date, total
   - Parses line items with quantities and prices
   - Handles multiple receipt formats
   - Regex-based pattern matching

4. **Receipt Service** (`ReceiptService`)
   - Orchestrates the full pipeline
   - Manages database operations
   - Handles file storage

## Database Schema

### Receipts Table
- `Id` (uuid, PK)
- `UserId` (uuid, FK → Users)
- `ImagePath` (string) - Path to stored image
- `Status` (enum) - Processing, ReviewRequired, Confirmed, Failed
- `MerchantName` (string, nullable)
- `ReceiptDate` (datetime, nullable)
- `TotalAmount` (decimal, nullable)
- `RawOcrText` (text) - Full OCR output
- `OcrConfidence` (decimal) - Average confidence (0-100)
- `ErrorMessage` (string, nullable)
- `UploadedAt` (datetime)
- `UpdatedAt` (datetime, nullable)

### ReceiptItems Table
- `Id` (uuid, PK)
- `ReceiptId` (uuid, FK → Receipts)
- `ItemName` (string)
- `Quantity` (int)
- `UnitPrice` (decimal, nullable)
- `TotalPrice` (decimal)
- `LineNumber` (int)
- `IsManuallyAdded` (bool)
- `OcrConfidence` (decimal, nullable)
- `CreatedAt` (datetime)

## API Endpoints

### 1. Upload Receipt
```
POST /api/receipts/upload
Content-Type: multipart/form-data

Parameters:
- userId (form): Guid
- image (form): IFormFile (JPG, PNG, PDF max 10MB)

Response: ReceiptResponseDto
{
  "id": "guid",
  "userId": "guid",
  "status": "ReviewRequired",
  "merchantName": "NTUC FairPrice",
  "receiptDate": "2025-11-18T00:00:00Z",
  "totalAmount": 84.35,
  "ocrConfidence": 78.5,
  "items": [
    {
      "id": "guid",
      "itemName": "FER ROCHER T-162006",
      "quantity": 1,
      "totalPrice": 19.90,
      "lineNumber": 0,
      "isManuallyAdded": false,
      "ocrConfidence": 82.3
    }
  ]
}
```

### 2. Get Receipt
```
GET /api/receipts/{receiptId}

Response: ReceiptResponseDto
```

### 3. Get User Receipts
```
GET /api/receipts/user/{userId}

Response: ReceiptResponseDto[]
```

### 4. Add Manual Item
```
POST /api/receipts/items
Content-Type: application/json

Body:
{
  "receiptId": "guid",
  "itemName": "Extra Item",
  "quantity": 1,
  "unitPrice": 5.00,
  "totalPrice": 5.00
}

Response: ReceiptItemDto
```

### 5. Confirm Receipt
```
POST /api/receipts/confirm
Content-Type: application/json

Body:
{
  "receiptId": "guid",
  "items": [
    {
      "id": "guid (optional for new items)",
      "itemName": "Corrected Name",
      "quantity": 1,
      "totalPrice": 19.90,
      "lineNumber": 0,
      "isManuallyAdded": false
    }
  ]
}

Response: ReceiptResponseDto
```

### 6. Assign Items (Placeholder)
```
POST /api/receipts/{receiptId}/assign

Response: 501 Not Implemented
```

## Setup Instructions

### 1. Database Migration

**Option A: With .env configured**
```bash
dotnet ef database update
```

**Option B: Manual SQL on Supabase**
Run the SQL in `migration.sql` on your Supabase SQL Editor.

### 2. Configuration

Ensure `appsettings.json` has OCR settings:
```json
{
  "Ocr": {
    "TessDataPath": "tessdata",
    "Language": "eng+chi_sim"
  }
}
```

### 3. Language Data Files

The `tessdata` directory contains:
- `eng.traineddata` - English language model
- `chi_sim.traineddata` - Chinese Simplified model

These are automatically copied to the output directory during build.

### 4. File Storage

Receipt images are stored in:
```
backend/uploads/receipts/
```

This directory is created automatically on first upload.

## Expected OCR Accuracy

Based on receipt types:

| Receipt Type | Expected Accuracy |
|-------------|------------------|
| Digital receipts (PDF, app screenshots) | 85-95% |
| Clean physical receipts | 70-85% |
| Wrinkled/poor quality | 50-70% |
| Mixed language (Chinese + English) | 65-80% |

**Overall average: 70-75%**

## User Review Workflow

The OCR is designed with a **mandatory review step**:

1. User uploads receipt
2. OCR processes and extracts items
3. Status = "ReviewRequired"
4. User reviews items in UI
5. User can:
   - Edit item names
   - Adjust quantities/prices
   - Add missing items
   - Remove incorrect items
6. User confirms
7. Status = "Confirmed"
8. Items ready for assignment

This ensures accuracy even when OCR makes mistakes.

## Testing the System

### Test with Postman/cURL

1. Start the API:
```bash
dotnet run
```

2. Upload a receipt:
```bash
curl -X POST http://localhost:5000/api/receipts/upload \
  -F "userId=YOUR_USER_GUID" \
  -F "image=@/path/to/receipt.jpg"
```

3. Review the response to see extracted items

4. Get the receipt:
```bash
curl http://localhost:5000/api/receipts/{RECEIPT_ID}
```

### Test Receipt Types

The system has been tested with:
- ✅ NTUC FairPrice receipts (Singapore)
- ✅ Restaurant receipts (Astons, takeaway)
- ✅ Digital receipts (Swensen's email)
- ✅ App screenshots (Pandamart)
- ✅ Mixed language receipts (Chinese + English)

## Troubleshooting

### OCR Returns Empty Text
- Check image quality (not too blurry)
- Verify tessdata files exist in output directory
- Check logs for preprocessing errors

### Low Confidence Scores
- Image may be too dark/light
- Try with better quality photo
- Ensure receipt is flat (not wrinkled)

### Parser Misses Items
- Check raw OCR text in response (`rawOcrText`)
- Parser uses regex patterns for common formats
- Add manual items if needed

### Database Connection Errors
- Verify .env file has correct Supabase credentials
- Check connection string format
- Test with `dotnet ef database update`

## Optimization Tips

For best OCR results, advise users to:
1. Take photos in good lighting
2. Keep receipt flat (not wrinkled)
3. Ensure receipt is in focus
4. Hold phone directly above receipt
5. Use flash if receipt is faded

## Future Enhancements

- [ ] Receipt format detection (auto-select parser)
- [ ] Train custom Tesseract model on receipt data
- [ ] Add support for more languages
- [ ] Implement assignment feature (assign items to family members)
- [ ] Add receipt image viewer in frontend
- [ ] Bulk upload support
- [ ] Receipt duplicate detection

## Technical Notes

### Why Tesseract?
- Completely free (no API costs)
- Works offline
- Good accuracy with preprocessing
- Multi-language support
- Active development

### Preprocessing Pipeline
The preprocessing is aggressive to maximize OCR accuracy:
1. **Auto-orient**: Fixes rotated phone photos
2. **Resize**: Improves processing speed
3. **Grayscale**: Removes color noise
4. **Contrast**: Makes text stand out (1.5x)
5. **Sharpen**: Defines text edges (1.5f Gaussian)
6. **Brightness**: Optimizes for binarization (1.1x)
7. **Binary threshold**: Black text on white (0.5f)

### Parser Patterns
The parser uses multiple regex patterns to match:
- `1x Item Name $9.99` (quantity + item + price)
- `2 Item Name` on one line, `$19.98` on next
- `TOTAL: $84.35` (total extraction)
- `DATE: 18/11/2025` (date extraction)
- ISO dates: `2025-11-18`

## Performance

- **Upload + OCR**: ~2-5 seconds per receipt
- **Average file size**: 2-5 MB per image
- **Database size**: ~10KB per receipt with items
- **Memory usage**: ~100MB for Tesseract engine

## Support

For issues or questions:
1. Check logs in console output
2. Verify tessdata files are in place
3. Test with high-quality receipt images first
4. Review `rawOcrText` in API response to debug parser issues
