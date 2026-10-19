using System.ComponentModel.DataAnnotations;
using AppForSEII.API.Models;

namespace AppForSEII.API.DTOs.PurchaseDTOs;

public class ItemsForPurchaseDTO
{
	public ItemsForPurchaseDTO(
		int id,
		string name,
		string brand,
		decimal price,
		int? recommendedAge,
		Gender? gender,
		string? size,
		int quantityAvailableForPurchase,
		SportForPurchaseDTO sport)
	{
		Id = id;
		Name = name;
		Brand = brand;
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
	
	[StringLength(50)]
	public string Brand { get; set; }
	
	[Range(typeof(decimal), "0.01", "9999999999999999.99")]
	public decimal Price { get; set; }
	
	public int? RecommendedAge { get; set; }
	
	public Gender? Gender { get; set; }
	
	public string? Size { get; set; }
	
	[Range(0, int.MaxValue)]
	public int QuantityAvailableForPurchase { get; set; }
	
	public SportForPurchaseDTO Sport { get; set; }
}
