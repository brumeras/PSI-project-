using KNOTS.Services;
using KNOTS.Services.Compability;
using KNOTS.Services.Interfaces;
using Moq;

namespace TestProject1.UserServiceTests.CompatabilityTests;

public class CalculateAllCompatibilitiesUnit{
    [Fact]
    public void CalculateAllCompatibilities_ReturnsCorrectPairwiseResultsUNIT(){
        var roomCode = "room1";
        var players = new List<string> { "pirmas", "antras", "trecias" };

        var mockRepo = new Mock<InterfaceSwipeRepository>();
        mockRepo.Setup(r => r.GetPlayerSwipes(roomCode, "pirmas")).Returns(new List<PlayerSwipe>
        {
            new PlayerSwipe("pirmas", "s1", "statement 1", true),
            new PlayerSwipe("pirmas", "s2", "statement 2", false),
        });
        mockRepo.Setup(r => r.GetPlayerSwipes(roomCode, "antras")).Returns(new List<PlayerSwipe>
        {
            new PlayerSwipe("antras", "s1", "statement 1", true),
            new PlayerSwipe("antras", "s2", "statement 2", true),
        });
        mockRepo.Setup(r => r.GetPlayerSwipes(roomCode, "trecias")).Returns(new List<PlayerSwipe>
        {
            new PlayerSwipe("trecias", "s1", "statement 1", false),
            new PlayerSwipe("trecias", "s2", "statement 2", false),
        });

        var calculator = new CompatibilityCalculator(mockRepo.Object);
        var res = calculator.CalculateAllCompatibilities(roomCode, players);
        
        Assert.Equal(3, res.Count());
        Assert.Contains(res, r => r.Player1 == "pirmas" && r.Player2 == "antras");
        Assert.Contains(res, r => r.Player1 == "pirmas" && r.Player2 == "trecias");
        Assert.Contains(res, r => r.Player1 == "antras" && r.Player2 == "trecias");
        
        var pair1 = res.First(r => r.Player1 == "pirmas" && r.Player2 == "antras");
        Assert.Equal(2, pair1.TotalStatements);
        Assert.Equal(1, pair1.MatchingSwipes);
        Assert.Equal(50, pair1.Percentage);
        
        var pair2 = res.First(r => r.Player1 == "pirmas" && r.Player2 == "trecias");
        Assert.Equal(2, pair2.TotalStatements);
        Assert.Equal(1, pair2.MatchingSwipes);
        Assert.Equal(50, pair2.Percentage);
        
        var pair3 = res.First(r => r.Player1 == "antras" && r.Player2 == "trecias");
        Assert.Equal(2, pair3.TotalStatements);
        Assert.Equal(0, pair3.MatchingSwipes);
        Assert.Equal(0, pair3.Percentage);
        
        var emptyResult = calculator.CalculateAllCompatibilities(roomCode, new List<string>());
        Assert.Empty(emptyResult);
        
        var singlePlayerResult = calculator.CalculateAllCompatibilities(roomCode, new List<string> { "onlyplayer" });
        Assert.Empty(singlePlayerResult);
    }
    
}