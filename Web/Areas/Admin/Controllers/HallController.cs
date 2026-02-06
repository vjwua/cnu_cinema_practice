using AutoMapper;
using cnu_cinema_practice.ViewModels.Halls;
using Core.Constants;
using Core.DTOs.Halls;
using Core.DTOs.Seats;
using Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace cnu_cinema_practice.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = RoleNames.Admin)]
public class HallController(
    IHallService hallService,
    IMapper mapper) : Controller
{

    public async Task<IActionResult> Index()
    {
        var halls = await hallService.GetAllAsync();
        var viewModels = mapper.Map<IEnumerable<HallListViewModel>>(halls);
        return View(viewModels);
    }

    public IActionResult Create()
    {
        return View(new CreateHallViewModel());
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateHallViewModel model)
    {
        var dto = mapper.Map<CreateHallDTO>(model);
        dto.SeatLayout = new byte[dto.Rows, dto.Columns];
        var rowCount = dto.Rows;
        var colCount = dto.Columns;
        for (int i = 0; i < rowCount; i++)
        {
            for (int j = 0; j < colCount; j++)
            {
                dto.SeatLayout[i, j] = 1;
            }
        }
        var details = await hallService.CreateAsync(dto);

        bool exists = await hallService.ExistsAsync(details.Id);
        if (exists == false)
        {
            ModelState.AddModelError(string.Empty, "Could not create new Hall");
            //return View(model);
        }
        return RedirectToAction("Index");
    }

    public async Task<IActionResult> Edit(int id)
    {
        var dto = await hallService.GetByIdAsync(id);
        HallDetailViewModel viewmodel = mapper.Map<HallDetailViewModel>(dto);
        return View(viewmodel);
    }
    
    [HttpPost]
    public async Task<IActionResult> Edit(HallDetailViewModel model)
    {
        var existingDto = await hallService.GetByIdAsync(model.Id);
        if (model.Name != existingDto.Name)
        {
            await hallService.UpdateHallInfo(new UpdateHallDTO()
            {
                Id = model.Id,
                Name = model.Name,
            });
        }

        if ((byte)model.Rows != existingDto.Rows
            ||(byte)model.Columns != existingDto.Columns)
        {
            await hallService.UpdateHallDimensions(model.Id, model.Rows, model.Columns);
            byte[,] layout = new byte[model.Rows, model.Columns];
            var seats = (await hallService.GetSeatsByHall(model.Id)).ToList();
            var currentMaxRow = existingDto.Rows - 1;
            var currentMaxCol = existingDto.Columns - 1;
            foreach (var seat in seats)
            {
                if (seat.RowNum >= model.Rows
                    || seat.SeatNum >= model.Columns)
                {
                    //await hallService.SetSeatTypesAsync(seat.Id, 0);
                    await hallService.DeleteAsync(seat.Id);
                }
                else
                {
                    layout[seat.RowNum, seat.SeatNum] = (byte) seat.SeatTypeId;
                }
            }

            for (byte i = 0; i < model.Rows; i++)
            {
                for (byte j = 0; j < model.Columns; j++)
                {
                    if (layout[i, j] == 0)
                    {
                        await hallService.CreateSeatAsync(model.Id, new SeatDTO()
                        {
                            HallId = model.Id,
                            RowNum = i,
                            SeatNum = j,
                            SeatTypeId = 1,
                        });
                    }
                }
            }

            await hallService.SaveChangesAsync();
        }
        return RedirectToAction("Index");
    }
    
    [HttpPost]
    public IActionResult ToggleSeat(int id, string name, int r, int c, int rc, int cc, string ls)
    {
        byte row = (byte) r;
        byte col = (byte) c;
        UpdateHallViewModel vm = new UpdateHallViewModel()
        {
            SeatLayout = new byte[rc, cc],
            Name = name,
            Id = id,
        };
        for (int i = 0; i < rc; i++)
        {
            for (int j = 0; j < cc; j++)
            {
                vm.SeatLayout[i, j] = (byte)(ls[i * cc + j] - '0');
            }
        }
        vm.SeatLayout[row, col] = (byte)((vm.SeatLayout[row, col] + 1) % 4);
        return View("Layout", vm);
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        await hallService.DeleteAsync(id);
        return RedirectToAction("Index");
    }

    public async Task<IActionResult> Layout(int id)
    {
        var dto = await hallService.GetByIdAsync(id);

        var viewmodel = new UpdateHallViewModel();
        var seats = await hallService.GetSeatsByHall(id);
        byte[,] layout = new byte[dto.Rows, dto.Columns];
        foreach (var seat in seats)
        {
            var r = seat.RowNum;
            var c = seat.SeatNum;
            if (r < dto.Rows && c < dto.Columns) layout[r, c] = (byte)seat.SeatTypeId;
        }

        viewmodel.SeatLayout = layout;
        viewmodel.Id = dto.Id;
        viewmodel.Name = dto.Name;
        return View(viewmodel);
    }

    [HttpPost]
    public IActionResult ResetLayout(int id, string name, int r, int c)
    {
        var viewmodel = new UpdateHallViewModel()
        {
            Id = id,
            Name = name,
            SeatLayout = new byte[r, c],
        };
        for (int i = 0; i < r; i++)
        {
            for (int j = 0; j < c; j++)
            {
                viewmodel.SeatLayout[i, j] = 1;
            }
        }

        return View("Layout", viewmodel);
    }

    [HttpPost]
    public async Task<IActionResult> UpdateLayout(int id, int r, int c, string ls)
    {
        await hallService.UpdateLayout(id, r, c, ls);
        return RedirectToAction("Index");
    }
}