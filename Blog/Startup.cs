public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        // Cấu hình các middleware và luồng xử lý yêu cầu
        // Ví dụ: app.UseRouting(), app.UseEndpoints()
    }
}