using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.ViewModels;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Commerce.Settings;

public class PricePartSettingsDisplayDriver : ContentTypePartDefinitionDisplayDriver
{
    private readonly IStringLocalizer<PricePartSettingsDisplayDriver> _s;
    private readonly IMoneyService _moneyService;

    public PricePartSettingsDisplayDriver(IStringLocalizer<PricePartSettingsDisplayDriver> localizer, IMoneyService moneyService)
    {
        _s = localizer;
        _moneyService = moneyService;
    }

    public override IDisplayResult Edit(ContentTypePartDefinition model, IUpdateModel updater)
    {
        if (model.PartDefinition.Name != nameof(PricePart)) return null;

        return Initialize("PricePartSettings_Edit", (Action<PricePartSettingsViewModel>)(viewModel =>
        {
            var settings = model.GetSettings<PricePartSettings>();

            viewModel.CurrencySelectionMode = settings.CurrencySelectionMode;
            viewModel.CurrencySelectionModes = new List<SelectListItem>
            {
                new(CurrencySelectionModeEnum.AllCurrencies.ToString(), _s["All Currencies"]),
                new(CurrencySelectionModeEnum.DefaultCurrency.ToString(), _s["Default Currency"]),
                new(CurrencySelectionModeEnum.SpecificCurrency.ToString(), _s["Specific Currency"]),
            };
            viewModel.SpecificCurrencyIsoCode = settings.SpecificCurrencyIsoCode;
            viewModel.Currencies = _moneyService.Currencies
                .OrderBy(c => c.CurrencyIsoCode)
                .Select(c => new SelectListItem(
                    c.CurrencyIsoCode,
                    $"{c.CurrencyIsoCode} {c.Symbol} - {_s[c.EnglishName]}"));
        })).Location("Content");
    }

    public override async Task<IDisplayResult> UpdateAsync(ContentTypePartDefinition model, UpdateTypePartEditorContext context)
    {
        if (model.PartDefinition.Name != nameof(PricePart)) return null;

        var viewModel = new PricePartSettingsViewModel();

        await context.Updater.TryUpdateModelAsync(
            viewModel,
            Prefix,
            m => m.CurrencySelectionMode,
            m => m.SpecificCurrencyIsoCode);

        context.Builder.WithSettings(new PricePartSettings
        {
            CurrencySelectionMode = viewModel.CurrencySelectionMode,
            SpecificCurrencyIsoCode = viewModel.CurrencySelectionMode == CurrencySelectionModeEnum.SpecificCurrency
                    ? viewModel.SpecificCurrencyIsoCode
                    : null,
        });

        return await EditAsync(model, context.Updater);
    }
}
