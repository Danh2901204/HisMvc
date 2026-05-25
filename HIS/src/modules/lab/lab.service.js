const LabOrder = require('../doctor/models/laborder.model'); // Assuming laborder model is in doctor module

exports.submitLabResult = async (labOrderId, result) => {
    const labOrder = await LabOrder.findById(labOrderId);
    if (!labOrder) {
        throw new Error('Lab order not found');
    }
    labOrder.result = result;
    labOrder.status = 'completed';
    labOrder.date = new Date();
    await labOrder.save();
    return labOrder;
};