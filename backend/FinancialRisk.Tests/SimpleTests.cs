using Microsoft.VisualStudio.TestTools.UnitTesting;
using FinancialRisk.Api.Models;

namespace FinancialRisk.Tests
{
    [TestClass]
    public class SimpleTests
    {
        [TestMethod]
        public void BasicTest_ShouldPass()
        {
            // Arrange
            var testValue = 42;

            // Act
            var result = testValue * 2;

            // Assert
            Assert.AreEqual(84, result);
        }

        [TestMethod]
        public void ApiResponseTest_ShouldPass()
        {
            // Arrange
            var response = new ApiResponse<string>
            {
                Success = true,
                Data = "test data",
                ErrorMessage = null
            };

            // Act & Assert
            Assert.IsTrue(response.Success);
            Assert.AreEqual("test data", response.Data);
            Assert.IsNull(response.ErrorMessage);
        }

        [TestMethod]
        public void PortfolioBuilderTest_ShouldPass()
        {
            // Arrange
            var portfolio = new PortfolioBuilder
            {
                Id = "test-id",
                Name = "Test Portfolio",
                Description = "Test Description"
            };

            // Act & Assert
            Assert.AreEqual("test-id", portfolio.Id);
            Assert.AreEqual("Test Portfolio", portfolio.Name);
            Assert.AreEqual("Test Description", portfolio.Description);
        }
    }
}
