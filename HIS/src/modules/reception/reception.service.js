const Patient = require('./models/patient.model');
const Visit = require('./models/visit.model');

exports.getPatients = async (searchTerm) => {
    let query = {};
    if (searchTerm) {
        query = {
            $or: [
                { name: { $regex: searchTerm, $options: 'i' } },
                { patientId: { $regex: searchTerm, $options: 'i' } },
            ],
        };
    }
    return await Patient.find(query);
};

exports.createPatient = async (patientData) => {
    const patient = new Patient(patientData);
    return await patient.save();
};

exports.createVisit = async (visitData) => {
    const visit = new Visit(visitData);
    return await visit.save();
};