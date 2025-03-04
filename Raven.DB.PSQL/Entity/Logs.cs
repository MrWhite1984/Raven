using Microsoft.Extensions.Logging;
using Raven.DB.PSQL.Entity.@enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raven.DB.PSQL.Entity
{
    public class Logs
    {
        public Guid Id { get; set; }
        public LogLevel LogLevel { get; set; }
        public DateTime DateTime { get; set; }
        public string Message { get; set; }
    }
}
