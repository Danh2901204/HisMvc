using HisMvc.Models;
using HisMvc.Models.Chatbot;

namespace HisMvc.Services;

internal static class PublicAppointmentExtensions
{
    public static List<PublicSlotView> BookableOnly(this IEnumerable<PublicSlotView> slots) =>
        slots.Where(s => s.CanBook).ToList();

    public static List<SlotOption> ToSlotOptions(this IEnumerable<PublicSlotView> slots) =>
        slots.Select(s => new SlotOption
        {
            TimeSlotId = s.TimeSlotId,
            Label = $"{s.Start} - {s.End}",
            Available = s.CanBook ? 1 : 0
        }).ToList();
}
