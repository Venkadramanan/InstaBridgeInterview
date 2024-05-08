using System.Text.Json;
using System.Text.Json.Nodes;
using OrchardCore.Deployment;
using OrchardCore.Json.Serialization;
using OrchardCore.OpenId.Deployment;
using OrchardCore.OpenId.Recipes;
using OrchardCore.OpenId.Services;
using OrchardCore.OpenId.Settings;
using OrchardCore.Recipes.Models;
using OrchardCore.Tests.Stubs;

namespace OrchardCore.Tests.Modules.OrchardCore.OpenId
{
    public class OpenIdServerDeploymentSourceTests
    {
        private static OpenIdServerSettings CreateSettings(string authority, OpenIdServerSettings.TokenFormat tokenFormat, bool initializeAllProperties)
        {
            var result = new OpenIdServerSettings
            {
                Authority = new Uri(authority),
                AccessTokenFormat = tokenFormat
            };

            if (initializeAllProperties)
            {
                result.TokenEndpointPath = "/connect/token";
                result.AuthorizationEndpointPath = "/connect/authorize";
                result.LogoutEndpointPath = "/connect/logout";
                result.UserinfoEndpointPath = "/connect/userinfo";
                result.IntrospectionEndpointPath = "/connect/introspect";
                result.RevocationEndpointPath = "/connect/revoke";

                result.EncryptionCertificateStoreLocation = StoreLocation.LocalMachine;
                result.EncryptionCertificateStoreName = StoreName.My;
                result.EncryptionCertificateThumbprint = Guid.NewGuid().ToString();

                result.SigningCertificateStoreLocation = StoreLocation.LocalMachine;
                result.SigningCertificateStoreName = StoreName.My;
                result.SigningCertificateThumbprint = Guid.NewGuid().ToString();

                result.AllowAuthorizationCodeFlow = true;
                result.AllowClientCredentialsFlow = true;
                result.AllowHybridFlow = true;
                result.AllowImplicitFlow = true;
                result.AllowPasswordFlow = true;
                result.AllowRefreshTokenFlow = true;

                result.DisableAccessTokenEncryption = true;
                result.DisableRollingRefreshTokens = true;
                result.UseReferenceAccessTokens = true;
                result.RequireProofKeyForCodeExchange = true;
            }

            return result;
        }

        private static Mock<IOpenIdServerService> CreateServerServiceWithSettingsMock(OpenIdServerSettings settings)
        {
            var serverService = new Mock<IOpenIdServerService>();

            serverService
                .Setup(m => m.GetSettingsAsync())
                .ReturnsAsync(settings);

            serverService
                .Setup(m => m.LoadSettingsAsync())
                .ReturnsAsync(settings);

            return serverService;
        }

        [Fact]
        public async Task ServerDeploymentSourceIsReadableByRecipe()
        {
            // Arrange
            var recipeFile = "Recipe.json";

            var expectedSettings = CreateSettings("https://deploy.localhost", OpenIdServerSettings.TokenFormat.JsonWebToken, true);
            var deployServerServiceMock = CreateServerServiceWithSettingsMock(expectedSettings);

            var actualSettings = CreateSettings("https://recipe.localhost", OpenIdServerSettings.TokenFormat.DataProtection, false);
            var recipeServerServiceMock = CreateServerServiceWithSettingsMock(actualSettings);

            var settingsProperties = typeof(OpenIdServerSettings)
                .GetProperties();

            foreach (var property in settingsProperties)
            {
                Assert.NotEqual(
                    property.GetValue(expectedSettings),
                    property.GetValue(actualSettings));
            }

            var fileBuilder = new MemoryFileBuilder();
            var descriptor = new RecipeDescriptor();
            var result = new DeploymentPlanResult(fileBuilder, descriptor);

            var jsonOptions = new JsonSerializerOptions(JOptions.Base);
            jsonOptions.Converters.Add(System.Text.Json.Serialization.DynamicJsonConverter.Instance);
            jsonOptions.Converters.Add(PathStringJsonConverter.Instance);

            var deploymentSource = new OpenIdServerDeploymentSource(deployServerServiceMock.Object);

            // Act
            await deploymentSource.ProcessDeploymentStepAsync(new OpenIdServerDeploymentStep(), result);
            await result.FinalizeAsync();

            var deploy = JsonNode.Parse(
                fileBuilder.GetFileContents(
                    recipeFile,
                    Encoding.UTF8));

            var recipeContext = new RecipeExecutionContext
            {
                RecipeDescriptor = descriptor,
                Name = deploy["steps"][0].Value<string>("name"),
                Step = (JsonObject)deploy["steps"][0],
            };

            var recipeStep = new OpenIdServerSettingsStep(recipeServerServiceMock.Object);
            await recipeStep.ExecuteAsync(recipeContext);

            // Assert
            foreach (var property in settingsProperties)
            {
                Assert.Equal(
                    property.GetValue(expectedSettings),
                    property.GetValue(actualSettings));
            }
        }
    }
}
