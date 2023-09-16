using Autofac.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Producer.Ioc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddMvc(options =>
{
	options.EnableEndpointRouting = false;
});

builder.Services.AddControllers().AddNewtonsoftJson(options =>
{
	options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
});


builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();

#region ========== Swagger and JWT Token ==========

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
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
			c.SwaggerEndpoint("/swagger/v1/swagger.json", "RABBITMQ PRODUCER REST API V1");
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

app.UseMvc();

app.Run();
