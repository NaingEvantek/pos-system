import React, { useState, useEffect } from "react";
import {
  getSalesSummary,
  getInventoryReport,
  getCustomerDebitReport,
  getDailySalesReport,
} from "../services/api";

function Reports() {
  const [activeReport, setActiveReport] = useState("sales");
  const [salesData, setSalesData] = useState(null);
  const [inventoryData, setInventoryData] = useState(null);
  const [debitData, setDebitData] = useState(null);
  const [dailyData, setDailyData] = useState(null);
  const [loading, setLoading] = useState(false);
  const [dateRange, setDateRange] = useState({
    startDate: new Date().toISOString().split("T")[0],
    endDate: new Date().toISOString().split("T")[0],
  });

  useEffect(() => {
    loadReportData();
  }, [activeReport, dateRange]);

  const loadReportData = async () => {
    setLoading(true);
    try {
      switch (activeReport) {
        case "sales":
          const sales = await getSalesSummary(
            dateRange.startDate,
            dateRange.endDate,
          );
          setSalesData(sales);
          break;
        case "inventory":
          const inventory = await getInventoryReport();
          setInventoryData(inventory);
          break;
        case "debits":
          const debits = await getCustomerDebitReport();
          setDebitData(debits);
          break;
        case "daily":
          const daily = await getDailySalesReport(7);
          setDailyData(daily);
          break;
      }
    } catch (err) {
      console.error("Failed to load report:", err);
    } finally {
      setLoading(false);
    }
  };

  const renderSalesReport = () => (
    <div className="report-content">
      <div className="stats-grid">
        <div className="stat-card">
          <h3>${salesData?.totalRevenue?.toLocaleString() || "0"}</h3>
          <p>Total Revenue</p>
        </div>
        <div className="stat-card">
          <h3>{salesData?.totalSales || 0}</h3>
          <p>Total Sales</p>
        </div>
        <div className="stat-card">
          <h3>${salesData?.averageSale?.toLocaleString() || "0"}</h3>
          <p>Average Sale</p>
        </div>
        <div className="stat-card">
          <h3>${salesData?.totalTax?.toLocaleString() || "0"}</h3>
          <p>Total Tax</p>
        </div>
      </div>

      {salesData?.paymentMethods && (
        <div className="report-section">
          <h3>Payment Methods</h3>
          <table className="report-table">
            <thead>
              <tr>
                <th>Method</th>
                <th>Count</th>
                <th>Total</th>
              </tr>
            </thead>
            <tbody>
              {salesData.paymentMethods.map((pm, idx) => (
                <tr key={idx}>
                  <td>{pm.method}</td>
                  <td>{pm.count}</td>
                  <td>{pm.total.toLocaleString()}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {salesData?.topProducts && (
        <div className="report-section">
          <h3>Top Products</h3>
          <table className="report-table">
            <thead>
              <tr>
                <th>Product</th>
                <th>Quantity Sold</th>
                <th>Revenue</th>
              </tr>
            </thead>
            <tbody>
              {salesData.topProducts.map((product, idx) => (
                <tr key={idx}>
                  <td>{product.product}</td>
                  <td>{product.quantity}</td>
                  <td>{product.revenue.toLocaleString()}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );

  const renderInventoryReport = () => (
    <div className="report-content">
      <div className="stats-grid">
        <div className="stat-card">
          <h3>{inventoryData?.totalProducts || 0}</h3>
          <p>Total Products</p>
        </div>
        <div className="stat-card">
          <h3>${inventoryData?.totalValue?.toFixed(2) || "0.00"}</h3>
          <p>Inventory Value</p>
        </div>
        <div className="stat-card alert">
          <h3>{inventoryData?.lowStockItems?.length || 0}</h3>
          <p>Low Stock Items</p>
        </div>
        <div className="stat-card alert">
          <h3>{inventoryData?.outOfStockItems?.length || 0}</h3>
          <p>Out of Stock</p>
        </div>
      </div>

      {inventoryData?.lowStockItems &&
        inventoryData.lowStockItems.length > 0 && (
          <div className="report-section">
            <h3>⚠️ Low Stock Alert</h3>
            <table className="report-table">
              <thead>
                <tr>
                  <th>Product</th>
                  <th>Stock</th>
                  <th>Price</th>
                </tr>
              </thead>
              <tbody>
                {inventoryData.lowStockItems.map((item) => (
                  <tr key={item.id}>
                    <td>{item.name}</td>
                    <td className="low-stock">{item.stock}</td>
                    <td>{item.retailPrice.toLocaleString()}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}

      {inventoryData?.byCategory && (
        <div className="report-section">
          <h3>Inventory by Category</h3>
          <table className="report-table">
            <thead>
              <tr>
                <th>Category</th>
                <th>Products</th>
                <th>Total Value</th>
              </tr>
            </thead>
            <tbody>
              {inventoryData.byCategory.map((cat, idx) => (
                <tr key={idx}>
                  <td>{cat.category}</td>
                  <td>{cat.count}</td>
                  <td>{cat.totalValue.toLocaleString()}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );

  const renderDebitReport = () => (
    <div className="report-content">
      <div className="stats-grid">
        <div className="stat-card">
          <h3>{debitData?.totalRoyalCustomers || 0}</h3>
          <p>Royal Customers</p>
        </div>
        <div className="stat-card">
          <h3>{debitData?.customersWithDebit || 0}</h3>
          <p>With Outstanding Debt</p>
        </div>
        <div className="stat-card alert">
          <h3>
            {debitData?.totalOutstandingDebit?.toLocaleString() || "0"} MMK
          </h3>
          <p>Total Outstanding</p>
        </div>
      </div>

      {debitData?.customers && debitData.customers.length > 0 && (
        <div className="report-section">
          <h3>Customer Debits (Unlimited Credit)</h3>
          <table className="report-table">
            <thead>
              <tr>
                <th>Customer</th>
                <th>Phone</th>
                <th>Current Debit</th>
              </tr>
            </thead>
            <tbody>
              {debitData.customers.map((customer) => (
                <tr key={customer.id}>
                  <td>{customer.name}</td>
                  <td>{customer.phone}</td>
                  <td className="debit-amount">
                    {customer.currentDebit.toLocaleString()} MMK
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );

  const renderDailyReport = () => (
    <div className="report-content">
      <div className="stats-grid">
        <div className="stat-card">
          <h3>${dailyData?.totalRevenue?.toLocaleString() || "0"}</h3>
          <p>Total Revenue (7 days)</p>
        </div>
        <div className="stat-card">
          <h3>{dailyData?.totalSales || 0}</h3>
          <p>Total Sales</p>
        </div>
        <div className="stat-card">
          <h3>${dailyData?.averageDailyRevenue?.toLocaleString() || "0"}</h3>
          <p>Average Daily</p>
        </div>
      </div>

      {dailyData?.dailyBreakdown && (
        <div className="report-section">
          <h3>Daily Breakdown</h3>
          <table className="report-table">
            <thead>
              <tr>
                <th>Date</th>
                <th>Sales</th>
                <th>Revenue</th>
                {/* <th>Tax</th> */}
              </tr>
            </thead>
            <tbody>
              {dailyData.dailyBreakdown.map((day, idx) => (
                <tr key={idx}>
                  <td>{new Date(day.date).toLocaleDateString()}</td>
                  <td>{day.salesCount}</td>
                  <td>{day.revenue.toLocaleString()}</td>
                  {/* <td>${day.tax.toLocaleString()}</td> */}
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );

  return (
    <div className="reports-page">
      <div className="page-header">
        <h2>Reports & Analytics</h2>
        <div className="date-range-picker">
          <input
            type="date"
            value={dateRange.startDate}
            onChange={(e) =>
              setDateRange({ ...dateRange, startDate: e.target.value })
            }
          />
          <span>to</span>
          <input
            type="date"
            value={dateRange.endDate}
            onChange={(e) =>
              setDateRange({ ...dateRange, endDate: e.target.value })
            }
          />
        </div>
      </div>

      <div className="report-tabs">
        <button
          className={`report-tab ${activeReport === "sales" ? "active" : ""}`}
          onClick={() => setActiveReport("sales")}
        >
          📊 Sales Summary
        </button>
        <button
          className={`report-tab ${activeReport === "inventory" ? "active" : ""}`}
          onClick={() => setActiveReport("inventory")}
        >
          📦 Inventory
        </button>
        <button
          className={`report-tab ${activeReport === "debits" ? "active" : ""}`}
          onClick={() => setActiveReport("debits")}
        >
          💳 Customer Debits
        </button>
        <button
          className={`report-tab ${activeReport === "daily" ? "active" : ""}`}
          onClick={() => setActiveReport("daily")}
        >
          📈 Daily Trend
        </button>
      </div>

      {loading ? (
        <div className="loading">Loading report data...</div>
      ) : (
        <>
          {activeReport === "sales" && salesData && renderSalesReport()}
          {activeReport === "inventory" &&
            inventoryData &&
            renderInventoryReport()}
          {activeReport === "debits" && debitData && renderDebitReport()}
          {activeReport === "daily" && dailyData && renderDailyReport()}
        </>
      )}
    </div>
  );
}

export default Reports;
