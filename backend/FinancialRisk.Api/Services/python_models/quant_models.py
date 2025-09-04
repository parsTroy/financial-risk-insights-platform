#!/usr/bin/env python3
"""
Quantitative Models Module for Python/.NET Interop

This module provides a unified interface for quantitative models that can be called
from the C# backend through Python.NET interop. It includes various financial models
for risk management, portfolio optimization, and quantitative analysis.

Author: Financial Risk Insights Platform
Version: 1.0.0
"""

import numpy as np
import pandas as pd
import scipy.stats as stats
import scipy.optimize as opt
from scipy.linalg import cholesky, LinAlgError
from typing import Dict, List, Tuple, Optional, Any, Union
import json
import logging
import time
import psutil
import sys
from datetime import datetime
from dataclasses import dataclass, asdict
from enum import Enum
import warnings
warnings.filterwarnings('ignore')

# Configure logging
logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

class ModelType(Enum):
    """Types of quantitative models"""
    RISK_MANAGEMENT = "risk_management"
    PORTFOLIO_OPTIMIZATION = "portfolio_optimization"
    PRICING = "pricing"
    STATISTICAL = "statistical"
    MACHINE_LEARNING = "machine_learning"
    TIME_SERIES = "time_series"

@dataclass
class ModelExecutionContext:
    """Context for model execution"""
    request_id: str
    model_name: str
    start_time: datetime
    memory_before: int
    python_version: str

# Risk Management Models

def _var_historical(parameters: Dict[str, Any], input_data: Dict[str, Any], options: Dict[str, Any]) -> Dict[str, Any]:
    """Calculate Value at Risk using historical simulation"""
    returns = np.array(input_data.get('returns', []))
    confidence_level = parameters.get('confidence_level', 0.95)
    
    if len(returns) == 0:
        raise ValueError("No returns data provided")
    
    var = -np.percentile(returns, (1 - confidence_level) * 100)
    
    return {
        'var': float(var),
        'confidence_level': confidence_level,
        'method': 'historical',
        'sample_size': len(returns)
    }

def _var_parametric(parameters: Dict[str, Any], input_data: Dict[str, Any], options: Dict[str, Any]) -> Dict[str, Any]:
    """Calculate Value at Risk using parametric method"""
    returns = np.array(input_data.get('returns', []))
    confidence_level = parameters.get('confidence_level', 0.95)
    
    if len(returns) == 0:
        raise ValueError("No returns data provided")
    
    mean_return = np.mean(returns)
    std_return = np.std(returns)
    
    # Calculate VaR using normal distribution
    z_score = stats.norm.ppf(1 - confidence_level)
    var = -(mean_return + z_score * std_return)
    
    return {
        'var': float(var),
        'confidence_level': confidence_level,
        'method': 'parametric',
        'mean_return': float(mean_return),
        'std_return': float(std_return),
        'z_score': float(z_score)
    }

def _var_monte_carlo(parameters: Dict[str, Any], input_data: Dict[str, Any], options: Dict[str, Any]) -> Dict[str, Any]:
    """Calculate Value at Risk using Monte Carlo simulation"""
    returns = np.array(input_data.get('returns', []))
    confidence_level = parameters.get('confidence_level', 0.95)
    num_simulations = parameters.get('num_simulations', 10000)
    
    if len(returns) == 0:
        raise ValueError("No returns data provided")
    
    mean_return = np.mean(returns)
    std_return = np.std(returns)
    
    # Generate Monte Carlo simulations
    simulated_returns = np.random.normal(mean_return, std_return, num_simulations)
    
    # Calculate VaR
    var = -np.percentile(simulated_returns, (1 - confidence_level) * 100)
    
    return {
        'var': float(var),
        'confidence_level': confidence_level,
        'method': 'monte_carlo',
        'num_simulations': num_simulations,
        'mean_return': float(mean_return),
        'std_return': float(std_return)
    }

def _cvar_calculation(parameters: Dict[str, Any], input_data: Dict[str, Any], options: Dict[str, Any]) -> Dict[str, Any]:
    """Calculate Conditional Value at Risk (CVaR)"""
    returns = np.array(input_data.get('returns', []))
    confidence_level = parameters.get('confidence_level', 0.95)
    
    if len(returns) == 0:
        raise ValueError("No returns data provided")
    
    # Calculate VaR first
    var = -np.percentile(returns, (1 - confidence_level) * 100)
    
    # Calculate CVaR as mean of returns below VaR threshold
    tail_returns = returns[returns <= -var]
    cvar = -np.mean(tail_returns) if len(tail_returns) > 0 else var
    
    return {
        'var': float(var),
        'cvar': float(cvar),
        'confidence_level': confidence_level,
        'tail_observations': len(tail_returns)
    }

def _stress_test(parameters: Dict[str, Any], input_data: Dict[str, Any], options: Dict[str, Any]) -> Dict[str, Any]:
    """Perform stress testing on portfolio"""
    portfolio_weights = np.array(input_data.get('portfolio_weights', []))
    asset_returns = np.array(input_data.get('asset_returns', []))
    stress_scenarios = parameters.get('stress_scenarios', {})
    
    if len(portfolio_weights) == 0 or len(asset_returns) == 0:
        raise ValueError("Portfolio weights and asset returns required")
    
    results = {}
    
    for scenario_name, stress_factors in stress_scenarios.items():
        # Apply stress factors to returns
        stressed_returns = asset_returns * np.array(stress_factors)
        
        # Calculate portfolio return under stress
        portfolio_return = np.dot(portfolio_weights, stressed_returns)
        
        results[scenario_name] = {
            'portfolio_return': float(portfolio_return),
            'stress_factors': stress_factors
        }
    
    return {
        'stress_results': results,
        'base_portfolio_return': float(np.dot(portfolio_weights, np.mean(asset_returns, axis=1)))
    }

# Portfolio Optimization Models

def _markowitz_optimization(parameters: Dict[str, Any], input_data: Dict[str, Any], options: Dict[str, Any]) -> Dict[str, Any]:
    """Markowitz mean-variance portfolio optimization"""
    expected_returns = np.array(input_data.get('expected_returns', []))
    covariance_matrix = np.array(input_data.get('covariance_matrix', []))
    risk_aversion = parameters.get('risk_aversion', 1.0)
    
    if len(expected_returns) == 0 or len(covariance_matrix) == 0:
        raise ValueError("Expected returns and covariance matrix required")
    
    n = len(expected_returns)
    
    # Objective function: maximize (expected_return - risk_aversion * variance)
    def objective(weights):
        portfolio_return = np.dot(weights, expected_returns)
        portfolio_variance = np.dot(weights, np.dot(covariance_matrix, weights))
        return -(portfolio_return - risk_aversion * portfolio_variance)
    
    # Constraints
    constraints = [{'type': 'eq', 'fun': lambda w: np.sum(w) - 1.0}]
    
    # Bounds
    bounds = [(0, 1) for _ in range(n)]
    
    # Initial guess
    x0 = np.ones(n) / n
    
    try:
        result = opt.minimize(objective, x0, method='SLSQP', bounds=bounds, constraints=constraints)
        
        if result.success:
            optimal_weights = result.x
            portfolio_return = np.dot(optimal_weights, expected_returns)
            portfolio_variance = np.dot(optimal_weights, np.dot(covariance_matrix, optimal_weights))
            portfolio_volatility = np.sqrt(portfolio_variance)
            sharpe_ratio = portfolio_return / portfolio_volatility if portfolio_volatility > 0 else 0
            
            return {
                'optimal_weights': optimal_weights.tolist(),
                'expected_return': float(portfolio_return),
                'expected_volatility': float(portfolio_volatility),
                'sharpe_ratio': float(sharpe_ratio),
                'risk_aversion': risk_aversion
            }
        else:
            raise ValueError(f"Optimization failed: {result.message}")
    except Exception as e:
        raise ValueError(f"Markowitz optimization failed: {str(e)}")

def _black_litterman(parameters: Dict[str, Any], input_data: Dict[str, Any], options: Dict[str, Any]) -> Dict[str, Any]:
    """Black-Litterman portfolio optimization"""
    # Simplified implementation
    market_cap_weights = np.array(input_data.get('market_cap_weights', []))
    views = np.array(input_data.get('views', []))
    risk_aversion = parameters.get('risk_aversion', 1.0)
    
    if len(market_cap_weights) == 0:
        raise ValueError("Market cap weights required")
    
    # For simplicity, return equal weights
    # In practice, this would implement the full Black-Litterman model
    optimal_weights = np.ones(len(market_cap_weights)) / len(market_cap_weights)
    
    return {
        'optimal_weights': optimal_weights.tolist(),
        'method': 'black_litterman',
        'risk_aversion': risk_aversion
    }

def _risk_parity(parameters: Dict[str, Any], input_data: Dict[str, Any], options: Dict[str, Any]) -> Dict[str, Any]:
    """Risk parity portfolio optimization"""
    covariance_matrix = np.array(input_data.get('covariance_matrix', []))
    
    if len(covariance_matrix) == 0:
        raise ValueError("Covariance matrix required")
    
    n = len(covariance_matrix)
    
    # Objective function: minimize sum of squared differences from equal risk contribution
    def objective(weights):
        portfolio_variance = np.dot(weights, np.dot(covariance_matrix, weights))
        risk_contributions = weights * np.dot(covariance_matrix, weights) / portfolio_variance
        target_contribution = 1.0 / n
        return np.sum((risk_contributions - target_contribution) ** 2)
    
    # Constraints
    constraints = [{'type': 'eq', 'fun': lambda w: np.sum(w) - 1.0}]
    
    # Bounds
    bounds = [(0, 1) for _ in range(n)]
    
    # Initial guess
    x0 = np.ones(n) / n
    
    try:
        result = opt.minimize(objective, x0, method='SLSQP', bounds=bounds, constraints=constraints)
        
        if result.success:
            return {
                'optimal_weights': result.x.tolist(),
                'method': 'risk_parity'
            }
        else:
            raise ValueError(f"Risk parity optimization failed: {result.message}")
    except Exception as e:
        raise ValueError(f"Risk parity optimization failed: {str(e)}")

def _efficient_frontier(parameters: Dict[str, Any], input_data: Dict[str, Any], options: Dict[str, Any]) -> Dict[str, Any]:
    """Calculate efficient frontier"""
    expected_returns = np.array(input_data.get('expected_returns', []))
    covariance_matrix = np.array(input_data.get('covariance_matrix', []))
    num_points = parameters.get('num_points', 50)
    
    if len(expected_returns) == 0 or len(covariance_matrix) == 0:
        raise ValueError("Expected returns and covariance matrix required")
    
    # Find minimum and maximum expected returns
    min_return = np.min(expected_returns)
    max_return = np.max(expected_returns)
    
    # Generate target returns
    target_returns = np.linspace(min_return, max_return, num_points)
    
    frontier_points = []
    
    for target_return in target_returns:
        try:
            # Optimize for minimum variance given target return
            n = len(expected_returns)
            
            def objective(weights):
                return np.dot(weights, np.dot(covariance_matrix, weights))
            
            constraints = [
                {'type': 'eq', 'fun': lambda w: np.sum(w) - 1.0},
                {'type': 'eq', 'fun': lambda w: np.dot(w, expected_returns) - target_return}
            ]
            
            bounds = [(0, 1) for _ in range(n)]
            x0 = np.ones(n) / n
            
            result = opt.minimize(objective, x0, method='SLSQP', bounds=bounds, constraints=constraints)
            
            if result.success:
                weights = result.x
                portfolio_return = np.dot(weights, expected_returns)
                portfolio_variance = np.dot(weights, np.dot(covariance_matrix, weights))
                portfolio_volatility = np.sqrt(portfolio_variance)
                sharpe_ratio = portfolio_return / portfolio_volatility if portfolio_volatility > 0 else 0
                
                frontier_points.append({
                    'expected_return': float(portfolio_return),
                    'expected_volatility': float(portfolio_volatility),
                    'sharpe_ratio': float(sharpe_ratio),
                    'weights': weights.tolist()
                })
        except Exception:
            continue
    
    return {
        'frontier_points': frontier_points,
        'num_points': len(frontier_points)
    }

# Statistical Models

def _garch_model(parameters: Dict[str, Any], input_data: Dict[str, Any], options: Dict[str, Any]) -> Dict[str, Any]:
    """GARCH model for volatility forecasting"""
    returns = np.array(input_data.get('returns', []))
    p = parameters.get('p', 1)
    q = parameters.get('q', 1)
    
    if len(returns) == 0:
        raise ValueError("Returns data required")
    
    # Simplified GARCH implementation
    # In practice, use arch library for full GARCH modeling
    volatility = np.std(returns)
    
    return {
        'volatility': float(volatility),
        'model_type': 'garch',
        'parameters': {'p': p, 'q': q}
    }

def _copula_model(parameters: Dict[str, Any], input_data: Dict[str, Any], options: Dict[str, Any]) -> Dict[str, Any]:
    """Copula model for dependence modeling"""
    data = np.array(input_data.get('data', []))
    copula_type = parameters.get('copula_type', 'gaussian')
    
    if len(data) == 0:
        raise ValueError("Data required")
    
    # Simplified copula implementation
    correlation_matrix = np.corrcoef(data.T)
    
    return {
        'correlation_matrix': correlation_matrix.tolist(),
        'copula_type': copula_type
    }

def _regime_switching(parameters: Dict[str, Any], input_data: Dict[str, Any], options: Dict[str, Any]) -> Dict[str, Any]:
    """Regime switching model"""
    returns = np.array(input_data.get('returns', []))
    num_regimes = parameters.get('num_regimes', 2)
    
    if len(returns) == 0:
        raise ValueError("Returns data required")
    
    # Simplified regime switching implementation
    # In practice, use specialized libraries like statsmodels
    
    return {
        'num_regimes': num_regimes,
        'regime_probabilities': [0.5, 0.5],  # Simplified
        'model_type': 'regime_switching'
    }

# Pricing Models

def _black_scholes(parameters: Dict[str, Any], input_data: Dict[str, Any], options: Dict[str, Any]) -> Dict[str, Any]:
    """Black-Scholes option pricing"""
    S = parameters.get('spot_price', 100.0)
    K = parameters.get('strike_price', 100.0)
    T = parameters.get('time_to_maturity', 1.0)
    r = parameters.get('risk_free_rate', 0.05)
    sigma = parameters.get('volatility', 0.2)
    option_type = parameters.get('option_type', 'call')
    
    # Black-Scholes formula
    d1 = (np.log(S / K) + (r + 0.5 * sigma ** 2) * T) / (sigma * np.sqrt(T))
    d2 = d1 - sigma * np.sqrt(T)
    
    if option_type.lower() == 'call':
        price = S * stats.norm.cdf(d1) - K * np.exp(-r * T) * stats.norm.cdf(d2)
    else:  # put
        price = K * np.exp(-r * T) * stats.norm.cdf(-d2) - S * stats.norm.cdf(-d1)
    
    return {
        'option_price': float(price),
        'option_type': option_type,
        'parameters': {
            'spot_price': S,
            'strike_price': K,
            'time_to_maturity': T,
            'risk_free_rate': r,
            'volatility': sigma
        }
    }

def _monte_carlo_pricing(parameters: Dict[str, Any], input_data: Dict[str, Any], options: Dict[str, Any]) -> Dict[str, Any]:
    """Monte Carlo option pricing"""
    S = parameters.get('spot_price', 100.0)
    K = parameters.get('strike_price', 100.0)
    T = parameters.get('time_to_maturity', 1.0)
    r = parameters.get('risk_free_rate', 0.05)
    sigma = parameters.get('volatility', 0.2)
    num_simulations = parameters.get('num_simulations', 10000)
    option_type = parameters.get('option_type', 'call')
    
    # Monte Carlo simulation
    np.random.seed(42)  # For reproducibility
    dt = T / 252  # Daily time steps
    num_steps = int(T * 252)
    
    prices = []
    for _ in range(num_simulations):
        price = S
        for _ in range(num_steps):
            price *= np.exp((r - 0.5 * sigma ** 2) * dt + sigma * np.sqrt(dt) * np.random.normal())
        prices.append(price)
    
    # Calculate option payoff
    if option_type.lower() == 'call':
        payoffs = np.maximum(np.array(prices) - K, 0)
    else:  # put
        payoffs = np.maximum(K - np.array(prices), 0)
    
    # Discount to present value
    option_price = np.exp(-r * T) * np.mean(payoffs)
    
    return {
        'option_price': float(option_price),
        'option_type': option_type,
        'num_simulations': num_simulations,
        'standard_error': float(np.std(payoffs) / np.sqrt(num_simulations))
    }

def _binomial_tree(parameters: Dict[str, Any], input_data: Dict[str, Any], options: Dict[str, Any]) -> Dict[str, Any]:
    """Binomial tree option pricing"""
    S = parameters.get('spot_price', 100.0)
    K = parameters.get('strike_price', 100.0)
    T = parameters.get('time_to_maturity', 1.0)
    r = parameters.get('risk_free_rate', 0.05)
    sigma = parameters.get('volatility', 0.2)
    n_steps = parameters.get('n_steps', 100)
    option_type = parameters.get('option_type', 'call')
    
    # Binomial tree parameters
    dt = T / n_steps
    u = np.exp(sigma * np.sqrt(dt))
    d = 1 / u
    p = (np.exp(r * dt) - d) / (u - d)
    
    # Initialize option values at maturity
    option_values = np.zeros(n_steps + 1)
    for i in range(n_steps + 1):
        stock_price = S * (u ** (n_steps - i)) * (d ** i)
        if option_type.lower() == 'call':
            option_values[i] = max(stock_price - K, 0)
        else:  # put
            option_values[i] = max(K - stock_price, 0)
    
    # Backward induction
    for step in range(n_steps - 1, -1, -1):
        for i in range(step + 1):
            option_values[i] = np.exp(-r * dt) * (p * option_values[i] + (1 - p) * option_values[i + 1])
    
    return {
        'option_price': float(option_values[0]),
        'option_type': option_type,
        'n_steps': n_steps,
        'tree_parameters': {
            'u': u,
            'd': d,
            'p': p
        }
    }

class QuantModelRegistry:
    """Registry for quantitative models"""
    
    def __init__(self):
        self.models = {}
        self._register_default_models()
    
    def _register_default_models(self):
        """Register default quantitative models"""
        # Risk Management Models
        self.register_model("var_historical", _var_historical, ModelType.RISK_MANAGEMENT)
        self.register_model("var_parametric", _var_parametric, ModelType.RISK_MANAGEMENT)
        self.register_model("var_monte_carlo", _var_monte_carlo, ModelType.RISK_MANAGEMENT)
        self.register_model("cvar_calculation", _cvar_calculation, ModelType.RISK_MANAGEMENT)
        self.register_model("stress_test", _stress_test, ModelType.RISK_MANAGEMENT)
        
        # Portfolio Optimization Models
        self.register_model("markowitz_optimization", _markowitz_optimization, ModelType.PORTFOLIO_OPTIMIZATION)
        self.register_model("black_litterman", _black_litterman, ModelType.PORTFOLIO_OPTIMIZATION)
        self.register_model("risk_parity", _risk_parity, ModelType.PORTFOLIO_OPTIMIZATION)
        self.register_model("efficient_frontier", _efficient_frontier, ModelType.PORTFOLIO_OPTIMIZATION)
        
        # Statistical Models
        self.register_model("garch_model", _garch_model, ModelType.STATISTICAL)
        self.register_model("copula_model", _copula_model, ModelType.STATISTICAL)
        self.register_model("regime_switching", _regime_switching, ModelType.STATISTICAL)
        
        # Pricing Models
        self.register_model("black_scholes", _black_scholes, ModelType.PRICING)
        self.register_model("monte_carlo_pricing", _monte_carlo_pricing, ModelType.PRICING)
        self.register_model("binomial_tree", _binomial_tree, ModelType.PRICING)
    
    def register_model(self, name: str, function, model_type: ModelType, metadata: Dict[str, Any] = None):
        """Register a quantitative model"""
        if metadata is None:
            metadata = {}
        
        self.models[name] = {
            'function': function,
            'type': model_type,
            'metadata': metadata,
            'registered_at': datetime.utcnow()
        }
    
    def get_model(self, name: str):
        """Get a registered model"""
        return self.models.get(name)
    
    def list_models(self) -> List[str]:
        """List all registered models"""
        return list(self.models.keys())
    
    def get_models_by_type(self, model_type: ModelType) -> List[str]:
        """Get models by type"""
        return [name for name, model in self.models.items() if model['type'] == model_type]

# Global model registry
model_registry = QuantModelRegistry()

def execute_model(request: Dict[str, Any]) -> Dict[str, Any]:
    """
    Execute a quantitative model based on the request
    
    Args:
        request: Model execution request containing model name, parameters, and input data
        
    Returns:
        Model execution result
    """
    start_time = time.time()
    memory_before = psutil.Process().memory_info().rss / 1024 / 1024  # MB
    
    try:
        model_name = request.get('model_name', '')
        parameters = request.get('parameters', {})
        input_data = request.get('input_data', {})
        options = request.get('options', {})
        
        logger.info(f"Executing model: {model_name}")
        
        # Get the model from registry
        model_info = model_registry.get_model(model_name)
        if not model_info:
            return {
                'success': False,
                'error': f'Model {model_name} not found',
                'model_name': model_name,
                'execution_time': time.time() - start_time
            }
        
        # Execute the model
        model_function = model_info['function']
        result = model_function(parameters, input_data, options)
        
        # Calculate execution metrics
        execution_time = time.time() - start_time
        memory_after = psutil.Process().memory_info().rss / 1024 / 1024  # MB
        memory_usage = memory_after - memory_before
        
        return {
            'success': True,
            'model_name': model_name,
            'results': result,
            'execution_time': execution_time,
            'memory_usage_mb': memory_usage,
            'python_version': sys.version,
            'warnings': []
        }
        
    except Exception as e:
        logger.error(f"Error executing model {model_name}: {str(e)}")
        return {
            'success': False,
            'error': str(e),
            'model_name': model_name,
            'execution_time': time.time() - start_time
        }

def get_available_models() -> List[str]:
    """Get list of available models"""
    return model_registry.list_models()

def get_model_metadata(model_name: str) -> Dict[str, Any]:
    """Get metadata for a specific model"""
    model_info = model_registry.get_model(model_name)
    if not model_info:
        return {
            'success': False,
            'error': f'Model {model_name} not found',
            'model_name': model_name
        }
    
    return {
        'success': True,
        'model_name': model_name,
        'model_type': model_info['type'].value,
        'metadata': model_info['metadata'],
        'registered_at': model_info['registered_at'].isoformat()
    }

def validate_model(model_name: str, parameters: Dict[str, Any]) -> bool:
    """Validate model parameters"""
    try:
        model_info = model_registry.get_model(model_name)
        if not model_info:
            return False
        
        # Basic validation - can be extended with schema validation
        return True
    except Exception:
        return False

def get_performance_metrics() -> Dict[str, Any]:
    """Get performance metrics for the interop service"""
    process = psutil.Process()
    memory_info = process.memory_info()
    
    return {
        'total_requests': 0,  # Would be tracked in a real implementation
        'successful_requests': 0,
        'failed_requests': 0,
        'success_rate': 0.0,
        'average_execution_time': 0.0,
        'memory_usage_mb': memory_info.rss / 1024 / 1024,
        'python_version': sys.version,
        'available_models': len(model_registry.list_models())
    }

if __name__ == "__main__":
    # Example usage
    print("Quantitative Models Module")
    print("=========================")
    print(f"Available models: {get_available_models()}")
    
    # Example model execution
    request = {
        'model_name': 'var_historical',
        'parameters': {'confidence_level': 0.95},
        'input_data': {'returns': np.random.normal(0, 0.02, 1000).tolist()}
    }
    
    result = execute_model(request)
    print(f"Example execution result: {result}")
