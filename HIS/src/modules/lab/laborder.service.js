// ... existing code ...
const LabOrder = require('./models/laborder.model');

exports.addLabOrder = async (orderData) => {
// ... existing code ...
};

exports.getLabOrdersByVisitId = async (visitId) => {
// ... existing code ...
};

exports.getAllLabOrders = async () => {
    return await LabOrder.find().populate('patientId').populate('labTestId').exec();
};

exports.getLabOrderById = async (id) => {
    return await LabOrder.findById(id).populate('patientId').populate('labTestId').exec();
};