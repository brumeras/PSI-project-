using Xunit;
using KNOTS.Services;
using System;
using System.Collections.Generic;

namespace KNOTS.Tests.Components.Pages
{
    public class HomeAuthenticationTests
    {
        // Fake implementation for testing
        private class FakeUserService : InterfaceUserService
        {
            public string? CurrentUser { get; private set; }
            public bool IsAuthenticated { get; private set; }

            public event Action? OnAuthenticationChanged;

            public (bool Success, string Message) RegisterUser(string username, string password)
            {
                return (true, "Registered");
            }

            public (bool Success, string Message) LoginUser(string username, string password)
            {
                CurrentUser = username;
                IsAuthenticated = true;
                OnAuthenticationChanged?.Invoke();
                return (true, "Logged in");
            }

            public void LogoutUser()
            {
                CurrentUser = null;
                IsAuthenticated = false;
                OnAuthenticationChanged?.Invoke();
            }

            public int GetTotalUsersCount()
            {
                return 1;
            }

            public void UpdateUserStatistics(string username, double compatibilityScore, bool wasBestMatch)
            {
                // Fake no-op
            }

            public List<User> GetLeaderboard(int topCount = 10)
            {
                return new List<User>();
            }

            public int GetUserRank(string username)
            {
                return 1;
            }

            // Helper for simpler test setup
            public void SetAuthenticated(bool state, string? user = null)
            {
                IsAuthenticated = state;
                CurrentUser = user;
            }
        }

        // ---------------------
        // TESTAI
        // ---------------------

        [Fact]
        public void UserService_WhenAuthenticated_ReturnsTrue()
        {
            var userService = new FakeUserService();
            userService.SetAuthenticated(true, "TestUser");

            Assert.True(userService.IsAuthenticated);
            Assert.Equal("TestUser", userService.CurrentUser);
        }

        [Fact]
        public void UserService_WhenNotAuthenticated_ReturnsFalse()
        {
            var userService = new FakeUserService();
            userService.SetAuthenticated(false);

            Assert.False(userService.IsAuthenticated);
        }

        [Fact]
        public void UserService_AfterLogout_IsNotAuthenticated()
        {
            var userService = new FakeUserService();
            userService.SetAuthenticated(true, "TestUser");

            userService.LogoutUser();

            Assert.False(userService.IsAuthenticated);
            Assert.Null(userService.CurrentUser);
        }

        [Fact]
        public void UserService_LoginUser_SetsAuthenticationAndUser()
        {
            var userService = new FakeUserService();

            var result = userService.LoginUser("TestUser", "pass");

            Assert.True(result.Success);
            Assert.True(userService.IsAuthenticated);
            Assert.Equal("TestUser", userService.CurrentUser);
        }

        [Fact]
        public void UserService_OnAuthenticationChanged_IsTriggered()
        {
            var userService = new FakeUserService();
            bool triggered = false;

            userService.OnAuthenticationChanged += () => triggered = true;

            userService.LoginUser("User", "pass");

            Assert.True(triggered);
        }

        [Fact]
        public void UserService_GetTotalUsersCount_ReturnsFakeValue()
        {
            var userService = new FakeUserService();
            Assert.Equal(1, userService.GetTotalUsersCount());
        }

        [Fact]
        public void UserService_GetLeaderboard_ReturnsList()
        {
            var userService = new FakeUserService();
            var leaderboard = userService.GetLeaderboard();

            Assert.NotNull(leaderboard);
            Assert.Empty(leaderboard);
        }

        [Fact]
        public void UserService_GetUserRank_ReturnsFakeValue()
        {
            var userService = new FakeUserService();
            Assert.Equal(1, userService.GetUserRank("someone"));
        }
    }
}
