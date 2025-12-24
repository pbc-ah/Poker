// Preload all card SVGs
const suits = ['C', 'D', 'H', 'S'];
const ranks = ['2', '3', '4', '5', '6', '7', '8', '9', 'T', 'J', 'Q', 'K', 'A'];

const cardImages = [];

// Generate all card file names
suits.forEach(suit => {
  ranks.forEach(rank => {
    cardImages.push(`${suit}${rank}`);
  });
});

let loadedCount = 0;
let totalCount = cardImages.length;

export function preloadCardImages(onProgress = null) {
  return new Promise((resolve, reject) => {
    const images = [];
    let errors = [];

    cardImages.forEach((cardName, index) => {
      const img = new Image();
      
      img.onload = () => {
        loadedCount++;
        if (onProgress) {
          onProgress(loadedCount, totalCount);
        }
        
        if (loadedCount === totalCount) {
          if (errors.length > 0) {
            console.warn(`Preloaded ${loadedCount - errors.length}/${totalCount} cards. Failed:`, errors);
          } else {
            console.log(`? Successfully preloaded all ${totalCount} card images`);
          }
          resolve({ loaded: loadedCount - errors.length, total: totalCount, errors });
        }
      };
      
      img.onerror = () => {
        errors.push(cardName);
        loadedCount++;
        
        if (onProgress) {
          onProgress(loadedCount, totalCount);
        }
        
        if (loadedCount === totalCount) {
          console.warn(`Preloaded ${loadedCount - errors.length}/${totalCount} cards. Failed:`, errors);
          resolve({ loaded: loadedCount - errors.length, total: totalCount, errors });
        }
      };
      
      img.src = `/cards/${cardName}.svg`;
      images.push(img);
    });

    // Timeout after 10 seconds
    setTimeout(() => {
      if (loadedCount < totalCount) {
        console.warn(`Preload timeout: Only ${loadedCount}/${totalCount} cards loaded`);
        resolve({ loaded: loadedCount, total: totalCount, timeout: true });
      }
    }, 10000);
  });
}

export { cardImages };
