import { useState, useEffect } from 'react';

// Types
interface HouseholdMember {
  id: string;
  userId: string;
  userName: string;
  role: string;
}

interface ReceiptItemAssignment {
  id?: string;
  householdMemberId: string;
  householdMemberName: string;
  assignedQuantity: number;
  baseAmount: number;
  serviceChargeAmount: number;
  gstAmount: number;
  totalAmount: number;
}

interface ReceiptItem {
  id: string;
  itemName: string;
  quantity: number;
  totalPrice: number;
  unitPrice?: number;
  lineNumber: number;
  isManuallyAdded: boolean;
  ocrConfidence?: number;
  assignments: ReceiptItemAssignment[];
}

interface ReceiptResponse {
  id: string;
  userId: string;
  householdId?: string;
  status: string;
  merchantName?: string;
  receiptDate?: string;
  totalAmount?: number;
  ocrConfidence?: number;
  errorMessage?: string;
  uploadedAt: string;
  items: ReceiptItem[];
  householdMembers: HouseholdMember[];
}

interface ItemAssignment {
  [itemId: string]: {
    [memberId: string]: number; // memberId -> quantity assigned
  };
}

export default function ReceiptUploadWithAssignment() {
  const [selectedFile, setSelectedFile] = useState<File | null>(null);
  const [loading, setLoading] = useState(false);
  const [receipt, setReceipt] = useState<ReceiptResponse | null>(null);
  const [items, setItems] = useState<ReceiptItem[]>([]);
  const [includeServiceCharge, setIncludeServiceCharge] = useState(false);
  const [includeGST, setIncludeGST] = useState(false);
  const [itemAssignments, setItemAssignments] = useState<ItemAssignment>({});
  const [error, setError] = useState<string | null>(null);

  // Placeholder IDs - replace with actual values from auth/context
  const userId = '550e8400-e29b-41d4-a716-446655440000';
  const householdId = '650e8400-e29b-41d4-a716-446655440000'; // Replace with actual household ID

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
    formData.append('householdId', householdId);
    formData.append('image', selectedFile);

    try {
      const response = await fetch('http://localhost:5001/api/receipts/upload', {
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

      // Initialize assignments: assign all quantity to the uploading user by default
      const initialAssignments: ItemAssignment = {};
      data.items.forEach(item => {
        initialAssignments[item.id] = {
          [data.householdMembers[0]?.id || '']: item.quantity
        };
      });
      setItemAssignments(initialAssignments);

      setSelectedFile(null);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Upload failed');
    } finally {
      setLoading(false);
    }
  };

  const handleQuantityChange = (itemId: string, memberId: string, value: string) => {
    const numValue = parseFloat(value) || 0;
    setItemAssignments(prev => ({
      ...prev,
      [itemId]: {
        ...prev[itemId],
        [memberId]: numValue
      }
    }));
  };

  const handleSplitItem = (itemId: string) => {
    const item = items.find(i => i.id === itemId);
    if (!item || !receipt) return;

    const memberCount = receipt.householdMembers.length;
    const splitQuantity = item.quantity / memberCount;

    const newAssignments: { [memberId: string]: number } = {};
    receipt.householdMembers.forEach(member => {
      newAssignments[member.id] = splitQuantity;
    });

    setItemAssignments(prev => ({
      ...prev,
      [itemId]: newAssignments
    }));
  };

  const calculateMemberTotals = () => {
    if (!receipt) return {};

    const memberTotals: { [memberId: string]: number } = {};

    receipt.householdMembers.forEach(member => {
      let total = 0;

      items.forEach(item => {
        const assignedQty = itemAssignments[item.id]?.[member.id] || 0;
        if (assignedQty > 0) {
          const unitPrice = item.totalPrice / item.quantity;
          let itemAmount = unitPrice * assignedQty;

          // Apply service charge to the item amount
          if (includeServiceCharge) {
            itemAmount += itemAmount * 0.10;
          }

          // Apply GST to (item + service charge)
          if (includeGST) {
            itemAmount += itemAmount * 0.09;
          }

          total += itemAmount;
        }
      });

      memberTotals[member.id] = total;
    });

    return memberTotals;
  };

  const handleConfirm = async () => {
    if (!receipt) return;

    setLoading(true);
    setError(null);

    try {
      // Build the assignment payload
      const itemAssignmentsPayload = items.map(item => ({
        receiptItemId: item.id,
        memberAssignments: Object.entries(itemAssignments[item.id] || {})
          .filter(([_, qty]) => qty > 0)
          .map(([memberId, qty]) => ({
            householdMemberId: memberId,
            assignedQuantity: qty
          }))
      }));

      const assignDto = {
        receiptId: receipt.id,
        householdId: receipt.householdId || householdId,
        applyServiceCharge: includeServiceCharge,
        applyGst: includeGST,
        itemAssignments: itemAssignmentsPayload
      };

      const response = await fetch('http://localhost:5001/api/receipts/assign', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json'
        },
        body: JSON.stringify(assignDto)
      });

      if (!response.ok) {
        const errorData = await response.json();
        throw new Error(errorData.message || 'Assignment failed');
      }

      const updatedReceipt: ReceiptResponse = await response.json();
      alert('Receipt confirmed and assigned to family members!');

      // Reset for next receipt
      setReceipt(null);
      setItems([]);
      setItemAssignments({});
      setIncludeServiceCharge(false);
      setIncludeGST(false);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Confirmation failed');
    } finally {
      setLoading(false);
    }
  };

  const memberTotals = calculateMemberTotals();
  const grandTotal = Object.values(memberTotals).reduce((sum, val) => sum + val, 0);

  return (
    <div className="max-w-6xl mx-auto p-6">
      <h1 className="text-3xl font-bold mb-6">Receipt Scanner with Family Assignment</h1>

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
      {receipt && !loading && (
        <div className="bg-white rounded-lg shadow p-6">
          <h2 className="text-xl font-semibold mb-4">Receipt Details & Assignment</h2>

          {/* Receipt Info */}
          <div className="grid grid-cols-2 gap-4 mb-6 p-4 bg-gray-50 rounded-md">
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
          </div>

          {/* GST/Service Charge Options */}
          <div className="flex gap-6 p-4 bg-gray-50 rounded-md mb-6">
            <label className="flex items-center gap-2 cursor-pointer">
              <input
                type="checkbox"
                checked={includeServiceCharge}
                onChange={(e) => setIncludeServiceCharge(e.target.checked)}
                className="w-4 h-4 text-blue-600 rounded focus:ring-2 focus:ring-blue-500"
              />
              <span className="text-sm font-medium">Service Charge (10%)</span>
            </label>
            <label className="flex items-center gap-2 cursor-pointer">
              <input
                type="checkbox"
                checked={includeGST}
                onChange={(e) => setIncludeGST(e.target.checked)}
                className="w-4 h-4 text-blue-600 rounded focus:ring-2 focus:ring-blue-500"
              />
              <span className="text-sm font-medium">GST (9%)</span>
            </label>
          </div>

          {/* Items with Assignment */}
          <div className="mb-6">
            <h3 className="font-semibold mb-3">Items & Family Member Assignment</h3>

            {items.length === 0 ? (
              <p className="text-gray-500 italic">No items found.</p>
            ) : (
              <div className="space-y-4">
                {items.map((item) => (
                  <div key={item.id} className="border rounded-md p-4">
                    <div className="flex justify-between items-start mb-3">
                      <div>
                        <p className="font-medium">{item.itemName}</p>
                        <p className="text-sm text-gray-600">
                          Total Qty: {item.quantity} | Price: ${item.totalPrice.toFixed(2)}
                        </p>
                      </div>
                      <button
                        onClick={() => handleSplitItem(item.id)}
                        className="px-3 py-1 text-sm bg-indigo-500 text-white rounded-md hover:bg-indigo-600"
                        title="Split equally among all members"
                      >
                        Split Equally
                      </button>
                    </div>

                    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-3">
                      {receipt.householdMembers.map(member => {
                        const assignedQty = itemAssignments[item.id]?.[member.id] || 0;
                        const unitPrice = item.totalPrice / item.quantity;
                        let memberItemAmount = unitPrice * assignedQty;

                        if (includeServiceCharge) {
                          memberItemAmount += memberItemAmount * 0.10;
                        }
                        if (includeGST) {
                          memberItemAmount += memberItemAmount * 0.09;
                        }

                        return (
                          <div key={member.id} className="bg-gray-50 p-3 rounded">
                            <label className="block text-sm font-medium mb-1">
                              {member.userName} ({member.role})
                            </label>
                            <input
                              type="number"
                              min="0"
                              step="0.5"
                              max={item.quantity}
                              value={assignedQty}
                              onChange={(e) => handleQuantityChange(item.id, member.id, e.target.value)}
                              className="w-full px-2 py-1 border rounded text-sm"
                            />
                            <p className="text-xs text-gray-600 mt-1">
                              ${memberItemAmount.toFixed(2)}
                            </p>
                          </div>
                        );
                      })}
                    </div>
                  </div>
                ))}
              </div>
            )}
          </div>

          {/* Family Member Totals */}
          {items.length > 0 && receipt.householdMembers.length > 0 && (
            <div className="border-t pt-4">
              <h3 className="font-semibold mb-3">Family Member Totals</h3>
              <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-4">
                {receipt.householdMembers.map(member => (
                  <div key={member.id} className="bg-blue-50 p-3 rounded">
                    <p className="text-sm font-medium">{member.userName}</p>
                    <p className="text-lg font-bold text-blue-700">
                      ${(memberTotals[member.id] || 0).toFixed(2)}
                    </p>
                  </div>
                ))}
              </div>
              <div className="mt-4 p-4 bg-green-50 rounded">
                <div className="flex justify-between items-center">
                  <span className="text-lg font-semibold">Grand Total:</span>
                  <span className="text-2xl font-bold text-green-700">
                    ${grandTotal.toFixed(2)}
                  </span>
                </div>
              </div>
            </div>
          )}

          {/* Action Buttons */}
          <div className="mt-6 flex gap-3">
            <button
              className="px-4 py-2 bg-green-600 text-white rounded-md hover:bg-green-700"
              onClick={handleConfirm}
              disabled={loading}
            >
              ✓ Confirm & Update Expenditures
            </button>
            <button
              className="px-4 py-2 border border-gray-300 rounded-md hover:bg-gray-50"
              onClick={() => {
                setReceipt(null);
                setItems([]);
                setItemAssignments({});
                setIncludeServiceCharge(false);
                setIncludeGST(false);
              }}
            >
              Upload Another
            </button>
          </div>
        </div>
      )}
    </div>
  );
}
