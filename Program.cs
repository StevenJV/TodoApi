using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<TodoDb>(opt => opt.UseInMemoryDatabase("TodoList"));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
var app = builder.Build();
if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI();
}
app.UseHttpsRedirection();

app.MapGet("/", () => "API for ToDo items.");

app.MapGet("/todoitems", async (TodoDb db) =>
    await db.Todos.Select(x => new TodoItemDTO(x)).ToListAsync());

app.MapGet("/todoitems/complete", async (TodoDb db) =>
    await db.Todos.Select(x => new TodoItemDTO(x)).ToListAsync());

app.MapGet("/todoitems/{id}", async (int id, TodoDb db) =>
    await db.Todos.FindAsync(id)
        is Todo todo
            ? Results.Ok(new TodoItemDTO(todo))
            : Results.NotFound());

app.MapPost("/todoitems", async (TodoItemDTO todoItemDTO, TodoDb db) =>
{
  var todoItem = new Todo
  {
    IsComplete = todoItemDTO.IsComplete,
    Name = todoItemDTO.Name
  };

  db.Todos.Add(todoItem);
  await db.SaveChangesAsync();

  return Results.Created($"/todoitems/{todoItem.Id}", new TodoItemDTO(todoItem));
});

app.MapPut("/todoitems/{id}", async (int id, TodoItemDTO todoItemDTO, TodoDb db) =>
{
  var todo = await db.Todos.FindAsync(id);

  if (todo is null) return Results.NotFound();

  todo.Name = todoItemDTO.Name;
  todo.IsComplete = todoItemDTO.IsComplete;

  await db.SaveChangesAsync();

  return Results.NoContent();
});

app.MapDelete("/todoitems/{id}", async (int id, TodoDb db) =>
{
  if (await db.Todos.FindAsync(id) is Todo todo)
  {
    db.Todos.Remove(todo);
    await db.SaveChangesAsync();
    return Results.Ok(new TodoItemDTO(todo));
  }

  return Results.NotFound();
});

app.Run();

public class Todo
{
  public int Id { get; set; }
  public string? Name { get; set; }
  public bool IsComplete { get; set; }
  public string? Secret { get; set; }
}

class TodoDb : DbContext
{
  public TodoDb(DbContextOptions<TodoDb> options)
      : base(options) { }

  public DbSet<Todo> Todos => Set<Todo>();
}

public class TodoItemDTO
{
  public int Id { get; set; }
  public string? Name { get; set; }
  public bool IsComplete { get; set; }


  public TodoItemDTO(Todo todoItem) =>
  (Id, Name, IsComplete) = (todoItem.Id, todoItem.Name, todoItem.IsComplete);
}
