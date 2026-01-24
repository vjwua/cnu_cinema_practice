using AutoMapper;
using cnu_cinema_practice.ViewModels;
using cnu_cinema_practice.ViewModels.Halls;
using Core.DTOs.Halls;
using Core.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace cnu_cinema_practice.Areas.Admin.Controllers.Admin;

[Area("Admin")]
public class HallController : Controller
{
    private readonly IHallService _hallService;
    private readonly IMapper _mapper;

    public HallController(IMapper mapper, IHallService hallService)
    {
        this._hallService = hallService;
        this._mapper = mapper;
    }

    public async Task<IActionResult> Index()
    {
        var halls = await _hallService.GetAllAsync();
        var viewModels = _mapper.Map<IEnumerable<HallListViewModel>>(halls);
        return View(viewModels);
    }

    public async Task<IActionResult> Details(int id)
    {
        bool exists = await _hallService.ExistsAsync(id);
        if (exists == false)
        {
            return NotFound();
        }

        var hall = await _hallService.GetByIdAsync(id);
        var details = _mapper.Map<HallDetailViewModel>(hall);
        return View(details);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateHallViewModel model)
    {
        var dto = _mapper.Map<CreateHallDTO>(model);
        var details = await _hallService.CreateAsync(dto);

        bool exists = await _hallService.ExistsAsync(details.Id);
        if (exists == false)
        {
            ModelState.AddModelError(string.Empty, "Could not create new Hall");
            return View(model);
        }
        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> Update(UpdateHallViewModel model)
    {
        bool exists = await _hallService.ExistsAsync(model.Id);
        if (exists == false)
        {
            ModelState.AddModelError(string.Empty, "Could not create new Hall");
            return View(model);
        }
        
        var dto = _mapper.Map<UpdateHallDTO>(model);
        await _hallService.UpdateHallInfo(dto);
        return RedirectToAction("Details", new { id = model.Id });
    }

    [HttpPost]
    public async Task Clear(UpdateHallViewModel model)
    {
        await _hallService.RemoveAllSeatsAsync(model.Id);
        RedirectToAction("Details", new { id = model.Id });
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        await _hallService.DeleteAsync(id);
        return RedirectToAction("Index");
    }
}