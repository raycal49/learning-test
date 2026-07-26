using IncidentMonitor.Application.Interfaces;
using IncidentMonitor.Infrastructure.Messaging;
using IncidentMonitor.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Testcontainers.PostgreSql;

namespace IncidentMonitor.IntegrationTests;

public sealed class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("test_db")
        .WithUsername("test_user")
        .WithPassword("test_pass")
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove real DbContext
            var dbDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (dbDescriptor != null)
                services.Remove(dbDescriptor);

            // Add test DbContext pointing at the container
            services.AddDbContext<AppDbContext>(opts =>
                opts.UseNpgsql(_postgres.GetConnectionString()));

            // Replace RabbitMQ publisher with a mock so tests don't need RabbitMQ
            var publisherDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(IMessagePublisher));
            if (publisherDescriptor != null)
                services.Remove(publisherDescriptor);

            var mockPublisher = new Mock<IMessagePublisher>();
            mockPublisher
                .Setup(p => p.PublishAsync(
                    It.IsAny<string>(),
                    It.IsAny<object>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            services.AddScoped<IMessagePublisher>(_ => mockPublisher.Object);

            // Remove RabbitMQ connection factory (not needed in tests)
            var rabbitDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(RabbitMqConnectionFactory));
            if (rabbitDescriptor != null)
                services.Remove(rabbitDescriptor);
        });
    }

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        // Run migrations against the test container
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();
    }

    public new async Task DisposeAsync()
    {
        await _postgres.StopAsync();
    }
}