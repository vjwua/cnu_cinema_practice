using AutoMapper;
using Core.DTOs.Halls;
using Core.Entities;
using Core.Enums;
using Core.Interfaces.Repositories;
using Core.Interfaces.Services;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Infrastructure.Repositories.Interfaces;


namespace Core.Services;

public class HallService : IHallService
{
    private readonly IMapper _mapper;
    private readonly IHallRepository _hallRepository;

    public HallService(IMapper mapper, IHallRepository hallRepo, ISeatRepository seatRepo)
    {
        this._mapper = mapper;
        this._hallRepository = hallRepo;
    }

    public async Task<IEnumerable<HallListDTO>> GetAllAsync()
    {
        var halls = await _hallRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<HallListDTO>>(halls);
    }

    public async Task<HallDetailDTO> GetByIdAsync(int hallId)
    {
        var hall = await _hallRepository.GetByIdAsync(hallId);
        return _mapper.Map<HallDetailDTO>(hall);
    }

    public async Task<HallDetailDTO> CreateAsync(CreateHallDTO hallInfo)
    {
        var hall = await _hallRepository.CreateAsync(_mapper.Map<Hall>(hallInfo));
        await _hallRepository.CreateSeatsAsync(hall.Id, hallInfo.SeatLayout);
        return _mapper.Map<HallDetailDTO>(hall);
    }

    public async Task UpdateHallInfo(UpdateHallDTO hallInfo)
    {
        if (hallInfo.Name != null) await _hallRepository.UpdateNameAsync(hallInfo.Id, hallInfo.Name);
        if (hallInfo.SeatLayout != null) await _hallRepository.UpdateSeatLayoutAsync(hallInfo.Id, hallInfo.SeatLayout);
    }

    public async Task RemoveAllSeatsAsync(int hallId)
    {
        await _hallRepository.RemoveAllSeatsAsync(hallId);
    }

    public async Task<int> GetSeatCountAsync(int hallId)
    {
        int seatCount = await _hallRepository.GetSeatCountAsync(hallId);
        return seatCount;
    }

    public async Task<bool> ExistsAsync(int hallId)
    {
        bool exists = await _hallRepository.ExistsAsync(hallId);
        return exists;
    }

    public async Task DeleteAsync(int hallId)
    {
        await _hallRepository.DeleteAsync(hallId);
    }
}