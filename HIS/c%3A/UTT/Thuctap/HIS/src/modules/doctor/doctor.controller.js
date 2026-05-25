const patientService = require('../patient/patient.service');
const visitService = require('../visit/visit.service');
const drugService = require('../drug/drug.service');
const prescriptionService = require('../prescription/prescription.service');
const labTestService = require('../labtest/labtest.service');
const labOrderService = require('../laborder/laborder.service');

const getDashboard = async (req, res) => {
  try {
    const patients = await patientService.getPatients();
    res.render('doctor/dashboard', {
      user: req.user,
      patients,
      title: 'Doctor Dashboard',
    });
  } catch (error) {
    res.status(500).send(error.message);
  }
};

const getPatient = async (req, res) => {
  try {
    const patient = await patientService.getPatientById(req.params.id);
    const visits = await visitService.getVisitsByPatientId(req.params.id);
    res.render('doctor/patient', {
      user: req.user,
      patient,
      visits,
      title: 'Patient Details',
    });
  } catch (error) {
    res.status(500).send(error.message);
  }
};

const getVisit = async (req, res) => {
  try {
    const visit = await visitService.getVisitById(req.params.id);
    const patient = await patientService.getPatientById(visit.patientId);
    const drugs = await drugService.getDrugs();
    const prescriptions = await prescriptionService.getPrescriptionsByVisitId(req.params.id);
    const labTests = await labTestService.getLabTests();
    const labOrders = await labOrderService.getLabOrdersByVisitId(req.params.id);
    res.render('doctor/visit', {
      user: req.user,
      visit,
      patient,
      drugs,
      prescriptions,
      labTests,
      labOrders,
      title: 'Visit Details',
    });
  } catch (error) {
    res.status(500).send(error.message);
  }
};

const saveNotes = async (req, res) => {
  try {
    const { symptoms, diagnosis } = req.body;
    await visitService.updateVisitNotes(req.params.id, { symptoms, diagnosis });
    res.redirect(`/doctor/visits/${req.params.id}`);
  } catch (error) {
    res.status(500).send(error.message);
  }
};

const addPrescription = async (req, res) => {
  try {
    const visit = await visitService.getVisitById(req.params.id);
    const prescriptionBody = {
      ...req.body,
      visitId: req.params.id,
      patientId: visit.patientId,
    };
    await prescriptionService.addPrescription(prescriptionBody);
    res.redirect(`/doctor/visits/${req.params.id}`);
  } catch (error) {
    res.status(500).send(error.message);
  }
};

const deletePrescription = async (req, res) => {
  try {
    const prescription = await prescriptionService.getPrescriptionById(req.params.id);
    await prescriptionService.deletePrescription(req.params.id);
    res.redirect(`/doctor/visits/${prescription.visitId}`);
  } catch (error) {
    res.status(500).send(error.message);
  }
};

const addLabOrder = async (req, res) => {
  try {
    const visit = await visitService.getVisitById(req.params.id);
    const orderBody = {
      ...req.body,
      visitId: req.params.id,
      patientId: visit.patientId,
    };
    await labOrderService.addLabOrder(orderBody);
    res.redirect(`/doctor/visits/${req.params.id}`);
  } catch (error) {
    res.status(500).send(error.message);
  }
};

module.exports = {
  getDashboard,
  getPatient,
  getVisit,
  saveNotes,
  addPrescription,
  deletePrescription,
  addLabOrder,
};