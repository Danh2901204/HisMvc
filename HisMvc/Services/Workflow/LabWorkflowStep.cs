using HisMvc.Data;
using HisMvc.Entities;
using HisMvc.Models;
using Microsoft.EntityFrameworkCore;

namespace HisMvc.Services.Workflow;

/// <summary>
/// BUOC 6 — Lab luu ket qua CLS.
/// </summary>
public class LabWorkflowStep
{
    private readonly AppDbContext _db;

    public LabWorkflowStep(AppDbContext db) => _db = db;

    public async Task<WorkflowResult> SaveLabResultAsync(int orderId, string resultText, string resultedBy, int? staffId)
    {
        var order = await _db.Orders
            .Include(o => o.OrderResult)
            .Include(o => o.Encounter)
            .FirstOrDefaultAsync(o => o.OrderId == orderId);

        if (order == null)
            return WorkflowResult.Fail("Không tìm thay chi dinh!");

        if (order.Status == OrderStatus.Cancelled)
            return WorkflowResult.Fail("Chỉ định đã hủy.");

        if (string.IsNullOrWhiteSpace(resultText))
            return WorkflowResult.Fail("Vui lòng nhập ket qua!");

        if (order.OrderResult == null)
        {
            order.OrderResult = new OrderResult
            {
                OrderId = orderId,
                ResultText = resultText.Trim(),
                ResultedBy = resultedBy,
                ResultedByStaffId = staffId,
                ResultedAt = DateTime.UtcNow
            };
            _db.OrderResults.Add(order.OrderResult);
        }
        else
        {
            order.OrderResult.ResultText = resultText.Trim();
            order.OrderResult.ResultedBy = resultedBy;
            order.OrderResult.ResultedByStaffId = staffId;
            order.OrderResult.ResultedAt = DateTime.UtcNow;
        }

        order.Status = OrderStatus.Resulted;
        order.CompletedAt = DateTime.UtcNow;

        if (order.Encounter != null && order.Encounter.Status == EncounterStatus.WaitingResult)
        {
            bool stillPending = await _db.Orders.AnyAsync(o =>
                o.EncounterId == order.EncounterId &&
                o.OrderId != orderId &&
                (o.Status == OrderStatus.Requested || o.Status == OrderStatus.InProgress));

            if (!stillPending)
                order.Encounter.Status = EncounterStatus.InService;
        }

        await _db.SaveChangesAsync();
        return WorkflowResult.Ok("Đã lưu ket qua thành công!");
    }
}
