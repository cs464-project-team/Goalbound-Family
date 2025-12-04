import { useState } from 'react';
import { getApiUrl } from '../config/api';
import { authenticatedFetch } from '../services/authService';

interface ReceiptItem {
  id: string;
  itemName: string;
  quantity: number;
  totalPrice: number;
  lineNumber: number;
  isManuallyAdded: boolean;
  ocrConfidence?: number;
}

interface ReceiptResponse {
  id: string;
  userId: string;
  status: string;
  merchantName?: string;
  receiptDate?: string;
  totalAmount?: number;
  ocrConfidence?: number;
  errorMessage?: string;
  uploadedAt: string;
  items: ReceiptItem[];
}

export default function ReceiptUpload() {
  const [selectedFile, setSelectedFile] = useState<File | null>(null);
  const [loading, setLoading] = useState(false);
  const [receipt, setReceipt] = useState<ReceiptResponse | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [items, setItems] = useState<ReceiptItem[]>([]);
  const [includeServiceCharge, setIncludeServiceCharge] = useState(false);
  const [includeGST, setIncludeGST] = useState(false);

  // Replace with actual user ID from your auth system
  const userId = '550e8400-e29b-41d4-a716-446655440000'; // Placeholder

  const handleFileSelect = (e: React.ChangeEvent<HTMLInputElement>) => {
    if (e.target.files && e.target.files[0]) {
      setSelectedFile(e.target.files[0]);
      setError(null);
    }
  };

  const handleUpload = async () => {
    if (!selectedFile) {
      setError('Please select a file first');
      return;
    }

    setLoading(true);
    setError(null);

    const formData = new FormData();
    formData.append('userId', userId);
    formData.append('image', selectedFile);

    try {
      const response = await authenticatedFetch(getApiUrl('/api/receipts/upload'), {
        method: 'POST',
        body: formData,
      });

      if (!response.ok) {
        const errorData = await response.json();
        throw new Error(errorData.message || 'Upload failed');
      }

      const data: ReceiptResponse = await response.json();
      setReceipt(data);
      setItems(data.items);
      setSelectedFile(null);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Upload failed');
    } finally {
      setLoading(false);
    }
  };

  const handleRemoveItem = (itemId: string) => {
    setItems(items.filter(item => item.id !== itemId));
  };

  // Calculate totals
  const subtotal = items.reduce((sum, item) => sum + item.totalPrice, 0);
  const serviceCharge = includeServiceCharge ? subtotal * 0.10 : 0;
  const subtotalWithService = subtotal + serviceCharge;
  const gst = includeGST ? subtotalWithService * 0.09 : 0;
  const grandTotal = subtotalWithService + gst;

  return (
    <div className="max-w-4xl mx-auto p-6">
      <h1 className="text-3xl font-bold mb-6">Receipt Scanner (OCR)</h1>

      {/* Upload Section */}
      <div className="bg-white rounded-lg shadow p-6 mb-6">
        <h2 className="text-xl font-semibold mb-4">Upload Receipt</h2>

        <div className="space-y-4">
          <div>
            <label className="block text-sm font-medium mb-2">
              Select Receipt Image (JPG, PNG, or PDF)
            </label>
            <input
              type="file"
              accept="image/*,.pdf"
              onChange={handleFileSelect}
              className="block w-full text-sm text-gray-500
                file:mr-4 file:py-2 file:px-4
                file:rounded-md file:border-0
                file:text-sm file:font-semibold
                file:bg-blue-50 file:text-blue-700
                hover:file:bg-blue-100"
            />
          </div>

          {selectedFile && (
            <div className="text-sm text-gray-600">
              Selected: {selectedFile.name} ({(selectedFile.size / 1024 / 1024).toFixed(2)} MB)
            </div>
          )}

          <button
            onClick={handleUpload}
            disabled={!selectedFile || loading}
            className="px-6 py-2 bg-blue-600 text-white rounded-md
              hover:bg-blue-700 disabled:bg-gray-300 disabled:cursor-not-allowed
              transition-colors"
          >
            {loading ? 'Processing...' : 'Upload & Scan Receipt'}
          </button>
        </div>

        {error && (
          <div className="mt-4 p-4 bg-red-50 border border-red-200 rounded-md">
            <p className="text-red-700">❌ {error}</p>
          </div>
        )}
      </div>

      {/* Loading State */}
      {loading && (
        <div className="bg-blue-50 rounded-lg shadow p-6 mb-6">
          <div className="flex items-center space-x-3">
            <div className="animate-spin rounded-full h-6 w-6 border-b-2 border-blue-600"></div>
            <span className="text-blue-700">Scanning receipt with OCR...</span>
          </div>
        </div>
      )}

      {/* Results Section */}
      {receipt && (
        <div className="bg-white rounded-lg shadow p-6">
          <h2 className="text-xl font-semibold mb-4">OCR Results</h2>

          {/* Receipt Info */}
          <div className="grid grid-cols-2 gap-4 mb-6 p-4 bg-gray-50 rounded-md">
            <div>
              <span className="text-sm text-gray-600">Status:</span>
              <p className="font-medium">
                {receipt.status === 'ReviewRequired' ? '⚠️ Review Required' : receipt.status}
              </p>
            </div>
            {receipt.merchantName && (
              <div>
                <span className="text-sm text-gray-600">Merchant:</span>
                <p className="font-medium">{receipt.merchantName}</p>
              </div>
            )}
            {receipt.receiptDate && (
              <div>
                <span className="text-sm text-gray-600">Date:</span>
                <p className="font-medium">
                  {new Date(receipt.receiptDate).toLocaleDateString()}
                </p>
              </div>
            )}
            {receipt.totalAmount && (
              <div>
                <span className="text-sm text-gray-600">Total:</span>
                <p className="font-medium">${receipt.totalAmount.toFixed(2)}</p>
              </div>
            )}
            {receipt.ocrConfidence && (
              <div>
                <span className="text-sm text-gray-600">OCR Confidence:</span>
                <p className="font-medium">{receipt.ocrConfidence.toFixed(1)}%</p>
              </div>
            )}
          </div>

          {/* Items */}
          <div>
            <h3 className="font-semibold mb-3">Extracted Items ({items.length})</h3>

            {items.length === 0 ? (
              <p className="text-gray-500 italic">No items found. Try a different image or add items manually.</p>
            ) : (
              <div className="space-y-2">
                {items.map((item) => (
                  <div
                    key={item.id}
                    className="flex items-center justify-between p-3 border rounded-md hover:bg-gray-50"
                  >
                    <div className="flex-1">
                      <p className="font-medium">{item.itemName}</p>
                      <p className="text-sm text-gray-600">
                        Qty: {item.quantity}
                        {item.ocrConfidence && (
                          <span className="ml-2 text-xs text-gray-500">
                            (Confidence: {item.ocrConfidence.toFixed(1)}%)
                          </span>
                        )}
                      </p>
                    </div>
                    <div className="flex items-center gap-4">
                      <div className="text-right">
                        <p className="font-semibold">${item.totalPrice.toFixed(2)}</p>
                        {item.isManuallyAdded && (
                          <span className="text-xs text-blue-600">Manual</span>
                        )}
                      </div>
                      <button
                        onClick={() => handleRemoveItem(item.id)}
                        className="px-3 py-1 text-sm bg-red-500 text-white rounded-md hover:bg-red-600 transition-colors"
                        title="Remove item"
                      >
                        ✕
                      </button>
                    </div>
                  </div>
                ))}
              </div>
            )}
          </div>

          {/* Charges and Totals */}
          {items.length > 0 && (
            <div className="mt-6 space-y-4">
              {/* Toggle Options */}
              <div className="flex gap-6 p-4 bg-gray-50 rounded-md">
                <label className="flex items-center gap-2 cursor-pointer">
                  <input
                    type="checkbox"
                    checked={includeServiceCharge}
                    onChange={(e) => setIncludeServiceCharge(e.target.checked)}
                    className="w-4 h-4 text-blue-600 rounded focus:ring-2 focus:ring-blue-500"
                  />
                  <span className="text-sm font-medium">Add Service Charge (10%)</span>
                </label>
                <label className="flex items-center gap-2 cursor-pointer">
                  <input
                    type="checkbox"
                    checked={includeGST}
                    onChange={(e) => setIncludeGST(e.target.checked)}
                    className="w-4 h-4 text-blue-600 rounded focus:ring-2 focus:ring-blue-500"
                  />
                  <span className="text-sm font-medium">Add GST (9%)</span>
                </label>
              </div>

              {/* Totals Breakdown */}
              <div className="border-t pt-4 space-y-2">
                <div className="flex justify-between text-sm">
                  <span className="text-gray-600">Subtotal:</span>
                  <span className="font-medium">${subtotal.toFixed(2)}</span>
                </div>
                {includeServiceCharge && (
                  <div className="flex justify-between text-sm">
                    <span className="text-gray-600">Service Charge (10%):</span>
                    <span className="font-medium">${serviceCharge.toFixed(2)}</span>
                  </div>
                )}
                {includeGST && (
                  <div className="flex justify-between text-sm">
                    <span className="text-gray-600">GST (9%):</span>
                    <span className="font-medium">${gst.toFixed(2)}</span>
                  </div>
                )}
                <div className="flex justify-between text-lg font-bold border-t pt-2">
                  <span>Grand Total:</span>
                  <span className="text-green-600">${grandTotal.toFixed(2)}</span>
                </div>
              </div>
            </div>
          )}

          {/* Action Buttons */}
          <div className="mt-6 flex gap-3">
            <button
              className="px-4 py-2 bg-green-600 text-white rounded-md hover:bg-green-700"
              onClick={() => alert('Confirm functionality - implement review workflow')}
            >
              ✓ Confirm Items
            </button>
            <button
              className="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700"
              onClick={() => alert('Add item functionality - implement add item form')}
            >
              + Add Item
            </button>
            <button
              className="px-4 py-2 border border-gray-300 rounded-md hover:bg-gray-50"
              onClick={() => {
                setReceipt(null);
                setItems([]);
                setIncludeServiceCharge(false);
                setIncludeGST(false);
              }}
            >
              Upload Another
            </button>
          </div>

          {receipt.errorMessage && (
            <div className="mt-4 p-4 bg-yellow-50 border border-yellow-200 rounded-md">
              <p className="text-yellow-700">⚠️ {receipt.errorMessage}</p>
            </div>
          )}
        </div>
      )}

      {/* Instructions */}
      <div className="mt-8 p-4 bg-gray-50 rounded-md text-sm text-gray-600">
        <h3 className="font-semibold mb-2">Tips for Best Results:</h3>
        <ul className="list-disc list-inside space-y-1">
          <li>Take photos in good lighting</li>
          <li>Keep receipt flat (not wrinkled)</li>
          <li>Ensure receipt is in focus</li>
          <li>Digital receipts (PDF/email) work best</li>
          <li>Expected accuracy: 95-99% (Azure Computer Vision)</li>
        </ul>
      </div>
    </div>
  );
}
