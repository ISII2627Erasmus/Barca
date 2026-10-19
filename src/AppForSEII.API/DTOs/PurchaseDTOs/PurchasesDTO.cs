using System.ComponentModel.DataAnnotations;
using AppForSEII.API.Models;

namespace AppForSEII.API.DTOs.PurchaseDTOs;

public sealed class GetPurchaseDTO
{
    public GetPurchaseDTO(
        int id,
        DateTime purchaseDate,
        string? purchaseDescription,
        double totalPrice,
        string street,
        string city,
        string province,
        string creditCardNumber,
        string ccv,
        string expirationDate,
        bool wrapped,
        PurchaseStatus purchaseStatus,
        string userId,
        PurchaseUserDTO user,
        IReadOnlyList<PurchaseItemDTO> purchaseItems)
    {
        Id = id;
        PurchaseDate = purchaseDate;
        PurchaseDescription = purchaseDescription;
        TotalPrice = totalPrice;
        Street = street;
        City = city;
        Province = province;
        CreditCardNumber = creditCardNumber;
        CCV = ccv;
        ExpirationDate = expirationDate;
        Wrapped = wrapped;
        PurchaseStatus = purchaseStatus;
        UserId = userId;
        User = user;
        PurchaseItems = purchaseItems;
    }

    public int Id { get; set; }
    public DateTime PurchaseDate { get; set; }
    public string? PurchaseDescription { get; set; }
    public double TotalPrice { get; set; }

    [StringLength(255, MinimumLength = 3)]
    public string Street { get; set; }

    [StringLength(255, MinimumLength = 3)]
    public string City { get; set; }

    [StringLength(255, MinimumLength = 3)]
    public string Province { get; set; }

    public string CreditCardNumber { get; set; }
    public string CCV { get; set; }
    public string ExpirationDate { get; set; }
    public bool Wrapped { get; set; }
    public PurchaseStatus PurchaseStatus { get; set; }
    public string UserId { get; set; }
    public PurchaseUserDTO User { get; set; }
    public IReadOnlyList<PurchaseItemDTO> PurchaseItems { get; set; }
}

public sealed class PurchaseUserDTO
{
    public PurchaseUserDTO(
        string? name,
        string? surname,
        DateOnly birthDate,
        int age,
        Gender gender)
    {
        Name = name;
        Surname = surname;
        BirthDate = birthDate;
        Age = age;
        Gender = gender;
    }

    public string? Name { get; set; }
    public string? Surname { get; set; }
    public DateOnly BirthDate { get; set; }
    public int Age { get; set; }
    public Gender Gender { get; set; }
}

public sealed class PurchaseItemDTO
{
    public PurchaseItemDTO(
        int purchaseId,
        int itemId,
        int quantity,
        decimal price,
        ItemForPurchaseDTO item)
    {
        PurchaseId = purchaseId;
        ItemId = itemId;
        Quantity = quantity;
        Price = price;
        Item = item;
    }

    public int PurchaseId { get; set; }
    public int ItemId { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public ItemForPurchaseDTO Item { get; set; }
}

public sealed class ItemForPurchaseDTO
{
    public ItemForPurchaseDTO(
        int id,
        string name,
        decimal price,
        int? recommendedAge,
        Gender? gender,
        string? size,
        int quantityAvailableForPurchase,
        SportForPurchaseDTO sport)
    {
        Id = id;
        Name = name;
        Price = price;
        RecommendedAge = recommendedAge;
        Gender = gender;
        Size = size;
        QuantityAvailableForPurchase = quantityAvailableForPurchase;
        Sport = sport;
    }

    public int Id { get; set; }

    [StringLength(100)]
    public string Name { get; set; }

    public decimal Price { get; set; }
    public int? RecommendedAge { get; set; }
    public Gender? Gender { get; set; }
    public string? Size { get; set; }
    public int QuantityAvailableForPurchase { get; set; }
    public SportForPurchaseDTO Sport { get; set; }
}

public sealed class SportForPurchaseDTO
{
    public SportForPurchaseDTO(
        int id,
        string name,
        int minimumNumberOfPlayers,
        int numberOfReferees,
        string description,
        string basicRules)
    {
        Id = id;
        Name = name;
        MinimumNumberOfPlayers = minimumNumberOfPlayers;
        NumberOfReferees = numberOfReferees;
        Description = description;
        BasicRules = basicRules;
    }

    public int Id { get; set; }

    [StringLength(50)]
    public string Name { get; set; }

    public int MinimumNumberOfPlayers { get; set; }
    public int NumberOfReferees { get; set; }
    public string Description { get; set; }
    public string BasicRules { get; set; }
}