using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration.Attributes;
using Serilog;

// http://hmsoftware.org/api/aa
// API that sits in front of poll_for_file.py
// API should write files into /home/dave/auto-archiver/poll-input
// which {guid}.json
//{ "url":"https://twitter.com/dave_mateer/status/1524341442738638848" }

// then send back the guid to the UI (as next step may take some minutes)

// python will then process

// POST on /api/aa
// GET on /api/aa/{guid} to get status and result json
// which will be
// 
//{
//    "cdn_url": "https://testhashing.fra1.cdn.digitaloceanspaces.com/web/asdfkjh-asdf/twitter__dave_mateer_status_1524341442738638848.html",
//    "screenshot": "https://testhashing.fra1.cdn.digitaloceanspaces.com/web/asdfkjh-asdf/twitter__dave_mateer_status_15243414427386388482022-05-25T06:11:49.240868.png",
//    "status": "twitter: success",
//    "thumbnail": "https://testhashing.fra1.cdn.digitaloceanspaces.com/web/asdfkjh-asdf/twitter__media_FSeMVBsWUAMDEun.jpg"
//}



var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();

var logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("logs/information.log")
    .CreateLogger();

builder.Logging.AddSerilog(logger);

var app = builder.Build();

logger.Information("****Starting Poll for file API - listening on http://hmsoftware.org/api/aa");

// returns text
app.MapGet("/textget", () => "hello from aa");

app.MapGet("/jsonget", () =>
   // this will serialise the object and returns json by default as is of type T
   // https://docs.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis?view=aspnetcore-6.0#responses
   new { Message = "hello world from jsonget" }
);

app.MapPost("/hs", Handler3);
async Task<IResult> Handler3(HSDto hsdtoIn)
{
    // csv helper to write inbound hsdto to a csv
    var recordsToWrite = new List<HSDto>();
    recordsToWrite.Add(hsdtoIn);

    var guid = Guid.NewGuid();

    var path = "/home/dave/hatespeech";
    await using (var writer = new StreamWriter($"{path}/input/{guid}.csv"))
    await using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
    {
        csv.WriteRecords(recordsToWrite);
    }

    // poll the output directory
    var outputFile = $"{path}/output/{guid}.csv";
    while (true)
    {
        if (File.Exists(outputFile))
        {
            var hsdto = new HSDto();

            // found output file, convert to json object
            using (var reader = new StreamReader(outputFile))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                var records = csv.GetRecords<PythonDTO>();
                foreach (var record in records)
                {
                    logger.Information($"Text: {record.Text} ");
                    logger.Information($"Prediction: {record.Prediction} ");
                    logger.Information($"Score: {record.HateScore} ");

                    hsdto.Text = record.Text;
                    hsdto.Score = record.HateScore;
                    hsdto.Prediction = record.Prediction;
                }
            }

            // clean up
            File.Delete(outputFile);
            return Results.Json(hsdto);
        }

        await Task.Delay(100);
    }
}

app.Run();

class PythonDTO
{
    public string Text { get; set; }
    public string Prediction { get; set; }
    [Name("Hate score")]
    public string HateScore { get; set; }
}

class HSDto
{
    public string Text { get; set; }
    public string? Score { get; set; }
    public string? Prediction { get; set; }
}

//class Todo
//{
//    public int Id { get; set; }
//    public string? Name { get; set; }
//    public bool IsComplete { get; set; }
//}

//class TodoDb : DbContext
//{
//    public TodoDb(DbContextOptions<TodoDb> options)
//        : base(options) { }

//    public DbSet<Todo> Todos => Set<Todo>();
//}









//var builder = WebApplication.CreateBuilder(args);
//var app = builder.Build();

////app.UseAuthentication();

//app.MapGet("/api/", () => "Hello World - api!");

//app.MapGet("/api/anon", () => "Anon endpoint2").AllowAnonymous();

//app.MapGet("/api/auth", () => "This endpoint requires authorization").RequireAuthorization();


//app.MapPost("/todoitems", async (Todo todo, TodoDb db) =>
//{
//    db.Todos.Add(todo);
//    await db.SaveChangesAsync();

//    return Results.Created($"/todoitems/{todo.Id}", todo);
//});

//app.Run();

//class TodoDb : DbContext
//{
//    public TodoDb(DbContextOptions<TodoDb> options)
//        : base(options) { }

//    public DbSet<Todo> Todos => Set<Todo>();
//}

//class Todo
//{
//    public int Id { get; set; }
//    public string? Name { get; set; }
//    public bool IsComplete { get; set; }
//}





//builder.Services.Configure<KestrelServerOptions>(options =>
//{
//    options.ConfigureHttpsDefaults(options =>
//        options.ClientCertificateMode = ClientCertificateMode.RequireCertificate);
//});

//builder.Services.AddAuthentication(
//        CertificateAuthenticationDefaults.AuthenticationScheme)
//    .AddCertificate(options =>
//    {
//        options.Events = new CertificateAuthenticationEvents
//        {
//            OnAuthenticationFailed = context =>
//              {
//                  return Task.CompletedTask;
//              },

//            OnCertificateValidated = context =>
//            {
//                var claims = new[]
//                {
//                    new Claim(
//                        ClaimTypes.NameIdentifier,
//                        context.ClientCertificate.Subject,
//                        ClaimValueTypes.String, context.Options.ClaimsIssuer),
//                    new Claim(
//                        ClaimTypes.Name,
//                        context.ClientCertificate.Subject,
//                        ClaimValueTypes.String, context.Options.ClaimsIssuer)
//                };

//                context.Principal = new ClaimsPrincipal(
//                    new ClaimsIdentity(claims, context.Scheme.Name));
//                context.Success();

//                return Task.CompletedTask;
//            }
//        };
//    });

//app.MapPut("/todoitems/{id}", async (int id, Todo inputTodo, TodoDb db) =>
//{
//    var todo = await db.Todos.FindAsync(id);

//    if (todo is null) return Results.NotFound();

//    todo.Name = inputTodo.Name;
//    todo.IsComplete = inputTodo.IsComplete;

//    await db.SaveChangesAsync();

//    return Results.NoContent();
//});

//app.MapDelete("/todoitems/{id}", async (int id, TodoDb db) =>
//{
//    if (await db.Todos.FindAsync(id) is Todo todo)
//    {
//        db.Todos.Remove(todo);
//        await db.SaveChangesAsync();
//        return Results.Ok(todo);
//    }

//    return Results.NotFound();
//});

//app.MapGet("/todoitems", async (TodoDb db) =>
//    await db.Todos.ToListAsync());

//app.MapGet("/todoitems/complete", async (TodoDb db) =>
//    await db.Todos.Where(t => t.IsComplete).ToListAsync());

//app.MapGet("/todoitems/{id}", async (int id, TodoDb db) =>
//    await db.Todos.FindAsync(id)
//        is Todo todo
//        ? Results.Ok(todo)
//        : Results.NotFound());


