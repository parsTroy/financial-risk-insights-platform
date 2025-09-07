window.efficientFrontierChart = {
    chart: null,

    init: function() {
        const ctx = document.getElementById('efficientFrontierChart').getContext('2d');
        
        this.chart = new Chart(ctx, {
            type: 'scatter',
            data: {
                datasets: []
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    title: {
                        display: true,
                        text: 'Efficient Frontier',
                        font: {
                            size: 16,
                            weight: 'bold'
                        }
                    },
                    legend: {
                        display: true,
                        position: 'top'
                    }
                },
                scales: {
                    x: {
                        type: 'linear',
                        position: 'bottom',
                        title: {
                            display: true,
                            text: 'Volatility (Risk)',
                            font: {
                                size: 14,
                                weight: 'bold'
                            }
                        },
                        grid: {
                            color: 'rgba(0,0,0,0.1)'
                        }
                    },
                    y: {
                        title: {
                            display: true,
                            text: 'Expected Return',
                            font: {
                                size: 14,
                                weight: 'bold'
                            }
                        },
                        grid: {
                            color: 'rgba(0,0,0,0.1)'
                        }
                    }
                },
                elements: {
                    point: {
                        radius: 6,
                        hoverRadius: 8
                    },
                    line: {
                        borderWidth: 2
                    }
                },
                interaction: {
                    intersect: false,
                    mode: 'point'
                }
            }
        });
    },

    updateData: function(data) {
        if (!this.chart) {
            this.init();
        }

        this.chart.data.datasets = [
            {
                label: 'Efficient Frontier',
                data: data.frontierPoints,
                borderColor: 'rgb(75, 192, 192)',
                backgroundColor: 'rgba(75, 192, 192, 0.2)',
                fill: false,
                tension: 0.1
            },
            {
                label: 'Individual Assets',
                data: data.assetPoints,
                backgroundColor: 'rgb(255, 99, 132)',
                borderColor: 'rgb(255, 99, 132)',
                pointStyle: 'circle'
            },
            {
                label: 'Optimal Portfolio',
                data: data.optimalPoint ? [data.optimalPoint] : [],
                backgroundColor: 'rgb(54, 162, 235)',
                borderColor: 'rgb(54, 162, 235)',
                pointStyle: 'star',
                pointRadius: 10
            }
        ];

        this.chart.update();
    }
};