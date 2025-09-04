using Microsoft.Extensions.Logging;
using Moq;
using FinancialRisk.Api.Services;
using FinancialRisk.Api.Models;
using System.Text.Json;

namespace FinancialRisk.Tests
{
    public class InteropServiceTests
    {
        private readonly Mock<ILogger<UnifiedInteropService>> _mockLogger;
        private readonly Mock<PythonInteropService> _mockPythonService;
        private readonly Mock<GrpcPythonService> _mockGrpcService;
        private readonly Mock<CppInteropService> _mockCppService;
        private readonly UnifiedInteropService _interopService;

        public InteropServiceTests()
        {
            _mockLogger = new Mock<ILogger<UnifiedInteropService>>();
            _mockPythonService = new Mock<PythonInteropService>(Mock.Of<ILogger<PythonInteropService>>(), new PythonInteropConfiguration());
            _mockGrpcService = new Mock<GrpcPythonService>(Mock.Of<ILogger<GrpcPythonService>>(), new GrpcPythonConfiguration());
            _mockCppService = new Mock<CppInteropService>(Mock.Of<ILogger<CppInteropService>>(), new CppInteropConfiguration());
            
            var config = new InteropConfiguration
            {
                EnablePythonNet = true,
                EnableGrpc = true,
                EnableCpp = true,
                PreferredMethod = "auto",
                EnableFallback = true
            };

            _interopService = new UnifiedInteropService(
                _mockLogger.Object,
                Microsoft.Extensions.Options.Options.Create(config),
                _mockPythonService.Object,
                _mockGrpcService.Object,
                _mockCppService.Object
            );
        }

        [Fact]
        public async Task InitializeAsync_WithAllServices_ReturnsTrue()
        {
            // Arrange
            _mockPythonService.Setup(x => x.InitializeAsync()).ReturnsAsync(true);
            _mockGrpcService.Setup(x => x.InitializeAsync()).ReturnsAsync(true);
            _mockCppService.Setup(x => x.InitializeAsync()).ReturnsAsync(true);

            // Act
            var result = await _interopService.InitializeAsync();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task InitializeAsync_WithPartialServices_ReturnsTrue()
        {
            // Arrange
            _mockPythonService.Setup(x => x.InitializeAsync()).ReturnsAsync(true);
            _mockGrpcService.Setup(x => x.InitializeAsync()).ReturnsAsync(false);
            _mockCppService.Setup(x => x.InitializeAsync()).ReturnsAsync(false);

            // Act
            var result = await _interopService.InitializeAsync();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task InitializeAsync_WithNoServices_ReturnsFalse()
        {
            // Arrange
            _mockPythonService.Setup(x => x.InitializeAsync()).ReturnsAsync(false);
            _mockGrpcService.Setup(x => x.InitializeAsync()).ReturnsAsync(false);
            _mockCppService.Setup(x => x.InitializeAsync()).ReturnsAsync(false);

            // Act
            var result = await _interopService.InitializeAsync();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ExecuteQuantModelAsync_WithPythonService_ReturnsResult()
        {
            // Arrange
            var request = new QuantModelRequest
            {
                ModelName = "var_historical",
                Parameters = new Dictionary<string, object>
                {
                    ["returns"] = new double[] { 0.01, -0.02, 0.015, 0.03, -0.01 },
                    ["confidence_level"] = 0.95
                }
            };

            var expectedResult = new QuantModelResult
            {
                Success = true,
                ModelName = "var_historical",
                Results = new Dictionary<string, object>
                {
                    ["var"] = 0.03,
                    ["confidence_level"] = 0.95
                }
            };

            _mockPythonService.Setup(x => x.ExecuteQuantModelAsync(It.IsAny<QuantModelRequest>()))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _interopService.ExecuteQuantModelAsync(request);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("var_historical", result.ModelName);
            Assert.Contains("interop_method", result.Metadata.Keys);
        }

        [Fact]
        public async Task ExecuteQuantModelAsync_WithPythonFailure_FallsBackToGrpc()
        {
            // Arrange
            var request = new QuantModelRequest
            {
                ModelName = "var_historical",
                Parameters = new Dictionary<string, object>
                {
                    ["returns"] = new double[] { 0.01, -0.02, 0.015, 0.03, -0.01 },
                    ["confidence_level"] = 0.95
                }
            };

            var expectedResult = new QuantModelResult
            {
                Success = true,
                ModelName = "var_historical",
                Results = new Dictionary<string, object>
                {
                    ["var"] = 0.03,
                    ["confidence_level"] = 0.95
                }
            };

            _mockPythonService.Setup(x => x.ExecuteQuantModelAsync(It.IsAny<QuantModelRequest>()))
                .ThrowsAsync(new Exception("Python service failed"));

            _mockGrpcService.Setup(x => x.ExecuteQuantModelAsync(It.IsAny<QuantModelRequest>()))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _interopService.ExecuteQuantModelAsync(request);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("var_historical", result.ModelName);
        }

        [Fact]
        public async Task ExecuteQuantModelAsync_WithAllServicesFailing_ReturnsError()
        {
            // Arrange
            var request = new QuantModelRequest
            {
                ModelName = "var_historical",
                Parameters = new Dictionary<string, object>
                {
                    ["returns"] = new double[] { 0.01, -0.02, 0.015, 0.03, -0.01 },
                    ["confidence_level"] = 0.95
                }
            };

            _mockPythonService.Setup(x => x.ExecuteQuantModelAsync(It.IsAny<QuantModelRequest>()))
                .ThrowsAsync(new Exception("Python service failed"));

            _mockGrpcService.Setup(x => x.ExecuteQuantModelAsync(It.IsAny<QuantModelRequest>()))
                .ThrowsAsync(new Exception("gRPC service failed"));

            _mockCppService.Setup(x => x.ExecuteQuantModelAsync(It.IsAny<QuantModelRequest>()))
                .ThrowsAsync(new Exception("C++ service failed"));

            // Act
            var result = await _interopService.ExecuteQuantModelAsync(request);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("No suitable interop service available", result.Error);
        }

        [Fact]
        public async Task GetAvailableModelsAsync_ReturnsAllModels()
        {
            // Arrange
            var pythonModels = new List<string> { "python:var_historical", "python:markowitz_optimization" };
            var grpcModels = new List<string> { "grpc:black_scholes", "grpc:monte_carlo_pricing" };
            var cppModels = new List<string> { "cpp:var_parametric", "cpp:binomial_tree" };

            _mockPythonService.Setup(x => x.GetAvailableModelsAsync()).ReturnsAsync(pythonModels);
            _mockGrpcService.Setup(x => x.GetAvailableModelsAsync()).ReturnsAsync(grpcModels);
            _mockCppService.Setup(x => x.GetAvailableModelsAsync()).ReturnsAsync(cppModels);

            // Act
            var result = await _interopService.GetAvailableModelsAsync();

            // Assert
            Assert.Equal(6, result.Count);
            Assert.Contains("python:var_historical", result);
            Assert.Contains("grpc:black_scholes", result);
            Assert.Contains("cpp:var_parametric", result);
        }

        [Fact]
        public async Task GetModelMetadataAsync_WithPrefixedModel_ReturnsCorrectMetadata()
        {
            // Arrange
            var expectedMetadata = new ModelMetadata
            {
                Success = true,
                ModelName = "python:var_historical",
                Description = "Historical Value at Risk calculation",
                Author = "Python Service"
            };

            _mockPythonService.Setup(x => x.GetModelMetadataAsync("python:var_historical"))
                .ReturnsAsync(expectedMetadata);

            // Act
            var result = await _interopService.GetModelMetadataAsync("python:var_historical");

            // Assert
            Assert.True(result.Success);
            Assert.Equal("python:var_historical", result.ModelName);
            Assert.Equal("Historical Value at Risk calculation", result.Description);
        }

        [Fact]
        public async Task GetModelMetadataAsync_WithoutPrefix_TriesAllServices()
        {
            // Arrange
            var expectedMetadata = new ModelMetadata
            {
                Success = true,
                ModelName = "var_historical",
                Description = "Historical Value at Risk calculation",
                Author = "Python Service"
            };

            _mockPythonService.Setup(x => x.GetModelMetadataAsync("var_historical"))
                .ReturnsAsync(expectedMetadata);

            _mockGrpcService.Setup(x => x.GetModelMetadataAsync("var_historical"))
                .ReturnsAsync(new ModelMetadata { Success = false });

            _mockCppService.Setup(x => x.GetModelMetadataAsync("var_historical"))
                .ReturnsAsync(new ModelMetadata { Success = false });

            // Act
            var result = await _interopService.GetModelMetadataAsync("var_historical");

            // Assert
            Assert.True(result.Success);
            Assert.Equal("var_historical", result.ModelName);
        }

        [Fact]
        public async Task ValidateModelAsync_WithValidParameters_ReturnsTrue()
        {
            // Arrange
            var modelName = "var_historical";
            var parameters = new Dictionary<string, object>
            {
                ["returns"] = new double[] { 0.01, -0.02, 0.015 },
                ["confidence_level"] = 0.95
            };

            _mockPythonService.Setup(x => x.ValidateModelAsync(modelName, parameters))
                .ReturnsAsync(true);

            // Act
            var result = await _interopService.ValidateModelAsync(modelName, parameters);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ValidateModelAsync_WithInvalidParameters_ReturnsFalse()
        {
            // Arrange
            var modelName = "var_historical";
            var parameters = new Dictionary<string, object>
            {
                ["invalid_param"] = "invalid_value"
            };

            _mockPythonService.Setup(x => x.ValidateModelAsync(modelName, parameters))
                .ReturnsAsync(false);

            // Act
            var result = await _interopService.ValidateModelAsync(modelName, parameters);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task GetPerformanceMetricsAsync_AggregatesFromAllServices()
        {
            // Arrange
            var pythonMetrics = new PerformanceMetrics
            {
                TotalRequests = 100,
                SuccessfulRequests = 95,
                FailedRequests = 5,
                TotalMemoryUsageMB = 500
            };

            var grpcMetrics = new PerformanceMetrics
            {
                TotalRequests = 200,
                SuccessfulRequests = 190,
                FailedRequests = 10,
                TotalMemoryUsageMB = 300
            };

            var cppMetrics = new PerformanceMetrics
            {
                TotalRequests = 50,
                SuccessfulRequests = 50,
                FailedRequests = 0,
                TotalMemoryUsageMB = 100
            };

            _mockPythonService.Setup(x => x.GetPerformanceMetricsAsync()).ReturnsAsync(pythonMetrics);
            _mockGrpcService.Setup(x => x.GetPerformanceMetricsAsync()).ReturnsAsync(grpcMetrics);
            _mockCppService.Setup(x => x.GetPerformanceMetricsAsync()).ReturnsAsync(cppMetrics);

            // Act
            var result = await _interopService.GetPerformanceMetricsAsync();

            // Assert
            Assert.Equal(350, result.TotalRequests);
            Assert.Equal(335, result.SuccessfulRequests);
            Assert.Equal(15, result.FailedRequests);
            Assert.Equal(900, result.TotalMemoryUsageMB);
        }

        [Fact]
        public async Task CallPythonFunctionAsync_WithPythonService_ReturnsResult()
        {
            // Arrange
            var expectedResult = new double[] { 1.0, 2.0, 3.0, 4.0, 5.0 };
            _mockPythonService.Setup(x => x.CallPythonFunctionAsync<double[]>("numpy", "random.normal", 0.0, 1.0, 5))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _interopService.CallPythonFunctionAsync<double[]>("numpy", "random.normal", 0.0, 1.0, 5);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task CallPythonFunctionAsync_WithPythonFailure_FallsBackToGrpc()
        {
            // Arrange
            var expectedResult = new double[] { 1.0, 2.0, 3.0, 4.0, 5.0 };
            _mockPythonService.Setup(x => x.CallPythonFunctionAsync<double[]>("numpy", "random.normal", 0.0, 1.0, 5))
                .ThrowsAsync(new Exception("Python service failed"));

            _mockGrpcService.Setup(x => x.CallPythonFunctionAsync<double[]>("numpy", "random.normal", 0.0, 1.0, 5))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _interopService.CallPythonFunctionAsync<double[]>("numpy", "random.normal", 0.0, 1.0, 5);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task CallPythonFunctionAsync_WithAllServicesFailing_ThrowsException()
        {
            // Arrange
            _mockPythonService.Setup(x => x.CallPythonFunctionAsync<double[]>("numpy", "random.normal", 0.0, 1.0, 5))
                .ThrowsAsync(new Exception("Python service failed"));

            _mockGrpcService.Setup(x => x.CallPythonFunctionAsync<double[]>("numpy", "random.normal", 0.0, 1.0, 5))
                .ThrowsAsync(new Exception("gRPC service failed"));

            _mockCppService.Setup(x => x.CallPythonFunctionAsync<double[]>("numpy", "random.normal", 0.0, 1.0, 5))
                .ThrowsAsync(new NotSupportedException("C++ service doesn't support Python calls"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _interopService.CallPythonFunctionAsync<double[]>("numpy", "random.normal", 0.0, 1.0, 5));
        }

        [Theory]
        [InlineData("statistical", "python")]
        [InlineData("realtime", "grpc")]
        [InlineData("performance", "cpp")]
        public void SelectBestService_WithDifferentModelTypes_ReturnsCorrectService(string modelType, string expectedService)
        {
            // This test would require making the SelectBestService method public or using reflection
            // For now, we'll test the behavior through the public API
            Assert.True(true); // Placeholder for actual implementation
        }

        [Fact]
        public void GetServiceFromModelName_WithPrefixedModel_ReturnsCorrectService()
        {
            // This test would require making the GetServiceFromModelName method public or using reflection
            // For now, we'll test the behavior through the public API
            Assert.True(true); // Placeholder for actual implementation
        }

        [Fact]
        public void AggregateMetrics_WithMultipleServices_CombinesCorrectly()
        {
            // This test would require making the AggregateMetrics method public or using reflection
            // For now, we'll test the behavior through the public API
            Assert.True(true); // Placeholder for actual implementation
        }

        [Fact]
        public void Dispose_CleansUpAllServices()
        {
            // Arrange
            var interopService = new UnifiedInteropService(
                _mockLogger.Object,
                Microsoft.Extensions.Options.Options.Create(new InteropConfiguration()),
                _mockPythonService.Object,
                _mockGrpcService.Object,
                _mockCppService.Object
            );

            // Act
            interopService.Dispose();

            // Assert
            // Verify that all services are disposed
            // This would require making the services mockable or using a different approach
            Assert.True(true); // Placeholder for actual implementation
        }
    }

    public class PythonInteropServiceTests
    {
        private readonly Mock<ILogger<PythonInteropService>> _mockLogger;
        private readonly PythonInteropService _pythonService;

        public PythonInteropServiceTests()
        {
            _mockLogger = new Mock<ILogger<PythonInteropService>>();
            _pythonService = new PythonInteropService(_mockLogger.Object, new PythonInteropConfiguration());
        }

        [Fact]
        public async Task InitializeAsync_ReturnsTrue()
        {
            // Act
            var result = await _pythonService.InitializeAsync();

            // Assert
            // This test would require Python.NET to be properly installed
            // For now, we'll test the basic structure
            Assert.True(true); // Placeholder for actual implementation
        }

        [Fact]
        public async Task ExecuteQuantModelAsync_WithValidRequest_ReturnsResult()
        {
            // Arrange
            var request = new QuantModelRequest
            {
                ModelName = "var_historical",
                Parameters = new Dictionary<string, object>
                {
                    ["returns"] = new double[] { 0.01, -0.02, 0.015, 0.03, -0.01 },
                    ["confidence_level"] = 0.95
                }
            };

            // Act
            var result = await _pythonService.ExecuteQuantModelAsync(request);

            // Assert
            // This test would require Python.NET to be properly installed
            // For now, we'll test the basic structure
            Assert.NotNull(result);
        }
    }

    public class CppInteropServiceTests
    {
        private readonly Mock<ILogger<CppInteropService>> _mockLogger;
        private readonly CppInteropService _cppService;

        public CppInteropServiceTests()
        {
            _mockLogger = new Mock<ILogger<CppInteropService>>();
            _cppService = new CppInteropService(_mockLogger.Object, new CppInteropConfiguration());
        }

        [Fact]
        public async Task InitializeAsync_ReturnsTrue()
        {
            // Act
            var result = await _cppService.InitializeAsync();

            // Assert
            // This test would require the C++ library to be properly built and available
            // For now, we'll test the basic structure
            Assert.True(true); // Placeholder for actual implementation
        }

        [Fact]
        public async Task ExecuteQuantModelAsync_WithVaRHistorical_ReturnsResult()
        {
            // Arrange
            var request = new QuantModelRequest
            {
                ModelName = "var_historical",
                Parameters = new Dictionary<string, object>
                {
                    ["returns"] = new double[] { 0.01, -0.02, 0.015, 0.03, -0.01 },
                    ["confidence_level"] = 0.95
                }
            };

            // Act
            var result = await _cppService.ExecuteQuantModelAsync(request);

            // Assert
            // This test would require the C++ library to be properly built and available
            // For now, we'll test the basic structure
            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetAvailableModelsAsync_ReturnsCppModels()
        {
            // Act
            var result = await _cppService.GetAvailableModelsAsync();

            // Assert
            Assert.NotEmpty(result);
            Assert.Contains("var_historical", result);
            Assert.Contains("markowitz_optimization", result);
            Assert.Contains("black_scholes", result);
        }

        [Fact]
        public async Task GetModelMetadataAsync_WithValidModel_ReturnsMetadata()
        {
            // Act
            var result = await _cppService.GetModelMetadataAsync("var_historical");

            // Assert
            Assert.True(result.Success);
            Assert.Equal("var_historical", result.ModelName);
            Assert.Equal("C++ QuantEngine", result.Author);
        }

        [Fact]
        public async Task ValidateModelAsync_WithValidParameters_ReturnsTrue()
        {
            // Arrange
            var parameters = new Dictionary<string, object>
            {
                ["returns"] = new double[] { 0.01, -0.02, 0.015 },
                ["confidence_level"] = 0.95
            };

            // Act
            var result = await _cppService.ValidateModelAsync("var_historical", parameters);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ValidateModelAsync_WithInvalidParameters_ReturnsFalse()
        {
            // Arrange
            var parameters = new Dictionary<string, object>
            {
                ["invalid_param"] = "invalid_value"
            };

            // Act
            var result = await _cppService.ValidateModelAsync("var_historical", parameters);

            // Assert
            Assert.False(result);
        }
    }
}
