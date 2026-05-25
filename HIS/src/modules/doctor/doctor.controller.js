const doctorService = require('./doctor.service');

const getDashboard = async (req, res) => {
    try {
        const stats = await doctorService.getStatistics();
        const patients = await doctorService.getPatients();
        res.render('doctor/dashboard', {
            user: req.user,
            stats,
            patients,
        });
    } catch (error) {
        res.status(500).send(error.message);
    }
};

const getPatientDetails = async (req, res) => {
    try {
        const { patient, visits } = await doctorService.getPatientById(req.params.id);
        if (!patient) {
            return res.status(404).send('Patient not found');
        }
        res.render('doctor/patient-details', {
            user: req.user,
            patient,
            visits,
        });
    } catch (error) {
        res.status(500).send(error.message);
    }
};

module.exports = {
    getDashboard,
    getPatientDetails,
};