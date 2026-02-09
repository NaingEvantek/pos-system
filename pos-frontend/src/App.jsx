import { useState, useEffect } from "react";
import {
  getProducts,
  createSale,
  printReceipt,
  isAuthenticated,
  getCurrentUser,
} from "./services/api";
import Login from "./components/Login";
import Sidebar from "./components/Sidebar";
import ProductList from "./components/ProductList";
import Cart from "./components/Cart";
import ProductManagement from "./components/ProductManagement";
import CustomerManagement from "./components/CustomerManagement";
import SalesHistory from "./components/SalesHistory";
import Reports from "./components/Reports";
import "./index.css";
import logo from "../assets/images/zllogo4.png";

function App() {
  const [isLoggedIn, setIsLoggedIn] = useState(false);
  const [currentUser, setCurrentUser] = useState(null);
  const [currentView, setCurrentView] = useState("pos");
  const [products, setProducts] = useState([]);
  const [cart, setCart] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");

  useEffect(() => {
    // Check if user is already logged in
    if (isAuthenticated()) {
      const user = getCurrentUser();
      setCurrentUser(user);
      setIsLoggedIn(true);
    }
    setLoading(false);
  }, []);

  useEffect(() => {
    if (isLoggedIn && currentView === "pos") {
      loadProducts();
    }
  }, [currentView, isLoggedIn]);

  const handleLoginSuccess = (user) => {
    setCurrentUser(user);
    setIsLoggedIn(true);
  };

  const handleLogout = () => {
    setIsLoggedIn(false);
    setCurrentUser(null);
    setCart([]);
    setCurrentView("pos");
  };

  const loadProducts = async () => {
    try {
      setLoading(true);
      const data = await getProducts();
      setProducts(data);
      setError("");
    } catch (err) {
      setError(
        `Failed to load products. Make sure the API is running on ${import.meta.env.VITE_Backend_API_URL}`,
      );
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  const addToCart = (product) => {
    const existingItem = cart.find((item) => item.productId === product.id);

    if (product.stock > 0 && existingItem?.quantity <= product.stock) {
      if (existingItem) {
        setCart(
          cart.map((item) =>
            item.productId === product.id
              ? {
                  ...item,
                  quantity: item.quantity + 1,
                  total: (item.quantity + 1) * item.unitPrice,
                }
              : item,
          ),
        );
      } else {
        setCart([
          ...cart,
          {
            productId: product.id,
            productName: product.name,
            quantity: 1,
            unitPrice: product.price,
            total: product.price,
          },
        ]);
      }
    } else {
      setError(`Insufficient stock for this product : ${product.name}`);
      setTimeout(() => {
        setError("");
      }, 3000);
    }
  };

  const updateQuantity = (productId, delta) => {
    setCart(
      cart.map((item) => {
        if (item.productId === productId) {
          const newQuantity = Math.max(1, item.quantity + delta);
          return {
            ...item,
            quantity: newQuantity,
            total: newQuantity * item.unitPrice,
          };
        }
        return item;
      }),
    );
  };

  const removeFromCart = (productId) => {
    setCart(cart.filter((item) => item.productId !== productId));
  };

  const clearCart = () => {
    setCart([]);
  };

  const handleCheckout = async (checkoutData) => {
    if (cart.length === 0) {
      setError("Cart is empty");
      return;
    }

    try {
      setLoading(true);
      setError("");

      let customerId = checkoutData.customerId;

      // Create customer if Walk-In or Online and not exists
      if (
        (checkoutData.customerType === "walkin" ||
          checkoutData.customerType === "online") &&
        !customerId
      ) {
        try {
          const newCustomerData = {
            name: checkoutData.customerName,
            phone: checkoutData.customerPhone || "",
            email: checkoutData.customerEmail || "",
            address: checkoutData.customerAddress || "",
            type: checkoutData.customerType === "online" ? 1 : 0, // 0=WalkIn, 1=Online
          };

          const { createCustomer } = await import("./services/api");
          const createdCustomer = await createCustomer(newCustomerData);
          customerId = createdCustomer.id;
        } catch (err) {
          console.error("Failed to create customer:", err);
          // Continue with sale even if customer creation fails
        }
      }

      // Create sale
      const saleData = {
        customerName: checkoutData.customerName,
        customerId: customerId,
        paymentMethod: checkoutData.paymentMethod,
        discount: checkoutData.discountAmount,
        paymentAmount: checkoutData.paymentAmount,
        balance: checkoutData.balance,
        isPaid: checkoutData.isPaid,
        priceType: checkoutData.priceType === "wholesale" ? 1 : 0, // 0=Retail, 1=Wholesale
        items: cart,
      };

      const sale = await createSale(saleData);

      // Print receipt

      async function imageToBase64(url) {
        const res = await fetch(url);
        const blob = await res.blob();

        return new Promise((resolve) => {
          const reader = new FileReader();
          reader.onloadend = () => resolve(reader.result); // data:image/png;base64,...
          reader.readAsDataURL(blob);
        });
      }

      const logoBase64 = await imageToBase64(logo);

      const receiptData = {
        saleId: sale.id,
        saleDate: sale.saleDate,
        customerName: sale.customerName,
        paymentMethod: sale.paymentMethod,
        subtotal: sale.subtotal,
        tax: sale.tax || 0,
        discount: sale.discount,
        totalAmount: sale.totalAmount,
        paymentAmount: checkoutData.paymentAmount,
        balance: checkoutData.balance,
        items: sale.items,
        logoBase64: logoBase64,
      };

      try {
        const printResult = await printReceipt(receiptData);

        if (printResult.previewHtml) {
          const printWindow = window.open("", "_blank");
          if (printWindow) {
            printWindow.document.write(printResult.previewHtml);
            printWindow.document.close();

            setTimeout(() => {
              printWindow.print();
            }, 500);
          }
        }

        if (checkoutData.balance >= 0) {
          setSuccess(
            `Sale completed! Receipt #${sale.id}. Change: $${checkoutData.balance.toFixed(2)}`,
          );
        } else {
          setSuccess(
            `Sale completed! Receipt #${sale.id}. Balance on credit: $${Math.abs(checkoutData.balance).toFixed(2)}`,
          );
        }
      } catch (printErr) {
        console.error("Print error:", printErr);
        setSuccess(
          `Sale completed! Receipt #${sale.id}. (Print service unavailable)`,
        );
      }

      clearCart();
      loadProducts();

      setTimeout(() => setSuccess(""), 5000);
    } catch (err) {
      setError("Failed to complete sale: " + err.message);
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  const renderView = () => {
    switch (currentView) {
      case "pos":
        return (
          <>
            {error && <div className="error">{error}</div>}
            {success && <div className="success">{success}</div>}

            {loading && products.length === 0 ? (
              <div className="loading">Loading products...</div>
            ) : (
              <div className="main-content">
                <ProductList products={products} onAddToCart={addToCart} />
                <Cart
                  cart={cart}
                  onUpdateQuantity={updateQuantity}
                  onRemoveItem={removeFromCart}
                  onCheckout={handleCheckout}
                  loading={loading}
                />
              </div>
            )}
          </>
        );

      case "products":
        return <ProductManagement />;

      case "customers":
        return <CustomerManagement />;

      case "sales":
        return <SalesHistory />;

      case "reports":
        return <Reports />;

      default:
        return <div>View not found</div>;
    }
  };

  // Show login page if not authenticated
  if (!isLoggedIn) {
    return <Login onLoginSuccess={handleLoginSuccess} />;
  }

  return (
    <div className="app">
      <Sidebar
        currentView={currentView}
        onViewChange={setCurrentView}
        onLogout={handleLogout}
      />

      <div className="main-container">
        <div className="content-wrapper">{renderView()}</div>
      </div>
    </div>
  );
}

export default App;
