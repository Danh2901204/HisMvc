const express = require('express');
const router = express.Router();
const bedController = require('./bed.controller');
const { protect } = require('../../middlewares/auth.middleware');
const requireRole = require('../../middlewares/requireRole.js');
const { PERMISSIONS } = require('../../config/permissions');

router.use(protect, requireRole(PERMISSIONS.MANAGE_BEDS));

router.get('/', bedController.listBeds);
router.get('/new', bedController.createBedForm);
router.post('/', bedController.createBed);
router.get('/:id/edit', bedController.editBedForm);
router.post('/:id', bedController.updateBed);
router.post('/:id/delete', bedController.deleteBed);

module.exports = router;