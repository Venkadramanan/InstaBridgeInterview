using System;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using OrchardCore.Facebook.Login.Services;
using OrchardCore.Facebook.Login.Settings;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.Facebook.Login.Recipes
{
    /// <summary>
    /// This recipe step sets general Facebook Login settings.
    /// </summary>
    public class FacebookLoginSettingsStep : IRecipeStepHandler
    {
        private readonly IFacebookLoginService _loginService;

        public FacebookLoginSettingsStep(IFacebookLoginService loginService)
        {
            _loginService = loginService;
        }

        public async Task ExecuteAsync(RecipeExecutionContext context)
        {
            if (!string.Equals(context.Name, nameof(FacebookLoginSettings), StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var model = context.Step.ToObject<FacebookLoginSettingsStepModel>();
            var settings = await _loginService.LoadSettingsAsync();

            settings.CallbackPath = model.CallbackPath;

            await _loginService.UpdateSettingsAsync(settings);
        }
    }

    public class FacebookLoginSettingsStepModel
    {
        public string CallbackPath { get; set; }
    }
}
