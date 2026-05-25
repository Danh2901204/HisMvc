const Prescription = require('./prescription.model');

const addPrescription = async (prescriptionBody) => {
  const prescription = await Prescription.create(prescriptionBody);
  return prescription;
};

const getPrescriptionsByVisitId = async (visitId) => {
  return Prescription.find({ visitId }).populate('drugId');
};

const getPrescriptionById = async (id) => {
  return Prescription.findById(id);
};

const deletePrescription = async (prescriptionId) => {
  const prescription = await Prescription.findById(prescriptionId);
  if (!prescription) {
    throw new Error('Prescription not found');
  }
  await prescription.remove();
  return prescription;
};

module.exports = {
  addPrescription,
  getPrescriptionsByVisitId,
  getPrescriptionById,
  deletePrescription,
};