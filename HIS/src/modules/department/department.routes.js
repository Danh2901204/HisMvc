const express = require('express');
const router = express.Router();
const departmentController = require('./department.controller');
const { protect } = require('../../middlewares/auth.middleware');
const requireRole = require('../../middlewares/requireRole.js');
const { PERMISSIONS } = require('../../config/permissions');

router.use(protect, requireRole(PERMISSIONS.MANAGE_DEPARTMENTS));

router.get('/', departmentController.listDepartments);
router.get('/new', departmentController.createDepartmentForm);
router.post('/', departmentController.createDepartment);
router.get('/:id/edit', departmentController.editDepartmentForm);
router.post('/:id', departmentController.updateDepartment);
router.post('/:id/delete', departmentController.deleteDepartment);

module.exports = router;