using System.Globalization;
using System.Text.Json;
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
app.MapGet("/api/aa", () => "hello from aa3");

// eg 7da1c3b5-1dd7-45b2-bf30-1b7f61d7e331
app.MapGet("/api/aa/{guid}", (Guid guid) =>
{
    // check if output file exists yet
    var path = "/home/dave/auto-archiver";
    var fileName = path + $"/poll-output/{guid}.json";
    if (File.Exists(fileName))
    {
        // if exists deserialise
        var json = File.ReadAllText(fileName);

        var aaDto = JsonSerializer.Deserialize<AADto>(json);
        return aaDto;
    }

    var notReady = new AADto
    {
        guid = guid,
        status = "processing"
    };
    return notReady;
});


app.MapPost("/api/aa", Handler3);
async Task<IResult> Handler3(AADto aadto)
{
    var guid = Guid.NewGuid();
    aadto.guid = guid;
    aadto.status = "in queue to be processed";
    string jsonString = JsonSerializer.Serialize(aadto);

    // write this json to disk so python app can pick it up
    var path = "/home/dave/auto-archiver";
    var fileName = path + $"/poll-input/{guid}.json";
    File.WriteAllText(fileName, jsonString);

    // csv helper to write inbound hsdto to a csv
    //var recordsToWrite = new List<AADto>();
    //recordsToWrite.Add(aadtoIn);


    //await using (var writer = new StreamWriter($"{path}/poll-input/{guid}.json"))
    //await using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
    //{
    //    csv.WriteRecords(recordsToWrite);
    //}

    //// poll the output directory
    //var outputFile = $"{path}/output/{guid}.csv";
    //while (true)
    //{
    //    if (File.Exists(outputFile))
    //    {
    //        var hsdto = new HSDto();

    //        // found output file, convert to json object
    //        using (var reader = new StreamReader(outputFile))
    //        using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
    //        {
    //            var records = csv.GetRecords<PythonDTO>();
    //            foreach (var record in records)
    //            {
    //                logger.Information($"Text: {record.Text} ");
    //                logger.Information($"Prediction: {record.Prediction} ");
    //                logger.Information($"Score: {record.HateScore} ");

    //                hsdto.Text = record.Text;
    //                hsdto.Score = record.HateScore;
    //                hsdto.Prediction = record.Prediction;
    //            }
    //        }

    //        // clean up
    //        File.Delete(outputFile);
    //        return Results.Json(hsdto);
    //    }


    //    await Task.Delay(100);
    //}
    return Results.Json(aadto);
}

app.Run();

//class PythonAADTO
//{
//    public Guid? guid { get; set; }
//    public string? url { get; set; }
//    public string? cdn_url { get; set; }
//    public string? screenshot { get; set; }
//    public string? thumbnail { get; set; }
//    public string status { get; set; }
//}

class AADto
{
    public Guid? guid { get; set; }
    public string? url { get; set; }
    public string? cdn_url { get; set; }
    public string? screenshot { get; set; }
    public string? thumbnail { get; set; }
    public string status { get; set; }
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


