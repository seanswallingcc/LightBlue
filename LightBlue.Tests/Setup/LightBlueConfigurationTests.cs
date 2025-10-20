using System;
using System.Configuration;
using System.Reflection;
using LightBlue.Setup;
using Xunit;

namespace LightBlue.Tests.Setup
{
    public class LightBlueConfigurationTests : IDisposable
    {
        private readonly string[] _environmentVariablesToCleanup;

        public LightBlueConfigurationTests()
        {
            // Track environment variables we set during tests for cleanup
            _environmentVariablesToCleanup = new[]
            {
                "TestKey1",
                "TestKey2", 
                "ExistingKey",
                "IntegrationTestKey1",
                "IntegrationTestKey2",
            };
        }

        public void Dispose()
        {
            // Clean up environment variables after each test
            foreach (var envVar in _environmentVariablesToCleanup)
            {
                Environment.SetEnvironmentVariable(envVar, null);

                // We only have ExistingKey in app.config, reset to just that
                ConfigurationManager.RefreshSection("appSettings");
            }
            
            // Reset LightBlueConfiguration context to null using reflection
            var contextField = typeof(LightBlueConfiguration).GetField("_context", 
                BindingFlags.NonPublic | BindingFlags.Static);
            contextField?.SetValue(null, null);
        }

        [Fact]
        public void LoadAzureEnvironmentSettings_MethodCanBeInvokedWithoutException()
        {
            // Arrange
            Environment.SetEnvironmentVariable("TestKey1", "TestValue1");
            Environment.SetEnvironmentVariable("TestKey2", "TestValue2");

            // Act
            var exception = Record.Exception(() => LightBlueConfiguration.GetConfiguredContext());

            // Assert
            Assert.Null(exception); // Method should execute without throwing
        }

        [Fact]
        public void LoadAzureEnvironmentSettings_ProcessesEnvironmentVariablesCorrectly()
        {
            // Arrange
            Environment.SetEnvironmentVariable("TestKey1", "TestValue1");
            ILightBlueContext context = null;
            
            // Act
            var exception = Record.Exception(() => context = LightBlueConfiguration.GetConfiguredContext());
            
            // Assert
            Assert.Null(exception); // Method should execute without throwing
            Assert.Equal("TestValue1", context.Settings["TestKey1"]);
        }

        [Fact]
        public void LoadAzureEnvironmentSettings_HandlesEmptyEnvironmentVariables()
        {
            // Arrange
            Environment.SetEnvironmentVariable("TestKey1", "");
            ILightBlueContext context = null;

            // Act
            var exception = Record.Exception(() => context = LightBlueConfiguration.GetConfiguredContext());

            // Assert
            Assert.Null(exception); // Method should execute without throwing
            Assert.Null(context.Settings["TestKey1"]);
        }

        [Fact]
        public void LoadAzureEnvironmentSettings_HandlesNullEnvironmentVariables()
        {
            // Arrange
            Environment.SetEnvironmentVariable("TestKey1", "SomeValue");
            Environment.SetEnvironmentVariable("TestKey1", null); // Remove it
            ILightBlueContext context = null;

            // Act
            var exception = Record.Exception(() => context = LightBlueConfiguration.GetConfiguredContext());

            // Assert
            Assert.Null(exception); // Method should execute without throwing
            Assert.Null(context.Settings["TestKey1"]);
        }

        [Fact]
        public void LoadAzureEnvironmentSettings_IntegrationTest()
        {
            // Arrange
            ILightBlueContext context = null;
            var testEnvironmentVariables = new[]
            {
                ("ExistingKey", "NewValue"),
                ("IntegrationTestKey1", "IntegrationTestValue1"),
                ("IntegrationTestKey2", "Special!@#$%Characters"),
            };

            // ConfigurationManager.AppSettings already have ExistingKey from the app.config file
            Assert.Equal("ExistingValue", ConfigurationManager.AppSettings["ExistingKey"]);

            // Set up test environment variables
            foreach (var (key, value) in testEnvironmentVariables)
            {
                Environment.SetEnvironmentVariable(key, value);
            }

            // Act
            var exception = Record.Exception(() => context = LightBlueConfiguration.GetConfiguredContext());

            // Assert
            Assert.Null(exception); // Method should execute without throwing

            // Verify environment variables are still accessible (they weren't corrupted)
            foreach (var (key, value) in testEnvironmentVariables)
            {
                Assert.Equal(value, Environment.GetEnvironmentVariable(key));
            }

            // Verify that the context settings reflect the environment variables
            foreach (var (key, value) in testEnvironmentVariables)
            {
                Assert.Equal(value, context.Settings[key]);
            }
        }
    }
}