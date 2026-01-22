using AutoMapper;
using Core.Mapping;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Tests.Helpers;

/// <summary>
/// AutoMapper Test Fixture для xUnit
/// ✅ Оновлено для AutoMapper 16.0.0 з NullLoggerFactory
/// </summary>
public class AutoMapperTestFixture
{
    public IMapper Mapper { get; }

    public AutoMapperTestFixture()
    {
        // ✅ AutoMapper 16.0.0 - використовуємо MapperConfigurationExpression
        var configExpression = new MapperConfigurationExpression();
        configExpression.AddProfile<MovieMapping>();
        // Додайте інші профілі маппінгу тут
        // configExpression.AddProfile<HallMapping>();
        // configExpression.AddProfile<SessionMapping>();

        // ✅ NullLoggerFactory.Instance замість null
        var config = new MapperConfiguration(configExpression, NullLoggerFactory.Instance);
        config.AssertConfigurationIsValid();
        
        Mapper = config.CreateMapper();
    }
}

// Приклад використання з реальним AutoMapper
public class AutoMapperTests : IClassFixture<AutoMapperTestFixture>
{
    private readonly IMapper _mapper;

    public AutoMapperTests(AutoMapperTestFixture fixture)
    {
        _mapper = fixture.Mapper;
    }
}