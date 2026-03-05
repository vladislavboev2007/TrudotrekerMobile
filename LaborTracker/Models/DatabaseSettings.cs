using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborTracker.Models
{
    public class DatabaseSettings
    {
        public string ExternalDbPath { get; set; } = string.Empty;
        public bool UseExternalDb { get; set; } = false;
        public bool InitializeWithTestData { get; set; } = true;
    }
}