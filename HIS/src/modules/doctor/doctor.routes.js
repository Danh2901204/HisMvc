const express = require('express');
const router = express.Router();
const doctorController = require('./doctor.controller');
const { protect } = require('../../middlewares/auth.middleware');
const requireRole = require('../../middlewares/requireRole');
const { PERMISSIONS } = require('../../config/permissions');

// All routes in this file are protected and require doctor role
router.use(protect, requireRole(PERMISSIONS.VIEW_PATIENT_LIST));

// Dashboard
router.get('/', (req, res) => res.redirect('/doctors/dashboard'));
router.get('/dashboard', doctorController.getDashboard);
router.get('/patients/:id', doctorController.getPatientDetails);

module.exports = router;