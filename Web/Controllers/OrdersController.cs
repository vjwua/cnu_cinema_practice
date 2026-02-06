using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace cnu_cinema_practice.Controllers;

[Authorize]
public class OrdersController : Controller
{
    // Details page UI will be implemented later.
    [HttpGet("/orders/{id:int}")]
    public IActionResult Details(int id)
    {
        return StatusCode(StatusCodes.Status501NotImplemented,
            "Order details page is not implemented yet.");
    }
}