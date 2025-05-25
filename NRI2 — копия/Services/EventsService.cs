using Microsoft.Extensions.Logging;
using NRI.Data;
using NRI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRI.Services
{
    public class EventsService
    {
        private readonly ILogger<EventsService> _logger;
        private readonly AppDbContext _context;

        public EventsService(ILogger<EventsService> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public void AddEvent(EventsService newEvent)
        {

        }
    }
}
