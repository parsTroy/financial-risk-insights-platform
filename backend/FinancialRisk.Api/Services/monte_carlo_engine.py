#!/usr/bin/env python3
"""
Advanced Monte Carlo Engine for Financial Risk Analysis

This module provides a comprehensive Monte Carlo simulation engine for:
- Single asset VaR calculations
- Portfolio VaR calculations
- Stress testing scenarios
- Advanced distribution modeling
- Risk attribution analysis

Author: Financial Risk Insights Platform
Version: 1.0.0
"""

import numpy as np
import pandas as pd
import scipy.stats as stats
from scipy.optimize import minimize
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

class DistributionType(Enum):
    """Supported distribution types for Monte Carlo simulation"""
    NORMAL = "normal"
    T_STUDENT = "t_student"
    GARCH = "garch"
    COPULA = "copula"
    MIXTURE = "mixture"
    CUSTOM = "custom"

class SimulationType(Enum):
    """Types of Monte Carlo simulations"""
    SINGLE_ASSET = "single_asset"
    PORTFOLIO = "portfolio"
    STRESS_TEST = "stress_test"
    BACKTEST = "backtest"

@dataclass
class SimulationParameters:
    """Parameters for Monte Carlo simulation"""
    num_simulations: int = 10000
    time_horizon: int = 1
    confidence_levels: List[float] = None
    distribution_type: DistributionType = DistributionType.NORMAL
    custom_parameters: Dict[str, Any] = None
    use_antithetic_variates: bool = False
    use_control_variates: bool = False
    use_quasi_monte_carlo: bool = False
    seed: Optional[int] = None
    
    def __post_init__(self):
        if self.confidence_levels is None:
            self.confidence_levels = [0.95, 0.99]
        if self.custom_parameters is None:
            self.custom_parameters = {}

@dataclass
class AssetParameters:
    """Parameters for a single asset"""
    symbol: str
    initial_price: float
    expected_return: float = 0.0
    volatility: float = 0.0
    historical_returns: List[float] = None
    weight: float = 1.0
    distribution_params: Dict[str, Any] = None
    
    def __post_init__(self):
        if self.historical_returns is None:
            self.historical_returns = []
        if self.distribution_params is None:
            self.distribution_params = {}

@dataclass
class PortfolioParameters:
    """Parameters for portfolio simulation"""
    assets: List[AssetParameters]
    weights: List[float]
    correlation_matrix: Optional[np.ndarray] = None
    total_value: float = 1.0
    rebalance_frequency: int = 1  # Days between rebalancing
    
    def __post_init__(self):
        if self.correlation_matrix is None and len(self.assets) > 1:
            self.correlation_matrix = self._calculate_correlation_matrix()
    
    def _calculate_correlation_matrix(self) -> np.ndarray:
        """Calculate correlation matrix from historical returns"""
        if len(self.assets) < 2:
            return np.array([[1.0]])
        
        returns_matrix = []
        min_length = min(len(asset.historical_returns) for asset in self.assets if asset.historical_returns)
        
        for asset in self.assets:
            if asset.historical_returns and len(asset.historical_returns) >= min_length:
                returns_matrix.append(asset.historical_returns[:min_length])
        
        if len(returns_matrix) < 2:
            return np.eye(len(self.assets))
        
        returns_df = pd.DataFrame(returns_matrix).T
        return returns_df.corr().values

@dataclass
class SimulationResult:
    """Results from Monte Carlo simulation"""
    simulated_returns: List[float]
    simulated_prices: List[float]
    var_values: Dict[float, float]
    cvar_values: Dict[float, float]
    expected_value: float
    standard_deviation: float
    skewness: float
    kurtosis: float
    percentiles: Dict[float, float]
    success: bool = True
    error_message: str = ""
    simulation_metadata: Dict[str, Any] = None
    
    def __post_init__(self):
        if self.simulation_metadata is None:
            self.simulation_metadata = {}

@dataclass
class PortfolioSimulationResult:
    """Results from portfolio Monte Carlo simulation"""
    portfolio_returns: List[float]
    portfolio_values: List[float]
    portfolio_var: Dict[float, float]
    portfolio_cvar: Dict[float, float]
    expected_return: float
    portfolio_volatility: float
    asset_results: List[SimulationResult]
    var_contributions: List[float]
    marginal_var: List[float]
    component_var: List[float]
    diversification_ratio: float
    success: bool = True
    error_message: str = ""
    simulation_metadata: Dict[str, Any] = None
    
    def __post_init__(self):
        if self.simulation_metadata is None:
            self.simulation_metadata = {}

class MonteCarloEngine:
    """
    Advanced Monte Carlo simulation engine for financial risk analysis
    """
    
    def __init__(self, parameters: SimulationParameters):
        self.parameters = parameters
        self.rng = self._setup_random_generator()
        self.distributions = self._setup_distributions()
        
    def _setup_random_generator(self) -> np.random.Generator:
        """Setup random number generator with specified seed"""
        if self.parameters.seed is not None:
            return np.random.default_rng(self.parameters.seed)
        return np.random.default_rng()
    
    def _setup_distributions(self) -> Dict[str, Any]:
        """Setup distribution objects for different types"""
        return {
            'normal': self._normal_distribution,
            't_student': self._t_student_distribution,
            'garch': self._garch_distribution,
            'copula': self._copula_distribution,
            'mixture': self._mixture_distribution
        }
    
    def simulate_single_asset(self, asset: AssetParameters) -> SimulationResult:
        """
        Perform Monte Carlo simulation for a single asset
        
        Args:
            asset: Asset parameters including historical data
            
        Returns:
            SimulationResult with VaR, CVaR, and other statistics
        """
        try:
            logger.info(f"Starting Monte Carlo simulation for {asset.symbol}")
            
            # Estimate parameters from historical data if available
            if asset.historical_returns:
                mean_return, volatility = self._estimate_parameters(asset.historical_returns)
            else:
                mean_return = asset.expected_return
                volatility = asset.volatility
            
            # Generate random returns
            simulated_returns = self._generate_returns(
                mean_return, volatility, asset.distribution_params
            )
            
            # Calculate simulated prices
            simulated_prices = [asset.initial_price * np.exp(ret) for ret in simulated_returns]
            
            # Calculate VaR and CVaR for different confidence levels
            var_values = {}
            cvar_values = {}
            
            for conf_level in self.parameters.confidence_levels:
                var_values[conf_level] = self._calculate_var(simulated_returns, conf_level)
                cvar_values[conf_level] = self._calculate_cvar(simulated_returns, conf_level)
            
            # Calculate additional statistics
            expected_value = np.mean(simulated_returns)
            standard_deviation = np.std(simulated_returns, ddof=1)
            skewness = stats.skew(simulated_returns)
            kurtosis = stats.kurtosis(simulated_returns)
            
            # Calculate percentiles
            percentiles = self._calculate_percentiles(simulated_returns)
            
            # Prepare metadata
            metadata = {
                'distribution_type': self.parameters.distribution_type.value,
                'num_simulations': self.parameters.num_simulations,
                'estimated_mean': mean_return,
                'estimated_volatility': volatility,
                'time_horizon': self.parameters.time_horizon
            }
            
            result = SimulationResult(
                simulated_returns=simulated_returns.tolist(),
                simulated_prices=simulated_prices,
                var_values=var_values,
                cvar_values=cvar_values,
                expected_value=float(expected_value),
                standard_deviation=float(standard_deviation),
                skewness=float(skewness),
                kurtosis=float(kurtosis),
                percentiles=percentiles,
                simulation_metadata=metadata
            )
            
            logger.info(f"Monte Carlo simulation completed for {asset.symbol}")
            return result
            
        except Exception as e:
            logger.error(f"Error in Monte Carlo simulation for {asset.symbol}: {str(e)}")
            return SimulationResult(
                simulated_returns=[],
                simulated_prices=[],
                var_values={},
                cvar_values={},
                expected_value=0.0,
                standard_deviation=0.0,
                skewness=0.0,
                kurtosis=0.0,
                percentiles={},
                success=False,
                error_message=str(e)
            )
    
    def simulate_portfolio(self, portfolio: PortfolioParameters) -> PortfolioSimulationResult:
        """
        Perform Monte Carlo simulation for a portfolio
        
        Args:
            portfolio: Portfolio parameters including assets and weights
            
        Returns:
            PortfolioSimulationResult with portfolio VaR, CVaR, and attribution
        """
        try:
            logger.info(f"Starting portfolio Monte Carlo simulation with {len(portfolio.assets)} assets")
            
            # Normalize weights
            total_weight = sum(portfolio.weights)
            normalized_weights = np.array(portfolio.weights) / total_weight
            
            # Simulate each asset
            asset_results = []
            for asset in portfolio.assets:
                asset_result = self.simulate_single_asset(asset)
                asset_results.append(asset_result)
            
            # Generate correlated returns
            if len(portfolio.assets) > 1 and portfolio.correlation_matrix is not None:
                correlated_returns = self._generate_correlated_returns(
                    asset_results, portfolio.correlation_matrix
                )
            else:
                # Use independent returns
                correlated_returns = [result.simulated_returns for result in asset_results]
            
            # Calculate portfolio returns
            portfolio_returns = []
            portfolio_values = []
            
            for i in range(self.parameters.num_simulations):
                portfolio_return = 0.0
                portfolio_value = 0.0
                
                for j, asset in enumerate(portfolio.assets):
                    asset_return = correlated_returns[j][i]
                    weight = normalized_weights[j]
                    asset_value = asset.initial_price * np.exp(asset_return)
                    
                    portfolio_return += weight * asset_return
                    portfolio_value += weight * asset_value
                
                portfolio_returns.append(portfolio_return)
                portfolio_values.append(portfolio_value)
            
            portfolio_returns = np.array(portfolio_returns)
            
            # Calculate portfolio VaR and CVaR
            portfolio_var = {}
            portfolio_cvar = {}
            
            for conf_level in self.parameters.confidence_levels:
                portfolio_var[conf_level] = self._calculate_var(portfolio_returns, conf_level)
                portfolio_cvar[conf_level] = self._calculate_cvar(portfolio_returns, conf_level)
            
            # Calculate portfolio statistics
            expected_return = np.mean(portfolio_returns)
            portfolio_volatility = np.std(portfolio_returns, ddof=1)
            
            # Calculate risk attribution
            var_contributions = []
            marginal_var = []
            component_var = []
            
            for i, asset in enumerate(portfolio.assets):
                weight = normalized_weights[i]
                asset_var = asset_results[i].var_values.get(0.95, 0.0)
                
                var_contributions.append(weight * asset_var)
                marginal_var.append(asset_var)
                component_var.append(weight * asset_var)
            
            # Calculate diversification ratio
            undiversified_var = sum(var_contributions)
            diversified_var = portfolio_var.get(0.95, 0.0)
            diversification_ratio = undiversified_var / diversified_var if diversified_var > 0 else 1.0
            
            # Prepare metadata
            metadata = {
                'num_assets': len(portfolio.assets),
                'portfolio_weights': normalized_weights.tolist(),
                'diversification_ratio': diversification_ratio,
                'correlation_used': portfolio.correlation_matrix is not None
            }
            
            result = PortfolioSimulationResult(
                portfolio_returns=portfolio_returns.tolist(),
                portfolio_values=portfolio_values,
                portfolio_var=portfolio_var,
                portfolio_cvar=portfolio_cvar,
                expected_return=float(expected_return),
                portfolio_volatility=float(portfolio_volatility),
                asset_results=asset_results,
                var_contributions=var_contributions,
                marginal_var=marginal_var,
                component_var=component_var,
                diversification_ratio=diversification_ratio,
                simulation_metadata=metadata
            )
            
            logger.info("Portfolio Monte Carlo simulation completed")
            return result
            
        except Exception as e:
            logger.error(f"Error in portfolio Monte Carlo simulation: {str(e)}")
            return PortfolioSimulationResult(
                portfolio_returns=[],
                portfolio_values=[],
                portfolio_var={},
                portfolio_cvar={},
                expected_return=0.0,
                portfolio_volatility=0.0,
                asset_results=[],
                var_contributions=[],
                marginal_var=[],
                component_var=[],
                diversification_ratio=1.0,
                success=False,
                error_message=str(e)
            )
    
    def perform_stress_test(self, asset: AssetParameters, 
                          stress_scenarios: Dict[str, Dict[str, float]]) -> Dict[str, SimulationResult]:
        """
        Perform stress testing scenarios
        
        Args:
            asset: Base asset parameters
            stress_scenarios: Dictionary of stress scenarios with parameters
            
        Returns:
            Dictionary of simulation results for each stress scenario
        """
        results = {}
        
        for scenario_name, stress_params in stress_scenarios.items():
            logger.info(f"Running stress test scenario: {scenario_name}")
            
            # Create stressed asset parameters
            stressed_asset = AssetParameters(
                symbol=asset.symbol,
                initial_price=asset.initial_price,
                expected_return=asset.expected_return * stress_params.get('return_multiplier', 1.0),
                volatility=asset.volatility * stress_params.get('volatility_multiplier', 1.0),
                historical_returns=asset.historical_returns,
                weight=asset.weight,
                distribution_params=asset.distribution_params
            )
            
            # Run simulation
            result = self.simulate_single_asset(stressed_asset)
            result.simulation_metadata['stress_scenario'] = scenario_name
            result.simulation_metadata['stress_parameters'] = stress_params
            
            results[scenario_name] = result
        
        return results
    
    def _estimate_parameters(self, returns: List[float]) -> Tuple[float, float]:
        """Estimate mean and volatility from historical returns"""
        returns_array = np.array(returns)
        mean_return = np.mean(returns_array)
        volatility = np.std(returns_array, ddof=1)
        return mean_return, volatility
    
    def _generate_returns(self, mean: float, volatility: float, 
                         distribution_params: Dict[str, Any]) -> np.ndarray:
        """Generate random returns based on specified distribution"""
        distribution_func = self.distributions.get(
            self.parameters.distribution_type.value, 
            self._normal_distribution
        )
        
        return distribution_func(mean, volatility, distribution_params)
    
    def _normal_distribution(self, mean: float, volatility: float, 
                           params: Dict[str, Any]) -> np.ndarray:
        """Generate returns from normal distribution"""
        return self.rng.normal(mean, volatility, self.parameters.num_simulations)
    
    def _t_student_distribution(self, mean: float, volatility: float, 
                              params: Dict[str, Any]) -> np.ndarray:
        """Generate returns from Student's t-distribution"""
        df = params.get('degrees_of_freedom', 5.0)
        t_samples = self.rng.standard_t(df, self.parameters.num_simulations)
        return mean + volatility * t_samples
    
    def _garch_distribution(self, mean: float, volatility: float, 
                          params: Dict[str, Any]) -> np.ndarray:
        """Generate returns from GARCH process"""
        omega = params.get('omega', 0.0001)
        alpha = params.get('alpha', 0.1)
        beta = params.get('beta', 0.85)
        
        returns = np.zeros(self.parameters.num_simulations)
        variance = volatility ** 2
        
        for i in range(self.parameters.num_simulations):
            # Generate return
            z = self.rng.standard_normal()
            returns[i] = mean + np.sqrt(variance) * z
            
            # Update variance (GARCH(1,1))
            variance = omega + alpha * returns[i] ** 2 + beta * variance
        
        return returns
    
    def _copula_distribution(self, mean: float, volatility: float, 
                           params: Dict[str, Any]) -> np.ndarray:
        """Generate returns using copula (simplified implementation)"""
        # This is a simplified implementation
        # In practice, you would use proper copula modeling
        return self._normal_distribution(mean, volatility, params)
    
    def _mixture_distribution(self, mean: float, volatility: float, 
                            params: Dict[str, Any]) -> np.ndarray:
        """Generate returns from mixture of distributions"""
        mixture_weights = params.get('weights', [0.7, 0.3])
        mixture_means = params.get('means', [mean, mean * 0.5])
        mixture_vols = params.get('volatilities', [volatility, volatility * 1.5])
        
        returns = np.zeros(self.parameters.num_simulations)
        
        for i in range(self.parameters.num_simulations):
            # Choose distribution component
            component = self.rng.choice(len(mixture_weights), p=mixture_weights)
            returns[i] = self.rng.normal(mixture_means[component], mixture_vols[component])
        
        return returns
    
    def _generate_correlated_returns(self, asset_results: List[SimulationResult], 
                                   correlation_matrix: np.ndarray) -> List[List[float]]:
        """Generate correlated returns using Cholesky decomposition"""
        try:
            # Perform Cholesky decomposition
            L = cholesky(correlation_matrix, lower=True)
            
            # Generate independent standard normal variables
            n_assets = len(asset_results)
            independent_samples = self.rng.standard_normal((n_assets, self.parameters.num_simulations))
            
            # Apply Cholesky transformation
            correlated_samples = L @ independent_samples
            
            # Convert to returns
            correlated_returns = []
            for i, asset_result in enumerate(asset_results):
                # Scale by asset-specific volatility
                asset_vol = asset_result.standard_deviation
                asset_mean = asset_result.expected_value
                
                returns = asset_mean + asset_vol * correlated_samples[i]
                correlated_returns.append(returns.tolist())
            
            return correlated_returns
            
        except LinAlgError:
            logger.warning("Cholesky decomposition failed, using independent returns")
            return [result.simulated_returns for result in asset_results]
    
    def _calculate_var(self, returns: np.ndarray, confidence_level: float) -> float:
        """Calculate Value at Risk"""
        return -np.percentile(returns, (1 - confidence_level) * 100)
    
    def _calculate_cvar(self, returns: np.ndarray, confidence_level: float) -> float:
        """Calculate Conditional Value at Risk (Expected Shortfall)"""
        var_threshold = -self._calculate_var(returns, confidence_level)
        tail_returns = returns[returns <= var_threshold]
        return -np.mean(tail_returns) if len(tail_returns) > 0 else 0.0
    
    def _calculate_percentiles(self, returns: np.ndarray) -> Dict[float, float]:
        """Calculate various percentiles"""
        percentiles = [1, 5, 10, 25, 50, 75, 90, 95, 99]
        return {p/100: float(np.percentile(returns, p)) for p in percentiles}

def run_monte_carlo_simulation(symbol: str, returns_data: List[float], 
                             distribution_type: str = "normal", 
                             num_simulations: int = 10000,
                             confidence_levels: List[float] = None) -> Dict[str, Any]:
    """
    Main function for running Monte Carlo simulation from C# API
    
    Args:
        symbol: Asset symbol
        returns_data: Historical returns data
        distribution_type: Type of distribution to use
        num_simulations: Number of simulation runs
        confidence_levels: List of confidence levels for VaR calculation
        
    Returns:
        Dictionary containing simulation results
    """
    try:
        if confidence_levels is None:
            confidence_levels = [0.95, 0.99]
        
        # Create simulation parameters
        params = SimulationParameters(
            num_simulations=num_simulations,
            confidence_levels=confidence_levels,
            distribution_type=DistributionType(distribution_type.lower())
        )
        
        # Create asset parameters
        asset = AssetParameters(
            symbol=symbol,
            initial_price=100.0,  # Default initial price
            historical_returns=returns_data
        )
        
        # Run simulation
        engine = MonteCarloEngine(params)
        result = engine.simulate_single_asset(asset)
        
        # Convert result to dictionary for JSON serialization
        result_dict = asdict(result)
        
        # Add additional metrics
        result_dict['var_95'] = result.var_values.get(0.95, 0.0)
        result_dict['var_99'] = result.var_values.get(0.99, 0.0)
        result_dict['cvar_95'] = result.cvar_values.get(0.95, 0.0)
        result_dict['cvar_99'] = result.cvar_values.get(0.99, 0.0)
        
        return result_dict
        
    except Exception as e:
        logger.error(f"Error in Monte Carlo simulation: {str(e)}")
        return {
            'success': False,
            'error': str(e),
            'var_95': 0.0,
            'var_99': 0.0,
            'cvar_95': 0.0,
            'cvar_99': 0.0
        }

def run_portfolio_monte_carlo_simulation(portfolio_data: Dict[str, Any]) -> Dict[str, Any]:
    """
    Main function for running portfolio Monte Carlo simulation from C# API
    
    Args:
        portfolio_data: Dictionary containing portfolio information
        
    Returns:
        Dictionary containing portfolio simulation results
    """
    try:
        # Extract portfolio information
        assets_data = portfolio_data.get('assets', [])
        weights = portfolio_data.get('weights', [])
        num_simulations = portfolio_data.get('num_simulations', 10000)
        confidence_levels = portfolio_data.get('confidence_levels', [0.95, 0.99])
        distribution_type = portfolio_data.get('distribution_type', 'normal')
        
        # Create asset parameters
        assets = []
        for asset_data in assets_data:
            asset = AssetParameters(
                symbol=asset_data['symbol'],
                initial_price=asset_data.get('initial_price', 100.0),
                historical_returns=asset_data.get('returns', [])
            )
            assets.append(asset)
        
        # Create portfolio parameters
        portfolio = PortfolioParameters(
            assets=assets,
            weights=weights
        )
        
        # Create simulation parameters
        params = SimulationParameters(
            num_simulations=num_simulations,
            confidence_levels=confidence_levels,
            distribution_type=DistributionType(distribution_type.lower())
        )
        
        # Run simulation
        engine = MonteCarloEngine(params)
        result = engine.simulate_portfolio(portfolio)
        
        # Convert result to dictionary for JSON serialization
        result_dict = asdict(result)
        
        # Add portfolio-level metrics
        result_dict['portfolio_var_95'] = result.portfolio_var.get(0.95, 0.0)
        result_dict['portfolio_var_99'] = result.portfolio_var.get(0.99, 0.0)
        result_dict['portfolio_cvar_95'] = result.portfolio_cvar.get(0.95, 0.0)
        result_dict['portfolio_cvar_99'] = result.portfolio_cvar.get(0.99, 0.0)
        
        return result_dict
        
    except Exception as e:
        logger.error(f"Error in portfolio Monte Carlo simulation: {str(e)}")
        return {
            'success': False,
            'error': str(e),
            'portfolio_var_95': 0.0,
            'portfolio_var_99': 0.0,
            'portfolio_cvar_95': 0.0,
            'portfolio_cvar_99': 0.0
        }

if __name__ == "__main__":
    # Example usage
    import sys
    
    if len(sys.argv) > 1:
        # Command line interface for C# integration
        symbol = sys.argv[1]
        returns_json = sys.argv[2]
        distribution_type = sys.argv[3] if len(sys.argv) > 3 else "normal"
        num_simulations = int(sys.argv[4]) if len(sys.argv) > 4 else 10000
        
        try:
            returns_data = json.loads(returns_json)
            result = run_monte_carlo_simulation(symbol, returns_data, distribution_type, num_simulations)
            print(json.dumps(result))
        except Exception as e:
            error_result = {
                'success': False,
                'error': str(e),
                'var_95': 0.0,
                'var_99': 0.0,
                'cvar_95': 0.0,
                'cvar_99': 0.0
            }
            print(json.dumps(error_result))
    else:
        # Demo mode
        print("Monte Carlo Engine Demo")
        print("======================")
        
        # Generate sample data
        np.random.seed(42)
        sample_returns = np.random.normal(0.001, 0.02, 252).tolist()
        
        # Run simulation
        result = run_monte_carlo_simulation("AAPL", sample_returns, "normal", 10000)
        
        print(f"VaR 95%: {result['var_95']:.4f}")
        print(f"VaR 99%: {result['var_99']:.4f}")
        print(f"CVaR 95%: {result['cvar_95']:.4f}")
        print(f"CVaR 99%: {result['cvar_99']:.4f}")
        print(f"Expected Return: {result['expected_value']:.4f}")
        print(f"Volatility: {result['standard_deviation']:.4f}")
