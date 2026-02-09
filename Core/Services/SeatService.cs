using AutoMapper;
using Core.DTOs.Seats;
using Core.Entities;
using Core.Interfaces.Repositories;
using Core.Interfaces.Services;

namespace Core.Services;

public class SeatService : ISeatService
{
    private readonly IMapper _mapper;
    private readonly ISeatRepository _seatRepository;

    public SeatService(IMapper mapper, ISeatRepository seatRepo)
    {
        _mapper = mapper;
        _seatRepository = seatRepo;
    }

    public async Task<SeatDTO> GetByIdAsync(int id)
    {
        Seat? seat = await _seatRepository.GetByIdAsync(id);
        return _mapper.Map<SeatDTO>(seat);
    }

    public async Task<IEnumerable<SeatDTO>> GetBySessionIdAsync(int sessionId)
    {
        IEnumerable<Seat> seatsBySession = await _seatRepository.GetBySessionIdAsync(sessionId);
        return _mapper.Map<IEnumerable<SeatDTO>>(seatsBySession);
    }

    public async Task<IEnumerable<SeatDTO>> GetAvailableSeatsAsync(int sessionId)
    {
        IEnumerable<Seat> availiableSeats = await _seatRepository.GetAvailableSeatsAsync(sessionId);
        return _mapper.Map<IEnumerable<SeatDTO>>(availiableSeats);
    }

    public async Task<bool> ReserveSeatAsync(SeatDTO seat, int sessionId, decimal price, string? userId = null)
    {
        bool result = await _seatRepository.ReserveSeatAsync(seat.Id, sessionId, price, userId);
        return result;
    }

    public async Task<int?> GetReservationIdAsync(int seatId, int sessionId)
    {
        return await _seatRepository.GetReservationIdAsync(seatId, sessionId);
    }

    public async Task<bool> IsSeatAvailableAsync(SeatDTO seat, int sessionId)
    {
        bool result = await _seatRepository.IsSeatAvailableAsync(seat.Id, sessionId);
        return result;
    }

    public async Task<IEnumerable<SeatType>> GetSeatTypesAsync()
    {
        var seatTypes = await _seatRepository.GetSeatTypesAsync();
        return seatTypes;
    }
}