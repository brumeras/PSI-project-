using KNOTS.Compability;
using KNOTS.Services.Compability;
using KNOTS.Services.Interfaces;
using Moq;

namespace TestProject1.UserServiceTests.CompatabilityTests;

public class GetPlayerStatisticsUnit {
    [Fact]
    public void GetPlayerStatistics_ReturnsCorrectResults(){
        var res = new List<CompatibilityScore> {
            new CompatibilityScore("pirmas", "antras", 1, 2, new List<string>{"s1"}),
            new CompatibilityScore("pirmas", "trecias", 2, 2, new List<string>{"s1", "s2"}),
            new CompatibilityScore("antras", "trecias", 0, 2, new List<string>()),
        };
        var calculator = new CompatibilityCalculator(Mock.Of<InterfaceSwipeRepository>());
        var statpirmas = calculator.GetPlayerStatistics("pirmas", res);
        var statantras = calculator.GetPlayerStatistics("antras", res);
        var statidk =  calculator.GetPlayerStatistics("idk", res);
        
        Assert.Equal("pirmas", statpirmas.PlayerUsername);
        Assert.Equal(1, statpirmas.GamesPlayed);
        Assert.InRange(statpirmas.AverageCompatibility, 50, 100);
        Assert.InRange(statpirmas.BestMatchPercentage, 50, 100);
        Assert.True(statpirmas.WasBestMatch == false || statpirmas.WasBestMatch == true);
        
        Assert.Equal("antras", statantras.PlayerUsername);
        Assert.Equal(1, statantras.GamesPlayed);
        Assert.InRange(statantras.AverageCompatibility, 0, 100);
        
        Assert.Equal("idk", statidk.PlayerUsername);
        Assert.Equal(0, statidk.AverageCompatibility);
        Assert.Equal(0, statidk.BestMatchPercentage);
        Assert.False(statidk.WasBestMatch);
        
    }
    
}