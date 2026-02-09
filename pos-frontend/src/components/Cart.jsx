import React, { useState, useEffect } from 'react';
import { getCustomers } from '../services/api';

function Cart({ cart, onUpdateQuantity, onRemoveItem, onCheckout, loading }) {
  const [customerType, setCustomerType] = useState('walkin');
  const [selectedCustomer, setSelectedCustomer] = useState(null);
  const [royalCustomers, setRoyalCustomers] = useState([]);
  const [customerName, setCustomerName] = useState('');
  const [customerPhone, setCustomerPhone] = useState('');
  const [customerEmail, setCustomerEmail] = useState('');
  const [paymentMethod, setPaymentMethod] = useState('Cash');
  const [discountPercent, setDiscountPercent] = useState(0);
  const [paymentAmount, setPaymentAmount] = useState('');

  useEffect(() => {
    loadRoyalCustomers();
  }, []);

  useEffect(() => {
    // Auto-fill payment amount for walk-in and online customers
    if (customerType !== 'royal' && cart.length > 0) {
      const total = subtotal - discountAmount;
      setPaymentAmount(total.toFixed(2));
    }
  }, [cart, discountPercent, customerType]);

  const loadRoyalCustomers = async () => {
    try {
      const customers = await getCustomers();
      setRoyalCustomers(customers.filter(c => c.type === 'Royal'));
    } catch (err) {
      console.error('Failed to load royal customers:', err);
    }
  };

  const subtotal = cart.reduce((sum, item) => sum + item.total, 0);
  const discountAmount = subtotal * (discountPercent / 100);
  const totalAfterDiscount = subtotal - discountAmount;
  const payment = parseFloat(paymentAmount) || 0;
  const balance = payment - totalAfterDiscount;

  const handleCustomerTypeChange = (type) => {
    setCustomerType(type);
    setSelectedCustomer(null);
    setCustomerName('');
    setCustomerPhone('');
    setCustomerEmail('');
    setDiscountPercent(0);
    setPaymentAmount('');
  };

  const handleRoyalCustomerSelect = (customerId) => {
    const customer = royalCustomers.find(c => c.id === parseInt(customerId));
    setSelectedCustomer(customer || null);
  };

  const handleCheckout = () => {
    // Validation for Walk-In
    if (customerType === 'walkin' && !customerName.trim()) {
      alert('Please enter customer name for Walk-In sale');
      return;
    }

    // Validation for Online
    if (customerType === 'online') {
      if (!customerName.trim() || !customerPhone.trim()) {
        alert('Please enter customer name and phone number for Online customer');
        return;
      }
    }

    // Validation for Royal
    if (customerType === 'royal' && !selectedCustomer) {
      alert('Please select a Royal customer');
      return;
    }

    // Payment validation - Walk-In and Online must be fully paid
    if (customerType !== 'royal') {
      if (payment < totalAfterDiscount) {
        alert(`Walk-In and Online customers must pay in full. Total: $${totalAfterDiscount.toFixed(2)}`);
        return;
      }
    }

    // For Royal customers - no credit limit check (unlimited)
    if (customerType === 'royal' && balance < 0 && selectedCustomer) {
      // Just informational, no validation needed
      console.log('Royal customer buying on credit:', Math.abs(balance));
    }

    const checkoutData = {
      customerType,
      customerName: selectedCustomer?.name || customerName,
      customerPhone: selectedCustomer?.phone || customerPhone,
      customerEmail: selectedCustomer?.email || customerEmail,
      customerId: selectedCustomer?.id,
      paymentMethod,
      discountPercent,
      discountAmount,
      paymentAmount: payment,
      balance,
      priceType: customerType === 'royal' ? 'wholesale' : 'retail',
      isPaid: balance >= 0 // Fully paid if balance is 0 or positive (change given)
    };

    onCheckout(checkoutData);
    
    // Reset form
    setCustomerName('');
    setCustomerPhone('');
    setCustomerEmail('');
    setDiscountPercent(0);
    setPaymentAmount('');
    setSelectedCustomer(null);
  };

  return (
    <div className="cart-section">
      <h2>Shopping Cart</h2>
      
      {/* Customer Type Selection */}
      <div className="customer-type-section">
        <label className="section-label">Customer Type *</label>
        <div className="customer-type-buttons">
          <button
            type="button"
            className={`customer-type-btn ${customerType === 'walkin' ? 'active' : ''}`}
            onClick={() => handleCustomerTypeChange('walkin')}
          >
            🚶 Walk-In
          </button>
          <button
            type="button"
            className={`customer-type-btn ${customerType === 'online' ? 'active' : ''}`}
            onClick={() => handleCustomerTypeChange('online')}
          >
            🌐 Online
          </button>
          <button
            type="button"
            className={`customer-type-btn ${customerType === 'royal' ? 'active' : ''}`}
            onClick={() => handleCustomerTypeChange('royal')}
          >
            👑 Royal
          </button>
        </div>
      </div>

      {/* Customer Information */}
      {customerType === 'royal' ? (
        <div className="form-group">
          <label>Royal Customer *</label>
          <select
            value={selectedCustomer?.id || ''}
            onChange={(e) => handleRoyalCustomerSelect(e.target.value)}
          >
            <option value="">-- Select Royal Customer --</option>
            {royalCustomers.map(customer => (
              <option key={customer.id} value={customer.id}>
                {customer.name} - Debit: {customer.currentDebit.toLocaleString()} MMK
              </option>
            ))}
          </select>
          {selectedCustomer && (
            <div className="customer-info-box">
              <div className="info-row">
                <span>Current Debit:</span>
                <span className="debit-amount">{selectedCustomer.currentDebit.toLocaleString()} MMK</span>
              </div>
              <p className="credit-note">✓ Unlimited Credit Available</p>
            </div>
          )}
        </div>
      ) : (
        <>
          <div className="form-group">
            <label>Customer Name *</label>
            <input
              type="text"
              value={customerName}
              onChange={(e) => setCustomerName(e.target.value)}
              placeholder="Enter customer name"
            />
          </div>
          {customerType === 'online' && (
            <>
              <div className="form-group">
                <label>Phone Number *</label>
                <input
                  type="tel"
                  value={customerPhone}
                  onChange={(e) => setCustomerPhone(e.target.value)}
                  placeholder="Enter phone number"
                />
              </div>
              <div className="form-group">
                <label>Email (Optional)</label>
                <input
                  type="email"
                  value={customerEmail}
                  onChange={(e) => setCustomerEmail(e.target.value)}
                  placeholder="Enter email"
                />
              </div>
            </>
          )}
        </>
      )}

      {/* Cart Items */}
      {cart.length === 0 ? (
        <div className="empty-cart">
          <p>Cart is empty</p>
          <p style={{ fontSize: '12px', marginTop: '10px' }}>
            Click on products to add them
          </p>
        </div>
      ) : (
        <>
          <div className="cart-items">
            {cart.map(item => (
              <div key={item.productId} className="cart-item">
                <div className="cart-item-info">
                  <div className="cart-item-name">{item.productName}</div>
                  <div className="cart-item-details">
                    ${item.unitPrice.toFixed(2)} × {item.quantity} = ${item.total.toFixed(2)}
                  </div>
                </div>
                <div className="cart-item-actions">
                  <button 
                    className="qty-btn"
                    onClick={() => onUpdateQuantity(item.productId, -1)}
                  >
                    -
                  </button>
                  <span style={{ minWidth: '20px', textAlign: 'center' }}>
                    {item.quantity}
                  </span>
                  <button 
                    className="qty-btn"
                    onClick={() => onUpdateQuantity(item.productId, 1)}
                  >
                    +
                  </button>
                  <button 
                    className="remove-btn"
                    onClick={() => onRemoveItem(item.productId)}
                  >
                    ×
                  </button>
                </div>
              </div>
            ))}
          </div>

          <div className="cart-summary">
            <div className="summary-row">
              <span>Subtotal:</span>
              <span>${subtotal.toFixed(2)}</span>
            </div>
            
            {/* Discount */}
            <div className="summary-row discount-row">
              <span className="discount-label">
                Discount:
                <input
                  type="number"
                  className="discount-input"
                  value={discountPercent}
                  onChange={(e) => setDiscountPercent(Math.max(0, Math.min(100, parseFloat(e.target.value) || 0)))}
                  min="0"
                  max="100"
                  step="1"
                  placeholder="0"
                />
                <span className="percent-sign">%</span>
              </span>
              <span className="discount-value">-${discountAmount.toFixed(2)}</span>
            </div>

            <div className="summary-row total-row">
              <span>Total:</span>
              <span>${totalAfterDiscount.toFixed(2)}</span>
            </div>
          </div>

          {/* Payment Section */}
          <div className="payment-section-cart">
            <div className="form-group">
              <label>Payment Method</label>
              <select 
                value={paymentMethod}
                onChange={(e) => setPaymentMethod(e.target.value)}
              >
                <option value="Cash">Cash</option>
                <option value="Credit Card">Credit Card</option>
                <option value="Debit Card">Debit Card</option>
                <option value="Mobile Payment">Mobile Payment</option>
                {customerType === 'royal' && <option value="Credit">Credit (On Account)</option>}
              </select>
            </div>

            <div className="form-group">
              <label>Payment Amount *</label>
              <input
                type="number"
                value={paymentAmount}
                onChange={(e) => setPaymentAmount(e.target.value)}
                placeholder="Enter amount"
                step="0.01"
                min="0"
              />
            </div>

            {/* Balance/Change Display */}
            {payment > 0 && (
              <div className={`balance-box ${balance >= 0 ? 'positive' : 'negative'}`}>
                <div className="balance-label">
                  {balance >= 0 ? '💵 Change' : customerType === 'royal' ? '💳 Balance (Credit)' : '⚠️ Insufficient Payment'}
                </div>
                <div className="balance-amount">
                  ${Math.abs(balance).toFixed(2)}
                </div>
              </div>
            )}

            {/* Royal Customer Credit Info */}
            {customerType === 'royal' && selectedCustomer && balance < 0 && (
              <div className="credit-info-box">
                <p className="info-text">Amount on credit: {Math.abs(balance).toLocaleString()} MMK</p>
                <p className="info-text">
                  New debit: {(selectedCustomer.currentDebit + Math.abs(balance)).toLocaleString()} MMK
                </p>
                <p className="credit-unlimited">✓ Unlimited Credit Available</p>
              </div>
            )}

            {/* Walk-In/Online Payment Warning */}
            {(customerType === 'walkin' || customerType === 'online') && payment > 0 && balance < 0 && (
              <div className="payment-warning">
                ⚠️ {customerType === 'walkin' ? 'Walk-In' : 'Online'} customers must pay in full
              </div>
            )}
          </div>

          <button 
            className="btn btn-primary btn-checkout"
            onClick={handleCheckout}
            disabled={loading || cart.length === 0}
          >
            {loading ? 'Processing...' : `Complete Sale - $${totalAfterDiscount.toFixed(2)}`}
          </button>
        </>
      )}
    </div>
  );
}

export default Cart;
