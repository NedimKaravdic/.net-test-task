window.performAjaxSync = async function () {
    console.log('Starting AJAX Sync from JS...');
    try {
        const response = await fetch('/api/sync', { 
            method: 'POST',
            headers: { 'Content-Type': 'application/json' }
        });
        console.log('Sync Response Status:', response.status);
        return response.status;
    } catch (err) {
        console.error('AJAX sync critical failure:', err);
        return 500;
    }
};

window.marketChart = null;
window.renderChart = (canvasId, labels, data, assetName) => {
    const ctx = document.getElementById(canvasId);
    if (!ctx) return;

    if (window.marketChart) {
        window.marketChart.destroy();
    }

    const isPositive = data.length > 1 && data[data.length - 1] >= data[0];
    const lineColor = isPositive ? '#10b981' : '#ef4444';
    const gradientColor = isPositive ? 'rgba(16, 185, 129, 0.2)' : 'rgba(239, 68, 68, 0.2)';

    const gradient = ctx.getContext('2d').createLinearGradient(0, 0, 0, 400);
    gradient.addColorStop(0, gradientColor);
    gradient.addColorStop(1, 'rgba(0, 0, 0, 0)');

    window.marketChart = new Chart(ctx, {
        type: 'line',
        data: {
            labels: labels,
            datasets: [{
                label: `${assetName} Price`,
                data: data,
                borderColor: lineColor,
                backgroundColor: gradient,
                borderWidth: 2,
                pointRadius: 0,
                pointHoverRadius: 6,
                fill: true,
                tension: 0.1
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            interaction: {
                intersect: false,
                mode: 'index',
            },
            plugins: {
                legend: { display: false },
                tooltip: {
                    backgroundColor: '#0f172a',
                    titleColor: '#94a3b8',
                    bodyColor: '#f8fafc',
                    borderColor: '#334155',
                    borderWidth: 1,
                    padding: 12,
                    displayColors: false,
                    callbacks: {
                        label: function(context) {
                            return `$${context.parsed.y.toFixed(4)}`;
                        }
                    }
                }
            },
            scales: {
                x: {
                    grid: { display: false, drawBorder: false },
                    ticks: { color: '#475569', maxTicksLimit: 8 }
                },
                y: {
                    grid: { color: '#1e293b', borderDash: [5, 5], drawBorder: false },
                    ticks: { color: '#475569' },
                    position: 'right'
                }
            }
        }
    });
};
