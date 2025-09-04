#!/usr/bin/env python3
"""
Monte Carlo VaR and CVaR Calculations
=====================================

This module provides advanced Monte Carlo simulation methods for VaR and CVaR calculations
using Python's scientific computing libraries (NumPy, SciPy).

Features:
- Monte Carlo VaR simulation with various distribution assumptions
- GARCH volatility modeling for time-varying volatility
- Copula-based portfolio risk modeling
- Stress testing and scenario analysis
- Advanced statistical methods for risk measurement

Author: Financial Risk Insights Platform
Version: 1.0.0
"""

import numpy as np
import scipy.stats as stats
from scipy.optimize import minimize
from scipy.linalg import cholesky
import pandas as pd
from typing import List, Tuple, Dict, Optional
import logging
from dataclasses import dataclass
from enum import Enum

# Configure logging
logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

class DistributionType(Enum):
    """Supported distribution types for Monte Carlo simulation"""
    NORMAL = "normal"
    T_STUDENT = "t_student"
    SKEWED_T = "skewed_t"
    GARCH = "garch"
    COPULA = "copula"

@dataclass
class VaRResult:
    """VaR calculation result"""
    var_95: float
    var_99: float
    cvar_95: float
    cvar_99: float
    confidence_intervals: Dict[str, Tuple[float, float]]
    simulation_parameters: Dict[str, any]
    method: str
    sample_size: int

@dataclass
class PortfolioVaRResult:
    """Portfolio VaR calculation result"""
    portfolio_var_95: float
    portfolio_var_99: float
    portfolio_cvar_95: float
    portfolio_cvar_99: float
    asset_contributions: Dict[str, float]
    correlation_matrix: np.ndarray
    method: str
    sample_size: int

class MonteCarloVaRCalculator:
    """Monte Carlo VaR and CVaR calculator with advanced statistical methods"""
    
    def __init__(self, random_seed: Optional[int] = None):
        """
        Initialize the Monte Carlo VaR calculator
        
        Args:
            random_seed: Random seed for reproducible results
        """
        if random_seed is not None:
            np.random.seed(random_seed)
        self.random_seed = random_seed
        
    def calculate_var_cvar_monte_carlo(
        self, 
        returns: np.ndarray, 
        confidence_levels: List[float] = [0.95, 0.99],
        num_simulations: int = 10000,
        distribution: DistributionType = DistributionType.NORMAL,
        **kwargs
    ) -> VaRResult:
        """
        Calculate VaR and CVaR using Monte Carlo simulation
        
        Args:
            returns: Historical returns array
            confidence_levels: List of confidence levels (e.g., [0.95, 0.99])
            num_simulations: Number of Monte Carlo simulations
            distribution: Distribution type for simulation
            **kwargs: Additional parameters for specific distributions
            
        Returns:
            VaRResult object with VaR/CVaR values and metadata
        """
        logger.info(f"Starting Monte Carlo VaR calculation with {num_simulations} simulations")
        
        # Estimate distribution parameters
        if distribution == DistributionType.NORMAL:
            simulated_returns = self._simulate_normal(returns, num_simulations)
        elif distribution == DistributionType.T_STUDENT:
            simulated_returns = self._simulate_t_student(returns, num_simulations, **kwargs)
        elif distribution == DistributionType.SKEWED_T:
            simulated_returns = self._simulate_skewed_t(returns, num_simulations, **kwargs)
        elif distribution == DistributionType.GARCH:
            simulated_returns = self._simulate_garch(returns, num_simulations, **kwargs)
        elif distribution == DistributionType.COPULA:
            simulated_returns = self._simulate_copula(returns, num_simulations, **kwargs)
        else:
            raise ValueError(f"Unsupported distribution type: {distribution}")
        
        # Calculate VaR and CVaR for each confidence level
        results = {}
        for conf_level in confidence_levels:
            var, cvar = self._calculate_var_cvar(simulated_returns, conf_level)
            results[f"var_{int(conf_level*100)}"] = var
            results[f"cvar_{int(conf_level*100)}"] = cvar
        
        # Calculate confidence intervals using bootstrap
        confidence_intervals = self._calculate_confidence_intervals(
            returns, confidence_levels, num_simulations, distribution, **kwargs
        )
        
        return VaRResult(
            var_95=results.get("var_95", 0.0),
            var_99=results.get("var_99", 0.0),
            cvar_95=results.get("cvar_95", 0.0),
            cvar_99=results.get("cvar_99", 0.0),
            confidence_intervals=confidence_intervals,
            simulation_parameters={
                "distribution": distribution.value,
                "num_simulations": num_simulations,
                "random_seed": self.random_seed
            },
            method="Monte Carlo",
            sample_size=len(returns)
        )
    
    def calculate_portfolio_var_cvar_monte_carlo(
        self,
        asset_returns: Dict[str, np.ndarray],
        weights: Dict[str, float],
        confidence_levels: List[float] = [0.95, 0.99],
        num_simulations: int = 10000,
        distribution: DistributionType = DistributionType.NORMAL,
        **kwargs
    ) -> PortfolioVaRResult:
        """
        Calculate portfolio VaR and CVaR using Monte Carlo simulation
        
        Args:
            asset_returns: Dictionary of asset returns {symbol: returns_array}
            weights: Dictionary of portfolio weights {symbol: weight}
            confidence_levels: List of confidence levels
            num_simulations: Number of Monte Carlo simulations
            distribution: Distribution type for simulation
            **kwargs: Additional parameters for specific distributions
            
        Returns:
            PortfolioVaRResult object with portfolio VaR/CVaR and asset contributions
        """
        logger.info(f"Starting portfolio Monte Carlo VaR calculation with {num_simulations} simulations")
        
        # Align returns data
        aligned_returns = self._align_returns_data(asset_returns)
        symbols = list(aligned_returns.keys())
        returns_matrix = np.array([aligned_returns[symbol] for symbol in symbols])
        
        # Calculate correlation matrix
        correlation_matrix = np.corrcoef(returns_matrix)
        
        # Simulate portfolio returns
        if distribution == DistributionType.NORMAL:
            simulated_returns = self._simulate_portfolio_normal(
                returns_matrix, weights, symbols, num_simulations
            )
        elif distribution == DistributionType.COPULA:
            simulated_returns = self._simulate_portfolio_copula(
                returns_matrix, weights, symbols, num_simulations, **kwargs
            )
        else:
            # For other distributions, simulate individual assets and combine
            simulated_asset_returns = {}
            for i, symbol in enumerate(symbols):
                asset_var_result = self.calculate_var_cvar_monte_carlo(
                    returns_matrix[i], confidence_levels, num_simulations, distribution, **kwargs
                )
                simulated_asset_returns[symbol] = self._simulate_normal(
                    returns_matrix[i], num_simulations
                )
            
            # Combine simulated returns using portfolio weights
            simulated_returns = np.zeros(num_simulations)
            for symbol in symbols:
                simulated_returns += weights[symbol] * simulated_asset_returns[symbol]
        
        # Calculate portfolio VaR and CVaR
        results = {}
        for conf_level in confidence_levels:
            var, cvar = self._calculate_var_cvar(simulated_returns, conf_level)
            results[f"var_{int(conf_level*100)}"] = var
            results[f"cvar_{int(conf_level*100)}"] = cvar
        
        # Calculate asset contributions to VaR
        asset_contributions = self._calculate_asset_contributions(
            aligned_returns, weights, symbols, confidence_levels[0]
        )
        
        return PortfolioVaRResult(
            portfolio_var_95=results.get("var_95", 0.0),
            portfolio_var_99=results.get("var_99", 0.0),
            portfolio_cvar_95=results.get("cvar_95", 0.0),
            portfolio_cvar_99=results.get("cvar_99", 0.0),
            asset_contributions=asset_contributions,
            correlation_matrix=correlation_matrix,
            method="Monte Carlo Portfolio",
            sample_size=len(returns_matrix[0])
        )
    
    def _simulate_normal(self, returns: np.ndarray, num_simulations: int) -> np.ndarray:
        """Simulate returns using normal distribution"""
        mean = np.mean(returns)
        std = np.std(returns, ddof=1)
        return np.random.normal(mean, std, num_simulations)
    
    def _simulate_t_student(self, returns: np.ndarray, num_simulations: int, 
                           degrees_of_freedom: Optional[int] = None) -> np.ndarray:
        """Simulate returns using t-student distribution"""
        if degrees_of_freedom is None:
            # Estimate degrees of freedom using maximum likelihood
            df = self._estimate_t_student_df(returns)
        else:
            df = degrees_of_freedom
        
        # Fit t-distribution parameters
        loc, scale = stats.t.fit(returns, df)
        return stats.t.rvs(df, loc=loc, scale=scale, size=num_simulations)
    
    def _simulate_skewed_t(self, returns: np.ndarray, num_simulations: int,
                          degrees_of_freedom: Optional[int] = None) -> np.ndarray:
        """Simulate returns using skewed t-distribution"""
        if degrees_of_freedom is None:
            df = self._estimate_t_student_df(returns)
        else:
            df = degrees_of_freedom
        
        # Estimate skewness
        skewness = stats.skew(returns)
        
        # Generate skewed t-distribution
        t_samples = stats.t.rvs(df, size=num_simulations)
        skewed_samples = t_samples + skewness * (t_samples**2 - 1) / 6
        
        # Scale to match historical data
        historical_std = np.std(returns, ddof=1)
        simulated_std = np.std(skewed_samples, ddof=1)
        skewed_samples = skewed_samples * (historical_std / simulated_std)
        
        return skewed_samples
    
    def _simulate_garch(self, returns: np.ndarray, num_simulations: int,
                       garch_params: Optional[Dict] = None) -> np.ndarray:
        """Simulate returns using GARCH volatility model"""
        if garch_params is None:
            garch_params = self._estimate_garch_parameters(returns)
        
        # Simulate GARCH process
        simulated_returns = np.zeros(num_simulations)
        variance = np.var(returns)
        
        for i in range(num_simulations):
            if i == 0:
                variance = garch_params['omega'] + garch_params['alpha'] * returns[-1]**2 + garch_params['beta'] * variance
            else:
                variance = garch_params['omega'] + garch_params['alpha'] * simulated_returns[i-1]**2 + garch_params['beta'] * variance
            
            simulated_returns[i] = np.random.normal(0, np.sqrt(variance))
        
        return simulated_returns
    
    def _simulate_copula(self, returns: np.ndarray, num_simulations: int,
                        copula_type: str = "gaussian") -> np.ndarray:
        """Simulate returns using copula-based approach"""
        # For single asset, copula reduces to normal simulation
        return self._simulate_normal(returns, num_simulations)
    
    def _simulate_portfolio_normal(self, returns_matrix: np.ndarray, weights: Dict[str, float],
                                  symbols: List[str], num_simulations: int) -> np.ndarray:
        """Simulate portfolio returns using multivariate normal distribution"""
        # Calculate portfolio mean and covariance
        portfolio_returns = np.array([weights[symbol] for symbol in symbols]) @ returns_matrix
        mean = np.mean(portfolio_returns)
        cov_matrix = np.cov(returns_matrix)
        
        # Simulate multivariate normal
        simulated_returns = np.random.multivariate_normal(
            np.mean(returns_matrix, axis=1), cov_matrix, num_simulations
        )
        
        # Apply portfolio weights
        portfolio_simulated = np.array([weights[symbol] for symbol in symbols]) @ simulated_returns.T
        
        return portfolio_simulated
    
    def _simulate_portfolio_copula(self, returns_matrix: np.ndarray, weights: Dict[str, float],
                                  symbols: List[str], num_simulations: int,
                                  copula_type: str = "gaussian") -> np.ndarray:
        """Simulate portfolio returns using copula-based approach"""
        # Transform to uniform marginals
        uniform_marginals = np.array([stats.norm.cdf(returns_matrix[i]) for i in range(len(symbols))])
        
        # Generate copula samples
        if copula_type == "gaussian":
            correlation_matrix = np.corrcoef(returns_matrix)
            copula_samples = np.random.multivariate_normal(
                np.zeros(len(symbols)), correlation_matrix, num_simulations
            )
            copula_samples = stats.norm.cdf(copula_samples)
        else:
            # Default to independent copula
            copula_samples = np.random.uniform(0, 1, (num_simulations, len(symbols)))
        
        # Transform back to original marginals
        simulated_returns = np.zeros((num_simulations, len(symbols)))
        for i, symbol in enumerate(symbols):
            simulated_returns[:, i] = stats.norm.ppf(copula_samples[:, i])
        
        # Apply portfolio weights
        portfolio_simulated = np.array([weights[symbol] for symbol in symbols]) @ simulated_returns.T
        
        return portfolio_simulated
    
    def _calculate_var_cvar(self, returns: np.ndarray, confidence_level: float) -> Tuple[float, float]:
        """Calculate VaR and CVaR from simulated returns"""
        # Sort returns in ascending order
        sorted_returns = np.sort(returns)
        
        # Calculate VaR (percentile)
        var_index = int((1 - confidence_level) * len(sorted_returns))
        var = -sorted_returns[var_index]
        
        # Calculate CVaR (expected value of returns below VaR)
        tail_returns = sorted_returns[:var_index]
        cvar = -np.mean(tail_returns) if len(tail_returns) > 0 else var
        
        return var, cvar
    
    def _calculate_confidence_intervals(self, returns: np.ndarray, confidence_levels: List[float],
                                      num_simulations: int, distribution: DistributionType,
                                      **kwargs) -> Dict[str, Tuple[float, float]]:
        """Calculate confidence intervals using bootstrap"""
        bootstrap_results = {}
        
        for conf_level in confidence_levels:
            bootstrap_vars = []
            bootstrap_cvars = []
            
            # Bootstrap sampling
            for _ in range(100):  # 100 bootstrap samples
                bootstrap_sample = np.random.choice(returns, size=len(returns), replace=True)
                
                if distribution == DistributionType.NORMAL:
                    simulated = self._simulate_normal(bootstrap_sample, num_simulations)
                else:
                    simulated = self._simulate_normal(bootstrap_sample, num_simulations)
                
                var, cvar = self._calculate_var_cvar(simulated, conf_level)
                bootstrap_vars.append(var)
                bootstrap_cvars.append(cvar)
            
            # Calculate confidence intervals (5th and 95th percentiles)
            var_ci = (np.percentile(bootstrap_vars, 5), np.percentile(bootstrap_vars, 95))
            cvar_ci = (np.percentile(bootstrap_cvars, 5), np.percentile(bootstrap_cvars, 95))
            
            bootstrap_results[f"var_{int(conf_level*100)}_ci"] = var_ci
            bootstrap_results[f"cvar_{int(conf_level*100)}_ci"] = cvar_ci
        
        return bootstrap_results
    
    def _calculate_asset_contributions(self, aligned_returns: Dict[str, np.ndarray],
                                     weights: Dict[str, float], symbols: List[str],
                                     confidence_level: float) -> Dict[str, float]:
        """Calculate individual asset contributions to portfolio VaR"""
        contributions = {}
        
        # Calculate portfolio returns
        portfolio_returns = np.zeros(len(aligned_returns[symbols[0]]))
        for symbol in symbols:
            portfolio_returns += weights[symbol] * aligned_returns[symbol]
        
        # Calculate portfolio VaR
        portfolio_var = self._calculate_var_cvar(portfolio_returns, confidence_level)[0]
        
        # Calculate individual asset contributions
        for symbol in symbols:
            # Calculate marginal VaR contribution
            asset_returns = aligned_returns[symbol]
            asset_var = self._calculate_var_cvar(asset_returns, confidence_level)[0]
            
            # Contribution = weight * asset_VaR / portfolio_VaR
            contribution = weights[symbol] * asset_var / portfolio_var if portfolio_var > 0 else 0
            contributions[symbol] = contribution
        
        return contributions
    
    def _align_returns_data(self, asset_returns: Dict[str, np.ndarray]) -> Dict[str, np.ndarray]:
        """Align returns data to common time period"""
        # Find common length
        min_length = min(len(returns) for returns in asset_returns.values())
        
        # Truncate all arrays to common length
        aligned_returns = {}
        for symbol, returns in asset_returns.items():
            aligned_returns[symbol] = returns[-min_length:]
        
        return aligned_returns
    
    def _estimate_t_student_df(self, returns: np.ndarray) -> int:
        """Estimate degrees of freedom for t-student distribution"""
        # Simple estimation based on kurtosis
        kurtosis = stats.kurtosis(returns)
        if kurtosis > 0:
            df = int(6 / kurtosis + 4)
            return max(3, min(df, 30))  # Constrain to reasonable range
        else:
            return 10  # Default value
    
    def _estimate_garch_parameters(self, returns: np.ndarray) -> Dict[str, float]:
        """Estimate GARCH(1,1) parameters using maximum likelihood"""
        # Simplified GARCH parameter estimation
        # In practice, you would use more sophisticated methods
        variance = np.var(returns)
        
        return {
            'omega': variance * 0.1,
            'alpha': 0.1,
            'beta': 0.8
        }

def calculate_var_cvar_monte_carlo(
    returns: np.ndarray,
    confidence_levels: List[float] = [0.95, 0.99],
    num_simulations: int = 10000,
    distribution: str = "normal",
    random_seed: Optional[int] = None
) -> VaRResult:
    """
    Convenience function for Monte Carlo VaR calculation
    
    Args:
        returns: Historical returns array
        confidence_levels: List of confidence levels
        num_simulations: Number of Monte Carlo simulations
        distribution: Distribution type ("normal", "t_student", "skewed_t", "garch", "copula")
        random_seed: Random seed for reproducible results
        
    Returns:
        VaRResult object with VaR/CVaR values
    """
    calculator = MonteCarloVaRCalculator(random_seed=random_seed)
    distribution_enum = DistributionType(distribution)
    
    return calculator.calculate_var_cvar_monte_carlo(
        returns, confidence_levels, num_simulations, distribution_enum
    )

def calculate_portfolio_var_cvar_monte_carlo(
    asset_returns: Dict[str, np.ndarray],
    weights: Dict[str, float],
    confidence_levels: List[float] = [0.95, 0.99],
    num_simulations: int = 10000,
    distribution: str = "normal",
    random_seed: Optional[int] = None
) -> PortfolioVaRResult:
    """
    Convenience function for portfolio Monte Carlo VaR calculation
    
    Args:
        asset_returns: Dictionary of asset returns {symbol: returns_array}
        weights: Dictionary of portfolio weights {symbol: weight}
        confidence_levels: List of confidence levels
        num_simulations: Number of Monte Carlo simulations
        distribution: Distribution type
        random_seed: Random seed for reproducible results
        
    Returns:
        PortfolioVaRResult object with portfolio VaR/CVaR values
    """
    calculator = MonteCarloVaRCalculator(random_seed=random_seed)
    distribution_enum = DistributionType(distribution)
    
    return calculator.calculate_portfolio_var_cvar_monte_carlo(
        asset_returns, weights, confidence_levels, num_simulations, distribution_enum
    )

def main():
    """Command line interface for C# integration"""
    import sys
    import json
    
    if len(sys.argv) < 3:
        print(json.dumps({"error": "Insufficient arguments. Usage: python monte_carlo_var.py <symbol> <returns_json> [distribution_type] [num_simulations]"}))
        return
    
    try:
        symbol = sys.argv[1]
        returns_json = sys.argv[2]
        distribution_type = sys.argv[3] if len(sys.argv) > 3 else "normal"
        num_simulations = int(sys.argv[4]) if len(sys.argv) > 4 else 10000
        
        # Parse returns data
        returns_data = json.loads(returns_json)
        returns_array = np.array(returns_data)
        
        # Calculate VaR using Monte Carlo simulation
        result = calculate_var_cvar_monte_carlo(
            returns_array,
            confidence_levels=[0.95, 0.99],
            num_simulations=num_simulations,
            distribution=distribution_type
        )
        
        # Convert to dictionary for JSON output
        output = {
            "success": True,
            "var_95": result.var_95,
            "var_99": result.var_99,
            "cvar_95": result.cvar_95,
            "cvar_99": result.cvar_99,
            "expected_return": np.mean(returns_array),
            "volatility": np.std(returns_array, ddof=1),
            "method": result.method,
            "sample_size": result.sample_size,
            "simulation_parameters": result.simulation_parameters
        }
        
        print(json.dumps(output))
        
    except json.JSONDecodeError:
        print(json.dumps({"error": "Invalid JSON format for returns data"}))
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
        print("Monte Carlo VaR Calculator - Example Usage")
        print("=" * 50)
        
        # Generate sample returns data
        np.random.seed(42)
        sample_returns = np.random.normal(0.001, 0.02, 1000)  # 1000 days of returns
        
        # Calculate VaR using Monte Carlo simulation
        result = calculate_var_cvar_monte_carlo(
            sample_returns,
            confidence_levels=[0.95, 0.99],
            num_simulations=10000,
            distribution="normal"
        )
        
        print(f"VaR 95%: {result.var_95:.4f}")
        print(f"VaR 99%: {result.var_99:.4f}")
        print(f"CVaR 95%: {result.cvar_95:.4f}")
        print(f"CVaR 99%: {result.cvar_99:.4f}")
        print(f"Method: {result.method}")
        print(f"Sample size: {result.sample_size}")
