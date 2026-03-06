## 📂 Структура репозитория
```text
LaborTracker/
├── .github/                          # GitHub Actions (CI/CD)
│   └── workflows/
│       └── build.yml                  # Автоматическая сборка APK
├── Platforms/                         # Платформозависимый код
│   ├── Android/                        # Android-специфичный код
│   ├── iOS/                            # iOS-специфичный код
│   └── Windows/                        # Windows-специфичный код
├── Resources/                          # Ресурсы приложения
│   ├── Fonts/                          # Шрифты (Montserrat)
│   ├── Images/                         # Изображения и иконки
│   └── Raw/                            # Сырые ресурсы (БД и др.)
├── Services/                           # Сервисы приложения
│   ├── DatabaseService.cs               # Работа с SQLite
│   └── ApiService.cs                    # Взаимодействие с API (перспектива)
├── Models/                              # Модели данных
│   ├── Employee.cs                      # Модель сотрудника
│   └── TaskItem.cs                      # Модель задачи
├── ViewModels/                          # ViewModel (MVVM)
│   ├── BaseViewModel.cs                  # Базовый класс
│   ├── MainViewModel.cs                   # Главная страница
│   ├── TasksViewModel.cs                   # Страница задач
│   └── EmployeesViewModel.cs                 # Страница сотрудников
├── Views/                               # Страницы (XAML)
│   ├── SplashPage.xaml                    # Загрузочный экран
│   ├── MainPage.xaml                       # Главная страница
│   ├── TasksPage.xaml                       # Страница задач
│   ├── EmployeesPage.xaml                    # Страница сотрудников
│   └── AddTaskPage.xaml                       # Добавление задачи
├── Converters/                          # Конвертеры для привязки
│   ├── InverseBooleanConverter.cs
│   └── TaskConverters.cs
├── App.xaml                             # Настройки приложения
├── App.xaml.cs                          # Логика приложения
├── AppShell.xaml                        # Оболочка навигации
├── AppShell.xaml.cs                     # Логика навигации
├── MauiProgram.cs                       # Точка входа и DI
├── appsettings.json                     # Конфигурация
├── LaborTracker.csproj            # Файл проекта
└── README.md                            # Документация
```
## 📥 Как клонировать и запустить
**1. Клонирование репозитория**
```bash
git clone https://github.com/vladislavboev2007/LaborTrackerMobile.git
cd LaborTrackerMobile
```
**2. Открытие в Visual Studio 2022**

Откройте файл LaborTrackerMobile.sln в Visual Studio 2022 (версия 17.8 или выше).

**3. Восстановление зависимостей**
```bash
dotnet restore
```
**4. Запуск на эмуляторе Android**

Выберите целевое устройство (Android Emulator) и нажмите F5 или выполните:

```bash
dotnet build -c Debug
dotnet run -f net9.0-android
```
## 📦 Сборка APK
**1. Создание ключа подписки (один раз)**
```powershell
cd C:\путь\к\проекту\LaborTrackerMobile
keytool -genkeypair -v -keystore LaborTracker.keystore -alias labor_tracker -keyalg RSA -keysize 2048 -validity 10000
```
Параметры:

**Пароль:** labor2024

**Организация:** LaborTracker

**Страна:** RU

**2. Сборка подписанного APK**
```powershell
dotnet publish -c Release -f net9.0-android `
  /p:AndroidKeyStore=true `
  /p:AndroidSigningKeyStore="C:\путь\к\проекту\LaborTrackerMobile\LaborTracker.keystore" `
  /p:AndroidSigningKeyAlias=labor_tracker `
  /p:AndroidSigningKeyPass=labor2024 `
  /p:AndroidSigningStorePass=labor2024
```
**3. Готовый APK**

Файл находится по пути:
```text
bin/Release/net9.0-android/publish/com.companyname.labortracker-Signed.apk
```
## 🛠️ Технологический стек
- **Платформа:**	.NET MAUI 9.0
- **Язык:**	C# 12.0
- **База данных:**	SQLite (Microsoft.Data.Sqlite)
- **Архитектура:**	MVVM (Model-View-ViewModel)
- **DI-контейнер:**	Microsoft.Extensions.DependencyInjection
- **Команды:**	CommunityToolkit.MVVM
- **Навигация:**	Shell (встроенная)
- **UI-компоненты:**	Microsoft.Maui.Controls

## 📋 Системные требования
**Для разработки:**
- Visual Studio 2022 17.8+
- .NET 9.0 SDK
- Android SDK (API 28+)
- Эмулятор Android или физическое устройство

**Для пользователя:**
- Android 9.0 и выше
- 2 ГБ RAM (рекомендуется 4 ГБ)
- 100 МБ свободного места

## 📱 Функционал приложения
- **Загрузка:**	Splash-экран с логотипом и инициализацией БД
- **Главная:** Статистика (общее время, всего задач, завершено)
- **Задачи:**	Список задач, поиск, фильтрация, действия
- **Сотрудники:**	Список сотрудников, добавление, удаление
- **Добавление:** задачи	Форма для создания новой задачи

Основные действия с задачами:
- ▶️ Старт — запуск таймера
- ⏹️ Стоп — остановка таймера
- ⓘ Инфо — просмотр деталей
- 🗑️ Удалить — удаление задачи

## 🔄 Синхронизация с сервером (перспектива)
В будущих версиях планируется синхронизация с веб-версией через API:

```csharp
public class ApiService
{
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "http://10.0.2.2:7000"; // Для эмулятора
    
    public async Task<List<Task>> GetTasks()
    {
        var response = await _httpClient.GetAsync("/api/tasks");
        // ... обработка ответа
    }
}
```

## 📄 Лицензия
Проект распространяется под лицензией MIT. Подробнее см. файл LICENSE.

## ⚠️ Важное примечание
Данный проект разработан с использованием .NET 9.0 и C# 12.0. Для корректной сборки и запуска необходима указанная версия SDK.
