using System;
using System.Collections.Generic;

namespace cnu_cinema_practice.Models;

public static class MockData
{
    private static string Today => DateTime.Now.ToString("yyyy-MM-dd");
    private static string Tomorrow => DateTime.Now.AddDays(1).ToString("yyyy-MM-dd");

    public static List<Movie> Movies => new()
    {
        new Movie
        {
            Id = "m1",
            Title = "Дюна: Частина Друга",
            Genre = new List<string> { "Фантастика", "Екшн" },
            Rating = 9.1,
            Duration = 166,
            Poster = "https://images.unsplash.com/photo-1626814026160-2237a95fc5a0?q=80&w=600&auto=format&fit=crop",
            Backdrop = "https://images.unsplash.com/photo-1506466010722-395aa2bef877?q=80&w=1200&auto=format&fit=crop",
            Description = "Пол Атрідес об’єднується з Чані та фрименами, щоб помститися змовникам, які знищили його родину.",
            AgeLimit = "12+",
            Director = "Дені Вільньов",
            Actors = new List<string> { "Тімоті Шаламе", "Зендея", "Ребекка Фергюсон" },
            Language = "Українська",
            Sessions = new List<Session>
            {
                new Session { Id = "s1", Date = Today, Time = "12:00", Price = 160, HallName = "Зал 1", HallType = "Standard", OccupiedSeats = new List<string> { "1-5", "1-6" } },
                new Session { Id = "s2", Date = Today, Time = "18:30", Price = 250, HallName = "IMAX Laser", HallType = "IMAX", OccupiedSeats = new List<string> { "4-10", "4-11" } },
                new Session { Id = "s3", Date = Tomorrow, Time = "20:00", Price = 350, HallName = "VIP Lounge", HallType = "LUX", OccupiedSeats = new List<string>() }
            }
        },
        new Movie
        {
            Id = "m2",
            Title = "Повстання Штатів",
            Genre = new List<string> { "Екшн", "Драма" },
            Rating = 7.8,
            Duration = 109,
            Poster = "https://images.unsplash.com/photo-1594909122845-11baa439b7bf?q=80&w=600&auto=format&fit=crop",
            Backdrop = "https://images.unsplash.com/photo-1485846234645-a62644f84728?q=80&w=1200&auto=format&fit=crop",
            Description = "Подорож через розділену Америку майбутнього, яка балансує на межі виживання.",
            AgeLimit = "16+",
            Director = "Алекс Гарленд",
            Actors = new List<string> { "Кірстен Данст", "Вагнер Моура", "Кейлі Спені" },
            Language = "Українська",
            Sessions = new List<Session>
            {
                new Session { Id = "s4", Date = Today, Time = "15:45", Price = 180, HallName = "Зал 3", HallType = "Standard", OccupiedSeats = new List<string> { "2-2" } },
                new Session { Id = "s5", Date = Tomorrow, Time = "19:15", Price = 220, HallName = "Зал 4", HallType = "4DX", OccupiedSeats = new List<string>() }
            }
        }
    };

    public static List<Snack> Snacks => new()
    {
        new Snack { Id = "sn1", Name = "Попкорн солоний (L)", Price = 145, Icon = "🍿" },
        new Snack { Id = "sn2", Name = "Начос з сиром", Price = 120, Icon = "🌮" },
        new Snack { Id = "sn3", Name = "Pepsi 0.5л", Price = 65, Icon = "🥤" },
        new Snack { Id = "sn4", Name = "M&Ms Кріспі", Price = 85, Icon = "🍬" }
    };
}
