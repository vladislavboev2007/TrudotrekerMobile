using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace LaborTracker.Models
{
    public class TaskItem : INotifyPropertyChanged
    {
        private int _taskId;
        public int TaskId
        {
            get => _taskId;
            set
            {
                if (_taskId != value)
                {
                    _taskId = value;
                    OnPropertyChanged(nameof(TaskId));
                }
            }
        }

        private string _name = string.Empty;
        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged(nameof(Name));
                }
            }
        }

        private string _description = string.Empty;
        public string Description
        {
            get => _description;
            set
            {
                if (_description != value)
                {
                    _description = value;
                    OnPropertyChanged(nameof(Description));
                }
            }
        }

        private TimeSpan? _start;
        public TimeSpan? Start
        {
            get => _start;
            set
            {
                if (_start != value)
                {
                    _start = value;
                    OnPropertyChanged(nameof(Start));

                    // Обновляем зависимые свойства
                    OnPropertyChanged(nameof(DurationMinutes));
                    OnPropertyChanged(nameof(CanStart));
                    OnPropertyChanged(nameof(CanStop));
                    UpdateStatus();
                }
            }
        }

        private TimeSpan? _final;
        public TimeSpan? Final
        {
            get => _final;
            set
            {
                if (_final != value)
                {
                    _final = value;
                    OnPropertyChanged(nameof(Final));

                    // Обновляем зависимые свойства
                    OnPropertyChanged(nameof(DurationMinutes));
                    OnPropertyChanged(nameof(CanStart));
                    OnPropertyChanged(nameof(CanStop));
                    UpdateStatus();
                }
            }
        }

        private DateTime _date;
        public DateTime Date
        {
            get => _date;
            set
            {
                if (_date != value)
                {
                    _date = value;
                    OnPropertyChanged(nameof(Date));
                }
            }
        }

        private int _empId;
        public int EmpId
        {
            get => _empId;
            set
            {
                if (_empId != value)
                {
                    _empId = value;
                    OnPropertyChanged(nameof(EmpId));
                }
            }
        }

        private string _employeeName = string.Empty;
        public string EmployeeName
        {
            get => _employeeName;
            set
            {
                if (_employeeName != value)
                {
                    _employeeName = value;
                    OnPropertyChanged(nameof(EmployeeName));
                }
            }
        }

        private string _status = "Не начата";
        public string Status
        {
            get => _status;
            set
            {
                if (_status != value)
                {
                    _status = value;
                    OnPropertyChanged(nameof(Status));
                    OnPropertyChanged(nameof(CanStart));
                    OnPropertyChanged(nameof(CanStop));
                }
            }
        }

        // Свойства для видимости кнопок
        public bool CanStart => Status == "Не начата";
        public bool CanStop => Status == "Выполняется";

        // Время выполнения в минутах
        public string DurationMinutes
        {
            get
            {
                if (Start.HasValue && Final.HasValue)
                {
                    var duration = Final.Value - Start.Value;
                    return $"{(int)duration.TotalMinutes} мин";
                }
                else if (Start.HasValue && !Final.HasValue)
                {
                    var currentDuration = DateTime.Now.TimeOfDay - Start.Value;
                    return $"+{(int)currentDuration.TotalMinutes} мин";
                }
                else
                {
                    return "0 мин";
                }
            }
        }

        // Полная длительность как TimeSpan
        public TimeSpan Duration =>
            (Start.HasValue && Final.HasValue) ?
            Final.Value - Start.Value : TimeSpan.Zero;

        // Метод для обновления статуса на основе времени
        private void UpdateStatus()
        {
            if (Start.HasValue && Final.HasValue)
            {
                Status = "Выполнена";
            }
            else if (Start.HasValue && !Final.HasValue)
            {
                Status = "Выполняется";
            }
            else
            {
                Status = "Не начата";
            }
        }

        // Методы для управления временем
        public void StartTask()
        {
            if (CanStart)
            {
                Start = DateTime.Now.TimeOfDay;
                Date = DateTime.Today;
                Status = "Выполняется";
            }
        }

        public void StopTask()
        {
            if (CanStop)
            {
                Final = DateTime.Now.TimeOfDay;
                Status = "Выполнена";
            }
        }

        private PropertyChangedEventHandler? _propertyChanged;
        public event PropertyChangedEventHandler? PropertyChanged
        {
            add => _propertyChanged += value;
            remove => _propertyChanged -= value;
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            _propertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}