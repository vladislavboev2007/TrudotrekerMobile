using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LaborTracker.Models;
using Microsoft.Data.Sqlite;
using Microsoft.Maui.Storage;

namespace LaborTracker.Services
{
    public class DatabaseService : IDisposable
    {
        private readonly string _databasePath;
        private SqliteConnection? _connection;

        public event Action<string>? ConnectionStatusChanged;

        public DatabaseService()
        {
            // Инициализируем путь к БД в зависимости от платформы
            _databasePath = GetDatabasePath();

            LogMessage($"📁 Используется база данных: {_databasePath}");
            LogMessage($"📁 Файл существует: {File.Exists(_databasePath)}");

            if (File.Exists(_databasePath))
            {
                LogMessage($"📁 Размер файла: {new FileInfo(_databasePath).Length} байт");
            }
            else
            {
                LogMessage($"⚠️ Файл БД не найден. Будет создана новая база.");
            }

            InitializeDatabase();
        }

        private string GetDatabasePath()
        {
#if ANDROID
            // Для Android используем локальную папку приложения
            var androidPath = Path.Combine(FileSystem.AppDataDirectory, "labortracker.db");
            LogMessage($"📱 Android AppDataDirectory: {FileSystem.AppDataDirectory}");

            // Если БД уже существует в локальной папке, используем её
            if (File.Exists(androidPath))
            {
                LogMessage($"✅ Найдена локальная БД на Android");
                return androidPath;
            }

            // Пытаемся скопировать из ресурсов Android (raw)
            try
            {
                CopyDatabaseFromAndroidResources(androidPath);
                if (File.Exists(androidPath))
                {
                    LogMessage($"✅ БД скопирована из ресурсов Android в: {androidPath}");
                    return androidPath;
                }
            }
            catch (Exception ex)
            {
                LogMessage($"⚠️ Не удалось скопировать БД из ресурсов Android: {ex.Message}");
            }

            LogMessage($"📝 Создаем новую БД на Android: {androidPath}");
            return androidPath;

#else
    // Для Windows используем путь рядом с исполняемым файлом
    var defaultPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "LaborTracker.db");
    LogMessage($"📝 Используется БД для Windows: {defaultPath}");
    return defaultPath;
#endif
        }

#if ANDROID
private void CopyDatabaseFromAndroidResources(string destinationPath)
{
    try
    {
        var context = Android.App.Application.Context;
        
        // Правильный ID ресурса
        int resourceId = context.Resources.GetIdentifier(
            "labortracker",        // имя ресурса
            "raw",                 // тип ресурса
            context.PackageName    // пакет
        );
        
        LogMessage($"🔍 Resource ID для labortracker: {resourceId}");
        
        if (resourceId > 0)
        {
            using var stream = context.Resources?.OpenRawResource(resourceId);
            if (stream != null)
            {
                using var fs = new FileStream(destinationPath, FileMode.Create);
                stream.CopyTo(fs);
                LogMessage($"✅ БД скопирована из ресурсов Android в: {destinationPath}");
                return;
            }
        }
        else
        {
            LogMessage($"⚠️ Ресурс 'labortracker' не найден в raw");
            
            // Попробуем альтернативный путь - из Assets
            try
            {
                using var assetStream = context.Assets?.Open("labortracker.db");
                if (assetStream != null)
                {
                    using var fs = new FileStream(destinationPath, FileMode.Create);
                    assetStream.CopyTo(fs);
                    LogMessage($"✅ БД скопирована из Assets");
                }
            }
            catch (Exception assetEx)
            {
                LogMessage($"⚠️ БД не найдена в Assets: {assetEx.Message}");
            }
        }
    }
    catch (Exception ex)
    {
        LogMessage($"❌ Ошибка копирования БД из ресурсов: {ex.Message}");
    }
}
#endif


#if ANDROID
        private void CopyDatabaseFromAssets(string destinationPath)
        {
            try
            {
                // Используем Android Assets для доступа к ресурсам
                using var stream = Android.App.Application.Context.Assets?.Open("LaborTracker.db");
                if (stream != null)
                {
                    using var fs = new FileStream(destinationPath, FileMode.Create);
                    stream.CopyTo(fs);
                    stream.Close();
                }
            }
            catch (Java.IO.FileNotFoundException)
            {
                LogMessage("📁 БД не найдена в Assets, будет создана новая");
            }
            catch (Exception ex)
            {
                LogMessage($"⚠️ Ошибка копирования БД: {ex.Message}");
            }
        }
#endif

        private void InitializeDatabase()
        {
            try
            {
                LogMessage("🔄 Инициализация базы данных...");

                // Создаем директорию, если её нет
                var directory = Path.GetDirectoryName(_databasePath);
                if (!Directory.Exists(directory) && directory != null)
                {
                    Directory.CreateDirectory(directory);
                    LogMessage($"📁 Создана директория: {directory}");
                }

                using var connection = new SqliteConnection($"Data Source={_databasePath}");
                connection.Open();
                LogMessage($"✅ Соединение с БД открыто: {connection.State}");

                // Создание таблицы Employee
                var createEmployeeTable = @"
                    CREATE TABLE IF NOT EXISTS Employee (
                        empId INTEGER PRIMARY KEY AUTOINCREMENT,
                        FIO TEXT NOT NULL
                    )";

                using (var command = new SqliteCommand(createEmployeeTable, connection))
                {
                    command.ExecuteNonQuery();
                    LogMessage("✅ Таблица Employee создана/проверена");
                }

                // Создание таблицы Task
                var createTaskTable = @"
                    CREATE TABLE IF NOT EXISTS Task (
                        taskId INTEGER PRIMARY KEY AUTOINCREMENT,
                        name TEXT NOT NULL,
                        description TEXT,
                        start TEXT,
                        final TEXT,
                        date TEXT,
                        empId INTEGER NOT NULL,
                        FOREIGN KEY (empId) REFERENCES Employee(empId) ON DELETE CASCADE
                    )";

                using (var command = new SqliteCommand(createTaskTable, connection))
                {
                    command.ExecuteNonQuery();
                    LogMessage("✅ Таблица Task создана/проверена");
                }

                // Проверяем данные в таблицах
                CheckExistingData(connection);

                // Загружаем тестовые данные, если таблицы пустые
                LoadTestDataIfNeeded(connection).Wait();

                LogMessage($"✅ База данных успешно инициализирована: {_databasePath}");
            }
            catch (Exception ex)
            {
                LogMessage($"❌ КРИТИЧЕСКАЯ ОШИБКА инициализации БД: {ex.Message}");
                LogMessage($"❌ StackTrace: {ex.StackTrace}");
                throw;
            }
        }

        private void CheckExistingData(SqliteConnection connection)
        {
            try
            {
                // Проверяем сотрудников
                var checkEmployees = new SqliteCommand("SELECT COUNT(*) FROM Employee", connection);
                var employeeCount = Convert.ToInt32(checkEmployees.ExecuteScalar());
                LogMessage($"📊 В таблице Employee: {employeeCount} записей");

                if (employeeCount > 0)
                {
                    var getEmployees = new SqliteCommand("SELECT empId, FIO FROM Employee", connection);
                    using var reader = getEmployees.ExecuteReader();
                    int counter = 0;
                    while (reader.Read())
                    {
                        LogMessage($"   {counter + 1}. ID: {reader.GetInt32(0)}, ФИО: {SafeGetString(reader, 1)}");
                        counter++;
                    }
                }

                // Проверяем задачи
                var checkTasks = new SqliteCommand("SELECT COUNT(*) FROM Task", connection);
                var taskCount = Convert.ToInt32(checkTasks.ExecuteScalar());
                LogMessage($"📊 В таблице Task: {taskCount} записей");

                if (taskCount > 0)
                {
                    var getTasks = new SqliteCommand(
                        "SELECT t.taskId, t.name, e.FIO FROM Task t LEFT JOIN Employee e ON t.empId = e.empId",
                        connection);
                    using var reader = getTasks.ExecuteReader();
                    int counter = 0;
                    while (reader.Read())
                    {
                        var employeeName = reader.IsDBNull(2) ? "Не назначен" : SafeGetString(reader, 2);
                        LogMessage($"   {counter + 1}. ID: {reader.GetInt32(0)}, Название: {SafeGetString(reader, 1)}, Исполнитель: {employeeName}");
                        counter++;
                    }
                }
            }
            catch (Exception ex)
            {
                LogMessage($"⚠️ Ошибка проверки данных: {ex.Message}");
            }
        }

        private async Task LoadTestDataIfNeeded(SqliteConnection connection)
        {
            try
            {
                var checkEmployees = new SqliteCommand("SELECT COUNT(*) FROM Employee", connection);
                var employeeCount = Convert.ToInt32(checkEmployees.ExecuteScalar());

                if (employeeCount == 0)
                {
                    LogMessage("📥 Загрузка тестовых данных...");

                    // Добавляем тестовых сотрудников
                    var testEmployees = new[]
                    {
                        new Employee { FIO = "Колпаков Матвей Николаевич" },
                        new Employee { FIO = "Петухов Кирилл Вячеславович" },
                        new Employee { FIO = "Смирнова Анна Владимировна" }
                    };

                    foreach (var employee in testEmployees)
                    {
                        await AddEmployeeAsync(employee);
                    }

                    // Добавляем тестовые задачи
                    var task1 = new TaskItem
                    {
                        Name = "Разработка интерфейса",
                        Description = "Создание главного экрана приложения",
                        Date = DateTime.Now.AddDays(-2),
                        EmpId = 1
                    };
                    task1.Start = TimeSpan.FromHours(9);
                    task1.Final = TimeSpan.FromHours(12).Add(TimeSpan.FromMinutes(30));
                    await AddTaskAsync(task1);

                    var task2 = new TaskItem
                    {
                        Name = "Тестирование функционала",
                        Description = "Проверка работы базы данных",
                        Date = DateTime.Now.AddDays(-1),
                        EmpId = 2
                    };
                    task2.Start = TimeSpan.FromHours(10);
                    await AddTaskAsync(task2);

                    var task3 = new TaskItem
                    {
                        Name = "Документация API",
                        Description = "Написание документации для REST API",
                        Date = DateTime.Now,
                        EmpId = 3
                    };
                    await AddTaskAsync(task3);

                    LogMessage("✅ Тестовые данные загружены");
                }
                else
                {
                    LogMessage($"📊 В базе уже есть {employeeCount} сотрудников");
                }
            }
            catch (Exception ex)
            {
                LogMessage($"⚠️ Ошибка загрузки тестовых данных: {ex.Message}");
            }
        }

        private SqliteConnection GetConnection()
        {
            if (_connection == null)
            {
                _connection = new SqliteConnection($"Data Source={_databasePath}");
                _connection.Open();
                LogMessage($"🔌 Создано новое соединение с БД. State: {_connection.State}");
            }
            else if (_connection.State != System.Data.ConnectionState.Open)
            {
                _connection.Open();
                LogMessage($"🔌 Переоткрыто соединение с БД. State: {_connection.State}");
            }

            return _connection;
        }

        public async Task<List<Employee>> GetEmployeesAsync()
        {
            var employees = new List<Employee>();

            try
            {
                LogMessage("🔄 GetEmployeesAsync: Начало загрузки сотрудников");

                var connection = GetConnection();
                LogMessage($"🔄 GetEmployeesAsync: Соединение: {connection.State}");

                var command = new SqliteCommand(
                    "SELECT empId, FIO FROM Employee ORDER BY empId",
                    connection);

                using var reader = await command.ExecuteReaderAsync();
                LogMessage($"🔄 GetEmployeesAsync: Reader создан. HasRows: {reader.HasRows}");

                int count = 0;
                while (await reader.ReadAsync())
                {
                    var employee = new Employee
                    {
                        EmpId = reader.GetInt32(0),
                        FIO = SafeGetString(reader, 1)
                    };

                    LogMessage($"✅ Сотрудник #{count + 1}: {employee.FIO}");

                    employees.Add(employee);
                    count++;
                }

                LogMessage($"✅ GetEmployeesAsync: Загружено {count} сотрудников");
                return employees;
            }
            catch (Exception ex)
            {
                LogMessage($"❌ Ошибка GetEmployeesAsync: {ex.Message}");
                LogMessage($"❌ StackTrace: {ex.StackTrace}");
                return employees;
            }
        }

        public async Task<List<TaskItem>> GetTasksAsync()
        {
            var tasks = new List<TaskItem>();

            try
            {
                LogMessage("🔄 GetTasksAsync: Начало загрузки задач");

                var connection = GetConnection();
                LogMessage($"🔄 GetTasksAsync: Соединение: {connection.State}");

                var sql = @"
                    SELECT t.taskId, t.name, t.description, 
                           t.start, t.final, t.date, 
                           t.empId, COALESCE(e.FIO, 'Не назначен') as employee_name
                    FROM Task t
                    LEFT JOIN Employee e ON t.empId = e.empId
                    ORDER BY t.date DESC, t.taskId DESC";

                LogMessage($"🔍 Выполняем SQL: {sql}");

                var command = new SqliteCommand(sql, connection);

                using var reader = await command.ExecuteReaderAsync();
                LogMessage($"🔄 GetTasksAsync: Reader создан. HasRows: {reader.HasRows}");

                int count = 0;
                while (await reader.ReadAsync())
                {
                    LogMessage($"🔍 Чтение записи #{count + 1}");

                    var task = new TaskItem
                    {
                        TaskId = reader.GetInt32(0),
                        Name = SafeGetString(reader, 1),
                        Description = SafeGetString(reader, 2),
                        EmpId = reader.GetInt32(6),
                        EmployeeName = SafeGetString(reader, 7)
                    };

                    LogMessage($"✅ Загружена задача: {task.Name}, ID: {task.TaskId}");

                    // Обработка времени
                    if (!reader.IsDBNull(3))
                    {
                        var startStr = reader.GetString(3);
                        if (TimeSpan.TryParse(startStr, out var startTime))
                            task.Start = startTime;
                    }

                    if (!reader.IsDBNull(4))
                    {
                        var finalStr = reader.GetString(4);
                        if (TimeSpan.TryParse(finalStr, out var finalTime))
                            task.Final = finalTime;
                    }

                    if (!reader.IsDBNull(5))
                    {
                        var dateStr = reader.GetString(5);
                        if (DateTime.TryParse(dateStr, out var date))
                            task.Date = date;
                        else
                            task.Date = DateTime.Now.Date;
                    }
                    else
                    {
                        task.Date = DateTime.Now.Date;
                    }

                    // Автоматически обновляем статус
                    if (task.Start.HasValue && task.Final.HasValue)
                    {
                        task.Status = "Выполнена";
                    }
                    else if (task.Start.HasValue && !task.Final.HasValue)
                    {
                        task.Status = "Выполняется";
                    }
                    else
                    {
                        task.Status = "Не начата";
                    }

                    tasks.Add(task);
                    count++;
                }

                LogMessage($"✅ GetTasksAsync: Загружено {count} задач");
                return tasks;
            }
            catch (Exception ex)
            {
                LogMessage($"❌ ОШИБКА GetTasksAsync: {ex.Message}");
                LogMessage($"❌ StackTrace: {ex.StackTrace}");
                return tasks;
            }
        }

        // Вспомогательный метод для безопасного чтения строк
        private string SafeGetString(SqliteDataReader reader, int colIndex)
        {
            try
            {
                if (!reader.IsDBNull(colIndex))
                {
                    var value = reader.GetString(colIndex);
                    // Удаляем NULL-символы и другие непечатаемые символы
                    if (!string.IsNullOrEmpty(value))
                    {
                        // Удаляем символы \0 и другие управляющие символы
                        var cleaned = new string(value.Where(c => !char.IsControl(c) || c == '\n' || c == '\r' || c == '\t').ToArray());
                        return cleaned.Trim();
                    }
                }
            }
            catch (Exception ex)
            {
                LogMessage($"⚠️ Ошибка чтения строки из колонки {colIndex}: {ex.Message}");
            }
            return string.Empty;
        }

        public async Task<bool> AddTaskAsync(TaskItem task)
        {
            try
            {
                var connection = GetConnection();
                var sql = @"
                    INSERT INTO Task 
                    (name, description, start, final, date, empId)
                    VALUES (@name, @description, @start, @final, @date, @empId)";

                var command = new SqliteCommand(sql, connection);

                command.Parameters.AddWithValue("@name", task.Name);
                command.Parameters.AddWithValue("@description",
                    string.IsNullOrEmpty(task.Description) ? (object)DBNull.Value : task.Description);
                command.Parameters.AddWithValue("@date", task.Date.ToString("yyyy-MM-dd"));
                command.Parameters.AddWithValue("@empId", task.EmpId);

                if (task.Start.HasValue)
                    command.Parameters.AddWithValue("@start", task.Start.Value.ToString(@"hh\:mm\:ss"));
                else
                    command.Parameters.AddWithValue("@start", DBNull.Value);

                if (task.Final.HasValue)
                    command.Parameters.AddWithValue("@final", task.Final.Value.ToString(@"hh\:mm\:ss"));
                else
                    command.Parameters.AddWithValue("@final", DBNull.Value);

                var result = await command.ExecuteNonQueryAsync();

                if (result > 0)
                {
                    LogMessage($"✅ Задача '{task.Name}' добавлена в БД");
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                LogMessage($"❌ Ошибка добавления задачи: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> AddEmployeeAsync(Employee employee)
        {
            try
            {
                var connection = GetConnection();
                var sql = @"
                    INSERT INTO Employee (FIO)
                    VALUES (@fio)";

                var command = new SqliteCommand(sql, connection);
                command.Parameters.AddWithValue("@fio", employee.FIO);

                var result = await command.ExecuteNonQueryAsync();

                if (result > 0)
                {
                    LogMessage($"✅ Сотрудник '{employee.FIO}' добавлен в БД");
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                LogMessage($"❌ Ошибка добавления сотрудника: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> CheckConnectionAsync()
        {
            try
            {
                var connection = GetConnection();
                var command = new SqliteCommand("SELECT 1", connection);
                var result = await command.ExecuteScalarAsync();

                LogMessage("✅ Соединение с базой данных успешно установлено");
                return true;
            }
            catch (Exception ex)
            {
                LogMessage($"❌ Ошибка подключения к БД: {ex.Message}");
                return false;
            }
        }

        public async Task<(int total, int completed, TimeSpan totalTime)> GetStatisticsAsync()
        {
            try
            {
                var connection = GetConnection();
                var sql = @"
                    SELECT 
                        COUNT(*) as total,
                        SUM(CASE WHEN final IS NOT NULL THEN 1 ELSE 0 END) as completed,
                        SUM(CASE WHEN final IS NOT NULL AND
                                      start IS NOT NULL 
                                 THEN (strftime('%s', final) - strftime('%s', start)) 
                                 ELSE 0 END) as total_time_seconds
                    FROM Task";

                var command = new SqliteCommand(sql, connection);

                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    var total = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
                    var completed = reader.IsDBNull(1) ? 0 : reader.GetInt32(1);
                    var totalSeconds = reader.IsDBNull(2) ? 0 : reader.GetInt64(2);
                    var totalTime = TimeSpan.FromSeconds(totalSeconds);

                    LogMessage($"📊 Статистика: {total} задач, {completed} завершено, время: {totalTime:hh\\:mm}");

                    return (total, completed, totalTime);
                }

                return (0, 0, TimeSpan.Zero);
            }
            catch (Exception ex)
            {
                LogMessage($"❌ Ошибка получения статистики: {ex.Message}");
                return (0, 0, TimeSpan.Zero);
            }
        }

        public async Task<bool> UpdateTaskAsync(TaskItem task)
        {
            try
            {
                var connection = GetConnection();
                var sql = @"
                    UPDATE Task 
                    SET name = @name, 
                        description = @description, 
                        start = @start, 
                        final = @final, 
                        date = @date, 
                        empId = @empId
                    WHERE taskId = @taskId";

                var command = new SqliteCommand(sql, connection);

                command.Parameters.AddWithValue("@taskId", task.TaskId);
                command.Parameters.AddWithValue("@name", task.Name);
                command.Parameters.AddWithValue("@description",
                    string.IsNullOrEmpty(task.Description) ? (object)DBNull.Value : task.Description);
                command.Parameters.AddWithValue("@date", task.Date.ToString("yyyy-MM-dd"));
                command.Parameters.AddWithValue("@empId", task.EmpId);

                if (task.Start.HasValue)
                    command.Parameters.AddWithValue("@start", task.Start.Value.ToString(@"hh\:mm\:ss"));
                else
                    command.Parameters.AddWithValue("@start", DBNull.Value);

                if (task.Final.HasValue)
                    command.Parameters.AddWithValue("@final", task.Final.Value.ToString(@"hh\:mm\:ss"));
                else
                    command.Parameters.AddWithValue("@final", DBNull.Value);

                var result = await command.ExecuteNonQueryAsync();

                if (result > 0)
                {
                    LogMessage($"✅ Задача '{task.Name}' обновлена в БД");
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                LogMessage($"❌ Ошибка обновления задачи: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteEmployeeAsync(int empId)
        {
            try
            {
                var connection = GetConnection();

                // Удаляем задачи сотрудника
                var deleteTasksSql = "DELETE FROM Task WHERE empId = @empId";
                using (var taskCommand = new SqliteCommand(deleteTasksSql, connection))
                {
                    taskCommand.Parameters.AddWithValue("@empId", empId);
                    await taskCommand.ExecuteNonQueryAsync();
                }

                // Удаляем сотрудника
                var deleteEmployeeSql = "DELETE FROM Employee WHERE empId = @empId";
                using (var employeeCommand = new SqliteCommand(deleteEmployeeSql, connection))
                {
                    employeeCommand.Parameters.AddWithValue("@empId", empId);
                    var result = await employeeCommand.ExecuteNonQueryAsync();

                    if (result > 0)
                    {
                        LogMessage($"✅ Сотрудник с ID {empId} удален из БД");
                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                LogMessage($"❌ Ошибка удаления сотрудника: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteTaskAsync(int taskId)
        {
            try
            {
                var connection = GetConnection();
                var sql = "DELETE FROM Task WHERE taskId = @taskId";

                using var command = new SqliteCommand(sql, connection);
                command.Parameters.AddWithValue("@taskId", taskId);

                var result = await command.ExecuteNonQueryAsync();

                if (result > 0)
                {
                    LogMessage($"✅ Задача с ID {taskId} удалена из БД");
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                LogMessage($"❌ Ошибка удаления задачи: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> HasUnfinishedTasksAsync(int empId)
        {
            try
            {
                var connection = GetConnection();
                var sql = @"
            SELECT COUNT(*) 
            FROM Task 
            WHERE empId = @empId 
            AND (final IS NULL OR final = '')";

                var command = new SqliteCommand(sql, connection);
                command.Parameters.AddWithValue("@empId", empId);

                var count = Convert.ToInt32(await command.ExecuteScalarAsync());
                return count > 0;
            }
            catch (Exception ex)
            {
                LogMessage($"❌ Ошибка проверки незавершенных задач: {ex.Message}");
                return false; // По умолчанию разрешаем удаление при ошибке
            }
        }

        private void LogMessage(string message)
        {
            var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
            var logMessage = $"[DB][{timestamp}] {message}";

            // Вывод в консоль
            Console.WriteLine(logMessage);

            // Вывод в отладку
            System.Diagnostics.Debug.WriteLine(logMessage);

            // Логирование в файл для Android
#if ANDROID
            try
            {
                var logPath = Path.Combine(FileSystem.AppDataDirectory, "database_log.txt");
                File.AppendAllText(logPath, $"{logMessage}{Environment.NewLine}");
            }
            catch { /* Игнорируем ошибки записи в лог */ }
#endif

            // Отправляем событие подписчикам
            ConnectionStatusChanged?.Invoke(message);
        }

        public void Dispose()
        {
            try
            {
                if (_connection != null)
                {
                    if (_connection.State == System.Data.ConnectionState.Open)
                    {
                        _connection.Close();
                        LogMessage("🔌 Соединение с БД закрыто");
                    }
                    _connection.Dispose();
                }
            }
            catch (Exception ex)
            {
                LogMessage($"⚠️ Ошибка при закрытии соединения: {ex.Message}");
            }
        }

        
    }
}