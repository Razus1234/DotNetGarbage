using DotNetGarbage;
using DotNetGarbage.Services;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

public class GarbageCollectionApiTests
{
    // ---------------------------------------------------------------------------------
    //  Test 1 – Service เก็บได้หลัง Request หมด Scope
    // ---------------------------------------------------------------------------------
    [Fact]
    public async Task HeavyService_Should_Be_Collected_After_Request()
    {
        WeakReference weakRef = default!;

        // 1) สร้าง Fake Service ที่เก็บ WeakReference ให้เรา
        var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.AddScoped<IHeavyService>(_ =>
                    {
                        var svc = new HeavyService();
                        weakRef = new WeakReference(svc);   // จับ instance
                        return svc;
                    });
                });
            });

        // 2) ✨ เรียก API หนึ่งครั้ง (สร้าง scope)
        var client = factory.CreateClient();
        var _ = await client.GetAsync("/mem/allocate");   // ทำให้บริการถูกสร้าง & ใช้งาน

        // 3) ✨ ปล่อย Strong ref + บังคับ GC
        client = null!;
        factory = null!;

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        Assert.False(weakRef.IsAlive); // => ผ่านถ้าเก็บได้
    }

    // ---------------------------------------------------------------------------------
    //  Test 2 – จำลอง Memory‑Leak ด้วย Static List แล้วพิสูจน์ว่าเก็บไม่ได้
    // ---------------------------------------------------------------------------------
    // static list ใน test‑project เพื่อจำลอง leak
    static readonly List<IHeavyService> Leaks = new();
    [Fact]
    public async Task HeavyService_Should_NOT_Be_Collected_When_Leaked()
    {
      
        WeakReference weakRef = default!;

        var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.AddScoped<IHeavyService>(_ =>
                    {
                        var svc = new HeavyService();
                        Leaks.Add(svc);              // ❌ ถือ ref ไว้ = leak
                        weakRef = new WeakReference(svc);
                        return svc;
                    });
                });
            });

        var client = factory.CreateClient();
        _ = await client.GetAsync("/mem/allocate");

        client = null!;
        factory = null!;

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        Assert.True(weakRef.IsAlive);                // ยังไม่ถูกเก็บ ==> red flag
        Leaks.Clear();                               // cleanup เพื่อไม่รั่วจริง ๆ เวลา run test ซ้ำ
    }

    // ---------------------------------------------------------------------------------
    //  Test 3 – วัด Memory ก่อน/หลัง เพื่อยืนยันว่าใช้แรมลดลงจริง
    // ---------------------------------------------------------------------------------
    [Fact]
    public async Task Memory_Usage_Should_Drop_After_GC()
    {
        long before, after;
        WeakReference weakRef = default!;

        var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.AddScoped<IHeavyService>(_ =>
                    {
                        var svc = new HeavyService();
                        weakRef = new WeakReference(svc);
                        return svc;
                    });
                });
            });

        var client = factory.CreateClient();
        await client.GetAsync("/mem/allocate");

        GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
        before = GC.GetTotalMemory(true);

        // ปล่อยทุก strong ref
        client = null!;
        factory = null!;

        GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
        after = GC.GetTotalMemory(true);

        Assert.False(weakRef.IsAlive);               // ถูกเก็บ
        Assert.True(after < before);                 // แรมลด (± jitter ได้)
    }
}
