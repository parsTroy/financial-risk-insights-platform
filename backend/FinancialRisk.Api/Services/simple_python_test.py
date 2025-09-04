#!/usr/bin/env python3
"""
Simple Python test for VaR calculations without external dependencies
"""

import math
import random
from typing import List, Tuple

def calculate_simple_var(returns: List[float], confidence_level: float) -> float:
    """Simple VaR calculation using percentile method"""
    if not returns:
        return 0.0
    
    sorted_returns = sorted(returns)
    index = int((1 - confidence_level) * len(sorted_returns))
    if index >= len(sorted_returns):
        index = len(sorted_returns) - 1
    if index < 0:
        index = 0
    
    return -sorted_returns[index]

def calculate_simple_cvar(returns: List[float], confidence_level: float) -> float:
    """Simple CVaR calculation using percentile method"""
    if not returns:
        return 0.0
    
    sorted_returns = sorted(returns)
    tail_count = int((1 - confidence_level) * len(sorted_returns))
    if tail_count <= 0:
        tail_count = 1
    if tail_count > len(sorted_returns):
        tail_count = len(sorted_returns)
    
    tail_returns = sorted_returns[:tail_count]
    return -sum(tail_returns) / len(tail_returns)

def monte_carlo_var(returns: List[float], confidence_level: float, num_simulations: int = 1000) -> Tuple[float, float]:
    """Simple Monte Carlo VaR simulation"""
    if not returns:
        return 0.0, 0.0
    
    # Calculate mean and standard deviation
    mean = sum(returns) / len(returns)
    variance = sum((x - mean) ** 2 for x in returns) / (len(returns) - 1)
    std_dev = math.sqrt(variance)
    
    # Generate random samples
    simulated_returns = []
    for _ in range(num_simulations):
        # Simple normal distribution approximation
        sample = mean + std_dev * (sum(random.random() for _ in range(12)) - 6)
        simulated_returns.append(sample)
    
    # Calculate VaR and CVaR
    var = calculate_simple_var(simulated_returns, confidence_level)
    cvar = calculate_simple_cvar(simulated_returns, confidence_level)
    
    return var, cvar

def main():
    print("🐍 Simple Python VaR Calculator Test")
    print("=" * 40)
    
    # Generate sample returns data
    random.seed(42)
    sample_returns = [random.gauss(0.001, 0.02) for _ in range(1000)]
    
    print(f"Generated {len(sample_returns)} sample returns")
    print(f"Mean return: {sum(sample_returns) / len(sample_returns):.6f}")
    print(f"Standard deviation: {math.sqrt(sum((x - sum(sample_returns) / len(sample_returns)) ** 2 for x in sample_returns) / (len(sample_returns) - 1)):.6f}")
    print()
    
    # Test VaR calculations
    confidence_levels = [0.90, 0.95, 0.99]
    
    for conf_level in confidence_levels:
        var = calculate_simple_var(sample_returns, conf_level)
        cvar = calculate_simple_cvar(sample_returns, conf_level)
        
        print(f"VaR {int(conf_level * 100)}%: {var:.6f}")
        print(f"CVaR {int(conf_level * 100)}%: {cvar:.6f}")
        print()
    
    # Test Monte Carlo simulation
    print("Monte Carlo VaR Simulation:")
    print("-" * 30)
    
    for conf_level in confidence_levels:
        var, cvar = monte_carlo_var(sample_returns, conf_level, 10000)
        print(f"MC VaR {int(conf_level * 100)}%: {var:.6f}")
        print(f"MC CVaR {int(conf_level * 100)}%: {cvar:.6f}")
        print()
    
    print("✅ Python VaR calculations completed successfully!")
    print("Ready for integration with C# backend! 🚀")

if __name__ == "__main__":
    main()
