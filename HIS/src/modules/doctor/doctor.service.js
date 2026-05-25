const Patient = require('../reception/models/patient.model');
const Visit = require('../reception/models/visit.model');
const User = require('../auth/models/user.model');
const LabOrder = require('../lab/models/laborder.model');

const getStatistics = async () => {
    const patientCount = await Patient.countDocuments();
    const appointmentCount = await Visit.countDocuments();
    const doctorCount = await User.countDocuments({ role: 'doctor' });
    const labOrderCount = await LabOrder.countDocuments();

    return {
        patientCount,
        appointmentCount,
        doctorCount,
        labOrderCount,
    };
};

const getPatients = async () => {
    return await Patient.find().sort({ createdAt: -1 });
};

const getPatientById = async (id) => {
    const patient = await Patient.findById(id);
    const visits = await Visit.find({ patient: id }).sort({ visitDate: -1 });
    return { patient, visits };
};

module.exports = {
    getStatistics,
    getPatients,
    getPatientById,
};