using Consumer.Infrastructure.Handlers;
using Consumer.Ioc;
using Microsoft.OpenApi.Models;
using ServiceBus.Event;
using ServiceBus.RabbitMQ;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

#region ========== Swagger and JWT Token ==========

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
	c.SwaggerDoc("v1", new OpenApiInfo { Title = "Swagger RabbitMq", Version = "v1" });
	c.EnableAnnotations();
});

#endregion ========== Swagger and JWT Token ==========

// DI
IocConfig.Register(builder.Services, builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseDeveloperExceptionPage();

	app.UseSwagger(c =>
	{
		c.PreSerializeFilters.Add((document, request) =>
		{
			var paths = document.Paths.ToDictionary(item => item.Key.ToLowerInvariant(), item => item.Value);
			document.Paths.Clear();
			foreach (var pathItem in paths)
			{
				document.Paths.Add(pathItem.Key, pathItem.Value);
			}
			c.SerializeAsV2 = true;
		});

		app.UseSwaggerUI(c =>
		{
			c.SwaggerEndpoint("/swagger/v1/swagger.json", "RABBITMQ CONSUMER REST API V1");
		});
	});
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
	endpoints.MapControllers();
});

app.Run();

void ConfigureEventBus(IApplicationBuilder app)
{
	var eventBus = app.ApplicationServices.GetRequiredService<IEventBusRabbitMQ>();

	eventBus.Subscribe<EmailIntegrationEvent, EmailIntegrationHandler>();
}