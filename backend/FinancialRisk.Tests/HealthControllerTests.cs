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
            dynamic value = result!.Value!;
            Assert.Equal("OK", (string)value.status);
        }
    }
}
