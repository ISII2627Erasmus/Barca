using AppForSEII.API.Models;

namespace AppForSEII.API.DTOs.PurchaseDTOs;

public record ItemsForPurchaseDTO(
    int Id,
    string Name,
    string Brand,
    decimal Price,
    int? RecommendedAge,
    Gender? Gender,
    string? Size,
    SportForPurchaseDTO Sport);

public record SportForPurchaseDTO(int Id, string Name);
