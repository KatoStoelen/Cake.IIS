﻿#region Using Statements
using Xunit;
using Shouldly;
#endregion



namespace Cake.IIS.Tests.Tests
{
    public class ApplicationAuthenticationTests
    {

        [Theory]
        [InlineData(true, false, true)]
        [InlineData(false, true, false)]
        public void Should_Set_Authentication(bool? anon, bool? basic, bool? win)
        {
            //Setup
            var websiteName = "Batman";
            CakeHelper.DeleteWebsite(websiteName);

            // Arrange
            var websiteSettings = CakeHelper.GetWebsiteSettings(websiteName);
            CakeHelper.CreateWebsite(websiteSettings);

            var appSettings = CakeHelper.GetApplicationSettings(websiteName);
            appSettings.Authentication = CakeHelper.GetAuthenticationSettings(anon, basic, win);

            // Act
            WebsiteManager manager = CakeHelper.CreateWebsiteManager();
            var added = manager.AddApplication(appSettings);

            //Assert
            added.ShouldBeTrue();
            var authentication = CakeHelper.ReadAuthenticationSettings(websiteName, appSettings.ApplicationPath);

            authentication.EnableAnonymousAuthentication.ShouldBe(anon);
            authentication.EnableBasicAuthentication.ShouldBe(basic);
            authentication.EnableWindowsAuthentication.ShouldBe(win);

            //Teardown
            CakeHelper.DeleteWebsite(websiteName);
        }

        [Fact]
        public void Should_Set_ApplicationAuthentication_Only()
        {
            //Setup
            var websiteName = "Batman";
            var serverAuth = CakeHelper.ReadAuthenticationSettings();
            CakeHelper.DeleteWebsite(websiteName);

            // Arrange
            var websiteSettings = CakeHelper.GetWebsiteSettings(websiteName);
            CakeHelper.CreateWebsite(websiteSettings);

            var appSettings = CakeHelper.GetApplicationSettings(websiteName);
            var appAuth = CakeHelper.GetAuthenticationSettings(
                !serverAuth.EnableAnonymousAuthentication.Value,
                !serverAuth.EnableBasicAuthentication.Value,
                !serverAuth.EnableWindowsAuthentication.Value);// setting application-authenication opposite to server-level-authentication
            appSettings.Authentication = appAuth;

            // Act
            WebsiteManager manager = CakeHelper.CreateWebsiteManager();
            var added = manager.AddApplication(appSettings);

            //Assert
            added.ShouldBeTrue();
            AssertAuthentication(CakeHelper.ReadAuthenticationSettings(), serverAuth);  //server Auth
            AssertAuthentication(CakeHelper.ReadAuthenticationSettings(websiteName), serverAuth);  //website Auth
            AssertAuthentication(CakeHelper.ReadAuthenticationSettings(websiteName, appSettings.ApplicationPath), appAuth);  //website Auth

            //Teardown
            CakeHelper.DeleteWebsite(websiteName);

        }

        [Fact]
        public void Should_Only_Set_NotNull_Setting()
        {
            //Setup
            var websiteName = "Batman";
            CakeHelper.DeleteWebsite(websiteName);

            // Arrange
            var websiteSettings = CakeHelper.GetWebsiteSettings(websiteName);
            var serverAuth = CakeHelper.ReadAuthenticationSettings();
            CakeHelper.CreateWebsite(websiteSettings);

            var anon = serverAuth.EnableAnonymousAuthentication.Value;
            var basic = serverAuth.EnableBasicAuthentication.Value;
            var win = serverAuth.EnableWindowsAuthentication.Value;



            var appSettings = CakeHelper.GetApplicationSettings(websiteName);
            appSettings.Authentication = CakeHelper.GetAuthenticationSettings(!anon, null, null);  //only resetting anonymous to inverse of default. 

            WebsiteManager manager = CakeHelper.CreateWebsiteManager();
            var added = manager.AddApplication(appSettings);

            //Assert
            var webAuth = CakeHelper.ReadAuthenticationSettings(websiteName);
            var appAuth = CakeHelper.ReadAuthenticationSettings(websiteName, appSettings.ApplicationPath);

            AssertAuthentication(serverAuth, webAuth);

            appAuth.EnableAnonymousAuthentication.ShouldBe(!anon);
            appAuth.EnableBasicAuthentication.ShouldBe(basic);
            appAuth.EnableWindowsAuthentication.ShouldBe(win);

            //Teardown
            CakeHelper.DeleteWebsite(websiteName);
        }



        private void AssertAuthentication(AuthenticationSettings expected, AuthenticationSettings actual)
        {
            Assert.Equal(expected.EnableAnonymousAuthentication, actual.EnableAnonymousAuthentication.Value);
            Assert.Equal(expected.EnableBasicAuthentication, actual.EnableBasicAuthentication.Value);
            Assert.Equal(expected.EnableWindowsAuthentication, actual.EnableWindowsAuthentication.Value);
        }
    }
}
