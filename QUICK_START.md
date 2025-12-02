# 🚀 Quick Start - Receipt OCR Testing

## Starting the Application

### Terminal 1: Backend
```bash
cd backend
dotnet run
```
✅ Backend: `http://localhost:5000`

### Terminal 2: Frontend
```bash
cd frontend
npm install  # First time only
npm run dev
```
✅ Frontend: `http://localhost:5173`

## Testing the Receipt Scanner

### 1. Navigate to Receipt Scanner
Open your browser and go to:
```
http://localhost:5173/receipt-scanner
```

(You'll need to login first if authentication is required)

### 2. Upload a Receipt
- Click "Choose File"
- Select one of your receipt images
- Click "Upload & Scan Receipt"
- Wait 2-5 seconds for OCR processing

### 3. Review Results
You'll see:
- ✅ Merchant name
- ✅ Date
- ✅ Total amount
- ✅ Extracted line items with quantities and prices
- ✅ OCR confidence scores

### 4. Test with Different Receipts
Try all 5 sample receipt types:
1. **Astons (PDF)** - Expected: 85-95% accuracy
2. **Swensen's (Email)** - Expected: 85-95% accuracy
3. **Pandamart (App)** - Expected: 85-90% accuracy
4. **Takeaway (Mixed language)** - Expected: 65-80% accuracy
5. **NTUC (Wrinkled)** - Expected: 50-70% accuracy

## API Endpoints Available

All endpoints work from frontend using `fetch`:

```typescript
// Upload receipt
POST http://localhost:5000/api/receipts/upload
Body: FormData with userId and image

// Get receipt
GET http://localhost:5000/api/receipts/{receiptId}

// Get all user receipts
GET http://localhost:5000/api/receipts/user/{userId}

// Add manual item
POST http://localhost:5000/api/receipts/items
Body: { receiptId, itemName, quantity, totalPrice }

// Confirm receipt
POST http://localhost:5000/api/receipts/confirm
Body: { receiptId, items: [...] }
```

## Component Files Created

✅ **Frontend:**
- [ReceiptUpload.tsx](frontend/src/components/ReceiptUpload.tsx) - Main upload component
- [ReceiptScanner.tsx](frontend/src/pages/ReceiptScanner.tsx) - Page wrapper
- Route added: `/receipt-scanner`

✅ **Backend:**
- Complete OCR pipeline (see [backend/OCR_SUMMARY.md](backend/OCR_SUMMARY.md))

## Next Steps for Full Implementation

### 1. Improve UI/UX
- [ ] Add loading spinner animation
- [ ] Add success/error toasts
- [ ] Add image preview before upload
- [ ] Add camera capture option (mobile)

### 2. Review & Edit Workflow
- [ ] Add inline edit for item names
- [ ] Add edit quantity/price inputs
- [ ] Add delete item button
- [ ] Add "Add Item" modal/form
- [ ] Implement confirm functionality

### 3. Assignment Feature
- [ ] Create assignment modal
- [ ] Fetch family members
- [ ] Assign items to members
- [ ] Save assignments to backend

### 4. Integration
- [ ] Connect to your actual user ID from auth context
- [ ] Add to navigation menu
- [ ] Style to match your app theme
- [ ] Add receipt history view
- [ ] Add receipt detail page

## Current User ID
The component uses a placeholder user ID:
```typescript
const userId = '550e8400-e29b-41d4-a716-446655440000';
```

**Replace this with:**
```typescript
const { user } = useAuthContext();
const userId = user?.id;
```

## Testing Tips

### ✅ DO:
- Test with high-quality receipt images first
- Try digital receipts (Astons, Swensen's) for best results
- Check browser console for API errors
- Verify backend is running before testing

### ❌ DON'T:
- Don't upload images larger than 10MB
- Don't use extremely wrinkled receipts for demo
- Don't expect 100% accuracy (70-75% is normal)

## Troubleshooting

### "Failed to fetch" or CORS error
- ✅ Make sure backend is running (`dotnet run`)
- ✅ Check backend URL is correct (`http://localhost:5000`)
- ✅ CORS is already configured in backend

### "Upload failed" error
- ✅ Check file size (< 10MB)
- ✅ Check file type (JPG, PNG, PDF only)
- ✅ Check backend console for error details

### No items extracted
- ✅ Try a different receipt with clearer text
- ✅ Check `rawOcrText` in browser dev tools (Network tab)
- ✅ This is expected behavior - users can add items manually

### Low confidence scores
- ✅ This is normal for physical receipts
- ✅ User review workflow handles this
- ✅ Digital receipts have higher confidence

## Documentation

- **Quick Start**: [OCR_QUICKSTART.md](backend/OCR_QUICKSTART.md)
- **Full Implementation**: [OCR_IMPLEMENTATION.md](backend/OCR_IMPLEMENTATION.md)
- **Summary**: [OCR_SUMMARY.md](backend/OCR_SUMMARY.md)

## Demo Strategy

For your school presentation:

1. **Pre-test receipts** - Know which ones work well
2. **Use digital receipts** - Astons and Swensen's are best
3. **Show the workflow** - Upload → Review → (Assignment)
4. **Acknowledge limitations** - "This is why we have user review"
5. **Focus on innovation** - Family expense assignment

## Component Usage Example

```tsx
// In any page/component
import ReceiptUpload from '@/components/ReceiptUpload';

function MyPage() {
  return (
    <div>
      <h1>My Custom Page</h1>
      <ReceiptUpload />
    </div>
  );
}
```

## Ready to Test! 🎉

1. ✅ Start backend: `cd backend && dotnet run`
2. ✅ Start frontend: `cd frontend && npm run dev`
3. ✅ Open: `http://localhost:5173/receipt-scanner`
4. ✅ Upload a receipt and see the magic happen!

---

**Questions?** Check the documentation in `backend/` or review component code.
