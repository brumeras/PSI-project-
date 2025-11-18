using KNOTS.Services;
using KNOTS.Services.Compability;
using KNOTS.Services.Interfaces;
using Moq;

namespace TestProject1.UserServiceTests.CompatabilityTests;
public class CalculateUnit : UserServiceTestBase {

    [Fact]
    public void Calculate_TwoPlayersWithMatchingSwipes_ReturnsCorectUNIT() {
        var fakeSwipes = new Dictionary<string, List<PlayerSwipe>>{
            ["pirmas"] = new List<PlayerSwipe>() {
                new PlayerSwipe("pirmas", "s1", "pirmas statement", true),
                new PlayerSwipe("pirmas", "s2", "antras statement", false),
                new PlayerSwipe("pirmas", "s3", "trecias statement", true),
            },
            ["antras"] = new List<PlayerSwipe>() {
                new PlayerSwipe("antras", "s1", "pirmas statement", true),
                new PlayerSwipe("antras", "s2", "antras statement", true),
                new PlayerSwipe("antras", "s3", "trecias statement", true),
            }
        };
        //naudojam packaga mockinimui repositoriju, pas destytoja konspekte rasta, mock arkartos ta interfeisa
        var mockRepo = new Mock<InterfaceSwipeRepository>();
        mockRepo.Setup(r => r.GetPlayerSwipes(It.IsAny<string>(), "pirmas")).Returns(fakeSwipes["pirmas"]);
        mockRepo.Setup(r => r.GetPlayerSwipes(It.IsAny<string>(), "antras")).Returns(fakeSwipes["antras"]);
        
        var calculator = new CompatibilityCalculator(mockRepo.Object);
        var res = calculator.Calculate("room1", "pirmas", "antras");
        Assert.Equal(3, res.TotalStatements);
        Assert.Equal(2, res.MatchingSwipes);
        Assert.Equal(66.67, res.Percentage);
        
    }
}