import axios from 'axios';

const API_BASE_URL =
  import.meta.env.API_BASE_URL;
const PRINT_SERVICE_URL =
  import.meta.env.PRINT_SERVICE_URL;

const api = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Add token to requests if available
api.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('token');
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

// Handle 401 responses (unauthorized)
api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response ? .status === 401) {
      // Token expired or invalid, redirect to login
      localStorage.removeItem('token');
      localStorage.removeItem('user');
      window.location.href = '/';
    }
    return Promise.reject(error);
  }
);

const printApi = axios.create({
  baseURL: PRINT_SERVICE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Authentication
export const login = async (username, password) => {
  const response = await api.post('/auth/login', {
    username,
    password
  });
  return response.data;
};

export const register = async (userData) => {
  const response = await api.post('/auth/register', userData);
  return response.data;
};

export const logout = () => {
  localStorage.removeItem('token');
  localStorage.removeItem('user');
};

export const getCurrentUser = () => {
  const userStr = localStorage.getItem('user');
  return userStr ? JSON.parse(userStr) : null;
};

export const isAuthenticated = () => {
  return !!localStorage.getItem('token');
};

// Products
export const getProducts = async () => {
  const response = await api.get('/products');
  return response.data;
};

export const getProduct = async (id) => {
  const response = await api.get(`/products/${id}`);
  return response.data;
};

export const createProduct = async (productData) => {
  const response = await api.post('/products', productData);
  return response.data;
};

export const updateProduct = async (id, productData) => {
  const response = await api.put(`/products/${id}`, productData);
  return response.data;
};

export const deleteProduct = async (id) => {
  const response = await api.delete(`/products/${id}`);
  return response.data;
};

// Sales
export const getSales = async () => {
  const response = await api.get('/sales');
  return response.data;
};

export const getSale = async (id) => {
  const response = await api.get(`/sales/${id}`);
  return response.data;
};

export const createSale = async (saleData) => {
  const response = await api.post('/sales', saleData);
  return response.data;
};

export const getTodaySales = async () => {
  const response = await api.get('/sales/today');
  return response.data;
};

// Print Service
export const printReceipt = async (receiptData) => {
  const response = await printApi.post('/print/receipt', receiptData);
  return response.data;
};

export const getAvailablePrinters = async () => {
  const response = await printApi.get('/print/printers');
  return response.data;
};

export const testPrint = async () => {
  const response = await printApi.post('/print/test');
  return response.data;
};

// Customers
export const getCustomers = async () => {
  const response = await api.get('/customers');
  return response.data;
};

export const getCustomer = async (id) => {
  const response = await api.get(`/customers/${id}`);
  return response.data;
};

export const createCustomer = async (customerData) => {
  const response = await api.post('/customers', customerData);
  return response.data;
};

export const updateCustomer = async (id, customerData) => {
  const response = await api.put(`/customers/${id}`, customerData);
  return response.data;
};

export const deleteCustomer = async (id) => {
  const response = await api.delete(`/customers/${id}`);
  return response.data;
};

export const getCustomerDebitBalance = async (id) => {
  const response = await api.get(`/customers/${id}/debit-balance`);
  return response.data;
};

export const getCustomerTransactions = async (id) => {
  const response = await api.get(`/customers/${id}/transactions`);
  return response.data;
};

export const createDebitTransaction = async (transactionData) => {
  const response = await api.post('/customers/debit-transaction', transactionData);
  return response.data;
};

export const getRoyalCustomers = async () => {
  const response = await api.get('/customers/royal');
  return response.data;
};

// Reports
export const getSalesSummary = async (startDate, endDate) => {
  const params = new URLSearchParams();
  if (startDate) params.append('startDate', startDate);
  if (endDate) params.append('endDate', endDate);
  const response = await api.get(`/reports/sales-summary?${params.toString()}`);
  return response.data;
};

export const getInventoryReport = async () => {
  const response = await api.get('/reports/inventory');
  return response.data;
};

export const getCustomerDebitReport = async () => {
  const response = await api.get('/reports/customer-debits');
  return response.data;
};

export const getDailySalesReport = async (days = 7) => {
  const response = await api.get(`/reports/daily-sales?days=${days}`);
  return response.data;
};

export const getCustomerSalesReport = async (startDate, endDate) => {
  const params = new URLSearchParams();
  if (startDate) params.append('startDate', startDate);
  if (endDate) params.append('endDate', endDate);
  const response = await api.get(`/reports/customer-sales?${params.toString()}`);
  return response.data;
};

export const getPriceTypeAnalysis = async (startDate, endDate) => {
  const params = new URLSearchParams();
  if (startDate) params.append('startDate', startDate);
  if (endDate) params.append('endDate', endDate);
  const response = await api.get(`/reports/price-type-analysis?${params.toString()}`);
  return response.data;
};

export default api;