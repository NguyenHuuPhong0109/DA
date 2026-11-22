using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace DACN_HTQL.Services;

public class VangHenBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<VangHenBackgroundService> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromMinutes(5);

    public VangHenBackgroundService(IServiceScopeFactory scopeFactory, ILogger<VangHenBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("[Auto No-Show Background] Service đã khởi động. Chu kỳ quét: 5 phút/lần.");

        // Chờ 10 giây ban đầu sau khi app khởi động để tránh tranh chấp với khởi tạo DB
        await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var vangHenService = scope.ServiceProvider.GetRequiredService<VangHenService>();

                var (soVangHen, soNhacLich) = await vangHenService.QuetVaXuLyAsync(stoppingToken);

                if (soVangHen > 0 || soNhacLich > 0)
                {
                    _logger.LogInformation("[Auto No-Show Background] Hoàn tất quét: {SoVangHen} vắng hẹn được xử lý, {SoNhacLich} nhắc lịch được gửi.",
                        soVangHen, soNhacLich);
                }
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogError(ex, "[Auto No-Show Background] Gặp lỗi trong tiến trình quét ngầm.");
            }

            try
            {
                await Task.Delay(_interval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }

        _logger.LogInformation("[Auto No-Show Background] Service đã dừng.");
    }
}
