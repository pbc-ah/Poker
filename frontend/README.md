# Poker Game Frontend

A modern, responsive poker game interface built with Vue 3 and custom CSS.

## Features

### ?? Design System
- **Modern UI/UX**: Clean, professional poker table design with realistic felt texture
- **Responsive Design**: Fully responsive layout that works on desktop, tablet, and mobile
- **Animations**: Smooth card dealing, betting, and transition animations
- **Accessibility**: WCAG compliant with proper color contrast and keyboard navigation
- **Dark Theme**: Beautiful dark theme optimized for extended gameplay

### ?? Game Features
- **Real-time Poker Table**: Interactive poker table with player positions
- **Community Cards Display**: Animated card dealing with realistic effects
- **Player Information**: Avatar circles, balances, and status indicators
- **Action Panel**: Intuitive betting interface with quick bet options
- **Waiting Screens**: Clean pre-game and post-round waiting interfaces
- **Results Display**: Showdown view with all players' cards

### ??? Architecture
- **Component-Based**: Modular Vue 3 components for maintainability
- **Composables**: Reusable game logic with `useGame()` composable
- **State Management**: Vuex store with persistent state
- **Utility Functions**: Helper functions for formatting, calculations, etc.
- **Constants**: Centralized configuration and constants

## Project Structure

```
Frontend/
??? public/
?   ??? cards/           # Card SVG assets
??? src/
?   ??? components/      # Vue components
?   ?   ??? ActionPanel.vue      # Player action controls
?   ?   ??? LobbyView.vue        # Room lobby interface
?   ?   ??? PokerTable.vue       # Main poker table
?   ?   ??? LoadingOverlay.vue   # Loading states
?   ?   ??? NotificationSystem.vue # Toast notifications
?   ??? views/          # Page views
?   ?   ??? Game.vue    # Game page
?   ?   ??? Lobby.vue   # Lobby page
?   ??? composables/    # Vue composables
?   ?   ??? useGame.js  # Game logic composable
?   ??? styles/         # CSS styles
?   ?   ??? variables.css   # CSS custom properties
?   ?   ??? global.css      # Global styles
?   ?   ??? responsive.css  # Responsive breakpoints
?   ??? utils/          # Utility functions
?   ?   ??? helpers.js      # Helper functions
?   ?   ??? constants.js    # App constants
?   ??? store/          # Vuex store
?   ?   ??? index.js
?   ??? router/         # Vue Router
?   ?   ??? index.js
?   ??? App.vue         # Root component
?   ??? main.js         # App entry point
??? index.html
??? package.json
```

## Design System

### Color Palette
- **Primary**: #2ecc71 (Green) - Success, ready states
- **Secondary**: #3498db (Blue) - Info, neutral actions
- **Accent**: #e74c3c (Red) - Danger, fold actions
- **Warning**: #f39c12 (Orange) - Warnings, pot amounts
- **Felt Green**: #0d5e2f - Poker table surface

### Typography
- **Primary Font**: Inter - Body text
- **Display Font**: Poppins - Headings
- **Mono Font**: JetBrains Mono - Chip amounts, balances

### Components

#### PokerTable
The main poker table component with:
- Elliptical felt surface with realistic shadows
- Circular player seat arrangement (up to 7 visible positions)
- Community cards area in center
- Pot display with chip stack visualization
- Animated card dealing and player turns

#### ActionPanel
Fixed bottom panel with:
- Action buttons (Fold, Check, Call, Raise/Bet)
- Raise panel with amount input and quick bet buttons
- Visual feedback for current turn
- Disabled state when waiting

#### LobbyView
Room browser with:
- Grid layout of available rooms
- Room cards showing status, ante, and player count
- Create room modal
- Join room form with player details

### Responsive Breakpoints
- **Desktop**: > 1024px - Full feature set
- **Tablet**: 768px - 1024px - Adjusted layouts
- **Mobile**: < 768px - Single column, compact controls
- **Small Mobile**: < 480px - Extra compact mode

### Animations
- **Card Deal**: Cards fly in from above with rotation
- **Card Reveal**: Smooth fade and slide transitions
- **Button Ripple**: Material design ripple effect
- **Pulse**: Attention-grabbing pulse for active players
- **Bounce**: Bouncing indicator for current turn
- **Fade In**: Page and component entrance animations

## Development

### Prerequisites
- Node.js 16+
- npm or yarn

### Setup
```bash
cd Frontend
npm install
npm run dev
```

### Build for Production
```bash
npm run build
```

### Code Style
- Use Vue 3 Composition API or Options API consistently
- Follow Vue style guide recommendations
- Use scoped styles in components
- Utilize CSS custom properties for theming

## Customization

### Theming
Edit `src/styles/variables.css` to customize:
- Colors
- Spacing
- Typography
- Shadows
- Animations

### Component Styling
Each component uses scoped styles that can be overridden via CSS custom properties.

### Game Rules
Modify `src/utils/constants.js` to adjust:
- Player limits
- Betting limits
- Polling intervals
- Game configuration

## Browser Support
- Chrome/Edge 90+
- Firefox 88+
- Safari 14+
- Mobile browsers (iOS Safari, Chrome Mobile)

## Performance
- Lazy-loaded routes
- Optimized animations
- Debounced/throttled API calls
- Efficient Vue reactivity
- CSS-based animations (GPU accelerated)

## Accessibility
- ARIA labels on interactive elements
- Keyboard navigation support
- High contrast ratios
- Screen reader friendly
- Focus indicators
- Reduced motion support

## Credits
- Card designs: Standard 52-card deck SVGs
- Fonts: Google Fonts (Inter, Poppins, JetBrains Mono)
- Icons: Unicode symbols
