import React, { useState, useEffect } from 'react';
import { getCustomers, createCustomer, updateCustomer, deleteCustomer, getCustomerDebitBalance } from '../services/api';

function CustomerManagement() {
  const [customers, setCustomers] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');
  const [showForm, setShowForm] = useState(false);
  const [editingCustomer, setEditingCustomer] = useState(null);
  const [filterType, setFilterType] = useState('All');
  
  const [formData, setFormData] = useState({
    name: '',
    phone: '',
    email: '',
    address: '',
    type: 0
  });

  const customerTypes = ['WalkIn', 'Online', 'Royal'];

  useEffect(() => {
    loadCustomers();
  }, []);

  const loadCustomers = async () => {
    try {
      setLoading(true);
      const data = await getCustomers();
      setCustomers(data);
      setError('');
    } catch (err) {
      setError('Failed to load customers');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  const handleInputChange = (e) => {
    const { name, value } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: value
    }));
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    
    try {
      setLoading(true);
      setError('');
      
      const customerData = {
        ...formData,
        type: parseInt(formData.type)
      };

      if (editingCustomer) {
        await updateCustomer(editingCustomer.id, customerData);
        setSuccess('Customer updated successfully!');
      } else {
        await createCustomer(customerData);
        setSuccess('Customer created successfully!');
      }

      resetForm();
      loadCustomers();
      
      setTimeout(() => setSuccess(''), 3000);
    } catch (err) {
      setError('Failed to save customer: ' + err.message);
    } finally {
      setLoading(false);
    }
  };

  const handleEdit = (customer) => {
    setEditingCustomer(customer);
    setFormData({
      name: customer.name,
      phone: customer.phone,
      email: customer.email,
      address: customer.address,
      type: customerTypes.indexOf(customer.type)
    });
    setShowForm(true);
  };

  const handleDelete = async (id) => {
    if (!window.confirm('Are you sure you want to delete this customer?')) {
      return;
    }

    try {
      setLoading(true);
      await deleteCustomer(id);
      setSuccess('Customer deleted successfully!');
      loadCustomers();
      setTimeout(() => setSuccess(''), 3000);
    } catch (err) {
      setError('Failed to delete customer: ' + err.message);
    } finally {
      setLoading(false);
    }
  };

  const resetForm = () => {
    setFormData({
      name: '',
      phone: '',
      email: '',
      address: '',
      type: 0
    });
    setEditingCustomer(null);
    setShowForm(false);
  };

  const filteredCustomers = filterType === 'All' 
    ? customers 
    : customers.filter(c => c.type === filterType);

  return (
    <div className="customer-management">
      <div className="page-header">
        <h2>Customer Management</h2>
        <div className="header-actions">
          <select
            className="type-filter"
            value={filterType}
            onChange={(e) => setFilterType(e.target.value)}
          >
            <option value="All">All Types</option>
            <option value="WalkIn">Walk-In</option>
            <option value="Online">Online</option>
            <option value="Royal">Royal</option>
          </select>
          <button 
            className="btn btn-primary"
            onClick={() => setShowForm(!showForm)}
          >
            {showForm ? '✕ Cancel' : '+ Add Customer'}
          </button>
        </div>
      </div>

      {error && <div className="error">{error}</div>}
      {success && <div className="success">{success}</div>}

      {showForm && (
        <div className="customer-form-card">
          <h3>{editingCustomer ? 'Edit Customer' : 'Add New Customer'}</h3>
          <form onSubmit={handleSubmit} className="customer-form">
            <div className="form-row">
              <div className="form-group">
                <label>Name *</label>
                <input
                  type="text"
                  name="name"
                  value={formData.name}
                  onChange={handleInputChange}
                  required
                  placeholder="Customer name"
                />
              </div>

              <div className="form-group">
                <label>Phone *</label>
                <input
                  type="tel"
                  name="phone"
                  value={formData.phone}
                  onChange={handleInputChange}
                  required
                  placeholder="Phone number"
                />
              </div>
            </div>

            <div className="form-row">
              <div className="form-group">
                <label>Email</label>
                <input
                  type="email"
                  name="email"
                  value={formData.email}
                  onChange={handleInputChange}
                  placeholder="Email address"
                />
              </div>

              <div className="form-group">
                <label>Customer Type *</label>
                <select
                  name="type"
                  value={formData.type}
                  onChange={handleInputChange}
                  required
                >
                  <option value="0">Walk-In</option>
                  <option value="1">Online</option>
                  <option value="2">Royal (Credit)</option>
                </select>
              </div>
            </div>

            <div className="form-group">
              <label>Address</label>
              <textarea
                name="address"
                value={formData.address}
                onChange={handleInputChange}
                placeholder="Customer address"
                rows="2"
              />
            </div>

            <div className="form-actions">
              <button type="button" className="btn btn-secondary" onClick={resetForm}>
                Cancel
              </button>
              <button type="submit" className="btn btn-primary" disabled={loading}>
                {loading ? 'Saving...' : (editingCustomer ? 'Update Customer' : 'Create Customer')}
              </button>
            </div>
          </form>
        </div>
      )}

      <div className="customers-table-container">
        <table className="customers-table">
          <thead>
            <tr>
              <th>ID</th>
              <th>Name</th>
              <th>Phone</th>
              <th>Email</th>
              <th>Type</th>
              <th>Current Debit</th>
              <th>Last Purchase</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            {loading && customers.length === 0 ? (
              <tr>
                <td colSpan="8" style={{ textAlign: 'center', padding: '40px' }}>
                  Loading customers...
                </td>
              </tr>
            ) : filteredCustomers.length === 0 ? (
              <tr>
                <td colSpan="8" style={{ textAlign: 'center', padding: '40px' }}>
                  No customers found
                </td>
              </tr>
            ) : (
              filteredCustomers.map(customer => (
                <tr key={customer.id}>
                  <td>{customer.id}</td>
                  <td className="customer-name">{customer.name}</td>
                  <td>{customer.phone}</td>
                  <td>{customer.email || '-'}</td>
                  <td>
                    <span className={`type-badge type-${customer.type.toLowerCase()}`}>
                      {customer.type}
                    </span>
                  </td>
                  <td>
                    {customer.currentDebit > 0 ? (
                      <span className="debit-amount">{customer.currentDebit.toLocaleString()} MMK</span>
                    ) : '-'}
                  </td>
                  <td>{customer.lastPurchase ? new Date(customer.lastPurchase).toLocaleDateString() : 'Never'}</td>
                  <td className="actions">
                    <button 
                      className="btn-icon btn-edit"
                      onClick={() => handleEdit(customer)}
                      title="Edit"
                    >
                      ✏️
                    </button>
                    <button 
                      className="btn-icon btn-delete"
                      onClick={() => handleDelete(customer.id)}
                      title="Delete"
                    >
                      🗑️
                    </button>
                  </td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
}

export default CustomerManagement;
