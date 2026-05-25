const express = require('express');
const router = express.Router();
const drugController = require('./drug.controller');
const { protect } = require('../../middlewares/auth.middleware');
const requireRole = require('../../middlewares/requireRole.js');
const { PERMISSIONS } = require('../../config/permissions');

router.use(protect, requireRole(PERMISSIONS.MANAGE_DRUGS));

router.get('/', drugController.listDrugs);
router.get('/new', drugController.createDrugForm);
router.post('/', drugController.createDrug);
router.get('/:id/edit', drugController.editDrugForm);
router.post('/:id', drugController.updateDrug);
router.post('/:id/delete', drugController.deleteDrug);

module.exports = router;