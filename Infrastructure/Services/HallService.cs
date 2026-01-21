using Microsoft.AspNetCore.Mvc.Controllers;
using AutoMapper;
using Core.DTOs.Halls;
using Core.Entities;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Infrastructure.Services;

public class HallService
{
    private readonly IMapper _mapper;
    private HallRepository hallRepo;

    HallService(IMapper mapper, CinemaDbContext con)
    {
        this._mapper = mapper;
        this.hallRepo = new HallRepository(con);
    }
    
    public async Task<List<HallListDTO>> GetHallNamesAsync()
    {
        var hallList = await hallRepo.GetAllAsync();
        return hallList.Select(hall => _mapper.Map<HallListDTO>(hall)).ToList();
    }

    public async Task<HallDetailDTO> GetHallDetailsAsync(int id)
    {
        bool hallExists = await hallRepo.ExistsAsync(id);
        HallDetailDTO details = new HallDetailDTO();
        if (hallExists)
        {
            Hall hall = await hallRepo.GetByIdAsync(id);
            details = _mapper.Map<HallDetailDTO>(hall);
        }
        return details;
    }
}
