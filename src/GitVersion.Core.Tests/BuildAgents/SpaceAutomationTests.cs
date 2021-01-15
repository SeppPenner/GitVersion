using System.IO;
using GitVersion;
using GitVersion.BuildAgents;
using GitVersion.Core.Tests.Helpers;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Shouldly;

namespace GitVersionCore.Tests.BuildAgents
{
    [TestFixture]
    public class SpaceAutomationTests : TestBase
    {
        private IEnvironment environment;
        private SpaceAutomation buildServer;
        private string setEnvironmentTempFilePath;

        [SetUp]
        public void SetUp()
        {
            var sp = ConfigureServices(services =>
            {
                services.AddSingleton<SpaceAutomation>();
            });
            environment = sp.GetService<IEnvironment>();
            buildServer = sp.GetService<SpaceAutomation>();
            environment.SetEnvironmentVariable(SpaceAutomation.EnvironmentVariableName, "true");

            setEnvironmentTempFilePath = Path.GetTempFileName();
        }

        [TearDown]
        public void TearDown()
        {
            environment.SetEnvironmentVariable(SpaceAutomation.EnvironmentVariableName, null);
            if (setEnvironmentTempFilePath != null && File.Exists(setEnvironmentTempFilePath))
            {
                File.Delete(setEnvironmentTempFilePath);
                setEnvironmentTempFilePath = null;
            }
        }

        [Test]
        public void CanApplyToCurrentContextShouldBeTrueWhenEnvironmentVariableIsSet()
        {
            // Act
            var result = buildServer.CanApplyToCurrentContext();

            // Assert
            result.ShouldBeTrue();
        }

        [Test]
        public void CanApplyToCurrentContextShouldBeFalseWhenEnvironmentVariableIsNotSet()
        {
            // Arrange
            environment.SetEnvironmentVariable(SpaceAutomation.EnvironmentVariableName, "");

            // Act
            var result = buildServer.CanApplyToCurrentContext();

            // Assert
            result.ShouldBeFalse();
        }

        [Test]
        public void GetCurrentBranchShouldHandleBranches()
        {
            // Arrange
            environment.SetEnvironmentVariable("JB_SPACE_GIT_BRANCH", "refs/heads/master");

            // Act
            var result = buildServer.GetCurrentBranch(false);

            // Assert
            result.ShouldBe("refs/heads/master");
        }

        [Test]
        public void GetCurrentBranchShouldHandleTags()
        {
            // Arrange
            environment.SetEnvironmentVariable("JB_SPACE_GIT_BRANCH", "refs/tags/1.0.0");

            // Act
            var result = buildServer.GetCurrentBranch(false);

            // Assert
            result.ShouldBe("refs/tags/1.0.0");
        }

        [Test]
        public void GetCurrentBranchShouldHandlePullRequests()
        {
            // Arrange
            environment.SetEnvironmentVariable("JB_SPACE_GIT_BRANCH", "refs/pull/1/merge");

            // Act
            var result = buildServer.GetCurrentBranch(false);

            // Assert
            result.ShouldBe("refs/pull/1/merge");
        }

        [Test]
        public void GetEmptyGenerateSetVersionMessage()
        {
            // Arrange
            var vars = new TestableVersionVariables("1.0.0");

            // Act
            var message = buildServer.GenerateSetVersionMessage(vars);

            // Assert
            message.ShouldBeEmpty();
        }
    }
}
