const express = require('express');
const router = express.Router();
const staffController = require('./staff.controller');

// Middleware to check for admin role can be added here later

// GET /admin/staff - List all staff
router.get('/', staffController.listStaff);

// GET /admin/staff/create - Show form to create new staff
router.get('/create', staffController.showCreateForm);

// POST /admin/staff/create - Handle creation of new staff
router.post('/create', staffController.createStaffMember);

// GET /admin/staff/edit/:id - Show form to edit a staff member
router.get('/edit/:id', staffController.showEditForm);

// POST /admin/g/staff/edit/:id - Handle update of a staff member
router.post('/edit/:id', staffController.updateStaffMember);

// POST /admin/staff/delete/:id - Handle deletion of a staff member
router.post('/delete/:id', staffController.deleteStaffMember);

// Hiển thị form tạo tài khoản cho nhân viên
router.get('/create-account/:id', staffController.showCreateAccountForm);

// Tạo tài khoản cho nhân viên
router.post('/create-account/:id', staffController.createAccountForStaff);

module.exports = router;