const labService = require('./lab.service');
const labOrderService = require('../doctor/laborder.service'); // Assuming laborder service is in doctor module
const patientService = require('../reception/reception.service'); // Assuming patient service is in reception module

exports.getDashboard = async (req, res) => {
    try {
        const labOrders = await labOrderService.getAllLabOrders(); // This function needs to be created in labOrderService
        res.render('lab/dashboard', {
            user: req.user,
            labOrders,
        });
    } catch (error) {
        res.status(500).send(error.message);
    }
};

exports.getLabOrder = async (req, res) => {
    try {
        const labOrder = await labOrderService.getLabOrderById(req.params.id);
        if (!labOrder) {
            return res.status(404).send('Lab order not found');
        }
        const patient = await patientService.getPatientById(labOrder.patientId);
        res.render('lab/result-input', {
            user: req.user,
            labOrder,
            patient,
        });
    } catch (error) {
        res.status(500).send(error.message);
    }
};

exports.submitLabResult = async (req, res) => {
    try {
        const { result } = req.body;
        await labService.submitLabResult(req.params.id, result);
        res.redirect('/lab');
    } catch (error) {
        res.status(500).send(error.message);
    }
};