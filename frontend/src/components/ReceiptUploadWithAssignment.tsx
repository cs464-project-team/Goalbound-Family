import { useState, useEffect, useCallback } from 'react';
import { useAuthContext } from '../context/AuthProvider';
import { getApiUrl } from '../config/api';

// Types
interface Household {
  id: string;
  name: string;
}

interface BudgetCategory {
  id: string;
  name: string;
}

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

interface ItemAssignment {
  [itemId: string]: {
    [memberId: string]: number; // memberId -> quantity assigned
  };
}

type Mode = 'ocr' | 'manual';

interface ManualExpenseItem {
  id: string;
  description: string;
  amount: string;
  assignedMemberId: string;
}

export default function ReceiptUploadWithAssignment() {
  // Get authenticated user
  const { session } = useAuthContext();
  const userId = session?.user?.id;

  // Mode selection
  const [mode, setMode] = useState<Mode>('ocr');

  // Household and category selection
  const [households, setHouseholds] = useState<Household[]>([]);
  const [selectedHousehold, setSelectedHousehold] = useState<Household | null>(null);
  const [householdMembers, setHouseholdMembers] = useState<HouseholdMember[]>([]);
  const [categories, setCategories] = useState<BudgetCategory[]>([]);
  const [selectedCategory, setSelectedCategory] = useState<BudgetCategory | null>(null);

  // OCR mode state
  const [selectedFile, setSelectedFile] = useState<File | null>(null);
  const [loading, setLoading] = useState(false);
  const [items, setItems] = useState<ReceiptItem[]>([]);
  const [includeServiceCharge, setIncludeServiceCharge] = useState(false);
  const [includeGST, setIncludeGST] = useState(false);
  const [itemAssignments, setItemAssignments] = useState<ItemAssignment>({});

  // OCR metadata (for new flow where receipt is only saved on confirmation)
  const [ocrMetadata, setOcrMetadata] = useState<{
    imagePath: string;
    originalFileName: string;
    merchantName?: string;
    receiptDate?: string;
    totalAmount?: number;
    rawOcrText?: string;
    ocrConfidence?: number;
  } | null>(null);

  // Manual mode state - support for multiple items
  const [manualDate, setManualDate] = useState(new Date().toISOString().split('T')[0]);
  const [manualItems, setManualItems] = useState<ManualExpenseItem[]>([
    { id: crypto.randomUUID(), description: '', amount: '', assignedMemberId: '' }
  ]);

  // OCR manual item addition state
  const [showAddItemForm, setShowAddItemForm] = useState(false);
  const [newItemName, setNewItemName] = useState('');
  const [newItemQuantity, setNewItemQuantity] = useState('1');
  const [newItemPrice, setNewItemPrice] = useState('');

  // Confirmation modal state
  const [showConfirmModal, setShowConfirmModal] = useState(false);

  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);

  // Fetch user's households when userId is available
  const fetchUserHouseholds = useCallback(async () => {
    try {
      // TODO: Replace with actual API endpoint
      const response = await fetch(getApiUrl(`/api/households/user/${userId}`));
      if (response.ok) {
        const data = await response.json();
        setHouseholds(data);
        if (data.length > 0) {
          setSelectedHousehold(data[0]);
        }
      }
    } catch (_err) {
      // Fallback to mock data for testing
      const mockHouseholds = [
        { id: '650e8400-e29b-41d4-a716-446655440000', name: 'Smith Family' }
      ];
      setHouseholds(mockHouseholds);
      setSelectedHousehold(mockHouseholds[0]);
    }
  }, [userId]);

  useEffect(() => {
    if (userId) {
      fetchUserHouseholds();
    }
  }, [userId, fetchUserHouseholds]);

  // Fetch household members
  const fetchHouseholdMembers = useCallback(async () => {
    if (!selectedHousehold) return;

    try {
      const response = await fetch(getApiUrl(`/api/households/${selectedHousehold.id}/members`));
      if (response.ok) {
        const data = await response.json();
        setHouseholdMembers(data);
        // Initialize first manual item with first member if not already set
        if (data.length > 0 && manualItems.length > 0 && !manualItems[0].assignedMemberId) {
          setManualItems(prev => prev.map((item, idx) =>
            idx === 0 ? { ...item, assignedMemberId: data[0].id } : item
          ));
        }
      }
    } catch (_err) {
      // console.error('Failed to fetch household members:', err);
    }
  }, [selectedHousehold, manualItems]);

  // Fetch budget categories
  const fetchBudgetCategories = useCallback(async () => {
    if (!selectedHousehold) return;

    try {
      const response = await fetch(getApiUrl(`/api/budgets/categories/${selectedHousehold.id}`));
      if (response.ok) {
        const data = await response.json();
        setCategories(data);
        if (data.length > 0) {
          setSelectedCategory(data[0]);
        }
      }
    } catch (_err) {
      // console.error('Failed to fetch categories:', err);
      // Fallback to mock data for testing
      const mockCategories = [
        { id: '750e8400-e29b-41d4-a716-446655440000', name: 'Groceries' },
        { id: '750e8400-e29b-41d4-a716-446655440001', name: 'Dining' },
        { id: '750e8400-e29b-41d4-a716-446655440002', name: 'Transport' },
        { id: '750e8400-e29b-41d4-a716-446655440003', name: 'Utilities' },
      ];
      setCategories(mockCategories);
      setSelectedCategory(mockCategories[0]);
    }
  }, [selectedHousehold]);

  // When household is selected, fetch members and categories
  useEffect(() => {
    if (selectedHousehold) {
      fetchHouseholdMembers();
      fetchBudgetCategories();
    }
  }, [selectedHousehold, fetchHouseholdMembers, fetchBudgetCategories]);

  const handleFileSelect = (e: React.ChangeEvent<HTMLInputElement>) => {
    if (e.target.files && e.target.files[0]) {
      setSelectedFile(e.target.files[0]);
      setError(null);
      setSuccess(null);
    }
  };

  const handleUpload = async () => {
    if (!selectedFile) {
      setError('Please select a file first');
      return;
    }

    if (!selectedHousehold) {
      setError('Please select a household first');
      return;
    }

    if (!userId) {
      setError('User not authenticated');
      return;
    }

    setLoading(true);
    setError(null);
    setSuccess(null);

    const formData = new FormData();
    formData.append('userId', userId);
    formData.append('householdId', selectedHousehold.id);
    formData.append('image', selectedFile);

    try {
      const response = await fetch(getApiUrl('/api/receipts/process-ocr'), {
        method: 'POST',
        body: formData,
      });

      if (!response.ok) {
        let errorMessage = 'Upload failed';
        try {
          const errorData = await response.json();
          // Build detailed error message
          errorMessage = errorData.message || errorMessage;
          if (errorData.innerException) {
            errorMessage += `\n\nInner Exception: ${errorData.innerException}`;
          }
          if (errorData.type) {
            errorMessage += `\n\nError Type: ${errorData.type}`;
          }
        } catch (e) {
          // If JSON parsing fails, use default message
          errorMessage = 'Unable to process receipt. Please try again.';
        }
        throw new Error(errorMessage);
      }

      const data = await response.json();

      // Check if OCR failed
      if (!data.success) {
        setError(data.errorMessage || 'We couldn\'t read your receipt. Please make sure the image is clear and try uploading again.');
        setItems([]);
        setOcrMetadata(null);
        return;
      }

      // Store OCR metadata for later use when confirming
      setOcrMetadata({
        imagePath: data.imagePath,
        originalFileName: data.originalFileName,
        merchantName: data.merchantName,
        receiptDate: data.receiptDate,
        totalAmount: data.totalAmount,
        rawOcrText: data.rawOcrText,
        ocrConfidence: data.ocrConfidence
      });

      // Convert the parsed items to the format expected by the rest of the component
      const parsedItems: ReceiptItem[] = data.items.map((item: { tempId: string; itemName: string; quantity: number; totalPrice: number; unitPrice?: number; lineNumber: number; isManuallyAdded: boolean; ocrConfidence?: number }) => ({
        id: item.tempId, // Use tempId as the item ID
        itemName: item.itemName,
        quantity: item.quantity,
        totalPrice: item.totalPrice,
        unitPrice: item.unitPrice || (item.totalPrice / item.quantity),
        lineNumber: item.lineNumber,
        isManuallyAdded: item.isManuallyAdded,
        ocrConfidence: item.ocrConfidence,
        assignments: []
      }));

      setItems(parsedItems);

      // Set household members from the response
      const householdMembersData: HouseholdMember[] = data.householdMembers || [];
      setHouseholdMembers(householdMembersData);

      // Initialize assignments: assign all quantity to first member by default
      const initialAssignments: ItemAssignment = {};
      parsedItems.forEach(item => {
        initialAssignments[item.id] = {
          [householdMembersData[0]?.id || '']: item.quantity
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
    // Handle empty string - set to 0
    if (value === '' || value === null || value === undefined) {
      setItemAssignments(prev => ({
        ...prev,
        [itemId]: {
          ...prev[itemId],
          [memberId]: 0
        }
      }));
      return;
    }

    // Parse as integer to avoid decimal issues and leading zeros
    const numValue = parseInt(value, 10);

    // If parsing fails or negative, set to 0
    if (isNaN(numValue) || numValue < 0) {
      setItemAssignments(prev => ({
        ...prev,
        [itemId]: {
          ...prev[itemId],
          [memberId]: 0
        }
      }));
      return;
    }

    // Get the item to check total quantity
    const item = items.find(i => i.id === itemId);
    if (!item) return;

    // Calculate current total assigned (excluding this member)
    const currentAssignments = itemAssignments[itemId] || {};
    const otherMembersTotal = Object.entries(currentAssignments)
      .filter(([mId]) => mId !== memberId)
      .reduce((sum, [, qty]) => sum + qty, 0);

    // Ensure total doesn't exceed item quantity
    const maxAllowedForMember = item.quantity - otherMembersTotal;

    // Show error if trying to exceed
    if (numValue > maxAllowedForMember) {
      setError(`Cannot allocate more than ${maxAllowedForMember} to this member (${item.quantity - otherMembersTotal} remaining)`);
      setTimeout(() => setError(null), 3000);
    }

    const finalValue = Math.min(numValue, maxAllowedForMember);

    setItemAssignments(prev => ({
      ...prev,
      [itemId]: {
        ...prev[itemId],
        [memberId]: finalValue
      }
    }));
  };

  const handleRemoveItem = (itemId: string) => {
    // Remove the item from the items list
    setItems(prev => prev.filter(item => item.id !== itemId));

    // Remove the item's assignments
    setItemAssignments(prev => {
      const newAssignments = { ...prev };
      delete newAssignments[itemId];
      return newAssignments;
    });
  };

  const handleSplitItem = (itemId: string) => {
    const item = items.find(i => i.id === itemId);
    if (!item) return;

    const memberCount = householdMembers.length;
    const baseQuantity = Math.floor(item.quantity / memberCount);
    const remainder = item.quantity % memberCount;

    const newAssignments: { [memberId: string]: number } = {};
    householdMembers.forEach((member, index) => {
      // Distribute remainder to first few members
      newAssignments[member.id] = baseQuantity + (index < remainder ? 1 : 0);
    });

    setItemAssignments(prev => ({
      ...prev,
      [itemId]: newAssignments
    }));
  };

  const calculateMemberTotals = () => {
    if (items.length === 0) return {};

    const memberTotals: { [memberId: string]: number } = {};

    householdMembers.forEach(member => {
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

  // Show confirmation modal with summary
  const handleConfirmOCR = () => {
    if (items.length === 0 || !selectedCategory) return;

    // Check if all items are fully allocated
    const unallocatedItems: string[] = [];
    items.forEach(item => {
      const totalAssigned = Object.values(itemAssignments[item.id] || {}).reduce((sum, qty) => sum + qty, 0);
      if (totalAssigned < item.quantity) {
        unallocatedItems.push(`${item.itemName} (${totalAssigned}/${item.quantity} allocated)`);
      }
    });

    if (unallocatedItems.length > 0) {
      setError(`Please fully allocate all items before confirming:\n${unallocatedItems.join('\n')}`);
      return;
    }

    setShowConfirmModal(true);
  };

  // Actually submit the receipt assignment
  const handleFinalConfirm = async () => {
    if (items.length === 0 || !selectedCategory || !userId || !ocrMetadata) return;

    setLoading(true);
    setError(null);
    setSuccess(null);

    try {
      // Build the assignment payload with full receipt data
      const itemAssignmentsPayload = items.map((item: ReceiptItem) => ({
        // Don't include receiptItemId since we're creating new items
        itemName: item.itemName,
        quantity: item.quantity,
        totalPrice: item.totalPrice,
        unitPrice: item.unitPrice || (item.totalPrice / item.quantity),
        lineNumber: item.lineNumber,
        isManuallyAdded: item.isManuallyAdded,
        ocrConfidence: item.ocrConfidence || 0,
        memberAssignments: Object.entries(itemAssignments[item.id] || {})
          .filter(([_, qty]) => qty > 0)
          .map(([memberId, qty]) => ({
            householdMemberId: memberId,
            assignedQuantity: qty
          }))
      }));

      const assignDto = {
        // Don't include receiptId - we're creating a new receipt
        userId: userId,
        householdId: selectedHousehold!.id,
        categoryId: selectedCategory.id,
        imagePath: ocrMetadata.imagePath,
        originalFileName: ocrMetadata.originalFileName,
        merchantName: ocrMetadata.merchantName,
        receiptDate: ocrMetadata.receiptDate,
        totalAmount: ocrMetadata.totalAmount,
        rawOcrText: ocrMetadata.rawOcrText,
        ocrConfidence: ocrMetadata.ocrConfidence,
        applyServiceCharge: includeServiceCharge,
        applyGst: includeGST,
        itemAssignments: itemAssignmentsPayload
      };

      const response = await fetch(getApiUrl('/api/receipts/assign'), {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json'
        },
        body: JSON.stringify(assignDto)
      });

      if (!response.ok) {
        let errorMessage = 'Assignment failed';
        try {
          const errorData = await response.json();
          // Build detailed error message
          errorMessage = errorData.message || errorMessage;
          if (errorData.innerException) {
            errorMessage += `\n\nInner Exception: ${errorData.innerException}`;
          }
          if (errorData.type) {
            errorMessage += `\n\nError Type: ${errorData.type}`;
          }
        } catch (e) {
          // If JSON parsing fails, use default message
          errorMessage = 'Unable to create expenses. Please try again.';
        }
        throw new Error(errorMessage);
      }

      setSuccess('✅ Receipt confirmed! Expenses have been created for each family member.');
      setShowConfirmModal(false);

      // Reset for next receipt
      setOcrMetadata(null);
      setItems([]);
      setItemAssignments({});
      setIncludeServiceCharge(false);
      setIncludeGST(false);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Confirmation failed');
      setShowConfirmModal(false);
    } finally {
      setLoading(false);
    }
  };

  const handleSubmitManualExpense = async () => {
    // Validation
    if (!selectedCategory || !selectedHousehold) {
      setError('Please select a household and category');
      return;
    }

    // Filter out empty items
    const validItems = manualItems.filter(item =>
      item.description.trim() && item.amount && item.assignedMemberId
    );

    if (validItems.length === 0) {
      setError('Please add at least one item with description, amount, and assigned member');
      return;
    }

    // Validate amounts
    for (const item of validItems) {
      const baseAmount = 0;parseFloat(item.amount);
      if (isNaN(baseAmount) || baseAmount <= 0) {
        setError(`Invalid amount for "${item.description}". Amount must be greater than 0.`);
        return;
      }
    }

    setLoading(true);
    setError(null);
    setSuccess(null);

    try {
      // Build bulk expense request
      const bulkExpenseItems = validItems.map(item => {
        const member = householdMembers.find(m => m.id === item.assignedMemberId);
        if (!member) throw new Error(`Member not found for item: ${item.description}`);

        return {
          userId: member.userId,
          amount: parseFloat(item.amount),
          description: item.description
        };
      });

      const bulkRequest = {
        householdId: selectedHousehold.id,
        categoryId: selectedCategory.id,
        date: new Date(manualDate).toISOString(),
        items: bulkExpenseItems
      };

      const response = await fetch(getApiUrl('/api/expenses/bulk'), {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json'
        },
        body: JSON.stringify(bulkRequest)
      });

      if (!response.ok) {
        let errorMessage = 'Failed to create expenses';
        try {
          const errorData = await response.json();
          // Build detailed error message
          errorMessage = errorData.message || errorMessage;
          if (errorData.innerException) {
            errorMessage += `\n\nInner Exception: ${errorData.innerException}`;
          }
          if (errorData.type) {
            errorMessage += `\n\nError Type: ${errorData.type}`;
          }
        } catch (e) {
          // If JSON parsing fails, use default message
          errorMessage = 'Unable to create expenses. Please try again.';
        }
        throw new Error(errorMessage);
      }

      const createdExpenses = await response.json();
      setSuccess(`✅ Successfully created ${createdExpenses.length} expense(s)!`);

      // Reset form
      setTimeout(() => {
        setManualItems([
          { id: crypto.randomUUID(), description: '', amount: '', assignedMemberId: householdMembers[0]?.id || '' }
        ]);
        setManualDate(new Date().toISOString().split('T')[0]);
        setSuccess(null);
      }, 2000);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to create expenses');
    } finally {
      setLoading(false);
    }
  };

  // Manual item management functions
  const addManualItem = () => {
    const newItem: ManualExpenseItem = {
      id: crypto.randomUUID(),
      description: '',
      amount: '',
      assignedMemberId: householdMembers[0]?.id || ''
    };
    setManualItems(prev => [...prev, newItem]);
  };

  const removeManualItem = (itemId: string) => {
    if (manualItems.length === 1) {
      setError('You must have at least one item');
      setTimeout(() => setError(null), 2000);
      return;
    }
    setManualItems(prev => prev.filter(item => item.id !== itemId));
  };

  const updateManualItem = (itemId: string, field: keyof ManualExpenseItem, value: string) => {
    setManualItems(prev => prev.map(item =>
      item.id === itemId ? { ...item, [field]: value } : item
    ));
  };

  // OCR manual item addition functions
  const handleAddManualItemToReceipt = () => {
    if (!newItemName.trim() || !newItemPrice) {
      setError('Please fill in item name and price');
      setTimeout(() => setError(null), 2000);
      return;
    }

    const quantity = parseInt(newItemQuantity, 10);
    const price = parseFloat(newItemPrice);

    if (isNaN(quantity) || quantity <= 0) {
      setError('Quantity must be a positive number');
      setTimeout(() => setError(null), 2000);
      return;
    }

    if (isNaN(price) || price <= 0) {
      setError('Price must be a positive number');
      setTimeout(() => setError(null), 2000);
      return;
    }

    // Create new manual item
    const newItem: ReceiptItem = {
      id: crypto.randomUUID(),
      itemName: newItemName,
      quantity: quantity,
      totalPrice: price,
      unitPrice: price / quantity,
      lineNumber: items.length + 1,
      isManuallyAdded: true,
      assignments: []
    };

    // Add to items list
    setItems(prev => [...prev, newItem]);

    // Initialize assignment for this item (assign all to first member by default)
    if (householdMembers.length > 0) {
      setItemAssignments(prev => ({
        ...prev,
        [newItem.id]: {
          [householdMembers[0].id]: quantity
        }
      }));
    }

    // Reset form
    setNewItemName('');
    setNewItemQuantity('1');
    setNewItemPrice('');
    setShowAddItemForm(false);
  };

  const cancelAddItem = () => {
    setNewItemName('');
    setNewItemQuantity('1');
    setNewItemPrice('');
    setShowAddItemForm(false);
  };

  const memberTotals = calculateMemberTotals();
  const grandTotal = Object.values(memberTotals).reduce((sum, val) => sum + val, 0);

  // Show loading state if user is not authenticated yet
  if (!userId) {
    return (
      <div className="max-w-6xl mx-auto p-6">
        <div className="bg-white rounded-lg shadow p-6">
          <p className="text-gray-600">Loading user information...</p>
        </div>
      </div>
    );
  }

  return (
    <div className="max-w-6xl mx-auto p-6">
      <h1 className="text-3xl font-bold mb-6">Expense Management</h1>

      {/* Household Selection */}
      <div className="bg-white rounded-lg shadow p-6 mb-6">
        <h2 className="text-xl font-semibold mb-4">Select Household</h2>
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div>
            <label className="block text-sm font-medium mb-2">Household</label>
            <select
              value={selectedHousehold?.id || ''}
              onChange={(e) => {
                const household = households.find(h => h.id === e.target.value);
                setSelectedHousehold(household || null);
              }}
              className="w-full px-3 py-2 border rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
            >
              {households.map(h => (
                <option key={h.id} value={h.id}>{h.name}</option>
              ))}
            </select>
          </div>
          <div>
            <label className="block text-sm font-medium mb-2">Category</label>
            <select
              value={selectedCategory?.id || ''}
              onChange={(e) => {
                const category = categories.find(c => c.id === e.target.value);
                setSelectedCategory(category || null);
              }}
              className="w-full px-3 py-2 border rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
            >
              {categories.map(c => (
                <option key={c.id} value={c.id}>{c.name}</option>
              ))}
            </select>
          </div>
        </div>
      </div>

      {/* Mode Selection */}
      <div className="bg-white rounded-lg shadow p-6 mb-6">
        <h2 className="text-xl font-semibold mb-4">Input Method</h2>
        <div className="flex gap-4">
          <button
            onClick={() => setMode('ocr')}
            className={`flex-1 py-3 px-4 rounded-md font-medium transition-colors ${
              mode === 'ocr'
                ? 'bg-blue-600 text-white'
                : 'bg-gray-100 text-gray-700 hover:bg-gray-200'
            }`}
          >
            📸 Scan Receipt (OCR)
          </button>
          <button
            onClick={() => setMode('manual')}
            className={`flex-1 py-3 px-4 rounded-md font-medium transition-colors ${
              mode === 'manual'
                ? 'bg-blue-600 text-white'
                : 'bg-gray-100 text-gray-700 hover:bg-gray-200'
            }`}
          >
            ✏️ Manual Entry
          </button>
        </div>
      </div>

      {/* Error/Success Messages */}
      {error && (
        <div className="mb-6 p-4 bg-red-50 border border-red-200 rounded-md">
          <p className="text-red-700 whitespace-pre-wrap">❌ {error}</p>
        </div>
      )}
      {success && (
        <div className="mb-6 p-4 bg-green-50 border border-green-200 rounded-md">
          <p className="text-green-700">✅ {success}</p>
        </div>
      )}

      {/* OCR Mode */}
      {mode === 'ocr' && (
        <>
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
                disabled={!selectedFile || loading || !selectedHousehold}
                className="px-6 py-2 bg-blue-600 text-white rounded-md
                  hover:bg-blue-700 disabled:bg-gray-300 disabled:cursor-not-allowed
                  transition-colors"
              >
                {loading ? 'Processing...' : 'Upload & Scan Receipt'}
              </button>
            </div>
          </div>

          {/* Loading State */}
          {loading && items.length === 0 && (
            <div className="bg-blue-50 rounded-lg shadow p-6 mb-6">
              <div className="flex items-center space-x-3">
                <div className="animate-spin rounded-full h-6 w-6 border-b-2 border-blue-600"></div>
                <span className="text-blue-700">Scanning receipt with OCR...</span>
              </div>
            </div>
          )}

          {/* Results Section */}
          {items.length > 0 && !loading && (
            <div className="bg-white rounded-lg shadow p-6">
              <h2 className="text-xl font-semibold mb-4">Receipt Details & Assignment</h2>

              {/* Receipt Info */}
              <div className="grid grid-cols-2 gap-4 mb-6 p-4 bg-gray-50 rounded-md">
                <div>
                  <label className="block text-sm text-gray-600 mb-1">Merchant:</label>
                  <input
                    type="text"
                    value={ocrMetadata?.merchantName || ''}
                    onChange={(e) => {
                      if (ocrMetadata) {
                        setOcrMetadata({ ...ocrMetadata, merchantName: e.target.value });
                      }
                    }}
                    placeholder="Enter merchant name"
                    className="w-full px-3 py-2 border-2 border-gray-300 rounded-md focus:border-blue-500 focus:outline-none font-medium bg-white"
                  />
                </div>
                {ocrMetadata?.receiptDate && (
                  <div>
                    <span className="text-sm text-gray-600">Date:</span>
                    <p className="font-medium">
                      {new Date(ocrMetadata.receiptDate).toLocaleDateString()}
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
                    {items.map((item) => {
                      // Calculate total assigned for this item
                      const totalAssigned = Object.values(itemAssignments[item.id] || {}).reduce((sum, qty) => sum + qty, 0);
                      const remaining = item.quantity - totalAssigned;
                      const isFullyAllocated = remaining === 0;
                      const isOverAllocated = remaining < 0;

                      return (
                        <div key={item.id} className="border-2 rounded-lg p-5 bg-white shadow-sm hover:shadow-md transition-shadow">
                          {/* Item Header */}
                          <div className="flex justify-between items-start mb-4">
                            <div className="flex-1">
                              <div className="flex items-center gap-2">
                                <input
                                  type="text"
                                  value={item.itemName}
                                  onChange={(e) => {
                                    const updatedItems = items.map(i =>
                                      i.id === item.id ? { ...i, itemName: e.target.value } : i
                                    );
                                    setItems(updatedItems);
                                  }}
                                  className="font-semibold text-lg text-gray-800 border-b-2 border-gray-300 focus:border-indigo-500 focus:outline-none bg-transparent px-1 py-0.5 transition-colors"
                                  placeholder="Item name"
                                />
                                {item.isManuallyAdded && (
                                  <span className="inline-block px-2 py-0.5 bg-blue-100 text-blue-700 text-xs font-semibold rounded">
                                    Manually Added
                                  </span>
                                )}
                              </div>
                              <div className="flex items-center gap-4 mt-2">
                                <div className="flex items-center gap-1">
                                  <label className="text-sm text-gray-600">Qty:</label>
                                  <input
                                    type="number"
                                    min="1"
                                    value={item.quantity}
                                    onChange={(e) => {
                                      const newQty = parseInt(e.target.value) || 1;
                                      const updatedItems = items.map(i =>
                                        i.id === item.id ? { ...i, quantity: newQty, unitPrice: i.totalPrice / newQty } : i
                                      );
                                      setItems(updatedItems);
                                    }}
                                    className="w-16 px-2 py-1 text-sm font-medium border border-gray-300 rounded focus:border-indigo-500 focus:outline-none"
                                  />
                                </div>
                                <div className="flex items-center gap-1">
                                  <label className="text-sm text-gray-600">Price:</label>
                                  <input
                                    type="number"
                                    step="0.01"
                                    min="0"
                                    value={item.totalPrice.toFixed(2)}
                                    onChange={(e) => {
                                      const newPrice = parseFloat(e.target.value) || 0;
                                      const updatedItems = items.map(i =>
                                        i.id === item.id ? { ...i, totalPrice: newPrice, unitPrice: newPrice / i.quantity } : i
                                      );
                                      setItems(updatedItems);
                                    }}
                                    className="w-20 px-2 py-1 text-sm font-medium border border-gray-300 rounded focus:border-indigo-500 focus:outline-none"
                                  />
                                </div>
                                <div className="flex items-center gap-1">
                                  <label className="text-sm text-gray-600">Unit:</label>
                                  <input
                                    type="number"
                                    step="0.01"
                                    min="0"
                                    value={(item.totalPrice / item.quantity).toFixed(2)}
                                    onChange={(e) => {
                                      const newUnitPrice = parseFloat(e.target.value) || 0;
                                      const updatedItems = items.map(i =>
                                        i.id === item.id ? { ...i, totalPrice: newUnitPrice * i.quantity, unitPrice: newUnitPrice } : i
                                      );
                                      setItems(updatedItems);
                                    }}
                                    className="w-20 px-2 py-1 text-sm font-medium border border-gray-300 rounded focus:border-indigo-500 focus:outline-none"
                                  />
                                </div>
                              </div>
                            </div>
                            <div className="flex gap-2">
                              <button
                                onClick={() => handleSplitItem(item.id)}
                                className="px-4 py-2 text-sm font-medium bg-indigo-500 text-white rounded-lg hover:bg-indigo-600 transition-colors shadow-sm"
                                title="Split equally among all members"
                              >
                                Split Equally
                              </button>
                              <button
                                onClick={() => handleRemoveItem(item.id)}
                                className="px-3 py-2 text-sm font-medium bg-red-500 text-white rounded-lg hover:bg-red-600 transition-colors shadow-sm"
                                title="Remove this item"
                              >
                                Remove
                              </button>
                            </div>
                          </div>

                          {/* Allocation Progress Bar */}
                          <div className="mb-4">
                            <div className="flex justify-between items-center mb-1">
                              <span className="text-xs font-medium text-gray-600">Allocation Progress</span>
                              <span className={`text-xs font-semibold ${
                                isOverAllocated ? 'text-red-600' :
                                isFullyAllocated ? 'text-green-600' :
                                'text-gray-600'
                              }`}>
                                {totalAssigned} / {item.quantity}
                                {remaining > 0 && ` (${remaining} left)`}
                                {isOverAllocated && ` (${Math.abs(remaining)} over)`}
                              </span>
                            </div>
                            <div className="w-full bg-gray-200 rounded-full h-2.5 overflow-hidden">
                              <div
                                className={`h-2.5 rounded-full transition-all duration-300 ${
                                  isOverAllocated ? 'bg-red-500' :
                                  isFullyAllocated ? 'bg-green-500' :
                                  'bg-blue-500'
                                }`}
                                style={{ width: `${Math.min((totalAssigned / item.quantity) * 100, 100)}%` }}
                              ></div>
                            </div>
                          </div>

                          {/* Member Assignment Grid */}
                          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-3">
                            {householdMembers.map(member => {
                              const assignedQty = itemAssignments[item.id]?.[member.id] || 0;
                              const unitPrice = item.totalPrice / item.quantity;
                              let memberItemAmount = unitPrice * assignedQty;

                              if (includeServiceCharge) {
                                memberItemAmount += memberItemAmount * 0.10;
                              }
                              if (includeGST) {
                                memberItemAmount += memberItemAmount * 0.09;
                              }

                              // Calculate max this member can have
                              const currentAssignments = itemAssignments[item.id] || {};
                              const otherMembersTotal = Object.entries(currentAssignments)
                                .filter(([mId]) => mId !== member.id)
                                .reduce((sum, [, qty]) => sum + qty, 0);
                              const maxForMember = item.quantity - otherMembersTotal;

                              return (
                                <div key={member.id} className="bg-gradient-to-br from-gray-50 to-gray-100 p-4 rounded-lg border border-gray-200">
                                  <label className="block text-sm font-semibold mb-2 text-gray-700">
                                    {member.userName}
                                    <span className="text-xs font-normal text-gray-500 ml-1">({member.role})</span>
                                  </label>
                                  <div className="relative">
                                    <input
                                      type="number"
                                      min="0"
                                      step="1"
                                      max={item.quantity}
                                      value={assignedQty || ''}
                                      onChange={(e) => handleQuantityChange(item.id, member.id, e.target.value)}
                                      className={`w-full px-3 py-2 border-2 rounded-lg text-sm font-medium outline-none transition-all ${
                                        assignedQty === maxForMember && maxForMember > 0
                                          ? 'border-green-400 bg-green-50 focus:border-green-500 focus:ring-2 focus:ring-green-200'
                                          : 'border-gray-300 focus:border-blue-500 focus:ring-2 focus:ring-blue-200'
                                      }`}
                                      placeholder="0"
                                    />
                                    {maxForMember > 0 && maxForMember < item.quantity && (
                                      <span className="absolute right-12 top-1/2 -translate-y-1/2 text-xs text-gray-400">
                                        max: {maxForMember}
                                      </span>
                                    )}
                                  </div>
                                  <div className="flex items-center justify-between mt-2">
                                    <p className="text-xs text-gray-500">
                                      Amount:
                                    </p>
                                    <p className="text-sm font-bold text-gray-700">
                                      ${memberItemAmount.toFixed(2)}
                                    </p>
                                  </div>
                                </div>
                              );
                            })}
                          </div>
                        </div>
                      );
                    })}
                  </div>
                )}

                {/* Add Manual Item Section */}
                <div className="mt-4">
                  {!showAddItemForm ? (
                    <button
                      onClick={() => setShowAddItemForm(true)}
                      className="px-4 py-2 text-sm bg-blue-500 text-white rounded-md hover:bg-blue-600 transition-colors"
                    >
                      + Add Missing Item
                    </button>
                  ) : (
                    <div className="border-2 border-blue-300 rounded-lg p-4 bg-blue-50">
                      <h4 className="font-semibold text-gray-800 mb-3">Add Manual Item</h4>

                      <div className="grid grid-cols-1 md:grid-cols-3 gap-3 mb-3">
                        <div>
                          <label className="block text-sm font-medium mb-1 text-gray-700">Item Name</label>
                          <input
                            type="text"
                            value={newItemName}
                            onChange={(e) => setNewItemName(e.target.value)}
                            placeholder="e.g., Coffee"
                            className="w-full px-3 py-2 border rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 text-sm"
                          />
                        </div>

                        <div>
                          <label className="block text-sm font-medium mb-1 text-gray-700">Quantity</label>
                          <input
                            type="number"
                            value={newItemQuantity}
                            onChange={(e) => setNewItemQuantity(e.target.value)}
                            min="1"
                            step="1"
                            className="w-full px-3 py-2 border rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 text-sm"
                          />
                        </div>

                        <div>
                          <label className="block text-sm font-medium mb-1 text-gray-700">Total Price ($)</label>
                          <input
                            type="number"
                            value={newItemPrice}
                            onChange={(e) => setNewItemPrice(e.target.value)}
                            min="0"
                            step="0.01"
                            placeholder="0.00"
                            className="w-full px-3 py-2 border rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 text-sm"
                          />
                        </div>
                      </div>

                      <div className="flex gap-2">
                        <button
                          onClick={handleAddManualItemToReceipt}
                          className="px-4 py-2 text-sm bg-green-600 text-white rounded-md hover:bg-green-700 transition-colors"
                        >
                          Add Item
                        </button>
                        <button
                          onClick={cancelAddItem}
                          className="px-4 py-2 text-sm border border-gray-300 rounded-md hover:bg-gray-50 transition-colors"
                        >
                          Cancel
                        </button>
                      </div>
                    </div>
                  )}
                </div>
              </div>

              {/* Family Member Totals */}
              {items.length > 0 && householdMembers.length > 0 && (
                <div className="border-t pt-4">
                  <h3 className="font-semibold mb-3">Family Member Totals</h3>
                  <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-4">
                    {householdMembers.map(member => (
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
              <div className="mt-6">
                {/* Warning if items not fully allocated */}
                {(() => {
                  const unallocated = items.filter(item => {
                    const totalAssigned = Object.values(itemAssignments[item.id] || {}).reduce((sum, qty) => sum + qty, 0);
                    return totalAssigned < item.quantity;
                  });
                  if (unallocated.length > 0) {
                    return (
                      <div className="mb-3 p-3 bg-amber-50 border-2 border-amber-300 rounded-lg">
                        <p className="text-sm font-semibold text-amber-800 mb-1">⚠️ Items not fully allocated:</p>
                        <ul className="text-xs text-amber-700 list-disc list-inside">
                          {unallocated.map(item => {
                            const totalAssigned = Object.values(itemAssignments[item.id] || {}).reduce((sum, qty) => sum + qty, 0);
                            return (
                              <li key={item.id}>
                                {item.itemName}: {totalAssigned}/{item.quantity} allocated
                              </li>
                            );
                          })}
                        </ul>
                      </div>
                    );
                  }
                  return null;
                })()}

                <div className="flex gap-3">
                  <button
                    className="px-4 py-2 bg-green-600 text-white rounded-md hover:bg-green-700 disabled:bg-gray-300 disabled:cursor-not-allowed"
                    onClick={handleConfirmOCR}
                    disabled={loading || !selectedCategory || items.some(item => {
                      const totalAssigned = Object.values(itemAssignments[item.id] || {}).reduce((sum, qty) => sum + qty, 0);
                      return totalAssigned < item.quantity;
                    })}
                  >
                    ✓ Confirm & Create Expenses
                  </button>
                  <button
                    className="px-4 py-2 border border-gray-300 rounded-md hover:bg-gray-50"
                    onClick={() => {
                      setOcrMetadata(null);
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
            </div>
          )}
        </>
      )}

      {/* Manual Entry Mode */}
      {mode === 'manual' && (
        <div className="bg-white rounded-lg shadow p-6">
          <h2 className="text-xl font-semibold mb-4">Manual Expense Entry</h2>

          <div className="space-y-6">
            {/* Date picker - shared across all items */}
            <div>
              <label className="block text-sm font-medium mb-2">Date (for all items)</label>
              <input
                type="date"
                value={manualDate}
                onChange={(e) => setManualDate(e.target.value)}
                className="w-full px-3 py-2 border rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
              />
            </div>

            {/* Multiple expense items */}
            <div className="space-y-4">
              <div className="flex justify-between items-center">
                <h3 className="text-md font-medium text-gray-700">Expense Items ({manualItems.length})</h3>
                <button
                  onClick={addManualItem}
                  className="px-3 py-1.5 text-sm bg-blue-500 text-white rounded-md hover:bg-blue-600 transition-colors"
                >
                  + Add Another Item
                </button>
              </div>

              {manualItems.map((item, index) => (
                <div key={item.id} className="border-2 border-gray-200 rounded-lg p-4 bg-gray-50 relative">
                  <div className="flex justify-between items-start mb-3">
                    <span className="inline-block px-2 py-1 bg-blue-100 text-blue-700 text-xs font-semibold rounded">
                      Item #{index + 1}
                    </span>
                    {manualItems.length > 1 && (
                      <button
                        onClick={() => removeManualItem(item.id)}
                        className="px-2 py-1 text-xs bg-red-500 text-white rounded hover:bg-red-600 transition-colors"
                        title="Remove this item"
                      >
                        Remove
                      </button>
                    )}
                  </div>

                  <div className="grid grid-cols-1 gap-3">
                    <div>
                      <label className="block text-sm font-medium mb-1 text-gray-700">Description</label>
                      <input
                        type="text"
                        value={item.description}
                        onChange={(e) => updateManualItem(item.id, 'description', e.target.value)}
                        placeholder="e.g., Grocery shopping, Transport fare"
                        className="w-full px-3 py-2 border rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 text-sm"
                      />
                    </div>

                    <div className="grid grid-cols-1 md:grid-cols-2 gap-3">
                      <div>
                        <label className="block text-sm font-medium mb-1 text-gray-700">Amount ($)</label>
                        <input
                          type="number"
                          value={item.amount}
                          onChange={(e) => updateManualItem(item.id, 'amount', e.target.value)}
                          min="0"
                          step="0.01"
                          placeholder="0.00"
                          className="w-full px-3 py-2 border rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 text-sm"
                        />
                      </div>

                      <div>
                        <label className="block text-sm font-medium mb-1 text-gray-700">Assign to</label>
                        <select
                          value={item.assignedMemberId}
                          onChange={(e) => updateManualItem(item.id, 'assignedMemberId', e.target.value)}
                          className="w-full px-3 py-2 border rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 text-sm"
                        >
                          {householdMembers.map(member => (
                            <option key={member.id} value={member.id}>
                              {member.userName} ({member.role})
                            </option>
                          ))}
                        </select>
                      </div>
                    </div>
                  </div>
                </div>
              ))}
            </div>

            {/* Summary */}
            <div className="border-t pt-4">
              <div className="bg-blue-50 rounded-lg p-4">
                <div className="flex justify-between items-center">
                  <span className="font-medium text-gray-700">Total Amount:</span>
                  <span className="text-2xl font-bold text-blue-700">
                    ${manualItems.reduce((sum, item) => sum + (parseFloat(item.amount) || 0), 0).toFixed(2)}
                  </span>
                </div>
                <p className="text-xs text-gray-600 mt-2">
                  {manualItems.filter(i => i.description && i.amount && i.assignedMemberId).length} of {manualItems.length} item(s) ready to submit
                </p>
              </div>
            </div>

            <button
              onClick={handleSubmitManualExpense}
              disabled={loading || !selectedCategory || !selectedHousehold}
              className="w-full px-6 py-3 bg-green-600 text-white rounded-md
                hover:bg-green-700 disabled:bg-gray-300 disabled:cursor-not-allowed
                transition-colors font-medium text-lg"
            >
              {loading ? 'Creating...' : `✓ Create ${manualItems.filter(i => i.description && i.amount && i.assignedMemberId).length} Expense(s)`}
            </button>
          </div>
        </div>
      )}

      {/* Confirmation Modal */}
      {showConfirmModal && items.length > 0 && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4">
          <div className="bg-white rounded-lg max-w-3xl w-full max-h-[90vh] overflow-y-auto shadow-2xl">
            <div className="sticky top-0 bg-gradient-to-r from-green-600 to-green-700 text-white p-6 rounded-t-lg">
              <h2 className="text-2xl font-bold">Confirm Receipt Submission</h2>
              <p className="text-green-100 mt-1">Please review the expense breakdown before confirming</p>
            </div>

            <div className="p-6">
              {/* Receipt Summary */}
              <div className="mb-6 bg-gray-50 p-4 rounded-lg border-2 border-gray-200">
                <h3 className="font-semibold text-lg mb-3 text-gray-800">Receipt Details</h3>
                <div className="grid grid-cols-2 gap-3 text-sm">
                  <div>
                    <span className="text-gray-600">Merchant:</span>
                    <span className="ml-2 font-medium text-gray-900">{ocrMetadata?.merchantName || 'Not specified'}</span>
                  </div>
                  <div>
                    <span className="text-gray-600">Date:</span>
                    <span className="ml-2 font-medium text-gray-900">
                      {ocrMetadata?.receiptDate ? new Date(ocrMetadata.receiptDate).toLocaleDateString() : 'N/A'}
                    </span>
                  </div>
                  <div>
                    <span className="text-gray-600">Total Amount:</span>
                    <span className="ml-2 font-medium text-gray-900">${ocrMetadata?.totalAmount?.toFixed(2)}</span>
                  </div>
                  <div>
                    <span className="text-gray-600">Category:</span>
                    <span className="ml-2 font-medium text-gray-900">{selectedCategory?.name}</span>
                  </div>
                </div>
                <div className="mt-3 flex gap-3 items-center">
                  {includeServiceCharge && (
                    <span className="inline-block bg-blue-100 text-blue-800 text-xs px-3 py-1 rounded-full font-medium">
                      + 10% Service Charge
                    </span>
                  )}
                  {includeGST && (
                    <span className="inline-block bg-purple-100 text-purple-800 text-xs px-3 py-1 rounded-full font-medium">
                      + 9% GST
                    </span>
                  )}
                </div>
              </div>

              {/* Member Expense Breakdown */}
              <div className="mb-6">
                <h3 className="font-semibold text-lg mb-3 text-gray-800">Expense Breakdown by Member</h3>
                <div className="space-y-3">
                  {(() => {
                    const memberTotals = calculateMemberTotals();
                    return householdMembers
                      .filter(member => (memberTotals[member.id] || 0) > 0)
                      .map(member => {
                        const memberTotal = memberTotals[member.id] || 0;

                        // Get all items assigned to this member
                        const memberItems = items
                          .map(item => {
                            const qty = itemAssignments[item.id]?.[member.id] || 0;
                            if (qty === 0) return null;

                            const unitPrice = item.totalPrice / item.quantity;
                            const baseAmount = unitPrice * qty;
                            let withService = baseAmount;
                            if (includeServiceCharge) {
                              withService += baseAmount * 0.10;
                            }
                            let totalAmount = withService;
                            if (includeGST) {
                              totalAmount += withService * 0.09;
                            }

                            return {
                              name: item.itemName,
                              qty,
                              amount: totalAmount
                            };
                          })
                          .filter(Boolean);

                        return (
                          <div key={member.id} className="bg-white border-2 border-gray-200 rounded-lg p-4 hover:border-green-300 transition-colors">
                            <div className="flex justify-between items-start mb-2">
                              <div>
                                <h4 className="font-semibold text-gray-900">{member.userName}</h4>
                                <p className="text-xs text-gray-500">{member.role}</p>
                              </div>
                              <div className="text-right">
                                <p className="text-lg font-bold text-green-600">${memberTotal.toFixed(2)}</p>
                                <p className="text-xs text-gray-500">{memberItems.length} item(s)</p>
                              </div>
                            </div>

                            <div className="mt-3 pt-3 border-t border-gray-100">
                              <p className="text-xs font-medium text-gray-600 mb-2">Items:</p>
                              <div className="space-y-1">
                                {memberItems.map((item: { name: string; qty: number; amount: number }, idx) => (
                                  <div key={idx} className="flex justify-between text-xs text-gray-600">
                                    <span>{item.name} (×{item.qty})</span>
                                    <span className="font-medium">${item.amount.toFixed(2)}</span>
                                  </div>
                                ))}
                              </div>
                            </div>
                          </div>
                        );
                      });
                  })()}
                </div>
              </div>

              {/* Total Summary */}
              <div className="bg-gradient-to-r from-green-50 to-emerald-50 border-2 border-green-200 rounded-lg p-4">
                <div className="flex justify-between items-center">
                  <span className="text-gray-700 font-medium">Total Expenses to Create:</span>
                  <span className="text-2xl font-bold text-green-700">
                    ${Object.values(calculateMemberTotals()).reduce((sum, val) => sum + val, 0).toFixed(2)}
                  </span>
                </div>
                <p className="text-xs text-gray-600 mt-2">
                  {Object.values(calculateMemberTotals()).filter(val => val > 0).length} expense(s) will be created for household members
                </p>
              </div>
            </div>

            {/* Modal Actions */}
            <div className="sticky bottom-0 bg-gray-50 px-6 py-4 border-t-2 border-gray-200 flex justify-end gap-3 rounded-b-lg">
              <button
                onClick={() => setShowConfirmModal(false)}
                className="px-5 py-2.5 border-2 border-gray-300 text-gray-700 rounded-lg hover:bg-gray-100 font-medium transition-colors"
                disabled={loading}
              >
                Cancel
              </button>
              <button
                onClick={handleFinalConfirm}
                className="px-5 py-2.5 bg-green-600 text-white rounded-lg hover:bg-green-700 disabled:bg-gray-300 font-medium transition-colors shadow-sm"
                disabled={loading}
              >
                {loading ? 'Creating Expenses...' : 'Confirm & Create Expenses'}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
