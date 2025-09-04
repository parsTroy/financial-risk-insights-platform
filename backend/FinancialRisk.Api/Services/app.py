#!/usr/bin/env python3
"""
Flask API for Python Quantitative Models
This service provides HTTP endpoints for Python-based quantitative models
"""

from flask import Flask, request, jsonify
import sys
import os
import traceback

# Add current directory to Python path
sys.path.append(os.path.dirname(os.path.abspath(__file__)))

app = Flask(__name__)

# Health check endpoint
@app.route('/health')
def health_check():
    """Health check endpoint for Railway"""
    return jsonify({
        "status": "healthy",
        "service": "python-models",
        "version": "1.0.0"
    })

# Monte Carlo VaR endpoint
@app.route('/monte-carlo/var', methods=['POST'])
def monte_carlo_var():
    """Calculate VaR using Monte Carlo simulation"""
    try:
        from monte_carlo_engine import MonteCarloVaRCalculator
        
        data = request.get_json()
        returns = data.get('returns', [])
        confidence_level = data.get('confidence_level', 0.95)
        num_simulations = data.get('num_simulations', 10000)
        distribution_type = data.get('distribution_type', 'normal')
        
        calculator = MonteCarloVaRCalculator()
        result = calculator.calculate_var(
            returns=returns,
            confidence_level=confidence_level,
            num_simulations=num_simulations,
            distribution_type=distribution_type
        )
        
        return jsonify({
            "success": True,
            "result": result
        })
        
    except Exception as e:
        return jsonify({
            "success": False,
            "error": str(e),
            "traceback": traceback.format_exc()
        }), 500

# Portfolio optimization endpoint
@app.route('/portfolio/optimize', methods=['POST'])
def portfolio_optimize():
    """Optimize portfolio using various methods"""
    try:
        from portfolio_optimizer import PortfolioOptimizer
        
        data = request.get_json()
        assets = data.get('assets', [])
        method = data.get('method', 'mean_variance')
        risk_aversion = data.get('risk_aversion', 1.0)
        
        optimizer = PortfolioOptimizer()
        result = optimizer.optimize_portfolio(
            assets=assets,
            method=method,
            risk_aversion=risk_aversion
        )
        
        return jsonify({
            "success": True,
            "result": result
        })
        
    except Exception as e:
        return jsonify({
            "success": False,
            "error": str(e),
            "traceback": traceback.format_exc()
        }), 500

# Quant models registry endpoint
@app.route('/quant-models/list', methods=['GET'])
def list_quant_models():
    """List available quantitative models"""
    try:
        from python_models.quant_models import QuantModelRegistry
        
        registry = QuantModelRegistry()
        models = registry.list_models()
        
        return jsonify({
            "success": True,
            "models": models
        })
        
    except Exception as e:
        return jsonify({
            "success": False,
            "error": str(e),
            "traceback": traceback.format_exc()
        }), 500

# Execute quant model endpoint
@app.route('/quant-models/execute', methods=['POST'])
def execute_quant_model():
    """Execute a specific quantitative model"""
    try:
        from python_models.quant_models import QuantModelRegistry
        
        data = request.get_json()
        model_name = data.get('model_name')
        parameters = data.get('parameters', {})
        
        registry = QuantModelRegistry()
        result = registry.execute_model(model_name, parameters)
        
        return jsonify({
            "success": True,
            "result": result
        })
        
    except Exception as e:
        return jsonify({
            "success": False,
            "error": str(e),
            "traceback": traceback.format_exc()
        }), 500

# Test endpoint
@app.route('/test', methods=['GET'])
def test_endpoint():
    """Test endpoint to verify service is working"""
    try:
        # Test imports
        from monte_carlo_engine import MonteCarloVaRCalculator
        from portfolio_optimizer import PortfolioOptimizer
        from python_models.quant_models import QuantModelRegistry
        
        return jsonify({
            "success": True,
            "message": "Python models service is working",
            "imports": {
                "monte_carlo_engine": "OK",
                "portfolio_optimizer": "OK",
                "quant_models": "OK"
            }
        })
        
    except Exception as e:
        return jsonify({
            "success": False,
            "error": str(e),
            "traceback": traceback.format_exc()
        }), 500

if __name__ == '__main__':
    port = int(os.environ.get('PORT', 5000))
    app.run(host='0.0.0.0', port=port, debug=False)
