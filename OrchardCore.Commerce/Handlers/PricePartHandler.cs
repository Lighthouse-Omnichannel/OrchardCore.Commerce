using System.Threading.Tasks;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Models;
using OrchardCore.ContentManagement.Handlers;

namespace OrchardCore.Commerce.Handlers;

public class PricePartHandler : ContentPartHandler<PricePart>
{
    private readonly IMoneyService _moneyService;

    public PricePartHandler(IMoneyService moneyService) => _moneyService = moneyService;

    public override Task LoadingAsync(LoadContentContext context, PricePart instance)
    {
        instance.Price = _moneyService.EnsureCurrency(instance.Price);

        return base.LoadingAsync(context, instance);
    }
}
