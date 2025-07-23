// Nicolas Qui Paie - Charts JavaScript Module

// Initialize Chart.js if available
let Chart;
try {
    Chart = window.Chart;
} catch (e) {
    console.warn('Chart.js not loaded, charts will not be available');
}

// Initialize vote trends chart
export function initializeVoteTrendsChart(labels, votesFor, votesAgainst) {
    if (!Chart) {
        console.warn('Chart.js not available');
        return;
    }

    const ctx = document.getElementById('voteTrendsChart');
    if (!ctx) {
        console.error('Chart canvas not found');
        return;
    }

    new Chart(ctx, {
        type: 'line',
        data: {
            labels: labels,
            datasets: [
                {
                    label: 'Nicolas Approuve',
                    data: votesFor,
                    borderColor: '#198754',
                    backgroundColor: 'rgba(25, 135, 84, 0.1)',
                    fill: true,
                    tension: 0.4
                },
                {
                    label: 'Nicolas Refuse',
                    data: votesAgainst,
                    borderColor: '#dc3545',
                    backgroundColor: 'rgba(220, 53, 69, 0.1)',
                    fill: true,
                    tension: 0.4
                }
            ]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    position: 'top',
                },
                title: {
                    display: false
                }
            },
            scales: {
                y: {
                    beginAtZero: true,
                    grid: {
                        color: 'rgba(0,0,0,0.1)'
                    }
                },
                x: {
                    grid: {
                        color: 'rgba(0,0,0,0.1)'
                    }
                }
            },
            interaction: {
                intersect: false,
                mode: 'index'
            }
        }
    });
}

// Initialize SignalR connection for real-time updates
window.setupVotingSignalR = function(proposalId) {
    if (typeof signalR === 'undefined') {
        console.warn('SignalR not available');
        return {
            dispose: () => {},
            invoke: () => Promise.resolve()
        };
    }

    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/votingHub")
        .build();

    connection.start().then(function () {
        connection.invoke("JoinProposal", proposalId.toString());
        connection.invoke("JoinGlobalUpdates");
    }).catch(function (err) {
        console.error('SignalR connection failed:', err);
    });

    // Listen for vote updates
    connection.on("VoteUpdate", function (update) {
        // Update UI with new vote counts
        updateVoteDisplay(update);
    });

    connection.on("GlobalVoteUpdate", function (update) {
        // Update global statistics if on analytics page
        updateGlobalStats(update);
    });

    return {
        dispose: () => {
            if (connection.state === signalR.HubConnectionState.Connected) {
                connection.invoke("LeaveProposal", proposalId.toString());
                connection.invoke("LeaveGlobalUpdates");
                connection.stop();
            }
        },
        invoke: (method, ...args) => {
            if (connection.state === signalR.HubConnectionState.Connected) {
                return connection.invoke(method, ...args);
            }
            return Promise.resolve();
        }
    };
};

// Update vote display with real-time data
function updateVoteDisplay(update) {
    // Update vote counts
    const votesForElements = document.querySelectorAll(`[data-proposal-id="${update.ProposalId}"] .votes-for`);
    const votesAgainstElements = document.querySelectorAll(`[data-proposal-id="${update.ProposalId}"] .votes-against`);
    const totalVotesElements = document.querySelectorAll(`[data-proposal-id="${update.ProposalId}"] .total-votes`);
    const approvalRateElements = document.querySelectorAll(`[data-proposal-id="${update.ProposalId}"] .approval-rate`);

    votesForElements.forEach(el => el.textContent = update.VotesFor);
    votesAgainstElements.forEach(el => el.textContent = update.VotesAgainst);
    totalVotesElements.forEach(el => el.textContent = update.TotalVotes);
    approvalRateElements.forEach(el => el.textContent = `${update.ApprovalRate.toFixed(1)}%`);

    // Update progress bars
    const progressBars = document.querySelectorAll(`[data-proposal-id="${update.ProposalId}"] .vote-progress`);
    progressBars.forEach(progressBar => {
        const forBar = progressBar.querySelector('.progress-bar.bg-success');
        const againstBar = progressBar.querySelector('.progress-bar.bg-danger');
        
        if (forBar && againstBar) {
            const forPercentage = update.TotalVotes > 0 ? (update.VotesFor / update.TotalVotes * 100) : 0;
            const againstPercentage = 100 - forPercentage;
            
            forBar.style.width = `${forPercentage}%`;
            againstBar.style.width = `${againstPercentage}%`;
        }
    });

    // Add pulse animation to updated elements
    const updatedElements = document.querySelectorAll(`[data-proposal-id="${update.ProposalId}"]`);
    updatedElements.forEach(el => {
        el.classList.add('pulse-animation');
        setTimeout(() => el.classList.remove('pulse-animation'), 2000);
    });
}

// Update global statistics
function updateGlobalStats(update) {
    // This would update dashboard statistics in real-time
    // Implementation depends on the specific dashboard layout
}

// Utility functions
window.nicolasUtils = {
    // Format numbers with French locale
    formatNumber: (num) => {
        return new Intl.NumberFormat('fr-FR').format(num);
    },

    // Format percentages
    formatPercentage: (num, decimals = 1) => {
        return `${num.toFixed(decimals)}%`;
    },

    // Animate counter
    animateCounter: (element, start, end, duration = 1000) => {
        if (!element) return;
        
        const range = end - start;
        const minTimer = 50;
        const stepTime = Math.abs(Math.floor(duration / range));
        const timer = Math.max(stepTime, minTimer);
        
        const startTime = new Date().getTime();
        const endTime = startTime + duration;
        
        function run() {
            const now = new Date().getTime();
            const remaining = Math.max((endTime - now) / duration, 0);
            const value = Math.round(end - (remaining * range));
            element.textContent = value;
            
            if (value === end) {
                return;
            }
            
            setTimeout(run, timer);
        }
        
        run();
    },

    // Show toast notification
    showToast: (message, type = 'info') => {
        // Create toast element
        const toast = document.createElement('div');
        toast.className = `toast align-items-center text-white bg-${type} border-0`;
        toast.setAttribute('role', 'alert');
        toast.setAttribute('aria-live', 'assertive');
        toast.setAttribute('aria-atomic', 'true');
        
        toast.innerHTML = `
            <div class="d-flex">
                <div class="toast-body">
                    ${message}
                </div>
                <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button>
            </div>
        `;
        
        // Add to toast container or create one
        let container = document.querySelector('.toast-container');
        if (!container) {
            container = document.createElement('div');
            container.className = 'toast-container position-fixed bottom-0 end-0 p-3';
            document.body.appendChild(container);
        }
        
        container.appendChild(toast);
        
        // Initialize and show toast
        if (typeof bootstrap !== 'undefined') {
            const bsToast = new bootstrap.Toast(toast);
            bsToast.show();
            
            // Remove toast after it's hidden
            toast.addEventListener('hidden.bs.toast', () => {
                toast.remove();
            });
        } else {
            // Fallback without Bootstrap
            toast.style.display = 'block';
            setTimeout(() => toast.remove(), 5000);
        }
    }
};