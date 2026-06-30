namespace Ucms.Application.Features.Outcomes.DTOs;

using Ucms.Application.Features.Skus.DTOs;

public record OutcomeStatsModel(
    List<OutcomeStatItemModel> CurrentPeriod,
    List<OutcomeStatItemModel> PreviousPeriod
);

public record OutcomeStatItemModel(
    SkuModel Sku,
    int Count
);
