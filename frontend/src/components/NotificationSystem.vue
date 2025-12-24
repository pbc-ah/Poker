<template>
  <transition-group name="notification" tag="div" class="notifications-container">
    <div 
      v-for="notification in notifications"
      :key="notification.id"
      :class="['notification', `notification-${notification.type}`]"
      @click="removeNotification(notification.id)"
    >
      <div class="notification-icon">
        {{ getIcon(notification.type) }}
      </div>
      <div class="notification-content">
        <div v-if="notification.title" class="notification-title">
          {{ notification.title }}
        </div>
        <div class="notification-message">
          {{ notification.message }}
        </div>
      </div>
      <button class="notification-close" @click.stop="removeNotification(notification.id)">
        ×
      </button>
    </div>
  </transition-group>
</template>

<script>
export default {
  name: 'NotificationSystem',
  data() {
    return {
      notifications: []
    };
  },
  methods: {
    addNotification({ type = 'info', title = '', message, duration = 5000 }) {
      const id = Date.now() + Math.random();
      
      this.notifications.push({
        id,
        type,
        title,
        message
      });

      if (duration > 0) {
        setTimeout(() => {
          this.removeNotification(id);
        }, duration);
      }
    },
    removeNotification(id) {
      const index = this.notifications.findIndex(n => n.id === id);
      if (index > -1) {
        this.notifications.splice(index, 1);
      }
    },
    getIcon(type) {
      const icons = {
        success: '?',
        error: '?',
        warning: '?',
        info: '?'
      };
      return icons[type] || icons.info;
    }
  }
};
</script>

<style scoped>
.notifications-container {
  position: fixed;
  top: var(--space-6);
  right: var(--space-6);
  z-index: var(--z-tooltip);
  display: flex;
  flex-direction: column;
  gap: var(--space-3);
  max-width: 400px;
  width: 100%;
}

.notification {
  background: rgba(0, 0, 0, 0.95);
  backdrop-filter: blur(10px);
  border-radius: var(--radius-xl);
  padding: var(--space-4);
  display: flex;
  align-items: flex-start;
  gap: var(--space-3);
  box-shadow: var(--shadow-xl);
  border: 2px solid;
  cursor: pointer;
  transition: all var(--transition-base);
}

.notification:hover {
  transform: translateX(-4px);
  box-shadow: var(--shadow-2xl);
}

.notification-success {
  border-color: var(--color-success);
  background: linear-gradient(135deg, rgba(46, 204, 113, 0.2), rgba(39, 174, 96, 0.1));
}

.notification-error {
  border-color: var(--color-danger);
  background: linear-gradient(135deg, rgba(231, 76, 60, 0.2), rgba(192, 57, 43, 0.1));
}

.notification-warning {
  border-color: var(--color-warning);
  background: linear-gradient(135deg, rgba(243, 156, 18, 0.2), rgba(230, 126, 34, 0.1));
}

.notification-info {
  border-color: var(--color-secondary);
  background: linear-gradient(135deg, rgba(52, 152, 219, 0.2), rgba(41, 128, 185, 0.1));
}

.notification-icon {
  font-size: var(--text-2xl);
  line-height: 1;
}

.notification-success .notification-icon {
  color: var(--color-success);
}

.notification-error .notification-icon {
  color: var(--color-danger);
}

.notification-warning .notification-icon {
  color: var(--color-warning);
}

.notification-info .notification-icon {
  color: var(--color-secondary);
}

.notification-content {
  flex: 1;
}

.notification-title {
  font-weight: 700;
  font-size: var(--text-base);
  color: var(--color-light);
  margin-bottom: var(--space-1);
}

.notification-message {
  font-size: var(--text-sm);
  color: var(--color-gray-300);
  line-height: 1.4;
}

.notification-close {
  background: none;
  border: none;
  color: var(--color-gray-400);
  font-size: var(--text-2xl);
  cursor: pointer;
  padding: 0;
  width: 24px;
  height: 24px;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 50%;
  transition: all var(--transition-fast);
}

.notification-close:hover {
  background: rgba(255, 255, 255, 0.1);
  color: var(--color-light);
}

/* Notification Transitions */
.notification-enter-active {
  animation: notificationIn 0.3s ease-out;
}

.notification-leave-active {
  animation: notificationOut 0.3s ease-in;
}

@keyframes notificationIn {
  from {
    opacity: 0;
    transform: translateX(100%) scale(0.8);
  }
  to {
    opacity: 1;
    transform: translateX(0) scale(1);
  }
}

@keyframes notificationOut {
  from {
    opacity: 1;
    transform: translateX(0) scale(1);
  }
  to {
    opacity: 0;
    transform: translateX(100%) scale(0.8);
  }
}

/* Responsive Design */
@media (max-width: 768px) {
  .notifications-container {
    top: var(--space-4);
    right: var(--space-4);
    left: var(--space-4);
    max-width: none;
  }
}
</style>
