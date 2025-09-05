// Efficient Frontier Chart using Chart.js
window.efficientFrontierChart = {
    chart: null,
    
    init: function(canvasId) {
        const ctx = document.getElementById(canvasId);
        if (!ctx) return;
        
        // Destroy existing chart if it exists
        if (this.chart) {
            this.chart.destroy();
        }
        
        this.chart = new Chart(ctx, {
            type: 'scatter',
            data: {
                datasets: []
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                interaction: {
                    intersect: false,
                    mode: 'point'
                },
                plugins: {
                    title: {
                        display: true,
                        text: 'Efficient Frontier - Risk vs Return',
                        font: {
                            size: 16
                        }
                    },
                    legend: {
                        display: false
                    },
                    tooltip: {
                        callbacks: {
                            title: function(context) {
                                return context[0].raw.label || '';
                            },
                            label: function(context) {
                                const point = context.raw;
                                return [
                                    `Volatility: ${(point.x * 100).toFixed(2)}%`,
                                    `Return: ${(point.y * 100).toFixed(2)}%`
                                ];
                            }
                        }
                    }
                },
                scales: {
                    x: {
                        title: {
                            display: true,
                            text: 'Volatility (Risk)',
                            font: {
                                size: 14
                            }
                        },
                        ticks: {
                            callback: function(value) {
                                return (value * 100).toFixed(1) + '%';
                            }
                        }
                    },
                    y: {
                        title: {
                            display: true,
                            text: 'Expected Return',
                            font: {
                                size: 14
                            }
                        },
                        ticks: {
                            callback: function(value) {
                                return (value * 100).toFixed(1) + '%';
                            }
                        }
                    }
                },
                onClick: function(event, elements) {
                    if (elements.length > 0) {
                        const element = elements[0];
                        const dataset = this.data.datasets[element.datasetIndex];
                        const point = dataset.data[element.index];
                        
                        // Trigger click event for Blazor
                        if (point.weights) {
                            window.blazorEfficientFrontier?.onPointClick(point);
                        }
                    }
                }
            }
        });
    },
    
    updateData: function(frontierPoints, individualAssets, specialPoints) {
        if (!this.chart) return;
        
        const datasets = [];
        
        // Add efficient frontier line
        if (frontierPoints && frontierPoints.length > 0) {
            datasets.push({
                label: 'Efficient Frontier',
                data: frontierPoints.map(p => ({
                    x: p.x,
                    y: p.y,
                    label: p.label,
                    weights: p.weights
                })),
                borderColor: '#007bff',
                backgroundColor: '#007bff',
                borderWidth: 2,
                pointRadius: 3,
                pointHoverRadius: 6,
                showLine: true,
                fill: false,
                tension: 0.1
            });
        }
        
        // Add individual assets
        if (individualAssets && individualAssets.length > 0) {
            datasets.push({
                label: 'Individual Assets',
                data: individualAssets.map(p => ({
                    x: p.x,
                    y: p.y,
                    label: p.label,
                    symbol: p.symbol
                })),
                borderColor: '#dc3545',
                backgroundColor: '#dc3545',
                pointRadius: 6,
                pointHoverRadius: 8,
                showLine: false
            });
        }
        
        // Add special points
        if (specialPoints) {
            if (specialPoints.minVolatility) {
                datasets.push({
                    label: 'Min Volatility',
                    data: [{
                        x: specialPoints.minVolatility.x,
                        y: specialPoints.minVolatility.y,
                        label: specialPoints.minVolatility.label,
                        weights: specialPoints.minVolatility.weights
                    }],
                    borderColor: '#28a745',
                    backgroundColor: '#28a745',
                    pointRadius: 8,
                    pointHoverRadius: 10,
                    showLine: false
                });
            }
            
            if (specialPoints.maxSharpe) {
                datasets.push({
                    label: 'Max Sharpe',
                    data: [{
                        x: specialPoints.maxSharpe.x,
                        y: specialPoints.maxSharpe.y,
                        label: specialPoints.maxSharpe.label,
                        weights: specialPoints.maxSharpe.weights
                    }],
                    borderColor: '#ffc107',
                    backgroundColor: '#ffc107',
                    pointRadius: 8,
                    pointHoverRadius: 10,
                    showLine: false
                });
            }
            
            if (specialPoints.maxReturn) {
                datasets.push({
                    label: 'Max Return',
                    data: [{
                        x: specialPoints.maxReturn.x,
                        y: specialPoints.maxReturn.y,
                        label: specialPoints.maxReturn.label,
                        weights: specialPoints.maxReturn.weights
                    }],
                    borderColor: '#17a2b8',
                    backgroundColor: '#17a2b8',
                    pointRadius: 8,
                    pointHoverRadius: 10,
                    showLine: false
                });
            }
        }
        
        this.chart.data.datasets = datasets;
        this.chart.update();
    },
    
    destroy: function() {
        if (this.chart) {
            this.chart.destroy();
            this.chart = null;
        }
    }
};

// Blazor interop functions
window.blazorEfficientFrontier = {
    onPointClick: function(point) {
        // This will be called from Blazor
        console.log('Point clicked:', point);
    }
};
