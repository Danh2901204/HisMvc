const express = require('express');
const router = express.Router();
const labController = require('./lab.controller');
const { protect, authorize } = require('../../middlewares/auth.middleware');

// Lab Dashboard
router.get('/', protect, authorize('lab_technician'), labController.getDashboard);

// View Lab Order and Enter Results
router.get('/:id', protect, authorize('lab_technician'), labController.getLabOrder);

// Submit Lab Results
router.post('/:id', protect, authorize('lab_technician'), labController.submitLabResult);

module.exports = router;