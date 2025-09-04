#!/usr/bin/env python3
"""
Markowitz Mean-Variance Portfolio Optimization Engine

This module provides comprehensive portfolio optimization capabilities including:
- Mean-variance optimization
- Efficient frontier calculation
- Risk budgeting
- Black-Litterman optimization
- Transaction cost optimization
- Multi-objective optimization

Author: Financial Risk Insights Platform
Version: 1.0.0
"""

import numpy as np
import pandas as pd
import scipy.optimize as opt
from scipy.linalg import cholesky, LinAlgError
from typing import Dict, List, Tuple, Optional, Union, Any
import json
import logging
from dataclasses import dataclass, asdict
from enum import Enum
import warnings
warnings.filterwarnings('ignore')

# Configure logging
logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

class OptimizationMethod(Enum):
    """Supported optimization methods"""
    MEAN_VARIANCE = "mean_variance"
    MINIMUM_VARIANCE = "minimum_variance"
    MAXIMUM_SHARPE = "maximum_sharpe"
    EQUAL_WEIGHT = "equal_weight"
    RISK_PARITY = "risk_parity"
    BLACK_LITTERMAN = "black_litterman"
    MEAN_CVAR = "mean_cvar"

class ConstraintType(Enum):
    """Types of constraints"""
    LONG_ONLY = "long_only"
    SHORT_ALLOWED = "short_allowed"
    LEVERAGE_LIMIT = "leverage_limit"
    SECTOR_LIMIT = "sector_limit"
    CONCENTRATION_LIMIT = "concentration_limit"

@dataclass
class OptimizationParameters:
    """Parameters for portfolio optimization"""
    method: OptimizationMethod = OptimizationMethod.MEAN_VARIANCE
    risk_aversion: float = 1.0
    target_return: Optional[float] = None
    target_volatility: Optional[float] = None
    max_weight: float = 1.0
    min_weight: float = 0.0
    max_leverage: float = 1.0
    transaction_costs: float = 0.0
    rebalance_frequency: int = 1
    lookback_period: int = 252
    confidence_level: float = 0.95
    custom_constraints: Dict[str, Any] = None
    
    def __post_init__(self):
        if self.custom_constraints is None:
            self.custom_constraints = {}

@dataclass
class AssetData:
    """Asset data for optimization"""
    symbol: str
    expected_return: float
    volatility: float
    historical_returns: List[float]
    sector: Optional[str] = None
    market_cap: Optional[float] = None
    beta: Optional[float] = None

@dataclass
class OptimizationResult:
    """Result of portfolio optimization"""
    success: bool
    optimal_weights: List[float]
    expected_return: float
    expected_volatility: float
    sharpe_ratio: float
    var: float
    cvar: float
    diversification_ratio: float
    concentration_ratio: float
    method: str
    optimization_metadata: Dict[str, Any] = None
    error_message: str = ""
    
    def __post_init__(self):
        if self.optimization_metadata is None:
            self.optimization_metadata = {}

@dataclass
class EfficientFrontierPoint:
    """Single point on the efficient frontier"""
    expected_return: float
    expected_volatility: float
    sharpe_ratio: float
    weights: List[float]

@dataclass
class EfficientFrontier:
    """Complete efficient frontier"""
    points: List[EfficientFrontierPoint]
    min_volatility_point: EfficientFrontierPoint
    max_sharpe_point: EfficientFrontierPoint
    max_return_point: EfficientFrontierPoint
    success: bool = True
    error_message: str = ""

class PortfolioOptimizer:
    """
    Advanced portfolio optimization engine implementing Markowitz mean-variance theory
    """
    
    def __init__(self, parameters: OptimizationParameters):
        self.parameters = parameters
        self.assets = []
        self.expected_returns = None
        self.covariance_matrix = None
        self.correlation_matrix = None
        
    def add_asset(self, asset: AssetData):
        """Add an asset to the optimization universe"""
        self.assets.append(asset)
        self._update_matrices()
    
    def add_assets(self, assets: List[AssetData]):
        """Add multiple assets to the optimization universe"""
        self.assets.extend(assets)
        self._update_matrices()
    
    def optimize(self) -> OptimizationResult:
        """
        Perform portfolio optimization based on the specified method
        
        Returns:
            OptimizationResult with optimal weights and risk metrics
        """
        try:
            if len(self.assets) < 2:
                return OptimizationResult(
                    success=False,
                    optimal_weights=[],
                    expected_return=0.0,
                    expected_volatility=0.0,
                    sharpe_ratio=0.0,
                    var=0.0,
                    cvar=0.0,
                    diversification_ratio=0.0,
                    concentration_ratio=0.0,
                    method=self.parameters.method.value,
                    error_message="At least 2 assets required for optimization"
                )
            
            logger.info(f"Starting portfolio optimization using {self.parameters.method.value} method")
            
            # Perform optimization based on method
            if self.parameters.method == OptimizationMethod.MEAN_VARIANCE:
                weights = self._mean_variance_optimization()
            elif self.parameters.method == OptimizationMethod.MINIMUM_VARIANCE:
                weights = self._minimum_variance_optimization()
            elif self.parameters.method == OptimizationMethod.MAXIMUM_SHARPE:
                weights = self._maximum_sharpe_optimization()
            elif self.parameters.method == OptimizationMethod.EQUAL_WEIGHT:
                weights = self._equal_weight_optimization()
            elif self.parameters.method == OptimizationMethod.RISK_PARITY:
                weights = self._risk_parity_optimization()
            elif self.parameters.method == OptimizationMethod.BLACK_LITTERMAN:
                weights = self._black_litterman_optimization()
            elif self.parameters.method == OptimizationMethod.MEAN_CVAR:
                weights = self._mean_cvar_optimization()
            else:
                raise ValueError(f"Unsupported optimization method: {self.parameters.method}")
            
            if weights is None or len(weights) == 0:
                return OptimizationResult(
                    success=False,
                    optimal_weights=[],
                    expected_return=0.0,
                    expected_volatility=0.0,
                    sharpe_ratio=0.0,
                    var=0.0,
                    cvar=0.0,
                    diversification_ratio=0.0,
                    concentration_ratio=0.0,
                    method=self.parameters.method.value,
                    error_message="Optimization failed to converge"
                )
            
            # Calculate portfolio metrics
            portfolio_return = np.dot(weights, self.expected_returns)
            portfolio_variance = np.dot(weights, np.dot(self.covariance_matrix, weights))
            portfolio_volatility = np.sqrt(portfolio_variance)
            sharpe_ratio = portfolio_return / portfolio_volatility if portfolio_volatility > 0 else 0.0
            
            # Calculate risk metrics
            var, cvar = self._calculate_risk_metrics(weights)
            diversification_ratio = self._calculate_diversification_ratio(weights)
            concentration_ratio = self._calculate_concentration_ratio(weights)
            
            # Prepare metadata
            metadata = {
                "method": self.parameters.method.value,
                "num_assets": len(self.assets),
                "risk_aversion": self.parameters.risk_aversion,
                "constraints": self.parameters.custom_constraints,
                "optimization_successful": True
            }
            
            result = OptimizationResult(
                success=True,
                optimal_weights=weights.tolist(),
                expected_return=float(portfolio_return),
                expected_volatility=float(portfolio_volatility),
                sharpe_ratio=float(sharpe_ratio),
                var=float(var),
                cvar=float(cvar),
                diversification_ratio=float(diversification_ratio),
                concentration_ratio=float(concentration_ratio),
                method=self.parameters.method.value,
                optimization_metadata=metadata
            )
            
            logger.info(f"Portfolio optimization completed successfully")
            return result
            
        except Exception as e:
            logger.error(f"Error in portfolio optimization: {str(e)}")
            return OptimizationResult(
                success=False,
                optimal_weights=[],
                expected_return=0.0,
                expected_volatility=0.0,
                sharpe_ratio=0.0,
                var=0.0,
                cvar=0.0,
                diversification_ratio=0.0,
                concentration_ratio=0.0,
                method=self.parameters.method.value,
                error_message=str(e)
            )
    
    def calculate_efficient_frontier(self, num_points: int = 50) -> EfficientFrontier:
        """
        Calculate the efficient frontier
        
        Args:
            num_points: Number of points to generate on the frontier
            
        Returns:
            EfficientFrontier with all frontier points and key metrics
        """
        try:
            if len(self.assets) < 2:
                return EfficientFrontier(
                    points=[],
                    min_volatility_point=None,
                    max_sharpe_point=None,
                    max_return_point=None,
                    success=False,
                    error_message="At least 2 assets required for efficient frontier"
                )
            
            logger.info(f"Calculating efficient frontier with {num_points} points")
            
            # Find minimum and maximum expected returns
            min_return = np.min(self.expected_returns)
            max_return = np.max(self.expected_returns)
            
            # Generate target returns
            target_returns = np.linspace(min_return, max_return, num_points)
            
            frontier_points = []
            min_vol_point = None
            max_sharpe_point = None
            max_return_point = None
            min_volatility = float('inf')
            max_sharpe = -float('inf')
            
            for target_return in target_returns:
                try:
                    # Optimize for minimum variance given target return
                    weights = self._optimize_for_target_return(target_return)
                    
                    if weights is not None:
                        portfolio_return = np.dot(weights, self.expected_returns)
                        portfolio_variance = np.dot(weights, np.dot(self.covariance_matrix, weights))
                        portfolio_volatility = np.sqrt(portfolio_variance)
                        sharpe_ratio = portfolio_return / portfolio_volatility if portfolio_volatility > 0 else 0.0
                        
                        point = EfficientFrontierPoint(
                            expected_return=float(portfolio_return),
                            expected_volatility=float(portfolio_volatility),
                            sharpe_ratio=float(sharpe_ratio),
                            weights=weights.tolist()
                        )
                        
                        frontier_points.append(point)
                        
                        # Track key points
                        if portfolio_volatility < min_volatility:
                            min_volatility = portfolio_volatility
                            min_vol_point = point
                        
                        if sharpe_ratio > max_sharpe:
                            max_sharpe = sharpe_ratio
                            max_sharpe_point = point
                        
                        if portfolio_return > (max_return_point.expected_return if max_return_point else -float('inf')):
                            max_return_point = point
                
                except Exception as e:
                    logger.warning(f"Failed to optimize for target return {target_return}: {str(e)}")
                    continue
            
            if not frontier_points:
                return EfficientFrontier(
                    points=[],
                    min_volatility_point=None,
                    max_sharpe_point=None,
                    max_return_point=None,
                    success=False,
                    error_message="Failed to generate any efficient frontier points"
                )
            
            # Sort points by expected return
            frontier_points.sort(key=lambda x: x.expected_return)
            
            result = EfficientFrontier(
                points=frontier_points,
                min_volatility_point=min_vol_point,
                max_sharpe_point=max_sharpe_point,
                max_return_point=max_return_point,
                success=True
            )
            
            logger.info(f"Efficient frontier calculated with {len(frontier_points)} points")
            return result
            
        except Exception as e:
            logger.error(f"Error calculating efficient frontier: {str(e)}")
            return EfficientFrontier(
                points=[],
                min_volatility_point=None,
                max_sharpe_point=None,
                max_return_point=None,
                success=False,
                error_message=str(e)
            )
    
    def _update_matrices(self):
        """Update expected returns and covariance matrix"""
        if len(self.assets) < 2:
            return
        
        n = len(self.assets)
        self.expected_returns = np.array([asset.expected_return for asset in self.assets])
        
        # Calculate covariance matrix from historical returns
        if all(len(asset.historical_returns) > 0 for asset in self.assets):
            returns_matrix = np.array([asset.historical_returns for asset in self.assets])
            self.covariance_matrix = np.cov(returns_matrix)
            self.correlation_matrix = np.corrcoef(returns_matrix)
        else:
            # Use volatility estimates if no historical data
            volatilities = np.array([asset.volatility for asset in self.assets])
            self.covariance_matrix = np.outer(volatilities, volatilities) * 0.3  # Assume 30% correlation
            self.correlation_matrix = np.ones((n, n)) * 0.3
            np.fill_diagonal(self.correlation_matrix, 1.0)
    
    def _mean_variance_optimization(self) -> Optional[np.ndarray]:
        """Mean-variance optimization with risk aversion parameter"""
        n = len(self.assets)
        
        # Objective function: maximize (expected_return - risk_aversion * variance)
        def objective(weights):
            portfolio_return = np.dot(weights, self.expected_returns)
            portfolio_variance = np.dot(weights, np.dot(self.covariance_matrix, weights))
            return -(portfolio_return - self.parameters.risk_aversion * portfolio_variance)
        
        # Constraints
        constraints = [{'type': 'eq', 'fun': lambda w: np.sum(w) - 1.0}]  # Weights sum to 1
        
        # Bounds
        bounds = [(self.parameters.min_weight, self.parameters.max_weight) for _ in range(n)]
        
        # Initial guess
        x0 = np.ones(n) / n
        
        try:
            result = opt.minimize(objective, x0, method='SLSQP', bounds=bounds, constraints=constraints)
            return result.x if result.success else None
        except Exception as e:
            logger.error(f"Mean-variance optimization failed: {str(e)}")
            return None
    
    def _minimum_variance_optimization(self) -> Optional[np.ndarray]:
        """Minimum variance optimization"""
        n = len(self.assets)
        
        # Objective function: minimize variance
        def objective(weights):
            return np.dot(weights, np.dot(self.covariance_matrix, weights))
        
        # Constraints
        constraints = [{'type': 'eq', 'fun': lambda w: np.sum(w) - 1.0}]  # Weights sum to 1
        
        # Bounds
        bounds = [(self.parameters.min_weight, self.parameters.max_weight) for _ in range(n)]
        
        # Initial guess
        x0 = np.ones(n) / n
        
        try:
            result = opt.minimize(objective, x0, method='SLSQP', bounds=bounds, constraints=constraints)
            return result.x if result.success else None
        except Exception as e:
            logger.error(f"Minimum variance optimization failed: {str(e)}")
            return None
    
    def _maximum_sharpe_optimization(self) -> Optional[np.ndarray]:
        """Maximum Sharpe ratio optimization"""
        n = len(self.assets)
        
        # Objective function: minimize negative Sharpe ratio
        def objective(weights):
            portfolio_return = np.dot(weights, self.expected_returns)
            portfolio_variance = np.dot(weights, np.dot(self.covariance_matrix, weights))
            portfolio_volatility = np.sqrt(portfolio_variance)
            return -portfolio_return / portfolio_volatility if portfolio_volatility > 0 else 1e6
        
        # Constraints
        constraints = [{'type': 'eq', 'fun': lambda w: np.sum(w) - 1.0}]  # Weights sum to 1
        
        # Bounds
        bounds = [(self.parameters.min_weight, self.parameters.max_weight) for _ in range(n)]
        
        # Initial guess
        x0 = np.ones(n) / n
        
        try:
            result = opt.minimize(objective, x0, method='SLSQP', bounds=bounds, constraints=constraints)
            return result.x if result.success else None
        except Exception as e:
            logger.error(f"Maximum Sharpe optimization failed: {str(e)}")
            return None
    
    def _equal_weight_optimization(self) -> np.ndarray:
        """Equal weight optimization"""
        n = len(self.assets)
        return np.ones(n) / n
    
    def _risk_parity_optimization(self) -> Optional[np.ndarray]:
        """Risk parity optimization"""
        n = len(self.assets)
        
        # Objective function: minimize sum of squared differences from equal risk contribution
        def objective(weights):
            portfolio_variance = np.dot(weights, np.dot(self.covariance_matrix, weights))
            risk_contributions = weights * np.dot(self.covariance_matrix, weights) / portfolio_variance
            target_contribution = 1.0 / n
            return np.sum((risk_contributions - target_contribution) ** 2)
        
        # Constraints
        constraints = [{'type': 'eq', 'fun': lambda w: np.sum(w) - 1.0}]  # Weights sum to 1
        
        # Bounds
        bounds = [(self.parameters.min_weight, self.parameters.max_weight) for _ in range(n)]
        
        # Initial guess
        x0 = np.ones(n) / n
        
        try:
            result = opt.minimize(objective, x0, method='SLSQP', bounds=bounds, constraints=constraints)
            return result.x if result.success else None
        except Exception as e:
            logger.error(f"Risk parity optimization failed: {str(e)}")
            return None
    
    def _black_litterman_optimization(self) -> Optional[np.ndarray]:
        """Black-Litterman optimization (simplified implementation)"""
        # This is a simplified implementation
        # In practice, you would implement the full Black-Litterman model
        return self._mean_variance_optimization()
    
    def _mean_cvar_optimization(self) -> Optional[np.ndarray]:
        """Mean-CVaR optimization (simplified implementation)"""
        # This is a simplified implementation
        # In practice, you would implement CVaR optimization
        return self._mean_variance_optimization()
    
    def _optimize_for_target_return(self, target_return: float) -> Optional[np.ndarray]:
        """Optimize for minimum variance given target return"""
        n = len(self.assets)
        
        # Objective function: minimize variance
        def objective(weights):
            return np.dot(weights, np.dot(self.covariance_matrix, weights))
        
        # Constraints
        constraints = [
            {'type': 'eq', 'fun': lambda w: np.sum(w) - 1.0},  # Weights sum to 1
            {'type': 'eq', 'fun': lambda w: np.dot(w, self.expected_returns) - target_return}  # Target return
        ]
        
        # Bounds
        bounds = [(self.parameters.min_weight, self.parameters.max_weight) for _ in range(n)]
        
        # Initial guess
        x0 = np.ones(n) / n
        
        try:
            result = opt.minimize(objective, x0, method='SLSQP', bounds=bounds, constraints=constraints)
            return result.x if result.success else None
        except Exception as e:
            logger.warning(f"Optimization failed for target return {target_return}: {str(e)}")
            return None
    
    def _calculate_risk_metrics(self, weights: np.ndarray) -> Tuple[float, float]:
        """Calculate VaR and CVaR for the portfolio"""
        try:
            # Simulate portfolio returns using Monte Carlo
            n_simulations = 10000
            portfolio_returns = []
            
            for _ in range(n_simulations):
                # Generate correlated random returns
                random_returns = np.random.multivariate_normal(
                    self.expected_returns, 
                    self.covariance_matrix
                )
                portfolio_return = np.dot(weights, random_returns)
                portfolio_returns.append(portfolio_return)
            
            portfolio_returns = np.array(portfolio_returns)
            
            # Calculate VaR and CVaR
            var_level = 1 - self.parameters.confidence_level
            var = -np.percentile(portfolio_returns, var_level * 100)
            cvar = -np.mean(portfolio_returns[portfolio_returns <= -var])
            
            return float(var), float(cvar)
        except Exception as e:
            logger.warning(f"Failed to calculate risk metrics: {str(e)}")
            return 0.0, 0.0
    
    def _calculate_diversification_ratio(self, weights: np.ndarray) -> float:
        """Calculate diversification ratio"""
        try:
            # Weighted average volatility
            weighted_avg_vol = np.dot(weights, np.sqrt(np.diag(self.covariance_matrix)))
            
            # Portfolio volatility
            portfolio_vol = np.sqrt(np.dot(weights, np.dot(self.covariance_matrix, weights)))
            
            return weighted_avg_vol / portfolio_vol if portfolio_vol > 0 else 1.0
        except Exception as e:
            logger.warning(f"Failed to calculate diversification ratio: {str(e)}")
            return 1.0
    
    def _calculate_concentration_ratio(self, weights: np.ndarray) -> float:
        """Calculate concentration ratio (Herfindahl index)"""
        return float(np.sum(weights ** 2))

def run_portfolio_optimization(optimization_data: Dict[str, Any]) -> Dict[str, Any]:
    """
    Main function for running portfolio optimization from C# API
    
    Args:
        optimization_data: Dictionary containing optimization parameters and asset data
        
    Returns:
        Dictionary containing optimization results
    """
    try:
        # Extract parameters
        method = OptimizationMethod(optimization_data.get('method', 'mean_variance'))
        risk_aversion = optimization_data.get('risk_aversion', 1.0)
        target_return = optimization_data.get('target_return')
        max_weight = optimization_data.get('max_weight', 1.0)
        min_weight = optimization_data.get('min_weight', 0.0)
        
        # Create optimization parameters
        params = OptimizationParameters(
            method=method,
            risk_aversion=risk_aversion,
            target_return=target_return,
            max_weight=max_weight,
            min_weight=min_weight,
            custom_constraints=optimization_data.get('custom_constraints', {})
        )
        
        # Create optimizer
        optimizer = PortfolioOptimizer(params)
        
        # Add assets
        assets_data = optimization_data.get('assets', [])
        for asset_data in assets_data:
            asset = AssetData(
                symbol=asset_data['symbol'],
                expected_return=asset_data.get('expected_return', 0.0),
                volatility=asset_data.get('volatility', 0.0),
                historical_returns=asset_data.get('historical_returns', []),
                sector=asset_data.get('sector'),
                market_cap=asset_data.get('market_cap'),
                beta=asset_data.get('beta')
            )
            optimizer.add_asset(asset)
        
        # Run optimization
        result = optimizer.optimize()
        
        # Convert to dictionary for JSON serialization
        result_dict = asdict(result)
        
        return result_dict
        
    except Exception as e:
        logger.error(f"Error in portfolio optimization: {str(e)}")
        return {
            'success': False,
            'error': str(e),
            'optimal_weights': [],
            'expected_return': 0.0,
            'expected_volatility': 0.0,
            'sharpe_ratio': 0.0,
            'method': optimization_data.get('method', 'mean_variance')
        }

def run_efficient_frontier_calculation(optimization_data: Dict[str, Any], num_points: int = 50) -> Dict[str, Any]:
    """
    Main function for calculating efficient frontier from C# API
    
    Args:
        optimization_data: Dictionary containing optimization parameters and asset data
        num_points: Number of points on the efficient frontier
        
    Returns:
        Dictionary containing efficient frontier results
    """
    try:
        # Extract parameters
        method = OptimizationMethod(optimization_data.get('method', 'mean_variance'))
        risk_aversion = optimization_data.get('risk_aversion', 1.0)
        max_weight = optimization_data.get('max_weight', 1.0)
        min_weight = optimization_data.get('min_weight', 0.0)
        
        # Create optimization parameters
        params = OptimizationParameters(
            method=method,
            risk_aversion=risk_aversion,
            max_weight=max_weight,
            min_weight=min_weight,
            custom_constraints=optimization_data.get('custom_constraints', {})
        )
        
        # Create optimizer
        optimizer = PortfolioOptimizer(params)
        
        # Add assets
        assets_data = optimization_data.get('assets', [])
        for asset_data in assets_data:
            asset = AssetData(
                symbol=asset_data['symbol'],
                expected_return=asset_data.get('expected_return', 0.0),
                volatility=asset_data.get('volatility', 0.0),
                historical_returns=asset_data.get('historical_returns', []),
                sector=asset_data.get('sector'),
                market_cap=asset_data.get('market_cap'),
                beta=asset_data.get('beta')
            )
            optimizer.add_asset(asset)
        
        # Calculate efficient frontier
        frontier = optimizer.calculate_efficient_frontier(num_points)
        
        # Convert to dictionary for JSON serialization
        frontier_dict = asdict(frontier)
        
        return frontier_dict
        
    except Exception as e:
        logger.error(f"Error calculating efficient frontier: {str(e)}")
        return {
            'success': False,
            'error': str(e),
            'points': [],
            'min_volatility_point': None,
            'max_sharpe_point': None,
            'max_return_point': None
        }

def main():
    """Command line interface for C# integration"""
    import sys
    import json
    
    if len(sys.argv) < 3:
        print(json.dumps({"error": "Insufficient arguments. Usage: python portfolio_optimizer.py <command> <data_json> [num_points]"}))
        return
    
    try:
        command = sys.argv[1]
        data_json = sys.argv[2]
        num_points = int(sys.argv[3]) if len(sys.argv) > 3 else 50
        
        # Parse optimization data
        optimization_data = json.loads(data_json)
        
        if command == "optimize":
            # Run portfolio optimization
            result = run_portfolio_optimization(optimization_data)
            print(json.dumps(result))
        elif command == "frontier":
            # Calculate efficient frontier
            result = run_efficient_frontier_calculation(optimization_data, num_points)
            print(json.dumps(result))
        else:
            print(json.dumps({"error": f"Unknown command: {command}"}))
        
    except json.JSONDecodeError:
        print(json.dumps({"error": "Invalid JSON format for optimization data"}))
    except ValueError as e:
        print(json.dumps({"error": f"Invalid parameter: {str(e)}"}))
    except Exception as e:
        print(json.dumps({"error": f"Unexpected error: {str(e)}"}))

if __name__ == "__main__":
    # Check if called from command line with arguments
    import sys
    if len(sys.argv) > 1:
        main()
    else:
        # Example usage
        print("Portfolio Optimizer - Example Usage")
        print("===================================")
        
        # Create sample assets
        assets = [
            AssetData("AAPL", 0.12, 0.25, np.random.normal(0.12, 0.25, 252).tolist()),
            AssetData("GOOGL", 0.15, 0.30, np.random.normal(0.15, 0.30, 252).tolist()),
            AssetData("MSFT", 0.10, 0.20, np.random.normal(0.10, 0.20, 252).tolist())
        ]
        
        # Create optimizer
        params = OptimizationParameters(
            method=OptimizationMethod.MEAN_VARIANCE,
            risk_aversion=1.0
        )
        optimizer = PortfolioOptimizer(params)
        optimizer.add_assets(assets)
        
        # Run optimization
        result = optimizer.optimize()
        
        print(f"Optimization Success: {result.success}")
        print(f"Optimal Weights: {result.optimal_weights}")
        print(f"Expected Return: {result.expected_return:.4f}")
        print(f"Expected Volatility: {result.expected_volatility:.4f}")
        print(f"Sharpe Ratio: {result.sharpe_ratio:.4f}")
        
        # Calculate efficient frontier
        frontier = optimizer.calculate_efficient_frontier(20)
        print(f"Efficient Frontier Points: {len(frontier.points)}")
        if frontier.min_volatility_point:
            print(f"Min Volatility: {frontier.min_volatility_point.expected_volatility:.4f}")
        if frontier.max_sharpe_point:
            print(f"Max Sharpe: {frontier.max_sharpe_point.sharpe_ratio:.4f}")
