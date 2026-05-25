const LabOrder = require('./laborder.model');

const addLabOrder = async (orderBody) => {
  const labOrder = await LabOrder.create(orderBody);
  return labOrder;
};

const getLabOrdersByVisitId = async (visitId) => {
  return LabOrder.find({ visitId }).populate('labTestId');
};

module.exports = {
  addLabOrder,
  getLabOrdersByVisitId,
};