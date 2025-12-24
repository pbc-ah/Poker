/**
 * Format chip amounts with proper currency symbol
 */
export function formatChips(amount) {
  if (typeof amount !== 'number') {
    return '0¢';
  }
  return `${amount.toLocaleString()}¢`;
}

/**
 * Get initials from a name
 */
export function getInitials(name) {
  if (!name) return '??';
  
  return name
    .split(' ')
    .map(word => word[0])
    .join('')
    .toUpperCase()
    .substring(0, 2);
}

/**
 * Format time duration
 */
export function formatDuration(seconds) {
  const minutes = Math.floor(seconds / 60);
  const remainingSeconds = seconds % 60;
  
  if (minutes > 0) {
    return `${minutes}m ${remainingSeconds}s`;
  }
  
  return `${remainingSeconds}s`;
}

/**
 * Validate ante amount
 */
export function isValidAnte(ante, minAnte = 5, increment = 5) {
  return ante >= minAnte && ante % increment === 0;
}

/**
 * Calculate pot odds
 */
export function calculatePotOdds(potSize, betSize) {
  if (betSize === 0) return 0;
  return ((betSize / (potSize + betSize)) * 100).toFixed(1);
}

/**
 * Get hand strength description
 */
export function getHandStrengthDescription(score) {
  const handRank = score >> 20;
  
  const hands = [
    'High Card',
    'One Pair',
    'Two Pair',
    'Three of a Kind',
    'Straight',
    'Flush',
    'Full House',
    'Four of a Kind',
    'Straight Flush',
    'Royal Flush'
  ];
  
  return hands[handRank] || 'Unknown';
}

/**
 * Debounce function
 */
export function debounce(func, wait) {
  let timeout;
  
  return function executedFunction(...args) {
    const later = () => {
      clearTimeout(timeout);
      func(...args);
    };
    
    clearTimeout(timeout);
    timeout = setTimeout(later, wait);
  };
}

/**
 * Throttle function
 */
export function throttle(func, limit) {
  let inThrottle;
  
  return function executedFunction(...args) {
    if (!inThrottle) {
      func.apply(this, args);
      inThrottle = true;
      setTimeout(() => inThrottle = false, limit);
    }
  };
}

/**
 * Generate random ID
 */
export function generateId() {
  return Math.random().toString(36).substring(2, 15) + 
         Math.random().toString(36).substring(2, 15);
}

/**
 * Deep clone object
 */
export function deepClone(obj) {
  return JSON.parse(JSON.stringify(obj));
}

/**
 * Check if object is empty
 */
export function isEmpty(obj) {
  return Object.keys(obj).length === 0;
}

/**
 * Sleep/delay function
 */
export function sleep(ms) {
  return new Promise(resolve => setTimeout(resolve, ms));
}

/**
 * Clamp number between min and max
 */
export function clamp(num, min, max) {
  return Math.min(Math.max(num, min), max);
}

/**
 * Parse card notation to display format
 */
export function parseCard(card) {
  if (!card || card.length !== 2) return { suit: '', rank: '' };
  
  const suitMap = {
    'H': '?',
    'D': '?',
    'C': '?',
    'S': '?'
  };
  
  const rankMap = {
    'T': '10',
    'J': 'Jack',
    'Q': 'Queen',
    'K': 'King',
    'A': 'Ace'
  };
  
  const suit = suitMap[card[0]] || card[0];
  const rank = rankMap[card[1]] || card[1];
  
  return { suit, rank };
}

/**
 * Get card color
 */
export function getCardColor(card) {
  if (!card) return 'black';
  const suit = card[0];
  return (suit === 'H' || suit === 'D') ? 'red' : 'black';
}
