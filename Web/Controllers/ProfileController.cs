using AutoMapper;
using cnu_cinema_practice.ViewModels.Account;
using Core.Constants;
using Core.Enums;
using Core.Interfaces.Services;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using cnu_cinema_practice.Components.Pages.Profile;
using Microsoft.AspNetCore.Http.HttpResults;

namespace cnu_cinema_practice.Controllers;

[Authorize]
public class ProfileController(
    UserManager<ApplicationUser> userManager,
    IOrderService orderService,
    IMapper mapper) : Controller
{
    public async Task<IResult> Index(DateTime? from, DateTime? to, OrderStatus? status)
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null)
        {
            return Results.Redirect(Url.Action("Login", "Account")!);
        }

        var roles = await userManager.GetRolesAsync(user);

        var effectiveFrom = from ?? DateTime.Today;
        var effectiveTo = to ?? DateTime.Today.AddDays(7);

        var totalOrdersCount = await orderService.CountUserOrdersAsync(user.Id);
        var orderDtos =
            (await orderService.GetUserOrdersFilteredBySessionAsync(user.Id, effectiveFrom, effectiveTo, status))
            .ToList();
        var orderViewModels = mapper.Map<List<OrderViewModel>>(orderDtos);

        var viewModel = new ProfileViewModel
        {
            Email = user.Email ?? "",
            IsAdmin = roles.Contains(RoleNames.Admin),

            TotalOrdersCount = totalOrdersCount,

            FilterFrom = effectiveFrom,
            FilterTo = effectiveTo,
            FilterStatus = status,

            Orders = orderViewModels
        };

        return new RazorComponentResult<cnu_cinema_practice.Components.Pages.Profile.Profile>(new { Model = viewModel });
    }
}