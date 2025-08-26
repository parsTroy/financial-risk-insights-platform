using FinancialRisk.Api.Controllers;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace FinancialRisk.Tests
{
    public class HealthControllerTests
    {
        [Fact]
        public void Get_ReturnsOkWithStatus()
        {
            // Arrange
            var controller = new HealthController();

            // Act
            var result = controller.Get() as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result!.StatusCode);
            
            // Check that the result contains the expected data structure
            var response = result.Value;
            Assert.NotNull(response);
            
            // Use reflection to check properties (more reliable than dynamic)
            var statusProperty = response.GetType().GetProperty("status");
            var timestampProperty = response.GetType().GetProperty("timestamp");
            
            Assert.NotNull(statusProperty);
            Assert.NotNull(timestampProperty);
            
            var statusValue = statusProperty.GetValue(response);
            var timestampValue = timestampProperty.GetValue(response);
            
            Assert.Equal("OK", statusValue);
            Assert.NotNull(timestampValue);
        }
    }
}
