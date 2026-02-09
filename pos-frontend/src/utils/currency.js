// Format number with commas (MMK currency - no decimals)
export const formatMMK = (amount) => {
  if (amount === null || amount === undefined) return '0';
  return Math.round(amount).toString().replace(/\B(?=(\d{3})+(?!\d))/g, ',');
};

// Parse comma-separated number back to integer
export const parseMMK = (value) => {
  if (!value) return 0;
  const cleaned = value.toString().replace(/,/g, '');
  return parseInt(cleaned) || 0;
};

// Format MMK with currency symbol
export const formatCurrency = (amount) => {
  return `${formatMMK(amount)} MMK`;
};

export default { formatMMK, parseMMK, formatCurrency };
