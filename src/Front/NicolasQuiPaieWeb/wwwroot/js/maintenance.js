// Nicolas Qui Paie - Maintenance Mode JavaScript
// Auto-retry functionality for maintenance mode

window.maintenanceMode = {
    // Auto-retry every 5 minutes when in maintenance mode
    startAutoRetry: function() {
        console.log('?? Maintenance mode: Auto-retry enabled (every 5 minutes)');
        
        // Auto-retry every 5 minutes
        setTimeout(function() {
            console.log('?? Auto-retry: Reloading page...');
            location.reload();
        }, 300000); // 5 minutes
        
        // Show periodic console messages
        setInterval(function() {
            console.log('?? Maintenance check at:', new Date().toLocaleTimeString('fr-FR'));
        }, 60000); // Every minute
    },
    
    // Manual retry with delay
    retryWithDelay: function(delaySeconds) {
        console.log(`?? Manual retry scheduled in ${delaySeconds} seconds`);
        setTimeout(function() {
            location.reload();
        }, delaySeconds * 1000);
    }
};

// Start auto-retry if we're in maintenance mode
document.addEventListener('DOMContentLoaded', function() {
    const maintenanceElement = document.getElementById('maintenance-mode');
    if (maintenanceElement) {
        window.maintenanceMode.startAutoRetry();
    }
});