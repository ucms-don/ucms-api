namespace Ucms.Api.Options;

using Microsoft.Extensions.Options;

public class EventBusOptions
{
    public const string SectionName = "RabbitMQ";

    public string Uri { get; set; } = default!;
    public string UserName { get; set; } = default!;
    public string Password { get; set; } = default!;
}

public class EventBusOptionsValidator : IValidateOptions<EventBusOptions>
{
    public ValidateOptionsResult Validate(string? name, EventBusOptions options)
    {
        var result = ValidateOptionsResult.Success;

        if (string.IsNullOrEmpty(options.Uri))
        {
            result = ValidateOptionsResult.Fail($"EventBus {nameof(EventBusOptions.Uri)} can not be empty");
        }

        if (string.IsNullOrEmpty(options.UserName))
        {
            result = ValidateOptionsResult.Fail($"EventBus {nameof(EventBusOptions.UserName)} can not be empty");
        }

        if (string.IsNullOrEmpty(options.Password))
        {
            result = ValidateOptionsResult.Fail($"EventBus {nameof(EventBusOptions.Password)} can not be empty");
        }

        return result;
    }
}
