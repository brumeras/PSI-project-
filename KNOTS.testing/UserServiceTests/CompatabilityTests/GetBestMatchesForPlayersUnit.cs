using KNOTS.Compability;
using KNOTS.Services.Compability;
using KNOTS.Services.Interfaces;
using Moq; 
using Xunit;

namespace TestProject1.UserServiceTests.CompatabilityTests
{
    public class GetBestMatchesForPlayersUnit
    {
        [Fact]
        public void GetBestMatchesForPlayers_ReturnsCorrectResults()
        {
            // Arrange
            var res = new List<CompatibilityScore>
            {
                new CompatibilityScore("pirmas", "antras", 3, 3, new List<string>{"s1", "s2", "s3"}),
                new CompatibilityScore("pirmas", "trecias", 1, 3, new List<string>{"s1"}),
                new CompatibilityScore("antras", "trecias", 2, 3, new List<string>{"s1", "s2"}),
            };

            // Mock the swipe repository (not actually used in this test)
            var swipeRepoMock = new Mock<InterfaceSwipeRepository>();
            
            // Create calculator with the mock
            var calculator = new CompatibilityCalculator(swipeRepoMock.Object);

            // Act
            var result = calculator.GetBestMatchesForPlayers(res);

            // Assert
            Assert.Equal("antras", result["pirmas"].BestMatchPartner);
            Assert.Equal(100, result["pirmas"].BestMatchPercentage);
            Assert.True(result["pirmas"].WasBestMatchForPartner);

            Assert.Equal("pirmas", result["antras"].BestMatchPartner);
            Assert.Equal(100, result["antras"].BestMatchPercentage);
            Assert.True(result["antras"].WasBestMatchForPartner);

            Assert.Equal("antras", result["trecias"].BestMatchPartner);
            Assert.InRange(result["trecias"].BestMatchPercentage, 65.1, 66.9);
            Assert.False(result["trecias"].WasBestMatchForPartner);
        }
    }
}