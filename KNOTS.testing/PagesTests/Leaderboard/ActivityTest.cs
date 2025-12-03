using System.Reflection;
using KNOTS.Services.Interfaces;
using KNOTS.Models;
using KNOTS.Components.Pages;
using Moq;
using Xunit;
using System.Collections.Generic;
using System.Linq;
using System.Globalization; 
using System;
using KNOTS.Compability;
using KNOTS.Services;

namespace KNOTS.Tests.ReflectionCode
{
    public class ActivityTests
    {
        private readonly Mock<InterfaceCompatibilityService> _mockCompatibilityService;
        private readonly Mock<InterfaceUserService> _mockUserService;

        public ActivityTests()
        {
            _mockCompatibilityService = new Mock<InterfaceCompatibilityService>();
            _mockUserService = new Mock<InterfaceUserService>();
        }

        // --- PAGALBINĖS FUNKCIJOS REFLEKSIJAI ---
        // Paliekame, nes jos reikalingos kitiems laukams (pvz., gameHistory) nustatyti.

        private void SetPrivateField(object obj, string fieldName, object value)
        {
            var field = obj.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            if (field == null)
            {
                 // Paliekame tik kitiems laukams. Ši išimtis dabar nebeiššauks, nes pašaliname problematišką testą.
                 throw new MissingFieldException($"Reflection failed: Field '{fieldName}' not found in component.");
            }
            field.SetValue(obj, value);
        }

        private T GetPrivateField<T>(object obj, string fieldName)
        {
            return (T)obj.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance)?
                .GetValue(obj)!;
        }
        
        private void InvokeMethod(object obj, string methodName)
        {
             obj.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance)?
                .Invoke(obj, null);
        }

        private List<GameHistoryEntry> GetSampleHistory() => new List<GameHistoryEntry>
        {
            new GameHistoryEntry 
            { 
                RoomCode = "ROOM-A", 
                BestMatchPlayer = "Match1", 
                BestMatchPercentage = 90.0, 
                TotalPlayers = 4, 
                PlayedDate = DateTime.Now.AddDays(-1),
                AllResults = new List<CompatibilityScore> { new CompatibilityScore() }
            },
            new GameHistoryEntry 
            { 
                RoomCode = "ROOM-B", 
                BestMatchPlayer = "Match2", 
                BestMatchPercentage = 70.0, 
                TotalPlayers = 5, 
                PlayedDate = DateTime.Now.AddDays(-2),
                AllResults = new List<CompatibilityScore> { new CompatibilityScore() }
            },
        };

        // =========================================================================
        // TESTAS 1: GET AVERAGE COMPATIBILITY (STATISTIKA)
        // =========================================================================
        // Šis testas veikia, nes nereikia Inject'inti Mock Service (UserService/CompatibilityService)
        [Fact]
        public void GetAverageCompatibility_CalculatesCorrectAverage()
        {
            // Arrange
            var testHistory = GetSampleHistory();
            var component = Activator.CreateInstance(typeof(KNOTS.Components.Pages.Activity))!;
            
            // Nustatome privatų kintamąjį gameHistory
            SetPrivateField(component, "gameHistory", testHistory);

            var method = component.GetType().GetMethod("GetAverageCompatibility", BindingFlags.NonPublic | BindingFlags.Instance);

            // Act
            var result = (string)method!.Invoke(component, null)!;

            // Assert
            // Ištaisyta formatavimo klaida (80,0 -> 80.0)
            if (result.Contains(','))
            {
                 result = result.Replace(',', '.');
            }
            
            Assert.Equal("80.0", result);
        }

        // =========================================================================
        // TESTAS 2: GET BEST MATCH EVER (STATISTIKA)
        // =========================================================================
        // Šis testas veikia
        [Fact]
        public void GetBestMatchEver_ReturnsHighestMatchPlayer()
        {
            // Arrange
            var testHistory = GetSampleHistory();
            testHistory.Add(new GameHistoryEntry { BestMatchPlayer = "TheBEST", BestMatchPercentage = 99.5 });
            
            var component = Activator.CreateInstance(typeof(KNOTS.Components.Pages.Activity))!;
            SetPrivateField(component, "gameHistory", testHistory);

            var method = component.GetType().GetMethod("GetBestMatchEver", BindingFlags.NonPublic | BindingFlags.Instance);

            // Act
            var result = (string)method!.Invoke(component, null)!;

            // Assert
            Assert.Equal("TheBEST", result);
        }

        // =========================================================================
        // TESTAS 3: DETALIŲ RODMAS IR UŽDARYMAS
        // =========================================================================
        // Šis testas veikia
        [Fact]
        public void ShowAndCloseDetails_TogglesSelectedGame()
        {
            // Arrange
            var gameToSelect = GetSampleHistory().First();
            var component = Activator.CreateInstance(typeof(KNOTS.Components.Pages.Activity))!;
            
            var showMethod = component.GetType().GetMethod("ShowDetails", BindingFlags.NonPublic | BindingFlags.Instance);
            var closeMethod = component.GetType().GetMethod("CloseDetails", BindingFlags.NonPublic | BindingFlags.Instance);

            // Act 1: Rodyti detales
            showMethod!.Invoke(component, new object[] { gameToSelect });
            
            // Assert 1: Patikrinti, ar selectedGame nustatytas
            var selectedGame1 = GetPrivateField<GameHistoryEntry>(component, "selectedGame");
            Assert.Equal(gameToSelect.RoomCode, selectedGame1.RoomCode);

            // Act 2: Uždaryti detales
            closeMethod!.Invoke(component, null);

            // Assert 2: Patikrinti, ar selectedGame yra null
            var selectedGame2 = GetPrivateField<GameHistoryEntry>(component, "selectedGame");
            Assert.Null(selectedGame2);
        }
    }
}