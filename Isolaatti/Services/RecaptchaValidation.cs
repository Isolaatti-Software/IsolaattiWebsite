using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Isolaatti.Config;
using Isolaatti.DTOs;
using Microsoft.Extensions.Options;

namespace Isolaatti.Services;

public class RecaptchaValidation
{
    private readonly IOptions<ReCaptchaConfig> _recaptchaConfig;
    private readonly HttpClientSingleton _httpClientSingleton;
    private const string RecaptchaApi = "https://www.google.com/recaptcha/api/siteverify";

    public RecaptchaValidation(IOptions<ReCaptchaConfig> recaptchaConfig, HttpClientSingleton httpClientSingleton)
    {
        _recaptchaConfig = recaptchaConfig;
        _httpClientSingleton = httpClientSingleton;
    }
    
    /// <summary>
    /// Validates recaptcha response from client.
    /// </summary>
    /// <param name="recaptchaResponse"></param>
    /// <returns>True if valid</returns>
    public async Task<bool> ValidateRecaptcha(string recaptchaResponse)
    {
        var recaptchaValidationResponseMessage = await _httpClientSingleton.Client.PostAsync(RecaptchaApi,
            new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("secret", _recaptchaConfig.Value.Secret),
                new KeyValuePair<string, string>("response", recaptchaResponse)
            }));

        var recaptchaValidation =
            await recaptchaValidationResponseMessage.Content.ReadFromJsonAsync<RecaptchaResponse>();

        return recaptchaValidation.Success;
    }
}