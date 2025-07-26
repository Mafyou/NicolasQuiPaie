// Nicolas Qui Paie - Charts JavaScript ES6 Module for Blazor WebAssembly
// Compatible with .NET 9 and C# 13.0 optimizations

// Check Chart.js availability from global scope
let Chart;
let chartJsAvailable = false;

// Initialize Chart.js reference
function initializeChart() {
    try {
        Chart = window.Chart;
        chartJsAvailable = window.chartJsAvailable || (typeof Chart !== 'undefined');
        if (chartJsAvailable) {
            console.log('Chart.js initialized successfully in module');
        } else {
            console.warn('Chart.js not available in global scope');
        }
    } catch (e) {
        console.warn('Chart.js not loaded, charts will not be available:', e);
        chartJsAvailable = false;
    }
}

// Call initialization immediately
initializeChart();

// Export function to initialize vote trends chart
export function initializeVoteTrendsChart(labels, votesFor, votesAgainst) {
    // Re-check Chart.js availability
    if (!chartJsAvailable) {
        initializeChart();
    }

    if (!chartJsAvailable || !Chart) {
        console.warn('Chart.js not available - cannot initialize vote trends chart');
        return false;
    }

    const ctx = document.getElementById('voteTrendsChart');
    if (!ctx) {
        console.error('Chart canvas "voteTrendsChart" not found');
        return false;
    }

    try {
        // Destroy existing chart if it exists
        const existingChart = Chart.getChart(ctx);
        if (existingChart) {
            existingChart.destroy();
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
                        tension: 0.4,
                        pointBackgroundColor: '#198754',
                        pointBorderColor: '#ffffff',
                        pointBorderWidth: 2,
                        pointRadius: 4
                    },
                    {
                        label: 'Nicolas Refuse',
                        data: votesAgainst,
                        borderColor: '#dc3545',
                        backgroundColor: 'rgba(220, 53, 69, 0.1)',
                        fill: true,
                        tension: 0.4,
                        pointBackgroundColor: '#dc3545',
                        pointBorderColor: '#ffffff',
                        pointBorderWidth: 2,
                        pointRadius: 4
                    }
                ]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        position: 'top',
                        labels: {
                            usePointStyle: true,
                            padding: 20
                        }
                    },
                    title: {
                        display: false
                    },
                    tooltip: {
                        mode: 'index',
                        intersect: false,
                        backgroundColor: 'rgba(0,0,0,0.8)',
                        titleColor: 'white',
                        bodyColor: 'white',
                        cornerRadius: 8
                    }
                },
                scales: {
                    y: {
                        beginAtZero: true,
                        grid: {
                            color: 'rgba(0,0,0,0.1)',
                            lineWidth: 1
                        },
                        ticks: {
                            color: '#666',
                            font: {
                                size: 12
                            }
                        }
                    },
                    x: {
                        grid: {
                            color: 'rgba(0,0,0,0.1)',
                            lineWidth: 1
                        },
                        ticks: {
                            color: '#666',
                            font: {
                                size: 12
                            }
                        }
                    }
                },
                interaction: {
                    intersect: false,
                    mode: 'index'
                },
                animation: {
                    duration: 1000,
                    easing: 'easeInOutQuart'
                }
            }
        });

        console.log('Vote trends chart initialized successfully');
        return true;
    } catch (error) {
        console.error('Failed to initialize vote trends chart:', error);
        return false;
    }
}

// Export function to create category distribution pie chart
export function initializeCategoryChart(categories, votes, colors) {
    // Re-check Chart.js availability
    if (!chartJsAvailable) {
        initializeChart();
    }

    if (!chartJsAvailable || !Chart) {
        console.warn('Chart.js not available - cannot initialize category chart');
        return false;
    }

    const ctx = document.getElementById('categoryChart');
    if (!ctx) {
        console.error('Chart canvas "categoryChart" not found');
        return false;
    }

    try {
        // Destroy existing chart if it exists
        const existingChart = Chart.getChart(ctx);
        if (existingChart) {
            existingChart.destroy();
        }

        new Chart(ctx, {
            type: 'doughnut',
            data: {
                labels: categories,
                datasets: [{
                    data: votes,
                    backgroundColor: colors,
                    borderWidth: 2,
                    borderColor: '#ffffff'
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        position: 'bottom',
                        labels: {
                            padding: 20,
                            usePointStyle: true
                        }
                    },
                    tooltip: {
                        backgroundColor: 'rgba(0,0,0,0.8)',
                        titleColor: 'white',
                        bodyColor: 'white',
                        cornerRadius: 8,
                        callbacks: {
                            label: function(context) {
                                const total = context.dataset.data.reduce((a, b) => a + b, 0);
                                const percentage = ((context.parsed * 100) / total).toFixed(1);
                                return `${context.label}: ${context.parsed} votes (${percentage}%)`;
                            }
                        }
                    }
                },
                animation: {
                    duration: 1000,
                    easing: 'easeInOutQuart'
                }
            }
        });

        console.log('Category chart initialized successfully');
        return true;
    } catch (error) {
        console.error('Failed to initialize category chart:', error);
        return false;
    }
}

// Export function to setup SignalR connection for real-time updates (compatible with SignalR 8.0.7)
export function setupVotingSignalR(proposalId) {
    if (!window.signalRAvailable || typeof signalR === 'undefined') {
        console.warn('SignalR not available');
        return {
            dispose: () => { },
            invoke: () => Promise.resolve()
        };
    }

    try {
        const connection = new signalR.HubConnectionBuilder()
            .withUrl("/votingHub", {
                skipNegotiation: true,
                transport: signalR.HttpTransportType.WebSockets
            })
            .withAutomaticReconnect([0, 2000, 10000, 30000])
            .build();

        connection.start()
            .then(function () {
                console.log('SignalR connection established');
                if (window.updateSignalRStatus) {
                    window.updateSignalRStatus('connected', 'Connexion temps réel active');
                }
                connection.invoke("JoinProposal", proposalId.toString());
                connection.invoke("JoinGlobalUpdates");
            })
            .catch(function (err) {
                console.error('SignalR connection failed:', err);
                if (window.updateSignalRStatus) {
                    window.updateSignalRStatus('disconnected', 'Connexion temps réel échouée');
                }
            });

        // Listen for vote updates
        connection.on("VoteUpdate", function (update) {
            updateVoteDisplay(update);
        });

        connection.on("GlobalVoteUpdate", function (update) {
            updateGlobalStats(update);
        });

        // Handle reconnection
        connection.onreconnected(function () {
            console.log('SignalR reconnected');
            if (window.updateSignalRStatus) {
                window.updateSignalRStatus('connected', 'Reconnexion réussie');
            }
            connection.invoke("JoinProposal", proposalId.toString());
            connection.invoke("JoinGlobalUpdates");
        });

        connection.onclose(function () {
            console.warn('SignalR connection closed');
            if (window.updateSignalRStatus) {
                window.updateSignalRStatus('disconnected', 'Connexion fermée');
            }
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
    } catch (error) {
        console.error('Failed to setup SignalR:', error);
        return {
            dispose: () => { },
            invoke: () => Promise.resolve()
        };
    }
}

// Update vote display with real-time data
function updateVoteDisplay(update) {
    try {
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

        console.log('Vote display updated successfully');
    } catch (error) {
        console.error('Failed to update vote display:', error);
    }
}

// Update global statistics
function updateGlobalStats(update) {
    try {
        // Update dashboard statistics in real-time
        const activeUsersEl = document.querySelector('.active-users-count');
        const totalVotesEl = document.querySelector('.total-votes-count');
        const activeProposalsEl = document.querySelector('.active-proposals-count');

        if (activeUsersEl && update.ActiveUsers !== undefined) {
            animateCounter(activeUsersEl, parseInt(activeUsersEl.textContent.replace(/\D/g, '')), update.ActiveUsers);
        }
        if (totalVotesEl && update.TotalVotes !== undefined) {
            animateCounter(totalVotesEl, parseInt(totalVotesEl.textContent.replace(/\D/g, '')), update.TotalVotes);
        }
        if (activeProposalsEl && update.ActiveProposals !== undefined) {
            animateCounter(activeProposalsEl, parseInt(activeProposalsEl.textContent.replace(/\D/g, '')), update.ActiveProposals);
        }

        console.log('Global stats updated successfully');
    } catch (error) {
        console.error('Failed to update global stats:', error);
    }
}

// Utility functions for enhanced UX
export const nicolasUtils = {
    // Format numbers with French locale
    formatNumber: (num) => {
        return new Intl.NumberFormat('fr-FR').format(num);
    },

    // Format percentages
    formatPercentage: (num, decimals = 1) => {
        return `${num.toFixed(decimals)}%`;
    },

    // Animate counter with smooth transition
    animateCounter: (element, start, end, duration = 1000) => {
        if (!element) return;

        const range = end - start;
        if (range === 0) return;

        const startTime = performance.now();

        function updateCounter(currentTime) {
            const elapsed = currentTime - startTime;
            const progress = Math.min(elapsed / duration, 1);
            
            // Easing function for smooth animation
            const easeOutCubic = 1 - Math.pow(1 - progress, 3);
            const current = Math.round(start + (range * easeOutCubic));
            
            element.textContent = nicolasUtils.formatNumber(current);

            if (progress < 1) {
                requestAnimationFrame(updateCounter);
            }
        }

        requestAnimationFrame(updateCounter);
    },

    // Show toast notification with enhanced styling
    showToast: (message, type = 'info', duration = 5000) => {
        const toast = document.createElement('div');
        toast.className = `toast align-items-center text-white bg-${type} border-0`;
        toast.setAttribute('role', 'alert');
        toast.setAttribute('aria-live', 'assertive');
        toast.setAttribute('aria-atomic', 'true');

        toast.innerHTML = `
            <div class="d-flex">
                <div class="toast-body">
                    <i class="fas fa-info-circle me-2"></i>
                    ${message}
                </div>
                <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button>
            </div>
        `;

        let container = document.querySelector('.toast-container');
        if (!container) {
            container = document.createElement('div');
            container.className = 'toast-container position-fixed bottom-0 end-0 p-3';
            container.style.zIndex = '9999';
            document.body.appendChild(container);
        }

        container.appendChild(toast);

        // Initialize and show toast
        if (typeof bootstrap !== 'undefined') {
            const bsToast = new bootstrap.Toast(toast, { delay: duration });
            bsToast.show();

            toast.addEventListener('hidden.bs.toast', () => {
                toast.remove();
            });
        } else {
            // Fallback without Bootstrap
            toast.style.display = 'block';
            setTimeout(() => toast.remove(), duration);
        }
    },

    // Check if Chart.js is available
    isChartJsAvailable: () => chartJsAvailable,
    
    // Get Chart.js reference
    getChart: () => Chart
};

// Global function reference for counter animation (for backward compatibility)
function animateCounter(element, start, end, duration = 1000) {
    return nicolasUtils.animateCounter(element, start, end, duration);
}

// Export animateCounter function
export { animateCounter };

console.log('Nicolas Charts ES6 module loaded successfully');