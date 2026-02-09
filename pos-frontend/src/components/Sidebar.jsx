import React from 'react';
import { logout, getCurrentUser } from '../services/api';

function Sidebar({ currentView, onViewChange, onLogout }) {
  const user = getCurrentUser();

  const menuItems = [
    { id: 'pos', label: '🛒 Point of Sale', icon: '💳' },
    { id: 'products', label: '📦 Manage Products', icon: '📦' },
    { id: 'customers', label: '👥 Customers', icon: '👥' },
    { id: 'sales', label: '📊 Sales History', icon: '📊' },
    { id: 'reports', label: '📈 Reports', icon: '📈' },
  ];

  const handleLogout = () => {
    logout();
    if (onLogout) {
      onLogout();
    }
  };

  return (
    <div className="sidebar">
      <div className="sidebar-header">
        <h2>POS System</h2>
        <p>Point of Sale</p>
      </div>

      {user && (
        <div className="user-info">
          <div className="user-avatar">
            {user.fullName?.charAt(0)?.toUpperCase() || user.username?.charAt(0)?.toUpperCase() || 'U'}
          </div>
          <div className="user-details">
            <div className="user-name">{user.fullName || user.username}</div>
            <div className="user-role">{user.role}</div>
          </div>
        </div>
      )}
      
      <nav className="sidebar-nav">
        {menuItems.map(item => (
          <button
            key={item.id}
            className={`nav-item ${currentView === item.id ? 'active' : ''}`}
            onClick={() => onViewChange(item.id)}
          >
            <span className="nav-icon">{item.icon}</span>
            <span className="nav-label">{item.label}</span>
          </button>
        ))}
      </nav>

      <div className="sidebar-footer">
        <button className="btn-logout" onClick={handleLogout}>
          <span>🚪</span>
          <span>Logout</span>
        </button>
        <p>v1.0.0</p>
      </div>
    </div>
  );
}

export default Sidebar;
