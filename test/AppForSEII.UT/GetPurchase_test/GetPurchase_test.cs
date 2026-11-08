using System.Security.Claims;
using AppForSEII.API.Controllers;
using AppForSEII.API.DTOs.PurchaseDTOs;
using Microsoft.AspNetCore.Http;

namespace AppForSEII.UT.PurchasesController_test.GetPurchase_test;

public class GetPurchase_test : IClassFixture<Databasefixture4GetOprations>
{
	private readonly Databasefixture4GetOprations _fixture;
	
	public GetPurchase_test(Databasefixture4GetOprations fixture)
	{
		_fixture = fixture;
	}
	
	[Fact]
	public async Task WithoutUser_ReturnsUnauthorized()
	{
		var result = await GetPurchase(null, 1); //null produces an empty principal, with no user ID claim.
												 //String such as purchase-user produces a principal with a NameIdentifier claim containing that string.
		
		Assert.IsType<UnauthorizedResult>(result.Result);
	}
	
	[Fact]
	public async Task OwnedByAnotherUser_ReturnsNotFound()
	{
		var purchase = await SavePurchase();
		var result = await GetPurchase("another-user", purchase.Id);
		
		Assert.IsType<NotFoundResult>(result.Result);
	}
	
	[Fact]
	public async Task ReturnsPurchaseForCurrentUser()
	{
		var purchase = await SavePurchase();
		var result = await GetPurchase(_fixture.Customer.Id, purchase.Id);
		
		var response = Assert.IsType<OkObjectResult>(result.Result);
		var actual = Assert.IsType<GetPurchaseDTO>(response.Value);
		var expected = new GetPurchaseDTO(
			purchase.Id,
			purchase.PurchaseDate,
			"Fixture purchase",
			purchase.TotalPrice,
			false,
			new PurchaseUserDTO("Taylor", "Smith"),
			new List<PurchaseItemDetailDTO>
			{
				new(
					_fixture.FootballBoots.Id,
					"Football Boots",
					"Nike",
					59.90m,
					21,
					Gender.Male,
					"42",
					2)
			});

		Assert.Equal(expected, actual);
	}
	
	private async Task<ActionResult<GetPurchaseDTO>> GetPurchase(
		string? userId,
		int purchaseId)
	{
		using var context = _fixture.CreateContext();
		var controller = new PurchasesController(context)
		{
			ControllerContext = new ControllerContext
			{
				HttpContext = new DefaultHttpContext { User = CreateUser(userId) }
			}
		};
		
		return await controller.GetPurchase(purchaseId);
	}
	
	private async Task<Purchase> SavePurchase()
	{
		using var context = _fixture.CreateContext(); // Creating data in class fixture
		var user = await context.ApplicationUsers
		.SingleAsync(user => user.Id == _fixture.Customer.Id);
		var item = await context.Items
		.Include(item => item.Sport)
		.SingleAsync(item => item.Id == _fixture.FootballBoots.Id);
		var purchase = _fixture.CreatePurchase(user, item, 2);
		
		context.Purchases.Add(purchase);
		await context.SaveChangesAsync();
		return purchase;
	}
	
	private static ClaimsPrincipal CreateUser(string? userId) => userId is null
	? new ClaimsPrincipal(): new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, userId) }, "UnitTest"));
}
