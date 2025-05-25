using NRI.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRI.Events
{
    public static class EventExtensions
    {
        public static bool CanJoin(this Event eventObj, int currentUserId)
        {
            return eventObj.Participants.Count < eventObj.MaxParticipants &&
                   !eventObj.Participants.Any(p => p.UserID == currentUserId) &&
                   eventObj.EventDate > DateTime.Now;
        }

        public static bool IsParticipant(this Event eventObj, int currentUserId)
        {
            return eventObj.Participants.Any(p => p.UserID == currentUserId);
        }

        public static string GetStatusText(this Event eventObj, int currentUserId)
        {
            if (eventObj.OrganizerID == currentUserId) return "Организатор";
            if (eventObj.IsParticipant(currentUserId)) return "Участник";
            if (eventObj.Participants.Count >= eventObj.MaxParticipants) return "Заполнено";
            return "Доступно";
        }
    }
}
