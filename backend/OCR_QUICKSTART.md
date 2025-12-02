# Receipt OCR - Quick Start Guide

## Setup (5 minutes)

### 1. Apply Database Migration

**If you have .env configured with Supabase:**
```bash
cd backend
dotnet ef database update
```

**OR run the SQL manually on Supabase:**
The SQL is in [migration.sql](migration.sql). Copy and run it in Supabase SQL Editor.

### 2. Start the API
```bash
cd backend
dotnet run
```

API will start at: `http://localhost:5000`

## Test the OCR

### Using cURL

1. **Upload a receipt** (replace with your user ID):
```bash
curl -X POST http://localhost:5000/api/receipts/upload \
  -F "userId=YOUR_USER_GUID_HERE" \
  -F "image=@/path/to/receipt.jpg" \
  | jq
```

2. **Get the receipt** (use ID from step 1):
```bash
curl http://localhost:5000/api/receipts/{RECEIPT_ID} | jq
```

### Using Postman

1. Create new request: `POST http://localhost:5000/api/receipts/upload`
2. Body → form-data:
   - `userId`: (your user GUID)
   - `image`: (select file)
3. Send
4. Review extracted items in response

## Expected Response

```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "userId": "550e8400-e29b-41d4-a716-446655440001",
  "status": "ReviewRequired",
  "merchantName": "NTUC FairPrice",
  "receiptDate": "2025-11-18T00:00:00Z",
  "totalAmount": 84.35,
  "ocrConfidence": 78.5,
  "uploadedAt": "2025-11-24T09:40:30.123Z",
  "items": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440002",
      "itemName": "FER ROCHER T-162006",
      "quantity": 1,
      "totalPrice": 19.90,
      "lineNumber": 0,
      "isManuallyAdded": false,
      "ocrConfidence": 82.3
    },
    {
      "id": "550e8400-e29b-41d4-a716-446655440003",
      "itemName": "PLASTIC BAG",
      "quantity": 1,
      "totalPrice": 0.05,
      "lineNumber": 1,
      "isManuallyAdded": false,
      "ocrConfidence": 76.8
    }
  ]
}
```

## User Workflow

### 1. Upload Receipt
User takes photo and uploads via app
↓
### 2. OCR Processing (2-5 seconds)
System extracts items automatically
↓
### 3. Review Items
User sees extracted items:
- ✅ Correct items → Keep
- ❌ Wrong items → Delete
- ✏️ Incorrect details → Edit
- ➕ Missing items → Add manually
↓
### 4. Confirm
```bash
curl -X POST http://localhost:5000/api/receipts/confirm \
  -H "Content-Type: application/json" \
  -d '{
    "receiptId": "YOUR_RECEIPT_ID",
    "items": [
      {
        "itemName": "Ferrero Rocher",
        "quantity": 1,
        "totalPrice": 19.90,
        "lineNumber": 0,
        "isManuallyAdded": false
      }
    ]
  }'
```
↓
### 5. Assignment (Coming Soon)
Assign items to family members

## Add Missing Item

If OCR missed an item:

```bash
curl -X POST http://localhost:5000/api/receipts/items \
  -H "Content-Type: application/json" \
  -d '{
    "receiptId": "YOUR_RECEIPT_ID",
    "itemName": "Chocolate Bar",
    "quantity": 1,
    "totalPrice": 3.50
  }'
```

## Tips for Best Results

### ✅ DO:
- Take photos in good lighting
- Keep receipt flat
- Ensure receipt is in focus
- Hold phone directly above receipt
- Use digital receipts when possible

### ❌ DON'T:
- Upload wrinkled receipts
- Take photos in shadow
- Upload blurry images
- Use receipts with faded text

## Accuracy Expectations

| Receipt Type | Accuracy |
|--------------|----------|
| Digital receipts (PDF/email) | 85-95% |
| Clean physical receipts | 70-85% |
| Wrinkled/poor quality | 50-70% |

**Average: 70-75%**

This is why user review is mandatory!

## Common Issues

### "No items found"
- Check if receipt text is readable
- Try with better lighting
- Review `rawOcrText` in response

### "Low confidence scores"
- Image may be too dark/blurry
- Retake photo with better quality

### "Wrong items detected"
- This is expected! That's why we have review step
- User can edit/delete wrong items
- Add missing items manually

## Next Steps

1. **Test with your receipt images**
   - Use the sample receipts you provided
   - Try different receipt types
   - Note accuracy for each type

2. **Build frontend UI**
   - Receipt upload component
   - Item review/edit interface
   - Add item form

3. **Implement assignment feature**
   - Assign items to family members
   - Track who paid for what

## API Reference

Full API documentation: [OCR_IMPLEMENTATION.md](OCR_IMPLEMENTATION.md)

## Need Help?

1. Check logs in console
2. Review [OCR_IMPLEMENTATION.md](OCR_IMPLEMENTATION.md)
3. Test with high-quality images first
4. Check `rawOcrText` in API response to debug parser

## File Locations

- **API**: `backend/Controllers/ReceiptsController.cs`
- **OCR Service**: `backend/Services/TesseractOcrService.cs`
- **Parser**: `backend/Services/ReceiptParserService.cs`
- **Models**: `backend/Models/Receipt.cs`, `backend/Models/ReceiptItem.cs`
- **Uploads**: `backend/uploads/receipts/`
- **Language data**: `backend/tessdata/`

## Status Codes

- `200 OK` - Success
- `201 Created` - Item added
- `400 Bad Request` - Invalid input
- `404 Not Found` - Receipt not found
- `500 Internal Server Error` - OCR/processing error
- `501 Not Implemented` - Assignment endpoint (coming soon)

---

**Ready to scan receipts!** 🧾✨
