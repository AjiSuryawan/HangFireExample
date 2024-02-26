using Hangfire;
using Hangfire.MySql;

var builder = WebApplication.CreateBuilder(args);
string Mysql_Url = builder.Configuration["HangfireMysqlProperties:Server"];
string MySql_Port = builder.Configuration["HangfireMysqlProperties:Port"];
string Mysql_Database = builder.Configuration["HangfireMysqlProperties:Database"];
string Mysql_User = builder.Configuration["HangfireMysqlProperties:User"];
string Mysql_Password = builder.Configuration["HangfireMysqlProperties:Password"];

// Add services to the container.
builder.Services.AddControllers();
//builder.Services.AddHangfire(config => config.UseSqlServerStorage("Data Source=DESKTOP-VU15N53;Initial Catalog=MovieContext;Integrated Security=True;TrustServerCertificate=true"));


builder.Services.AddHangfire(config =>
{
    // Configure Hangfire with MySQL storage
    config.UseStorage(new MySqlStorage($"server={Mysql_Url};port={MySql_Port};uid={Mysql_User};pwd={Mysql_Password};database={Mysql_Database};Allow User Variables=True", new MySqlStorageOptions
    {
        TransactionIsolationLevel = System.Transactions.IsolationLevel.ReadCommitted,   // 事务隔离级别
        QueuePollInterval = TimeSpan.FromSeconds(3),                                    // 作业队列轮询间隔
        JobExpirationCheckInterval = TimeSpan.FromHours(1),                             // 作业过期检查间隔(管理过期记录)
        CountersAggregateInterval = TimeSpan.FromMinutes(5),                            // 计数器统计间隔
        PrepareSchemaIfNecessary = true,                                                // 自动创建表
        DashboardJobListLimit = 50000,                                                  // 仪表盘显示作业限制
        TransactionTimeout = TimeSpan.FromMinutes(1),                                   // 事务超时时间
        TablesPrefix = "T_Hangfire",                                                    // hangfire表名前缀
        InvisibilityTimeout = TimeSpan.FromDays(1)                                      // 弃用属性，设定线程重开间隔
    }));

});


var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();
app.MapControllers();

// Enable Hangfire dashboard
app.UseHangfireDashboard();

// Configure Hangfire to run jobs in the background
app.UseHangfireServer();

// Schedule a background job to run every 5 seconds
//RecurringJob.AddOrUpdate(() => MyBackgroundJob.Execute(), "0 2 * * *");

app.Run();