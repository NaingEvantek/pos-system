import React, { useState, useEffect } from "react";
import { getSales, getTodaySales } from "../services/api";

function SalesHistory() {
  const [sales, setSales] = useState([]);
  const [todayStats, setTodayStats] = useState(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");
  const [selectedSale, setSelectedSale] = useState(null);

  useEffect(() => {
    loadSales();
    loadTodayStats();
  }, []);

  const loadSales = async () => {
    try {
      setLoading(true);
      const data = await getSales();
      setSales(data);
      setError("");
    } catch (err) {
      setError("Failed to load sales history");
      console.error(err);
    } finally {
      setLoading(false);
    }
  };
  console.log(sales);
  const loadTodayStats = async () => {
    try {
      const data = await getTodaySales();
      setTodayStats(data);
    } catch (err) {
      console.error("Failed to load today stats:", err);
    }
  };

  const formatDate = (dateString) => {
    const date = new Date(dateString);
    return date.toLocaleDateString() + " " + date.toLocaleTimeString();
  };

  // function formatDate(date) {
  //   const options = {
  //     day: "2-digit",
  //     month: "short",
  //     year: "numeric",
  //     hour: "2-digit",
  //     minute: "2-digit",
  //     hour12: true,
  //   };
  //   return new Intl.DateTimeFormat("en-US", options).format(date);
  // }

  return (
    <div className="sales-history">
      <div className="page-header">
        <h2>Sales History</h2>
        <button className="btn btn-primary" onClick={loadSales}>
          🔄 Refresh
        </button>
      </div>

      {error && <div className="error">{error}</div>}

      {todayStats && (
        <div className="stats-cards">
          <div className="stat-card">
            <div className="stat-icon">📊</div>
            <div className="stat-content">
              <h3>{todayStats.totalSales}</h3>
              <p>Today's Sales</p>
            </div>
          </div>
          <div className="stat-card stat-card-mmk">
            <div className="stat-icon">💰</div>
            <div className="stat-content">
              <h3>{todayStats.totalRevenue?.toLocaleString()}</h3>
              <p>Today's Revenue</p>
            </div>
          </div>
          <div className="stat-card">
            <div className="stat-icon">🛒</div>
            <div className="stat-content">
              <h3>{sales.length}</h3>
              <p>Total Sales</p>
            </div>
          </div>
        </div>
      )}

      <div className="sales-table-container">
        <table className="sales-table">
          <thead>
            <tr>
              <th>Receipt #</th>
              <th>Date & Time</th>
              <th>Customer</th>
              <th>Items</th>
              <th>Subtotal</th>
              <th>Dis.</th>
              <th>Total</th>
              <th>Payment</th>
              <th>Balance</th>
              <th>Details</th>
            </tr>
          </thead>
          <tbody>
            {loading && sales.length === 0 ? (
              <tr>
                <td
                  colSpan="9"
                  style={{ textAlign: "center", padding: "40px" }}
                >
                  Loading sales...
                </td>
              </tr>
            ) : sales.length === 0 ? (
              <tr>
                <td
                  colSpan="9"
                  style={{ textAlign: "center", padding: "40px" }}
                >
                  No sales found. Make your first sale!
                </td>
              </tr>
            ) : (
              sales.map((sale) => (
                <tr key={sale.id}>
                  <td className="receipt-id">#{sale.id}</td>
                  <td>{formatDate(sale.saleDate)}</td>
                  <td>{sale.customerName || "Walk-in"}</td>
                  <td className="items-count">
                    {sale.items?.length || 0} items
                  </td>

                  <td>{sale.subtotal?.toLocaleString()}</td>
                  <td>{sale.discount.toLocaleString()}</td>
                  <td className="total-amount">
                    {sale.totalAmount?.toLocaleString()}
                  </td>
                  <td className="payment-amount">
                    {sale.paymentAmount?.toLocaleString()}
                  </td>
                  <td className="sale-balance-amount">
                    {(sale.totalAmount - sale.paymentAmount).toLocaleString()}
                  </td>
                  <td>
                    <button
                      className="btn-icon"
                      onClick={() =>
                        setSelectedSale(
                          selectedSale?.id === sale.id ? null : sale,
                        )
                      }
                    >
                      {selectedSale?.id === sale.id ? "▲" : "▼"}
                    </button>
                  </td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>

      {selectedSale && (
        <div
          className="sale-details-modal"
          onClick={() => setSelectedSale(null)}
        >
          <div
            className="sale-details-card"
            onClick={(e) => e.stopPropagation()}
          >
            <div className="modal-header">
              <h3>Sale Details - Receipt #{selectedSale.id}</h3>
              <button
                className="btn-close"
                onClick={() => setSelectedSale(null)}
              >
                ✕
              </button>
            </div>

            <div className="modal-body">
              <div className="detail-row">
                <span>Date:</span>
                <span>{formatDate(selectedSale.saleDate)}</span>
              </div>
              <div className="detail-row">
                <span>Customer:</span>
                <span>{selectedSale.customerName || "Walk-in Customer"}</span>
              </div>
              <div className="detail-row">
                <span>Payment Method:</span>
                <span>{selectedSale.paymentMethod}</span>
              </div>

              <h4 style={{ marginTop: "20px", marginBottom: "10px" }}>
                Items:
              </h4>
              <table className="items-table">
                <thead>
                  <tr>
                    <th>Product</th>
                    <th>Qty</th>
                    <th>Price</th>
                    <th>Total</th>
                  </tr>
                </thead>
                <tbody>
                  {selectedSale.items?.map((item, index) => (
                    <tr key={index}>
                      <td>{item.productName}</td>
                      <td>{item.quantity}</td>
                      <td>{item.unitPrice?.toLocaleString()}</td>
                      <td>{item.total?.toLocaleString()}</td>
                    </tr>
                  ))}
                </tbody>
              </table>

              <div className="detail-totals">
                <div className="detail-row">
                  <span>Subtotal:</span>
                  <span>{selectedSale.subtotal?.toLocaleString()}</span>
                </div>
                <div className="detail-row">
                  <span>Discount :</span>
                  <span>{selectedSale.discount?.toLocaleString()}</span>
                </div>
                <div className="detail-row total">
                  <span>Total:</span>
                  <span>{selectedSale.totalAmount?.toLocaleString()}</span>
                </div>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}

export default SalesHistory;
