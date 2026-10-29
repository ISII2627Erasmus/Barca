using AppForSEII.API.Models;

namespace AppForSEII.API.DTOs.PurchaseDTOs;

public record GetPurchaseDTO(
    int Id,
    DateTime PurchaseDate,
    string? PurchaseDescription,
    decimal TotalPrice,
    bool Wrapped,
    PurchaseUserDTO User,
    List<PurchaseItemDTO> PurchaseItems);

public record PurchaseUserDTO(string? Name, string? Surname);

public record PurchaseItemDTO(
    int ItemId,
    string Name,
    string Brand,
    decimal Price,
    int? RecommendedAge,
    Gender? Gender,
    string? Size,
    int Quantity);
