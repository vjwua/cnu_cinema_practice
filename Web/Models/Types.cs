using System;
using System.Collections.Generic;

namespace cnu_cinema_practice.Models;

public class Movie
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public List<string> Genre { get; set; } = new();
    public double Rating { get; set; }
    public int Duration { get; set; }
    public string Poster { get; set; } = string.Empty;
    public string Backdrop { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string AgeLimit { get; set; } = string.Empty;
    public string Director { get; set; } = string.Empty;
    public List<string> Actors { get; set; } = new();
    public string Language { get; set; } = "Українська";
    public List<Session> Sessions { get; set; } = new();
}

public class Session
{
    public string Id { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty; // ISO date format for simplicity
    public string Time { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string HallName { get; set; } = string.Empty;
    public string HallType { get; set; } = "Standard";
    public List<string> OccupiedSeats { get; set; } = new();
}

public class Snack
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Icon { get; set; } = string.Empty;
}

public enum BookingStep
{
    Details,
    Seats,
    Snacks,
    Checkout,
    Ticket
}

public enum AppView
{
    Grid,
    Booking,
    Login,
    Cinemas
}
