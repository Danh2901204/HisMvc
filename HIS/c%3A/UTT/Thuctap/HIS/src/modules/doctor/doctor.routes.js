const express = require('express');
const { getDashboard, getPatient, getVisit, saveNotes, addPrescription, deletePrescription, addLabOrder } = require('./doctor.controller');
const { protect, authorize } = require('../../middlewares/auth.middleware');

const router = express.Router();

router.get('/dashboard', protect, authorize('doctor'), getDashboard);
router.get('/patients/:id', protect, authorize('doctor'), getPatient);
router.get('/visits/:id', protect, authorize('doctor'), getVisit);
router.post('/visits/:id/notes', protect, authorize('doctor'), saveNotes);
router.post('/visits/:id/prescriptions', protect, authorize('doctor'), addPrescription);
router.delete('/prescriptions/:id', protect, authorize('doctor'), deletePrescription);
router.post('/visits/:id/lab-orders', protect, authorize('doctor'), addLabOrder);

module.exports = router;